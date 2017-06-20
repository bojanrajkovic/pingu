using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class Adler32Implementations
    {
        readonly Random rand = new Random();
        readonly byte[] data = new byte[5000];

        [GlobalSetup]
        public void SetupData() => rand.NextBytes(data);

        [Benchmark]
        public unsafe int Smartest()
        {
            uint a = 1, b = 0;
            const int nmax = 5552;

            fixed (byte* ptr = data) {
                var buf = ptr;
                var len = data.Length;
                while (len > 0) {
                    var k = len < nmax ? len : nmax;
                    len -= k;
                    while (k >= 16) {
                        // This is a hand-unrolled 16 byte loop. Do not touch.
                        a += buf[0];
                        b += a;
                        a += buf[1];
                        b += a;
                        a += buf[2];
                        b += a;
                        a += buf[3];
                        b += a;
                        a += buf[4];
                        b += a;
                        a += buf[5];
                        b += a;
                        a += buf[6];
                        b += a;
                        a += buf[7];
                        b += a;
                        a += buf[8];
                        b += a;
                        a += buf[9];
                        b += a;
                        a += buf[10];
                        b += a;
                        a += buf[11];
                        b += a;
                        a += buf[12];
                        b += a;
                        a += buf[13];
                        b += a;
                        a += buf[14];
                        b += a;
                        a += buf[15];
                        b += a;
                        // End hand-unrolled loop.
                        buf += 16;
                        k -= 16;
                    }
                    if (k != 0) {
                        do {
                            a += *buf++;
                            b += a;
                        } while (--k > 0);
                    }
                    a %= 65521;
                    b %= 65521;
                }
                return unchecked((int)((b << 16) | a));
            }
        }

        [Benchmark]
        public unsafe int Smarter()
        {
            uint a = 1, b = 0;
            const int nmax = 5552;

            fixed (byte* ptr = data) {
                byte* buf = ptr;
                int len = data.Length, k;
                while (len > 0) {
                    k = len < nmax ? len : nmax;
                    len -= k;
                    if (k != 0) {
                        do {
                            a += *buf++;
                            b += a;
                        } while (--k > 0);
                    }
                    a %= 65521;
                    b %= 65521;
                }
                return unchecked((int)((b << 16) | a));
            }
        }

        [Benchmark]
        public unsafe int UsePointer()
        {
            uint a = 1, b = 0;

            fixed (byte* ptr = data) {
                for (var i = 0; i < data.Length; i++) {
                    a = (a + ptr[i]) % 65521;
                    b = (b + a) % 65521;
                }
            }

            return unchecked((int)((b << 16) | a));
        }

        [Benchmark(Baseline = true)]
        public int NoPointer()
        {
            uint a = 1, b = 0;

            // ReSharper disable ForCanBeConvertedToForeach
            for (var i = 0; i < data.Length; i++) {
            // ReSharper restore ForCanBeConvertedToForeach
                a = (a + data[i]) % 65521;
                b = (b + a) % 65521;
            }

            return unchecked((int)((b << 16) | a));
        }
    }
}
