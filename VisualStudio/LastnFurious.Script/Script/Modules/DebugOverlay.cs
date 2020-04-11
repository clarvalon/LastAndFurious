// Module_DebugOverlay - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_DebugOverlay;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_DebugOverlay
    {
        // Fields
        public bool DisplayDebugOverlay;
        public bool DisplayDebugInfo;
        public bool DisplayDebugAI;
        public bool DisplayDebugRace;
        public int SelectedPathNode;
        public DynamicSprite debugOver;
        public DynamicSprite debugAI;
        public int LastViewportX;
        public int LastViewportY;

        // Methods
        public void game_start()
        {
            debugOver = DynamicSprite.Create(gDebugOver.Width, gDebugOver.Height, false);
            gDebugOver.BackgroundGraphic = debugOver.Graphic;
            debugAI = DynamicSprite.Create(gDebugOver.Width, gDebugOver.Height, false);
            gDebugAI.BackgroundGraphic = debugAI.Graphic;
            SelectedPathNode = -1;
        }

        public void UpdateDebugAIRegions(DrawingSurface ds)
        {
            ds.DrawingColor = Game.GetColorFromRGB(255, 255, 255);
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                int veh = RobotsRB[i].vehicleIndex;
                if (veh < 0)
                    continue;
                VectorF pos = Cars[veh].position;
                if (pos == null)
                    continue;
                VectorF dir = VectorF.create(50, 0);
                dir.rotate(RobotsRB[i].targetAngle);
                dir.add(pos);
                int x1 = FloatToInt(pos.x, eRoundNearest) - GetViewportX();
                int y1 = FloatToInt(pos.y, eRoundNearest) - GetViewportY();
                int x2 = FloatToInt(dir.x, eRoundNearest) - GetViewportX();
                int y2 = FloatToInt(dir.y, eRoundNearest) - GetViewportY();
                ds.DrawLine(x1, y1, x2, y2);
            }
        }

        public void UpdateDebugAIPaths(DrawingSurface ds)
        {
            if (PathNodeCount == 0)
                return;
            int from = 0;
            int  to = 0;
            from = 0;
            to = MAX_PATH_NODES;
            int i = 0;
            for (i = from; i < to; i += 1)
            {
                if (Paths[i].pt == null)
                    continue;
                VectorF pt = Paths[i].pt;
                int x = FloatToInt(pt.x, eRoundNearest) - GetViewportX();
                int y = FloatToInt(pt.y, eRoundNearest) - GetViewportY();
                if (x >= 0 && x <= system.ViewportWidth && y >= 0 && y <= system.ViewportHeight)
                {
                    if (SelectedPathNode == i)
                        ds.DrawingColor = Game.GetColorFromRGB(255, 0, 0);
                    else 
                        ds.DrawingColor = Game.GetColorFromRGB(0, 255, 0);
                    ds.DrawCircle(x, y, DEBUG_AI_NODE_RADIUS);
                }
                int prev = Paths[i].prev;
                if (prev >= 0)
                {
                    VectorF ptprev = Paths[prev].pt;
                    ds.DrawingColor = Game.GetColorFromRGB(0, 255, 0);
                    ds.DrawLine(x, y, FloatToInt(ptprev.x, eRoundNearest) - GetViewportX(), FloatToInt(ptprev.y, eRoundNearest) - GetViewportY());
                }
                if (x >= 0 && x <= system.ViewportWidth && y >= 0 && y <= system.ViewportHeight)
                {
                    ds.DrawingColor = Game.GetColorFromRGB(225, 225, 225);
                    ds.DrawString(x + 10, y, eFontNotalot35Regular12, StringFormatAGS("#%d: %.2f,%.2f", i, pt.x, pt.y));
                    ds.DrawString(x + 10, y + GetFontLineSpacing(eFontNotalot35Regular12), eFontNotalot35Regular12, StringFormatAGS("R: %.2f, T: %.2f, S: %.2f", Paths[i].radius, Paths[i].threshold, Paths[i].speed));
                }
            }
            if (FirstPathNode != LastPathNode)
            {
                ds.DrawingColor = Game.GetColorFromRGB(0, 255, 255);
                int x1 = FloatToInt(Paths[FirstPathNode].pt.x, eRoundNearest) - GetViewportX();
                int y1 = FloatToInt(Paths[FirstPathNode].pt.y, eRoundNearest) - GetViewportY();
                int x2 = FloatToInt(Paths[LastPathNode].pt.x, eRoundNearest) - GetViewportX();
                int y2 = FloatToInt(Paths[LastPathNode].pt.y, eRoundNearest) - GetViewportY();
                ds.DrawLine(x1, y1, x2, y2);
            }
            ds.DrawingColor = Game.GetColorFromRGB(128, 0, 128);
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                int veh = RobotsPB[i].vehicleIndex;
                if (veh < 0)
                    continue;
                VectorF pos = Cars[veh].position;
                VectorF target = RobotsPB[i].targetPos;
                if (pos == null || target == null)
                    continue;
                int x1 = FloatToInt(pos.x, eRoundNearest) - GetViewportX();
                int y1 = FloatToInt(pos.y, eRoundNearest) - GetViewportY();
                int x2 = FloatToInt(target.x, eRoundNearest) - GetViewportX();
                int y2 = FloatToInt(target.y, eRoundNearest) - GetViewportY();
                ds.DrawLine(x1, y1, x2, y2);
            }
        }

        public void UpdateDebugAI()
        {
            DrawingSurface ds = debugAI.GetDrawingSurface();
            ds.Clear();
            if (ActiveAIType == eAIPaths)
                UpdateDebugAIPaths(ds);
            else if (ActiveAIType == eAIRegions)
                UpdateDebugAIRegions(ds);
            ds.Release();
        }

        public void UpdateDebugRace()
        {
            DrawingSurface ds = debugAI.GetDrawingSurface();
            ds.Clear();
            if (CheckptCount == 0)
                return;
            int from = 0;
            int  to = 0;
            from = 0;
            to = MAX_CHECKPOINTS;
            int i = 0;
            for (i = from; i < to; i += 1)
            {
                if (Checkpoints[i].pt == null)
                    continue;
                VectorF pt = Checkpoints[i].pt;
                int x = FloatToInt(pt.x, eRoundNearest) - GetViewportX();
                int y = FloatToInt(pt.y, eRoundNearest) - GetViewportY();
                if (x >= 0 && x <= system.ViewportWidth && y >= 0 && y <= system.ViewportHeight)
                {
                    if (SelectedPathNode == i)
                        ds.DrawingColor = Game.GetColorFromRGB(0, 255, 255);
                    else 
                        ds.DrawingColor = Game.GetColorFromRGB(0, 0, 255);
                    ds.DrawCircle(x, y, DEBUG_AI_NODE_RADIUS);
                }
                int prev = Checkpoints[i].prev;
                if (prev >= 0)
                {
                    VectorF ptprev = Checkpoints[prev].pt;
                    ds.DrawingColor = Game.GetColorFromRGB(0, 0, 255);
                    ds.DrawLine(x, y, FloatToInt(ptprev.x, eRoundNearest) - GetViewportX(), FloatToInt(ptprev.y, eRoundNearest) - GetViewportY());
                }
                if (x >= 0 && x <= system.ViewportWidth && y >= 0 && y <= system.ViewportHeight)
                {
                    ds.DrawingColor = Game.GetColorFromRGB(225, 225, 225);
                    ds.DrawString(x + 10, y, eFontNotalot35Regular12, StringFormatAGS("#%d: %.2f,%.2f", i, pt.x, pt.y));
                    ds.DrawString(x + 10, y + GetFontLineSpacing(eFontNotalot35Regular12), eFontNotalot35Regular12, StringFormatAGS("Order: %d", Checkpoints[i].order));
                }
            }
            if (FirstCheckpt != LastCheckpt)
            {
                ds.DrawingColor = Game.GetColorFromRGB(255, 255, 0);
                int x1 = FloatToInt(Checkpoints[FirstCheckpt].pt.x, eRoundNearest) - GetViewportX();
                int y1 = FloatToInt(Checkpoints[FirstCheckpt].pt.y, eRoundNearest) - GetViewportY();
                int x2 = FloatToInt(Checkpoints[LastCheckpt].pt.x, eRoundNearest) - GetViewportX();
                int y2 = FloatToInt(Checkpoints[LastCheckpt].pt.y, eRoundNearest) - GetViewportY();
                ds.DrawLine(x1, y1, x2, y2);
            }
            ds.DrawingColor = Game.GetColorFromRGB(200, 200, 200);
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                if (!Cars[i].IsInit || !Racers[i].IsActive)
                    continue;
                VectorF pos = Cars[i].position;
                VectorF target = Checkpoints[Racers[i].CurRaceNode].pt;
                if (pos == null || target == null)
                    continue;
                int x1 = FloatToInt(pos.x, eRoundNearest) - GetViewportX();
                int y1 = FloatToInt(pos.y, eRoundNearest) - GetViewportY();
                int x2 = FloatToInt(target.x, eRoundNearest) - GetViewportX();
                int y2 = FloatToInt(target.y, eRoundNearest) - GetViewportY();
                ds.DrawLine(x1, y1, x2, y2);
            }
            ds.Release();
        }

        public void late_repeatedly_execute_always()
        {
            if (IsGamePaused())
                return;
            if (!Cars[0].IsInit)
                return;
            if (DisplayDebugInfo)
            {
                String s1 = StringFormatAGS("Accel: %.2f; Power: %.2f; Brake: %.2f; Drive force: %.2f; Impact: %.2f[",Cars[0].Accelerator, Cars[0].EnginePower, Cars[0].brakePower, Cars[0].driveWheelForce, Cars[0].infoImpact.length());
                String s2 = StringFormatAGS("Pos: %.2f, %.2f; Dir: (%.2f) %.2f, %.2f; Velocity: (%.2f) %.2f, %.2f[",Cars[0].position.x, Cars[0].position.y, Cars[0].direction.angle(), Cars[0].direction.x, Cars[0].direction.y, Cars[0].velocity.length(), Cars[0].velocity.x, Cars[0].velocity.y);
                String s3 = StringFormatAGS("Grip: %.2f; AirRes: %.2f; SlideFrict: %.2f; RollFrict: %.2f; CustomTerRes: %.2f; Antiroll: %.2f, Antislide: %.2f[",Cars[0].driveWheelGrip, Track.AirResistance, Cars[0].envSlideFriction, Cars[0].envRollFriction, Cars[0].envResistance,Cars[0].infoRollAntiforce, Cars[0].infoSlideAntiforce);
                String s4 = StringFormatAGS("Steer angle: %.2f, Angular velocity: %.2f, Turning accel: (%.2f) %.2f, %.2f",Cars[0].steeringWheelAngle, Cars[0].angularVelocity, Cars[0].turningAccel.length(), Cars[0].turningAccel.x, Cars[0].turningAccel.y);
                lblCarPos.Text = s1.Append(s2);
                lblCarPos.Text = lblCarPos.Text.Append(s3);
                lblCarPos.Text = lblCarPos.Text.Append(s4);
            }
            if (DisplayDebugOverlay)
            {
                int xoff = -GetViewportX();
                int yoff = -GetViewportY();
                DrawingSurface ds = debugOver.GetDrawingSurface();
                ds.Clear(COLOR_TRANSPARENT);
                int i = 0;
                for (i = 0; i < MAX_RACING_CARS; i += 1)
                {
                    if (!Cars[i].IsInit)
                        continue;
                    ds.DrawingColor = Game.GetColorFromRGB(255, 0, 255);
                    float dirx = Cars[i].direction.x * 100.0f;
                    float diry = Cars[i].direction.y * 100.0f;
                    ds.DrawLine(Cars[i].c.x + xoff, Cars[i].c.y + yoff, Cars[i].c.x + FloatToInt(dirx, eRoundNearest) + xoff, Cars[i].c.y + FloatToInt(diry, eRoundNearest) + yoff);
                    ds.DrawingColor = Game.GetColorFromRGB(0, 255, 255);
                    dirx = Cars[i].velocity.x * 0.4f;
                    diry = Cars[i].velocity.y * 0.4f;
                    ds.DrawLine(Cars[i].c.x + xoff, Cars[i].c.y + yoff, Cars[i].c.x + FloatToInt(dirx, eRoundNearest) + xoff, Cars[i].c.y + FloatToInt(diry, eRoundNearest) + yoff);
                    ds.DrawingColor = Game.GetColorFromRGB(255, 0, 0);
                    ds.DrawLine(FloatToInt(Cars[i].collPoint[0].x, eRoundNearest) + xoff,FloatToInt(Cars[i].collPoint[0].y, eRoundNearest) + yoff,FloatToInt(Cars[i].collPoint[1].x, eRoundNearest) + xoff,FloatToInt(Cars[i].collPoint[1].y, eRoundNearest) + yoff);
                    ds.DrawLine(FloatToInt(Cars[i].collPoint[1].x, eRoundNearest) + xoff,FloatToInt(Cars[i].collPoint[1].y, eRoundNearest) + yoff,FloatToInt(Cars[i].collPoint[2].x, eRoundNearest) + xoff,FloatToInt(Cars[i].collPoint[2].y, eRoundNearest) + yoff);
                    ds.DrawLine(FloatToInt(Cars[i].collPoint[2].x, eRoundNearest) + xoff,FloatToInt(Cars[i].collPoint[2].y, eRoundNearest) + yoff,FloatToInt(Cars[i].collPoint[3].x, eRoundNearest) + xoff,FloatToInt(Cars[i].collPoint[3].y, eRoundNearest) + yoff);
                    ds.DrawLine(FloatToInt(Cars[i].collPoint[3].x, eRoundNearest) + xoff,FloatToInt(Cars[i].collPoint[3].y, eRoundNearest) + yoff,FloatToInt(Cars[i].collPoint[0].x, eRoundNearest) + xoff,FloatToInt(Cars[i].collPoint[0].y, eRoundNearest) + yoff);
                }
                ds.Release();
            }
            if (DisplayDebugAI)
            {
                UpdateDebugAI();
            }
            else if (DisplayDebugRace)
            {
                UpdateDebugRace();
            }
            LastViewportX = GetViewportX();
            LastViewportY = GetViewportY();
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int DEBUG_AI_NODE_RADIUS = 10;

        // Expose DebugOverlay methods so they can be used without instance prefix
        public static void UpdateDebugAI()
        {
            DebugOverlay.UpdateDebugAI();
        }

        public static void UpdateDebugRace()
        {
            DebugOverlay.UpdateDebugRace();
        }

        // Expose DebugOverlay variables so they can be used without instance prefix
        public static bool DisplayDebugOverlay { get { return DebugOverlay.DisplayDebugOverlay; } set { DebugOverlay.DisplayDebugOverlay = value; } }
        public static bool DisplayDebugInfo { get { return DebugOverlay.DisplayDebugInfo; } set { DebugOverlay.DisplayDebugInfo = value; } }
        public static bool DisplayDebugAI { get { return DebugOverlay.DisplayDebugAI; } set { DebugOverlay.DisplayDebugAI = value; } }
        public static bool DisplayDebugRace { get { return DebugOverlay.DisplayDebugRace; } set { DebugOverlay.DisplayDebugRace = value; } }
        public static int SelectedPathNode { get { return DebugOverlay.SelectedPathNode; } set { DebugOverlay.SelectedPathNode = value; } }

    }

    #endregion

}
