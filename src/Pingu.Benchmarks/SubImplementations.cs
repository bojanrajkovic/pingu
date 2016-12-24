using System;
using System.Numerics;
using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{

    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Arabic)]
    [RyuJitX64Job, LegacyJitX64Job, MonoJob]
    public class SubImplementations
    {
        [Params(5000)]
        public int TotalBytes { get; set; }

        [Params(4)]
        public int BytesPerPixel { get; set; }

        public byte[] RawScanline { get; set; }

        public byte[] TargetBuffer { get; set; }

        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [Setup]
        public void Setup()
        {
            RawScanline = new byte[TotalBytes];
            TargetBuffer = new byte[TotalBytes];
            rng.GetBytes(RawScanline);
        }

        [Benchmark]
        public unsafe void Pointers()
        {
            int targetOffset = 0;

            fixed (byte* targetPtr = TargetBuffer)
            fixed (byte* scanlinePtr = RawScanline) {
                Buffer.MemoryCopy(scanlinePtr, targetPtr + targetOffset, RawScanline.Length, BytesPerPixel);

                unchecked {
                    // We start immediately after the first pixel--its bytes are unchanged. We only copied
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline.
                    for (var x = BytesPerPixel; x < RawScanline.Length; x++)
                        targetPtr[x + targetOffset] = (byte)(scanlinePtr[x] - scanlinePtr[x - BytesPerPixel]);
                }
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolled()
        {
            int targetOffset = 0;

            fixed (byte* target = TargetBuffer)
            fixed (byte* raw = RawScanline) {
                Buffer.MemoryCopy(raw, target + targetOffset, RawScanline.Length, BytesPerPixel);

                unchecked {
                    // We start immediately after the first pixel--its bytes are unchanged. We only copied
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline. Unroll
                    // the loop a bit, as well.
                    int x = BytesPerPixel;
                    for (; RawScanline.Length - x > 8; x += 8) {
                        target[x + targetOffset] = (byte)(raw[x] - raw[x - BytesPerPixel]);
                        target[x + 1 + targetOffset] = (byte)(raw[x + 1] - raw[x + 1 - BytesPerPixel]);
                        target[x + 2 + targetOffset] = (byte)(raw[x + 2] - raw[x + 2 - BytesPerPixel]);
                        target[x + 3 + targetOffset] = (byte)(raw[x + 3] - raw[x + 3 - BytesPerPixel]);
                        target[x + 4 + targetOffset] = (byte)(raw[x + 4] - raw[x + 4 - BytesPerPixel]);
                        target[x + 5 + targetOffset] = (byte)(raw[x + 5] - raw[x + 5 - BytesPerPixel]);
                        target[x + 6 + targetOffset] = (byte)(raw[x + 6] - raw[x + 6 - BytesPerPixel]);
                        target[x + 7 + targetOffset] = (byte)(raw[x + 7] - raw[x + 7 - BytesPerPixel]);
                    }

                    for (; x < RawScanline.Length; x++)
                        target[x + targetOffset] = (byte)(raw[x] - raw[x - BytesPerPixel]);
                }
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolledPreOffset()
        {
            int targetOffset = 0;

            fixed (byte* targetPreoffset = TargetBuffer)
            fixed (byte* raw = RawScanline) {
                byte* target = targetPreoffset + targetOffset;
                Buffer.MemoryCopy(raw, target, RawScanline.Length, BytesPerPixel);

                unchecked {
                    // We start immediately after the first pixel--its bytes are unchanged. We only copied
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline. Unroll
                    // the loop a bit, as well.
                    int x = BytesPerPixel;
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

        [Benchmark(Baseline = true)]
        public void Naive()
        {
            int targetOffset = 0;
            Buffer.BlockCopy(RawScanline, 0, TargetBuffer, targetOffset, BytesPerPixel);

            unchecked {
                // We start immediately after the first pixel--its bytes are unchanged. We only copied
                // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline.
                for (var x = BytesPerPixel; x < RawScanline.Length; x++)
                    TargetBuffer[x + targetOffset] = (byte)(RawScanline[x] - RawScanline[x - BytesPerPixel]);
            }
        }

        [Benchmark]
        public unsafe void VectorAndPointer()
        {
            int vecSize = Vector<byte>.Count, length = RawScanline.Length;
            int targetOffset = 0;

            var chunks = (int)Math.Floor((double)(length - BytesPerPixel) / vecSize);

            fixed (byte* raw = RawScanline)
            fixed (byte* target = TargetBuffer) {
                Buffer.MemoryCopy(raw, target + targetOffset, length, BytesPerPixel);

                for (int i = 0; i < chunks; i++) {
                    int src = i * vecSize, dst = src + BytesPerPixel;
                    var vec = new Vector<byte>(RawScanline, dst) - new Vector<byte>(RawScanline, src);
                    vec.CopyTo(TargetBuffer, dst + targetOffset);
                }

                int start = BytesPerPixel + (vecSize * chunks) + targetOffset;
                for (int i = start; i < length; i++)
                    target[i] = unchecked((byte)(raw[i] - raw[i - BytesPerPixel]));
            }
        }
    }
}
