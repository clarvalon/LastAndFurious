// Module_RaceBuilder - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_RaceBuilder;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_RaceBuilder
    {
        // Fields
        public bool RaceBuilderEnabled;
        public bool WasMouseDown;
        public int WasMouseX;
        public int WasMouseY;

        // Methods
        public void EnableRaceBuilder(bool enable)
        {
            DisplayDebugRace = enable;
            gDebugAI.Visible = enable;
            Mouse.Visible = enable;
            RaceBuilderEnabled = enable;
            if (enable)
                UpdateDebugRace();
        }

        public int TryHitNode(int x, int y, float threshold)
        {
            if (CheckptCount == 0)
                return -1;
            VectorF hit = VectorF.create(x, y);
            float minDist = 9999.0f;
            int nearestNode = -1;
            int i = 0;
            for (i = 0; i < MAX_PATH_NODES; i += 1)
            {
                if (Checkpoints[i].pt == null)
                    continue;
                float dist = VectorF.distance(hit, Checkpoints[i].pt);
                if (dist <= threshold && dist < minDist)
                {
                    minDist = dist;
                    nearestNode = i;
                }
            }
            return nearestNode;
        }

        public void OnNewNode(int newnode, int prev, int next)
        {
            if (prev < 0)
            {
                FirstCheckpt = newnode;
                Checkpoints[newnode].prev = FirstCheckpt;
            }
            else 
            {
                Checkpoints[newnode].prev = prev;
                Checkpoints[prev].next = newnode;
            }
            if (next < 0)
            {
                LastCheckpt = newnode;
                Checkpoints[newnode].next = LastCheckpt;
            }
            else 
            {
                Checkpoints[newnode].next = next;
                Checkpoints[next].prev = newnode;
            }
            if (prev == LastCheckpt)
                LastCheckpt = newnode;
        }

        public int PutNewNode(int x, int y)
        {
            if (FreeCheckptSlot < 0)
                return 0;
            int newnode = FreeCheckptSlot;
            Checkpoints[newnode].Reset();
            Checkpoints[newnode].pt = VectorF.create(x, y);
            OnNewNode(newnode, LastCheckpt, FirstCheckpt);
            Checkpoints[newnode].order = CheckptCount;
            CheckptCount += 1;
            FindFirstFreeCheckpoint();
            return newnode;
        }

        public void DeleteNode(int node)
        {
            if (node < 0 || node >= MAX_PATH_NODES)
                return;
            int nodep = Checkpoints[node].prev;
            int noden = Checkpoints[node].next;
            if (node == nodep && node == noden)
            {
                FirstCheckpt = -1;
                LastCheckpt = -1;
            }
            else 
            {
                if (nodep >= 0)
                    Checkpoints[nodep].next = noden;
                if (noden >= 0)
                    Checkpoints[noden].prev = nodep;
                if (node == FirstCheckpt)
                    FirstCheckpt = noden;
                if (node == LastCheckpt)
                    LastCheckpt = nodep;
            }
            Checkpoints[node].Reset();
            CheckptCount -= 1;
            int fixnode = noden;
            while (fixnode != FirstCheckpt)
            {
                Checkpoints[fixnode].order -= 1;
                fixnode = Checkpoints[fixnode].next;
            }
            FindFirstFreeCheckpoint();
            if (SelectedPathNode == node)
                SelectedPathNode = -1;
        }

        public int TryInsertNode(int refNode, int x, int y)
        {
            if (FreeCheckptSlot < 0)
                return 0;
            int nodep = Checkpoints[refNode].prev;
            int noden = Checkpoints[refNode].next;
            if (nodep < 0 && noden < 0)
                return PutNewNode(x, y);
            int nodeopp = 0;
            if (nodep < 0)
            {
                nodeopp = noden;
            }
            else if (noden < 0)
            {
                nodeopp = nodep;
            }
            else 
            {
                VectorF hit = VectorF.create(x, y);
                VectorF hitDir = VectorF.subtract(hit, Checkpoints[refNode].pt);
                VectorF nextDir = VectorF.subtract(Checkpoints[noden].pt, Checkpoints[refNode].pt);
                VectorF prevDir = VectorF.subtract(Checkpoints[nodep].pt, Checkpoints[refNode].pt);
                if (Maths.AbsF(VectorF.angleBetween(hitDir, nextDir)) <= Maths.AbsF(VectorF.angleBetween(hitDir, prevDir)))
                    nodeopp = noden;
                else 
                    nodeopp = nodep;
            }
            VectorF newpt = Checkpoints[refNode].pt.clone();
            newpt.add(Checkpoints[nodeopp].pt);
            newpt.scale(0.5f);
            int insertPrev = 0;
            int insertNext = 0;
            if (nodeopp == noden)
            {
                insertPrev = refNode;
                insertNext = noden;
            }
            else 
            {
                insertPrev = nodep;
                insertNext = refNode;
            }
            int newnode = FreeCheckptSlot;
            Checkpoints[newnode].Reset();
            Checkpoints[newnode].pt = newpt;
            OnNewNode(newnode, insertPrev, insertNext);
            CheckptCount += 1;
            int fixnode = insertNext;
            while (fixnode != FirstCheckpt)
            {
                Checkpoints[fixnode].order += 1;
                fixnode = Checkpoints[fixnode].next;
            }
            FindFirstFreeCheckpoint();
            return newnode;
        }

        public void game_start()
        {
        }

        public void on_mouse_click(MouseButton button)
        {
            if (!RaceBuilderEnabled)
                return;
            if (button == eMouseLeft)
            {
                SelectedPathNode = TryHitNode(mouse.x + GetViewportX(), mouse.y + GetViewportY(), IntToFloat(PATH_NODE_SELECT_THRESHOLD));
                WasMouseX = mouse.x;
                WasMouseY = mouse.y;
            }
            else if (button == eMouseRight)
            {
                PutNewNode(mouse.x + GetViewportX(), mouse.y + GetViewportY());
            }
        }

        public void on_key_press(eKeyCode key)
        {
            if (!RaceBuilderEnabled)
                return;
            if (SelectedPathNode >= 0)
            {
                if (key == eKeyDelete)
                {
                    DeleteNode(SelectedPathNode);
                }
                else if (key == eKeyInsert)
                {
                    TryInsertNode(SelectedPathNode, mouse.x + GetViewportX(), mouse.y + GetViewportY());
                }
            }
        }

        public void repeatedly_execute_always()
        {
            if (!RaceBuilderEnabled)
                return;
            if (IsGamePaused())
                return;
            if (SelectedPathNode >= 0 && Mouse.IsButtonDown(eMouseLeft) && WasMouseDown && (mouse.x != WasMouseX || mouse.y != WasMouseY))
            {
                Checkpoints[SelectedPathNode].pt.x = IntToFloat(mouse.x + GetViewportX());
                Checkpoints[SelectedPathNode].pt.y = IntToFloat(mouse.y + GetViewportY());
            }
            WasMouseDown = Mouse.IsButtonDown(eMouseLeft);
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        // public const int PATH_NODE_SELECT_THRESHOLD = DEBUG_AI_NODE_RADIUS * 2;

        // Expose RaceBuilder methods so they can be used without instance prefix
        public static void EnableRaceBuilder(bool enable)
        {
            RaceBuilder.EnableRaceBuilder(enable);
        }

        // Expose RaceBuilder variables so they can be used without instance prefix
        public static bool RaceBuilderEnabled { get { return RaceBuilder.RaceBuilderEnabled; } set { RaceBuilder.RaceBuilderEnabled = value; } }

    }

    #endregion

}
