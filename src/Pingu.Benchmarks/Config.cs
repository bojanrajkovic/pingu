using System.IO;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Pingu.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            var isMonoPlatform = Path.DirectorySeparatorChar == '/';

            if (!isMonoPlatform)
                Add(MemoryDiagnoser.Default);

            Add(new RankColumn(NumeralSystem.Arabic));

            Add(JsonExporter.FullCompressed, JsonExporter.Full, JsonExporter.Brief);

            if (isMonoPlatform) {
                Add(Job.Mono);
                Add(Job.Mono.With(Jit.Llvm));
                Add(Job.RyuJitX64.With(Runtime.Core).WithGcServer(true).WithGcConcurrent(true));
            } else {
                Add(Job.RyuJitX64.With(Runtime.Core).WithGcConcurrent(true));
                Add(Job.RyuJitX64.With(Runtime.Clr).WithGcConcurrent(true));
            }
        }
    }
}
