﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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
            var idat = new IdatChunk(header, rawRgbaData);
            var end = new IendChunk();

            var pngFile = new PngFile() {
                header,
                idat,
                end
            };

            var path = Path.Combine(Path.GetDirectoryName(asm.Location), "Zooey.png");
            using (var fs = new FileStream(path, FileMode.Create))
                await pngFile.WriteFileAsync(fs);
        }
    }
}
