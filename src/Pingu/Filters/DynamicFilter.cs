using System;

using static Pingu.Math;

namespace Pingu.Filters
{
    class DynamicFilter : IFilter
    {
        static readonly IFilter[] PossibleFilters = new IFilter[] {
            NullFilter.Instance,
            SubFilter.Instance,
            UpFilter.Instance,
            AvgFilter.Instance,
            PaethFilter.Instance
        };

        private static readonly Lazy<DynamicFilter> lazy
            = new Lazy<DynamicFilter>(() => new DynamicFilter());

        public static DynamicFilter Instance => lazy.Value;

        internal DynamicFilter() { }

        public FilterType Type => FilterType.Dynamic;

        unsafe int SumAbsoluteDifferences(byte[] bytes)
        {
            int sum = 0, len = bytes.Length;
            unchecked {
                fixed (byte* ptr = bytes) {
                    sbyte* sb = (sbyte*)ptr;
                    for (; len >= 16; len -= 16, sb += 16)
                        sum += Abs(sb[0])  + Abs(sb[1])  + Abs(sb[2])  + Abs(sb[3]) +
                               Abs(sb[4])  + Abs(sb[5])  + Abs(sb[6])  + Abs(sb[7]) +
                               Abs(sb[8])  + Abs(sb[9])  + Abs(sb[10]) + Abs(sb[11]) +
                               Abs(sb[12]) + Abs(sb[13]) + Abs(sb[14]) + Abs(sb[15]);
                    for (; len > 0; len--, sb++)
                        sum += Abs(sb[0]);
                }
            }
            return sum;
        }

        public void FilterInto(byte[] targetBuffer, int targetOffset, byte[] rawScanline, byte[] previousScanline, int bytesPerPixel)
        {
            IFilter bestFilter = NullFilter.Instance;
            int bestSum = int.MaxValue;

            foreach (var filter in PossibleFilters) {
                filter.FilterInto(targetBuffer, 1, rawScanline, previousScanline, bytesPerPixel);
                var sum = SumAbsoluteDifferences(targetBuffer);

                if (sum < bestSum) {
                    bestSum = sum;
                    bestFilter = filter;
                }
            }

            // Redo this, it's cheaper than 5 allocations. Set the byte immediately before the offset to the chosen filter type.
            targetBuffer[targetOffset-1] = (byte)bestFilter.Type;
            bestFilter.FilterInto(targetBuffer, targetOffset, rawScanline, previousScanline, bytesPerPixel);
        }
    }
}
