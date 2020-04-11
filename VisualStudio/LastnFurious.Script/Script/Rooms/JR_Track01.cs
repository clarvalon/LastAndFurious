// Room_JR_Track01 - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using static LastnFurious.Room_JR_Track01;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Room_JR_Track01 // 305
    {
        // Fields
        public bool IsAIRace;
        public Timer tChangeAICamera = new Timer();
        public DynamicSprite RaceOverlay;
        public int RaceStartSequence;
        public Timer tSequence = new Timer();
        public int RaceEndSequence;
        public VectorF[] StartingGrid = new VectorF[MAX_RACING_CARS];

        // Methods
        public void CameraTargetPlayerCar(bool snap)
        {
            Camera.TargettingAcceleration = 0.0f;
            Camera.TargetCharacter = player;
            if (snap)
                Camera.Snap();
        }

        public void CameraTargetRandomAICar(bool snap)
        {
            Camera.TargettingAcceleration = 0.5f;
            Camera.TargetCharacter = character[cAICar1.ID + Random(5)];
            if (snap)
                Camera.Snap();
        }

        public void ClearRace()
        {
            gRaceOverlay.Visible = false;
            gRaceOverlay.BackgroundGraphic = 0;
            gRaceOverlay.Transparency = 0;
            if (RaceOverlay != null)
                RaceOverlay.Delete();
            ResetAI();
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                Cars[i].UnInit();
                Racers[i].Reset();
            }
            player.ChangeView(CARVIEWDUMMY);
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                character[cAICar1.ID + i].ChangeView(CARVIEWDUMMY);
                character[cAICar1.ID + i].ChangeRoom(-1);
            }
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                ThisRace.DriverPositions[i] = -1;
                ThisRace.RacersFinished = 0;
            }
        }

        public void DrawRacersTime(int racer, DrawingSurface ds, int x, int y)
        {
            float total_time = Racers[racer].Time;
            int minutes = FloatToInt(total_time / 60.0f, eRoundDown);
            int seconds = FloatToInt(total_time - IntToFloat(minutes) * 60.0f, eRoundDown);
            int subsecs = FloatToInt((total_time - IntToFloat(minutes) * 60.0f - IntToFloat(seconds)) * 60.0f, eRoundDown);
            AzureItalicFont.DrawText(StringFormatAGS("%02d:%02d:%02d", minutes, seconds, subsecs), ds, x, y);
        }

        public void DrawRaceOverlay(bool firstDraw)
        {
            //int COLOR_TRANSPARENT = 35000;
            DrawingSurface ds = RaceOverlay.GetDrawingSurface();
            if (firstDraw)
            {
                int y1 = (1 + ThisRace.Opponents) * 53;
                int y2 = 6 * 53;
                if (y1 < y2)
                {
                    ds.DrawingColor = COLOR_TRANSPARENT;
                    ds.DrawRectangle(0, y1, 160, y2);
                }
            }
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                int racer = ThisRace.DriverPositions[i];
                if (racer < 0 || !Racers[racer].IsActive)
                    continue;
                int x = 0;
                int y = 53 * i;
                ds.DrawImage(x + 4, y + 2, 100 + Racers[racer].Driver);
                if (Racers[racer].Finished > 0)
                {
                    ds.DrawingColor = COLOR_TRANSPARENT;
                    ds.DrawRectangle(x + 36, y + 17, x + 102, y + 51);
                    DrawRacersTime(racer, ds, x + 42, y + 22);
                }
            }
            ds.DrawingColor = COLOR_TRANSPARENT;
            ds.DrawRectangle(50, 369, 160, 400);
            AzureItalicFont.DrawText(StringFormatAGS("%02d/%02d", Racers[0].Lap, ThisRace.Laps), ds, 52, 377);
            DrawRacersTime(0, ds, 52, 389);
            ds.Release();
        }

        public void Shuffle(int[] ints, int length)
        {
            if (length < 2)
                return;
            int times = length * 2 + 1;
            for (; times > 0; times -= 1)
            {
                int index1 = Random(length - 1);
                int index2 = Random(length - 1);
                int temp = ints[index1];
                ints[index1] = ints[index2];
                ints[index2] = temp;
            }
            return;
        }

        public VectorF PositionCarOnGrid(int car, int gridpos)
        {
            VectorF pos = StartingGrid[gridpos].clone();
            pos.x += 4.0f + Cars[car].bodyLength / 2.0f;
            pos.y += 1.0f;
            Cars[car].Reset(pos, VectorF.create(-1, 0));
            return null;
        }

        public void SetupAIRace()
        {
            PauseGame();
            ClearRace();
            int[] drivers = new int[MAX_RACING_CARS];
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                drivers[i] = i;
            }
            Shuffle(drivers, MAX_RACING_CARS);
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                character[cAICar1.ID + i].Baseline = 1;
                Cars[i].SetCharacter(character[cAICar1.ID + i], 7 + drivers[i], eDirectionUp, CARVIEWPLAYER1 + i, 0, 0);
                PositionCarOnGrid(i, i);
            }
            ReadRaceConfig();
            LoadAI();
            LoadRaceCheckpoints();
            PlayersCarIndex = -1;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                AssignAIToCar(i);
                Racers[i].Activate(drivers[i]);
            }
            CameraTargetRandomAICar(true);
            IsAIRace = true;
            RaceStartSequence = 0;
            RaceEndSequence = 0;
            Timer.StopIt(tSequence);
            tSequence = null;
            UnPauseGame();
        }

        public void SetupSinglePlayerRace()
        {
            PauseGame();
            ClearRace();
            int[] drivers = new int[MAX_RACING_CARS - 1];
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS - 1; i += 1)
            {
                if (i < ThisRace.PlayerDriver)
                    drivers[i] = i;
                else 
                    drivers[i] = i + 1;
            }
            Shuffle(drivers, MAX_RACING_CARS - 1);
            cPlayerCar.Baseline = 1;
            Cars[0].SetCharacter(cPlayerCar, 7 + ThisRace.PlayerDriver, eDirectionUp, CARVIEWPLAYER1, 0, 0);
            PositionCarOnGrid(0, 0);
            for (i = 0; i < ThisRace.Opponents; i += 1)
            {
                character[cAICar1.ID + i].Baseline = 1;
                Cars[i + 1].SetCharacter(character[cAICar1.ID + i], 7 + drivers[i], eDirectionUp, CARVIEWPLAYER2 + i, 0, 0);
                PositionCarOnGrid(i + 1, i + 1);
            }
            ReadRaceConfig();
            LoadAI();
            LoadRaceCheckpoints();
            PlayersCarIndex = 0;
            Cars[0].strictCollisions = true;
            Racers[0].Activate(ThisRace.PlayerDriver);
            for (i = 0; i < ThisRace.Opponents; i += 1)
            {
                AssignAIToCar(i + 1);
                Racers[i + 1].Activate(drivers[i]);
            }
            CameraTargetPlayerCar(true);
            IsAIRace = false;
            RaceStartSequence = 0;
            RaceEndSequence = 0;
            Timer.StopIt(tSequence);
            tSequence = null;
            HoldRace = true;
            HoldAI = true;
            RaceOverlay = DynamicSprite.CreateFromExistingSprite(5);
            DrawRaceOverlay(true);
            gRaceOverlay.BackgroundGraphic = RaceOverlay.Graphic;
            gRaceOverlay.Visible = false;
            UnPauseGame();
        }

        public void RunStartSequence()
        {
            if (RaceStartSequence == 0)
            {
                gBanner.BackgroundGraphic = 15;
                gBanner.X = (system.ViewportWidth - gBanner.Width) / 2;
                gBanner.Y = -gBanner.Height;
                gBanner.Visible = true;
                gRaceOverlay.X = -gRaceOverlay.Width;
                gRaceOverlay.Visible = false;
                RaceStartSequence = 1;
            }
            else if (RaceStartSequence == 1)
            {
                if (gBanner.Y < 160)
                {
                    gBanner.Y = gBanner.Y + 12;
                }
                if (gBanner.Y > 160)
                {
                    gBanner.Y = 160;
                    tSequence = Timer.StartRT(0.8f);
                    RaceStartSequence = 2;
                }
            }
            else if (RaceStartSequence == 2)
            {
                if (Timer.HasExpired(tSequence))
                {
                    gBanner.BackgroundGraphic = 16;
                    tSequence = Timer.StartRT(0.8f);
                    RaceStartSequence = 3;
                }
            }
            else if (RaceStartSequence == 3)
            {
                if (Timer.HasExpired(tSequence))
                {
                    gBanner.BackgroundGraphic = 16;
                    tSequence = Timer.StartRT(0.8f);
                    gBanner.BackgroundGraphic = 17;
                    gRaceOverlay.Visible = true;
                    RaceStartSequence = 0;
                    HoldRace = false;
                    HoldAI = false;
                }
            }
        }

        public void RunEndSequence()
        {
            if (RaceEndSequence == 0)
            {
                gRaceOverlay.Transparency = 20;
                if (Racers[0].Finished == 1)
                {
                    gBanner.BackgroundGraphic = 21;
                }
                else 
                {
                    gBanner.BackgroundGraphic = 20;
                }
                gBanner.X = (system.ViewportWidth - gBanner.Width) / 2;
                gBanner.Y = -gBanner.Height;
                gBanner.Visible = true;
                RaceEndSequence = 1;
            }
            else if (RaceEndSequence == 1)
            {
                if (gBanner.Y < 160)
                {
                    gBanner.Y = gBanner.Y + 12;
                }
                if (gBanner.Y > 160)
                {
                    gBanner.Y = 160;
                    RaceEndSequence = 2;
                }
            }
            else if (RaceStartSequence == 2)
            {
            }
        }

        public void OnPlayerFinishedRace()
        {
            RunEndSequence();
        }

        public void TestLapComplete()
        {
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                if (!Racers[i].IsActive)
                    continue;
                if (i > 1 && !IsAIEnabledForCar(i))
                    continue;
                if (!(Checkpoints[Racers[i].CurRaceNode].order == 0 && Racers[i].CheckptsPassed > 0))
                    continue;
                int pt = 0;
                int hit_lap_test = 0;
                for (pt = 0; pt < NUM_COLLISION_POINTS; pt += 1)
                {
                    Region r = Region.GetAtRoomXY(FloatToInt(Cars[i].collPoint[pt].x, eRoundNearest), FloatToInt(Cars[i].collPoint[pt].y, eRoundNearest));
                    if (r.ID == 1)
                    {
                        hit_lap_test = 1;
                        break;
                    }
                }
                if (hit_lap_test > 0)
                {
                    Racers[i].SwitchToNextNode();
                    OnLapComplete(i);
                    if (Racers[i].Finished > 0)
                    {
                        if (i == 0)
                            OnPlayerFinishedRace();
                        else 
                            DisableAIForCar(i);
                    }
                }
            }
        }

        public override void room_Load()
        {
            StartingGrid[0] = VectorF.create(1140, 326 + 12);
            StartingGrid[1] = VectorF.create(1172, 273 + 12);
            StartingGrid[2] = VectorF.create(1204, 326 + 12);
            StartingGrid[3] = VectorF.create(1236, 273 + 12);
            StartingGrid[4] = VectorF.create(1268, 326 + 12);
            StartingGrid[5] = VectorF.create(1300, 273 + 12);
            FadeOut(0);
            StopAllAudio();
            SetupAIRace();
            aWelcome_to_the_Show.Play();
        }

        public override void room_AfterFadeIn()
        {
            FadeIn(50);
            if (IsAIRace)
                tChangeAICamera = Timer.StartRT(CHANGE_AI_CAMERA_TIME, eRepeat);
        }

        public override void room_Leave()
        {
            ClearRace();
            Timer.StopIt(tChangeAICamera);
            StopAllAudio();
        }

        public override void room_RepExec()
        {
            if (IsGamePaused())
                return;
            if (RaceStartSequence > 0)
            {
                RunStartSequence();
                return;
            }
            if (!IsAIRace)
            {
                TestLapComplete();
            }
            if (IsAIRace && Timer.HasExpired(tChangeAICamera))
            {
                CameraTargetRandomAICar(false);
            }
            if (gRaceOverlay.Visible)
            {
                if (gRaceOverlay.X < 0)
                {
                    gRaceOverlay.X = gRaceOverlay.X + 8;
                }
                else if (gRaceOverlay.X > 0)
                {
                    gRaceOverlay.X = 0;
                }
                DrawRaceOverlay(false);
            }
            if (RaceEndSequence > 0)
            {
                RunEndSequence();
                return;
            }
            if (gBanner.Visible)
            {
                gBanner.Y = gBanner.Y + 8;
                if (gBanner.Y > system.ViewportHeight)
                    gBanner.Visible = false;
            }
        }

        public override void on_key_press(eKeyCode key)
        {
            if (IsGamePaused())
                return;
            if (!gGameMenu.Visible && (IsAIRace && key != 392 || key == eKeyEscape))
            {
                if (IsAIRace)
                    DisplayGameMenu(eMenuMain, false);
                else 
                    DisplayGameMenu(eMenuMainInGame, true);
                ClaimEvent();
            }
        }

        public override void on_call(int value)
        {
            if (value == eRoom305_StartSinglePlayer)
            {
                HideGameMenu();
                FadeOut(50);
                SetupSinglePlayerRace();
                FadeIn(50);
                RunStartSequence();
            }
            else if (value == eRoom305_StartAIDemo)
            {
                HideGameMenu();
                FadeOut(50);
                SetupAIRace();
                FadeIn(50);
            }
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const float CHANGE_AI_CAMERA_TIME = 8.0f;


    }

    #endregion

}
