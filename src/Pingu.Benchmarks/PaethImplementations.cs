using System;
using System.Numerics;
using System.Runtime.CompilerServices;
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
    public class PaethImplementations
    {
        [Params(5000)]
        public int TotalBytes { get; set; }

        public bool HasPreviousScanline { get; set; } = true;

        [Params(4)]
        public int BytesPerPixel { get; set; }

        public byte[] TargetBuffer { get; set; }
        public byte[] RawScanline { get; set; }
        public byte[] PreviousScanline { get; set; }
        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [Setup]
        public void Setup()
        {
            TargetBuffer = new byte[TotalBytes];
            RawScanline = new byte[TotalBytes];
            PreviousScanline = HasPreviousScanline ? new byte[TotalBytes] : null;

            rng.GetBytes(RawScanline);

            if (HasPreviousScanline)
                rng.GetBytes(PreviousScanline);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int Abs(int value)
        {
            int temp = value >> 31;
            value ^= temp;
            value += temp & 1;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte PaethFastAbs(byte a, byte b, byte c)
        {
            int p = a + b - c,
                pa = Abs(p - a),
                pb = Abs(p - b),
                pc = Abs(p - c);

            return pa <= pb && pa <= pc ? a : (pb <= pc ? b : c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte PaethVec(byte a, byte b, byte c)
        {
            int p = a + b - c;
            var resVec = Vector3.Abs(new Vector3(p, p, p) - new Vector3(a, b, c));
            return resVec.X <= resVec.Y && resVec.X <= resVec.Z ? a : (resVec.Y <= resVec.Z ? b : c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte Paeth(byte a, byte b, byte c)
        {
            int p = a + b - c,
                pa = Math.Abs(p - a),
                pb = Math.Abs(p - b),
                pc = Math.Abs(p - c);

            return pa <= pb && pa <= pc ? a : (pb <= pc ? b : c);
        }

        // Skip the nullable access benchmark that we did in AvgImplementations, it's never going to be
        // as good as separate loops.

        [Benchmark(Baseline = true)]
        public void NaiveWithMathAbs()
        {
            // Paeth(x) = Raw(x) - PaethPredictor(Raw(x-bpp), Prior(x), Prior(x - bpp))
            int targetOffset = 0;

            if (PreviousScanline == null) {
                // First bpp bytes, a and c values passed to the Paeth predictor are 0. If previous scanline is null,
                // then the first 4 bytes just match the first 4 raw, as Paeth(0,0,0) is 0.
                Buffer.BlockCopy(RawScanline, 0, TargetBuffer, targetOffset, BytesPerPixel);
                // For the remaining bytes, we have a value for a, but not b and c, as there is no prior scanline.
                // Paeth(a,0,0) is a, so Paeth is just the sub filter in this case.
                for (var i = BytesPerPixel; i < RawScanline.Length; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - RawScanline[i - BytesPerPixel]));
            } else {
                int i = 0;
                // First BPP bytes, a and c are 0, but b has a value. Paeth(0,b,0) == b, so treat it as such.
                for (; i < BytesPerPixel; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - PreviousScanline[i]));
                // The remaining bytes, a and c have values!
                for (; i < RawScanline.Length; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - Paeth(
                        RawScanline[i - BytesPerPixel],
                        PreviousScanline[i],
                        PreviousScanline[i - BytesPerPixel]
                    )));
            }
        }

        [Benchmark]
        public unsafe void UnrolledWithMathAbs()
        {
            int targetOffset = 0;

            fixed (byte* targetPreoffset = TargetBuffer)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* raw = RawScanline) {
                byte* target = targetPreoffset + targetOffset;
                if (previous == null) {
                    // If the previous scanline is null, Paeth == Sub
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
                } else {
                    int i = 0;
                    unchecked {
                        // The first bpp bytes, Paeth = Up
                        for (; i < BytesPerPixel; i++)
                            target[i] = (byte)(raw[i] - previous[i]);

                        // The remaining bytes, a and c have values!
                        for (; RawScanline.Length - i > 8; i += 8) {
                            target[i] = (byte)(raw[i] - Paeth(raw[i - BytesPerPixel], previous[i], previous[i - BytesPerPixel]));
                            target[i + 1] = (byte)(raw[i + 1] - Paeth(raw[i + 1 - BytesPerPixel], previous[i + 1], previous[i + 1 - BytesPerPixel]));
                            target[i + 2] = (byte)(raw[i + 2] - Paeth(raw[i + 2 - BytesPerPixel], previous[i + 2], previous[i + 2 - BytesPerPixel]));
                            target[i + 3] = (byte)(raw[i + 3] - Paeth(raw[i + 3 - BytesPerPixel], previous[i + 3], previous[i + 3 - BytesPerPixel]));
                            target[i + 4] = (byte)(raw[i + 4] - Paeth(raw[i + 4 - BytesPerPixel], previous[i + 4], previous[i + 4 - BytesPerPixel]));
                            target[i + 5] = (byte)(raw[i + 5] - Paeth(raw[i + 5 - BytesPerPixel], previous[i + 5], previous[i + 5 - BytesPerPixel]));
                            target[i + 6] = (byte)(raw[i + 6] - Paeth(raw[i + 6 - BytesPerPixel], previous[i + 6], previous[i + 6 - BytesPerPixel]));
                            target[i + 7] = (byte)(raw[i + 7] - Paeth(raw[i + 7 - BytesPerPixel], previous[i + 7], previous[i + 7 - BytesPerPixel]));
                        }

                        for (; i < RawScanline.Length; i++)
                            target[i] = (byte)(raw[i] - Paeth(raw[i - BytesPerPixel], previous[i], previous[i - BytesPerPixel]));
                    }
                }
            }
        }

        [Benchmark]
        public void NaiveWithFastAbs()
        {
            // Paeth(x) = Raw(x) - PaethPredictor(Raw(x-bpp), Prior(x), Prior(x - bpp))
            int targetOffset = 0;

            if (PreviousScanline == null) {
                // First bpp bytes, a and c values passed to the Paeth predictor are 0. If previous scanline is null,
                // then the first 4 bytes just match the first 4 raw, as Paeth(0,0,0) is 0.
                Buffer.BlockCopy(RawScanline, 0, TargetBuffer, targetOffset, BytesPerPixel);
                // For the remaining bytes, we have a value for a, but not b and c, as there is no prior scanline.
                // Paeth(a,0,0) is a, so Paeth is just the sub filter in this case.
                for (var i = BytesPerPixel; i < RawScanline.Length; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - RawScanline[i - BytesPerPixel]));
            } else {
                int i = 0;
                // First BPP bytes, a and c are 0, but b has a value. Paeth(0,b,0) == b, so treat it as such.
                for (; i < BytesPerPixel; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - PreviousScanline[i]));
                // The remaining bytes, a and c have values!
                for (; i < RawScanline.Length; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - PaethFastAbs(
                        RawScanline[i - BytesPerPixel],
                        PreviousScanline[i],
                        PreviousScanline[i - BytesPerPixel]
                    )));
            }
        }

        [Benchmark]
        public unsafe void UnrolledWithFastAbs()
        {
            int targetOffset = 0;

            fixed (byte* targetPreoffset = TargetBuffer)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* raw = RawScanline) {
                byte* target = targetPreoffset + targetOffset;
                if (previous == null) {
                    // If the previous scanline is null, Paeth == Sub
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
                } else {
                    int i = 0;
                    unchecked {
                        // The first bpp bytes, Paeth = Up
                        for (; i < BytesPerPixel; i++)
                            target[i] = (byte)(raw[i] - previous[i]);

                        // The remaining bytes, a and c have values!
                        for (; RawScanline.Length - i > 8; i += 8) {
                            target[i] = (byte)(raw[i] - PaethFastAbs(raw[i - BytesPerPixel], previous[i], previous[i - BytesPerPixel]));
                            target[i + 1] = (byte)(raw[i + 1] - PaethFastAbs(raw[i + 1 - BytesPerPixel], previous[i + 1], previous[i + 1 - BytesPerPixel]));
                            target[i + 2] = (byte)(raw[i + 2] - PaethFastAbs(raw[i + 2 - BytesPerPixel], previous[i + 2], previous[i + 2 - BytesPerPixel]));
                            target[i + 3] = (byte)(raw[i + 3] - PaethFastAbs(raw[i + 3 - BytesPerPixel], previous[i + 3], previous[i + 3 - BytesPerPixel]));
                            target[i + 4] = (byte)(raw[i + 4] - PaethFastAbs(raw[i + 4 - BytesPerPixel], previous[i + 4], previous[i + 4 - BytesPerPixel]));
                            target[i + 5] = (byte)(raw[i + 5] - PaethFastAbs(raw[i + 5 - BytesPerPixel], previous[i + 5], previous[i + 5 - BytesPerPixel]));
                            target[i + 6] = (byte)(raw[i + 6] - PaethFastAbs(raw[i + 6 - BytesPerPixel], previous[i + 6], previous[i + 6 - BytesPerPixel]));
                            target[i + 7] = (byte)(raw[i + 7] - PaethFastAbs(raw[i + 7 - BytesPerPixel], previous[i + 7], previous[i + 7 - BytesPerPixel]));
                        }

                        for (; i < RawScanline.Length; i++)
                            target[i] = (byte)(raw[i] - PaethFastAbs(raw[i - BytesPerPixel], previous[i], previous[i - BytesPerPixel]));
                    }
                }
            }
        }

        [Benchmark]
        public unsafe void UnrolledWithFastAbsAndMovingPointers()
        {
            int targetOffset = 0;

            fixed (byte* targetPreoffset = TargetBuffer)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* raw = RawScanline) {
                byte* target = targetPreoffset + targetOffset;
                if (previous == null) {
                    // If the previous scanline is null, Paeth == Sub
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
                } else {
                    int i = 0;
                    unchecked {
                        byte* tgt = target, rawm = raw, prev = previous,
                              rawBpp = raw - BytesPerPixel, prevBpp = previous - BytesPerPixel;

                        // The first bpp bytes, Paeth = Up
                        for (; i < BytesPerPixel; i++) {
                            tgt[0] = (byte)(rawm[0] - prev[0]);
                            tgt++; rawm++; prev++; rawBpp++; prevBpp++;
                        }

                        // The remaining bytes, a and c have values!
                        for (; RawScanline.Length - i > 8; i += 8) {
                            tgt[0] = (byte)(rawm[0] - PaethFastAbs(rawBpp[0], prev[0], prevBpp[0]));
                            tgt[1] = (byte)(rawm[1] - PaethFastAbs(rawBpp[1], prev[1], prevBpp[1]));
                            tgt[2] = (byte)(rawm[2] - PaethFastAbs(rawBpp[2], prev[2], prevBpp[2]));
                            tgt[3] = (byte)(rawm[3] - PaethFastAbs(rawBpp[3], prev[3], prevBpp[3]));
                            tgt[4] = (byte)(rawm[4] - PaethFastAbs(rawBpp[4], prev[4], prevBpp[4]));
                            tgt[5] = (byte)(rawm[5] - PaethFastAbs(rawBpp[5], prev[5], prevBpp[5]));
                            tgt[6] = (byte)(rawm[6] - PaethFastAbs(rawBpp[6], prev[6], prevBpp[6]));
                            tgt[7] = (byte)(rawm[7] - PaethFastAbs(rawBpp[7], prev[7], prevBpp[7]));
                            tgt += 8; rawm += 8; prev += 8; rawBpp += 8; prevBpp += 8;
                        }

                        for (; i < RawScanline.Length; i++) {
                            tgt[0] = (byte)(rawm[0] - PaethFastAbs(rawBpp[0], prev[0], prevBpp[0]));
                            tgt++; rawm++; prev++; rawBpp++; prevBpp++;
                        }
                    }
                }
            }
        }

        [Benchmark]
        public void NaiveWithVecAbs()
        {
            // Paeth(x) = Raw(x) - PaethPredictor(Raw(x-bpp), Prior(x), Prior(x - bpp))
            int targetOffset = 0;

            if (PreviousScanline == null) {
                // First bpp bytes, a and c values passed to the Paeth predictor are 0. If previous scanline is null,
                // then the first 4 bytes just match the first 4 raw, as Paeth(0,0,0) is 0.
                Buffer.BlockCopy(RawScanline, 0, TargetBuffer, targetOffset, BytesPerPixel);
                // For the remaining bytes, we have a value for a, but not b and c, as there is no prior scanline.
                // Paeth(a,0,0) is a, so Paeth is just the sub filter in this case.
                for (var i = BytesPerPixel; i < RawScanline.Length; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - RawScanline[i - BytesPerPixel]));
            } else {
                int i = 0;
                // First BPP bytes, a and c are 0, but b has a value. Paeth(0,b,0) == b, so treat it as such.
                for (; i < BytesPerPixel; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - PreviousScanline[i]));
                // The remaining bytes, a and c have values!
                for (; i < RawScanline.Length; i++)
                    TargetBuffer[i + targetOffset] = unchecked((byte)(RawScanline[i] - PaethVec(
                        RawScanline[i - BytesPerPixel],
                        PreviousScanline[i],
                        PreviousScanline[i - BytesPerPixel]
                    )));
            }
        }

        [Benchmark]
        public unsafe void UnrolledWithVecAbs()
        {
            int targetOffset = 0;

            fixed (byte* targetPreoffset = TargetBuffer)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* raw = RawScanline) {
                byte* target = targetPreoffset + targetOffset;
                if (previous == null) {
                    // If the previous scanline is null, Paeth == Sub
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
                } else {
                    int i = 0;
                    unchecked {
                        // The first bpp bytes, Paeth = Up
                        for (; i < BytesPerPixel; i++)
                            target[i] = (byte)(raw[i] - previous[i]);

                        // The remaining bytes, a and c have values!
                        for (; RawScanline.Length - i > 8; i += 8) {
                            target[i] = (byte)(raw[i] - PaethVec(raw[i - BytesPerPixel], previous[i], previous[i - BytesPerPixel]));
                            target[i + 1] = (byte)(raw[i + 1] - PaethVec(raw[i + 1 - BytesPerPixel], previous[i + 1], previous[i + 1 - BytesPerPixel]));
                            target[i + 2] = (byte)(raw[i + 2] - PaethVec(raw[i + 2 - BytesPerPixel], previous[i + 2], previous[i + 2 - BytesPerPixel]));
                            target[i + 3] = (byte)(raw[i + 3] - PaethVec(raw[i + 3 - BytesPerPixel], previous[i + 3], previous[i + 3 - BytesPerPixel]));
                            target[i + 4] = (byte)(raw[i + 4] - PaethVec(raw[i + 4 - BytesPerPixel], previous[i + 4], previous[i + 4 - BytesPerPixel]));
                            target[i + 5] = (byte)(raw[i + 5] - PaethVec(raw[i + 5 - BytesPerPixel], previous[i + 5], previous[i + 5 - BytesPerPixel]));
                            target[i + 6] = (byte)(raw[i + 6] - PaethVec(raw[i + 6 - BytesPerPixel], previous[i + 6], previous[i + 6 - BytesPerPixel]));
                            target[i + 7] = (byte)(raw[i + 7] - PaethVec(raw[i + 7 - BytesPerPixel], previous[i + 7], previous[i + 7 - BytesPerPixel]));
                        }

                        for (; i < RawScanline.Length; i++)
                            target[i] = (byte)(raw[i] - PaethVec(raw[i - BytesPerPixel], previous[i], previous[i - BytesPerPixel]));
                    }
                }
            }
        }
    }
}