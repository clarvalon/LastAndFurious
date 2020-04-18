// Module_GameMenu - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_GameMenu;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;
using Microsoft.Xna.Framework.Input;

namespace LastnFurious
{
    public partial class Module_GameMenu
    {
        // Fields
        public MenuClass MenuType = default(MenuClass);
        public MenuClass PrevMenuType = default(MenuClass);
        public int MMOptionCount;
        public int MMSelection;
        public DynamicSprite SprOptions;
        public int MusicVol;
        public int EffectsVol;
        public SpriteFont SilverFont = new SpriteFont();
        public SpriteFont AzureItalicFont = new SpriteFont();
        public SpriteFont PurpleItalicFont = new SpriteFont();
        public bool IsDebugMode;

        // Methods
        public void SetMusic(int vol)
        {
            vol = Maths.Clamp(vol, 0, 100);
            MusicVol = vol;
            Game.SetAudioTypeVolume(eAudioTypeMusic, vol, eVolExistingAndFuture);
        }

        public void SetEffects(int vol)
        {
            vol = Maths.Clamp(vol, 0, 100);
            EffectsVol = vol;
            Game.SetAudioTypeVolume(eAudioTypeSound,  vol, eVolExistingAndFuture);
        }

        public void SetDefaultOptions()
        {
            
#if  DEBUG

            IsDebugMode = true;
            
#endif

            
#if !  DEBUG

            IsDebugMode = false;
            
#endif

            SetMusic(100);
            SetEffects(100);
            ThisRace.PlayerDriver = 0;
            ThisRace.Laps = 3;
            ThisRace.Opponents = 3;
            ThisRace.AiAndPhysics = ePhysicsSafe;
            ThisRace.CarCollisions = true;
        }

        public String GetAiAndPhysicsString(AiAndPhysicsOption option)
        {
            switch (option)
            {
                case ePhysicsSafe: return "safe";
                case ePhysicsWild: return "wild";
            }
            return "";
        }

        public AiAndPhysicsOption GetAiAndPhysicsOption(String option)
        {
            if (option.CompareTo("safe", false) == 0)
                return ePhysicsSafe;
            if (option.CompareTo("wild", false) == 0)
                return ePhysicsWild;
            return ePhysicsSafe;
        }

        public void LoadOptions()
        {
            IniFile ini = new IniFile();
            if (!ini.Load("$SAVEGAMEDIR$/options.ini"))
                return;
            IsDebugMode = ini.ReadBool("main", "debug_mode", IsDebugMode);
            SetMusic(ini.ReadInt("main", "music", MusicVol));
            SetEffects(ini.ReadInt("main", "sound", EffectsVol));
            ThisRace.PlayerDriver = ini.ReadInt("race", "driver", ThisRace.PlayerDriver);
            ThisRace.Laps = ini.ReadInt("race", "laps", ThisRace.Laps);
            String value = GetAiAndPhysicsString(ThisRace.AiAndPhysics);
            value = ini.Read("race", "physics", value);
            ThisRace.AiAndPhysics = GetAiAndPhysicsOption(value);
            ThisRace.Opponents = ini.ReadInt("race", "opponents", ThisRace.Opponents);
            ThisRace.CarCollisions = ini.ReadBool("race", "car_collisions", ThisRace.CarCollisions);
        }

        public void SaveOptions()
        {
            IniFile ini = new IniFile();
            ini.WriteBool("main", "debug_mode", IsDebugMode);
            ini.WriteInt("main", "music", MusicVol);
            ini.WriteInt("main", "sound", EffectsVol);
            ini.WriteInt("race", "driver", ThisRace.PlayerDriver);
            ini.WriteInt("race", "laps", ThisRace.Laps);
            ini.WriteInt("race", "opponents", ThisRace.Opponents);
            ini.Write("race", "physics", GetAiAndPhysicsString(ThisRace.AiAndPhysics));
            ini.WriteBool("race", "car_collisions", ThisRace.CarCollisions);
            ini.Save("$SAVEGAMEDIR$/options.ini");
        }

        public void UpdateSelection()
        {
            if (MenuType == eMenuStart || MenuType == eMenuLogin)
            {
                btnMMSelector.X = DIAMOND_X;
                btnMMSelector.Y = STARTMENU_OPTION_POS_TOP + MMSelection * STARTMENU_OPTION_SPACING + OPTION_HEIGHT / 2 - Game.SpriteHeight[btnMMSelector.Graphic] / 2;
            }
            else if (MenuType != eMenuCredits)
            {
                btnMMSelector.X = 0;
                btnMMSelector.Y = OPTION_POS_TOP + MMSelection * OPTION_SPACING + SELECTOR_Y_OFF;
            }
        }

        public void UpdateOptionValues()
        {
            if (MenuType == eMenuStart || MenuType == MenuClass.eMenuLogin)
                return;
            DrawingSurface ds = SprOptions.GetDrawingSurface();
            ds.DrawingColor = COLOR_TRANSPARENT;
            ds.DrawRectangle(OPTION_VALUE_X, 0, SprOptions.Width - 1, SprOptions.Height - 1);
            String value = null;
            int portrait_sprite = 100 + ThisRace.PlayerDriver;
            int portrait_x = OPTION_VALUE_X + SilverFont.GlyphWidth * 5 / 2 - 10 - Game.SpriteWidth[2] - 5;
            int portrait_y = OPTION_POS_TOP + OPTION_SPACING + OPTION_HEIGHT / 2 - Game.SpriteHeight[2] / 2;
            int car_sprite = 7 + ThisRace.PlayerDriver;
            int car_x = OPTION_VALUE_X + SilverFont.GlyphWidth * 5 / 2 - 10 + 5;
            int car_y = portrait_y;
            int car_xoff = (Game.SpriteWidth[2] - Game.SpriteWidth[car_sprite]) / 2;
            int car_yoff = (Game.SpriteHeight[2] - Game.SpriteHeight[car_sprite]) / 2;
            switch (MenuType) 
            {
                case eMenuMain:case eMenuMainInGame:if (MusicVol == 0)value = " OFF >";
                else if (MusicVol == 100)
                    value = "< FULL";
                else 
                    value = StringFormatAGS("< %02d >", MusicVol);
                SilverFont.DrawText(value, ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING * 2);
                break;
                case eMenuSetupRace:SilverFont.DrawText("<   >", ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING);
                ds.DrawImage(portrait_x, portrait_y, 2);
                ds.DrawImage(portrait_x + 2, portrait_y + 2, portrait_sprite);
                value = StringFormatAGS("< %d >", ThisRace.Laps);
                SilverFont.DrawText(value, ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING * 2);
                value = StringFormatAGS("< %d >", ThisRace.Opponents);
                SilverFont.DrawText(value, ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING * 3);
                if (ThisRace.AiAndPhysics == ePhysicsWild)
                    value = "Wild";
                else 
                    value = "Safe";
                SilverFont.DrawText(value, ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING * 4);
                if (ThisRace.CarCollisions)
                    value = "ON";
                else 
                    value = "OFF";
                SilverFont.DrawText(value, ds, OPTION_VALUE_X, OPTION_POS_TOP + OPTION_SPACING * 5);
                ds.DrawImage(car_x, car_y, 2);
                ds.DrawImage(car_x + car_xoff, car_y + car_yoff, car_sprite);
                break;
            }
            ds.Release();
            btnMenuOptions.NormalGraphic = SprOptions.Graphic;
        }

        public void ArrangeMenu()
        {
            if (SprOptions == null)
                SprOptions = DynamicSprite.Create(gGameMenu.Width, gGameMenu.Height);
            DrawingSurface ds = SprOptions.GetDrawingSurface();
            ds.Clear();
            int y = 0;
            switch (MenuType)
            {
                case eMenuLogin:
                    if (PlatformService.IsSigningIn)
                    {
                        SilverFont.DrawTextCentered($"Signing into {PlatformService.ServiceName}", ds, 0, STARTMENU_OPTION_POS_TOP, 640);
                    }
                    else
                    {
                        if (System.OperatingSystem == eOSXboxUWP)
                            SilverFont.DrawTextCentered("Press A", ds, 0, STARTMENU_OPTION_POS_TOP, 640);
                        else
                            SilverFont.DrawTextCentered("Press Any Key", ds, 0, STARTMENU_OPTION_POS_TOP, 640);
                        MMOptionCount = 1;
                    }
                    break;
                case eMenuStart:
                    SilverFont.DrawText("Start", ds, STARTMENU_OPTION_X, STARTMENU_OPTION_POS_TOP);
                    SilverFont.DrawText("Credits", ds, STARTMENU_OPTION_X, STARTMENU_OPTION_POS_TOP + STARTMENU_OPTION_SPACING);
                    SilverFont.DrawText("Quit", ds, STARTMENU_OPTION_X, STARTMENU_OPTION_POS_TOP + STARTMENU_OPTION_SPACING * 2);
                    MMOptionCount = 3;
                    if (PlatformService.IsSignedIn)
                        PurpleItalicFont.DrawTextCentered(PlatformService.GamerTag, ds, 0, STARTMENU_OPTION_POS_TOP - 20, 640);
                    break;
                case eMenuMain:
                    SilverFont.DrawText("Race", ds, OPTION_X, OPTION_POS_TOP);
                    SilverFont.DrawText("Watch Demo", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING);
                    SilverFont.DrawText("Music", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 2);
                    SilverFont.DrawText("Quit", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 3);
                    MMOptionCount = 4;
                    break;
                case eMenuMainInGame:
                    SilverFont.DrawText("Continue", ds, OPTION_X, OPTION_POS_TOP);
                    SilverFont.DrawText("Restart", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING);
                    SilverFont.DrawText("Music", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 2);
                    SilverFont.DrawText("Quit", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 3);
                    MMOptionCount = 4;
                    break;
                case eMenuSetupRace:
                    SilverFont.DrawText("Go!", ds, OPTION_X, OPTION_POS_TOP);
                    SilverFont.DrawText("Driver", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING);
                    SilverFont.DrawText("Laps", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 2);
                    SilverFont.DrawText("Opponents", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 3);
                    SilverFont.DrawText("Physics", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 4);
                    SilverFont.DrawText("Collisions", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 5);
                    SilverFont.DrawText("Back", ds, OPTION_X, OPTION_POS_TOP + OPTION_SPACING * 6);
                    MMOptionCount = 7;
                    break;
                case eMenuCredits:
                    ds.DrawingColor = Game.GetColorFromRGB(11, 15, 54);
                    ds.DrawRectangle(0, 0, 640, 400);
                    y = 40;
                    PurpleItalicFont.DrawTextCentered("CODE", ds, 0, y, ds.Width);
                    y += PurpleItalicFont.Height + 10;
                    AzureItalicFont.DrawTextCentered("Crimson Wizard", ds, 0, y, ds.Width);
                    y += 40;
                    PurpleItalicFont.DrawTextCentered("ART & TECH IDEAS", ds, 0, y, ds.Width);
                    y += PurpleItalicFont.Height + 10;
                    AzureItalicFont.DrawTextCentered("Jim Reed", ds, 0, y, ds.Width);
                    y += 40;
                    PurpleItalicFont.DrawTextCentered("MUSIC", ds, 0, y, ds.Width);
                    y += PurpleItalicFont.Height + 10;
                    AzureItalicFont.DrawTextCentered("\"Car Theft 101\" by Eric Matyas", ds, 0, y, ds.Width);
                    y += AzureItalicFont.Height;
                    AzureItalicFont.DrawTextCentered("www.soundimage.org", ds, 0, y, ds.Width);
                    y += AzureItalicFont.Height + 10;
                    AzureItalicFont.DrawTextCentered("\"Welcome to the Show\" by Kevin MacLeod", ds, 0, y, ds.Width);
                    y += AzureItalicFont.Height;
                    AzureItalicFont.DrawTextCentered("incompetech.com", ds, 0, y, ds.Width);
                    y += AzureItalicFont.Height + 40;
                    PurpleItalicFont.DrawTextCentered("PORT TO C# / XAGE", ds, 0, y, ds.Width);
                    y += PurpleItalicFont.Height + 10;
                    AzureItalicFont.DrawTextCentered("Dan Alexander", ds, 0, y, ds.Width);
                    y += 40;
                    PurpleItalicFont.DrawTextCentered("Press any key to continue", ds, 0, STARTMENU_OPTION_POS_TOP + STARTMENU_OPTION_SPACING * 2, ds.Width);
                    break;
            }
            ds.Release();
            if (MenuType != eMenuCredits)
                UpdateOptionValues();
            btnMenuOptions.NormalGraphic = SprOptions.Graphic;
            btnMenuOptions.Visible = true;
            if (MenuType == eMenuLogin)
            {
                btnMMSelector.Visible = false;
                btnMMVrStrip.Visible = false;
                gUnderlay.Visible = false;
            }
            else if (MenuType == eMenuStart)
            {
                btnMMSelector.NormalGraphic = 1;
                btnMMSelector.Visible = true;
                btnMMVrStrip.Visible = false;
                gUnderlay.Visible = false;
            }
            else if (MenuType == eMenuCredits)
            {
                btnMMSelector.Visible = false;
                btnMMVrStrip.Visible = false;
                gUnderlay.Visible = false;
            }
            else 
            {
                btnMMSelector.NormalGraphic = 4;
                btnMMSelector.Visible = true;
                btnMMVrStrip.Visible = true;
                gUnderlay.Visible = true;
            }
        }

        public void DisplayGameMenu(MenuClass menu, bool pausedInGame)
        {
            MenuType = menu;
            if (pausedInGame && !IsGamePaused())
            {
                PauseGame();
                gRaceOverlay.Visible = false;
                gBanner.Visible = false;
            }
            else 
            {
            }
            AllButtonsState(gGameMenu, false, false);
            ArrangeMenu();
            MMSelection = 0;
            UpdateSelection();
            gGameMenu.Visible = true;
        }

        public void HideGameMenu()
        {
            if (!gGameMenu.Visible)
                return;
            SaveOptions();
            AllButtonsState(gGameMenu, false, false);
            gGameMenu.Visible = false;
            btnMenuOptions.NormalGraphic = 0;
            SprOptions.Delete();
            SprOptions = null;
            gUnderlay.Visible = false;
            MenuType = eMenuNone;
            if (IsGamePaused())
            {
                gRaceOverlay.Visible = true;
                gBanner.Visible = true;
                UnPauseGame();
            }
        }

        public void OnToMainMenu()
        {
            HideGameMenu();
            Wait(1);
            player.ChangeRoom(305);
        }

        public void OnQuit()
        {
            SaveOptions();
            QuitGame(0);
        }

        public void SwitchToMenu(MenuClass menu)
        {
            PrevMenuType = MenuType;
            DisplayGameMenu(menu, IsGamePaused());
        }

        public void OnWatchDemo()
        {
            HideGameMenu();
        }

        public void OnDriverChange(bool prev)
        {
            if (prev)
            {
                if (ThisRace.PlayerDriver > 0)
                    ThisRace.PlayerDriver -= 1;
            }
            else 
            {
                if (ThisRace.PlayerDriver < 5)
                    ThisRace.PlayerDriver += 1;
            }
            UpdateOptionValues();
        }

        public void OnLapsChange(bool prev)
        {
            if (prev)
            {
                if (ThisRace.Laps > 1)
                    ThisRace.Laps -= 1;
            }
            else 
            {
                if (ThisRace.Laps < 9)
                    ThisRace.Laps += 1;
            }
            UpdateOptionValues();
        }

        public void OnOpponents(bool prev)
        {
            if (prev)
            {
                if (ThisRace.Opponents > 0)
                    ThisRace.Opponents -= 1;
            }
            else 
            {
                if (ThisRace.Opponents < MAX_RACING_CARS - 1)
                    ThisRace.Opponents += 1;
            }
            UpdateOptionValues();
        }

        public void OnPhysicsType()
        {
            if (ThisRace.AiAndPhysics == ePhysicsSafe)
                ThisRace.AiAndPhysics = ePhysicsWild;
            else 
                ThisRace.AiAndPhysics = ePhysicsSafe;
            UpdateOptionValues();
        }

        public void OnCollisions()
        {
            ThisRace.CarCollisions = !ThisRace.CarCollisions;
            UpdateOptionValues();
        }

        public void OnMusic(bool down)
        {
            if (down)
                SetMusic(MusicVol - 5);
            else 
                SetMusic(MusicVol + 5);
            if (gGameMenu.Visible)
                UpdateOptionValues();
        }

        public void OnEffects(bool down)
        {
            if (down)
                SetEffects(EffectsVol - 5);
            else 
                SetEffects(EffectsVol + 5);
            if (gGameMenu.Visible)
                UpdateOptionValues();
        }

        public void OnGo()
        {
            HideGameMenu();
            CallRoomScript(eRoom305_StartSinglePlayer);
        }

        public void StartAsyncLogIn()
        {
            PlatformService.SignInAsync();
            SwitchToMenu(eMenuLogin); // Redraw 
        }

        public void ConfirmSelection()
        {
            if (MenuType == eMenuLogin)
            {
                StartAsyncLogIn();
                return;
            }
            else if (MenuType == eMenuStart)
            {
                switch (MMSelection) 
                {
                    case 0: OnToMainMenu();
                    break;
                    case 1: SwitchToMenu(eMenuCredits);
                    break;
                    case 2: OnQuit();
                    break;
                }
            }
            else if (MenuType == eMenuMain)
            {
                switch (MMSelection) 
                {
                    case 0: SwitchToMenu(eMenuSetupRace);
                    break;
                    case 1: OnWatchDemo();
                    break;
                    case 2: OnMusic(false);
                    break;
                    case 3: OnQuit();
                    break;
                }
            }
            else if (MenuType == eMenuSetupRace)
            {
                switch (MMSelection) 
                {
                    case 0: OnGo();
                    break;
                    case 1: OnDriverChange(false);
                    break;
                    case 2: OnLapsChange(false);
                    break;
                    case 3: OnOpponents(false);
                    break;
                    case 4: OnPhysicsType();
                    break;
                    case 5: OnCollisions();
                    break;
                    case 6: SwitchToMenu(PrevMenuType);
                    break;
                }
            }
            else if (MenuType == eMenuMainInGame)
            {
                switch (MMSelection) 
                {
                    case 0: HideGameMenu();
                    break;
                    case 1: SwitchToMenu(eMenuSetupRace);
                    break;
                    case 2: OnMusic(false);
                    break;
                    case 3: OnQuit();
                    break;
                }
            }
            else if (MenuType == eMenuCredits)
            {
                SwitchToMenu(eMenuStart);
            }
        }

        public void ChangeOption(bool left)
        {
            if (MenuType == eMenuMain || MenuType == eMenuMainInGame)
            {
                switch (MMSelection) 
                {
                    case 2: OnMusic(left);
                    break;
                }
            }
            else if (MenuType == eMenuSetupRace)
            {
                switch (MMSelection) 
                {
                    case 1: OnDriverChange(left);
                    break;
                    case 2: OnLapsChange(left);
                    break;
                    case 3: OnOpponents(left);
                    break;
                    case 4: OnPhysicsType();
                    break;
                    case 5: OnCollisions();
                    break;
                }
            }
        }

        public void CancelMenu()
        {
            if (MenuType == eMenuMain || MenuType == eMenuMainInGame)
            {
                HideGameMenu();
            }
            else if (MenuType == eMenuSetupRace)
            {
                SwitchToMenu(PrevMenuType);
            }
        }

        public void on_button_press(Buttons button) 
        {
            if (!gGameMenu.Visible)
                return;
            
            if (MenuType == eMenuCredits)
            {
                SwitchToMenu(eMenuStart);
                return;
            }
            if (button == Buttons.B || button == Buttons.Back)
            {
                CancelMenu();
            }
            else if (button == Buttons.DPadUp || button == Buttons.LeftThumbstickUp)
            {
                if (MMSelection > 0)
                {
                    MMSelection -= 1;
                    UpdateSelection();
                }
            }
            else if (button == Buttons.DPadDown || button == Buttons.LeftThumbstickDown)
            {
                if (MMSelection < MMOptionCount - 1)
                {
                    MMSelection += 1;
                    UpdateSelection();
                }
            }
            else if (button == Buttons.DPadLeft || button == Buttons.LeftThumbstickLeft)
            {
                ChangeOption(true);
            }
            else if (button == Buttons.DPadRight || button == Buttons.LeftThumbstickRight)
            {
                ChangeOption(false);
            }
            else if (button == Buttons.A || button == Buttons.Start)
            {
                ConfirmSelection();
            }
            ClaimEvent();
        }

        public void on_key_press(eKeyCode key) 
        {
            if (!gGameMenu.Visible)
                return;
            if (MenuType == eMenuCredits)
            {
                SwitchToMenu(eMenuStart);
                return;
            }
            if (key == eKeyEscape)
            {
                CancelMenu();
            }
            else if (key == eKeyUpArrow)
            {
                if (MMSelection > 0)
                {
                    MMSelection -= 1;
                    UpdateSelection();
                }
            }
            else if (key == eKeyDownArrow)
            {
                if (MMSelection < MMOptionCount - 1)
                {
                    MMSelection += 1;
                    UpdateSelection();
                }
            }
            else if (key == eKeyLeftArrow)
            {
                ChangeOption(true);
            }
            else if (key == eKeyRightArrow)
            {
                ChangeOption(false);
            }
            else if (key == eKeyReturn || key == eKeySpace)
            {
                ConfirmSelection();
            }
            ClaimEvent();
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int OPTION_HEIGHT = 28;
        public const int DIAMOND_X = 219;
        public const int STARTMENU_OPTION_POS_TOP = 303;
        public const int STARTMENU_OPTION_X = 255;
        public const int STARTMENU_OPTION_SPACING = 32;
        public const int OPTION_POS_TOP = 50;
        public const int OPTION_X = 97;
        public const int OPTION_SPACING = 50;
        public const int OPTION_VALUE_X = 371;
        public const int SELECTOR_Y_OFF = -10;

        // Expose Enums and instances of each
        public enum MenuClass
        {
            eMenuNone = 0, 
            eMenuStart = 1, 
            eMenuMain = 2, 
            eMenuMainInGame = 3, 
            eMenuSetupRace = 4, 
            eMenuCredits = 5,
            eMenuLogin = 6
        }
        public const MenuClass eMenuNone = MenuClass.eMenuNone;
        public const MenuClass eMenuLogin = MenuClass.eMenuLogin;
        public const MenuClass eMenuStart = MenuClass.eMenuStart;
        public const MenuClass eMenuMain = MenuClass.eMenuMain;
        public const MenuClass eMenuMainInGame = MenuClass.eMenuMainInGame;
        public const MenuClass eMenuSetupRace = MenuClass.eMenuSetupRace;
        public const MenuClass eMenuCredits = MenuClass.eMenuCredits;

        public class Room305Events
        {
            // Empty constructor for serialization
            public Room305Events()
            {
            }
            public int Value; // Should be readonly but is public to allow serialisation

            public static readonly Room305Events eRoom305_StartSinglePlayer = new Room305Events(0);
            public static readonly Room305Events eRoom305_StartAIDemo = new Room305Events(1);

            [XmlIgnore]
            private string DisplayValue
            {
                get
                {
                    Room305Events e = this;
                    if (e.Value == 0)
                        return "eRoom305_StartSinglePlayer";
                    if (e.Value == 1)
                        return "eRoom305_StartAIDemo";
                    return "?";
                }
            }

            public Room305Events(int value)
            {
                Value = value;
            }

            public static implicit operator Room305Events(int value)
            {
                if (value == 0)
                    return eRoom305_StartSinglePlayer;
                if (value == 1)
                    return eRoom305_StartAIDemo;
                return eRoom305_StartSinglePlayer;
            }

            public static implicit operator int (Room305Events value)
            {
                return value.Value;
            }

        }

        // Instances
        public static readonly Room305Events eRoom305_StartSinglePlayer = Room305Events.eRoom305_StartSinglePlayer;
        public static readonly Room305Events eRoom305_StartAIDemo = Room305Events.eRoom305_StartAIDemo;
        // Expose GameMenu methods so they can be used without instance prefix
        public static void DisplayGameMenu(MenuClass menu, bool pausedInGame = false)
        {
            GameMenu.DisplayGameMenu(menu, pausedInGame);
        }

        public static void HideGameMenu()
        {
            GameMenu.HideGameMenu();
        }

        public static void SetMusic(int vol)
        {
            GameMenu.SetMusic(vol);
        }

        public static void SetEffects(int vol)
        {
            GameMenu.SetEffects(vol);
        }

        public static void SetDefaultOptions()
        {
            GameMenu.SetDefaultOptions();
        }

        public static void LoadOptions()
        {
            GameMenu.LoadOptions();
        }

        public static void SaveOptions()
        {
            GameMenu.SaveOptions();
        }

        // Expose GameMenu variables so they can be used without instance prefix
        public static bool IsDebugMode { get { return GameMenu.IsDebugMode; } set { GameMenu.IsDebugMode = value; } }
        public static int MusicVol { get { return GameMenu.MusicVol; } set { GameMenu.MusicVol = value; } }
        public static int EffectsVol { get { return GameMenu.EffectsVol; } set { GameMenu.EffectsVol = value; } }
        public static SpriteFont SilverFont { get { return GameMenu.SilverFont; } set { GameMenu.SilverFont = value; } }
        public static SpriteFont AzureItalicFont { get { return GameMenu.AzureItalicFont; } set { GameMenu.AzureItalicFont = value; } }
        public static SpriteFont PurpleItalicFont { get { return GameMenu.PurpleItalicFont; } set { GameMenu.PurpleItalicFont = value; } }

    }

    #endregion

}
