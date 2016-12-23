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
    public class UpImplementations
    {
        [Params(5000)]
        public int TotalBytes { get; set; }

        public byte[] TargetBuffer { get; set; }
        public byte[] RawScanline { get; set; }
        public byte[] PreviousScanline { get; set; }

        static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        [Setup]
        public void Setup()
        {
            TargetBuffer = new byte[TotalBytes];
            RawScanline = new byte[TotalBytes];
            PreviousScanline = new byte[TotalBytes];
            rng.GetBytes(RawScanline);
            rng.GetBytes(PreviousScanline);
        }

        [Benchmark(Baseline = true)]
        public unsafe void Naive()
        {
            int targetOffset = 0;

            for (var i = 0; i < RawScanline.Length; i++)
                TargetBuffer[i + targetOffset] = (byte)((RawScanline[i] - PreviousScanline[i]) % 256);
        }

        [Benchmark]
        public unsafe void PointersOnly()
        {
            int targetOffset = 0;

            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* target = TargetBuffer) {
                for (var i = 0; i < RawScanline.Length; i++)
                    target[i + targetOffset] = (byte)((raw[i] - previous[i]) % 256);
            }
        }

        [Benchmark]
        public unsafe void PointersUnrolled()
        {
            int targetOffset = 0;

            fixed (byte* raw = RawScanline)
            fixed (byte* previous = PreviousScanline)
            fixed (byte* target = TargetBuffer) {
                for (var i = 0; RawScanline.Length - i > 8; i += 8) {
                    target[i + targetOffset] = (byte)((raw[i] - previous[i]) % 256);
                    target[i + 1 + targetOffset] = (byte)((raw[i + 1] - previous[i + 1]) % 256);
                    target[i + 2 + targetOffset] = (byte)((raw[i + 2] - previous[i + 2]) % 256);
                    target[i + 3 + targetOffset] = (byte)((raw[i + 3] - previous[i + 3]) % 256);
                    target[i + 4 + targetOffset] = (byte)((raw[i + 4] - previous[i + 4]) % 256);
                    target[i + 5 + targetOffset] = (byte)((raw[i + 5] - previous[i + 5]) % 256);
                    target[i + 6 + targetOffset] = (byte)((raw[i + 6] - previous[i + 6]) % 256);
                    target[i + 7 + targetOffset] = (byte)((raw[i + 7] - previous[i + 7]) % 256);
                }

                for (var i = RawScanline.Length - 8; i < RawScanline.Length; i++)
                    target[i + targetOffset] = (byte)((raw[i] - previous[i]) % 256);
            }
        }

        [Benchmark]
        public unsafe void VectorAndPointer()
        {
            int vecSize = Vector<byte>.Count, length = TotalBytes;
            int targetOffset = 0;

            var chunks = (int)Math.Floor((double)(length / vecSize));

            fixed (byte* rawPtr = RawScanline) {
                fixed (byte* prevPtr = PreviousScanline) {
                    fixed (byte* targetPtr = TargetBuffer) {
                        for (int i = 0; i < chunks; i++) {
                            int src = i * vecSize;
                            var vec = (new Vector<byte>(RawScanline, src) - new Vector<byte>(PreviousScanline, src));
                            vec.CopyTo(TargetBuffer, src + targetOffset);
                        }

                        int start = vecSize * chunks + targetOffset;
                        for (int i = start; i < length; i++)
                            targetPtr[i] = unchecked((byte)(rawPtr[i] - prevPtr[i]));
                    }
                }
            }
        }
    }
}
