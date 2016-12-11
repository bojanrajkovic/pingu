using BenchmarkDotNet.Running;

using Pingu.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        var adler = new Adler32Implementations();
        adler.SetupData();

        var smarter = adler.Smarter();
        var known = adler.UsePointer();
        var smartest = adler.Smartest();

        System.Console.WriteLine($"Smartest: {smartest} - Smarter: {smarter} - Known: {known}");
        System.Console.ReadKey();

        var switcher = new BenchmarkSwitcher(new[] {
            typeof (Adler32Implementations),
            typeof (SubImplementations)
        });

        switcher.Run(args);
    }
}