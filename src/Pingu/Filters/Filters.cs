using System;

namespace Pingu.Filters
{
    static class DefaultFilters
    {
        public static IFilter GetFilterForType(FilterType filter)
        {
            switch (filter) {
                case FilterType.None:
                    return NullFilter.Instance;
                case FilterType.Sub:
                    return SubFilter.Instance;
                case FilterType.Up:
                    return UpFilter.Instance;
                case FilterType.Average:
                    throw new NotImplementedException();
                case FilterType.Paeth:
                    throw new NotImplementedException();
                case FilterType.Dynamic:
                    throw new ArgumentException("Dynamic filter should not be reached.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter));
            }
        }
    }
}
