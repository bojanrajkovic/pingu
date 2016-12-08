namespace Pingu.Filters
{
    public enum FilterType : byte
    {
        None = 0,
        Sub = 1,
        Up = 2,
        Average = 3,
        Paeth = 4,
        Dynamic = byte.MaxValue
    }
}
