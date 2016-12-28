using System;
using System.IO;
using System.Threading.Tasks;

using Xunit;

using Pingu.Chunks;
using Pingu.Colors;
using Pingu.Filters;

namespace Pingu.Tests
{
    public class ChunkTests
    {
        class BadChunk : Chunk
        {
            public override int Length => 10;
            public override string Name => "ibad";
            protected override Task<byte[]> GetChunkDataAsync() => Task.FromResult(Array.Empty<byte>());
        }

        [Fact]
        public async Task Chunk_throws_if_length_mismatch()
        {
            var stream = new MemoryStream();
            var chunk = new BadChunk();
            await Assert.ThrowsAsync<Exception>(async () => await chunk.WriteSelfToStreamAsync(stream));
        }

        [Fact]
        public void Idat_throws_exception_for_garbage_filter_type()
        {
            var arex = Assert.Throws<ArgumentOutOfRangeException>(() => new IdatChunk(
                new IhdrChunk(1, 1, 8, ColorType.Grayscale), 
                new byte[0], 
                (FilterType) (10)
            ));
            Assert.Equal("filterType", arex.ParamName);
        }

        [Fact]
        public void Ihdr_throws_for_unsupported_bit_depth()
        {
            var arex = Assert.Throws<ArgumentException>(() => new IhdrChunk(1, 1, 11, ColorType.Grayscale));
            Assert.Equal("bitDepth", arex.ParamName);
        }

        [Fact]
        public void Ihdr_throws_for_unsupported_width()
        {
            var arex = Assert.Throws<ArgumentOutOfRangeException>(() => new IhdrChunk(0, 1, 8, ColorType.Grayscale));
            Assert.Equal("width", arex.ParamName);
        }

        [Fact]
        public void Ihdr_throws_for_unsupported_height()
        {
            var arex = Assert.Throws<ArgumentOutOfRangeException>(() => new IhdrChunk(1, 0, 8, ColorType.Grayscale));
            Assert.Equal("height", arex.ParamName);
        }

        [Theory]
        [InlineData(ColorType.Grayscale)]
        [InlineData(ColorType.GrayscaleAlpha)]
        [InlineData(ColorType.Indexed)]
        public void Idat_throws_for_unsupported_color_type(ColorType colorType)
        {
            var arex = Assert.Throws<ArgumentException>(() => new IdatChunk (
                new IhdrChunk (1, 1, 8, colorType),
                Array.Empty<byte>(),
                FilterType.Dynamic
            ));
        }

        [Fact]
        public void Ihdr_throws_for_garbage_color_type()
        {
            var arex = Assert.Throws<ArgumentOutOfRangeException>(() => new IhdrChunk (
                1,
                1,
                16,
                (ColorType)10
            ));
            Assert.Equal("colorType", arex.ParamName);
        }

        [Theory]
        [InlineData(ColorType.Indexed,        1,  false)]
        [InlineData(ColorType.Indexed,        2,  false)]
        [InlineData(ColorType.Indexed,        4,  false)]
        [InlineData(ColorType.Indexed,        8,  false)]
        [InlineData(ColorType.Indexed,        16, true)]
        [InlineData(ColorType.Grayscale,      1,  false)]
        [InlineData(ColorType.Grayscale,      2,  false)]
        [InlineData(ColorType.Grayscale,      4,  false)]
        [InlineData(ColorType.Grayscale,      8,  false)]
        [InlineData(ColorType.Grayscale,      16, false)]
        [InlineData(ColorType.GrayscaleAlpha, 1,  true)]
        [InlineData(ColorType.GrayscaleAlpha, 2,  true)]
        [InlineData(ColorType.GrayscaleAlpha, 4,  true)]
        [InlineData(ColorType.GrayscaleAlpha, 8,  false)]
        [InlineData(ColorType.GrayscaleAlpha, 16, false)]
        [InlineData(ColorType.Truecolor,      1,  true)]
        [InlineData(ColorType.Truecolor,      2,  true)]
        [InlineData(ColorType.Truecolor,      4,  true)]
        [InlineData(ColorType.Truecolor,      8,  false)]
        [InlineData(ColorType.Truecolor,      16, false)]
        [InlineData(ColorType.TruecolorAlpha, 1,  true)]
        [InlineData(ColorType.TruecolorAlpha, 2,  true)]
        [InlineData(ColorType.TruecolorAlpha, 4,  true)]
        [InlineData(ColorType.TruecolorAlpha, 8,  false)]
        [InlineData(ColorType.TruecolorAlpha, 16, false)]
        public void Ihdr_accepts_valid_bit_depths_for_color_type(ColorType colorType, int bitDepth, bool throws)
        {
            if (throws) {
                Assert.Throws<Exception>(() => new IhdrChunk (
                    1,
                    1,
                    bitDepth,
                    colorType
                ));
            } else {
                var chunk = new IhdrChunk (1, 1, bitDepth, colorType);
            }
        }
    }
}
