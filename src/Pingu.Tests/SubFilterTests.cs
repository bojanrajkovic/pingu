using System.Collections.Generic;

using Xunit;

using Pingu.Filters;

namespace Pingu.Tests
{
    public class SubFilterTests
    {
        public static IEnumerable<object[]> SubFilterTestVectors()
        {
            unchecked {
                yield return new object[] {
                    new byte [] { 0x80, 0x60, 0x70, 0x50, 0x10, 0x20, 0x30, 0x40 },
                    new byte [] { 0x80, 0x60, 0x70, 0x50, (byte)(0x10-0x80), (byte)(0x20-0x60), (byte)(0x30-0x70), (byte)(0x40-0x50) },
                    4
                };

                yield return new object[] {
                    new byte [] { 0x12, 0x21, 0x28, 0x35 },
                    new byte [] { 0x12, 0x21, 0x28 - 0x12, 0x35-0x21 },
                    2
                };

                yield return new object[] {
                    new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, 0xb, 0xc, 0xd },
                    new byte[] { 0x1, 0x2, 0x3, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4 },
                    4
                };
            }
        }

        [Theory]
        [MemberData(nameof(SubFilterTestVectors))]
        public void Can_filter_correctly(byte[] input, byte[] expected, int bytesPerPixel)
        {
            var filter = SubFilter.Instance;
            var filteredScanline = new byte[expected.Length];
            filter.FilterInto(filteredScanline, 0, input, null, bytesPerPixel);

            Assert.Equal(expected, filteredScanline);
        }
    }
}
