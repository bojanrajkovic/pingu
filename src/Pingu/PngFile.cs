﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Pingu.Chunks;

namespace Pingu
{

    public class PngFile : IEnumerable<Chunk>
    {
        readonly List<Chunk> chunksToWrite = new List<Chunk>();
        static readonly byte[] Magic = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };

        public void Add(Chunk chunk) => chunksToWrite.Add(chunk);

        IEnumerator<Chunk> IEnumerable<Chunk>.GetEnumerator() => chunksToWrite.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => chunksToWrite.GetEnumerator();

        public int ChunkCount => chunksToWrite.Count;

        public async Task WriteFileAsync(Stream target)
        {
            await target.WriteAsync(Magic, 0, Magic.Length).ConfigureAwait(false);
            foreach (var chunk in this)
                await chunk.WriteSelfToStreamAsync(target).ConfigureAwait(false);
        }
    }
}
