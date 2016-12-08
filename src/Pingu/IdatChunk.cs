using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Pingu.Filters;

namespace Pingu
{

    public class IdatChunk : Chunk
    {
        int compressedLength;
        byte[] rawRgbData, compressedData;
        IhdrChunk imageInfo;

        public override string Name => "IDAT";
        public override int Length => compressedLength;
        public FilterType FilterType { get; }

        public IdatChunk(IhdrChunk imageInfo, byte[] rawRgbData, FilterType filterType)
        {
            this.rawRgbData = rawRgbData ?? throw new ArgumentNullException(nameof(rawRgbData));
            this.imageInfo = imageInfo ?? throw new ArgumentNullException(nameof(imageInfo));

            // TODO: Maybe throw our own exception type here?
            if (!Enum.IsDefined(typeof(FilterType), (byte)filterType))
                throw new ArgumentOutOfRangeException(
                    nameof(filterType),
                    $"Filter type {filterType} is not defined."
                );

            if (filterType == FilterType.Dynamic)
                throw new NotSupportedException("Dynamic filter range is not supported yet.");

            FilterType = filterType;
        }

        protected override async Task<byte[]> GetChunkDataAsync() => await GetCompressedDataAsync();

        public async Task<byte[]> GetCompressedDataAsync()
        {
            if (compressedData != null)
                return compressedData;

            int pixelWidth = GetPixelWidthForImage();

            // Set up a couple of memory streams where we're going to hold our data.
            // We need one wrapper stream over the raw data, a stream for the final data,
            // and one for the 
            MemoryStream readerStream = new MemoryStream(rawRgbData),
                         compressedStream = new MemoryStream(),
                         scanlineStream = new MemoryStream();

            // Write the zlib stream header. :-). 0x78 indicates DEFLATE compression (8) with
            // a sliding window of 2^7 (32 kB). 0xDA includes a check bit for the header and
            // an indication that we're using the optimal compression algorithm.
            compressedStream.Write(new byte[] { 0x78, 0xDA }, 0, 2);

            // Apply filtering. Both byte[]s always represent the unfiltered scanline.
            byte[] scanline = new byte[imageInfo.Width * pixelWidth],
                   previousScanline = null;
            for (int i = 0; i < imageInfo.Height; i++) {
                await readerStream.ReadAsync(scanline, 0, scanline.Length);
                scanlineStream.WriteByte((byte) FilterType);
                await scanlineStream.WriteAsync(Filter(scanline, previousScanline, pixelWidth), 0, scanline.Length);
                previousScanline = scanline;
            }

            // Deflate the data and write it to the final compressed stream.
            scanlineStream.Seek(0, SeekOrigin.Begin);
            using (var ds = new DeflateStream(compressedStream, CompressionLevel.Optimal, true))
                await scanlineStream.CopyToAsync(ds);

            // Write the ADLER32 Zlib checksum
            var adler = Adler32.Compute(scanlineStream.ToArray());
            var adlerBytes = GetBytesForInteger(adler);
            compressedStream.Write(adlerBytes, 0, adlerBytes.Length);

            // Store the compressed data and length.
            compressedData = compressedStream.ToArray();
            compressedLength = compressedData.Length;

            return compressedData;
        }

        private byte[] Filter(byte[] scanline, byte[] previousScanline, int pixelWidth)
        {
            return DefaultFilters.GetFilterForType(FilterType)
                                 .Filter(scanline, previousScanline, pixelWidth);
        }

        int GetPixelWidthForImage()
        {
            switch (imageInfo.ColorType) {
                case 2:
                    return 3;
                case 6:
                    return 4;
                default:
                    throw new Exception($"Don't know how to deal with color type {imageInfo.ColorType}.");
            }
        }
    }
}
