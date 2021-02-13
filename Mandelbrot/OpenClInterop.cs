using System;
using System.Runtime.InteropServices;

namespace Mandelbrot
{
    public static class OpenClInterop
    {
        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ListOpenCLDevices();

        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void PrecompileKernels();

        [DllImport("OpenCL/OpenCLRendering.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void OpenCLRender(out IntPtr memory, uint width, uint height, uint n, uint r,
                                                double xMin, double xMax, double yMin, double yMax);

        public static IntPtr Render(uint width, uint height, uint n, uint r,
                                    double xMin, double xMax, double yMin, double yMax)
        {
            OpenCLRender(out IntPtr memory, width, height, n, r, xMin, xMax, yMin, yMax);
            return memory;
        }

        public static void InitializeOpenCl()
        {
            ListOpenCLDevices();
            PrecompileKernels();
        }
    }
}