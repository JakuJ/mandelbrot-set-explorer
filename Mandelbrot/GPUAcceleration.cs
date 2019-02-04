using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace Mandelbrot
{
    public static class GPUAcceleration
    {
        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListOpenCLDevices();

        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void OpenCLRender(out IntPtr memory, int width, int height, int N, int R, double xMin, double xMax, double yMin, double yMax);

        /// <summary>
        /// Generates the <see cref="Bitmap"/> using native OpenCL Mandelbrot set implementation.
        /// </summary>
        /// <returns>The bitmap.</returns>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="N">Number of iterations.</param>
        /// <param name="R">Escape radius.</param>
        /// <param name="xMin">Real part lower bound.</param>
        /// <param name="xMax">Real part upper bound.</param>
        /// <param name="yMin">Imaginary part lower bound.</param>
        /// <param name="yMax">Imaginary part upper bound.</param>
        public static Bitmap GenerateBitmap(int width, int height, int N, int R, double xMin, double xMax, double yMin, double yMax)
        {
            int size = width * height * 3;

            OpenCLRender(out IntPtr memory, width, height, N, R, xMin, xMax, yMin, yMax);

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            bmpData.Scan0 = memory;
            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}
