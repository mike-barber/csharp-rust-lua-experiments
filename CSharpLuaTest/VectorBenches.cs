using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Validators;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using NLua;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace CSharpLuaTest
{
    [MemoryDiagnoser]
    public class VectorBenches : IDisposable
    {
        Lua _state;

        VectorWrapper _v1 = new VectorWrapper(new DenseVector(Enumerable.Range(0, 5).Select(x => (double)x).ToArray()));
        VectorWrapper _v2 = new VectorWrapper(new DenseVector(Enumerable.Range(5, 5).Select(x => (double)x).ToArray()));

        public VectorBenches()
        {
            _state = CreateState();
        }

        private static Lua CreateState()
        {
            var state = new Lua();
            state.LoadCLRPackage();
            state.DoString(@"
                import('MathNet.Numerics.LinearAlgebra.Double')
                import('CSharpLuaTest')
            ");

            state["v1"] = new VectorWrapper(new DenseVector(Enumerable.Range(0, 5).Select(x => (double)x).ToArray())); ;
            state["v2"] = new VectorWrapper(new DenseVector(Enumerable.Range(0, 5).Select(x => (double)x).ToArray())); ;

            return state;
        }

        public static void SelfTest()
        {
            var me = new VectorBenches();
            me.ClrSetup();
            me.Baseline();
            me.AddFunction();
            me.AddFunction();
            me.AddFunction();
            me.AddFunction();
            me.Add();
            me.Add();
            me.Add();
            me.Add();
            me.Compound();
        }

        // just setup and teardown of a Lua state
        [Benchmark]
        public void ClrSetup()
        {
            using var state = CreateState();
            state.Close();
        }

        [Benchmark]
        public int Baseline()
        {
            var res = _v1 + _v2;
            return res.V.Count;
        }

        [Benchmark]
        public int AddFunction()
        {
            _state.DoString("function func(a,b) return a+b end");
            var fn = _state.GetFunction("func");
            var res = (VectorWrapper)fn.Call(_v1, _v2)[0];
            return res.V.Count;
        }

        [Benchmark]
        public int Add()
        {
            _state["v1"] = _v1;
            _state["v2"] = _v2;
            var res = (VectorWrapper)_state.DoString("return v1 + v2")[0];
            //_state.DoString("collectgarbage()");
            return res.V.Count;
        }

        [Benchmark]
        public int AddFunc()
        {
            _state["v1"] = _v1;
            _state["v2"] = _v2;
            var res = (VectorWrapper)_state.DoString("return v1 + v2")[0];
            //_state.DoString("collectgarbage()");
            return res.V.Count;
        }

        [Benchmark]
        public int Compound()
        {
            _state["v1"] = _v1;
            _state["v2"] = _v2;
            var res = (VectorWrapper)_state.DoString("return (v1*5 + v2) / 100")[0];
            //_state.DoString("collectgarbage()");
            return res.V.Count;
        }

        public void Dispose()
        {
            _state?.Dispose();
        }
    }
}
