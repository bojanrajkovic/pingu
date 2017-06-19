using System;
using System.Linq;
using BenchmarkDotNet.Running;

using Pingu.Benchmarks;
using Pingu.Benchmarks.ImplementationBenchmarks;

class Program
{
    public static bool IsCIBuild { get; set; }

    static void Main(string[] args)
    {
        // TestAdler ();
        // TestCrc32();
        // TestSub();
        // TestUp();
        // TestAvg();
        // TestPaeth();
        // TestMinSad();
        // TestFloor();

        IsCIBuild = args.Length > 0 && args[0] == "--ci";

        if (!IsCIBuild) {
            // These are all the various benchmarks written as part of testing
            // fast implementations for parts of PNG.
            var switcher = new BenchmarkSwitcher(new[] {
                typeof (Crc32Implementations),
                typeof (Adler32Implementations),
                typeof (MinSadImplementations),
                typeof (FloorImplementations),
                typeof (SubImplementations),
                typeof (UpImplementations),
                typeof (AvgImplementations),
                typeof (PaethImplementations),
                typeof (UpFilterBenchmark)
            });

            switcher.Run(args);
        } else {
            // We want to run these benchmarks in CI to catch perf regressions.
            BenchmarkRunner.Run<UpFilterBenchmark>();
            BenchmarkRunner.Run<AvgFilterBenchmark>();
            BenchmarkRunner.Run<SubFilterBenchmark>();
            BenchmarkRunner.Run<PaethFilterBenchmark>();
            BenchmarkRunner.Run<DynamicFilterBenchmark>();
            /*BenchmarkRunner.Run<PngFileBenchmark>();*/
        }
    }

    static void TestFloor()
    {
        var floor = new FloorImplementations { FloatsToFloor = 10 };
        floor.Setup();

        int[] mathFloor = new int[floor.FloatsToFloor], fastFloor = new int[floor.FloatsToFloor];

        floor.MathFloor();
        Buffer.BlockCopy(floor.FlooredData, 0, mathFloor, 0, floor.FloatsToFloor);

        floor.FastFloor();
        Buffer.BlockCopy(floor.FlooredData, 0, fastFloor, 0, floor.FloatsToFloor);

        SequenceEqualFloor(mathFloor, fastFloor, floor.FloorData);
    }

    static void TestMinSad()
    {
        var minsad = new MinSadImplementations { TotalBytes = 27 };
        minsad.Setup();

        Console.WriteLine($"Normal: {minsad.ByteByByte()} - Signed: {minsad.SignedBytesByByteFastAbs()} - Unrolled 8: {minsad.SignedBytesUnrolledFastAbs8()}");
        Console.WriteLine($"Unrolled 16: {minsad.SignedBytesUnrolledFastAbs16()}");
    }

    static void TestPaeth()
    {
        var paeth = new PaethImplementations { TotalBytes = 5000, HasPreviousScanline = true, BytesPerPixel = 4 };
        byte[] naiveMath = new byte[paeth.TotalBytes], naiveFast = new byte[paeth.TotalBytes],
               naiveVec = new byte[paeth.TotalBytes], unrolledMath = new byte[paeth.TotalBytes],
               unrolledFast = new byte[paeth.TotalBytes], unrolledVec = new byte[paeth.TotalBytes],
               unrolledMotion = new byte[paeth.TotalBytes], fasterAbs = new byte[paeth.TotalBytes],
               unrolledLessMath = new byte[paeth.TotalBytes];

        paeth.Setup();

        paeth.NaiveWithMathAbs();
        Buffer.BlockCopy(paeth.TargetBuffer, 0, naiveMath, 0, paeth.TotalBytes);

        paeth.NaiveWithFastAbs();
        Buffer.BlockCopy(paeth.TargetBuffer, 0, naiveFast, 0, paeth.TotalBytes);

        paeth.NaiveWithVecAbs();
        Buffer.BlockCopy(paeth.TargetBuffer, 0, naiveVec, 0, paeth.TotalBytes);

        paeth.UnrolledWithMathAbs();
        Buffer.BlockCopy(paeth.TargetBuffer, 0, unrolledMath, 0, paeth.TotalBytes);

        paeth.UnrolledWithFastAbs();
        Buffer.BlockCopy(paeth.TargetBuffer, 0, unrolledFast, 0, paeth.TotalBytes);

        paeth.UnrolledWithVecAbs();
        Buffer.BlockCopy(paeth.TargetBuffer, 0, unrolledVec, 0, paeth.TotalBytes);

        paeth.UnrolledWithFastAbsAndMovingPointers();
        Buffer.BlockCopy(paeth.TargetBuffer, 0, unrolledMotion, 0, paeth.TotalBytes);

        paeth.UnrolledWithFasterAbsAndMovingPointers();
        Buffer.BlockCopy(paeth.TargetBuffer, 0, fasterAbs, 0, paeth.TotalBytes);

        paeth.UnrolledWithFasterAbsLessArithmeticAndMovingPointers();
        Buffer.BlockCopy(paeth.TargetBuffer, 0, unrolledLessMath, 0, paeth.TotalBytes);

        SequenceEqualUp(naiveMath, naiveFast, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, naiveVec, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, unrolledMath, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, unrolledFast, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, unrolledVec, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, unrolledMotion, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, fasterAbs, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, unrolledLessMath, paeth.RawScanline, paeth.PreviousScanline);
    }

    static void TestUp()
    {
        var up = new UpImplementations { TotalBytes = 5000 };
        byte[] naive = new byte[up.TotalBytes], ptr = new byte[up.TotalBytes], unr = new byte[up.TotalBytes],
               vec = new byte[up.TotalBytes], unro = new byte[up.TotalBytes], motion = new byte[up.TotalBytes];

        up.Setup();

        up.Naive();
        Buffer.BlockCopy(up.TargetBuffer, 0, naive, 0, up.TotalBytes);

        up.Pointers();
        Buffer.BlockCopy(up.TargetBuffer, 0, ptr, 0, up.TotalBytes);

        up.PointersUnrolled();
        Buffer.BlockCopy(up.TargetBuffer, 0, unr, 0, up.TotalBytes);

        up.PointersUnrolledPreOffset();
        Buffer.BlockCopy(up.TargetBuffer, 0, unro, 0, up.TotalBytes);

        up.PointersUnrolledPreOffsetMotion();
        Buffer.BlockCopy(up.TargetBuffer, 0, motion, 0, up.TotalBytes);

        up.VectorAndPointer();
        Buffer.BlockCopy(up.TargetBuffer, 0, vec, 0, up.TotalBytes);

        SequenceEqualUp(naive, ptr, up.RawScanline, up.PreviousScanline);
        SequenceEqualUp(naive, unr, up.RawScanline, up.PreviousScanline);
        SequenceEqualUp(naive, unro, up.RawScanline, up.PreviousScanline);
        SequenceEqualUp(naive, motion, up.RawScanline, up.PreviousScanline);
        SequenceEqualUp(naive, vec, up.RawScanline, up.PreviousScanline);
    }

    static void TestAvg()
    {
        var avg = new AvgImplementations { TotalBytes = 5000, BytesPerPixel = 4, HasPreviousScanline = true };
        byte[] naive = new byte[avg.TotalBytes], naiveLoops = new byte[avg.TotalBytes], pointer = new byte[avg.TotalBytes],
               unrolled = new byte[avg.TotalBytes], vec = new byte[avg.TotalBytes], motion = new byte [avg.TotalBytes];

        avg.Setup();

        avg.NaiveWithNullablePrevious();
        Buffer.BlockCopy(avg.TargetBuffer, 0, naive, 0, avg.TotalBytes);

        avg.NaiveWithSeparateLoops();
        Buffer.BlockCopy(avg.TargetBuffer, 0, naiveLoops, 0, avg.TotalBytes);

        avg.Pointer();
        Buffer.BlockCopy(avg.TargetBuffer, 0, pointer, 0, avg.TotalBytes);

        avg.PointerUnrolled();
        Buffer.BlockCopy(avg.TargetBuffer, 0, unrolled, 0, avg.TotalBytes);

        avg.SmartVectorized();
        Buffer.BlockCopy(avg.TargetBuffer, 0, vec, 0, avg.TotalBytes);

        avg.PointerUnrolledMotion();
        Buffer.BlockCopy(avg.TargetBuffer, 0, motion, 0, avg.TotalBytes);

        SequenceEqualUp(naive, naiveLoops, avg.RawScanline, avg.PreviousScanline);
        SequenceEqualUp(naive, pointer, avg.RawScanline, avg.PreviousScanline);
        SequenceEqualUp(naive, unrolled, avg.RawScanline, avg.PreviousScanline);
        SequenceEqualUp(naive, vec, avg.RawScanline, avg.PreviousScanline);
        SequenceEqualUp(naive, motion, avg.RawScanline, avg.PreviousScanline);
    }

    static void TestSub()
    {
        var sub = new SubImplementations { BytesPerPixel = 4, TotalBytes = 8 };
        byte[] naive = new byte[sub.TotalBytes], pointer = new byte[sub.TotalBytes], unrolled = new byte[sub.TotalBytes],
               preoffset = new byte[sub.TotalBytes], vec = new byte[sub.TotalBytes], motion = new byte[sub.TotalBytes];

        sub.Setup();

        sub.Naive();
        Buffer.BlockCopy(sub.TargetBuffer, 0, naive, 0, sub.TotalBytes);

        sub.Pointers();
        Buffer.BlockCopy(sub.TargetBuffer, 0, pointer, 0, sub.TotalBytes);

        sub.PointersUnrolled();
        Buffer.BlockCopy(sub.TargetBuffer, 0, unrolled, 0, sub.TotalBytes);

        sub.PointersUnrolledPreOffset();
        Buffer.BlockCopy(sub.TargetBuffer, 0, preoffset, 0, sub.TotalBytes);

        sub.VectorAndPointer();
        Buffer.BlockCopy(sub.TargetBuffer, 0, vec, 0, sub.TotalBytes);

        sub.PointersUnrolledPreOffsetMotion();
        Buffer.BlockCopy(sub.TargetBuffer, 0, motion, 0, sub.TotalBytes);

        SequenceEqualSub(naive, pointer, sub.RawScanline);
        SequenceEqualSub(naive, unrolled, sub.RawScanline);
        SequenceEqualSub(naive, preoffset, sub.RawScanline);
        SequenceEqualSub(naive, vec, sub.RawScanline);
        SequenceEqualSub(naive, motion, sub.RawScanline);
    }

    static void TestCrc32()
    {
        var crc32 = new Crc32Implementations() { TotalBytes = 5000 };
        crc32.Setup();

        var ours = (uint)crc32.MyCRC32();
        var corefx = crc32.CoreFxCRC32();

        Console.WriteLine($"Ours: {ours} - CoreFX: {corefx}");
    }

    static void TestAdler()
    {
        var adler = new Adler32Implementations();
        adler.SetupData();

        var smarter = adler.Smarter();
        var known = adler.UsePointer();
        var smartest = adler.Smartest();

        Console.WriteLine($"Smartest: {smartest} - Smarter: {smarter} - Known: {known}");
    }

    static void SequenceEqualFloor(int[] expected, int[] actual, float[] data)
    {
        int i = 0;
        bool equal = true;

        for (; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                equal = false;
                break;
            }
        }

        if (!equal)
        {
            Console.WriteLine("Sequences are not equal from test methods, please check implementation.");
            Console.WriteLine($"Sequences differ at index {i}, expected {expected[i]}, actual {actual[i]}");
        }
        else
        {
            Console.WriteLine("Sequences are equal, press any key to proceed.");
        }
    }

    static void SequenceEqualSub(byte[] expected, byte[] actual, byte[] data)
    {
        int i = 0;
        bool equal = true;

        for (; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                equal = false;
                break;
            }
        }

        if (!equal)
        {
            Console.WriteLine("Sequences are not equal from test methods, please check implementation.");
            Console.WriteLine($"Sequences differ at index {i}, expected {expected[i]}, actual {actual[i]}");
            if (expected.Length < 99)
            {
                Console.WriteLine("Printing data, then expected, then actual.");
                Console.WriteLine(string.Join("|", Enumerable.Range(0, expected.Length).Select(x => x.ToString("00"))));
                Console.WriteLine(string.Join("|", Enumerable.Repeat("==", expected.Length)));
                Console.WriteLine(BitConverter.ToString(data).Replace("-", "|"));
                Console.WriteLine(BitConverter.ToString(expected).Replace("-", "|"));
                Console.WriteLine(BitConverter.ToString(actual).Replace("-", "|"));
            }
        }
        else
        {
            Console.WriteLine("Sequences are equal, press any key to proceed.");
        }
    }

    static void SequenceEqualUp(byte[] expected, byte[] actual, byte[] data, byte[] previous)
    {
        int i = 0;
        bool equal = true;

        for (; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                equal = false;
                break;
            }
        }

        if (!equal)
        {
            Console.WriteLine("Sequences are not equal from test methods, please check implementation.");
            Console.WriteLine($"Sequences differ at index {i}, expected {expected[i]}, actual {actual[i]}");
            if (expected.Length < 99)
            {
                Console.WriteLine("Printing data, then previous, then expected, then actual.");
                Console.WriteLine(string.Join("-", Enumerable.Range(0, expected.Length).Select(x => x.ToString("00"))));
                Console.WriteLine(BitConverter.ToString(data));
                Console.WriteLine(BitConverter.ToString(previous));
                Console.WriteLine(BitConverter.ToString(expected));
                Console.WriteLine(BitConverter.ToString(actual));
            }
        }
        else
        {
            Console.WriteLine("Sequences are equal, press any key to proceed.");
        }
    }
}