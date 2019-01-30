using System;
using System.Numerics;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace Mandelbrot
{
    public class MandelbrotSet
    {
        public double xMin, xMax, yMin, yMax;

        public MandelbrotSet()
        {
            xMin = -2.5;
            xMax = 1.5;
            yMin = -1.25;
            yMax = 1.25;
        }

        private Color DeepColoring(Complex c, int N = 200, double R = 100, double Epsilon = 0.001)
        {
            Complex z = c;
            Complex dz = 1;
            double power = 1;
            int i = 0;

            while (i < N)
            {
                if (dz.Magnitude < Epsilon)
                    break;

                if (z.Magnitude > R)
                    return GetColor(z.Magnitude / power);

                dz = 2 * dz * z;
                z = z * z + c;

                power *= 2;
                i++;
            }
            return Color.FromArgb(0, 0, 0);
        }

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
                    Complex c = new Complex(xMin + x * dx, yMin + y * dy);
                    Color newColor = DeepColoring(c);
                    bmp.SetPixel(x, y, newColor);
                });
            });

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
}
