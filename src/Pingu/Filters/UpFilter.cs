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
            fixed (byte* target = targetBuffer) {
                int i = 0;
                for (; rawScanline.Length - i > 8; i += 8) {
                    target[i + targetOffset] = (byte)((raw[i] - previous[i]) % 256);
                    target[i + 1 + targetOffset] = (byte)((raw[i + 1] - previous[i + 1]) % 256);
                    target[i + 2 + targetOffset] = (byte)((raw[i + 2] - previous[i + 2]) % 256);
                    target[i + 3 + targetOffset] = (byte)((raw[i + 3] - previous[i + 3]) % 256);
                    target[i + 4 + targetOffset] = (byte)((raw[i + 4] - previous[i + 4]) % 256);
                    target[i + 5 + targetOffset] = (byte)((raw[i + 5] - previous[i + 5]) % 256);
                    target[i + 6 + targetOffset] = (byte)((raw[i + 6] - previous[i + 6]) % 256);
                    target[i + 7 + targetOffset] = (byte)((raw[i + 7] - previous[i + 7]) % 256);
                }

                for (; i < rawScanline.Length; i++)
                    target[i + targetOffset] = (byte)((raw[i] - previous[i]) % 256);
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
