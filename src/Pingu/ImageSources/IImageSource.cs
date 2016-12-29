namespace Pingu.ImageSources
{
    public interface IImageSource
    {
        int BitDepth { get; }
        ImageFormat Format { get; }
        byte[] RawData { get; }

        IImageSource ConvertToIndexedImageSource(int targetBitDepth);
        IImageSource ConvertToGrayscaleImageSource(int targetBitDepth);
        IImageSource ConvertToGrayscaleAlphaImageSource(int targetBitDepth);
        IImageSource ConvertToTruecolorImageSource(int targetBitDepth);
        IImageSource ConvertToTruecolorAlphaImageSource(int targetBitDepth);
    }
}