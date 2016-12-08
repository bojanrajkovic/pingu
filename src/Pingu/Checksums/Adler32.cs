using System;
using System.IO;
using System.Threading.Tasks;

namespace Pingu.Checksums
{
    static class Adler32
    {
        const int Adler32Modulus = 65521;

        public static unsafe int Compute(byte[] data)
        {
            uint a = 1, b = 0;
            fixed (byte* ptr = data) {
                for (var i = 0; i < data.Length; i++) {
                    a = (a + ptr[i]) % Adler32Modulus;
                    b = (b + a) % Adler32Modulus;
                }
            }
            return unchecked((int)((b << 16) | a));
        }

        public static async Task<int> ComputeAsync(Stream data)
        {
            using (var ms = new MemoryStream()) {
                await data.CopyToAsync(ms);
                return Compute(ms.ToArray());
            }
        }
    }
}
