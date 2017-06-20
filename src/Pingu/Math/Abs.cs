using System.Runtime.CompilerServices;

namespace Pingu
{
    static partial class Math
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int value)
        {
            var mask = value >> 31;
            return (value ^ mask) - mask;
        }
    }
}
