using System;
using System.Linq;
using BenchmarkDotNet.Running;

using Pingu.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        // TestAdler ();
        // TestSub();
        // TestUp();
        // TestAvg();
        // TestPaeth();

        var switcher = new BenchmarkSwitcher(new[] {
            typeof (Adler32Implementations),
            typeof (SubImplementations),
            typeof (UpImplementations),
            typeof (AvgImplementations),
            typeof (PaethImplementations)
        });

        switcher.Run(args);
    }

    static void TestPaeth()
    {
        var paeth = new PaethImplementations { TotalBytes = 5000, HasPreviousScanline = true, BytesPerPixel = 4 };
        byte[] naiveMath = new byte[paeth.TotalBytes], naiveFast = new byte[paeth.TotalBytes],
               naiveVec = new byte[paeth.TotalBytes], unrolledMath = new byte[paeth.TotalBytes],
               unrolledFast = new byte[paeth.TotalBytes], unrolledVec = new byte[paeth.TotalBytes],
               unrolledMotion = new byte[paeth.TotalBytes];

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

        SequenceEqualUp(naiveMath, naiveFast, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, naiveVec, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, unrolledMath, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, unrolledFast, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, unrolledVec, paeth.RawScanline, paeth.PreviousScanline);
        SequenceEqualUp(naiveMath, unrolledMotion, paeth.RawScanline, paeth.PreviousScanline);
    }

    static void TestUp()
    {
        var up = new UpImplementations { TotalBytes = 5000 };
        byte[] naive = new byte[5000], ptr = new byte[5000], unr = new byte[5000], vec = new byte[5000],
               unro = new byte[5000];

        up.Setup();

        up.Naive();
        Buffer.BlockCopy(up.TargetBuffer, 0, naive, 0, 5000);

        up.Pointers();
        Buffer.BlockCopy(up.TargetBuffer, 0, ptr, 0, 5000);

        up.PointersUnrolled();
        Buffer.BlockCopy(up.TargetBuffer, 0, unr, 0, 5000);

        up.PointersUnrolledPreOffset();
        Buffer.BlockCopy(up.TargetBuffer, 0, unro, 0, 5000);

        up.VectorAndPointer();
        Buffer.BlockCopy(up.TargetBuffer, 0, vec, 0, 5000);

        SequenceEqualUp(naive, ptr, up.RawScanline, up.PreviousScanline);
        SequenceEqualUp(naive, unr, up.RawScanline, up.PreviousScanline);
        SequenceEqualUp(naive, unro, up.RawScanline, up.PreviousScanline);
        SequenceEqualUp(naive, vec, up.RawScanline, up.PreviousScanline);
    }

    static void TestAvg()
    {
        var avg = new AvgImplementations { TotalBytes = 5000, BytesPerPixel = 4, HasPreviousScanline = false };
        byte[] naive = new byte[5000], naiveLoops = new byte[5000], pointer = new byte[5000],
               unrolled = new byte[5000], vec = new byte[5000];

        avg.Setup();

        avg.NaiveWithNullablePrevious();
        Buffer.BlockCopy(avg.TargetBuffer, 0, naive, 0, 5000);

        avg.NaiveWithSeparateLoops();
        Buffer.BlockCopy(avg.TargetBuffer, 0, naiveLoops, 0, 5000);

        avg.Pointer();
        Buffer.BlockCopy(avg.TargetBuffer, 0, pointer, 0, 5000);

        avg.PointerUnrolled();
        Buffer.BlockCopy(avg.TargetBuffer, 0, unrolled, 0, 5000);

        avg.SmartVectorized();
        Buffer.BlockCopy(avg.TargetBuffer, 0, vec, 0, 5000);

        SequenceEqualUp(naive, naiveLoops, avg.RawScanline, avg.PreviousScanline);
        SequenceEqualUp(naive, pointer, avg.RawScanline, avg.PreviousScanline);
        SequenceEqualUp(naive, unrolled, avg.RawScanline, avg.PreviousScanline);
        SequenceEqualUp(naive, vec, avg.RawScanline, avg.PreviousScanline);
    }

    static void TestSub()
    {
        var sub = new SubImplementations { BytesPerPixel = 4, TotalBytes = 5000 };
        byte[] naive = new byte[5000], pointer = new byte[5000], unrolled = new byte[5000],
               preoffset = new byte[5000], vec = new byte[5000];

        sub.Setup();

        sub.Naive();
        Buffer.BlockCopy(sub.TargetBuffer, 0, naive, 0, 5000);

        sub.Pointers();
        Buffer.BlockCopy(sub.TargetBuffer, 0, pointer, 0, 5000);

        sub.PointersUnrolled();
        Buffer.BlockCopy(sub.TargetBuffer, 0, unrolled, 0, 5000);

        sub.PointersUnrolledPreOffset();
        Buffer.BlockCopy(sub.TargetBuffer, 0, preoffset, 0, 5000);

        sub.VectorAndPointer();
        Buffer.BlockCopy(sub.TargetBuffer, 0, vec, 0, 5000);


        SequenceEqualSub(naive, pointer, sub.RawScanline);
        SequenceEqualSub(naive, unrolled, sub.RawScanline);
        SequenceEqualSub(naive, preoffset, sub.RawScanline);
        SequenceEqualSub(naive, vec, sub.RawScanline);
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

    static void SequenceEqualSub(byte[] expected, byte[] actual, byte[] data)
    {
        int i = 0;
        bool equal = true;

        for (; i < expected.Length; i++) {
            if (expected[i] != actual[i]) {
                equal = false;
                break;
            }
        }

        if (!equal) {
            Console.WriteLine("Sequences are not equal from test methods, please check implementation.");
            Console.WriteLine($"Sequences differ at index {i}, expected {expected[i]}, actual {actual[i]}");
            if (expected.Length < 99) {
                Console.WriteLine("Printing data, then expected, then actual.");
                Console.WriteLine(string.Join("-", Enumerable.Range(0, expected.Length).Select(x => x.ToString("00"))));
                Console.WriteLine(BitConverter.ToString(data));
                Console.WriteLine(BitConverter.ToString(expected));
                Console.WriteLine(BitConverter.ToString(actual));
            }
        } else {
            Console.WriteLine("Sequences are equal, press any key to proceed.");
        }
    }

    static void SequenceEqualUp(byte[] expected, byte[] actual, byte[] data, byte[] previous)
    {
        int i = 0;
        bool equal = true;

        for (; i < expected.Length; i++) {
            if (expected[i] != actual[i]) {
                equal = false;
                break;
            }
        }

        if (!equal) {
            Console.WriteLine("Sequences are not equal from test methods, please check implementation.");
            Console.WriteLine($"Sequences differ at index {i}, expected {expected[i]}, actual {actual[i]}");
            if (expected.Length < 99) {
                Console.WriteLine("Printing data, then previous, then expected, then actual.");
                Console.WriteLine(string.Join("-", Enumerable.Range(0, expected.Length).Select(x => x.ToString("00"))));
                Console.WriteLine(BitConverter.ToString(data));
                Console.WriteLine(BitConverter.ToString(previous));
                Console.WriteLine(BitConverter.ToString(expected));
                Console.WriteLine(BitConverter.ToString(actual));
            }
        } else {
            Console.WriteLine("Sequences are equal, press any key to proceed.");
        }
    }
}