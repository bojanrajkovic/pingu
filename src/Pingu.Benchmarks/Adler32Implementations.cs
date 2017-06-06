using System;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class Adler32Implementations
    {
        Random rand = new Random();
        byte[] data = new byte[5000];

        [GlobalSetup]
        public void SetupData() => rand.NextBytes(data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do16(byte* buf, ref uint a, ref uint b) { Do8(buf, 0, ref a, ref b); Do8(buf, 8, ref a, ref b); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do8(byte* buf, int i, ref uint a, ref uint b) { Do4(buf, i, ref a, ref b); Do4(buf, i + 4, ref a, ref b); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do4(byte* buf, int i, ref uint a, ref uint b) { Do2(buf, i, ref a, ref b); Do2(buf, i + 2, ref a, ref b); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do2(byte* buf, int i, ref uint a, ref uint b) { Do1(buf, i, ref a, ref b); Do1(buf, i + 1, ref a, ref b); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do1(byte* buf, int i, ref uint a, ref uint b) { a += buf[i]; b += a; }

        [Benchmark]
        public unsafe int Smartest()
        {
            uint a = 1, b = 0;
            const int nmax = 5552;

            fixed (byte* ptr = data) {
                byte* buf = ptr;
                int len = data.Length, k;
                while (len > 0) {
                    k = len < nmax ? (int)len : nmax;
                    len -= k;
                    while (k >= 16) {
                        Do16(buf, ref a, ref b);
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
                    k = len < nmax ? (int)len : nmax;
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

            for (var i = 0; i < data.Length; i++) {
                a = (a + data[i]) % 65521;
                b = (b + a) % 65521;
            }

            return unchecked((int)((b << 16) | a));
        }
    }
}
