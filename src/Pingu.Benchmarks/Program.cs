using System;
using System.Linq;
using BenchmarkDotNet.Running;

using Pingu.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        // TestAdler ();
        TestSub();

        var switcher = new BenchmarkSwitcher(new[] {
            typeof (Adler32Implementations),
            typeof (SubImplementations)
        });

        switcher.Run(args);
    }

    static void TestSub()
    {
        var sub = new SubImplementations { BytesPerPixel = 4, TotalBytes = 5000 };
        sub.Setup();

        var vec = sub.VectorAndPointer();
        var ptr = sub.PointersOnly();

        SequenceEqual(ptr, vec, sub.Data);
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

    static void SequenceEqual(byte[] expected, byte[] actual, byte[] data)
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
}