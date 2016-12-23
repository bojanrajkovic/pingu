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

        public byte[] Data { get; set; }

        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [Setup]
        public void Setup()
        {
            Data = new byte[TotalBytes];
            rng.GetBytes(Data);
        }

        [Benchmark]
        public unsafe byte[] PointersOnly()
        {
            byte[] targetBuffer = new byte[Data.Length];
            fixed (byte* targetPtr = targetBuffer) {
                fixed (byte* scanlinePtr = Data) {
                    Buffer.MemoryCopy(scanlinePtr, targetPtr, Data.Length, BytesPerPixel);

                    unchecked {
                        // We start immediately after the first pixel--its bytes are unchanged. We only copied
                        // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline.
                        for (var x = BytesPerPixel; x < Data.Length; x++)
                            targetPtr[x] = (byte)((scanlinePtr[x] - scanlinePtr[x - BytesPerPixel]) % 256);
                    }
                }
            }
            return targetBuffer;
        }

        [Benchmark]
        public unsafe byte[] PointersOnlyUnrolled()
        {
            byte[] targetBuffer = new byte[Data.Length];
            fixed (byte* targetPtr = targetBuffer) {
                fixed (byte* scanlinePtr = Data) {
                    Buffer.MemoryCopy(scanlinePtr, targetPtr, Data.Length, BytesPerPixel);

                    unchecked {
                        // We start immediately after the first pixel--its bytes are unchanged. We only copied
                        // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline. Unroll
                        // the loop a bit, as well.
                        for (var x = BytesPerPixel; x < Data.Length - 8; x += 8) {
                            targetPtr[x] = (byte)((scanlinePtr[x] - scanlinePtr[x - BytesPerPixel]) % 256);
                            targetPtr[x + 1] = (byte)((scanlinePtr[x + 1] - scanlinePtr[x + 1 - BytesPerPixel]) % 256);
                            targetPtr[x + 2] = (byte)((scanlinePtr[x + 2] - scanlinePtr[x + 2 - BytesPerPixel]) % 256);
                            targetPtr[x + 3] = (byte)((scanlinePtr[x + 3] - scanlinePtr[x + 3 - BytesPerPixel]) % 256);
                            targetPtr[x + 4] = (byte)((scanlinePtr[x + 4] - scanlinePtr[x + 4 - BytesPerPixel]) % 256);
                            targetPtr[x + 5] = (byte)((scanlinePtr[x + 5] - scanlinePtr[x + 5 - BytesPerPixel]) % 256);
                            targetPtr[x + 6] = (byte)((scanlinePtr[x + 6] - scanlinePtr[x + 6 - BytesPerPixel]) % 256);
                            targetPtr[x + 7] = (byte)((scanlinePtr[x + 7] - scanlinePtr[x + 7 - BytesPerPixel]) % 256);
                        }

                        for (var x = Data.Length - 8; x < Data.Length; x++)
                            targetPtr[x] = (byte)((scanlinePtr[x] - scanlinePtr[x - BytesPerPixel]) % 256);
                    }
                }
            }
            return targetBuffer;
        }

        [Benchmark(Baseline = true)]
        public byte[] Naive()
        {
            byte[] targetBuffer = new byte[Data.Length];
            Buffer.BlockCopy(Data, 0, targetBuffer, 0, BytesPerPixel);

            unchecked {
                // We start immediately after the first pixel--its bytes are unchanged. We only copied
                // bytesPerPixel bytes from the scanline, so we need to read over the raw scanline.
                for (var x = BytesPerPixel; x < Data.Length; x++)
                    targetBuffer[x] = (byte)((Data[x] - Data[x - BytesPerPixel]) % 256);
            }

            return targetBuffer;
        }

        [Benchmark]
        public unsafe byte[] VectorAndPointer()
        {
            int vecSize = Vector<byte>.Count, length = Data.Length;
            byte[] result = new byte[length];

            var chunks = (int)Math.Floor((double)(length - BytesPerPixel) / vecSize);

            fixed (byte* dataPtr = Data) {
                fixed (byte* resultPtr = result) {
                    Buffer.MemoryCopy(dataPtr, resultPtr, length, BytesPerPixel);

                    for (int i = 0; i < chunks; i++) {
                        int src = i * vecSize, dst = src + BytesPerPixel;
                        var vec = new Vector<byte>(Data, dst) - new Vector<byte>(Data, src);
                        vec.CopyTo(result, dst);
                    }

                    int start = BytesPerPixel + (vecSize * chunks);
                    for (int i = start; i < length; i++)
                        resultPtr[i] = unchecked((byte)(dataPtr[i] - dataPtr[i - BytesPerPixel]));
                }
            }

            return result;
        }
    }
}
