using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Pingu.Benchmarks
{
    public class Adler32Benchmark
    {
        Random rand = new Random();
        byte[] data = new byte[4096];

        [Setup]
        public void SetupData ()
        {
            rand.NextBytes(data);
        }

        [Benchmark]
        public async Task<uint> ComputeAdler32Pointer()
        {
            return await Adler32.CalculateAdler32Async(data);
        }

        [Benchmark]
        public async Task<uint> ComputeAdler32NoPointer()
        {
            var stream = new MemoryStream(data);
            uint a = 1, b = 0;
            var buffer = new byte[4 * 1024];
            var read = 0;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                for (var i = 0; i < read; i++) {
                    a = (a + buffer[i]) % 65521;
                    b = (b + a) % 65521;
                }
            }

            return (b << 16) | a;
        }
    }
}
