using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

using Pingu.Checksums;
using Pingu.Colors;
using Pingu.Filters;

namespace Pingu.Chunks
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

            if (imageInfo.ColorType != ColorType.Truecolor && imageInfo.ColorType != ColorType.TruecolorAlpha)
                throw new ArgumentException("Pingu currently does not support anything other than " +
                                            "truecolor (with optional alpha) images.");

            FilterType = filterType;
        }

        protected override Task<byte[]> GetChunkDataAsync() => GetCompressedDataAsync();

        // TODO: Break this up into several methods for different types of images. Palletized images
        // require a slightly different process than truecolor, and grayscale and alpha require a yet
        // again different process. Currently, this only handles truecolor images of 8/16 bit depth.

        public async Task<byte[]> GetCompressedDataAsync()
        {
            if (compressedData != null)
                return compressedData;

            int pixelWidth = (imageInfo.ColorType == ColorType.Truecolor ? 3 : 4) *
                             imageInfo.BitDepth/8;

            // Set up a couple of streams where we're going to hold our data.
            // We need a stream for the final data and a deflate wrapper over it
            var compressedStream = new MemoryStream();
            var deflateStream = new DeflateStream(compressedStream, CompressionLevel.Optimal, true);

            // Write the zlib stream header. :-). 0x78 indicates DEFLATE compression (8) with
            // a sliding window of 2^7 (32 kB). 0xDA includes a check bit for the header and
            // an indication that we're using the optimal compression algorithm and default
            // Huffman tree.
            compressedStream.Write(new byte[] { 0x78, 0xDA }, 0, 2);

            var adler = new Adler32();

            // Apply filtering. Both byte[]s always represent the unfiltered scanline. Allocations
            // and copies are the devil, so we do as few as possible here. We do no more than 3 allocations
            // no matter what. We do 2*Height and change (some filters might copy a few bytes)
            // copies, as well, but dynamic filtering repeats a lot of filter work. Luckily, filters
            // are fast.
            byte[] previousScanline = null, scanline = new byte[imageInfo.Width * pixelWidth],
                   scanlineToWrite = new byte[1 + imageInfo.Width * pixelWidth];

            var tempStream = new MemoryStream();

            // TODO: Someday, instead of writing to a stream, do this all with direct offsets
            // into an array. We should be able to preallocate an array of the correct length
            // and simply write the bytes and update offsets.
            for (int i = 0; i < imageInfo.Height; i++) {
                // Copy raw RGB data to scanline.
                Buffer.BlockCopy(rawRgbData, i * scanline.Length, scanline, 0, scanline.Length);

                // Filter scanline into scanlineToWrite.
                FilterInto(scanlineToWrite, 1, scanline, previousScanline, pixelWidth);

                // Write scanline to stream.
                await tempStream.WriteAsync(scanlineToWrite, 0, scanlineToWrite.Length);

                // Allocate previous scanline if needed.
                previousScanline = previousScanline ?? new byte[scanline.Length];

                // Copy current scanline onto previous scanline.
                Buffer.BlockCopy(scanline, 0, previousScanline, 0, scanline.Length);
            }

            var data = tempStream.ToArray();
            adler.FeedBlock(data);
            await deflateStream.WriteAsync(data, 0, data.Length);

            tempStream.Dispose();
            deflateStream.Dispose();

            // Write the ADLER32 Zlib checksum
            var adlerBytes = GetBytesForInteger(adler.Hash);
            compressedStream.Write(adlerBytes, 0, adlerBytes.Length);

            // Store the compressed data and length.
            compressedData = compressedStream.ToArray();
            compressedLength = compressedData.Length;

            compressedStream.Dispose();

            return compressedData;
        }

        void FilterInto(
            byte[] scanlineToWrite,
            int targetOffset,
            byte[] rawScanline,
            byte[] previousScanline,
            int pixelWidth) {
                var filter = DefaultFilters.GetFilterForType(FilterType);
                scanlineToWrite[targetOffset-1] = (byte)filter.Type;
                filter.FilterInto(scanlineToWrite, targetOffset, rawScanline, previousScanline, pixelWidth);
            }
    }
}