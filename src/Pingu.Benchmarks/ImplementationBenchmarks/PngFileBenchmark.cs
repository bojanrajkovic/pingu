using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Pingu.Chunks;
using Pingu.Colors;
using Pingu.Filters;

namespace Pingu.Benchmarks.ImplementationBenchmarks
{
    [Config(typeof(Config))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    public class PngFileBenchmark
    {
        byte[] rawRgbaData;

        [Params(FilterType.Dynamic, FilterType.Average, FilterType.Paeth, FilterType.Sub, FilterType.Up)]
        public FilterType FilterType { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var asm = typeof(PngFileBenchmark).GetTypeInfo().Assembly;
            var resource = asm.GetManifestResourceStream("Pingu.Benchmarks.Zooey.RGBA32");

            using (var ms = new MemoryStream()) {
                resource.CopyTo(ms);
                rawRgbaData = ms.ToArray();
            }
        }

        [Benchmark(Baseline = true)]
        public async Task CreatePNGFile()
        {
            var header = new IhdrChunk(752, 1334, 8, ColorType.TruecolorAlpha);
            var idat = new IdatChunk(header, rawRgbaData, FilterType);
            var end = new IendChunk();

            var pngFile = new PngFile() {
                header,
                idat,
                end
            };

            using (var ms = new MemoryStream())
                await pngFile.WriteFileAsync(ms);
        }
    }
}
