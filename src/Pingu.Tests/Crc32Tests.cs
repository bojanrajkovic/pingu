using System;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

namespace Pingu.Tests
{
    public class Crc32Tests
    {
        static IEnumerable<object[]> Crc32TestVectors()
        {
            byte[] Ascii(string input) => Encoding.ASCII.GetBytes(input);
            byte[] Hex(string input) => Enumerable.Range(0, input.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(input.Substring(x, 2), 16))
                     .ToArray();

            yield return new object[] { Ascii("123456789"), 0xCBF43926 };
            yield return new object[] { Hex("49484452000002F0000005360806000000"), 0xF3B0825D };
        }

        [Theory]
        [MemberData(nameof(Crc32TestVectors))]
        public void Test_CRC32_Computation(byte[] input, uint expected)
        {
            var actual = Crc32Helper.UpdateCrc32(0, input, 0, input.Length);
            Assert.Equal(expected, actual);
        }
    }
}
