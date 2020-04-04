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
    public class Program
    {
        static void Main(string[] args)
        {
            // test basic functions
            BasicFunctionTests.Test();

            // test some more interesting stuff
            VectorTests.AddVectors();
            VectorTests.AddVectorsOperator();
            VectorTests.AddVectorsWrapper();

            // setup for a relatively quick run
            var benchmarkConfig = DefaultConfig.Instance.With(
                Job.Default
                    .WithIterationCount(10)
                    .WithIterationTime(TimeInterval.FromMilliseconds(100)));

            BenchmarkRunner.Run<BasicFunctionTests>(benchmarkConfig);
        }
    }
}
