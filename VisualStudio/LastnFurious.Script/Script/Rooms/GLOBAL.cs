// Room_GLOBAL - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using static LastnFurious.Room_GLOBAL;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Room_GLOBAL
    {
        // Fields

        // Methods
        public override void game_start()
        {
            Mouse.Mode = eModePointer;
            Mouse.Visible = false;
            DynamicSprite font = DynamicSprite.CreateFromExistingSprite(13);
            AzureItalicFont.CreateFromSprite(font, 13, 13, 0, 126, null, null);
            font.Delete();
            font = DynamicSprite.CreateFromExistingSprite(22);
            PurpleItalicFont.CreateFromSprite(font, 13, 13, 0, 126, null, null);
            font.Delete();
            font = DynamicSprite.CreateFromExistingSprite(14);
            int last = 126;
            int total = last + 1;
            int[] offs = new int[total];
            int[] widths = new int[total];
            int i = 0;
            for (i = 0; i < total; i += 1)
            {
                offs[i] = 0;
            }
            for (i = 0; i < total; i += 1)
            {
                widths[i] = 28;
            }
            offs['t'] = 3;
            offs['!'] = 9;
            offs['`'] = 9;
            offs['.'] = 9;
            offs[','] = 9;
            offs['I'] = 9;
            offs['i'] = 9;
            offs['j'] = 0;
            offs['l'] = 9;
            offs[':'] = 9;
            offs[';'] = 9;
            offs['|'] = 9;
            widths['t'] -= (3 + 7) - 4;
            widths['!'] -= 18 - 4;
            widths['`'] -= 18 - 4;
            widths['.'] -= 18 - 4;
            widths[','] -= 18 - 4;
            widths['I'] -= 18 - 4;
            widths['i'] -= 18 - 4;
            widths['j'] -= 9 - 4;
            widths['l'] -= 18 - 4;
            widths[':'] -= 18 - 4;
            widths[';'] -= 18 - 4;
            widths['|'] -= 18 - 4;
            SilverFont.CreateFromSprite(font, 32, 34, 0, last, offs, widths);
            font.Delete();
            SetDefaultOptions();
            LoadOptions();
        }

        public override void repeatedly_execute()
        {
        }

        public override void repeatedly_execute_always()
        {
        }

        public override void on_key_press(eKeyCode keycode)
        {
            if (IsDebugMode)
            {
                if (keycode == eKeyQ)
                {
                    DisplayDebugInfo = !DisplayDebugInfo;
                    gDebugInfo.Visible = !gDebugInfo.Visible;
                }
                if (keycode == eKeyW)
                {
                    DisplayDebugOverlay = !DisplayDebugOverlay;
                    gDebugOver.Visible = !gDebugOver.Visible;
                }
                if (keycode == eKeyE)
                {
                    DisplayDebugAI = !DisplayDebugAI;
                    DisplayDebugRace = !DisplayDebugAI;
                    gDebugAI.Visible = DisplayDebugAI;
                }
                if (keycode == eKeyR)
                {
                    DisplayDebugRace = !DisplayDebugRace;
                    DisplayDebugAI = !DisplayDebugRace;
                    gDebugAI.Visible = DisplayDebugRace;
                }
                if (keycode == eKeyA)
                {
                    if (RaceBuilderEnabled)
                        EnableRaceBuilder(false);
                    EnableAIBuilder(!AIBuilderEnabled);
                }
                else if (keycode == eKeyZ)
                {
                    if (AIBuilderEnabled)
                        EnableAIBuilder(false);
                    EnableRaceBuilder(!RaceBuilderEnabled);
                }
                if (keycode == eKeyCtrlS)
                {
                    if (AIBuilderEnabled)
                        SaveAIPaths();
                    else if (RaceBuilderEnabled)
                        SaveRaceCheckpoints();
                }
                if (keycode == eKeyCtrlL)
                {
                    LoadRaceCheckpoints();
                    LoadAIPaths();
                }
            }
            if (keycode == eKeyF12)
                SaveScreenShot("$SAVEGAMEDIR$/screenshot.bmp");
            if (keycode == eKeyCtrlV)
                Debug(1,0);
            if (keycode == eKeyCtrlA)
                Debug(2,0);
            if (keycode == eKeyCtrlX)
                Debug(3,0);
        }

        public override void on_mouse_click(MouseButton button)
        {
        }

        public void dialog_request(int param)
        {
        }

        public void ReadRaceConfig()
        {
            String cfg_file = null;
            if (ThisRace.AiAndPhysics == ePhysicsWild)
                cfg_file = "race_wild.ini";
            else 
                cfg_file = "race_safe.ini";
            IniFile ini = new IniFile();
            if (!ini.Load(StringFormatAGS("$APPDATADIR$/Data/%s", cfg_file)))
            {
                if (!ini.Load(StringFormatAGS("$INSTALLDIR$/Data/%s", cfg_file)))
                    return;
            }
            Track.Gravity = ini.ReadFloat("track", "gravity", Track.Gravity);
            Track.AirResistance = ini.ReadFloat("track", "air_resistance", Track.AirResistance);
            int i = 0;
            for (i = 0; i < MAX_WALKABLE_AREAS; i += 1)
            {
                String section = StringFormatAGS("area%d", i);
                if (!ini.SectionExists(section))
                    continue;
                Track.IsObstacle[i] = ini.ReadBool(section, "is_obstacle", Track.IsObstacle[i]);
                Track.TerraSlideFriction[i] = ini.ReadFloat(section, "slide_friction", Track.TerraSlideFriction[i]);
                Track.TerraRollFriction[i] = ini.ReadFloat(section, "roll_friction", Track.TerraRollFriction[i]);
                Track.TerraGrip[i] = ini.ReadFloat(section, "grip", Track.TerraGrip[i]);
                Track.EnvResistance[i] = ini.ReadFloat(section, "env_resistance", Track.EnvResistance[i]);
            }
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                Cars[i].bodyLength = ini.ReadFloat("car", "bodyLength", Cars[i].bodyLength);
                Cars[i].bodyWidth = ini.ReadFloat("car", "bodyLength", Cars[i].bodyWidth);
                Cars[i].distanceBetweenAxles = ini.ReadFloat("car", "distanceBetweenAxles", Cars[i].distanceBetweenAxles);
                Cars[i].bodyMass = ini.ReadFloat("car", "bodyMass", Cars[i].bodyMass);
                Cars[i].bodyAerodynamics = ini.ReadFloat("car", "bodyAerodynamics", Cars[i].bodyAerodynamics);
                Cars[i].hardImpactLossFactor = ini.ReadFloat("car", "hardImpactLossFactor", Cars[i].hardImpactLossFactor);
                Cars[i].softImpactLossFactor = ini.ReadFloat("car", "softImpactLossFactor", Cars[i].softImpactLossFactor);
                Cars[i].engineMaxPower = ini.ReadFloat("car", "engineMaxPower", Cars[i].engineMaxPower);
                Cars[i].stillTurningVelocity = ini.ReadFloat("car", "stillTurningVelocity", Cars[i].stillTurningVelocity);
                Cars[i].driftVelocityFactor = ini.ReadFloat("car", "driftVelocityFactor", Cars[i].driftVelocityFactor);
            }
            UISteeringAngle = Maths.DegreesToRadians(ini.ReadFloat("car_control", "steeringAngle", Maths.RadiansToDegrees(UISteeringAngle)));
            DisplayDebugOverlay = ini.ReadBool("debug", "displayDebugOverlay", DisplayDebugOverlay);
            DisplayDebugInfo = ini.ReadBool("debug", "displayDebugInfo", DisplayDebugInfo);
            gDebugOver.Visible = DisplayDebugOverlay;
            gDebugInfo.Visible = DisplayDebugInfo;
        }

        // Expose Global Variables

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose GLOBAL methods so they can be used without instance prefix
        public static void ReadRaceConfig()
        {
            GLOBAL.ReadRaceConfig();
        }


    }

    #endregion

}
