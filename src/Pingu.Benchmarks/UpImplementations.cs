using System.Numerics;
using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class UpImplementations
    {
        const int TargetOffset = 0;
        public int TotalBytes { get; set; } = 5000;

        public byte[] TargetBuffer { get; private set; }
        public byte[] RawScanline { get; private set; }
        public byte[] PreviousScanline { get; private set; }

        static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        [GlobalSetup]
        public void Setup()
        {
            TargetBuffer = new byte[TotalBytes];
            RawScanline = new byte[TotalBytes];
            PreviousScanline = new byte[TotalBytes];
            Rng.GetBytes(RawScanline);
            Rng.GetBytes(PreviousScanline);
        }

        [Benchmark(Baseline = true)]
        public void Naive()
        {
            for (var i = 0; i < RawScanline.Length; i++)
                TargetBuffer[i + TargetOffset] = (byte)(RawScanline[i] - PreviousScanline[i]);
        }

        [Benchmark]
        public unsafe void Pointers()
        {
            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* target = TargetBuffer) {
                for (var i = 0; i < RawScanline.Length; i++)
                    target[i + TargetOffset] = (byte)(raw[i] - previous[i]);
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolled()
        {
            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* target = TargetBuffer) {
                var i = 0;
                for (; RawScanline.Length - i > 8; i += 8) {
                    target[i + TargetOffset] = (byte)(raw[i] - previous[i]);
                    target[i + 1 + TargetOffset] = (byte)(raw[i + 1] - previous[i + 1]);
                    target[i + 2 + TargetOffset] = (byte)(raw[i + 2] - previous[i + 2]);
                    target[i + 3 + TargetOffset] = (byte)(raw[i + 3] - previous[i + 3]);
                    target[i + 4 + TargetOffset] = (byte)(raw[i + 4] - previous[i + 4]);
                    target[i + 5 + TargetOffset] = (byte)(raw[i + 5] - previous[i + 5]);
                    target[i + 6 + TargetOffset] = (byte)(raw[i + 6] - previous[i + 6]);
                    target[i + 7 + TargetOffset] = (byte)(raw[i + 7] - previous[i + 7]);
                }

                for (; i < RawScanline.Length; i++)
                    target[i + TargetOffset] = (byte)(raw[i] - previous[i]);
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolledPreOffset()
        {
            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* targetUnoffset = TargetBuffer) {
                var target = targetUnoffset + TargetOffset;
                var i = 0;
                for (; RawScanline.Length - i > 8; i += 8) {
                    target[i] = (byte)(raw[i] - previous[i]);
                    target[i + 1] = (byte)(raw[i + 1] - previous[i + 1]);
                    target[i + 2] = (byte)(raw[i + 2] - previous[i + 2]);
                    target[i + 3] = (byte)(raw[i + 3] - previous[i + 3]);
                    target[i + 4] = (byte)(raw[i + 4] - previous[i + 4]);
                    target[i + 5] = (byte)(raw[i + 5] - previous[i + 5]);
                    target[i + 6] = (byte)(raw[i + 6] - previous[i + 6]);
                    target[i + 7] = (byte)(raw[i + 7] - previous[i + 7]);
                }

                for (; i < RawScanline.Length; i++)
                    target[i] = (byte)(raw[i] - previous[i]);
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolledPreOffsetMotion()
        {
            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* targetUnoffset = TargetBuffer) {
                byte* target = targetUnoffset + TargetOffset, rawm = raw, prev = previous;
                var i = 0;

                for (; RawScanline.Length - i > 8; i += 8) {
                    target[0] = (byte)(rawm[0] - prev[0]);
                    target[1] = (byte)(rawm[1] - prev[1]);
                    target[2] = (byte)(rawm[2] - prev[2]);
                    target[3] = (byte)(rawm[3] - prev[3]);
                    target[4] = (byte)(rawm[4] - prev[4]);
                    target[5] = (byte)(rawm[5] - prev[5]);
                    target[6] = (byte)(rawm[6] - prev[6]);
                    target[7] = (byte)(rawm[7] - prev[7]);
                    target += 8; rawm += 8; prev += 8;
                }

                for (; i < RawScanline.Length; i++) {
                    target[0] = (byte)(rawm[0] - prev[0]);
                    target++; rawm++; prev++;
                }
            }
        }

        [Benchmark]
        public unsafe void VectorAndPointer()
        {
            int vecSize = Vector<byte>.Count, length = TotalBytes;
            var chunks = (int)((float)(length / vecSize));

            fixed (byte* rawPtr = RawScanline) {
                fixed (byte* prevPtr = PreviousScanline) {
                    fixed (byte* targetPtr = TargetBuffer) {
                        for (var i = 0; i < chunks; i++) {
                            var src = i * vecSize;
                            var vec = (new Vector<byte>(RawScanline, src) - new Vector<byte>(PreviousScanline, src));
                            vec.CopyTo(TargetBuffer, src + TargetOffset);
                        }

                        var start = vecSize * chunks + TargetOffset;
                        for (var i = start; i < length; i++)
                            targetPtr[i] = unchecked((byte)(rawPtr[i] - prevPtr[i]));
                    }
                }
            }
        }
    }
}
