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

        static readonly bool UseVectors = true;

        static SubFilter ()
        {
            // If we're on Mono, don't use vectors.
            if (System.Type.GetType("Mono.Runtime") != null)
                UseVectors = false;

            // If Vectors aren't hardware accelerated, use pointers.
            if (!Vector.IsHardwareAccelerated)
                UseVectors = false;
        }

        internal SubFilter () { }

        public FilterType Type => FilterType.Sub;

        unsafe void PointersOnly(
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
                    // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline. Unroll
                    // the loop a bit, as well.
                    for (var x = bytesPerPixel; x < scanline.Length - 8; x += 8) {
                        targetPtr[x + targetOffset] = (byte)((scanlinePtr[x] - scanlinePtr[x - bytesPerPixel]) % 256);
                        targetPtr[x + 1 + targetOffset] = (byte)((scanlinePtr[x + 1] - scanlinePtr[x + 1 - bytesPerPixel]) % 256);
                        targetPtr[x + 2 + targetOffset] = (byte)((scanlinePtr[x + 2] - scanlinePtr[x + 2 - bytesPerPixel]) % 256);
                        targetPtr[x + 3 + targetOffset] = (byte)((scanlinePtr[x + 3] - scanlinePtr[x + 3 - bytesPerPixel]) % 256);
                        targetPtr[x + 4 + targetOffset] = (byte)((scanlinePtr[x + 4] - scanlinePtr[x + 4 - bytesPerPixel]) % 256);
                        targetPtr[x + 5 + targetOffset] = (byte)((scanlinePtr[x + 5] - scanlinePtr[x + 5 - bytesPerPixel]) % 256);
                        targetPtr[x + 6 + targetOffset] = (byte)((scanlinePtr[x + 6] - scanlinePtr[x + 6 - bytesPerPixel]) % 256);
                        targetPtr[x + 7 + targetOffset] = (byte)((scanlinePtr[x + 7] - scanlinePtr[x + 7 - bytesPerPixel]) % 256);
                    }

                    for (var x = scanline.Length - 8; x < scanline.Length; x++)
                        targetPtr[x + targetOffset] = (byte)((scanlinePtr[x] - scanlinePtr[x - bytesPerPixel]) % 256);
                }
            }
        }

        unsafe void VectorAndPointer(
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
            if (UseVectors)
                VectorAndPointer(targetBuffer, targetOffset, scanline, previousScanline, bytesPerPixel);
            else
                PointersOnly(targetBuffer, targetOffset, scanline, previousScanline, bytesPerPixel);
        }
    }
}
