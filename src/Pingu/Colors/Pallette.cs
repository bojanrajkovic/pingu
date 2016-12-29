using System;
using System.Collections.Generic;

namespace Pingu.Colors
{
    public class Pallette
    {
        int[] palletteEntries;
        Dictionary<int, int> reversePallette;

        int currentPalletteEntry = 0;

        public int PalletteSize { get; }

        public Pallette(int bitDepth)
        {
            // The pallette size must not be larger than 2^bitDepth entries.
            PalletteSize = 2 << (bitDepth-1);
            palletteEntries = new int[PalletteSize];
            reversePallette = new Dictionary<int, int> (PalletteSize);
        }

        internal int AddColorToPallette(Color c) => AddPacked(c.Packed);

        internal int AddColorToPallette(byte r, byte g, byte b) => AddPacked(ColorHelpers.PackRgb(r, g, b));

        int AddPacked (int packedColor)
        {
            if (currentPalletteEntry == PalletteSize)
                throw new Exception($"Pallette is at capacity, increase bit depth for the image.");

            palletteEntries[currentPalletteEntry] = packedColor;
            reversePallette[packedColor] = currentPalletteEntry;

            return currentPalletteEntry++;
        }

        internal int GetPackedColor(int i) => palletteEntries[i];

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