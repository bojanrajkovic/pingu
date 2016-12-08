using System;
using System.IO;
using System.Threading.Tasks;

namespace Pingu.Checksums
{
    class Adler32
    {
        const int Adler32Modulus = 65521;

        uint a = 1, b = 0;

        public int Hash => unchecked((int)((b << 16) | a));

        public unsafe Adler32 FeedBlock(byte[] data)
        {
            fixed (byte* ptr = data) {
                for (var i = 0; i < data.Length; i++) {
                    a = (a + ptr[i]) % Adler32Modulus;
                    b = (b + a) % Adler32Modulus;
                }
            }

            return this;
        }

        public static int Compute(byte[] data) => new Adler32().FeedBlock(data).Hash;

        public static async Task<int> ComputeAsync(Stream data)
        {
            using (var ms = new MemoryStream()) {
                await data.CopyToAsync(ms);
                return Compute(ms.ToArray());
            }
        }
    }
}
