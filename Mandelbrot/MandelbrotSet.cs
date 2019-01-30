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
    public class MandelbrotSet
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
        /// Initializes a new instance of the <see cref="T:Mandelbrot.MandelbrotSet"/> class.
        /// </summary>
        public MandelbrotSet()
        {
            xMin = -2.5;
            xMax = 1.5;
            yMin = -1.25;
            yMax = 1.25;
        }
        /// <summary>
        /// Returns a <see cref="Color"/> for a complex number (<paramref name="c_re"/> + <paramref name="c_im"/> i)
        /// </summary>
        /// <returns>The coloring.</returns>
        /// <param name="c_re">Real part.</param>
        /// <param name="c_im">Imaginary part.</param>
        /// <param name="N">Number of iterations.</param>
        /// <param name="R">Escape radius.</param>
        private Color DeepColoring(double c_re, double c_im, int N = 200, double R = 500)
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

            int red = (int)Math.Floor(255 / 2 * (1 + Math.Cos(a * x)));
            int green = (int)Math.Floor(255 / 2 * (1 + Math.Cos(b * x)));
            int blue = (int)Math.Floor(255 / 2 * (1 + Math.Cos(c * x)));

            return Color.FromArgb(red, green, blue);
        }
        /// <summary>
        /// Generates the image (<see cref="TextureTarget.Texture2D"/>) of the Mandelbrot set bounded by the internal parameters, with given resolution.
        /// </summary>
        /// <returns>The texture id.</returns>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        public int GenerateTexture(int width, int height)
        {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            Bitmap bmp = new Bitmap(width, height);

            double dx = (xMax - xMin) / bmp.Width;
            double dy = (yMax - yMin) / bmp.Height;

            Parallel.For(0, bmp.Width, x =>
            {
                Parallel.For(0, bmp.Height, y =>
                {
                    Color newColor = DeepColoring(xMin + x * dx, yMin + y * dy);
                    bmp.SetPixel(x, y, newColor);
                });
            });

            #region Texture magic
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

            #endregion
            return id;
        }
    }
}
