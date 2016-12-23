using System.Collections.Generic;

using Xunit;

using Pingu.Filters;

namespace Pingu.Tests
{
    public class AvgFilterTests
    {
        public static IEnumerable<object[]> AvgFilterTestVectors()
        {
            unchecked {
                yield return new object[] {
                    new byte [] { 10, 22, 47, 91, 106, 82, 28, 111 },
                    new byte [] { 91, 34, 18, 211, 235, 111, 9, 255 },
                    new byte [] { 86, 23, 251, 166, 137, 53, 242, 94 },
                    4
                };

                yield return new object[] {
                    null,
                    new byte [] { 91, 34, 18, 211, 235, 111, 9, 255 },
                    new byte [] { 91, 34, 18, 211, 190, 94, 0, 150 },
                    4
                };
            }
        }

        [Theory]
        [MemberData(nameof(AvgFilterTestVectors))]
        public void Can_filter_correctly(byte[] previous, byte[] current, byte[] expected, int bytesPerPixel)
        {
            var filter = AvgFilter.Instance;
            var filteredScanline = new byte[expected.Length];
            filter.FilterInto(filteredScanline, 0, current, previous, bytesPerPixel);

            Assert.Equal(expected, filteredScanline);
        }
    }
}
