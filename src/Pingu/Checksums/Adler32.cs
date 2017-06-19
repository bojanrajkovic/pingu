using System.IO;
using System.Threading.Tasks;

namespace Pingu.Checksums
{
    class Adler32
    {
        const int Adler32Modulus = 65521;
        const int NMax = 5552;

        uint a = 1, b;

        public int Hash => unchecked((int)((b << 16) | a));

        // See Adler32Implementation in the benchmarks suite. This is the best implementation
        // on RyuJIT and Mono, and not far behind on the legacy 64-bit JIT. It relies on the fact that
        // we can do up to NMax adds of 0xff before we reach a number higher than the Adler32 modulus
        // to defer the relatively expensive modulo operation in favor of a lot of adds and subtracts.
        // This implementation also approximates an unrolled 16-byte sum loop by defining a set of methods
        // like zlib's macros and asking the JIT to aggressively inline them. RyuJIT and Mono support this,
        // the legacy JIT I think does not.
        public unsafe Adler32 FeedBlock(byte[] data, int offset, int length)
        {
            fixed (byte* ptr = data) {
                byte* buf = ptr + offset;
                while (length > 0) {
                    var chunkSize = length < NMax ? length : NMax;
                    length -= chunkSize;
                    while (chunkSize >= 16) {
                        // This is a hand-unrolled 16 byte loop. Do not touch.
                        a += buf[0];
                        b += a;
                        a += buf[1];
                        b += a;
                        a += buf[2];
                        b += a;
                        a += buf[3];
                        b += a;
                        a += buf[4];
                        b += a;
                        a += buf[5];
                        b += a;
                        a += buf[6];
                        b += a;
                        a += buf[7];
                        b += a;
                        a += buf[8];
                        b += a;
                        a += buf[9];
                        b += a;
                        a += buf[10];
                        b += a;
                        a += buf[11];
                        b += a;
                        a += buf[12];
                        b += a;
                        a += buf[13];
                        b += a;
                        a += buf[14];
                        b += a;
                        a += buf[15];
                        b += a;
                        // End hand-unrolled loop.
                        buf += 16;
                        chunkSize -= 16;
                    }
                    if (chunkSize != 0) {
                        do {
                            a += *buf++;
                            b += a;
                        } while (--chunkSize > 0);
                    }
                    a %= Adler32Modulus;
                    b %= Adler32Modulus;
                }
            }

            return this;
        }

        public Adler32 FeedBlock(byte[] data) => FeedBlock(data, 0, data.Length);

        public static int Compute(byte[] data) => new Adler32().FeedBlock(data).Hash;

        public static async Task<int> ComputeAsync(Stream data)
        {
            var adler = new Adler32();
            int read;
            byte[] buf = new byte[8192];

            while ((read = await data.ReadAsync(buf, 0, buf.Length)) > 0)
                adler.FeedBlock(buf, 0, read);

            return adler.Hash;
        }
    }
}
