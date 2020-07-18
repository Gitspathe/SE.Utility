using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SE.Core.Extensions
{
    public static class NumericsExtensions
    {
#if NETSTANDARD2_1
        public const float _PI_OVER180 = MathF.PI / 180;
#else
        public const float _PI_OVER180 = (float)Math.PI / 180;
#endif

        public static double ToRadians(this double val)
        {
            return _PI_OVER180 * val;
        }

        public static float ToRadians(this float val)
        {
            return _PI_OVER180 * val;
        }

        public static int Round(this int i, int increment)
        {
            return ((int)(i / (float)increment) * increment);
        }

        public static float ToRotation(this Vector2 vector)
        {
#if NETSTANDARD2_1
            return MathF.Atan2(vector.X, -vector.Y);
#else
            return (float) Math.Atan2(vector.X, -vector.Y);
#endif
        }

        public static Vector2 GetRotationVector(this float degrees, float length = 1.0f)
        {
#if NETSTANDARD2_1
            return new Vector2(MathF.Cos(degrees.ToRadians()) * length, MathF.Sin(degrees.ToRadians()) * length);
#else
            return new Vector2((float)Math.Cos(degrees.ToRadians()) * length, (float)Math.Sin(degrees.ToRadians()) * length);
#endif
        }

        public static float GetRotationFacing(this Vector2 srcPos, Vector2 destPos)
        {
            Vector2 vec = srcPos - destPos;
#if NETSTANDARD2_1
            return MathF.Atan2(vec.Y, vec.X);
#else
            return (float)Math.Atan2(vec.Y, vec.X);
#endif
        }

        public static bool Intersects(this Vector4 bounds, Vector2 point) 
            => bounds.X <= point.X 
               && point.X < bounds.X + bounds.Z 
               && bounds.Y <= point.Y 
               && point.Y < bounds.Y + bounds.W;

        public static bool Intersects(this Vector4 bounds, Vector4 otherBounds) 
            => otherBounds.X < bounds.X + bounds.Z &&
               bounds.X < otherBounds.X + otherBounds.Z &&
               otherBounds.Y < bounds.Y + bounds.W &&
               bounds.Y < otherBounds.Y + otherBounds.W;
    }

}
