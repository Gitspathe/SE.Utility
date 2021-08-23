using System;
using System.Numerics;

namespace SE.Utility
{
    // Based off of Mercury Particle Engine's FastRand:
    // https://github.com/Matthew-Davey/mercury-particle-engine/blob/develop/Mercury.ParticleEngine.Core/FastRand.cs

    /// <summary>
    /// Fast random generator.
    /// </summary>
    public sealed class FRandom
    {
        private int x;

        public FRandom(int seed = 0)
        {
            if (seed == 0)
                seed = Environment.TickCount;

            x = seed;
        }

        public int Next()
        {
            x = 214013 * x + 2531011;
            return (x >> 16) & 0x7FFF;
        }

        public int Next(int max) => (int)(max * NextSingle());

        public int Next(int min, int max) => (int)((max - min) * NextSingle()) + min;

        public float NextSingle() => Next() / (float)short.MaxValue;

        public float NextSingle(float max) => max * NextSingle();

        public float NextSingle(float min, float max) => ((max - min) * NextSingle()) + min;

        public float NextAngle()
        {
#if MODERN_DOTNET
            return NextSingle(-MathF.PI, MathF.PI);
#else
            return NextSingle((float)-Math.PI, (float)Math.PI);
        #endif
        }

        public float NextAngle(float max)
        {
        #if MODERN_DOTNET
            return NextSingle(-MathF.PI, -MathF.PI + (MathF.PI * 2.0f * max));
        #else
            return NextSingle((float)-Math.PI, (float)(-Math.PI + (Math.PI * 2.0f * max)));
        #endif
        }

        public void NextUnitVector(out Vector2 vector)
        {
            float angle = NextAngle();
        #if MODERN_DOTNET
            vector = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        #else
            vector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        #endif
        }

        public Vector2 NextUnitVector()
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
