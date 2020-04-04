using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
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
            BasicFunctionTests.SelfTest();

            // test some more interesting stuff
            VectorTests.AddVectors();
            VectorTests.AddVectorsOperator();
            VectorTests.AddVectorsWrapper();
            VectorTests.DictionaryTest();

            // setup for a relatively quick run
            var benchmarkConfig = DefaultConfig.Instance.With(
                Job.Default
                    .With(CoreRuntime.Core31)
                    .With(BenchmarkDotNet.Toolchains.InProcess.NoEmit.InProcessNoEmitToolchain.Instance)
                    .WithIterationCount(10)
                    .WithIterationTime(TimeInterval.FromMilliseconds(100)));

            VectorBenches.SelfTest();

            BenchmarkRunner.Run<VectorBenches>(benchmarkConfig);
            BenchmarkRunner.Run<BasicFunctionTests>(benchmarkConfig);
        }
    }
}
