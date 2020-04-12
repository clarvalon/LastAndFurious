// Module_RaceUI - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_RaceUI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;
using Microsoft.Xna.Framework.Input;

namespace LastnFurious
{
    public partial class Module_RaceUI
    {
        // Fields
        public int PlayersCarIndex;
        public float UISteeringAngle;

        // Methods
        public void game_start()
        {
            PlayersCarIndex = -1;
            UISteeringAngle = Maths.Pi / 6.0f;
        }

        public void repeatedly_execute_always()
        {
            if (IsGamePaused())
                return;
            if (PlayersCarIndex < 0 || !Cars[PlayersCarIndex].IsInit)
                return;

            var gamePadState = GetGamePadState();

            // Accelerator
            if (IsKeyPressed(eKeyUpArrow))
                Cars[PlayersCarIndex].Accelerator = 1.0f;
            else if (gamePadState.Triggers.Right > 0f)
                Cars[PlayersCarIndex].Accelerator = gamePadState.Triggers.Right;
            else
                Cars[PlayersCarIndex].Accelerator = 0.0f;

            // Brakes
            if (IsKeyPressed(eKeyDownArrow))
                Cars[PlayersCarIndex].Brakes = 1.0f;
            else if (gamePadState.Triggers.Left > 0f)
                Cars[PlayersCarIndex].Brakes = gamePadState.Triggers.Left;
            else 
                Cars[PlayersCarIndex].Brakes = 0.0f;

            // Steering
            float deadzone = 0.1f;
            if (IsKeyPressed(eKeyLeftArrow))
                Cars[PlayersCarIndex].steeringWheelAngle = -UISteeringAngle;
            else if (IsKeyPressed(eKeyRightArrow))
                Cars[PlayersCarIndex].steeringWheelAngle = UISteeringAngle;
            else if (gamePadState.ThumbSticks.Left.X < -deadzone)
                Cars[PlayersCarIndex].steeringWheelAngle = gamePadState.ThumbSticks.Left.X;
            else if (gamePadState.ThumbSticks.Left.X > deadzone)
                Cars[PlayersCarIndex].steeringWheelAngle = gamePadState.ThumbSticks.Left.X;
            else if (gamePadState.DPad.Left == ButtonState.Pressed)
                Cars[PlayersCarIndex].steeringWheelAngle = -UISteeringAngle;
            else if (gamePadState.DPad.Right == ButtonState.Pressed)
                Cars[PlayersCarIndex].steeringWheelAngle = UISteeringAngle;
            else
                Cars[PlayersCarIndex].steeringWheelAngle = 0.0f;
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose RaceUI variables so they can be used without instance prefix
        public static int PlayersCarIndex { get { return RaceUI.PlayersCarIndex; } set { RaceUI.PlayersCarIndex = value; } }
        public static float UISteeringAngle { get { return RaceUI.UISteeringAngle; } set { RaceUI.UISteeringAngle = value; } }

    }

    #endregion

}
