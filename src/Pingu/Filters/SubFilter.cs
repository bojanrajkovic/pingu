using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Pingu.Filters
{
    class SubFilter : IFilter
    {
        private static readonly Lazy<SubFilter> lazy
            = new Lazy<SubFilter>(() => new SubFilter());

        public static SubFilter Instance => lazy.Value;

        internal SubFilter () { }

        public FilterType Type => FilterType.Sub;

        unsafe void UnrolledPointerFilterInto(
            byte[] targetBuffer,
            int targetOffset,
            byte[] rawScanline,
            byte[] previousScanline,
            int bytesPerPixel)
        {
            fixed (byte* targetPreoffset = targetBuffer)
            fixed (byte* raw = rawScanline) {
                byte* target = targetPreoffset + targetOffset;
                Buffer.MemoryCopy(raw, target, rawScanline.Length, bytesPerPixel);

                unchecked {
                    // We start immediately after the first pixel--its bytes are unchanged. We only copied
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline. Unroll
                    // the loop a bit, as well.
                    int x = bytesPerPixel;
                    for (; rawScanline.Length - x > 8; x += 8) {
                        target[x] = (byte)(raw[x] - raw[x - bytesPerPixel]);
                        target[x + 1] = (byte)(raw[x + 1] - raw[x + 1 - bytesPerPixel]);
                        target[x + 2] = (byte)(raw[x + 2] - raw[x + 2 - bytesPerPixel]);
                        target[x + 3] = (byte)(raw[x + 3] - raw[x + 3 - bytesPerPixel]);
                        target[x + 4] = (byte)(raw[x + 4] - raw[x + 4 - bytesPerPixel]);
                        target[x + 5] = (byte)(raw[x + 5] - raw[x + 5 - bytesPerPixel]);
                        target[x + 6] = (byte)(raw[x + 6] - raw[x + 6 - bytesPerPixel]);
                        target[x + 7] = (byte)(raw[x + 7] - raw[x + 7 - bytesPerPixel]);
                    }

                    for (; x < rawScanline.Length; x++)
                        target[x] = (byte)(raw[x] - raw[x - bytesPerPixel]);
                }
            }
        }

        unsafe void VectorAndPointerFilterInto(
            byte[] targetBuffer,
            int targetOffset,
            byte[] scanline,
            byte[] previousScanline,
            int bytesPerPixel)
        {
            int vecSize = Vector<byte>.Count, length = scanline.Length;
            var chunks = (int)Math.Floor((double)(length - bytesPerPixel) / vecSize);

            fixed (byte* dataPtr = scanline)
            fixed (byte* resultPtr = targetBuffer) {
                Buffer.MemoryCopy(dataPtr, resultPtr + targetOffset, length, bytesPerPixel);

                for (int i = 0; i < chunks; i++) {
                    int src = i * vecSize, dst = src + bytesPerPixel;
                    var vec = new Vector<byte>(scanline, dst) - new Vector<byte>(scanline, src);
                    vec.CopyTo(targetBuffer, dst + targetOffset);
                }

                int start = bytesPerPixel + (vecSize * chunks);
                for (int i = start; i < length; i++)
                    resultPtr[i+targetOffset] = unchecked((byte)(dataPtr[i] - dataPtr[i - bytesPerPixel]));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void FilterInto(
            byte[] targetBuffer,
            int targetOffset,
            byte[] scanline,
            byte[] previousScanline,
            int bytesPerPixel)
        {
            // This is not a performance issue--the JITter will treat a `static readonly bool` as a constant
            // and will do DCE to eliminate the wrong branch here.
            if (DefaultFilters.UseVectors)
                VectorAndPointerFilterInto(targetBuffer, targetOffset, scanline, previousScanline, bytesPerPixel);
            else
                UnrolledPointerFilterInto(targetBuffer, targetOffset, scanline, previousScanline, bytesPerPixel);
        }
    }
}
