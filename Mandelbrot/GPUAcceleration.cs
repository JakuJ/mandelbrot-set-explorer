using System;
using System.Runtime.InteropServices;

namespace Mandelbrot
{
    public static class GpuAcceleration
    {
        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ListOpenCLDevices();

        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PrecompileKernels();

        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void OpenCLRender(out IntPtr memory,
            uint width,
            uint height,
            uint n,
            uint r,
            double xMin,
            double xMax,
            double yMin,
            double yMax);
    }
}