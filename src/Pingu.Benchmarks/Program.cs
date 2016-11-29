using BenchmarkDotNet.Running;

using Pingu.Benchmarks;

class Program
{
    static void Main(string[] args)
    {
        var switcher = new BenchmarkSwitcher(new[] {
            typeof (Adler32Benchmark),
        });

        switcher.Run(args);
    }
}