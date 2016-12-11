using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Xunit;

using Pingu.Chunks;
using Pingu.Filters;

namespace Pingu.Tests
{
    public class PngFileTests
    {
        [Theory]
        [InlineData("Pingu.Tests.Zooey.RGBA32")]
        public async Task Can_write_PNG_file(string imageName)
        {
            var asm = typeof(PngFileTests).GetTypeInfo().Assembly;
            var resource = asm.GetManifestResourceStream(imageName);

            byte[] rawRgbaData;

            using (var ms = new MemoryStream()) {
                await resource.CopyToAsync(ms);
                rawRgbaData = ms.ToArray();
            }

            var header = new IhdrChunk(752, 1334, 8);
            var idat = new IdatChunk(header, rawRgbaData, FilterType.Dynamic);
            var end = new IendChunk();

            var pngFile = new PngFile() {
                header,
                idat,
                end
            };

            var path = Path.Combine(Path.GetDirectoryName(asm.Location), "Zooey.png");
            using (var fs = new FileStream(path, FileMode.Create))
                await pngFile.WriteFileAsync(fs);

            Assert.True(File.Exists(path));

            var result = ToolHelper.RunPngCheck(path);

            Assert.Equal(0, result.ExitCode);
            System.Console.Write(result.StandardOutput);
        }
    }
}
