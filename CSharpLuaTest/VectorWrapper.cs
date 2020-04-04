using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;


namespace CSharpLuaTest
{
    public class VectorWrapper
    {
        public readonly Vector<double> V;

        public VectorWrapper(Vector<double> vector)
        {
            V = vector;
        }

        public static VectorWrapper operator *(VectorWrapper lhs, VectorWrapper rhs) => new VectorWrapper(lhs.V.Clone().PointwiseMultiply(rhs.V.Clone()));
        public static VectorWrapper operator /(VectorWrapper lhs, VectorWrapper rhs) => new VectorWrapper(lhs.V.Clone().PointwiseDivide(rhs.V.Clone()));
        public static VectorWrapper operator +(VectorWrapper lhs, VectorWrapper rhs) => new VectorWrapper(lhs.V.Clone().Add(rhs.V.Clone()));
        public static VectorWrapper operator -(VectorWrapper lhs, VectorWrapper rhs) => new VectorWrapper(lhs.V.Clone().Subtract(rhs.V.Clone()));

        public static VectorWrapper operator *(VectorWrapper lhs, double rhs) => new VectorWrapper(lhs.V.Clone() * rhs);
        public static VectorWrapper operator /(VectorWrapper lhs, double rhs) => new VectorWrapper(lhs.V.Clone() / rhs);
        public static VectorWrapper operator +(VectorWrapper lhs, double rhs) => new VectorWrapper(lhs.V.Clone() + rhs);
        public static VectorWrapper operator -(VectorWrapper lhs, double rhs) => new VectorWrapper(lhs.V.Clone() - rhs);

        public static VectorWrapper operator *(double lhs, VectorWrapper rhs) => new VectorWrapper(lhs * rhs.V.Clone());
        public static VectorWrapper operator /(double lhs, VectorWrapper rhs) => new VectorWrapper(lhs / rhs.V.Clone());
        public static VectorWrapper operator +(double lhs, VectorWrapper rhs) => new VectorWrapper(lhs + rhs.V.Clone());
        public static VectorWrapper operator -(double lhs, VectorWrapper rhs) => new VectorWrapper(lhs - rhs.V.Clone());

        public double this[int i]
        {
            get => V[i];
            set => V[i] = value;
        }

        public override string ToString()
        {
            return "wrapper " + V.ToString();
        }
    }
}
