using System;

namespace Pingu.ImageSources
{
    public class TruecolorImageSource : IImageSource
    {
        public int BitDepth { get; }
        public ImageFormat Format => ImageFormat.Rgb;
        public byte[] RawData { get; }
        
        public TruecolorImageSource(int targetBitDepth, byte[] targetData)
        {
            BitDepth = targetBitDepth;
            RawData = targetData;
        }

        public IImageSource ConvertToGrayscaleAlphaImageSource(int targetBitDepth)
        {
            throw new NotImplementedException();
        }

        public IImageSource ConvertToGrayscaleImageSource(int targetBitDepth)
        {
            throw new NotImplementedException();
        }

        public IImageSource ConvertToIndexedImageSource(int targetBitDepth)
        {
            throw new NotImplementedException();
        }

        public IImageSource ConvertToTruecolorAlphaImageSource(int targetBitDepth)
        {
            throw new NotImplementedException();
        }

        public IImageSource ConvertToTruecolorImageSource(int targetBitDepth)
        {
            throw new NotImplementedException();
        }
    }
}