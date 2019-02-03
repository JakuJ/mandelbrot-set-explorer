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
        public static extern void OpenCLRender(ref IntPtr memory, int width, int height, int N, int R, float xMin, float xMax, float yMin, float yMax);

        public static Bitmap GenerateBitmap(int width, int height, int N, int R, double xMin, double xMax, double yMin, double yMax)
        {
            IntPtr memory = IntPtr.Zero;
            int size = width * height * 3;

            OpenCLRender(ref memory, width, height, N, R, (float)xMin, (float)xMax, (float)yMin, (float)yMax);


            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            bmpData.Scan0 = memory;
            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}
