using System.Collections.Generic;

using Xunit;

using Pingu.Filters;

namespace Pingu.Tests
{
    public class PaethFilterTests
    {
        static IEnumerable<object[]> PaethFilterTestVectors()
        {
            yield return new object[] {
                new byte [] { 10, 22, 47, 91, 106, 82, 28, 111 },
                new byte [] { 91, 34, 18, 211, 235, 111, 9, 255 },
                new byte [] { 81, 12, 227, 120, 129, 29, 247, 44 },
                4
            };

            yield return new object[] {
                null,
                new byte [] { 91, 34, 18, 211, 235, 111, 9, 255 },
                new byte [] { 91, 34, 18, 211, 144, 77, 247, 44 },
                4
            };

            yield return new object[] {
                new byte [] { 10, 22, 47, 91, 106, 82, 28, 111, 23, 43, 99, 101, 12 },
                new byte [] { 91, 34, 18, 211, 235, 111, 9, 255, 34, 191, 54, 91, 233 },
                new byte [] { 81, 12, 227, 120, 129, 29, 247, 44, 184, 109, 211, 92, 210 },
                4
            };
        }

        [Theory]
        [MemberData(nameof(PaethFilterTestVectors))]
        public void Can_filter_correctly(byte[] previous, byte[] current, byte[] expected, int bytesPerPixel)
        {
            var filter = PaethFilter.Instance;
            var filteredScanline = new byte[expected.Length];
            filter.FilterInto(filteredScanline, 0, current, previous, bytesPerPixel);

            Assert.Equal(expected, filteredScanline);
        }
    }
}
