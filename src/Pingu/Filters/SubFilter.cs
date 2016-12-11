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
    }
}
