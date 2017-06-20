using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class Crc32Implementations
    {
        [Params(5000)]
        public int TotalBytes { get; set; }

        public byte[] Data { get; set; }

        static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        [GlobalSetup]
        public void Setup()
        {
            Data = new byte[TotalBytes];
            Rng.GetBytes(Data);
        }

        [Benchmark(Baseline = true)]
        public int MyCrc32()
        {
            return Checksums.Crc32.Compute(Data);
        }

        [Benchmark]
        public uint CoreFxCrc32()
        {
            return System.IO.Compression.Crc32Helper.UpdateCrc32(0, Data, 0, Data.Length);
        }
    }
}
