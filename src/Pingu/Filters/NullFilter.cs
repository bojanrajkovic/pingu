using System;

namespace Pingu.Filters
{
    class NullFilter : IFilter
    {
        private static readonly Lazy<NullFilter> lazy
            = new Lazy<NullFilter>(() => new NullFilter());

        public static NullFilter Instance => lazy.Value;

        internal NullFilter() { }

        public byte[] Filter(byte[] scanline, byte[] previousScanline, int bytesPerPixel) => scanline;

        public byte[] ReverseFilter(byte[] scanline, byte[] previousScanline, int bytesPerPixel) => scanline;
    }
}