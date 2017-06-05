using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class FloorImplementations
    {
        public float[] FloorData { get; private set; }
        public int[] FlooredData { get; private set; }

        [Params(5000)]
        public int FloatsToFloor { get; set; }

        Random r = new Random();

        static float NextFloat(Random random)
        {
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, random.Next(0, 128));
            return (float)(mantissa * exponent);
        }

        [Setup]
        public void Setup()
        {
            FloorData = new float[FloatsToFloor];
            FlooredData = new int[FloatsToFloor];

            for (int i = 0; i < FloatsToFloor; i++)
                FloorData[i] = NextFloat(r);
        }

        [Benchmark(Baseline = true)]
        public void MathFloor()
        {
            for (int i = 0; i < FloatsToFloor; i++)
                FlooredData[i] = (int) Math.Floor(FloorData[i]);
        }

        [Benchmark]
        public void FastFloor()
        {
            // We know our data fits in the range of an int...
            for (int i = 0; i < FloatsToFloor; i++)
                FlooredData[i] = (int)FloorData[i];
        }
    }
}