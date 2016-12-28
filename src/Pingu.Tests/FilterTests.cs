using System;

using Xunit;

using Pingu.Filters;

namespace Pingu.Tests
{
    public class FilterTests
    {
        [Fact]
        public void Get_filter_throws_for_invalid_filter_type()
        {
            var arex = Assert.Throws<ArgumentOutOfRangeException>(() => DefaultFilters.GetFilterForType((FilterType)11));
            Assert.Equal("filter", arex.ParamName);
        }
    }
}