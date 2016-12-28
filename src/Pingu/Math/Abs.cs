using System.Runtime.CompilerServices;

namespace Pingu
{
    static partial class Math
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int value)
        {
            int mask = value >> 31;
            return (value ^ mask) - mask;
        }
    }
}
