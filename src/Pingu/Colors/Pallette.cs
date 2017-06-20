using System;
using System.Collections.Generic;

namespace Pingu.Colors
{
    public class Pallette
    {
        readonly int[] palletteEntries;
        readonly Dictionary<int, int> reversePallette;

        int currentPalletteEntry;

        // ReSharper disable MemberCanBePrivate.Global
        public int PalletteSize { get; }
        // ReSharper restore MemberCanBePrivate.Global

        public Pallette(int bitDepth)
        {
            // The pallette size must not be larger than 2^bitDepth entries.
            PalletteSize = 2 << (bitDepth-1);
            palletteEntries = new int[PalletteSize];
            reversePallette = new Dictionary<int, int> (PalletteSize);
        }

        internal int AddColorToPallette(byte r, byte g, byte b)
        {
            if (currentPalletteEntry == PalletteSize)
                throw new Exception("Pallette is at capacity, increase bit depth for the image.");

            int color = ColorHelpers.PackRgb(r, g, b);
            palletteEntries[currentPalletteEntry] = color;
            reversePallette[color] = currentPalletteEntry;

            return currentPalletteEntry++;
        }

        public byte[] this[int i] => ColorHelpers.UnpackRgb(palletteEntries[i]);

        public int this[byte r, byte g, byte b]
        {
            get {
                var color = r << 16 | g << 8 | b;
                return reversePallette[color];
            }
        }
    }
}