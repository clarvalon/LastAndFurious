// Module_RaceAI - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_RaceAI;
using static LastnFurious.RaceAIStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_RaceAI
    {
        // Fields
        public AIType ActiveAIType = default(AIType);
        public bool HoldAI;
        public RaceAI[] RobotsPB = CreateAndInstantiateArray<RaceAI>(MAX_RACING_CARS);
        public RaceAIRegionBased[] RobotsRB = CreateAndInstantiateArray<RaceAIRegionBased>(MAX_RACING_CARS);
        public PathNode[] Paths = CreateAndInstantiateArray<PathNode>(MAX_PATH_NODES);
        public int FirstPathNode;
        public int LastPathNode;
        public int PathNodeCount;
        public int FreePathSlot;
        public ColorToAngle[] RegionAngles = CreateAndInstantiateArray<ColorToAngle>(16);
        public DynamicSprite AIRegions;
        public DrawingSurface AIRegionsDS;

        // Methods
        public void game_start()
        {
            ActiveAIType = eAINone;
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                RobotsPB[i].Reset();
            }
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                RobotsRB[i].Reset();
            }
            for (i = 0; i < MAX_PATH_NODES; i += 1)
            {
                Paths[i].Reset();
            }
            PathNodeCount = 0;
            FirstPathNode = -1;
            LastPathNode = -1;
            FreePathSlot = 0;
        }

        public void repeatedly_execute_always()
        {
            if (ActiveAIType == eAINone)
                return;
            if (IsGamePaused() || HoldAI)
                return;
            float delta_time = 1.0f / IntToFloat(GetGameSpeed());
            int i = 0;
            if (ActiveAIType == eAIPaths)
            {
                for (i = 0; i < MAX_RACING_CARS; i += 1)
                {
                    if (RobotsPB[i].vehicleIndex >= 0)
                        RobotsPB[i].Run(delta_time);
                }
            }
            else if (ActiveAIType == eAIRegions)
            {
                for (i = 0; i < MAX_RACING_CARS; i += 1)
                {
                    if (RobotsRB[i].vehicleIndex >= 0)
                        RobotsRB[i].Run(delta_time);
                }
            }
        }

        public void ResetAI()
        {
            int i = 0;
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                RobotsPB[i].Reset();
            }
            for (i = 0; i < MAX_RACING_CARS; i += 1)
            {
                RobotsRB[i].Reset();
            }
            for (i = 0; i < MAX_PATH_NODES; i += 1)
            {
                Paths[i].Reset();
            }
            if (AIRegionsDS)
            {
                AIRegionsDS.Release();
                AIRegionsDS = null;
            }
            if (AIRegions)
            {
                AIRegions.Delete();
                AIRegions = null;
            }
            ActiveAIType = eAINone;
        }

        public void on_event(EventType eventVar, int data)
        {
            if (eventVar == eEventLeaveRoom)
            {
                ResetAI();
            }
        }

        public int FindFirstFreeNode()
        {
            int free = -1;
            int i = 0;
            for (i = FreePathSlot; i < MAX_PATH_NODES; i += 1)
            {
                if (Paths[i].pt == null)
                {
                    free = i;
                    break;
                }
            }
            if (free >= 0)
            {
                FreePathSlot = free;
                return free;
            }
            for (i = 0; i < FreePathSlot; i += 1)
            {
                if (Paths[i].pt == null)
                {
                    free = i;
                    break;
                }
            }
            FreePathSlot = free;
            return free;
        }

        public void LoadAIPaths()
        {
            File f = File.Open("$APPDATADIR$/Data/aipaths.dat", eFileRead);
            if (f == null)
            {
                f = File.Open("$INSTALLDIR$/Data/aipaths.dat", eFileRead);
                if (f == null)
                    return;
            }
            int n = 0;
            for (n = 0; n < MAX_PATH_NODES; n += 1)
            {
                Paths[n].Reset();
            }
            PathNodeCount = 0;
            FirstPathNode = f.ReadInt();
            LastPathNode = f.ReadInt();
            n = FirstPathNode;
            int loads = 0;
            do 
            {
                loads += 1;
                if (loads % 50 == 0)
                    Wait(1);
                int x = f.ReadInt();
                int y = f.ReadInt();
                Paths[n].pt = VectorF.create(x, y);
                Paths[n].radius = IntToFloat(f.ReadInt());
                Paths[n].threshold = IntToFloat(f.ReadInt());
                Paths[n].speed = IntToFloat(f.ReadInt());
                Paths[n].prev = f.ReadInt();
                Paths[n].next = f.ReadInt();
                PathNodeCount += 1;
                n = Paths[n].next;
            }
            while (n != FirstPathNode);
                
            FindFirstFreeNode();
        }

        public void SaveAIPaths()
        {
            File f = File.Open("$APPDATADIR$/Data/aipaths.dat", eFileWrite);
            if (f == null)
            {
                Display("Failed to open 'Data/aipaths.dat' for writing!");
                return;
            }
            f.WriteInt(FirstPathNode);
            f.WriteInt(LastPathNode);
            int i = FirstPathNode;
            do 
            {
                f.WriteInt(FloatToInt(Paths[i].pt.x, eRoundNearest));
                f.WriteInt(FloatToInt(Paths[i].pt.y, eRoundNearest));
                f.WriteInt(FloatToInt(Paths[i].radius, eRoundNearest));
                f.WriteInt(FloatToInt(Paths[i].threshold, eRoundNearest));
                f.WriteInt(FloatToInt(Paths[i].speed, eRoundNearest));
                f.WriteInt(Paths[i].prev);
                f.WriteInt(Paths[i].next);
                i = Paths[i].next;
            }
            while (i != FirstPathNode);
                
            f.Close();
        }

        public void LoadAIRegions()
        {
            AIRegions = DynamicSprite.CreateFromFile("$INSTALLDIR$/Data/airegions.png");
            if (AIRegions == null)
                return;
            IniFile ini = new IniFile();
            if (!ini.Load("$INSTALLDIR$/Data/airegions.ini"))
                return;
            String rgb_name_r = "color_r";
            String rgb_name_g = "color_g";
            String rgb_name_b = "color_b";
            String angle_name = "angle";
            int i = 0;
            for (i = 0; i < 16; i += 1)
            {
                String section = StringFormatAGS("region%d", i);
                if (!ini.KeyExists(section, rgb_name_r))
                    continue;
                RegionAngles[i].rgb[0] = ini.ReadInt(section, rgb_name_r);
                RegionAngles[i].rgb[1] = ini.ReadInt(section, rgb_name_g);
                RegionAngles[i].rgb[2] = ini.ReadInt(section, rgb_name_b);
                RegionAngles[i].color = Game.GetColorFromRGB(RegionAngles[i].rgb[0], RegionAngles[i].rgb[1], RegionAngles[i].rgb[2]);
                RegionAngles[i].angle = Maths.DegreesToRadians(ini.ReadFloat(section, angle_name));
            }

            // Optimisation - store drawing surface in local memory as only used for getting data 
            // This is much faster than pulling the data from the GPU, though takes up a lot of memory
            AIRegionsDS = AIRegions.GetDrawingSurface(true); 
        }

        public void LoadAI()
        {
            ResetAI();
            ActiveAIType = eAINone;
            if (ThisRace.AiAndPhysics == ePhysicsWild)
            {
                ActiveAIType = eAIPaths;
                LoadAIPaths();
                return;
            }
            else if (ThisRace.AiAndPhysics == ePhysicsSafe)
            {
                ActiveAIType = eAIRegions;
                LoadAIRegions();
                return;
            }
        }

        public void AssignAIToCar(int car_index)
        {
            switch(ActiveAIType) 
            {
                case eAIPaths:RobotsPB[car_index].vehicleIndex = car_index;
                RobotsRB[car_index].Reset();
                break;
                case eAIRegions:RobotsPB[car_index].Reset();
                RobotsRB[car_index].vehicleIndex = car_index;
                break;
            }
            Cars[car_index].strictCollisions = false;
        }

        public bool IsAIEnabledForCar(int car_index)
        {
            switch(ActiveAIType) 
            {
                case eAIPaths:return RobotsPB[car_index].vehicleIndex >= 0;
                case eAIRegions:return RobotsRB[car_index].vehicleIndex >= 0;
            }
            return false;
        }

        public void DisableAIForCar(int car_index)
        {
            RobotsPB[car_index].Reset();
            RobotsRB[car_index].Reset();
            Cars[car_index].Accelerator = 0.0f;
            Cars[car_index].Brakes = 0.0f;
            Cars[car_index].steeringWheelAngle = 0.0f;
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int MAX_PATH_NODES = 200;
        public const float DEFAULT_PATH_CHECK_RADIUS = 50.0f;

        // Expose Enums and instances of each
        public enum AIType
        {
            eAINone = 0, 
            eAIPaths = 1, 
            eAIRegions = 2
        }
        public const AIType eAINone = AIType.eAINone;
        public const AIType eAIPaths = AIType.eAIPaths;
        public const AIType eAIRegions = AIType.eAIRegions;

        // Expose RaceAI methods so they can be used without instance prefix
        public static int FindFirstFreeNode()
        {
            return RaceAIInstance.FindFirstFreeNode();
        }

        public static void LoadAIPaths()
        {
            RaceAIInstance.LoadAIPaths();
        }

        public static void SaveAIPaths()
        {
            RaceAIInstance.SaveAIPaths();
        }

        public static void LoadAIRegions()
        {
            RaceAIInstance.LoadAIRegions();
        }

        public static void LoadAI()
        {
            RaceAIInstance.LoadAI();
        }

        public static void AssignAIToCar(int car_index)
        {
            RaceAIInstance.AssignAIToCar(car_index);
        }

        public static bool IsAIEnabledForCar(int car_index)
        {
            return RaceAIInstance.IsAIEnabledForCar(car_index);
        }

        public static void DisableAIForCar(int car_index)
        {
            RaceAIInstance.DisableAIForCar(car_index);
        }

        public static void ResetAI()
        {
            RaceAIInstance.ResetAI();
        }

        // Expose RaceAI variables so they can be used without instance prefix
        public static AIType ActiveAIType { get { return RaceAIInstance.ActiveAIType; } set { RaceAIInstance.ActiveAIType = value; } }
        public static bool HoldAI { get { return RaceAIInstance.HoldAI; } set { RaceAIInstance.HoldAI = value; } }
        public static RaceAI[] RobotsPB { get { return RaceAIInstance.RobotsPB; } set { RaceAIInstance.RobotsPB = value; } }
        public static RaceAIRegionBased[] RobotsRB { get { return RaceAIInstance.RobotsRB; } set { RaceAIInstance.RobotsRB = value; } }
        public static PathNode[] Paths { get { return RaceAIInstance.Paths; } set { RaceAIInstance.Paths = value; } }
        public static int FirstPathNode { get { return RaceAIInstance.FirstPathNode; } set { RaceAIInstance.FirstPathNode = value; } }
        public static int LastPathNode { get { return RaceAIInstance.LastPathNode; } set { RaceAIInstance.LastPathNode = value; } }
        public static int PathNodeCount { get { return RaceAIInstance.PathNodeCount; } set { RaceAIInstance.PathNodeCount = value; } }
        public static int FreePathSlot { get { return RaceAIInstance.FreePathSlot; } set { RaceAIInstance.FreePathSlot = value; } }
        public static ColorToAngle[] RegionAngles { get { return RaceAIInstance.RegionAngles; } set { RaceAIInstance.RegionAngles = value; } }
        public static DynamicSprite AIRegions { get { return RaceAIInstance.AIRegions; } set { RaceAIInstance.AIRegions = value; } }
        public static DrawingSurface AIRegionsDS { get { return RaceAIInstance.AIRegionsDS; } set { RaceAIInstance.AIRegionsDS = value; } }

    }

    #endregion

    #region PathNode (AGS struct from .ash converted to class)

    public class PathNode
    {
        // Fields
        public VectorF pt;
        public float radius;
        public float threshold;
        public float speed;
        public int prev;
        public int next;

        // Methods
        public void Reset()
        {
            this.pt = null;
            this.radius = 1.0f;
            this.threshold = 1.0f;
            this.speed = -1.0f;
            this.next = -1;
            this.prev = -1;
        }

    }

    #endregion

    #region RaceAI (AGS struct from .ash converted to class)

    public class RaceAI
    {
        // Fields
        public int vehicleIndex;
        public VectorF targetPos;
        public VectorF targetDir;
        public float targetCheckRadius;
        public float targetThreshold;
        public float targetSpeedHint;
        public int currentNode;

        // Methods
        public bool TestShouldChooseNewTarget()
        {
            if (this.targetPos == null)
                return true;
            int curNode = this.currentNode;
            if (curNode >= 0)
            {
                int prevNode = Paths[curNode].prev;
                int nextNode = Paths[curNode].next;
                if (nextNode >= 0 && (prevNode < 0 || VectorF.distance(Cars[this.vehicleIndex].position, Paths[prevNode].pt) > VectorF.distance(Paths[this.currentNode].pt, Paths[prevNode].pt)) && VectorF.distance(Cars[this.vehicleIndex].position, Paths[nextNode].pt) < VectorF.distance(Paths[this.currentNode].pt, Paths[nextNode].pt))
                    return true;
            }
            return VectorF.distance(Cars[this.vehicleIndex].position, this.targetPos) <= this.targetCheckRadius;
        }

        public bool ChooseNewTarget()
        {
            if (PathNodeCount > 0)
            {
                if (this.currentNode >= 0 && Paths[this.currentNode].pt != null)
                {
                    this.currentNode = Paths[this.currentNode].next;
                }
                else 
                {
                    this.currentNode = FirstPathNode;
                }
                this.targetPos = Paths[this.currentNode].pt.clone();
                this.targetCheckRadius = Paths[this.currentNode].radius;
                this.targetThreshold = Paths[this.currentNode].threshold;
                this.targetSpeedHint = Paths[this.currentNode].speed;
            }
            else 
            {
            }
            return true;
        }

        public void DriveToTheTarget()
        {
            if (this.targetPos == null)
                return;
            this.targetDir = VectorF.subtract(this.targetPos, Cars[this.vehicleIndex].position);
            float angleThreshold = 0f;
            if (!this.targetDir.isZero())
                angleThreshold = Maths.ArcTan(this.targetThreshold / this.targetDir.length());
            float angleBetween = VectorF.angleBetween(Cars[this.vehicleIndex].direction, this.targetDir);
            if (angleBetween >= -angleThreshold && angleBetween <= angleThreshold)
            {
                Cars[this.vehicleIndex].steeringWheelAngle = 0.0f;
            }
            else 
            {
                if (angleBetween > 0.0f)
                    Cars[this.vehicleIndex].steeringWheelAngle = UISteeringAngle;
                else 
                    Cars[this.vehicleIndex].steeringWheelAngle = -UISteeringAngle;
            }
            Cars[this.vehicleIndex].Brakes = 0.0f;
            if (this.targetSpeedHint < 0.0f)
            {
                Cars[this.vehicleIndex].Accelerator = 1.0f;
            }
            else 
            {
                float speed = Cars[this.vehicleIndex].velocity.length();
                if (speed < this.targetSpeedHint)
                {
                    Cars[this.vehicleIndex].Accelerator = 1.0f;
                }
                else if (speed > this.targetSpeedHint)
                {
                    Cars[this.vehicleIndex].Accelerator = 0.0f;
                    Cars[this.vehicleIndex].Brakes = 1.0f;
                }
            }
        }

        public void Reset()
        {
            this.vehicleIndex = -1;
            this.currentNode = -1;
            this.targetPos = null;
            this.targetDir = null;
        }

        public void Run(float deltaTime)
        {
            if (this.TestShouldChooseNewTarget())
            {
                if (!this.ChooseNewTarget())
                    return;
            }
            this.DriveToTheTarget();
        }

    }

    #endregion

    #region RaceAIRegionBased (AGS struct from .ash converted to class)

    public class RaceAIRegionBased
    {
        // Fields
        public int vehicleIndex;
        public float targetAngle;

        // Methods
        public void Run(float deltaTime)
        {
            if (this.vehicleIndex < 0)
                return;
            if (AIRegionsDS == null)
            {
                if (AIRegions == null)
                    return;
                AIRegionsDS = AIRegions.GetDrawingSurface();
            }
            float angleSum = 0f;
            int agscolor = AIRegionsDS.GetPixel(FloatToInt(Cars[this.vehicleIndex].position.x, eRoundNearest), FloatToInt(Cars[this.vehicleIndex].position.y, eRoundNearest));
            int i = 0;
            for (i = 0; i < 16; i += 1)
            {
                if (RegionAngles[i].color == agscolor)
                {
                    angleSum = RegionAngles[i].angle;
                    break;
                }
            }
            this.targetAngle = angleSum;
            float dirAngle = Cars[this.vehicleIndex].direction.angle();
            float angleBetween = Maths.AnglePiFast(this.targetAngle - Maths.Angle2Pi(dirAngle));
            float steeringDT = UISteeringAngle * deltaTime * 1.1f;
            if (Maths.AbsF(angleBetween) <= Maths.AbsF(steeringDT))
            {
                Cars[this.vehicleIndex].steeringWheelAngle = 0.0f;
                Cars[this.vehicleIndex].direction.set2(1.0f, 0.0f);
                Cars[this.vehicleIndex].direction.rotate(this.targetAngle);
            }
            else if (angleBetween > 0.0f)
                Cars[this.vehicleIndex].steeringWheelAngle = UISteeringAngle;
            else if (angleBetween < 0.0f)
                Cars[this.vehicleIndex].steeringWheelAngle = -UISteeringAngle;
            else 
                Cars[this.vehicleIndex].steeringWheelAngle = 0.0f;
            Cars[this.vehicleIndex].Accelerator = 1.0f;
            Cars[this.vehicleIndex].Brakes = 0.0f;
        }

        public void Reset()
        {
            this.vehicleIndex = -1;
            this.targetAngle = 0.0f;
        }

    }

    #endregion

    #region ColorToAngle (AGS struct from .ash converted to class)

    public class ColorToAngle
    {
        // Fields
        public int[] rgb = new int[3];
        public int color;
        public float angle;

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class RaceAIStaticRef
    {
        // Static Methods
        public static void ResetAI()
        {
            GlobalBase.RaceAIInstance.ResetAI();
        }

        public static int FindFirstFreeNode()
        {
            return GlobalBase.RaceAIInstance.FindFirstFreeNode();
        }

        public static void LoadAIPaths()
        {
            GlobalBase.RaceAIInstance.LoadAIPaths();
        }

        public static void SaveAIPaths()
        {
            GlobalBase.RaceAIInstance.SaveAIPaths();
        }

        public static void LoadAIRegions()
        {
            GlobalBase.RaceAIInstance.LoadAIRegions();
        }

        public static void LoadAI()
        {
            GlobalBase.RaceAIInstance.LoadAI();
        }

    }

    #endregion
    
}
