using System.Collections.Generic;

using Xunit;

using Pingu.Filters;

namespace Pingu.Tests
{
    public class UpFilterTests
    {
        public static IEnumerable<object[]> UpFilterTestVectors()
        {
            unchecked {
                yield return new object[] {
                    new byte [] { 0x80, 0x60, 0x70, 0x50, 0x10, 0x20, 0x30, 0x40 },
                    new byte [] { 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18 },
                    new byte [] { 0x6f, 0x4e, 0x5d, 0x3c, 0xfb, 0x0a, 0x19, 0x28 }
                };

                yield return new object[] {
                    new byte [] { 0x12, 0x21, 0x28, 0x35 },
                    new byte [] { 0x1, 0x7, 0x19, 0x31 },
                    new byte [] { 0x11, 0x1a, 0xf, 0x4 }
                };

                yield return new object[] {
                    new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, 0xb, 0xc, 0xd },
                    new byte[] { 0x1, 0x2, 0x3, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4, 0x4 },
                    new byte[] { 0, 0, 0, 0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 }
                };
            }
        }

        [Theory]
        [MemberData(nameof(UpFilterTestVectors))]
        public void Can_filter_correctly_without_vectors(byte[] input, byte[] previous, byte[] expected)
        {
            var filter = UpFilter.Instance;
            var filteredScanline = new byte[expected.Length];
            filter.UnrolledPointerFilterInto(filteredScanline, 0, input, previous);

            Assert.Equal(expected, filteredScanline);
        }

        [Theory]
        [MemberData(nameof(UpFilterTestVectors))]
        public void Can_filter_correctly(byte[] input, byte[] previous, byte[] expected)
        {
            var filter = UpFilter.Instance;
            var filteredScanline = new byte[expected.Length];
            filter.VectorAndPointerFilterInto(filteredScanline, 0, input, previous);

            Assert.Equal(expected, filteredScanline);
        }
    }
}
