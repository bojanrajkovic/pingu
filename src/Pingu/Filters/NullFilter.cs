using System;

namespace Pingu.Filters
{
    class NullFilter : IFilter
    {
        static readonly Lazy<NullFilter> Lazy
            = new Lazy<NullFilter>(() => new NullFilter());

        public static NullFilter Instance => Lazy.Value;

        NullFilter() { }

        public FilterType Type => FilterType.None;

        public void FilterInto(byte[] targetBuffer, int targetOffset, byte[] rawScanline, byte[] previousScanline, int bytesPerPixel) =>
            Buffer.BlockCopy(rawScanline, 0, targetBuffer, targetOffset, rawScanline.Length);
    }
}