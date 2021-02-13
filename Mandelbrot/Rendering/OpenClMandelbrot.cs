using System;
using System.Drawing;
using System.Drawing.Imaging;

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
        public override Bitmap Render(int width, int height)
        {
            IntPtr memory = OpenClInterop.Render((uint)width, (uint)height, (uint)N, (uint)R, XMin, XMax, YMin, YMax);

            const PixelFormat bmpFormat = PixelFormat.Format32bppArgb;
            int stride = GetStride(width, bmpFormat);

            return new Bitmap(width, height, stride, bmpFormat, memory);
        }

        private static int GetStride(int width, PixelFormat pxFormat)
        {
            int bitsPerPixel = ((int)pxFormat >> 8) & 0xFF;
            int validBitsPerLine = width * bitsPerPixel;
            return (validBitsPerLine + 31) / 32 * 4;
        }
    }
}