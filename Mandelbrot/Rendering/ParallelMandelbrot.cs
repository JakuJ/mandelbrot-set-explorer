using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering
{
    public class ParallelMandelbrot : MandelbrotSet, IDisposable
    {
        private Image<Rgba32> image = new(1, 1);

        public void Dispose()
        {
            image.Dispose();
            GC.SuppressFinalize(this);
        }

        public override Image<Rgba32> Render(int width, int height)
        {
            if (image.Width != width || image.Height != height)
            {
                image.Dispose();

                var config = Configuration.Default.Clone();
                config.PreferContiguousImageBuffers = true;
                image = new Image<Rgba32>(config, width, height);
            }

            var dx = Width / image.Width;
            var dy = Height / image.Height;

            if (!image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
            {
                throw new Exception("This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
            }

            unsafe
            {
                using var pinHandle = memory.Pin();
                Rgba32* ptr = (Rgba32*) pinHandle.Pointer;

                Parallel.For(0,
                    image.Height,
                    y =>
                    {
                        Rgba32* row = ptr + y * image.Width;
                        for (var x = 0; x < image.Width; ++x)
                        {
                            *(row + x) = DeepColoring(XMin + x * dx, YMin + y * dy);
                        }
                    });
            }

            return image;
        }

        private Rgba32 DeepColoring(double cRe, double cIm)
        {
            double zRe = 0;
            double zIm = 0;

            double zReSqr = 0;
            double zImSqr = 0;

            double radius = R * R;

            for (var i = 0; i < N; i++)
            {
                if (zReSqr + zImSqr > radius)
                {
                    return GetColor(i, (float) (zReSqr + zImSqr));
                }

                zIm = zRe * zIm;
                zIm += zIm + cIm;

                zRe = zReSqr - zImSqr + cRe;
                zReSqr = zRe * zRe;
                zImSqr = zIm * zIm;
            }

            return Color.Black;
        }

        private static Rgba32 GetColor(int i, float zs)
        {
            const float b = 0.23570226f, c = 0.124526508f;
            var x = i + MathF.Log2(zs) * .5f;

            var red = .5f * (1 - MathF.Cos(x));
            var green = .5f * (1 - MathF.Cos(b * x));
            var blue = .5f * (1 - MathF.Cos(c * x));

            return new Rgba32(red, green, blue);
        }
    }
}
