using System;

using Xunit;

using Pingu.Colors;

namespace Pingu.Tests
{
    public class TransparencyMapTests
    {
        [Fact]
        public void Adding_too_many_entries_throws_exception()
        {
            var map = new TransparencyMap(1);
            map.AddTransparencyToMap(0, 255);
            map.AddTransparencyToMap(1, 213);

            var arex = Assert.Throws<ArgumentOutOfRangeException>(() => map.AddTransparencyToMap(2, 49));
            Assert.Equal ("palletteIndex", arex.ParamName);
        }

        [Fact]
        public void Can_retrieve_items_from_transparency_map()
        {
            var map = new TransparencyMap(1);
            map.AddTransparencyToMap(0, 201);

            Assert.Equal(201, map[0]);
        }
    }
}