using System;

namespace Pingu.Colors
{
    public class TransparencyMap
    {
        readonly byte[] transparencyMapEntries;

        // ReSharper disable MemberCanBePrivate.Global
        public int TransparencyMapSize { get; }
        // ReSharper restore MemberCanBePrivate.Global

        public TransparencyMap(int bitDepth)
        {
            TransparencyMapSize = 2 << (bitDepth-1);
            transparencyMapEntries = new byte[TransparencyMapSize];
        }

        internal void AddTransparencyToMap(int palletteIndex, byte alpha)
        {
            if (palletteIndex >= TransparencyMapSize)
                throw new ArgumentOutOfRangeException(
                    nameof (palletteIndex),
                    $"Pallette index {palletteIndex} is too large for this transparency map."
                );

            transparencyMapEntries[palletteIndex] = alpha;
        }

        public byte this[int i] => transparencyMapEntries[i];
    }
}