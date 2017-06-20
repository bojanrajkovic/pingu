using System;

namespace Pingu.Filters
{
    class AvgFilter : IFilter
    {
        static readonly Lazy<AvgFilter> Lazy
            = new Lazy<AvgFilter>(() => new AvgFilter());

        public static AvgFilter Instance => Lazy.Value;

        AvgFilter() { }

        public FilterType Type => FilterType.Average;

        public unsafe void FilterInto(
            byte[] targetBuffer, 
            int targetOffset, 
            byte[] rawScanline, 
            byte[] previousScanline, 
            int bytesPerPixel)
        {
            fixed (byte* targetUnoffset = targetBuffer)
            fixed (byte* raw = rawScanline)
            fixed (byte* previous = previousScanline) {
                byte* target = targetUnoffset + targetOffset;

                if (previous == null) {
                    // Basically Sub, but with the Raw(x-bpp) value divided by 2
                    Buffer.MemoryCopy(raw, target, rawScanline.Length, bytesPerPixel);
                    unchecked {
                        var x = bytesPerPixel;
                        target += bytesPerPixel;
                        byte* rawm = raw + bytesPerPixel, rawBpp = raw;

                        for (; rawScanline.Length - x > 8; x += 8) {
                            target[0] = (byte)(rawm[0] - rawBpp[0] / 2);
                            target[1] = (byte)(rawm[1] - rawBpp[1] / 2);
                            target[2] = (byte)(rawm[2] - rawBpp[2] / 2);
                            target[3] = (byte)(rawm[3] - rawBpp[3] / 2);
                            target[4] = (byte)(rawm[4] - rawBpp[4] / 2);
                            target[5] = (byte)(rawm[5] - rawBpp[5] / 2);
                            target[6] = (byte)(rawm[6] - rawBpp[6] / 2);
                            target[7] = (byte)(rawm[7] - rawBpp[7] / 2);
                            target += 8; rawm += 8; rawBpp += 8;
                        }

                        for (; x < rawScanline.Length; x++) {
                            target[0] = (byte)(rawm[0] - rawBpp[0] / 2);
                            target++; rawm++; rawBpp++;
                        }
                    }
                } else {
                    var i = 0;
                    byte* rawm = raw, prev = previous, rawBpp = raw - bytesPerPixel;

                    unchecked {
                        for (; i < bytesPerPixel; i++) {
                            target[0] = (byte)(rawm[0] - prev[0] / 2);
                            target++; rawm++; prev++; rawBpp++;
                        }

                        for (; rawScanline.Length - i > 8; i += 8) {
                            target[0] = (byte)(rawm[0] - (rawBpp[0] + prev[0]) / 2);
                            target[1] = (byte)(rawm[1] - (rawBpp[1] + prev[1]) / 2);
                            target[2] = (byte)(rawm[2] - (rawBpp[2] + prev[2]) / 2);
                            target[3] = (byte)(rawm[3] - (rawBpp[3] + prev[3]) / 2);
                            target[4] = (byte)(rawm[4] - (rawBpp[4] + prev[4]) / 2);
                            target[5] = (byte)(rawm[5] - (rawBpp[5] + prev[5]) / 2);
                            target[6] = (byte)(rawm[6] - (rawBpp[6] + prev[6]) / 2);
                            target[7] = (byte)(rawm[7] - (rawBpp[7] + prev[7]) / 2);
                            target += 8; rawm += 8; prev += 8; rawBpp += 8;
                        }

                        for (; i < rawScanline.Length; i++) {
                            target[0] = (byte)(rawm[0] - (rawBpp[0] + prev[0]) / 2);
                            target++; rawm++; prev++; rawBpp++;
                        }
                    }
                }
            }
        }
    }
}
