using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering
{
    public class ParallelMandelbrot : MandelbrotSet
    {
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
                    return GetColor(Math.Sqrt(zReSqr + zImSqr) / (1 << i));
                }

                zIm = zRe * zIm;
                zIm += zIm + cIm;

                zRe = zReSqr - zImSqr + cRe;
                zReSqr = zRe * zRe;
                zImSqr = zIm * zIm;
            }

            return new Rgba32(0, 1f, 0);
        }

        private static Rgba32 GetColor(double v, float k = 2)
        {
            const float a = 1.4427f, b = 0.34f, c = 0.18f;
            var x = MathF.Log((float) v) / k;

            var red = 0.5f * (1f + MathF.Cos(a * x));
            var green = 0.5f * (1f + MathF.Cos(b * x));
            var blue = 0.5f * (1f + MathF.Cos(c * x));

            return new Rgba32(red, green, blue);
        }

        /// <inheritdoc />
        /// <summary>
        /// Calculates the Mandelbrot set using nested <see cref="M:System.Threading.Tasks.Parallel.For(System.Int32,System.Int32,System.Action{System.Int32})" /> and generates a <see cref="T:System.Drawing.Bitmap" />;
        /// </summary>
        /// <returns>A <see cref="T:System.Drawing.Bitmap" /> object</returns>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        public override Image<Rgba32> Render(int width, int height)
        {
            Image<Rgba32> img = new(width, height);

            var dx = Width / img.Width;
            var dy = Height / img.Height;

            Parallel.For(0L,
                img.Width,
                x =>
                {
                    Parallel.For(0L,
                        img.Height,
                        y => { img[(int) x, (int) y] = DeepColoring(XMin + x * dx, YMin + y * dy); });
                });

            return img;
        }
    }
}
