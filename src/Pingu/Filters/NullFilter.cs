using System;

namespace Pingu.Filters
{
    class NullFilter : IFilter
    {
        private static readonly Lazy<NullFilter> lazy
            = new Lazy<NullFilter>(() => new NullFilter());

        public static NullFilter Instance => lazy.Value;

        internal NullFilter() { }

        public byte[] ReverseFilter(byte[] scanline, byte[] previousScanline, int bytesPerPixel) => scanline;

        public void FilterInto(byte[] targetBuffer, int targetOffset, byte[] rawScanline, byte[] previousScanline, int bytesPerPixel) =>
            Buffer.BlockCopy(rawScanline, 0, targetBuffer, targetOffset, rawScanline.Length);
    }
}