using System;

using BenchmarkDotNet.Attributes;

using Pingu.Checksums;

namespace Pingu.Benchmarks
{
    public class Adler32Benchmark
    {
        Random rand = new Random();
        byte[] data = new byte[4096];

        [Setup]
        public void SetupData() => rand.NextBytes(data);

        [Benchmark]
        public int ComputeAdler32Pointer() => Adler32.Compute(data);

        [Benchmark]
        public int ComputeAdler32NoPointer()
        {
            uint a = 1, b = 0;

            for (var i = 0; i < data.Length; i++) {
                a = (a + data[i]) % 65521;
                b = (b + a) % 65521;
            }

            return unchecked((int)((b << 16) | a));
        }
    }
}
