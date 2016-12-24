using System;
using System.Runtime.CompilerServices;

namespace Pingu.Filters
{
    class PaethFilter : IFilter
    {
        private static readonly Lazy<PaethFilter> lazy
            = new Lazy<PaethFilter>(() => new PaethFilter());

        public static PaethFilter Instance => lazy.Value;

        internal PaethFilter() { }

        public FilterType Type => FilterType.Paeth;

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

        public unsafe void FilterInto(byte[] targetBuffer, int targetOffset, byte[] rawScanline, byte[] previousScanline, int bytesPerPixel)
        {
            fixed (byte* targetPreoffset = targetBuffer)
            fixed (byte* previous = previousScanline)
            fixed (byte* raw = rawScanline) {
                byte* target = targetPreoffset + targetOffset;
                if (previous == null) {
                    // When the previous scanline is null, Paeth becomes Sub. Delegate to our very fast Sub implementation,
                    // which is either vectorized or unrolled.
                    SubFilter.Instance.FilterInto(targetBuffer, targetOffset, rawScanline, previousScanline, bytesPerPixel);
                } else {
                    int i = 0;
                    unchecked {
                        // The first bpp bytes, Paeth = Up
                        for (; i < bytesPerPixel; i++)
                            target[i] = (byte)(raw[i] - previous[i]);

                        // The remaining bytes, a and c have values!
                        for (; rawScanline.Length - i > 8; i += 8) {
                            target[i] = (byte)(raw[i] - PaethFastAbs(raw[i - bytesPerPixel], previous[i], previous[i - bytesPerPixel]));
                            target[i + 1] = (byte)(raw[i + 1] - PaethFastAbs(raw[i + 1 - bytesPerPixel], previous[i + 1], previous[i + 1 - bytesPerPixel]));
                            target[i + 2] = (byte)(raw[i + 2] - PaethFastAbs(raw[i + 2 - bytesPerPixel], previous[i + 2], previous[i + 2 - bytesPerPixel]));
                            target[i + 3] = (byte)(raw[i + 3] - PaethFastAbs(raw[i + 3 - bytesPerPixel], previous[i + 3], previous[i + 3 - bytesPerPixel]));
                            target[i + 4] = (byte)(raw[i + 4] - PaethFastAbs(raw[i + 4 - bytesPerPixel], previous[i + 4], previous[i + 4 - bytesPerPixel]));
                            target[i + 5] = (byte)(raw[i + 5] - PaethFastAbs(raw[i + 5 - bytesPerPixel], previous[i + 5], previous[i + 5 - bytesPerPixel]));
                            target[i + 6] = (byte)(raw[i + 6] - PaethFastAbs(raw[i + 6 - bytesPerPixel], previous[i + 6], previous[i + 6 - bytesPerPixel]));
                            target[i + 7] = (byte)(raw[i + 7] - PaethFastAbs(raw[i + 7 - bytesPerPixel], previous[i + 7], previous[i + 7 - bytesPerPixel]));
                        }

                        for (; i < rawScanline.Length; i++)
                            target[i] = (byte)(raw[i] - PaethFastAbs(raw[i - bytesPerPixel], previous[i], previous[i - bytesPerPixel]));
                    }
                }
            }
        }
    }
}
