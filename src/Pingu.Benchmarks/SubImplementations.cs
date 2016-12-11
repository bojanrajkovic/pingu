using System;
using System.Numerics;
using System.Security.Cryptography;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;

namespace Pingu.Benchmarks
{

    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(NumeralSystem.Stars)]
    [LegacyJitX64Job, RyuJitX64Job, MonoJob]
    public class SubImplementations
    {
        [Params(5000)]
        public int TotalBytes { get; set; }

        [Params(4)]
        public int BytesPerPixel { get; set; }

        byte[] data;

        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [Setup]
        public void Setup()
        {
            data = new byte[TotalBytes];
            rng.GetBytes(data);
        }

        [Benchmark]
        public unsafe byte[] PointersOnly()
        {
            byte[] targetBuffer = new byte[data.Length];
            fixed (byte* targetPtr = targetBuffer) {
                fixed (byte* scanlinePtr = data) {
                    Buffer.MemoryCopy(scanlinePtr, targetPtr, data.Length, BytesPerPixel);

                    unchecked {
                        // We start immediately after the first pixel--its bytes are unchanged. We only copied
                        // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline.
                        for (var x = BytesPerPixel; x < data.Length; x++)
                            targetPtr[x] = (byte)((scanlinePtr[x] - scanlinePtr[x - BytesPerPixel]) % 256);
                    }
                }
            }
            return targetBuffer;
        }

        [Benchmark(Baseline = true)]
        public byte[] Naive()
        {
            byte[] targetBuffer = new byte[data.Length];
            Buffer.BlockCopy(data, 0, targetBuffer, 0, BytesPerPixel);

            unchecked {
                // We start immediately after the first pixel--its bytes are unchanged. We only copied
                // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline.
                for (var x = BytesPerPixel; x < data.Length; x++)
                    targetBuffer[x] = (byte)((data[x] - data[x - BytesPerPixel]) % 256);
            }

            return targetBuffer;
        }

        [Benchmark]
        public unsafe byte[] VectorAndPointer()
        {
            var vecSize = Vector<byte>.Count;
            byte[] result = new byte[data.Length];

            var chunks = (int)Math.Floor((float)(data.Length - BytesPerPixel) / vecSize);

            fixed (byte* dataPtr = data) {
                fixed (byte* resultPtr = result) {
                    Buffer.MemoryCopy(dataPtr, resultPtr, data.Length, BytesPerPixel);

                    for (int i = 0; i < chunks; i++) {
                        int target = BytesPerPixel + i * vecSize;
                        var vec = new Vector<byte>(data, target) - new Vector<byte>(data, i * vecSize);
                        vec.CopyTo(result, target);
                    }

                    int start = (BytesPerPixel + (vecSize * chunks));
                    for (int i = start; i < data.Length; i++)
                        resultPtr[i] = unchecked((byte)(dataPtr[i] - dataPtr[i - BytesPerPixel]));
                }
            }

            return result;
        }
    }
}
