using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

using Pingu.Checksums;

namespace Pingu.Tests
{
    public class Crc32Tests
    {
        public static IEnumerable<object[]> Crc32TestVectors()
        {
            byte[] ascii(string input) => Encoding.ASCII.GetBytes(input);
            byte[] hex(string input) => Enumerable.Range(0, input.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(input.Substring(x, 2), 16))
                     .ToArray();

            yield return new object[] { ascii("123456789"), 0xCBF43926 };
            yield return new object[] { hex("49484452000002F0000005360806000000"), 0xF3B0825D };
        }

        [Theory]
        [MemberData(nameof(Crc32TestVectors))]
        public void Test_CRC32_Computation(byte[] input, uint expected)
        {
            unchecked {
                var actual = Crc32.Compute(input);
                Assert.Equal((int)expected, actual);
            }
        }
    }
}
