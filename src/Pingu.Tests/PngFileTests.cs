using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Xunit;

using Pingu.Chunks;
using Pingu.Colors;
using Pingu.Filters;

namespace Pingu.Tests
{
    public class PngFileTests
    {
        [Theory]
        [InlineData("Pingu.Tests.Zooey.RGBA32", FilterType.None, 8)]
        [InlineData("Pingu.Tests.Zooey.RGBA32", FilterType.Sub, 8)]
        [InlineData("Pingu.Tests.Zooey.RGBA32", FilterType.Up, 8)]
        [InlineData("Pingu.Tests.Zooey.RGBA32", FilterType.Average, 8)]
        [InlineData("Pingu.Tests.Zooey.RGBA32", FilterType.Paeth, 8)]
        [InlineData("Pingu.Tests.Zooey.RGBA32", FilterType.Dynamic, 8)]
        [InlineData("Pingu.Tests.Zooey.RGBA64", FilterType.None, 16)]
        [InlineData("Pingu.Tests.Zooey.RGBA64", FilterType.Sub, 16)]
        [InlineData("Pingu.Tests.Zooey.RGBA64", FilterType.Up, 16)]
        [InlineData("Pingu.Tests.Zooey.RGBA64", FilterType.Average, 16)]
        [InlineData("Pingu.Tests.Zooey.RGBA64", FilterType.Paeth, 16)]
        [InlineData("Pingu.Tests.Zooey.RGBA64", FilterType.Dynamic, 16)]
        public async Task Can_write_PNG_file(string imageName, FilterType type, int bitDepth)
        {
            var asm = typeof(PngFileTests).GetTypeInfo().Assembly;
            var resource = asm.GetManifestResourceStream(imageName);

            byte[] rawRgbaData;

            using (var ms = new MemoryStream()) {
                await resource.CopyToAsync(ms);
                rawRgbaData = ms.ToArray();
            }

            var header = new IhdrChunk(752, 1334, bitDepth, ColorType.TruecolorAlpha);
            var idat = new IdatChunk(header, rawRgbaData, type);
            var end = new IendChunk();

            var pngFile = new PngFile() {
                header,
                idat,
                end
            };

            var path = Path.Combine(Path.GetDirectoryName(asm.Location), $"Zooey-{bitDepth}-{type}.png");
            using (var fs = new FileStream(path, FileMode.Create))
                await pngFile.WriteFileAsync(fs);

            Assert.True(File.Exists(path));

            var result = ToolHelper.RunPngCheck(path);

            Assert.Equal(0, result.ExitCode);
            System.Console.Write(result.StandardOutput);
        }
    }
}
