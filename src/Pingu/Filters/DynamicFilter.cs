using System;
using System.Collections.Generic;
using System.Text;

namespace Pingu.Filters
{
    class DynamicFilter : IFilter
    {
        static readonly IFilter[] PossibleFilters = new IFilter[] {
            SubFilter.Instance,
            UpFilter.Instance,
            // AverageFilter.Instance,
            // PaethFilter.Instance
        };

        private static readonly Lazy<DynamicFilter> lazy
            = new Lazy<DynamicFilter>(() => new DynamicFilter());

        public static DynamicFilter Instance => lazy.Value;

        internal DynamicFilter() { }

        public FilterType Type => FilterType.Dynamic;

        unsafe int SumAbsoluteDifferences(byte[] bytes)
        {
            fixed (byte* ptr = bytes) {
                int sum = 0;
                for (var i = 0; i < bytes.Length; i++) {
                    var val = ptr[i];
                    sum += val < 128 ? val : 256 - val;
                }
                return sum;
            }
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
