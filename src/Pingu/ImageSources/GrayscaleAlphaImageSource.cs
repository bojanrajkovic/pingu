using System;

namespace Pingu.ImageSources
{
    public class GrayscaleAlphaImageSource : IImageSource
    {
        public int BitDepth { get; }
        public ImageFormat Format => ImageFormat.GrayscaleAlpha;
        public byte[] RawData { get; }

        internal GrayscaleAlphaImageSource(int bitDepth, byte[] rawData)
        {
            BitDepth = bitDepth;
            RawData = rawData;
        }

        public IImageSource ConvertToGrayscaleAlphaImageSource(int bitDepth)
        {
            throw new NotImplementedException();
        }

        public IImageSource ConvertToGrayscaleImageSource(int bitDepth)
        {
            throw new NotImplementedException();
        }

        public IImageSource ConvertToIndexedImageSource(int bitDepth)
        {
            throw new NotImplementedException();
        }

        public IImageSource ConvertToTruecolorAlphaImageSource(int bitDepth)
        {
            throw new NotImplementedException();
        }

        public IImageSource ConvertToTruecolorImageSource(int bitDepth)
        {
            throw new NotImplementedException();
        }
    }
}