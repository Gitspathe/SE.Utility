using System;
using System.Numerics;
using System.Threading;

namespace SE.Utility
{
    public static class Random
    {
        static int seed = Environment.TickCount;

        static readonly ThreadLocal<System.Random> random =
            new ThreadLocal<System.Random>(() => new System.Random(Interlocked.Increment(ref seed)));

        public static float Next(float max = 1.0f) 
            => (float)(random.Value.NextDouble() * max);

        public static int Next(int max) 
            => random.Value.Next(max);

        public static float Next(float min, float max) 
            => (float)random.Value.NextDouble() * (max - min) + min;

        public static int Next(int min, int max) 
            => random.Value.Next(min, max);

        public static float NextAngle()
        {
#if MODERN_DOTNET
            return Next(-MathF.PI, MathF.PI);
#else
            return Next((float)-Math.PI, (float)Math.PI);
#endif
        }

        public static float NextAngle(float max)
        {
#if MODERN_DOTNET
            return Next(-MathF.PI, -MathF.PI + (MathF.PI * 2.0f * max));
#else
            return Next((float)-Math.PI, (float)(-Math.PI + (Math.PI * 2.0f * max)));
#endif
        }

        public static void NextUnitVector(out Vector2 vector)
        {
            float angle = NextAngle();
#if MODERN_DOTNET
            vector = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
#else
            vector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
#endif
        }

        public static Vector2 NextUnitVector()
        {
            float angle = NextAngle();
#if MODERN_DOTNET
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
#else
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
#endif
        }
    }
}
