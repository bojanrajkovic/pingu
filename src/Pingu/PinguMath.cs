using System.Runtime.CompilerServices;

namespace Pingu
{
    static class PinguMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int value)
        {
            int temp = value >> 31;
            value ^= temp;
            value += temp & 1;
            return value;
        }
    }
}
