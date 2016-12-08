using System;

namespace Pingu.Filters
{
    class SubFilter : IFilter
    {
        private static readonly Lazy<SubFilter> lazy
            = new Lazy<SubFilter>(() => new SubFilter());

        public static SubFilter Instance => lazy.Value;

        internal SubFilter () { }

        public unsafe void FilterInto(
            byte[] targetBuffer,
            int targetOffset,
            byte[] scanline,
            byte[] previousScanline,
            int bytesPerPixel)
        {
            fixed (byte* targetPtr = targetBuffer)
            fixed (byte* scanlinePtr = scanline) {
                Buffer.MemoryCopy(scanlinePtr, targetPtr + targetOffset, scanline.Length, bytesPerPixel);

                unchecked {
                    // We start immediately after the first pixel--its bytes are unchanged. We only copied
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline.
                    for (var x = bytesPerPixel; x < scanline.Length; x++)
                        targetPtr[x+targetOffset] = (byte)((scanlinePtr[x] - scanlinePtr[x - bytesPerPixel]) % 256);
                }
            }
        }

        public unsafe byte[] ReverseFilter(byte[] filteredScanline, byte[] previousScanline, int bytesPerPixel)
        {
            byte[] rawScanline = new byte[filteredScanline.Length];

            fixed (byte* rawPtr = rawScanline)
            fixed (byte* scanlinePtr = filteredScanline) {
                Buffer.MemoryCopy(scanlinePtr, rawPtr, filteredScanline.Length, bytesPerPixel);

                unchecked {
                    // As in the filter, the first pixel's unchanged, so start at the 2nd.
                    for (var x = bytesPerPixel; x < rawScanline.Length; x++)
                        rawPtr[x] = (byte)((scanlinePtr[x] + scanlinePtr[x - bytesPerPixel]) % 256);
                }
            }

            return rawScanline;
        }
    }
}
