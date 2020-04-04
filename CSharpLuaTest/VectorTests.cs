using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLua;

using MLA = MathNet.Numerics.LinearAlgebra;

namespace CSharpLuaTest
{
    public class VectorWrapper
    {
        public readonly MLA.Vector<double> V;

        public VectorWrapper(MLA.Vector<double> vector)
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


    public class VectorTests
    {
        public static DenseVector V1() => new DenseVector(new double[] { 0, 1, 2, 3, 4 });
        public static DenseVector V2() => new DenseVector(new double[] { 5, 6, 7, 8, 9 });

        /// <summary>
        /// Simple case -- calling a C# function 
        /// </summary>
        public static void AddVectors()
        {
            using var state = new Lua();
            state.LoadCLRPackage();
            state.DoString(@"
                import('MathNet.Numerics.LinearAlgebra.Double')
            ");

            var v1 = V1();
            var v2 = V2();
            state["v1"] = v1;
            state["v2"] = v2;

            state.DoString(@"
                res = v1:Add(v2)
            ");

            var res = state["res"] as DenseVector;
            Console.WriteLine(res);
        }

        /// <summary>
        /// This is a bit messy -- overriding the metadata table
        /// </summary>
        public static void AddVectorsOperator()
        {
            using var state = new Lua();
            state.LoadCLRPackage();
            state.DoString(@"
                import('MathNet.Numerics.LinearAlgebra.Double')
            ");

            // Setup correct overloads for DenseVector -- these don't get set correctly by default, 
            // although add and subtract are.
            var refVec = new DenseVector(1);
            state["ref_vec"] = refVec;
            state.DoString(@"
                getmetatable(ref_vec).__mul = function(a,b) return a:PointwiseMultiply(b) end
                getmetatable(ref_vec).__div = function(a,b) return a:PointwiseDivide(b) end
            ");

            var v1 = V1();
            var v2 = V2();
            state["v1"] = V1();
            state["v2"] = V2();

            // no messing with metatables -- this just works
            var add = state.DoString(@"
                return v1 + v2
            ")[0] as DenseVector;
            Console.WriteLine("add " + add);

            var sub = state.DoString(@"
                return v1 - v2
            ")[0] as DenseVector;
            Console.WriteLine("sub " + sub);

            // these require the metatable to be set up correctly to work
            var mul = state.DoString(@"
                return v1 * v2
            ")[0] as DenseVector;
            Console.WriteLine("mul " + mul);

            var div = state.DoString(@"
                return v1 / v2
            ")[0] as DenseVector;
            Console.WriteLine("div " + div);

            state.DoString(@"print(v1.mt)");
            state.DoString(@"print(v2.mt)");
            state.DoString(@"print(getmetatable(v1))");
            state.DoString(@"print(getmetatable(v2))");

            var x = v1.PointwiseMultiply(v2);
        }

        /// <summary>
        /// Probably easier to do this -- just define a wrapper.
        /// </summary>
        public static void AddVectorsWrapper()
        {
            using var state = new Lua();
            state.LoadCLRPackage();
            state.DoString(@"
                import('MathNet.Numerics.LinearAlgebra.Double')
                import('CSharpLuaTest')
            ");

            var v1 = new VectorWrapper(V1());
            var v2 = new VectorWrapper(V2());

            state["v1"] = v1;
            state["v2"] = v2;

            // no messing with metatables -- this just works
            var add = state.DoString(@"
                return v1 + v2
            ")[0] as VectorWrapper;
            Console.WriteLine("add " + add);

            var sub = state.DoString(@"
                return v1 - v2
            ")[0] as VectorWrapper;
            Console.WriteLine("sub " + sub);

            // these require the metatable to be set up correctly to work
            var mul = state.DoString(@"
                return v1 * v2
            ")[0] as VectorWrapper;
            Console.WriteLine("mul " + mul);

            var div = state.DoString(@"
                return v1 / v2
            ")[0] as VectorWrapper;
            Console.WriteLine("div " + div);

            var compound = state.DoString(@"
                return (v1 / 4.0 + 10) * 3.3
            ")[0] as VectorWrapper;
            Console.WriteLine("compound " + compound);

            state.DoString(@"print(v1.mt)");
            state.DoString(@"print(v2.mt)");
            state.DoString(@"print(getmetatable(v1))");
            state.DoString(@"print(getmetatable(v2))");
        }



    }
}
