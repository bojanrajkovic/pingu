using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Pingu.Chunks
{
    public abstract class Chunk
    {
        internal async Task WriteSelfToStreamAsync(Stream stream)
        {
            var chunkType = new byte[] { (byte)Name[0], (byte)Name[1], (byte)Name[2], (byte)Name[3] };
            var data = await GetChunkDataAsync();
            var length = GetBytesForInteger(Length);

            if (Length != data.Length)
                throw new Exception("Chunk claimed length does not match data array length.");

            var chunkTypeAndData = new byte[chunkType.Length + data.Length];
            Buffer.BlockCopy(chunkType, 0, chunkTypeAndData, 0, chunkType.Length);
            Buffer.BlockCopy(data, 0, chunkTypeAndData, chunkType.Length, data.Length);
            var crc32 = CalculateCRC32(chunkTypeAndData);

            await stream.WriteAsync(length, 0, length.Length);
            await stream.WriteAsync(chunkType, 0, chunkType.Length);
            await stream.WriteAsync(data, 0, data.Length);
            await stream.WriteAsync(crc32, 0, crc32.Length);
        }

        byte[] CalculateCRC32(byte[] data) => GetBytesForInteger((int) Crc32Helper.UpdateCrc32(0, data, 0, data.Length));

        public abstract string Name { get; }
        public abstract int Length { get; }

        protected abstract Task<byte[]> GetChunkDataAsync();

        protected byte[] GetBytesForInteger(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }
    }
}
