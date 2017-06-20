using System;
using System.Runtime.CompilerServices;

using static Pingu.Math;

namespace Pingu.Filters
{
    class PaethFilter : IFilter
    {
        static readonly Lazy<PaethFilter> Lazy
            = new Lazy<PaethFilter>(() => new PaethFilter());

        public static PaethFilter Instance => Lazy.Value;

        PaethFilter() { }

        public FilterType Type => FilterType.Paeth;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte PaethFastAbs(byte a, byte b, byte c)
        {
            int pc = c, pa = b - pc, pb = a - pc;
            pc = Abs(pa + pb);
            pa = Abs(pa);
            pb = Abs(pb);

            return pa <= pb && pa <= pc ? a : (pb <= pc ? b : c);
        }

        public unsafe void FilterInto(byte[] targetBuffer, int targetOffset, byte[] rawScanline, byte[] previousScanline, int bytesPerPixel)
        {
            fixed (byte* targetPreoffset = targetBuffer)
            fixed (byte* previous = previousScanline)
            fixed (byte* raw = rawScanline) {
                var target = targetPreoffset + targetOffset;
                if (previous == null) {
                    // When the previous scanline is null, Paeth becomes Sub. Delegate to our very fast Sub implementation,
                    // which is either vectorized or unrolled.
                    SubFilter.Instance.FilterInto(targetBuffer, targetOffset, rawScanline, previousScanline, bytesPerPixel);
                } else {
                    var i = 0;
                    unchecked {
                        byte* tgt = target, rawm = raw, prev = previous,
                              rawBpp = raw - bytesPerPixel, prevBpp = previous - bytesPerPixel;

                        // The first bpp bytes, Paeth = Up
                        for (; i < bytesPerPixel; i++) {
                            tgt[0] = (byte)(rawm[0] - prev[0]);
                            tgt++; rawm++; prev++; rawBpp++; prevBpp++;
                        }

                        // The remaining bytes, a and c have values!
                        for (; rawScanline.Length - i > 8; i += 8) {
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

                        for (; i < rawScanline.Length; i++) {
                            tgt[0] = (byte)(rawm[0] - PaethFastAbs(rawBpp[0], prev[0], prevBpp[0]));
                            tgt++; rawm++; prev++; rawBpp++; prevBpp++;
                        }
                    }
                }
            }
        }
    }
}
