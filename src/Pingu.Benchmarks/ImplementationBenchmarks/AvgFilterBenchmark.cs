using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks.ImplementationBenchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class AvgFilterBenchmark
    {
        const int TotalBytes = 5000;
        byte[] targetBuffer, rawScanline, previousScanline;

        [Params(3, 4)]
        public int BytesPerPixel { get; set; }

        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [GlobalSetup]
        public void Setup()
        {
            targetBuffer = new byte[TotalBytes];
            rawScanline = new byte[TotalBytes];
            previousScanline = new byte[TotalBytes];
            rng.GetBytes(rawScanline);
            rng.GetBytes(previousScanline);
        }

        [Benchmark(Baseline = true)]
        public void PinguAvgFilter()
        {
            var filter = Filters.AvgFilter.Instance;
            filter.FilterInto(targetBuffer, 0, rawScanline, previousScanline, BytesPerPixel);
        }
    }
}
