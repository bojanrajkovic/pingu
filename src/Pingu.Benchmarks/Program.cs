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
        TestAvg();

        var switcher = new BenchmarkSwitcher(new[] {
            typeof (Adler32Implementations),
            typeof (SubImplementations),
            typeof (UpImplementations),
            typeof (AvgImplementations)
        });

        switcher.Run(args);
    }

    static void TestUp()
    {
        var up = new UpImplementations { TotalBytes = 5000 };
        byte[] naive = new byte[5000], ptr = new byte[5000], unr = new byte[5000], vec = new byte[5000];

        up.Setup();

        up.Naive();
        Buffer.BlockCopy(up.TargetBuffer, 0, naive, 0, 5000);

        up.PointersOnly();
        Buffer.BlockCopy(up.TargetBuffer, 0, ptr, 0, 5000);

        up.PointersUnrolled();
        Buffer.BlockCopy(up.TargetBuffer, 0, unr, 0, 5000);

        up.VectorAndPointer();
        Buffer.BlockCopy(up.TargetBuffer, 0, vec, 0, 5000);

        SequenceEqualUp(naive, ptr, up.RawScanline, up.PreviousScanline);
        SequenceEqualUp(naive, unr, up.RawScanline, up.PreviousScanline);
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
        sub.Setup();

        var vec = sub.VectorAndPointer();
        var ptr = sub.PointersOnly();
        var unr = sub.PointersOnlyUnrolled();

        SequenceEqualSub(ptr, unr, sub.Data);
        Console.ReadKey();
    }

    static void TestAdler()
    {
        var adler = new Adler32Implementations();
        adler.SetupData();

        var smarter = adler.Smarter();
        var known = adler.UsePointer();
        var smartest = adler.Smartest();

        Console.WriteLine($"Smartest: {smartest} - Smarter: {smarter} - Known: {known}");
        Console.ReadKey();
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