using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Pingu.Checksums
{
    class Adler32
    {
        const int Adler32Modulus = 65521;
        const int NMax = 5552;

        uint a = 1, b = 0;

        public int Hash => unchecked((int)((b << 16) | a));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do16(byte* buf, ref uint a, ref uint b) { Do8(buf, 0, ref a, ref b); Do8(buf, 8, ref a, ref b); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do8(byte* buf, int i, ref uint a, ref uint b) { Do4(buf, i, ref a, ref b); Do4(buf, i + 4, ref a, ref b); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do4(byte* buf, int i, ref uint a, ref uint b) { Do2(buf, i, ref a, ref b); Do2(buf, i + 2, ref a, ref b); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do2(byte* buf, int i, ref uint a, ref uint b) { Do1(buf, i, ref a, ref b); Do1(buf, i + 1, ref a, ref b); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Do1(byte* buf, int i, ref uint a, ref uint b) { a += buf[i]; b += a; }

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
                int chunkSize;
                while (length > 0) {
                    chunkSize = length < NMax ? length : NMax;
                    length -= chunkSize;
                    while (chunkSize >= 16) {
                        Do16(buf, ref a, ref b);
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
