using System;

namespace Pingu.Filters
{
    class UpFilter : IFilter
    {
        private static readonly Lazy<UpFilter> lazy
            = new Lazy<UpFilter>(() => new UpFilter());

        public static UpFilter Instance => lazy.Value;

        internal UpFilter() { }

        public FilterType Type => FilterType.Up;

        public unsafe void FilterInto(byte[] targetBuffer, int targetOffset, byte[] rawScanline, byte[] previousScanline, int bytesPerPixel)
        {
            // The Up filter reads the previous scanline, so if it's null, this must be the first
            // scanline and it's unfiltered.
            if (previousScanline == null) {
                Buffer.BlockCopy(rawScanline, 0, targetBuffer, targetOffset, rawScanline.Length);
                return;
            }

            fixed (byte* raw = rawScanline)
            fixed (byte* previous = previousScanline)
            fixed (byte* target = targetBuffer) {
                for (var i = 0; i < rawScanline.Length; i++)
                    target[i + targetOffset] = (byte)((raw[i] - previous[i]) % 256);
            }
        }
    }
}
