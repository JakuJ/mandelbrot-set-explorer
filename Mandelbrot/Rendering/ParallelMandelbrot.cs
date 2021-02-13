using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Mandelbrot.Rendering
{
    public class ParallelMandelbrot : MandelbrotSet
    {
        /// <summary>
        /// Returns a <see cref="Color"/> for a complex number (<paramref name="cRe"/> + <paramref name="cIm"/> i)
        /// </summary>
        /// <returns>The coloring.</returns>
        /// <param name="cRe">Real part.</param>
        /// <param name="cIm">Imaginary part.</param>
        private Color DeepColoring(double cRe, double cIm)
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

            return Color.Black;
        }

        /// <summary>
        /// Returns the <see cref="Color"/> for a positive value <paramref name="v"/>.
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="v">V.</param>
        /// <param name="k">An arbitrary constant controlling the color.</param>
        private static Color GetColor(double v, double k = 2)
        {
            const double a = 1.4427, b = 0.34, c = 0.18;
            double x = Math.Log(v) / k;

            var red = (int)Math.Floor(127 * (1 - Math.Cos(a * x)));
            var green = (int)Math.Floor(127 * (1 - Math.Cos(b * x)));
            var blue = (int)Math.Floor(127 * (1 - Math.Cos(c * x)));

            return Color.FromArgb(red, green, blue);
        }

        /// <inheritdoc />
        /// <summary>
        /// Calculates the Mandelbrot set using nested <see cref="M:System.Threading.Tasks.Parallel.For(System.Int32,System.Int32,System.Action{System.Int32})" /> and generates a <see cref="T:System.Drawing.Bitmap" />;
        /// </summary>
        /// <returns>A <see cref="T:System.Drawing.Bitmap" /> object</returns>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        public override Bitmap Render(int width, int height)
        {
            Bitmap bmp = new(width, height);

            double dx = Width / bmp.Width;
            double dy = Height / bmp.Height;

            Parallel.For(0, bmp.Width, x =>
            {
                // TODO: Maybe just one loop?
                Parallel.For(0, bmp.Height, y =>
                {
                    Color newColor = DeepColoring(XMin + x * dx, YMin + y * dy);
                    bmp.SetPixel(x, y, newColor);
                });
            });

            return bmp;
        }
    }
}