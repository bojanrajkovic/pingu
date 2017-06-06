using System.Runtime.CompilerServices;
using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class MinSadImplementations
    {
        [Params(5000)]
        public int TotalBytes { get; set; }

        public byte[] Data { get; set; }

        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [GlobalSetup]
        public void Setup()
        {
            Data = new byte[TotalBytes];
            rng.GetBytes(Data);
        }

        [Benchmark(Baseline = true)]
        public unsafe int ByteByByte()
        {
            fixed (byte* ptr = Data) {
                int sum = 0;
                for (var i = 0; i < TotalBytes; i++) {
                    var val = ptr[i];
                    sum += val < 128 ? val : 256 - val;
                }
                return sum;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int Abs(int value)
        {
            int temp = value >> 31;
            value ^= temp;
            value += temp & 1;
            return value;
        }

        [Benchmark]
        public unsafe int SignedBytesByByteFastAbs()
        {
            int sum = 0;
            unchecked {
                fixed (byte* ptr = Data) {
                    sbyte* sb = (sbyte*) ptr;
                    for (var i = 0; i < TotalBytes; i++)
                        sum += Abs(sb[i]);
                }
            }
            return sum;
        }

        [Benchmark]
        public unsafe int SignedBytesUnrolledFastAbs8 ()
        {
            int sum = 0, len = TotalBytes;
            unchecked {
                fixed (byte* ptr = Data) {
                    sbyte* sb = (sbyte*)ptr;
                    for (; len >= 8; len -= 8, sb += 8)
                        sum += Abs(sb[0]) + Abs(sb[1]) + Abs(sb[2]) + Abs(sb[3]) +
                               Abs(sb[4]) + Abs(sb[5]) + Abs(sb[6]) + Abs(sb[7]);
                    for (; len > 0; len--, sb++)
                        sum += Abs(sb[0]);
                }
            }
            return sum;
        }

        [Benchmark]
        public unsafe int SignedBytesUnrolledFastAbs16()
        {
            int sum = 0, len = TotalBytes;
            unchecked {
                fixed (byte* ptr = Data) {
                    sbyte* sb = (sbyte*)ptr;
                    for (; len >= 16; len -= 16, sb += 16)
                        sum += Abs(sb[0]) + Abs(sb[1]) + Abs(sb[2]) + Abs(sb[3]) +
                               Abs(sb[4]) + Abs(sb[5]) + Abs(sb[6]) + Abs(sb[7]) +
                               Abs(sb[8]) + Abs(sb[9]) + Abs(sb[10]) + Abs(sb[11]) +
                               Abs(sb[12]) + Abs(sb[13]) + Abs(sb[14]) + Abs(sb[15]);
                    for (; len > 0; len--, sb++)
                        sum += Abs(sb[0]);
                }
            }
            return sum;
        }
    }
}
