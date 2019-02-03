using System;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

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
        public double xMin;
        /// <summary>
        /// The upper bound of the real axis.
        /// </summary>
        public double xMax;
        /// <summary>
        /// The lower bound of the imaginary axis.
        /// </summary>
        public double yMin;
        /// <summary>
        /// The upper bound of the imaginary axis.
        /// </summary>
        public double yMax;
        /// <summary>
        /// Gets the window width.
        /// </summary>
        /// <value>The width.</value>
        protected double Width => xMax - xMin;
        /// <summary>
        /// Gets the window height.
        /// </summary>
        /// <value>The height.</value>
        protected double Height => yMax - yMin;
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
            xMin = -2.5;
            xMax = 1.5;
            yMin = -1.25;
            yMax = 1.25;

            N = 200;
            R = 1024;
        }
        /// <summary>
        /// Zooms in or out at a specified location by a given factor.
        /// </summary>
        /// <param name="dx">Real part of the new location as a fraction of the window width.</param>
        /// <param name="dy">Imaginary part of the new location as a fraction of the window height.</param>
        /// <param name="factor">Zooming factor.</param>
        public void Zoom(double dx, double dy, double factor)
        {
            double newX = xMin + Width * dx;
            double newY = yMin + Height * dy;

            dx = Width / (2 * factor); dy = Height / (2 * factor);

            xMin = newX - dx;
            xMax = newX + dx;
            yMin = newY - dy;
            yMax = newY + dy;
        }
        /// <summary>
        /// Render the bitmap for specified image width and height.
        /// </summary>
        /// <returns>The rendered bitmap object.</returns>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        protected abstract Bitmap Render(int width, int height);
        /// <summary>
        /// Generates the image (<see cref="TextureTarget.Texture2D"/>) of the Mandelbrot set bounded by the internal parameters, with given resolution.
        /// </summary>
        /// <returns>The texture id.</returns>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        public int GenerateTexture(int width, int height)
        {
            Bitmap bmp = Render(width, height);

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                data.Width, data.Height,
                0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return id;
        }
    }

    public class ParallelMandelbrot : MandelbrotSet
    {
        /// <summary>
        /// Returns a <see cref="Color"/> for a complex number (<paramref name="c_re"/> + <paramref name="c_im"/> i)
        /// </summary>
        /// <returns>The coloring.</returns>
        /// <param name="c_re">Real part.</param>
        /// <param name="c_im">Imaginary part.</param>
        private Color DeepColoring(double c_re, double c_im)
        {
            double z_re = 0;
            double z_im = 0;

            double z_re_sqr = 0;
            double z_im_sqr = 0;

            double Radius = R * R;

            for (int i = 0; i < N; i++)
            {
                if (z_re_sqr + z_im_sqr > Radius)
                {
                    return GetColor(Math.Sqrt(z_re_sqr + z_im_sqr) / Math.Pow(2, i));
                }

                z_im = z_re * z_im;
                z_im += z_im;
                z_im += c_im;

                z_re = z_re_sqr - z_im_sqr + c_re;
                z_re_sqr = z_re * z_re;
                z_im_sqr = z_im * z_im;
            }
            return Color.FromArgb(0, 0, 0);
        }
        /// <summary>
        /// Returns the <see cref="Color"/> for a positive value <paramref name="V"/>.
        /// </summary>
        /// <returns>The color.</returns>
        /// <param name="V">V.</param>
        /// <param name="K">An arbitrary constant controlling the color.</param>
        private Color GetColor(double V, double K = 2)
        {
            double a = 1.4427,
                   b = 0.34,
                   c = 0.18;

            double x = Math.Log(V) / K;

            int red = (int)Math.Floor(255 / 2 * (1 - Math.Cos(a * x)));
            int green = (int)Math.Floor(255 / 2 * (1 - Math.Cos(b * x)));
            int blue = (int)Math.Floor(255 / 2 * (1 - Math.Cos(c * x)));

            return Color.FromArgb(red, green, blue);
        }
        /// <summary>
        /// Calculates the Mandelbrot set using nested <see cref="Parallel.For(int, int, Action{int})"/> and generates a <see cref="Bitmap"/>;
        /// </summary>
        /// <returns>A <see cref="Bitmap"/> object</returns>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        protected override Bitmap Render(int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);

            double dx = Width / bmp.Width;
            double dy = Height / bmp.Height;

            Parallel.For(0, bmp.Width, x =>
            {
                Parallel.For(0, bmp.Height, y =>
                {
                    Color newColor = DeepColoring(xMin + x * dx, yMin + y * dy);
                    bmp.SetPixel(x, y, newColor);
                });
            });

            return bmp;
        }
    }

    public class OpenCLMandelbrot : MandelbrotSet
    {
        protected override Bitmap Render(int width, int height) => GPUAcceleration.GenerateBitmap(width, height, N, R, xMin, xMax, yMin, yMax);
    }
}
