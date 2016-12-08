namespace Pingu.Filters
{
    interface IFilter
    {
        byte[] Filter(byte[] scanline, byte[] previousScanline, int bytesPerPixel);
        byte[] ReverseFilter(byte[] scanline, byte[] previousScanline, int bytesPerPixel);
    }
}
