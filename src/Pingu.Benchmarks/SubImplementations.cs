using System;
using System.Numerics;
using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{

    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class SubImplementations
    {
        const int BytesPerPixel = 4;
        const int TargetOffset = 0;

        public int TotalBytes { get; set; } = 5000;
        public byte[] RawScanline { get; private set; }
        public byte[] TargetBuffer { get; private set; }

        static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        [GlobalSetup]
        public void Setup()
        {
            RawScanline = new byte[TotalBytes];
            TargetBuffer = new byte[TotalBytes];
            Rng.GetBytes(RawScanline);
        }

        [Benchmark]
        public unsafe void Pointers()
        {
            fixed (byte* targetPtr = TargetBuffer)
            fixed (byte* scanlinePtr = RawScanline) {
                Buffer.MemoryCopy(scanlinePtr, targetPtr + TargetOffset, RawScanline.Length, BytesPerPixel);

                unchecked {
                    // We start immediately after the first pixel--its bytes are unchanged. We only copied
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline.
                    for (var x = BytesPerPixel; x < RawScanline.Length; x++)
                        targetPtr[x + TargetOffset] = (byte)(scanlinePtr[x] - scanlinePtr[x - BytesPerPixel]);
                }
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolled()
        {
            fixed (byte* target = TargetBuffer)
            fixed (byte* raw = RawScanline) {
                Buffer.MemoryCopy(raw, target + TargetOffset, RawScanline.Length, BytesPerPixel);

                unchecked {
                    // We start immediately after the first pixel--its bytes are unchanged. We only copied
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline. Unroll
                    // the loop a bit, as well.
                    var x = BytesPerPixel;
                    for (; RawScanline.Length - x > 8; x += 8) {
                        target[x + TargetOffset] = (byte)(raw[x] - raw[x - BytesPerPixel]);
                        target[x + 1 + TargetOffset] = (byte)(raw[x + 1] - raw[x + 1 - BytesPerPixel]);
                        target[x + 2 + TargetOffset] = (byte)(raw[x + 2] - raw[x + 2 - BytesPerPixel]);
                        target[x + 3 + TargetOffset] = (byte)(raw[x + 3] - raw[x + 3 - BytesPerPixel]);
                        target[x + 4 + TargetOffset] = (byte)(raw[x + 4] - raw[x + 4 - BytesPerPixel]);
                        target[x + 5 + TargetOffset] = (byte)(raw[x + 5] - raw[x + 5 - BytesPerPixel]);
                        target[x + 6 + TargetOffset] = (byte)(raw[x + 6] - raw[x + 6 - BytesPerPixel]);
                        target[x + 7 + TargetOffset] = (byte)(raw[x + 7] - raw[x + 7 - BytesPerPixel]);
                    }

                    for (; x < RawScanline.Length; x++)
                        target[x + TargetOffset] = (byte)(raw[x] - raw[x - BytesPerPixel]);
                }
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolledPreOffset()
        {
            fixed (byte* targetPreoffset = TargetBuffer)
            fixed (byte* raw = RawScanline) {
                var target = targetPreoffset + TargetOffset;
                Buffer.MemoryCopy(raw, target, RawScanline.Length, BytesPerPixel);

                unchecked {
                    // We start immediately after the first pixel--its bytes are unchanged. We only copied
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline. Unroll
                    // the loop a bit, as well.
                    var x = BytesPerPixel;
                    for (; RawScanline.Length - x > 8; x += 8) {
                        target[x] = (byte)(raw[x] - raw[x - BytesPerPixel]);
                        target[x + 1] = (byte)(raw[x + 1] - raw[x + 1 - BytesPerPixel]);
                        target[x + 2] = (byte)(raw[x + 2] - raw[x + 2 - BytesPerPixel]);
                        target[x + 3] = (byte)(raw[x + 3] - raw[x + 3 - BytesPerPixel]);
                        target[x + 4] = (byte)(raw[x + 4] - raw[x + 4 - BytesPerPixel]);
                        target[x + 5] = (byte)(raw[x + 5] - raw[x + 5 - BytesPerPixel]);
                        target[x + 6] = (byte)(raw[x + 6] - raw[x + 6 - BytesPerPixel]);
                        target[x + 7] = (byte)(raw[x + 7] - raw[x + 7 - BytesPerPixel]);
                    }

                    for (; x < RawScanline.Length; x++)
                        target[x] = (byte)(raw[x] - raw[x - BytesPerPixel]);
                }
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolledPreOffsetMotion()
        {
            fixed (byte* targetPreoffset = TargetBuffer)
            fixed (byte* raw = RawScanline) {
                var target = targetPreoffset + TargetOffset;
                Buffer.MemoryCopy(raw, target, RawScanline.Length, BytesPerPixel);

                unchecked {
                    // We start immediately after the first pixel--its bytes are unchanged. We only copied
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline. Unroll
                    // the loop a bit, as well.
                    var x = BytesPerPixel;

                    target += BytesPerPixel;
                    byte* rawm = raw + BytesPerPixel, rawBpp = raw;

                    for (; RawScanline.Length - x > 8; x += 8) {
                        target[0] = (byte)(rawm[0] - rawBpp[0]);
                        target[1] = (byte)(rawm[1] - rawBpp[1]);
                        target[2] = (byte)(rawm[2] - rawBpp[2]);
                        target[3] = (byte)(rawm[3] - rawBpp[3]);
                        target[4] = (byte)(rawm[4] - rawBpp[4]);
                        target[5] = (byte)(rawm[5] - rawBpp[5]);
                        target[6] = (byte)(rawm[6] - rawBpp[6]);
                        target[7] = (byte)(rawm[7] - rawBpp[7]);
                        target += 8; rawm += 8; rawBpp += 8;
                    }

                    for (; x < RawScanline.Length; x++) {
                        target[0] = (byte)(rawm[0] - rawBpp[0]);
                        target++; rawm++; rawBpp++;
                    }
                }
            }
        }

        [Benchmark(Baseline = true)]
        public void Naive()
        {
            Buffer.BlockCopy(RawScanline, 0, TargetBuffer, TargetOffset, BytesPerPixel);

            unchecked {
                // We start immediately after the first pixel--its bytes are unchanged. We only copied
                // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline.
                for (var x = BytesPerPixel; x < RawScanline.Length; x++)
                    TargetBuffer[x + TargetOffset] = (byte)(RawScanline[x] - RawScanline[x - BytesPerPixel]);
            }
        }

        [Benchmark]
        public unsafe void VectorAndPointer()
        {
            int vecSize = Vector<byte>.Count, length = RawScanline.Length;
            var chunks = (int)((float)(length - BytesPerPixel) / vecSize);

            fixed (byte* raw = RawScanline)
            fixed (byte* target = TargetBuffer) {
                Buffer.MemoryCopy(raw, target + TargetOffset, length, BytesPerPixel);

                for (var i = 0; i < chunks; i++) {
                    int src = i * vecSize, dst = src + BytesPerPixel;
                    var vec = new Vector<byte>(RawScanline, dst) - new Vector<byte>(RawScanline, src);
                    vec.CopyTo(TargetBuffer, dst + TargetOffset);
                }

                var start = BytesPerPixel + (vecSize * chunks) + TargetOffset;
                for (var i = start; i < length; i++)
                    target[i] = unchecked((byte)(raw[i] - raw[i - BytesPerPixel]));
            }
        }
    }
}
