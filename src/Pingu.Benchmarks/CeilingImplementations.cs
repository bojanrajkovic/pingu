using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Arabic)]
    [RyuJitX64Job, LegacyJitX64Job, MonoJob]
    public class CeilingImplementations
    {
        public double[] CeilingData { get; private set; }
        public int[] CeilingedData { get; private set; }

        [Params(5000)]
        public int DoublesToCeil { get; set; }

        Random r = new Random();

        static double NextDouble(Random random)
        {
            return random.NextDouble() * (100 - 0) + 0;
        }

        [Setup]
        public void Setup()
        {
            CeilingData = new double[DoublesToCeil];
            CeilingedData = new int[DoublesToCeil];

            for (int i = 0; i < DoublesToCeil; i++)
                CeilingData[i] = NextDouble(r);
        }

        [Benchmark(Baseline = true)]
        public void MathCeil()
        {
            for (int i = 0; i < DoublesToCeil; i++)
                CeilingedData[i] = (int)Math.Ceiling(CeilingData[i]);
        }

        const double doubleMagicDelta = (1.5e-8);
        const double doubleMagicRoundEps = (.5f - doubleMagicDelta);
        const double doubleMagic = 6755399441055744.0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe int RoundToInt(double val)
        {
            val = val + doubleMagicRoundEps + doubleMagic;
            return ((int*)&val)[0];
        }

        [Benchmark]
        public unsafe void FastCeil()
        {
            for (int i = 0; i < DoublesToCeil; i++)
                CeilingedData[i] = RoundToInt(CeilingData[i]);
        }
    }
}
