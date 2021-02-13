using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Mandelbrot
{
    /// <summary>
    /// Represents the Mandelbrot set renderer
    /// </summary>
    public abstract class MandelbrotSet
    {
        /// <summary>
        /// The lower bound of the real axis.
        /// </summary>
        public double XMin;

        /// <summary>
        /// The upper bound of the real axis.
        /// </summary>
        public double XMax;

        /// <summary>
        /// The lower bound of the imaginary axis.
        /// </summary>
        protected double YMin;

        /// <summary>
        /// The upper bound of the imaginary axis.
        /// </summary>
        protected double YMax;

        /// <summary>
        /// Gets the window width.
        /// </summary>
        /// <value>The width.</value>
        protected double Width => XMax - XMin;

        /// <summary>
        /// Gets the window height.
        /// </summary>
        /// <value>The height.</value>
        protected double Height => YMax - YMin;

        /// <summary>
        /// The number of iterations before announcing a complex number non-divergent
        /// </summary>
        public int N;

        /// <summary>
        /// The escape radius.
        /// </summary>
        public int R;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mandelbrot.MandelbrotSet"/> class.
        /// </summary>
        protected MandelbrotSet()
        {
            XMin = -2.5;
            XMax = 1.5;
            YMin = -1.25;
            YMax = 1.25;

            N = 200;
            R = 2;
        }

        /// <summary>
        /// Zooms in or out at a specified location by a given factor.
        /// </summary>
        /// <param name="dx">Real part of the new location as a fraction of the window width.</param>
        /// <param name="dy">Imaginary part of the new location as a fraction of the window height.</param>
        /// <param name="factor">Zooming factor.</param>
        public void Zoom(double dx, double dy, double factor)
        {
            double newX = XMin + Width * dx;
            double newY = YMin + Height * dy;

            dx = Width / (2 * factor);
            dy = Height / (2 * factor);

            XMin = newX - dx;
            XMax = newX + dx;
            YMin = newY - dy;
            YMax = newY + dy;
        }

        /// <summary>
        /// Render the bitmap for specified image width and height.
        /// </summary>
        /// <returns>The rendered bitmap object.</returns>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public abstract Bitmap Render(int width, int height);
    }

    public class OpenClMandelbrot : MandelbrotSet
    {
        /// <inheritdoc />
        /// <summary>
        /// Generates the <see cref="T:System.Drawing.Bitmap" /> using native OpenCL Mandelbrot set implementation.
        /// </summary>
        /// <returns>The bitmap.</returns>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        public override Bitmap Render(int width, int height)
        {
            GpuAcceleration.OpenCLRender(out IntPtr memory,
                                         (uint)width,
                                         (uint)height,
                                         (uint)N,
                                         (uint)R,
                                         XMin,
                                         XMax,
                                         YMin,
                                         YMax);
            const PixelFormat bmpFormat = PixelFormat.Format32bppArgb;
            return new Bitmap(width, height, Extensions.GetStride(width, bmpFormat), bmpFormat, memory);
        }
    }

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
                    return GetColor(Math.Sqrt(zReSqr + zImSqr) / Math.Pow(2, i));
                }

                zIm = zRe * zIm;
                zIm += zIm + cIm;

                zRe = zReSqr - zImSqr + cRe;
                zReSqr = zRe * zRe;
                zImSqr = zIm * zIm;
            }

            return Color.FromArgb(0, 0, 0);
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