using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering
{
    public class ParallelMandelbrot : MandelbrotSet
    {
        private Image<Rgba32> img = new(1, 1);

        public override Image<Rgba32> Render(int width, int height)
        {
            if (img.Width != width || img.Height != height)
            {
                img = new Image<Rgba32>(width, height);
            }

            var dx = Width / img.Width;
            var dy = Height / img.Height;

            Parallel.For(0,
                img.Height,
                y =>
                {
                    for (var x = 0; x < img.Width; ++x)
                    {
                        img[x, y] = DeepColoring(XMin + x * dx, YMin + y * dy);
                    }
                });

            return img;
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
