﻿using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks.ImplementationBenchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class PaethFilterBenchmark
    {
        const int TotalBytes = 5000;
        byte[] targetBuffer, rawScanline, previousScanline;

        static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        [Params(3, 4)]
        public int BytesPerPixel { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            targetBuffer = new byte[TotalBytes];
            rawScanline = new byte[TotalBytes];
            previousScanline = new byte[TotalBytes];
            Rng.GetBytes(rawScanline);
            Rng.GetBytes(previousScanline);
        }

        [Benchmark(Baseline = true)]
        public void PinguPaethFilterPointers()
        {
            var filter = Filters.PaethFilter.Instance;
            filter.FilterInto(targetBuffer, 0, rawScanline, previousScanline, BytesPerPixel);
        }
    }
}
