using System;
using System.Threading.Tasks;

using Pingu.Colors;

namespace Pingu.Chunks
{
    public class IhdrChunk : Chunk
    {
        public override string Name => "IHDR";
        public override int Length => 13;

        public int Width { get; }
        public int Height { get; }
        public int BitDepth { get; }

        // ColorType 6 is true color, with alpha, in RGBA order. 2 is true color, w/o alpha, RGB order.
        public ColorType ColorType { get; }

        // 0 is the only allowed method, meaning DEFLATE with a sliding window of no more than 32768 bytes
        public byte CompressionMethod => 0;

        // 0 is the only allowed filter method, adaptive filtering with 5 basic filter types.
        public byte FilterMethod => 0;

        // 0 and 1 are the only allowed interlace methods. 0 means no interlacing, 1 means Adam7
        // interlacing.
        public byte InterlaceMethod => 0;

        // Our IHDR chunk will only have 3 fungible values, the rest are going to be hard-coded.
        public IhdrChunk(int width, int height, int bitDepth, ColorType colorType)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be within the range of 1 to 2^31-1.");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be within the range of 1 to 2^31-1.");

            if (!Enum.IsDefined (typeof (ColorType), colorType))
                throw new ArgumentOutOfRangeException(nameof(colorType), $"Undefined color type {colorType}.");

            if (bitDepth != 1 && bitDepth != 2 && bitDepth != 4 && bitDepth != 8 && bitDepth != 16)
                throw new ArgumentException("Bit depth must be one of 1, 2, 4, 8, or 16.", nameof (bitDepth));

            // Not all color types and bit depths can be combined.
            switch (colorType) {
                case ColorType.Indexed:
                    if (bitDepth != 1 && bitDepth != 2 && bitDepth != 4 && bitDepth != 8)
                        throw new Exception($"Bit depth {bitDepth} is not compatible with indexed color. " +
                                            "Only bit depths of 1, 2, 4, and 8 are allowed.");
                    break;
                case ColorType.GrayscaleAlpha:
                    if (bitDepth != 8 && bitDepth != 16)
                        throw new Exception($"Bit depth {bitDepth} is not compatible with grayscale color with alpha.");
                    break;
                case ColorType.Truecolor:
                    if (bitDepth != 8 && bitDepth != 16)
                        throw new Exception($"Bit depth {bitDepth} is not compatible with true color.");
                    break;
                case ColorType.TruecolorAlpha:
                    if (bitDepth != 8 && bitDepth != 16)
                        throw new Exception($"Bit depth {bitDepth} is not compatible with true color with alpha.");
                    break;
            }

            Width = width;
            Height = height;
            BitDepth = bitDepth;
            ColorType = colorType;
        }

        protected override Task<byte[]> GetChunkDataAsync()
        {
            byte[] chunkData = new byte[13],
                   widthBytes = GetBytesForInteger(Width),
                   heightBytes = GetBytesForInteger(Height);

            Buffer.BlockCopy(widthBytes, 0, chunkData, 0, widthBytes.Length);
            Buffer.BlockCopy(heightBytes, 0, chunkData, 4, heightBytes.Length);
            chunkData[8] = (byte) BitDepth;
            chunkData[9] = (byte) ColorType;
            chunkData[10] = CompressionMethod;
            chunkData[11] = FilterMethod;
            chunkData[12] = InterlaceMethod;

            return Task.FromResult(chunkData);
        }
    }
}
