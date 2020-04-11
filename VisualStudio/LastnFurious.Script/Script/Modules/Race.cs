// Module_Race - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_Race;
using static LastnFurious.RaceStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_Race
    {
        // Fields
        public Race ThisRace = new Race();
        public bool HoldRace;
        public Racer[] Racers = CreateAndInstantiateArray<Racer>(MAX_RACING_CARS);
        public Vehicle[] Cars = CreateAndInstantiateArray<Vehicle>(MAX_RACING_CARS);
        public RaceNode[] Checkpoints = CreateAndInstantiateArray<RaceNode>(MAX_CHECKPOINTS);
        public int FirstCheckpt;
        public int LastCheckpt;
        public int CheckptCount;
        public int FreeCheckptSlot;

        // Methods
        public void OnFinishedRace(int racer)
        {
            ThisRace.RacersFinished += 1;
            Racers[racer].Finished = ThisRace.RacersFinished;
        }

        public void OnLapComplete(int racer)
        {
            if (Racers[racer].Lap == ThisRace.Laps)
            {
                OnFinishedRace(racer);
            }
            else 
            {
                Racers[racer].Lap += 1;
            }
        }

        public void UpdateRacer(int index, float deltaTime)
        {
            if (Racers[index].Finished > 0)
                return;
            Racers[index].Time += deltaTime;
            if (CheckptCount > 0)
            {
                if (Racers[index].CurRaceNode < 0)
                {
                    Racers[index].CurRaceNode = FirstCheckpt;
                    Racers[index].CheckptsPassed = 0;
                }
                else 
                {
                    int curNode = Racers[index].CurRaceNode;
                    int nextNode = Checkpoints[curNode].next;
                    if (nextNode >= 0 && VectorF.distance(Cars[index].position, Checkpoints[nextNode].pt) < VectorF.distance(Checkpoints[curNode].pt, Checkpoints[nextNode].pt))
                    {
                        Racers[index].SwitchToNextNode();
                        if (Checkpoints[curNode].order == 0 && Racers[index].CheckptsPassed > 1)
                        {
                        }
                    }
                }
            }
        }

        public int FindFirstFreeCheckpoint()
        {
            int free = -1;
            int i = 0;
            for (i = FreeCheckptSlot; i < MAX_CHECKPOINTS; i += 1)
            {
                if (Checkpoints[i].pt == null)
                {
                    free = i;
                    break;
                }
            }
            if (free >= 0)
            {
                FreeCheckptSlot = free;
                return free;
            }
            for (i = 0; i < FreeCheckptSlot; i += 1)
            {
                if (Checkpoints[i].pt == null)
                {
                    free = i;
                    break;
                }
            }
            FreeCheckptSlot = free;
            return free;
        }

        public void LoadRaceCheckpoints()
        {
            File f = File.Open("$APPDATADIR$/Data/checkpoints.dat", eFileRead);
            if (f == null)
            {
                f = File.Open("$INSTALLDIR$/Data/checkpoints.dat", eFileRead);
                if (f == null)
                    return;
            }
            int n = 0;
            for (n = 0; n < MAX_CHECKPOINTS; n += 1)
            {
                Checkpoints[n].Reset();
            }
            CheckptCount = 0;
            FirstCheckpt = f.ReadInt();
            LastCheckpt = f.ReadInt();
            n = FirstCheckpt;
            int loads = 0;
            do 
            {
                loads += 1;
                if (loads % 50 == 0)
                    Wait(1);
                int x = f.ReadInt();
                int y = f.ReadInt();
                Checkpoints[n].pt = VectorF.create(x, y);
                Checkpoints[n].order = CheckptCount;
                Checkpoints[n].prev = f.ReadInt();
                Checkpoints[n].next = f.ReadInt();
                CheckptCount += 1;
                n = Checkpoints[n].next;
            }
            while (n != FirstCheckpt);
                
            f.Close();
            FindFirstFreeCheckpoint();
        }

        public void SaveRaceCheckpoints()
        {
            File f = File.Open("$APPDATADIR$/Data/checkpoints.dat", eFileWrite);
            if (f == null)
            {
                Display("Failed to open 'Data/checkpoints.dat' for writing!");
                return;
            }
            f.WriteInt(FirstCheckpt);
            f.WriteInt(LastCheckpt);
            int i = FirstCheckpt;
            do 
            {
                f.WriteInt(FloatToInt(Checkpoints[i].pt.x, eRoundNearest));
                f.WriteInt(FloatToInt(Checkpoints[i].pt.y, eRoundNearest));
                f.WriteInt(Checkpoints[i].prev);
                f.WriteInt(Checkpoints[i].next);
                i = Checkpoints[i].next;
            }
            while (i != FirstCheckpt);
                
            f.Close();
        }

        public void ResetRace()
        {
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                Racers[i].Reset();
                ThisRace.DriverPositions[i] = -1;
            }
            for (i = 0; i < MAX_CHECKPOINTS; i += 1)
            {
                Checkpoints[i].Reset();
            }
            ThisRace.Laps = 0;
            ThisRace.Opponents = 0;
            ThisRace.PlayerDriver = -1;
        }

        public bool RacerIsBehind(int racer1, int racer2)
        {
            if (!Racers[racer1].IsActive)
                return true;
            if (!Racers[racer2].IsActive)
                return false;
            if (Racers[racer2].Finished > 0 && Racers[racer1].Finished == 0)
                return true;
            if (Racers[racer1].Finished > 0 && Racers[racer2].Finished == 0)
                return false;
            if (Racers[racer1].Finished > 0 && Racers[racer2].Finished > 0)
                return Racers[racer1].Finished > Racers[racer2].Finished;
            if (Racers[racer1].CheckptsPassed < Racers[racer2].CheckptsPassed)
                return true;
            if (Racers[racer1].CheckptsPassed > Racers[racer2].CheckptsPassed)
                return false;
            return VectorF.distance(Cars[racer1].position, Checkpoints[Racers[racer1].CurRaceNode].pt) >VectorF.distance(Cars[racer2].position, Checkpoints[Racers[racer2].CurRaceNode].pt);
        }

        // Keep single instance
        bool[] impactPairs = new bool[MAX_RACING_CARS_SQUARED];
        VectorF[] rect = new VectorF[4];

        public void RunVeh2VehCollision()
        {
            // Reset
            for (int i2 = 0; i2 < impactPairs.Length; i2 += 1)
                impactPairs[i2] = false;
            for (int i2 = 0; i2 < rect.Length; i2 += 1)
                rect[i2] = null;

            // Optimisation (previously generated a lot of garbage)
            //bool[] impactPairs = new bool[MAX_RACING_CARS_SQUARED];
            //VectorF[] rect = new VectorF[4];

            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                if (!Cars[i].IsInit)
                    continue;
                int j = 0;
                for (j = 0; j < MAX_RACING_CARS; j += 1)
                {
                    if (j == i)
                        continue;
                    if (!Cars[j].IsInit)
                        continue;
                    if (i > j && impactPairs[i * MAX_RACING_CARS + j])
                        continue;
                    rect[0] = Cars[j].collPoint[0];
                    rect[1] = Cars[j].collPoint[1];
                    rect[2] = Cars[j].collPoint[2];
                    rect[3] = Cars[j].collPoint[3];
                    VectorF impact = Cars[i].DetectCollision(rect, Cars[j].velocity, j);
                    if (impact != null)
                    {
                        impactPairs[i * MAX_RACING_CARS + j] = true;
                        Cars[i].velocity.add(impact);
                        impact.negate();
                        Cars[j].velocity.add(impact);
                    }
                }
            }
        }

        public void repeatedly_execute_always()
        {
            if (IsGamePaused() || HoldRace)
                return;
            float delta_time = 1.0f / IntToFloat(GetGameSpeed());
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                if (Cars[i].IsInit)
                    Cars[i].Run(delta_time);
                if (ThisRace.CarCollisions)
                    RunVeh2VehCollision();
                if (Racers[i].IsActive)
                    UpdateRacer(i, delta_time);
            }
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                ThisRace.DriverPositions[i] = i;
                Racers[i].Place = i;
            }
            if (CheckptCount > 0)
            {
                i = 1;
                while (i < MAX_RACING_CARS)
                {
                    int j = i;
                    while (j > 0   && RacerIsBehind(ThisRace.DriverPositions[j - 1], ThisRace.DriverPositions[j]))
                    {
                        int temp = ThisRace.DriverPositions[j];
                        ThisRace.DriverPositions[j] = ThisRace.DriverPositions[j - 1];
                        ThisRace.DriverPositions[j - 1] = temp;
                        j -= 1;
                    }
                    i += 1;
                }
                for (i = 0; i < MAX_RACING_CARS; i += 1)
                {
                    int racer = ThisRace.DriverPositions[i];
                    Racers[racer].Place = i;
                }
            }
        }

        public void on_event(EventType eventVar, int data)
        {
            if (eventVar == eEventLeaveRoom)
            {
                int i = 0;
                for (i = 0; i < MAX_RACING_CARS; i += 1)
                {
                    Cars[i].UnInit();
                }
            }
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int MAX_RACING_CARS = 6;
        public const int MAX_RACING_CARS_SQUARED = 36;
        public const int MAX_CHECKPOINTS = 200;

        // Expose Enums and instances of each
        public enum AiAndPhysicsOption
        {
            ePhysicsSafe = 0, 
            ePhysicsWild = 1
        }
        public const AiAndPhysicsOption ePhysicsSafe = AiAndPhysicsOption.ePhysicsSafe;
        public const AiAndPhysicsOption ePhysicsWild = AiAndPhysicsOption.ePhysicsWild;

        // Expose Race methods so they can be used without instance prefix
        public static int FindFirstFreeCheckpoint()
        {
            return RaceInstance.FindFirstFreeCheckpoint();
        }

        public static void LoadRaceCheckpoints()
        {
            RaceInstance.LoadRaceCheckpoints();
        }

        public static void SaveRaceCheckpoints()
        {
            RaceInstance.SaveRaceCheckpoints();
        }

        public static void OnLapComplete(int racer)
        {
            RaceInstance.OnLapComplete(racer);
        }

        // Expose Race variables so they can be used without instance prefix
        public static Race ThisRace { get { return RaceInstance.ThisRace; } set { RaceInstance.ThisRace = value; } }
        public static bool HoldRace { get { return RaceInstance.HoldRace; } set { RaceInstance.HoldRace = value; } }
        public static Racer[] Racers { get { return RaceInstance.Racers; } set { RaceInstance.Racers = value; } }
        public static Vehicle[] Cars { get { return RaceInstance.Cars; } set { RaceInstance.Cars = value; } }
        public static RaceNode[] Checkpoints { get { return RaceInstance.Checkpoints; } set { RaceInstance.Checkpoints = value; } }
        public static int FirstCheckpt { get { return RaceInstance.FirstCheckpt; } set { RaceInstance.FirstCheckpt = value; } }
        public static int LastCheckpt { get { return RaceInstance.LastCheckpt; } set { RaceInstance.LastCheckpt = value; } }
        public static int CheckptCount { get { return RaceInstance.CheckptCount; } set { RaceInstance.CheckptCount = value; } }
        public static int FreeCheckptSlot { get { return RaceInstance.FreeCheckptSlot; } set { RaceInstance.FreeCheckptSlot = value; } }

    }

    #endregion

    #region Race (AGS struct from .ash converted to class)

    public class Race
    {
        // Fields
        public int Laps;
        public int Opponents;
        public int PlayerDriver;
        public AiAndPhysicsOption AiAndPhysics = default(AiAndPhysicsOption);
        public bool CarCollisions;
        public int[] DriverPositions = new int[MAX_RACING_CARS];
        public int RacersFinished;

    }

    #endregion

    #region RaceNode (AGS struct from .ash converted to class)

    public class RaceNode
    {
        // Fields
        public VectorF pt;
        public int order;
        public int prev;
        public int next;

        // Methods
        public void Reset()
        {
            this.pt = null;
            this.order = 0;
            this.next = -1;
            this.prev = -1;
        }

    }

    #endregion

    #region Racer (AGS struct from .ash converted to class)

    public class Racer
    {
        // Fields
        public bool IsActive;
        public int Driver;
        public int Lap;
        public int Place;
        public float Time;
        public int Finished;
        public int CurRaceNode;
        public int CheckptsPassed;

        // Methods
        public void Activate(int driver)
        {
            this.IsActive = true;
            this.Driver = driver;
            this.Lap = 1;
            this.Place = 0;
            this.Time = 0.0f;
            this.CurRaceNode = FirstCheckpt;
            this.CheckptsPassed = 0;
        }

        public void Reset()
        {
            this.IsActive = false;
            this.Driver = -1;
            this.Lap = 0;
            this.Place = 0;
            this.Time = 0.0f;
            this.Finished = 0;
            this.CurRaceNode = -1;
            this.CheckptsPassed = 0;
        }

        public void SwitchToNextNode()
        {
            this.CheckptsPassed += 1;
            this.CurRaceNode = Checkpoints[this.CurRaceNode].next;
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class RaceStaticRef
    {
        // Static Methods
        public static void OnFinishedRace(int racer)
        {
            GlobalBase.RaceInstance.OnFinishedRace(racer);
        }

        public static void UpdateRacer(int index, float deltaTime)
        {
            GlobalBase.RaceInstance.UpdateRacer(index, deltaTime);
        }

        public static int FindFirstFreeCheckpoint()
        {
            return GlobalBase.RaceInstance.FindFirstFreeCheckpoint();
        }

        public static void LoadRaceCheckpoints()
        {
            GlobalBase.RaceInstance.LoadRaceCheckpoints();
        }

        public static void SaveRaceCheckpoints()
        {
            GlobalBase.RaceInstance.SaveRaceCheckpoints();
        }

        public static void ResetRace()
        {
            GlobalBase.RaceInstance.ResetRace();
        }

        public static bool RacerIsBehind(int racer1, int racer2)
        {
            return GlobalBase.RaceInstance.RacerIsBehind(racer1, racer2);
        }

        public static void RunVeh2VehCollision()
        {
            GlobalBase.RaceInstance.RunVeh2VehCollision();
        }

    }

    #endregion
    
}
