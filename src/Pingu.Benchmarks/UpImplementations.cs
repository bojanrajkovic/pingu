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
        [Params(5000)]
        public int TotalBytes { get; set; }

        public byte[] TargetBuffer { get; set; }
        public byte[] RawScanline { get; set; }
        public byte[] PreviousScanline { get; set; }

        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [Setup]
        public void Setup()
        {
            TargetBuffer = new byte[TotalBytes];
            RawScanline = new byte[TotalBytes];
            PreviousScanline = new byte[TotalBytes];
            rng.GetBytes(RawScanline);
            rng.GetBytes(PreviousScanline);
        }

        [Benchmark(Baseline = true)]
        public unsafe void Naive()
        {
            int targetOffset = 0;

            for (var i = 0; i < RawScanline.Length; i++)
                TargetBuffer[i + targetOffset] = (byte)(RawScanline[i] - PreviousScanline[i]);
        }

        [Benchmark]
        public unsafe void Pointers()
        {
            int targetOffset = 0;

            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* target = TargetBuffer) {
                for (var i = 0; i < RawScanline.Length; i++)
                    target[i + targetOffset] = (byte)(raw[i] - previous[i]);
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolled()
        {
            int targetOffset = 0;

            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* target = TargetBuffer) {
                int i = 0;
                for (; RawScanline.Length - i > 8; i += 8) {
                    target[i + targetOffset] = (byte)(raw[i] - previous[i]);
                    target[i + 1 + targetOffset] = (byte)(raw[i + 1] - previous[i + 1]);
                    target[i + 2 + targetOffset] = (byte)(raw[i + 2] - previous[i + 2]);
                    target[i + 3 + targetOffset] = (byte)(raw[i + 3] - previous[i + 3]);
                    target[i + 4 + targetOffset] = (byte)(raw[i + 4] - previous[i + 4]);
                    target[i + 5 + targetOffset] = (byte)(raw[i + 5] - previous[i + 5]);
                    target[i + 6 + targetOffset] = (byte)(raw[i + 6] - previous[i + 6]);
                    target[i + 7 + targetOffset] = (byte)(raw[i + 7] - previous[i + 7]);
                }

                for (; i < RawScanline.Length; i++)
                    target[i + targetOffset] = (byte)(raw[i] - previous[i]);
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolledPreOffset()
        {
            int targetOffset = 0;

            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* targetUnoffset = TargetBuffer) {
                byte* target = targetUnoffset + targetOffset;
                int i = 0;
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
            int targetOffset = 0;

            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* targetUnoffset = TargetBuffer) {
                byte* target = targetUnoffset + targetOffset, rawm = raw, prev = previous;
                int i = 0;

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
            int targetOffset = 0;

            var chunks = (int)((float)(length / vecSize));

            fixed (byte* rawPtr = RawScanline) {
                fixed (byte* prevPtr = PreviousScanline) {
                    fixed (byte* targetPtr = TargetBuffer) {
                        for (int i = 0; i < chunks; i++) {
                            int src = i * vecSize;
                            var vec = (new Vector<byte>(RawScanline, src) - new Vector<byte>(PreviousScanline, src));
                            vec.CopyTo(TargetBuffer, src + targetOffset);
                        }

                        int start = vecSize * chunks + targetOffset;
                        for (int i = start; i < length; i++)
                            targetPtr[i] = unchecked((byte)(rawPtr[i] - prevPtr[i]));
                    }
                }
            }
        }
    }
}
