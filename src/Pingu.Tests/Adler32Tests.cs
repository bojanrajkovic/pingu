using System.Collections.Generic;
using System.Text;

using Pingu.Checksums;

using Xunit;

namespace Pingu.Tests
{
    public class Adler32Tests
    {
        public static IEnumerable<object[]> Adler32TestVectors ()
        {
            byte[] ascii (string input) => Encoding.ASCII.GetBytes(input);

            yield return new object[] { ascii(""), 0x00000001 };
            yield return new object[] { ascii("a"), 0x00620062 };
            yield return new object[] { ascii("abc"), 0x024d0127 };
            yield return new object[] { ascii("message digest"), 0x29750586 };
            yield return new object[] { ascii("abcdefghijklmnopqrstuvwxyz"), 0x90860b20 };
            yield return new object[] { ascii("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"), 0x8adb150c };
            yield return new object[] { ascii("12345678901234567890123456789012345678901234567890123456789012345678901234567890"), 0x97b61069 };
            yield return new object[] { ascii("Mark Adler"), 0x13070394 };
            yield return new object[] { new byte[] { 0x00, 0x01, 0x02, 0x03 }, 0x000e0007 };
            yield return new object[] { new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 }, 0x005c001d };
            yield return new object[] { ascii("\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f"), 0x02b80079 };
            yield return new object[] { ascii("\x41\x41\x41\x41"), 0x028e0105 };
            yield return new object[] { ascii("\x42\x42\x42\x42\x42\x42\x42\x42"), 0x09500211 };
            yield return new object[] { ascii("\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43"), 0x23a80431 };
        }

        [Theory]
        [MemberData (nameof (Adler32TestVectors))]
        public void Test_Adler32_computation(byte[] input, uint expected)
        {
            unchecked {
                var actual = Adler32.Compute(input);
                Assert.Equal((int)expected, actual);
            }
        }
    }
}
