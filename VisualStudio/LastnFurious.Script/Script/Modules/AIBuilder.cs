// Module_AIBuilder - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_AIBuilder;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_AIBuilder
    {
        // Fields
        public bool AIBuilderEnabled;
        public bool WasMouseDown;
        public int WasMouseX;
        public int WasMouseY;

        // Methods
        public void EnableAIBuilder(bool enable)
        {
            DisplayDebugAI = enable;
            gDebugAI.Visible = enable;
            Mouse.Visible = enable;
            AIBuilderEnabled = enable;
            if (enable)
                UpdateDebugAI();
        }

        public int TryHitNode(int x, int y, float threshold)
        {
            if (PathNodeCount == 0)
                return -1;
            VectorF hit = VectorF.create(x, y);
            float minDist = 9999.0f;
            int nearestNode = -1;
            int i = 0;
            for (i = 0; i < MAX_PATH_NODES; i += 1)
            {
                if (Paths[i].pt == null)
                    continue;
                float dist = VectorF.distance(hit, Paths[i].pt);
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
                FirstPathNode = newnode;
                Paths[newnode].prev = FirstPathNode;
            }
            else 
            {
                Paths[newnode].prev = prev;
                Paths[prev].next = newnode;
            }
            if (next < 0)
            {
                LastPathNode = newnode;
                Paths[newnode].next = LastPathNode;
            }
            else 
            {
                Paths[newnode].next = next;
                Paths[next].prev = newnode;
            }
            if (prev == LastPathNode)
                LastPathNode = newnode;
            Paths[newnode].radius = DEFAULT_PATH_CHECK_RADIUS;
            Paths[newnode].threshold = DEFAULT_PATH_CHECK_RADIUS;
        }

        public int PutNewNode(int x, int y)
        {
            if (FreePathSlot < 0)
                return 0;
            int newnode = FreePathSlot;
            Paths[newnode].Reset();
            Paths[newnode].pt = VectorF.create(x, y);
            OnNewNode(newnode, LastPathNode, FirstPathNode);
            PathNodeCount += 1;
            FindFirstFreeNode();
            return newnode;
        }

        public void DeleteNode(int node)
        {
            if (node < 0 || node >= MAX_PATH_NODES)
                return;
            int nodep = Paths[node].prev;
            int noden = Paths[node].next;
            if (node == nodep && node == noden)
            {
                FirstPathNode = -1;
                LastPathNode = -1;
            }
            else 
            {
                if (nodep >= 0)
                    Paths[nodep].next = noden;
                if (noden >= 0)
                    Paths[noden].prev = nodep;
                if (node == FirstPathNode)
                    FirstPathNode = noden;
                if (node == LastPathNode)
                    LastPathNode = nodep;
            }
            Paths[node].Reset();
            PathNodeCount -= 1;
            FindFirstFreeNode();
            if (SelectedPathNode == node)
                SelectedPathNode = -1;
        }

        public int TryInsertNode(int refNode, int x, int y)
        {
            if (FreePathSlot < 0)
                return 0;
            int nodep = Paths[refNode].prev;
            int noden = Paths[refNode].next;
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
                VectorF hitDir = VectorF.subtract(hit, Paths[refNode].pt);
                VectorF nextDir = VectorF.subtract(Paths[noden].pt, Paths[refNode].pt);
                VectorF prevDir = VectorF.subtract(Paths[nodep].pt, Paths[refNode].pt);
                if (Maths.AbsF(VectorF.angleBetween(hitDir, nextDir)) <= Maths.AbsF(VectorF.angleBetween(hitDir, prevDir)))
                    nodeopp = noden;
                else 
                    nodeopp = nodep;
            }
            VectorF newpt = Paths[refNode].pt.clone();
            newpt.add(Paths[nodeopp].pt);
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
            int newnode = FreePathSlot;
            Paths[newnode].Reset();
            Paths[newnode].pt = newpt;
            OnNewNode(newnode, insertPrev, insertNext);
            PathNodeCount += 1;
            FindFirstFreeNode();
            return newnode;
        }

        public void game_start()
        {
        }

        public void on_mouse_click(MouseButton button)
        {
            if (!AIBuilderEnabled)
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
            if (!AIBuilderEnabled)
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
                else if (key == eKeyHome)
                {
                    String input = Game.InputBox(StringFormatAGS("Speed (now %.2f)", Paths[SelectedPathNode].speed));
                    if (!String.IsNullOrEmpty(input))
                        Paths[SelectedPathNode].speed = input.AsFloat();
                }
                else if (key == eKeyPageUp)
                {
                    String input = Game.InputBox(StringFormatAGS("Check radius (now %.2f)", Paths[SelectedPathNode].radius));
                    if (!String.IsNullOrEmpty(input))
                        Paths[SelectedPathNode].radius = input.AsFloat();
                }
                else if (key == eKeyPageDown)
                {
                    String input = Game.InputBox(StringFormatAGS("Direction threshold (now %.2f)", Paths[SelectedPathNode].threshold));
                    if (!String.IsNullOrEmpty(input))
                        Paths[SelectedPathNode].threshold = input.AsFloat();
                }
            }
        }

        public void repeatedly_execute_always()
        {
            if (!AIBuilderEnabled)
                return;
            if (IsGamePaused())
                return;
            if (SelectedPathNode >= 0 && Mouse.IsButtonDown(eMouseLeft) && WasMouseDown && (mouse.x != WasMouseX || mouse.y != WasMouseY))
            {
                Paths[SelectedPathNode].pt.x = IntToFloat(mouse.x + GetViewportX());
                Paths[SelectedPathNode].pt.y = IntToFloat(mouse.y + GetViewportY());
            }
            WasMouseDown = Mouse.IsButtonDown(eMouseLeft);
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int PATH_NODE_SELECT_THRESHOLD = DEBUG_AI_NODE_RADIUS * 2;

        // Expose AIBuilder methods so they can be used without instance prefix
        public static void EnableAIBuilder(bool enable)
        {
            AIBuilder.EnableAIBuilder(enable);
        }

        // Expose AIBuilder variables so they can be used without instance prefix
        public static bool AIBuilderEnabled { get { return AIBuilder.AIBuilderEnabled; } set { AIBuilder.AIBuilderEnabled = value; } }

    }

    #endregion

}
