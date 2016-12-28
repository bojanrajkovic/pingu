using System;
using System.Collections.Generic;
using System.Text;

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
                        sum += PinguMath.Abs(sb[0])  + PinguMath.Abs(sb[1])  + PinguMath.Abs(sb[2])  + PinguMath.Abs(sb[3]) +
                               PinguMath.Abs(sb[4])  + PinguMath.Abs(sb[5])  + PinguMath.Abs(sb[6])  + PinguMath.Abs(sb[7]) +
                               PinguMath.Abs(sb[8])  + PinguMath.Abs(sb[9])  + PinguMath.Abs(sb[10]) + PinguMath.Abs(sb[11]) +
                               PinguMath.Abs(sb[12]) + PinguMath.Abs(sb[13]) + PinguMath.Abs(sb[14]) + PinguMath.Abs(sb[15]);
                    for (; len > 0; len--, sb++)
                        sum += PinguMath.Abs(sb[0]);
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
