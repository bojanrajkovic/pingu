using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Pingu.Checksums;

using Xunit;

namespace Pingu.Tests
{
    public class Adler32Tests
    {
        static IEnumerable<object[]> Adler32TestVectors ()
        {
            byte[] Ascii (string input) => Encoding.ASCII.GetBytes(input);

            yield return new object[] { Ascii(""), 0x00000001 };
            yield return new object[] { Ascii("a"), 0x00620062 };
            yield return new object[] { Ascii("abc"), 0x024d0127 };
            yield return new object[] { Ascii("message digest"), 0x29750586 };
            yield return new object[] { Ascii("abcdefghijklmnopqrstuvwxyz"), 0x90860b20 };
            yield return new object[] { Ascii("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"), 0x8adb150c };
            yield return new object[] { Ascii("12345678901234567890123456789012345678901234567890123456789012345678901234567890"), 0x97b61069 };
            yield return new object[] { Ascii("Mark Adler"), 0x13070394 };
            yield return new object[] { new byte[] { 0x00, 0x01, 0x02, 0x03 }, 0x000e0007 };
            yield return new object[] { new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 }, 0x005c001d };
            yield return new object[] { Ascii("\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f"), 0x02b80079 };
            yield return new object[] { Ascii("\x41\x41\x41\x41"), 0x028e0105 };
            yield return new object[] { Ascii("\x42\x42\x42\x42\x42\x42\x42\x42"), 0x09500211 };
            yield return new object[] { Ascii("\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43\x43"), 0x23a80431 };
        }

        [Fact]
        public async Task Can_compute_file_checksum()
        {
            var asm = typeof(PngFileTests).GetTypeInfo().Assembly;
            var resource = asm.GetManifestResourceStream("Pingu.Tests.Zooey.RGBA32");
            var checksum = await Adler32.ComputeAsync(resource);

            Assert.Equal(unchecked((int) 0xf14287e8), checksum);
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

        [Theory]
        [MemberData (nameof (Adler32TestVectors))]
        public async Task Test_Adler32_stream_computation(byte[] input, uint expected)
        {
            unchecked {
                var stream = new MemoryStream(input);
                var actual = await Adler32.ComputeAsync(stream);
                Assert.Equal((int)expected, actual);
            }
        }
    }
}