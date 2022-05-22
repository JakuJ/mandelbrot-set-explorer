using System;
using System.Buffers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering;

public static class Helpers
{
    public static Image<T> ContiguousImage<T>(int width, int height) where T : unmanaged, IPixel<T>
    {
        var config = Configuration.Default.Clone();
        config.PreferContiguousImageBuffers = true;

        return new Image<T>(config, width, height);
    }

    public static MemoryHandle GetImageMemory<T>(Image<T> image) where T : unmanaged, IPixel<T>
    {
        if (!image.DangerousTryGetSinglePixelMemory(out var memory))
            throw new Exception("This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");

        return memory.Pin();
    }
}
