﻿namespace Pingu.Filters
{
    interface IFilter
    {
        void FilterInto(byte[] targetBuffer, int targetOffset, byte[] rawScanline, byte[] previousScanline, int bytesPerPixel);
    }
}
