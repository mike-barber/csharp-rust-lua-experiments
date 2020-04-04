using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using MSI = MoonSharp.Interpreter;

namespace CSharpLuaTest
{
    [MemoryDiagnoser]
    public class BasicFunctionTests
    {
        readonly NLua.Lua _luaState = new NLua.Lua();
        readonly NLua.Lua _luaStateFunction = new NLua.Lua();
        readonly NLua.LuaFunction _luaFunction;

        readonly MSI.Script _msScript = new MSI.Script();
        readonly object _msFunction;

        readonly NLua.Lua _luaStateCallback;
        readonly NLua.LuaFunction _luaFunctionCallback;

        public BasicFunctionTests()
        {
            _luaStateFunction.DoString(_factorialFunction);
            _luaFunction = _luaStateFunction.GetFunction("fact");

            _msScript.DoString(_factorialFunction);
            _msFunction = _msScript.Globals["fact"];

            // callbacks
            _luaStateCallback = new NLua.Lua();
            _luaStateCallback.LoadCLRPackage();
            _luaStateCallback.DoString(@"
                import ('CSharpLuaTest')
            ");
            // call the static c# function -- don't need to wrap it inside a "real" lua function
            _luaFunctionCallback = _luaStateCallback.GetFunction("BasicFunctionTests.Factorial");
        }


        public static void SelfTest()
        {
            var me = new BasicFunctionTests();

            Console.WriteLine(me.MoonSharpTest());
            Console.WriteLine(me.NLuaTestNew());
            Console.WriteLine(me.NLuaTestKeep());
            Console.WriteLine(me.NLuaTestFunctionEval());
            Console.WriteLine(me.CSharpFactorial(15));
            Console.WriteLine(me.MoonSharpTestFunction(15));
            Console.WriteLine(me.NLuaTestFunctionCall(15));
            Console.WriteLine(me.NLuaTestFunctionCallback(15));
        }

        static string _factorialScript = @"    
        -- defines a factorial function
        function fact (n)
            if (n == 0) then
                return 1
            else
                return n*fact(n - 1)
            end
        end

        return fact(5)";

        static string _factorialFunction = @"    
        -- defines a factorial function
        function fact (n)
            if (n == 0) then
                return 1
            else
                return n*fact(n - 1)
            end
        end";


        [Benchmark]
        public long MoonSharpTest()
        {
            var res = MoonSharp.Interpreter.Script.RunString(_factorialScript);
            return (long)res.Number;
        }

        [Benchmark]
        public long NLuaTestNew()
        {
            using var state = new NLua.Lua();
            var res = state.DoString(_factorialScript);
            return (long)res[0];
        }

        [Benchmark]
        public long NLuaTestKeep()
        {
            var res = _luaState.DoString(_factorialScript);
            return (long)res[0];
        }


        [Benchmark]
        public long NLuaTestFunctionEval()
        {
            var res = _luaStateFunction.DoString("return fact(5)");
            return (long)res[0];
        }

        public static long Factorial(int n)
        {
            if (n == 0)
            {
                return 1;
            }
            return n * Factorial(n - 1);
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(5)]
        [Arguments(10)]
        [Arguments(15)]
        public long CSharpFactorial(int n)
        {
            return Factorial(n);
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(5)]
        [Arguments(10)]
        [Arguments(15)]
        public long NLuaTestFunctionCall(int n)
        {
            return (long)_luaFunction.Call(n)[0];
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(5)]
        [Arguments(10)]
        [Arguments(15)]
        public long MoonSharpTestFunction(int n)
        {
            var res = _msScript.Call(_msFunction, n);
            return (long)res.Number;
        }

        [Benchmark]
        [Arguments(1)]
        [Arguments(5)]
        [Arguments(10)]
        [Arguments(15)]
        public long NLuaTestFunctionCallback(int n)
        {
            return (long)_luaFunctionCallback.Call(n)[0];
        }
    }
}
