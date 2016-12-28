using System;
using System.IO;
using System.Threading.Tasks;

using Xunit;

using Pingu.Chunks;
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
    }
}
