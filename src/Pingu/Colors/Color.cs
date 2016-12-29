using System;
using System.Collections.Generic;
using System.Text;

namespace Pingu.Colors
{
    public struct Color
    {
        public byte R, G, B, A;

        public byte Grayscale => ColorHelpers.RgbToGrayscale(R, G, B);
        public int Packed => ColorHelpers.PackRgb(R, G, B);

        public static Color FromPackedRgb(int packed, byte alpha = 0xff)
        {
            byte[] unpacked = ColorHelpers.UnpackRgb(packed);
            return new Color(unpacked[0], unpacked[1], unpacked[2], alpha);
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
