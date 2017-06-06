using System;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            Add(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default);
            Add(new RankColumn(NumeralSystem.Arabic));
            Add(HardwareCounter.CacheMisses, HardwareCounter.BranchMispredictions, HardwareCounter.TotalCycles);

            var platform = Environment.OSVersion.Platform;

            if (platform == PlatformID.MacOSX || platform == PlatformID.Unix) {
                Add(Job.Mono);
                Add(Job.Mono.With(Jit.Llvm));
            } else {
                Add(Job.RyuJitX64.With(Runtime.Core).With(CsProjNet46Toolchain.Instance).WithGcServer(true).WithGcConcurrent(true));
                Add(Job.RyuJitX64.With(Runtime.Clr).WithGcServer(true).WithGcConcurrent(true));
                Add(Job.LegacyJitX64.With(Runtime.Clr).WithGcServer(true).WithGcConcurrent(true));
                Add(Job.LegacyJitX86.With(Runtime.Clr).WithGcServer(true).WithGcConcurrent(true));
                Add(Job.Mono.With(new MonoRuntime("Mono", @"C:\Program Files\Mono\bin\mono.exe")));
                Add(Job.Mono.With(new MonoRuntime("Mono", @"C:\Program Files\Mono\bin\mono.exe")).With(Jit.Llvm));
            }
        }
    }
}
