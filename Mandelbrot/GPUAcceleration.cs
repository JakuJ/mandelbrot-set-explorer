using System;
using System.Runtime.InteropServices;

namespace Mandelbrot
{
    public static class GPUAcceleration
    {
        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListOpenCLDevices();

        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void OpenCLRender(out IntPtr memory, out bool format32bit, int width, int height, int N, int R, double xMin, double xMax, double yMin, double yMax);
    }
}
