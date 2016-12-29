using System.Runtime.CompilerServices;

namespace Pingu
{
    static partial class Math
    {
        const double doubleMagicDelta = (1.5e-8);
        const double doubleMagicRoundEps = (.5f - doubleMagicDelta);
        const double doubleMagic = 6755399441055744.0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int Ceil(double val)
        {
            val = val + doubleMagicRoundEps + doubleMagic;
            return ((int*)&val)[0];
        }
    }
}
