using System;
using System.Threading.Tasks;

namespace Pingu.Chunks
{
    public class IhdrChunk : Chunk
    {
        public override string Name => "IHDR";
        public override int Length => 13;

        public int Width { get; }
        public int Height { get; }
        public byte BitDepth { get; }

        // ColorType 6 is true color, with alpha, in RGBA order. 2 is true color, w/o alpha, RGB order.
        public byte ColorType => 6;

        // 0 is the only allowed method, meaning DEFLATE with a sliding window of no more than 32768 bytes
        public byte CompressionMethod => 0;

        // 0 is the only allowed filter method, adaptive filtering with 5 basic filter types.
        public byte FilterMethod => 0;

        // 0 and 1 are the only allowed interlace methods. 0 means no interlacing, 1 means Adam7
        // interlacing.
        public byte InterlaceMethod => 0;

        // Our IHDR chunk will only have 3 fungible values, the rest are going to be hard-coded.
        public IhdrChunk(int width, int height, byte bitDepth)
        {
            if (bitDepth != 8 && bitDepth != 16)
                throw new ArgumentOutOfRangeException(nameof(bitDepth), "Bit depth must be 8 or 16 bits.");

            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be > 0.");

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be > 0.");

            Width = width;
            Height = height;
            BitDepth = bitDepth;
        }

        protected override Task<byte[]> GetChunkDataAsync()
        {
            byte[] chunkData = new byte[13],
                   widthBytes = GetBytesForInteger(Width),
                   heightBytes = GetBytesForInteger(Height);

            Buffer.BlockCopy(widthBytes, 0, chunkData, 0, widthBytes.Length);
            Buffer.BlockCopy(heightBytes, 0, chunkData, 4, heightBytes.Length);
            chunkData[8] = BitDepth;
            chunkData[9] = ColorType;
            chunkData[10] = CompressionMethod;
            chunkData[11] = FilterMethod;
            chunkData[12] = InterlaceMethod;

            return Task.FromResult(chunkData);
        }
    }
}
