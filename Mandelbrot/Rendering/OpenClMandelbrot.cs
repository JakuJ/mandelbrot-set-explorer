using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering
{
    public class OpenClMandelbrot : MandelbrotSet
    {
        /// <inheritdoc />
        /// <summary>
        /// Generates the <see cref="T:System.Drawing.Bitmap" /> using native OpenCL Mandelbrot set implementation.
        /// </summary>
        /// <returns>The bitmap.</returns>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        public override Image<Rgba32> Render(int width, int height)
        {
            // IntPtr memory = OpenClInterop.Render((uint)width, (uint)height, (uint)N, (uint)R, XMin, XMax, YMin, YMax);
            //
            // const PixelFormat bmpFormat = PixelFormat.Format32bppArgb;
            // int stride = GetStride(width, bmpFormat);
            //
            // return new Bitmap(width, height, stride, bmpFormat, memory);
            throw new NotImplementedException();
        }

        private static int GetStride(int width, PixelFormat pxFormat)
        {
            int bitsPerPixel = ((int) pxFormat >> 8) & 0xFF;
            int validBitsPerLine = width * bitsPerPixel;
            return (validBitsPerLine + 31) / 32 * 4;
        }
    }
}
