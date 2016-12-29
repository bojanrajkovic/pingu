using System.Runtime.CompilerServices;

namespace Pingu
{
    static partial class Math
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Reduce(byte gray, int targetBits) => (byte)(((float)gray / 256) * (2 << (targetBits - 1)));
    }
}
