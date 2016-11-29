using System;
using System.IO;
using System.Threading.Tasks;

namespace Pingu
{
    public static class Adler32
    {
        const int Adler32Modulus = 65521;

        public static Task<uint> CalculateAdler32Async(byte[] data) =>
            CalculateAdler32Async(new MemoryStream(data));

        public static async Task<uint> CalculateAdler32Async(Stream data)
        {
            uint a = 1, b = 0;
            var buffer = new byte[4 * 1024];
            var read = 0;

            while ((read = await data.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                for (var i = 0; i < read; i++) {
                    a = (a + buffer[i]) % Adler32Modulus;
                    b = (b + a) % Adler32Modulus;
                }
            }

            return (b << 16) | a;
        }
    }
}
