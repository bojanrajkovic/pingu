using System;
using System.Numerics;

namespace Pingu.Filters
{
    class UpFilter : IFilter
    {
        private static readonly Lazy<UpFilter> lazy
            = new Lazy<UpFilter>(() => new UpFilter());

        public static UpFilter Instance => lazy.Value;

        internal UpFilter() { }

        public FilterType Type => FilterType.Up;

        public unsafe void UnrolledPointerFilterInto(
            byte[] targetBuffer,
            int targetOffset,
            byte[] rawScanline,
            byte[] previousScanline)
        {
            fixed (byte* raw = rawScanline)
            fixed (byte* previous = previousScanline)
            fixed (byte* targetUnoffset = targetBuffer) {
                byte* target = targetUnoffset + targetOffset, rawm = raw, prev = previous;
                int i = 0;

                for (; rawScanline.Length - i > 8; i += 8) {
                    target[0] = (byte)(rawm[0] - prev[0]);
                    target[1] = (byte)(rawm[1] - prev[1]);
                    target[2] = (byte)(rawm[2] - prev[2]);
                    target[3] = (byte)(rawm[3] - prev[3]);
                    target[4] = (byte)(rawm[4] - prev[4]);
                    target[5] = (byte)(rawm[5] - prev[5]);
                    target[6] = (byte)(rawm[6] - prev[6]);
                    target[7] = (byte)(rawm[7] - prev[7]);
                    target += 8; rawm += 8; prev += 8;
                }

                for (; i < rawScanline.Length; i++) {
                    target[0] = (byte)(rawm[0] - prev[0]);
                    target++; rawm++; prev++;
                }
            }
        }

        public unsafe void VectorAndPointerFilterInto(
            byte[] targetBuffer,
            int targetOffset,
            byte[] rawScanline,
            byte[] previousScanline)
        {
            int vecSize = Vector<byte>.Count, length = rawScanline.Length;

            var chunks = (int)Math.Floor((double)(length / vecSize));

            fixed (byte* rawPtr = rawScanline)
            fixed (byte* prevPtr = previousScanline)
            fixed (byte* targetPtr = targetBuffer) {
                for (int i = 0; i < chunks; i++) {
                    int src = i * vecSize;
                    var vec = (new Vector<byte>(rawScanline, src) - new Vector<byte>(previousScanline, src));
                    vec.CopyTo(targetBuffer, src + targetOffset);
                }

                int start = vecSize * chunks + targetOffset;
                for (int i = start; i < length; i++)
                    targetPtr[i] = unchecked((byte)(rawPtr[i] - prevPtr[i]));
            }
        }

        public unsafe void FilterInto(byte[] targetBuffer, int targetOffset, byte[] rawScanline, byte[] previousScanline, int bytesPerPixel)
        {
            // The Up filter reads the previous scanline, so if it's null, this must be the first
            // scanline and it's unfiltered.
            if (previousScanline == null) {
                Buffer.BlockCopy(rawScanline, 0, targetBuffer, targetOffset, rawScanline.Length);
                return;
            }

            if (DefaultFilters.UseVectors)
                VectorAndPointerFilterInto(targetBuffer, targetOffset, rawScanline, previousScanline);
            else
                UnrolledPointerFilterInto(targetBuffer, targetOffset, rawScanline, previousScanline);
        }
    }
}
