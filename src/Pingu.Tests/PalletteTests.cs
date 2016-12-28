using System;

using Xunit;

using Pingu.Colors;

namespace Pingu.Tests
{
    public class PalletteTests
    {
        [Fact]
        public void Can_access_pallette_by_index()
        {
            var pallette = new Pallette (1);
            pallette.AddColorToPallette (17, 93, 45);

            var colorByIndex = pallette[0];

            Assert.Equal(17, colorByIndex[0]);
            Assert.Equal(93, colorByIndex[1]);
            Assert.Equal(45, colorByIndex[2]);
        }

        [Fact]
        public void Can_get_index_for_color()
        {
            var pallette = new Pallette (1);
            pallette.AddColorToPallette (17, 93, 45);

            var indexForColor = pallette[17, 93, 45];

            Assert.Equal(0, indexForColor);
        }

        [Fact]
        public void Adding_too_many_entries_throws_exception()
        {
            var pallette = new Pallette(1);
            pallette.AddColorToPallette(255, 19, 10);
            pallette.AddColorToPallette(17, 29, 99);

            Assert.Throws<Exception>(() => pallette.AddColorToPallette(93, 49, 111));
        }
    }
}