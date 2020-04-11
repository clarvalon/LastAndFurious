// Module_Vectors - Type "override" followed by space to see list of C# methods to implement
using static LastnFurious.GlobalBase;
using System.Diagnostics;
using static LastnFurious.Module_Vectors;
using static LastnFurious.VectorsStaticRef;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Clarvalon.XAGE.Global;

namespace LastnFurious
{
    public partial class Module_Vectors
    {
        // Fields

        // Methods
    }

    #region Globally Exposed Items

    public partial class GlobalBase
    {

    }

    #endregion

    #region VectorF (AGS managed struct from .ash converted to class)

    public class VectorF
    {
        // Fields
        public float x;
        public float y;

        // Methods
        public VectorF clone()
        {
            VectorF v = new VectorF();
            v.x = this.x;
            v.y = this.y;
            return v;
        }

        public bool isZero()
        {
            return this.x > -TINY_FLOAT && this.x < TINY_FLOAT && this.y > -TINY_FLOAT && this.y < TINY_FLOAT;
        }

        public static bool isNull(VectorF v)
        {
            return v == null || v.x > -TINY_FLOAT && v.x < TINY_FLOAT && v.y > -TINY_FLOAT && v.y < TINY_FLOAT;
        }

        public float angle()
        {
            return Maths.ArcTan2(this.y, this.x);
        }

        public float length()
        {
            return Maths.Sqrt(this.x * this.x + this.y * this.y);
        }

        public float lengthSquared()
        {
            return (this.x * this.x + this.y * this.y);
        }

        public void add(VectorF v)
        {
            this.x += v.x;
            this.y += v.y;
        }

        public void add2(float x, float y)
        {
            this.x += x;
            this.y += y;
        }

        public void addScaled(VectorF v, float scale)
        {
            this.x += v.x * scale;
            this.y += v.y * scale;
        }

        public void max(VectorF other)
        {
            this.x = Maths.MaxF(this.x, other.x);
            this.y = Maths.MaxF(this.y, other.y);
        }

        public void min(VectorF other)
        {
            this.x = Maths.MinF(this.x, other.x);
            this.y = Maths.MinF(this.y, other.y);
        }

        public void negate()
        {
            this.x = -this.x;
            this.y = -this.y;
        }

        public void normalize()
        {
            float len = this.length();
            if (len == 0.0f)
                return;
            float n = 1.0f / len;
            this.x *= n;
            this.y *= n;
            return;
        }

        public void rotate(float rads)
        {
            float x = this.x * Maths.Cos(rads) - this.y * Maths.Sin(rads);
            float y = this.x * Maths.Sin(rads) + this.y * Maths.Cos(rads);
            this.x = x;
            this.y = y;
        }

        public void scale(float scale)
        {
            this.x *= scale;
            this.y *= scale;
        }

        public void set(VectorF v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        public void set2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public void clampXY(VectorF min, VectorF max)
        {
            this.x = Maths.ClampF(this.x, min.x, max.x);
            this.y = Maths.ClampF(this.y, min.y, max.y);
        }

        public void makeZero()
        {
            this.x = 0.0f;
            this.y = 0.0f;
        }

        public void truncate(float max_length)
        {
            float length = this.length();
            if (length == 0.0f)
                return;
            float n = max_length / length;
            if (n < 1.0f)
            {
                this.x *= n;
                this.y *= n;
            }
        }

        public static VectorF create(int x, int y)
        {
            VectorF v = new VectorF();
            v.x = IntToFloat(x);
            v.y = IntToFloat(y);
            return v;
        }

        public static VectorF createF(float x, float y)
        {
            VectorF v = new VectorF();
            v.x = x;
            v.y = y;
            return v;
        }

        public static float distance(VectorF a, VectorF b)
        {
            return Maths.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
        }

        public static float dotProduct(VectorF a, VectorF b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static float angleBetween(VectorF a, VectorF b)
        {
            float angle = Maths.ArcTan2(b.y, b.x) - Maths.ArcTan2(a.y, a.x);
            return Maths.AnglePiFast(angle);
        }

        public static float projection(VectorF a, VectorF b)
        {
            if (b.isZero())
                return 0.0f;
            return (a.x * b.x + a.y * b.y) / b.length();
        }

        public static VectorF subtract(VectorF a, VectorF b)
        {
            VectorF v = new VectorF();
            v.x = a.x - b.x;
            v.y = a.y - b.y;
            return v;
        }

        public static VectorF zero()
        {
            return new VectorF();
        }

    }

    #endregion

    #region Static class for referencing parent class without prefixing with instance (AGS struct workaround)

    public static class VectorsStaticRef
    {
    }

    #endregion
    
}
