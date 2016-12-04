using System;
using System.IO;
using System.Threading.Tasks;

namespace Pingu
{
    public static class Adler32
    {
        const int Adler32Modulus = 65521;

        public static Task<int> ComputeAsync(byte[] data) =>
            ComputeAsync(new MemoryStream(data));

        public static async Task<int> ComputeAsync(Stream data)
        {
            uint a = 1, b = 0;
            var buffer = new byte[4 * 1024];
            var read = 0;

            while ((read = await data.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                unsafe {
                    fixed (byte* bufPtr = buffer) {
                        for (var i = 0; i < read; i++) {
                            a = (a + *(bufPtr + i)) % Adler32Modulus;
                            b = (b + a) % Adler32Modulus;
                        }
                    }
                }
            }

            unchecked {
                return (int)((b << 16) | a);
            }
        }
    }
}
