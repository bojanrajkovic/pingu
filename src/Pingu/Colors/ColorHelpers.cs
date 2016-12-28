using System.Runtime.CompilerServices;

namespace Pingu.Colors
{
    static class ColorHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PackRgb (byte r, byte g, byte b) => r << 16 | g << 8 | b;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] UnpackRgb (int color)
        {
            return new byte[] {
                (byte)(color >> 16),
                (byte)((color >> 8) & 0xff),
                (byte)(color & 0xff)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RgbToGrayscale (int packedColor)
        {
            var unpacked = UnpackRgb(packedColor);
            return RgbToGrayscale(unpacked[0], unpacked[1], unpacked[2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RgbToGrayscale (byte r, byte g, byte b) => (byte)(0.2126 * r + 0.7152 * g + 0.0722 * b);
    }
}