namespace Pingu.Filters
{
    interface IFilter
    {
        FilterType Type { get; }
        void FilterInto(byte[] targetBuffer, int targetOffset, byte[] rawScanline, byte[] previousScanline, int bytesPerPixel);
    }
}
