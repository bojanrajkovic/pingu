using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Pingu.Filters
{
    class SubFilter : IFilter
    {
        static readonly Lazy<SubFilter> Lazy
            = new Lazy<SubFilter>(() => new SubFilter());

        public static SubFilter Instance => Lazy.Value;

        SubFilter () { }

        public FilterType Type => FilterType.Sub;

        internal unsafe void UnrolledPointerFilterInto(
            byte[] targetBuffer,
            int targetOffset,
            byte[] rawScanline,
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

                    target += bytesPerPixel;
                    byte* rawm = raw + bytesPerPixel, rawBpp = raw;

                    for (; rawScanline.Length - x > 8; x += 8) {
                        target[0] = (byte)(rawm[0] - rawBpp[0]);
                        target[1] = (byte)(rawm[1] - rawBpp[1]);
                        target[2] = (byte)(rawm[2] - rawBpp[2]);
                        target[3] = (byte)(rawm[3] - rawBpp[3]);
                        target[4] = (byte)(rawm[4] - rawBpp[4]);
                        target[5] = (byte)(rawm[5] - rawBpp[5]);
                        target[6] = (byte)(rawm[6] - rawBpp[6]);
                        target[7] = (byte)(rawm[7] - rawBpp[7]);
                        target += 8; rawm += 8; rawBpp += 8;
                    }

                    for (; x < rawScanline.Length; x++) {
                        target[0] = (byte)(rawm[0] - rawBpp[0]);
                        target++; rawm++; rawBpp++;
                    }
                }
            }
        }

        internal unsafe void VectorAndPointerFilterInto(
            byte[] targetBuffer,
            int targetOffset,
            byte[] scanline,
            int bytesPerPixel)
        {
            int vecSize = Vector<byte>.Count, length = scanline.Length;
            var chunks = (int)((float)(length - bytesPerPixel) / vecSize);

            fixed (byte* dataPtr = scanline)
            fixed (byte* resultPtr = targetBuffer) {
                Buffer.MemoryCopy(dataPtr, resultPtr + targetOffset, length, bytesPerPixel);

                for (var i = 0; i < chunks; i++) {
                    int src = i * vecSize, dst = src + bytesPerPixel;
                    var vec = new Vector<byte>(scanline, dst) - new Vector<byte>(scanline, src);
                    vec.CopyTo(targetBuffer, dst + targetOffset);
                }

                var start = bytesPerPixel + vecSize * chunks;
                for (var i = start; i < length; i++)
                    resultPtr[i+targetOffset] = unchecked((byte)(dataPtr[i] - dataPtr[i - bytesPerPixel]));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FilterInto(
            byte[] targetBuffer,
            int targetOffset,
            byte[] scanline,
            byte[] previousScanline,
            int bytesPerPixel)
        {
            // This is not a performance issue--the JITter will treat a `static readonly bool` as a constant
            // and will do DCE to eliminate the wrong branch here.
            if (DefaultFilters.UseVectors)
                VectorAndPointerFilterInto(targetBuffer, targetOffset, scanline, bytesPerPixel);
            else
                UnrolledPointerFilterInto(targetBuffer, targetOffset, scanline, bytesPerPixel);
        }
    }
}
