using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Pingu
{

    public class IdatChunk : Chunk
    {
        int compressedLength;
        byte[] rawRgbData, compressedData;
        IhdrChunk imageInfo;

        public override string Name => "IDAT";
        public override int Length => compressedLength;

        public IdatChunk(IhdrChunk imageInfo, byte[] rawRgbData)
        {
            this.rawRgbData = rawRgbData;
            this.imageInfo = imageInfo;
        }

        protected override async Task<byte[]> GetChunkDataAsync() => await GetCompressedDataAsync();

        public async Task<byte[]> GetCompressedDataAsync()
        {
            if (compressedData != null)
                return compressedData;

            int pixelWidth;
            switch (imageInfo.ColorType) {
                case 2:
                    pixelWidth = 3;
                    break;
                case 6:
                    pixelWidth = 4;
                    break;
                default:
                    throw new Exception($"Don't know how to deal with color type {imageInfo.ColorType}.");
            }

            // Set up a couple of memory streams where we're going to hold our data.
            // We need one wrapper stream over the raw data, a stream for the final data,
            // and one for the 
            MemoryStream readerStream = new MemoryStream(rawRgbData),
                         compressedStream = new MemoryStream(),
                         scanlineStream = new MemoryStream();

            // Write the zlib stream header. :-).
            compressedStream.Write(new byte[] { 0x78, 0x9C }, 0, 2);

            // Pre-process the raw RGB data to add the filter stream.
            var scanline = new byte[imageInfo.Width * pixelWidth];
            for (int i = 0; i < imageInfo.Height; i++) {
                await readerStream.ReadAsync(scanline, 0, scanline.Length);
                scanlineStream.WriteByte(0);
                await scanlineStream.WriteAsync(scanline, 0, scanline.Length);
            }

            // Deflate the data and write it to the final compressed stream.
            scanlineStream.Seek(0, SeekOrigin.Begin);
            using (var ds = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                await scanlineStream.CopyToAsync(ds);

            // Write the ADLER32 Zlib checksum
            scanlineStream.Seek(0, SeekOrigin.Begin);
            var adler = await Adler32.CalculateAdler32Async(scanlineStream);
            var adlerBytes = GetBytesForInteger(adler);
            compressedStream.Write(adlerBytes, 0, adlerBytes.Length);

            // Store the compressed data and length.
            compressedData = compressedStream.ToArray();
            compressedLength = compressedData.Length;

            return compressedData;
        }
    }
}
