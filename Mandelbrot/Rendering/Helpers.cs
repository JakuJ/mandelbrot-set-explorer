using System;
using System.Buffers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering;

public sealed class ContiguousImage<T> : IDisposable where T : unmanaged, IPixel<T>
{
    private MemoryHandle memoryHandle;
    public Image<T> Image { get; }
    public unsafe void* Pointer => memoryHandle.Pointer;

    public ContiguousImage(int width, int height)
    {
        var config = Configuration.Default.Clone();
        config.PreferContiguousImageBuffers = true;

        Image = new Image<T>(config, width, height);

        if (!Image.DangerousTryGetSinglePixelMemory(out var memory))
            throw new Exception("This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");

        memoryHandle = memory.Pin();
    }

    public void Dispose()
    {
        memoryHandle.Dispose();
        Image.Dispose();
    }
}
