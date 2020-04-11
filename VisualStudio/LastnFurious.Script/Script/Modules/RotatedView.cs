// Module_RotatedView - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_RotatedView;
using static LastnFurious.RotatedViewStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_RotatedView
    {
        // Fields
        public int[] LoopAngles = new int[MAX_DIRECTIONS];

        // Methods
        public void InitLoopAngles()
        {
            LoopAngles[eDirectionRight] = 0;
            LoopAngles[eDirectionDownRight] = 45;
            LoopAngles[eDirectionDown] = 90;
            LoopAngles[eDirectionDownLeft] = 135;
            LoopAngles[eDirectionLeft] = 180;
            LoopAngles[eDirectionUpLeft] = 225;
            LoopAngles[eDirectionUp] = 270;
            LoopAngles[eDirectionUpRight] = 315;
        }

        public void game_start()
        {
            InitLoopAngles();
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int MAX_DIRECTIONS = 8;


    }

    #endregion

    #region RotatedView (AGS struct from .ash converted to class)

    public class RotatedView
    {
        // Methods
        public static int AngleForLoop(CharacterDirection loop)
        {
            return LoopAngles[loop];
        }

        public static DynamicSprite[] CreateLoop(int view, int loop, int base_loop = 0)
        {
            if (loop == base_loop || view >= Game.ViewCount || loop >= Game.GetLoopCountForView(view) || base_loop >= Game.GetLoopCountForView(view))
                return null;
            int rot_angle = LoopAngles[loop] - LoopAngles[base_loop];
            rot_angle = Maths.Angle360(rot_angle);
            int frame = 0;
            int base_frame_count = Game.GetFrameCountForLoop(view, base_loop);
            int dest_frame_count = Game.GetFrameCountForLoop(view, loop);
            int frame_count = Maths.Max(base_frame_count, dest_frame_count);
            DynamicSprite[] sprarr = new DynamicSprite[frame_count];
            for (frame = 0; frame < frame_count; frame += 1)
            {
                ViewFrame vf = Game.GetViewFrame(view, base_loop, frame);
                DynamicSprite spr = DynamicSprite.CreateFromExistingSprite(vf.Graphic);
                spr.Rotate(rot_angle);
                vf = Game.GetViewFrame(view, loop, frame);
                vf.Graphic = spr.Graphic;
                sprarr[frame] = spr;
            }
            return sprarr;
        }

        public static DynamicSprite[] CreateAllLoops(int view, int base_loop = 0)
        {
            if (view >= Game.ViewCount)
                return null;
            int loop = 0;
            int loop_count = Game.GetLoopCountForView(view);
            int frames_total = 0;
            for (loop = 0; loop < loop_count; loop += 1)
            {
                if (loop == base_loop)
                    continue;
                frames_total += Game.GetFrameCountForLoop(view, loop);
            }
            DynamicSprite[] spr_all = new DynamicSprite[frames_total];
            int write_at = 0;
            for (loop = 0; loop < loop_count; loop += 1)
            {
                if (loop == base_loop)
                    continue;
                int frame_count = Game.GetFrameCountForLoop(view, loop);
                DynamicSprite[] spr_loop = RotatedView.CreateLoop(view, loop, base_loop);
                int frame = 0;
                for (frame = 0; frame < frame_count; frame += 1)
                {
                    spr_all[write_at + frame] = spr_loop[frame];
                }
                write_at += frame_count;
            }
            return spr_all;
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class RotatedViewStaticRef
    {
        // Static Fields
        public static int[] LoopAngles { get { return GlobalBase.RotatedViewInstance.LoopAngles; } set { GlobalBase.RotatedViewInstance.LoopAngles = value; } }

        // Static Methods
        public static void InitLoopAngles()
        {
            GlobalBase.RotatedViewInstance.InitLoopAngles();
        }

    }

    #endregion
    
}
