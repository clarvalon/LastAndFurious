// Module_MathUtil - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_MathUtil;
using static LastnFurious.MathUtilStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_MathUtil
    {
        // Fields

        // Methods
        public Point ParsePoint(String s)
        {
            int sep_at = s.IndexOf(",");
            if (sep_at < 0)
                return null;
            Point p = new Point();
            String s1 = s.Substring(0, sep_at);
            String s2 = s.Substring(sep_at + 1, s.Length - sep_at + 1);
            p.x = s1.AsInt();
            p.y = s2.AsInt();
            return p;
        }

    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {
        // Expose AGS singular #defines as C# constants (or static getters)
        public const float TINY_FLOAT = 0.00000001f;


    }

    #endregion

    #region Point (AGS managed struct from .ash converted to class)

    public class Point
    {
        // Fields
        public int x;
        public int y;

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class MathUtilStaticRef
    {
        // Static Methods
        public static Point ParsePoint(String s)
        {
            return GlobalBase.MathUtil.ParsePoint(s);
        }

    }

    #endregion
    
}

#region Maths (extending existing internal class)

public static partial class Maths
{
    public static int Abs(int value)
    {
        if (value >= 0)
            return value;
        return -value;
    }

    public static int Max(int a, int b)
    {
        if (a >= b)
            return a;
        return b;
    }

    public static int Min(int a, int b)
    {
        if (a <= b)
            return a;
        return b;
    }

    public static int Clamp(int value, int min, int max)
    {
        return Maths.Min(max, Maths.Max(min, value));
    }

    public static float AbsF(float value)
    {
        if (value >= 0.0f)
            return value;
        return -value;
    }

    public static float MaxF(float a, float b)
    {
        if (a >= b)
            return a;
        return b;
    }

    public static float MinF(float a, float b)
    {
        if (a <= b)
            return a;
        return b;
    }

    public static float ClampF(float value, float min, float max)
    {
        return Maths.MinF(max, Maths.MaxF(min, value));
    }

    public static int Angle360(int degrees)
    {
        int angle = degrees % 360;
        if (angle >= 0)
            return angle;
        return 360 - (-angle);
    }

    public static float AnglePiFast(float rads)
    {
        if (rads > Maths.Pi)
            return rads - Maths.Pi * 2.0f;
        else if (rads < -Maths.Pi)
            return rads + Maths.Pi * 2.0f;
        return rads;
    }

    public static float Angle2Pi(float rads)
    {
        float pi2 = Maths.Pi * 2.0f;
        float angle = 0f;
        if (rads >= 0.0f)
            angle = rads - pi2 * IntToFloat(FloatToInt(rads / pi2, eRoundDown));
        else 
            angle = rads - pi2 * IntToFloat(FloatToInt(rads / pi2, eRoundUp));
        if (angle >= 0.0f)
            return angle;
        return pi2 - (-angle);
    }

}

#endregion

