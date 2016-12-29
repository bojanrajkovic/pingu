using System;
using System.Runtime.CompilerServices;

using Pingu.Colors;
using static Pingu.Math;

namespace Pingu.ImageSources
{
    public class IndexedImageSource : IImageSource
    {
        public ImageFormat Format => ImageFormat.Indexed;
        public byte[] RawData { get; }
        public int BitDepth { get; }
        public int Width { get; }
        public int Height { get; }

        public Pallette Pallette { get; }
        public TransparencyMap TransparencyMap { get; private set; }

        internal IndexedImageSource(
            int bitDepth,
            byte[] rawData,
            int width,
            int height,
            Pallette pallette,
            TransparencyMap transparencyMap)
        {
            if (bitDepth != 1 && bitDepth != 2 && bitDepth != 4 && bitDepth != 8)
                throw new ArgumentOutOfRangeException(
                    nameof(bitDepth),
                    "Indexed color image sources only support depths of 1, 2, 4, and 8 bits."
                );

            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than 0!");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Height must be greater than 0!");

            BitDepth = bitDepth;
            RawData = rawData ?? throw new ArgumentNullException(nameof(rawData));
            Pallette = pallette ?? throw new ArgumentNullException(nameof(pallette));
            TransparencyMap = transparencyMap;
            Width = width;
            Height = height;

            if (rawData.Length == 0)
                throw new ArgumentException($"Raw data must have at least 1 byte.");

            // If the transparency map is null, populate it with 0xff
            if (TransparencyMap == null) {
                TransparencyMap = new TransparencyMap(BitDepth);
                for (int i = 0; i < Pallette.PalletteSize; i++)
                    TransparencyMap.AddTransparencyToMap(i, 0xff);
            }
        }

        public unsafe IImageSource ConvertToGrayscaleAlphaImageSource(int targetBitDepth)
        {
            if (targetBitDepth != 8 && targetBitDepth != 16)
                throw new ArgumentOutOfRangeException(
                    nameof(targetBitDepth),
                    "Grayscale w/ alpha image sources only support bit depths of 8 or 16."
                );

            // For every input pixel, we're going to need targetBitDepth/4 pixels of output space.
            byte[] targetData = new byte[Width * Height * (targetBitDepth / 4)];

            fixed (byte* fixedRaw = RawData)
            fixed (byte* fixedTarget = targetData) {
                byte* raw = fixedRaw, target = fixedTarget;
                int remainder = Width * Height, total = Width * Height,
                    positionMultiplier = targetBitDepth / 4,
                    colorPosition = targetBitDepth == 16 ? 1 : 0,
                    alphaPosition = targetBitDepth == 16 ? 3 : 1;
                int[] colors = new int[8];
                byte[] alphas = new byte[8];

                while (remainder > 0) {
                    var pixelsToUnpack = remainder >= 8 ? 8 : remainder;

                    UnpackPixels(BitDepth, ref raw, pixelsToUnpack, ref colors, ref alphas);

                    for (int i = 0; i < pixelsToUnpack; i++) {
                        target[i * positionMultiplier + colorPosition] = ColorHelpers.RgbToGrayscale(colors[i]);
                        target[i * positionMultiplier + alphaPosition] = alphas[i];
                    }

                    // No need to increment the raw pointer, UnpackPixels has done it for us.
                    remainder -= pixelsToUnpack;
                    target += pixelsToUnpack * positionMultiplier;
                }
            }

            return new GrayscaleAlphaImageSource(targetBitDepth, targetData);
        }

        public unsafe IImageSource ConvertToGrayscaleImageSource(int targetBitDepth)
        {
            if (targetBitDepth != 1 && targetBitDepth != 2 && targetBitDepth != 4 && targetBitDepth != 8 && targetBitDepth != 16)
                throw new ArgumentOutOfRangeException(
                    nameof(targetBitDepth),
                    "Grayscale image sources only support bit depths of 1, 2, 4, 8, or 16."
                );

            // For every input pixel, we're going to need targetBitDepth/8 pixels of output space.
            // So this ranges from only 1/8 of a byte for 1-bit space to 2 bytes for 16-bit.
            byte[] targetData = new byte[Ceil(Width * Height * (double)targetBitDepth / 8)];

            fixed (byte* fixedRaw = RawData)
            fixed (byte* fixedTarget = targetData) {
                byte* raw = fixedRaw, target = fixedTarget;
                int remainder = Width * Height, total = Width * Height,
                    colorPosition = targetBitDepth == 16 ? 1 : 0;
                int[] colors = new int[8];
                byte[] alphas = new byte[8], targetColors = new byte[8];

                while (remainder > 0) {
                    var pixelsToUnpack = remainder >= 8 ? 8 : remainder;

                    Array.Clear(colors, pixelsToUnpack, 8 - pixelsToUnpack);

                    UnpackPixels(BitDepth, ref raw, pixelsToUnpack, ref colors, ref alphas);
                    PackGrayscalePixels(targetBitDepth, ref target, pixelsToUnpack, colors);

                    // No need to increment the raw pointer, UnpackPixels has done it for us,
                    // and PackPixels has incremented the target pointer.
                    remainder -= pixelsToUnpack;
                }
            }

            return new GrayscaleAlphaImageSource(targetBitDepth, targetData);
        }

        public IImageSource ConvertToIndexedImageSource(int targetBitDepth)
        {
            if (targetBitDepth == BitDepth)
                return this;

            if (targetBitDepth < BitDepth)
                throw new ArgumentException($"Pingu cannot convert indexed images to lower bit levels.", nameof(targetBitDepth));

            if (targetBitDepth > 8)
                throw new ArgumentOutOfRangeException(
                    nameof(targetBitDepth),
                    "Indexed color images do not support bit depths larger than 8."
                );

            throw new NotImplementedException();
        }

        public unsafe IImageSource ConvertToTruecolorImageSource(int targetBitDepth)
        {
            return ConvertToTruecolor(targetBitDepth, false);
        }

        public unsafe IImageSource ConvertToTruecolorAlphaImageSource(int targetBitDepth)
        {
            return ConvertToTruecolor(targetBitDepth, true);
        }

        unsafe IImageSource ConvertToTruecolor(int targetBitDepth, bool includeAlpha)
        {
            if (targetBitDepth != 8 && targetBitDepth != 16)
                throw new ArgumentOutOfRangeException(
                    nameof(targetBitDepth),
                    "Truecolor images only support bit depths of 8 or 16."
                );

            // 3 pixels => 9/12 bytes @ 1/2 bytes per sample (target bit depth = 8 or 16)
            // 9 bytes for targetBitDepth = 8, 18 bytes for targetBitDepth = 16 w/o alpha
            // 12 bytes for targetBitDepth = 8, 24 bytes for targetBitDepth = 16 w/ alpha
            int bytesPerPixel = (includeAlpha ? 4 : 3) * targetBitDepth / 8;
            byte[] targetData = new byte[Width * Height * bytesPerPixel];

            fixed (byte* fixedRaw = RawData)
            fixed (byte* fixedTarget = targetData) {
                byte* raw = fixedRaw, target = fixedTarget;
                int remainder = Width * Height, total = Width * Height,
                    colorPosition = targetBitDepth == 16 ? 1 : 0;
                int[] colors = new int[8];
                byte[] alphas = new byte[8];

                while (remainder > 0) {
                    var pixelsToUnpack = remainder >= 8 ? 8 : remainder;

                    UnpackPixels(BitDepth, ref raw, pixelsToUnpack, ref colors, ref alphas);

                    // 8 RGB pixels
                    // 16-bit: 0R0G0B0A
                    // 8-bit: RGBA

                    for (int i = 0; i < pixelsToUnpack; i++) {
                        var color = Color.FromPackedRgb(colors[i], alphas[i]);
                        target[i * bytesPerPixel + 0 + colorPosition] = color.R;
                        target[i * bytesPerPixel + 1 + colorPosition * 2] = color.G;
                        target[i * bytesPerPixel + 2 + colorPosition * 3] = color.B;
                        if (includeAlpha)
                            target[i * bytesPerPixel + 3 + colorPosition * 4] = color.A;
                    }

                    remainder -= pixelsToUnpack;
                    target += pixelsToUnpack * bytesPerPixel;
                }
            }

            if (!includeAlpha)
                return new TruecolorImageSource(targetBitDepth, targetData);
            else
                return new TruecolorAlphaImageSource(targetBitDepth, targetData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void PackGrayscalePixels(int targetBitDepth, ref byte* target, int pixelsToPack, int[] colors)
        {
            // 3 pixels at 4 bit depth / 8 bits per byte = 3*(4/8) = 1.5 bytes = 2 bytes
            int packedBytes = Ceil(pixelsToPack * (targetBitDepth / 8));

            // At most, we can pack 64 bytes of colors.
            switch (targetBitDepth) {
                case 16:
                    for (int i = 0; i < pixelsToPack; i++, target += 2)
                        target[1] = ColorHelpers.RgbToGrayscale(colors[i]);
                    break;
                case 8:
                    for (int i = 0; i < pixelsToPack; i++, target++)
                        *target = ColorHelpers.RgbToGrayscale(colors[i]);
                    break;
                case 4:
                    // Since we know we need at least `packedBytes` bytes, we can base our loop on that.
                    // The caller is clearing the colors array before unpacking each set of up to 8, so we
                    // can *also* rely on any members of the color array past `colors[pixelsToPack-i]` being 0
                    // and thus reducing to 0 bits in the packing.
                    for (int i = 0, colorIndex = 0; i < packedBytes; i++, colorIndex += 2, target++)
                        *target = (byte)(Reduce(ColorHelpers.RgbToGrayscale(colors[colorIndex]), 4) << 4 |
                                         Reduce(ColorHelpers.RgbToGrayscale(colors[colorIndex + 1]), 4));
                    break;
                case 2:
                    for (int i = 0, colorIndex = 0; i < packedBytes; i++, colorIndex += 4, target++)
                        *target = (byte)(
                            Reduce(ColorHelpers.RgbToGrayscale(colors[colorIndex]), 2) << 6 |
                            Reduce(ColorHelpers.RgbToGrayscale(colors[colorIndex + 1]), 2) << 4 |
                            Reduce(ColorHelpers.RgbToGrayscale(colors[colorIndex + 2]), 2) << 2 |
                            Reduce(ColorHelpers.RgbToGrayscale(colors[colorIndex + 3]), 2)
                        );
                    break;
                case 1:
                    // Here we need at most 1 packed byte, so just pack all the colors.
                    *target = (byte)(
                        Reduce(ColorHelpers.RgbToGrayscale(colors[0]), 1) << 7 |
                        Reduce(ColorHelpers.RgbToGrayscale(colors[1]), 1) << 6 |
                        Reduce(ColorHelpers.RgbToGrayscale(colors[2]), 1) << 5 |
                        Reduce(ColorHelpers.RgbToGrayscale(colors[3]), 1) << 4 |
                        Reduce(ColorHelpers.RgbToGrayscale(colors[4]), 1) << 3 |
                        Reduce(ColorHelpers.RgbToGrayscale(colors[5]), 1) << 2 |
                        Reduce(ColorHelpers.RgbToGrayscale(colors[6]), 1) << 1 |
                        Reduce(ColorHelpers.RgbToGrayscale(colors[7]), 1)
                    );
                    target++;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void UnpackPixels(int bitDepth, ref byte* src, int pixelsToUnpack, ref int[] colors, ref byte[] alphas)
        {
            // The number of bytes it takes to unpack `pixelsToUnpack` pixels is equal to
            // the ceil(pixelsToUnpack*(bitDepth/8)). The logic here is that each pixel takes up 
            // (bitDepth/8) of a byte, so we need to multiply that fraction by the # of pixels to get
            // some number of bytes. So, if we want to unpack 3 pixels and bit depth is 1, we need 
            // ceil(3 * (1/8)) => ceil(3/) => 1 byte. Likewise, 3 pixels of bit depth 4, the calculation
            // is ceil(3 * (4/8)) => ceil(1.5) => 2 bytes.
            var bytesToRead = Ceil(pixelsToUnpack * ((float)bitDepth / 8));

            // At a maximum of 8 pixels to unpack @ 8 bits per pixel, packedColors needs to be a long.
            long packedColors = 0;
            for (int i = 0; i < bytesToRead; i++, src++)
                packedColors = (((long)*src) << (56 - (i * 8)) | packedColors);
            long mask = -1 ^ (((long)1 << (64 - bitDepth)) - 1);
            for (int i = 0; i < pixelsToUnpack; i++) {
                int pixel = (int)((ulong)((packedColors << i * bitDepth) & mask) >> (64 - bitDepth));
                colors[i] = Pallette.GetPackedColor(pixel);
                alphas[i] = TransparencyMap[pixel];
            }
        }
    }
}