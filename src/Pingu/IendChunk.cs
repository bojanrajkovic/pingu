using System;
using System.Threading.Tasks;

namespace Pingu
{

    public class IendChunk : Chunk
    {
        public override string Name => "IEND";
        public override int Length => 0;

        protected override Task<byte[]> GetChunkDataAsync() => Task.FromResult(Array.Empty<byte>());
    }
}
