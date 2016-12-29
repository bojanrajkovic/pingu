using System;
using Pingu.Colors;
using Pingu.ImageSources;
using Xunit;

namespace Pingu.Tests
{
    public class ColorFixture
    {
        public Color[] PalletteTestColors { get; } = new Color[256];

        public ColorFixture()
        {
            var r = new Random();
            var colorBytes = new byte[4];

            for (int i = 0; i < PalletteTestColors.Length; i++) {
                r.NextBytes(colorBytes);
                var color = new Color(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]);
                PalletteTestColors[i] = color;
            }
        }
    }

    public class IndexedImageSourceTests : IClassFixture<ColorFixture>
    {
        Color[] PalletteTestColors;

        public IndexedImageSourceTests(ColorFixture colors)
        {
            PalletteTestColors = colors.PalletteTestColors;
        }

        [Fact]
        public void Can_convert_single_bit_image_data_to_grayscale_alpha_16_image_data()
        {
            var pallette = new Pallette(1);
            var transparencyMap = new TransparencyMap(1);

            for (int i = 0; i < 2; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 0b11011000, 0b10000000 };
            var indexedImageSource = new IndexedImageSource(1, imageData, 9, 1, pallette, transparencyMap);

            var grayscaleImageSource = indexedImageSource.ConvertToGrayscaleAlphaImageSource(16);
            var grayscaleData = grayscaleImageSource.RawData;

            // Should be 36 bytes -- 9 pixels.
            var expectedData = new byte[] {
                0x00, PalletteTestColors[1].Grayscale, 0x00, PalletteTestColors[1].A,
                0x00, PalletteTestColors[1].Grayscale, 0x00, PalletteTestColors[1].A,
                0x00, PalletteTestColors[0].Grayscale, 0x00, PalletteTestColors[0].A,
                0x00, PalletteTestColors[1].Grayscale, 0x00, PalletteTestColors[1].A,
                0x00, PalletteTestColors[1].Grayscale, 0x00, PalletteTestColors[1].A,
                0x00, PalletteTestColors[0].Grayscale, 0x00, PalletteTestColors[0].A,
                0x00, PalletteTestColors[0].Grayscale, 0x00, PalletteTestColors[0].A,
                0x00, PalletteTestColors[0].Grayscale, 0x00, PalletteTestColors[0].A,
                0x00, PalletteTestColors[1].Grayscale, 0x00, PalletteTestColors[1].A
            };

            Assert.Equal(expectedData, grayscaleData);
        }

        [Fact]
        public void Can_convert_two_bit_image_data_to_grayscale_alpha_16_image_data()
        {
            var pallette = new Pallette(2);
            var transparencyMap = new TransparencyMap(2);

            for (int i = 0; i < 4; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 0b00011011, 0b11100100, 0b01000000 };
            var indexedImageSource = new IndexedImageSource(2, imageData, 9, 1, pallette, transparencyMap);

            var grayscaleImageSource = indexedImageSource.ConvertToGrayscaleAlphaImageSource(16);
            var grayscaleData = grayscaleImageSource.RawData;

            // Should be 36 bytes -- 9 pixels.
            var expectedData = new byte[] {
                0x00, PalletteTestColors[0].Grayscale, 0x00, PalletteTestColors[0].A,
                0x00, PalletteTestColors[1].Grayscale, 0x00, PalletteTestColors[1].A,
                0x00, PalletteTestColors[2].Grayscale, 0x00, PalletteTestColors[2].A,
                0x00, PalletteTestColors[3].Grayscale, 0x00, PalletteTestColors[3].A,
                0x00, PalletteTestColors[3].Grayscale, 0x00, PalletteTestColors[3].A,
                0x00, PalletteTestColors[2].Grayscale, 0x00, PalletteTestColors[2].A,
                0x00, PalletteTestColors[1].Grayscale, 0x00, PalletteTestColors[1].A,
                0x00, PalletteTestColors[0].Grayscale, 0x00, PalletteTestColors[0].A,
                0x00, PalletteTestColors[1].Grayscale, 0x00, PalletteTestColors[1].A
            };

            Assert.Equal(expectedData, grayscaleData);
        }

        [Fact]
        public void Can_convert_four_bit_image_data_to_grayscale_alpha_16_image_data()
        {
            var pallette = new Pallette(4);
            var transparencyMap = new TransparencyMap(4);

            for (int i = 0; i < 16; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 0b01011010, 0b11110001, 0b00000110, 0b00100100, 0b11000000 };
            var indexedImageSource = new IndexedImageSource(4, imageData, 9, 1, pallette, transparencyMap);

            var grayscaleImageSource = indexedImageSource.ConvertToGrayscaleAlphaImageSource(16);
            var grayscaleData = grayscaleImageSource.RawData;

            // Should be 36 bytes -- 9 pixels.
            var expectedData = new byte[] {
                0x00, PalletteTestColors[5].Grayscale, 0x00, PalletteTestColors[5].A,
                0x00, PalletteTestColors[10].Grayscale, 0x00, PalletteTestColors[10].A,
                0x00, PalletteTestColors[15].Grayscale, 0x00, PalletteTestColors[15].A,
                0x00, PalletteTestColors[1].Grayscale, 0x00, PalletteTestColors[1].A,
                0x00, PalletteTestColors[0].Grayscale, 0x00, PalletteTestColors[0].A,
                0x00, PalletteTestColors[6].Grayscale, 0x00, PalletteTestColors[6].A,
                0x00, PalletteTestColors[2].Grayscale, 0x00, PalletteTestColors[2].A,
                0x00, PalletteTestColors[4].Grayscale, 0x00, PalletteTestColors[4].A,
                0x00, PalletteTestColors[12].Grayscale, 0x00, PalletteTestColors[12].A
            };

            Assert.Equal(expectedData, grayscaleData);
        }

        [Fact]
        public void Can_convert_eight_bit_image_data_to_grayscale_alpha_16_image_data()
        {
            var pallette = new Pallette(8);
            var transparencyMap = new TransparencyMap(8);

            for (int i = 0; i < 256; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 13, 44, 97, 93, 11, 5, 8, 9, 239 };
            var indexedImageSource = new IndexedImageSource(8, imageData, 9, 1, pallette, transparencyMap);

            var grayscaleImageSource = indexedImageSource.ConvertToGrayscaleAlphaImageSource(16);
            var grayscaleData = grayscaleImageSource.RawData;

            // Should be 36 bytes -- 9 pixels.
            var expectedData = new byte[] {
                0x00, PalletteTestColors[13].Grayscale, 0x00, PalletteTestColors[13].A,
                0x00, PalletteTestColors[44].Grayscale, 0x00, PalletteTestColors[44].A,
                0x00, PalletteTestColors[97].Grayscale, 0x00, PalletteTestColors[97].A,
                0x00, PalletteTestColors[93].Grayscale, 0x00, PalletteTestColors[93].A,
                0x00, PalletteTestColors[11].Grayscale, 0x00, PalletteTestColors[11].A,
                0x00, PalletteTestColors[5].Grayscale, 0x00, PalletteTestColors[5].A,
                0x00, PalletteTestColors[8].Grayscale, 0x00, PalletteTestColors[8].A,
                0x00, PalletteTestColors[9].Grayscale, 0x00, PalletteTestColors[9].A,
                0x00, PalletteTestColors[239].Grayscale, 0x00, PalletteTestColors[239].A
            };

            Assert.Equal(expectedData, grayscaleData);
        }

        [Fact]
        public void Can_convert_single_bit_image_data_to_grayscale_alpha_8_image_data()
        {
            var pallette = new Pallette(1);
            var transparencyMap = new TransparencyMap(1);

            for (int i = 0; i < 2; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 0b11011000, 0b10000000 };
            var indexedImageSource = new IndexedImageSource(1, imageData, 9, 1, pallette, transparencyMap);

            var grayscaleImageSource = indexedImageSource.ConvertToGrayscaleAlphaImageSource(8);
            var grayscaleData = grayscaleImageSource.RawData;

            // Should be 18 bytes -- 9 pixels.
            var expectedData = new byte[] {
                PalletteTestColors[1].Grayscale, PalletteTestColors[1].A,
                PalletteTestColors[1].Grayscale, PalletteTestColors[1].A,
                PalletteTestColors[0].Grayscale, PalletteTestColors[0].A,
                PalletteTestColors[1].Grayscale, PalletteTestColors[1].A,
                PalletteTestColors[1].Grayscale, PalletteTestColors[1].A,
                PalletteTestColors[0].Grayscale, PalletteTestColors[0].A,
                PalletteTestColors[0].Grayscale, PalletteTestColors[0].A,
                PalletteTestColors[0].Grayscale, PalletteTestColors[0].A,
                PalletteTestColors[1].Grayscale, PalletteTestColors[1].A
            };

            Assert.Equal(expectedData, grayscaleData);
        }

        [Fact]
        public void Can_convert_two_bit_image_data_to_grayscale_alpha_8_image_data()
        {
            var pallette = new Pallette(2);
            var transparencyMap = new TransparencyMap(2);

            for (int i = 0; i < 4; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 0b00011011, 0b11100100, 0b01000000 };
            var indexedImageSource = new IndexedImageSource(2, imageData, 9, 1, pallette, transparencyMap);

            var grayscaleImageSource = indexedImageSource.ConvertToGrayscaleAlphaImageSource(8);
            var grayscaleData = grayscaleImageSource.RawData;

            // Should be 36 bytes -- 9 pixels.
            var expectedData = new byte[] {
                PalletteTestColors[0].Grayscale, PalletteTestColors[0].A,
                PalletteTestColors[1].Grayscale, PalletteTestColors[1].A,
                PalletteTestColors[2].Grayscale, PalletteTestColors[2].A,
                PalletteTestColors[3].Grayscale, PalletteTestColors[3].A,
                PalletteTestColors[3].Grayscale, PalletteTestColors[3].A,
                PalletteTestColors[2].Grayscale, PalletteTestColors[2].A,
                PalletteTestColors[1].Grayscale, PalletteTestColors[1].A,
                PalletteTestColors[0].Grayscale, PalletteTestColors[0].A,
                PalletteTestColors[1].Grayscale, PalletteTestColors[1].A
            };

            Assert.Equal(expectedData, grayscaleData);
        }

        [Fact]
        public void Can_convert_four_bit_image_data_to_grayscale_alpha_8_image_data()
        {
            var pallette = new Pallette(4);
            var transparencyMap = new TransparencyMap(4);

            for (int i = 0; i < 16; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 0b01011010, 0b11110001, 0b00000110, 0b00100100, 0b11000000 };
            var indexedImageSource = new IndexedImageSource(4, imageData, 9, 1, pallette, transparencyMap);

            var grayscaleImageSource = indexedImageSource.ConvertToGrayscaleAlphaImageSource(8);
            var grayscaleData = grayscaleImageSource.RawData;

            // Should be 36 bytes -- 9 pixels.
            var expectedData = new byte[] {
                PalletteTestColors[5].Grayscale, PalletteTestColors[5].A,
                PalletteTestColors[10].Grayscale, PalletteTestColors[10].A,
                PalletteTestColors[15].Grayscale, PalletteTestColors[15].A,
                PalletteTestColors[1].Grayscale, PalletteTestColors[1].A,
                PalletteTestColors[0].Grayscale, PalletteTestColors[0].A,
                PalletteTestColors[6].Grayscale, PalletteTestColors[6].A,
                PalletteTestColors[2].Grayscale, PalletteTestColors[2].A,
                PalletteTestColors[4].Grayscale, PalletteTestColors[4].A,
                PalletteTestColors[12].Grayscale, PalletteTestColors[12].A
            };

            Assert.Equal(expectedData, grayscaleData);
        }

        [Fact]
        public void Can_convert_eight_bit_image_data_to_grayscale_alpha_8_image_data()
        {
            var pallette = new Pallette(8);
            var transparencyMap = new TransparencyMap(8);

            for (int i = 0; i < 256; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 13, 44, 97, 93, 11, 5, 8, 9, 239 };
            var indexedImageSource = new IndexedImageSource(8, imageData, 9, 1, pallette, transparencyMap);

            var grayscaleImageSource = indexedImageSource.ConvertToGrayscaleAlphaImageSource(8);
            var grayscaleData = grayscaleImageSource.RawData;

            // Should be 36 bytes -- 9 pixels.
            var expectedData = new byte[] {
                PalletteTestColors[13].Grayscale, PalletteTestColors[13].A,
                PalletteTestColors[44].Grayscale, PalletteTestColors[44].A,
                PalletteTestColors[97].Grayscale, PalletteTestColors[97].A,
                PalletteTestColors[93].Grayscale, PalletteTestColors[93].A,
                PalletteTestColors[11].Grayscale, PalletteTestColors[11].A,
                PalletteTestColors[5].Grayscale, PalletteTestColors[5].A,
                PalletteTestColors[8].Grayscale, PalletteTestColors[8].A,
                PalletteTestColors[9].Grayscale, PalletteTestColors[9].A,
                PalletteTestColors[239].Grayscale, PalletteTestColors[239].A
            };

            Assert.Equal(expectedData, grayscaleData);
        }

        [Fact]
        public void Can_convert_single_bit_image_data_to_truecolor_16_image_data()
        {
            var pallette = new Pallette(1);
            var transparencyMap = new TransparencyMap(1);

            for (int i = 0; i < 2; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 0b11011000, 0b10000000 };
            var indexedImageSource = new IndexedImageSource(1, imageData, 9, 1, pallette, transparencyMap);

            var truecolorImageSource = indexedImageSource.ConvertToTruecolorImageSource(16);
            var colorData = truecolorImageSource.RawData;

            // Should be 54 bytes -- 9 pixels @ 6 bytes per pixel.
            var expectedData = new byte[] {
                0x00, PalletteTestColors[1].R, 0x00, PalletteTestColors[1].G, 0x00, PalletteTestColors[1].B,
                0x00, PalletteTestColors[1].R, 0x00, PalletteTestColors[1].G, 0x00, PalletteTestColors[1].B,
                0x00, PalletteTestColors[0].R, 0x00, PalletteTestColors[0].G, 0x00, PalletteTestColors[0].B,
                0x00, PalletteTestColors[1].R, 0x00, PalletteTestColors[1].G, 0x00, PalletteTestColors[1].B,
                0x00, PalletteTestColors[1].R, 0x00, PalletteTestColors[1].G, 0x00, PalletteTestColors[1].B,
                0x00, PalletteTestColors[0].R, 0x00, PalletteTestColors[0].G, 0x00, PalletteTestColors[0].B,
                0x00, PalletteTestColors[0].R, 0x00, PalletteTestColors[0].G, 0x00, PalletteTestColors[0].B,
                0x00, PalletteTestColors[0].R, 0x00, PalletteTestColors[0].G, 0x00, PalletteTestColors[0].B,
                0x00, PalletteTestColors[1].R, 0x00, PalletteTestColors[1].G, 0x00, PalletteTestColors[1].B
            };

            Assert.Equal(expectedData, colorData);
        }

        [Fact]
        public void Can_convert_single_bit_image_data_to_truecolor_8_image_data()
        {
            var pallette = new Pallette(1);
            var transparencyMap = new TransparencyMap(1);

            for (int i = 0; i < 2; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 0b11011000, 0b10000000 };
            var indexedImageSource = new IndexedImageSource(1, imageData, 9, 1, pallette, transparencyMap);

            var truecolorImageSource = indexedImageSource.ConvertToTruecolorImageSource(8);
            var colorData = truecolorImageSource.RawData;

            // Should be 27 bytes -- 9 pixels @ 3 bytes per pixel.
            var expectedData = new byte[] {
                PalletteTestColors[1].R, PalletteTestColors[1].G, PalletteTestColors[1].B,
                PalletteTestColors[1].R, PalletteTestColors[1].G, PalletteTestColors[1].B,
                PalletteTestColors[0].R, PalletteTestColors[0].G, PalletteTestColors[0].B,
                PalletteTestColors[1].R, PalletteTestColors[1].G, PalletteTestColors[1].B,
                PalletteTestColors[1].R, PalletteTestColors[1].G, PalletteTestColors[1].B,
                PalletteTestColors[0].R, PalletteTestColors[0].G, PalletteTestColors[0].B,
                PalletteTestColors[0].R, PalletteTestColors[0].G, PalletteTestColors[0].B,
                PalletteTestColors[0].R, PalletteTestColors[0].G, PalletteTestColors[0].B,
                PalletteTestColors[1].R, PalletteTestColors[1].G, PalletteTestColors[1].B
            };

            Assert.Equal(expectedData, colorData);
        }

        [Fact]
        public void Conversion_to_indexed_image_source_with_same_bit_depth_returns_same_instance()
        {
            var pallette = new Pallette(8);
            var transparencyMap = new TransparencyMap(8);

            for (int i = 0; i < 256; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 13, 44, 97, 93, 11, 5, 8, 9, 239 };
            var indexedImageSource = new IndexedImageSource(8, imageData, 9, 1, pallette, transparencyMap);

            var converted = indexedImageSource.ConvertToIndexedImageSource(8);

            Assert.Same(indexedImageSource, converted);
        }

        [Fact]
        public void Conversion_to_indexed_image_source_with_lower_bit_depth_throws()
        {
            var pallette = new Pallette(8);
            var transparencyMap = new TransparencyMap(8);

            for (int i = 0; i < 256; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 13, 44, 97, 93, 11, 5, 8, 9, 239 };
            var indexedImageSource = new IndexedImageSource(8, imageData, 9, 1, pallette, transparencyMap);

            var arex = Assert.Throws<ArgumentException>(() => indexedImageSource.ConvertToIndexedImageSource(4));
            Assert.Equal("targetBitDepth", arex.ParamName);
        }

        [Fact]
        public void Conversion_to_indexed_image_source_with_target_bit_depth_more_than_8_throws()
        {
            var pallette = new Pallette(8);
            var transparencyMap = new TransparencyMap(8);

            for (int i = 0; i < 256; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 13, 44, 97, 93, 11, 5, 8, 9, 239 };
            var indexedImageSource = new IndexedImageSource(8, imageData, 9, 1, pallette, transparencyMap);

            var arex = Assert.Throws<ArgumentOutOfRangeException>(() => indexedImageSource.ConvertToIndexedImageSource(16));
            Assert.Equal("targetBitDepth", arex.ParamName);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(4, true)]
        [InlineData(8, false)]
        [InlineData(16, false)]
        public void Conversion_to_grayscale_with_alpha_fails_if_target_bit_depth_not_8_or_16(int targetBitDepth, bool throws)
        {
            var pallette = new Pallette(8);
            var transparencyMap = new TransparencyMap(8);

            for (int i = 0; i < 256; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 13, 44, 97, 93, 11, 5, 8, 9, 239 };
            var indexedImageSource = new IndexedImageSource(8, imageData, 9, 1, pallette, transparencyMap);

            if (throws) {
                var arex = Assert.Throws<ArgumentOutOfRangeException>(() => indexedImageSource.ConvertToGrayscaleAlphaImageSource(targetBitDepth));
                Assert.Equal("targetBitDepth", arex.ParamName);
            }
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(3, true)]
        [InlineData(2, false)]
        [InlineData(4, false)]
        [InlineData(8, false)]
        [InlineData(16, false)]
        public void Conversion_to_grayscale_fails_if_target_bit_depth_not_1_2_4_8_or_16(int targetBitDepth, bool throws)
        {
            var pallette = new Pallette(8);
            var transparencyMap = new TransparencyMap(8);

            for (int i = 0; i < 256; i++) {
                var palletteIndex = pallette.AddColorToPallette(PalletteTestColors[i]);
                transparencyMap.AddTransparencyToMap(palletteIndex, PalletteTestColors[i].A);
            }

            var imageData = new byte[] { 13, 44, 97, 93, 11, 5, 8, 9, 239 };
            var indexedImageSource = new IndexedImageSource(8, imageData, 9, 1, pallette, transparencyMap);

            if (throws) {
                var arex = Assert.Throws<ArgumentOutOfRangeException>(() => indexedImageSource.ConvertToGrayscaleImageSource(targetBitDepth));
                Assert.Equal("targetBitDepth", arex.ParamName);
            }
        }
    }
}