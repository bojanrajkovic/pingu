using System;
using System.Numerics;
using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class AvgImplementations
    {
        [Params(5000)]
        public int TotalBytes { get; set; }

        [Params(true, false)]
        public bool HasPreviousScanline { get; set; }

        [Params(4)]
        public int BytesPerPixel { get; set; }

        public byte[] TargetBuffer { get; set; }
        public byte[] RawScanline { get; set; }
        public byte[] PreviousScanline { get; set; }

        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [GlobalSetup]
        public void Setup()
        {
            TargetBuffer = new byte[TotalBytes];
            RawScanline = new byte[TotalBytes];
            PreviousScanline = HasPreviousScanline ? new byte[TotalBytes] : null;

            rng.GetBytes(RawScanline);

            if (HasPreviousScanline)
                rng.GetBytes(PreviousScanline);
        }

        [Benchmark(Baseline = true)]
        public unsafe void NaiveWithNullablePrevious()
        {
            int targetOffset = 0;

            int i = 0;
            for (; i < BytesPerPixel; i++)
                TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - ((PreviousScanline?[i] ?? 0)) / 2));
            for (; i < RawScanline.Length; i++)
                TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - (RawScanline[i - BytesPerPixel] + (PreviousScanline?[i] ?? 0)) / 2));
        }

        [Benchmark]
        public unsafe void NaiveWithSeparateLoops()
        {
            int targetOffset = 0;

            if (PreviousScanline == null) {
                Buffer.BlockCopy(RawScanline, 0, TargetBuffer, targetOffset, BytesPerPixel);
                for (var i = BytesPerPixel; i < RawScanline.Length; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - RawScanline[i - BytesPerPixel] / 2));
            } else {
                int i = 0;
                for (; i < BytesPerPixel; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - PreviousScanline[i] / 2));
                for (; i < RawScanline.Length; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - (RawScanline[i - BytesPerPixel] + PreviousScanline[i]) / 2));
            }
        }

        [Benchmark]
        public unsafe void Pointer()
        {
            int targetOffset = 0;

            fixed (byte* targetUnoffset = TargetBuffer)
            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline) {
                byte* target = targetUnoffset + targetOffset;

                if (previous == null) {
                    Buffer.MemoryCopy(raw, target, BytesPerPixel, BytesPerPixel);
                    for (var i = BytesPerPixel; i < RawScanline.Length; i++)
                        target[i] = unchecked((byte)(raw[i] - raw[i - BytesPerPixel] / 2));
                } else {
                    int i = 0;
                    for (; i < BytesPerPixel;  i++)
                        target[i] = unchecked((byte)(raw[i] - previous[i] / 2));
                    for (; i < RawScanline.Length; i++)
                        target[i] = unchecked((byte)(raw[i] - (raw[i - BytesPerPixel] + previous[i]) / 2));
                }
            }
        }

        [Benchmark]
        public unsafe void PointerUnrolled()
        {
            int targetOffset = 0;

            fixed (byte* targetUnoffset = TargetBuffer)
            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline) {
                byte* target = targetUnoffset + targetOffset;

                if (previous == null) {
                    Buffer.MemoryCopy(raw, target, BytesPerPixel, BytesPerPixel);
                    int i = BytesPerPixel;
                    for (; RawScanline.Length - i > 8; i += 8) {
                        target[i] = unchecked((byte)(raw[i] - raw[i - BytesPerPixel] / 2));
                        target[i + 1] = unchecked((byte)(raw[i + 1] - raw[i + 1 - BytesPerPixel] / 2));
                        target[i + 2] = unchecked((byte)(raw[i + 2] - raw[i + 2 - BytesPerPixel] / 2));
                        target[i + 3] = unchecked((byte)(raw[i + 3] - raw[i + 3 - BytesPerPixel] / 2));
                        target[i + 4] = unchecked((byte)(raw[i + 4] - raw[i + 4 - BytesPerPixel] / 2));
                        target[i + 5] = unchecked((byte)(raw[i + 5] - raw[i + 5 - BytesPerPixel] / 2));
                        target[i + 6] = unchecked((byte)(raw[i + 6] - raw[i + 6 - BytesPerPixel] / 2));
                        target[i + 7] = unchecked((byte)(raw[i + 7] - raw[i + 7 - BytesPerPixel] / 2));
                    }
                    for (; i < RawScanline.Length; i++)
                        target[i] = unchecked((byte)(raw[i] - raw[i - BytesPerPixel] / 2));
                } else {
                    int i = 0;
                    for (; i < BytesPerPixel; i++)
                        target[i] = unchecked((byte)(raw[i] - previous[i] / 2));
                    for (; RawScanline.Length - i > 8; i += 8) {
                        target[i] = unchecked((byte)(raw[i] - (raw[i - BytesPerPixel] + previous[i]) / 2));
                        target[i + 1] = unchecked((byte)(raw[i + 1] - (raw[i + 1 - BytesPerPixel] + previous[i + 1]) / 2));
                        target[i + 2] = unchecked((byte)(raw[i + 2] - (raw[i + 2 - BytesPerPixel] + previous[i + 2]) / 2));
                        target[i + 3] = unchecked((byte)(raw[i + 3] - (raw[i + 3 - BytesPerPixel] + previous[i + 3]) / 2));
                        target[i + 4] = unchecked((byte)(raw[i + 4] - (raw[i + 4 - BytesPerPixel] + previous[i + 4]) / 2));
                        target[i + 5] = unchecked((byte)(raw[i + 5] - (raw[i + 5 - BytesPerPixel] + previous[i + 5]) / 2));
                        target[i + 6] = unchecked((byte)(raw[i + 6] - (raw[i + 6 - BytesPerPixel] + previous[i + 6]) / 2));
                        target[i + 7] = unchecked((byte)(raw[i + 7] - (raw[i + 7 - BytesPerPixel] + previous[i + 7]) / 2));
                    }
                    for (; i < RawScanline.Length; i++)
                        target[i] = unchecked((byte)(raw[i] - (raw[i - BytesPerPixel] + previous[i]) / 2));
                }
            }
        }

        [Benchmark]
        public unsafe void PointerUnrolledMotion()
        {
            int targetOffset = 0;

            fixed (byte* targetUnoffset = TargetBuffer)
            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline) {
                byte* target = targetUnoffset + targetOffset;

                if (previous == null) {
                    // Basically Sub, but with the Raw(x-bpp) value divided by 2
                    Buffer.MemoryCopy(raw, target, RawScanline.Length, BytesPerPixel);

                    unchecked {
                        int x = BytesPerPixel;

                        target += BytesPerPixel;
                        byte* rawm = raw + BytesPerPixel, rawBpp = raw;

                        for (; RawScanline.Length - x > 8; x += 8) {
                            target[0] = (byte)(rawm[0] - rawBpp[0]/2);
                            target[1] = (byte)(rawm[1] - rawBpp[1]/2);
                            target[2] = (byte)(rawm[2] - rawBpp[2]/2);
                            target[3] = (byte)(rawm[3] - rawBpp[3]/2);
                            target[4] = (byte)(rawm[4] - rawBpp[4]/2);
                            target[5] = (byte)(rawm[5] - rawBpp[5]/2);
                            target[6] = (byte)(rawm[6] - rawBpp[6]/2);
                            target[7] = (byte)(rawm[7] - rawBpp[7]/2);
                            target += 8; rawm += 8; rawBpp += 8;
                        }

                        for (; x < RawScanline.Length; x++) {
                            target[0] = (byte)(rawm[0] - rawBpp[0]/2);
                            target++; rawm++; rawBpp++;
                        }
                    }
                } else {
                    int i = 0;
                    byte* rawm = raw, prev = previous, rawBpp = raw - BytesPerPixel;

                    unchecked {
                        for (; i < BytesPerPixel; i++) {
                            target[0] = (byte)(rawm[0] - prev[0] / 2);
                            target++; rawm++; prev++; rawBpp++;
                        }

                        for (; RawScanline.Length - i > 8; i += 8) {
                            target[0] = (byte)(rawm[0] - (rawBpp[0] + prev[0]) / 2);
                            target[1] = (byte)(rawm[1] - (rawBpp[1] + prev[1]) / 2);
                            target[2] = (byte)(rawm[2] - (rawBpp[2] + prev[2]) / 2);
                            target[3] = (byte)(rawm[3] - (rawBpp[3] + prev[3]) / 2);
                            target[4] = (byte)(rawm[4] - (rawBpp[4] + prev[4]) / 2);
                            target[5] = (byte)(rawm[5] - (rawBpp[5] + prev[5]) / 2);
                            target[6] = (byte)(rawm[6] - (rawBpp[6] + prev[6]) / 2);
                            target[7] = (byte)(rawm[7] - (rawBpp[7] + prev[7]) / 2);
                            target += 8; rawm += 8; prev += 8; rawBpp += 8;
                        }

                        for (; i < RawScanline.Length; i++) {
                            target[0] = (byte)(rawm[0] - (rawBpp[0] + prev[0]) / 2);
                            target++; rawm++; prev++; rawBpp++;
                        }
                    }
                }
            }
        }

        [Benchmark]
        public unsafe void SmartVectorized()
        {
            // Turns out, vectorizing this doesn't work very well, because vector division is unaccelerated. See
            // https://github.com/aklomp/sse-intrinsics-tests/blob/master/lib/mm_div_epu8.h for an implementation of
            // 8-byte division, but S.N.V lacks unpack/shift instructions, so we can't implement this now. This
            // benchmark left in for completeness sake/hope that SNV will sprout unpack/shift someday and we can
            // implement fast vector division.
            int targetOffset = 0;

            fixed (byte* targetUnoffset = TargetBuffer)
            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline) {
                byte* target = targetUnoffset + targetOffset;

                if (previous == null) {
                    Buffer.MemoryCopy(raw, target, BytesPerPixel, BytesPerPixel);

                    int vecSize = Vector<byte>.Count, length = TotalBytes;
                    var chunks = (int)((float)(length - BytesPerPixel) / vecSize);
                    var twos = new Vector<byte>(2);

                    for (int i = 0; i < chunks; i++) {
                        int src = i * vecSize, dst = src + BytesPerPixel + targetOffset;
                        var vec = new Vector<byte>(RawScanline, dst) - Vector.Divide(new Vector<byte>(RawScanline, src), twos);
                        vec.CopyTo(TargetBuffer, dst);
                    }

                    int start = BytesPerPixel + (vecSize * chunks);
                    for (int i = start; i < RawScanline.Length; i++)
                        target[i] = unchecked((byte)(raw[i] - raw[i - BytesPerPixel] / 2));
                } else {
                    int i = 0;
                    for (; i < BytesPerPixel; i++)
                        target[i] = unchecked((byte)(raw[i] - previous[i] / 2));
                    for (; RawScanline.Length - i > 8; i += 8) {
                        target[i] = unchecked((byte)(raw[i] - (raw[i - BytesPerPixel] + previous[i]) / 2));
                        target[i + 1] = unchecked((byte)(raw[i + 1] - (raw[i + 1 - BytesPerPixel] + previous[i + 1]) / 2));
                        target[i + 2] = unchecked((byte)(raw[i + 2] - (raw[i + 2 - BytesPerPixel] + previous[i + 2]) / 2));
                        target[i + 3] = unchecked((byte)(raw[i + 3] - (raw[i + 3 - BytesPerPixel] + previous[i + 3]) / 2));
                        target[i + 4] = unchecked((byte)(raw[i + 4] - (raw[i + 4 - BytesPerPixel] + previous[i + 4]) / 2));
                        target[i + 5] = unchecked((byte)(raw[i + 5] - (raw[i + 5 - BytesPerPixel] + previous[i + 5]) / 2));
                        target[i + 6] = unchecked((byte)(raw[i + 6] - (raw[i + 6 - BytesPerPixel] + previous[i + 6]) / 2));
                        target[i + 7] = unchecked((byte)(raw[i + 7] - (raw[i + 7 - BytesPerPixel] + previous[i + 7]) / 2));
                    }
                    for (; i < RawScanline.Length; i++)
                        target[i] = unchecked((byte)(raw[i] - (raw[i - BytesPerPixel] + previous[i]) / 2));
                }
            }
        }
    }
}
