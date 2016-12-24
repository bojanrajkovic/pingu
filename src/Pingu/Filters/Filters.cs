using System;
using System.Numerics;

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
                    return AvgFilter.Instance;
                case FilterType.Paeth:
                    return PaethFilter.Instance;
                case FilterType.Dynamic:
                    return DynamicFilter.Instance;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filter));
            }
        }

        public static readonly bool UseVectors = true;

        static DefaultFilters()
        {
            // If we're on Mono, don't use vectors.
            if (Type.GetType("Mono.Runtime") != null)
                UseVectors = false;

            // If Vectors aren't hardware accelerated, use pointers.
            if (!Vector.IsHardwareAccelerated)
                UseVectors = false;
        }
    }
}
