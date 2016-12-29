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

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
