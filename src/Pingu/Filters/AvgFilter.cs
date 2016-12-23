using System;

namespace Pingu.Filters
{
    class AvgFilter : IFilter
    {
        private static readonly Lazy<AvgFilter> lazy
            = new Lazy<AvgFilter>(() => new AvgFilter());

        public static AvgFilter Instance => lazy.Value;

        internal AvgFilter() { }

        public FilterType Type => FilterType.Average;

        public unsafe void FilterInto(
            byte[] targetBuffer, 
            int targetOffset, 
            byte[] rawScanline, 
            byte[] previousScanline, 
            int bytesPerPixel)
        {
            fixed (byte* targetUnoffset = targetBuffer)
            fixed (byte* raw = rawScanline)
            fixed (byte* previous = previousScanline) {
                byte* target = targetUnoffset + targetOffset;

                if (previous == null) {
                    Buffer.MemoryCopy(raw, target, bytesPerPixel, bytesPerPixel);
                    int i = bytesPerPixel;
                    for (; rawScanline.Length - i > 8; i += 8) {
                        target[i] = unchecked((byte)(raw[i] - raw[i - bytesPerPixel] / 2));
                        target[i + 1] = unchecked((byte)(raw[i + 1] - raw[i + 1 - bytesPerPixel] / 2));
                        target[i + 2] = unchecked((byte)(raw[i + 2] - raw[i + 2 - bytesPerPixel] / 2));
                        target[i + 3] = unchecked((byte)(raw[i + 3] - raw[i + 3 - bytesPerPixel] / 2));
                        target[i + 4] = unchecked((byte)(raw[i + 4] - raw[i + 4 - bytesPerPixel] / 2));
                        target[i + 5] = unchecked((byte)(raw[i + 5] - raw[i + 5 - bytesPerPixel] / 2));
                        target[i + 6] = unchecked((byte)(raw[i + 6] - raw[i + 6 - bytesPerPixel] / 2));
                        target[i + 7] = unchecked((byte)(raw[i + 7] - raw[i + 7 - bytesPerPixel] / 2));
                    }
                    for (; i < rawScanline.Length; i++)
                        target[i] = unchecked((byte)(raw[i] - raw[i - bytesPerPixel] / 2));
                } else {
                    int i = 0;
                    for (; i < bytesPerPixel; i++)
                        target[i] = unchecked((byte)(raw[i] - previous[i] / 2));
                    for (; rawScanline.Length - i > 8; i += 8) {
                        target[i] = unchecked((byte)(raw[i] - (raw[i - bytesPerPixel] + previous[i]) / 2));
                        target[i + 1] = unchecked((byte)(raw[i + 1] - (raw[i + 1 - bytesPerPixel] + previous[i + 1]) / 2));
                        target[i + 2] = unchecked((byte)(raw[i + 2] - (raw[i + 2 - bytesPerPixel] + previous[i + 2]) / 2));
                        target[i + 3] = unchecked((byte)(raw[i + 3] - (raw[i + 3 - bytesPerPixel] + previous[i + 3]) / 2));
                        target[i + 4] = unchecked((byte)(raw[i + 4] - (raw[i + 4 - bytesPerPixel] + previous[i + 4]) / 2));
                        target[i + 5] = unchecked((byte)(raw[i + 5] - (raw[i + 5 - bytesPerPixel] + previous[i + 5]) / 2));
                        target[i + 6] = unchecked((byte)(raw[i + 6] - (raw[i + 6 - bytesPerPixel] + previous[i + 6]) / 2));
                        target[i + 7] = unchecked((byte)(raw[i + 7] - (raw[i + 7 - bytesPerPixel] + previous[i + 7]) / 2));
                    }
                    for (; i < rawScanline.Length; i++)
                        target[i] = unchecked((byte)(raw[i] - (raw[i - bytesPerPixel] + previous[i]) / 2));
                }
            }
        }
    }
}
