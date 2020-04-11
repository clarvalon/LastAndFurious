// Module_Track - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_Track;
using static LastnFurious.TrackStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_Track
    {
        // Fields
        public float Gravity;
        public float AirResistance;
        public bool[] IsObstacle = new bool[MAX_WALKABLE_AREAS];
        public float[] TerraSlideFriction = new float[MAX_WALKABLE_AREAS];
        public float[] TerraRollFriction = new float[MAX_WALKABLE_AREAS];
        public float[] EnvResistance = new float[MAX_WALKABLE_AREAS];
        public float[] TerraGrip = new float[MAX_WALKABLE_AREAS];

        // Methods
    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const int MAX_WALKABLE_AREAS = 16;


    }

    #endregion

    #region Track (AGS struct from .ash converted to class)

    public class Track
    {
        // Properties
        public static float Gravity
        {
            get
            {
                return ParentInstance.Gravity;
            }
            set
            {
                ParentInstance.Gravity = value;
            }
        }

        public static float AirResistance
        {
            get
            {
                return ParentInstance.AirResistance;
            }
            set
            {
                ParentInstance.AirResistance = value;
            }
        }

        public static bool[] IsObstacle
        {
            get
            {
                return ParentInstance.IsObstacle;
            }
            set
            {
                ParentInstance.IsObstacle = value;
            }
        }

        public static float[] TerraSlideFriction
        {
            get
            {
                return ParentInstance.TerraSlideFriction;
            }
            set
            {
                ParentInstance.TerraSlideFriction = value;
            }
        }

        public static float[] TerraRollFriction
        {
            get
            {
                return ParentInstance.TerraRollFriction;
            }
            set
            {
                ParentInstance.TerraRollFriction = value;
            }
        }

        public static float[] TerraGrip
        {
            get
            {
                return ParentInstance.TerraGrip;
            }
            set
            {
                TerraGrip = value;// Maths.ClampF(value, 0.0f, 1.0f);
            }
        }

        public static float[] EnvResistance
        {
            get
            {
                return ParentInstance.EnvResistance;
            }
            set
            {
                ParentInstance.EnvResistance = value;
            }
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class TrackStaticRef
    {
        // Static Fields
        public static float Gravity { get { return GlobalBase.TrackInstance.Gravity; } set { GlobalBase.TrackInstance.Gravity = value; } }
        public static float AirResistance { get { return GlobalBase.TrackInstance.AirResistance; } set { GlobalBase.TrackInstance.AirResistance = value; } }
        public static bool[] IsObstacle { get { return GlobalBase.TrackInstance.IsObstacle; } set { GlobalBase.TrackInstance.IsObstacle = value; } }
        public static float[] TerraSlideFriction { get { return GlobalBase.TrackInstance.TerraSlideFriction; } set { GlobalBase.TrackInstance.TerraSlideFriction = value; } }
        public static float[] TerraRollFriction { get { return GlobalBase.TrackInstance.TerraRollFriction; } set { GlobalBase.TrackInstance.TerraRollFriction = value; } }
        public static float[] EnvResistance { get { return GlobalBase.TrackInstance.EnvResistance; } set { GlobalBase.TrackInstance.EnvResistance = value; } }
        public static float[] TerraGrip { get { return GlobalBase.TrackInstance.TerraGrip; } set { GlobalBase.TrackInstance.TerraGrip = value; } }

    }

    #endregion
    
}
