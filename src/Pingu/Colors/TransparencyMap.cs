using System;

namespace Pingu.Colors
{
    public class TransparencyMap
    {
        byte[] transparencyMapEntries;

        public int TransparencyMapSize { get; }

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