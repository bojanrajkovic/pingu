using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class UpFilterBenchmark
    {
        const int TotalBytes = 5000;
        byte[] targetBuffer, rawScanline, previousScanline;

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
        public void PinguUpFilterVectors()
        {
            var filter = Filters.UpFilter.Instance;
            filter.VectorAndPointerFilterInto(targetBuffer, 0, rawScanline, previousScanline);
        }

        [Benchmark]
        public void PinguUpFilterPointers()
        {
            var filter = Filters.UpFilter.Instance;
            filter.UnrolledPointerFilterInto(targetBuffer, 0, rawScanline, previousScanline);
        }
    }
}
