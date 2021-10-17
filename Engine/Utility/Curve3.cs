using System.Numerics;

namespace SE.Utility
{
    public class Curve3
    {
        public Curve X = new Curve();
        public Curve Y = new Curve();
        public Curve Z = new Curve();

        public Curve3()
        {
            X = new Curve();
            Y = new Curve();
            Z = new Curve();
        }

        public Curve3(Curve x, Curve y, Curve z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 Evaluate(float position)
            => new Vector3(
                X.Evaluate(position),
                Y.Evaluate(position),
                Z.Evaluate(position));

        public void Add(float position, Vector3 value)
        {
            X.Keys.Add(position, value.X);
            Y.Keys.Add(position, value.Y);
            Z.Keys.Add(position, value.Z);
        }
    }
}
