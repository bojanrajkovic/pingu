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
            Assert.Throws<ArgumentOutOfRangeException>(() => new IdatChunk(new IhdrChunk(1, 1, 8), new byte[0], (FilterType) (10)));
        }
    }
}
