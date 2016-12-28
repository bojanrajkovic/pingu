using Xunit;

using Pingu.Colors;

namespace Pingu.Tests
{
    public class ColorHelperTests
    {
        [Theory]
        [InlineData(255, 0  , 0  ,  54)]
        [InlineData(0  , 255, 0  , 182)]
        [InlineData(0  , 0  , 255,  18)]
        [InlineData(0  , 255, 255, 200)]
        [InlineData(255, 0  , 255,  72)]
        [InlineData(255, 255, 0  , 236)]
        [InlineData(158, 85 , 54 ,  98)]
        [InlineData(155, 160, 52 , 151)]
        [InlineData(100, 0  , 150,  32)]
        public void Correctly_converts_RGB_to_grayscale(byte r, byte g, byte b, byte gray)
        {
            Assert.Equal(gray, ColorHelpers.RgbToGrayscale(r, g, b));
        }

        [Theory]
        [InlineData(255, 0  , 0  , 16711680)]
        [InlineData(0  , 255, 0  ,    65280)]
        [InlineData(0  , 0  , 255,      255)]
        [InlineData(0  , 255, 255,    65535)]
        [InlineData(255, 0  , 255, 16711935)]
        [InlineData(255, 255, 0  , 16776960)]
        [InlineData(158, 85 , 54 , 10376502)]
        [InlineData(155, 160, 52 , 10199092)]
        [InlineData(100, 0  , 150,  6553750)]
        public void Correctly_packs_colors(byte r, byte g, byte b, int packed)
        {
            Assert.Equal(packed, ColorHelpers.PackRgb(r, g, b));
        }

        [Theory]
        [InlineData(255, 0  , 0  , 16711680)]
        [InlineData(0  , 255, 0  ,    65280)]
        [InlineData(0  , 0  , 255,      255)]
        [InlineData(0  , 255, 255,    65535)]
        [InlineData(255, 0  , 255, 16711935)]
        [InlineData(255, 255, 0  , 16776960)]
        [InlineData(158, 85 , 54 , 10376502)]
        [InlineData(155, 160, 52 , 10199092)]
        [InlineData(100, 0  , 150,  6553750)]
        public void Correctly_unpacks_colors(byte r, byte g, byte b, int packed)
        {
            var unpacked = ColorHelpers.UnpackRgb(packed);
            Assert.Equal(r, unpacked[0]);
            Assert.Equal(g, unpacked[1]);
            Assert.Equal(b, unpacked[2]);
        }

        [Theory]
        [InlineData(16711680,  54)]
        [InlineData(65280   , 182)]
        [InlineData(255     ,  18)]
        [InlineData(65535   , 200)]
        [InlineData(16711935,  72)]
        [InlineData(16776960, 236)]
        [InlineData(10376502,  98)]
        [InlineData(10199092, 151)]
        [InlineData(6553750 ,  32)]
        public void Can_convert_packed_color_to_grayscale(int packed, byte gray)
        {
            Assert.Equal(gray, ColorHelpers.RgbToGrayscale(packed));   
        }
    }
}