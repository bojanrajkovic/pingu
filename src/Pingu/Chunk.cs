using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pingu
{
    public abstract class Chunk
    {
        public async Task WriteSelfToStreamAsync(Stream stream)
        {
            var chunkType = Encoding.ASCII.GetBytes(Name);
            var data = await GetChunkDataAsync();
            var length = GetBytesForInteger(Length);

            if (Length != data.Length)
                throw new Exception("Chunk claimed length does not match data array length.");

            var chunkTypeAndData = new byte[chunkType.Length + data.Length];
            Buffer.BlockCopy(chunkType, 0, chunkTypeAndData, 0, chunkType.Length);
            Buffer.BlockCopy(data, 0, chunkTypeAndData, chunkType.Length, data.Length);
            var crc32 = await CalculateCRC32Async(chunkTypeAndData);
            Array.Reverse(crc32);

            await stream.WriteAsync(length, 0, length.Length);
            await stream.WriteAsync(chunkType, 0, chunkType.Length);
            await stream.WriteAsync(data, 0, data.Length);
            await stream.WriteAsync(crc32, 0, crc32.Length);
        }

        async Task<byte[]> CalculateCRC32Async(byte[] data) 
            => BitConverter.GetBytes(await Crc32.ComputeAsync(data));

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

        protected byte[] GetBytesForInteger(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }
    }
}
