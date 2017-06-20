using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks.ImplementationBenchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class SubFilterBenchmark
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
        public void PinguSubFilterVectors()
        {
            var filter = Filters.SubFilter.Instance;
            filter.VectorAndPointerFilterInto(targetBuffer, 0, rawScanline, BytesPerPixel);
        }

        [Benchmark]
        public void PinguSubFilterPointers()
        {
            var filter = Filters.SubFilter.Instance;
            filter.UnrolledPointerFilterInto(targetBuffer, 0, rawScanline, BytesPerPixel);
        }
    }
}
