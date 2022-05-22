using System;
using System.Buffers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering;

public static class Helpers
{
    public static Image<Rgba32> ContiguousImage(int width, int height)
    {
        var config = Configuration.Default.Clone();
        config.PreferContiguousImageBuffers = true;

        return new Image<Rgba32>(config, width, height);
    }

    public static MemoryHandle GetImageMemory(Image<Rgba32> image)
    {
        if (!image.DangerousTryGetSinglePixelMemory(out var memory))
            throw new Exception("This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");

        return memory.Pin();
    }
}
