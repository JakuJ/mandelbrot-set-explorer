using System;

namespace Mandelbrot
{
    public static class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            GpuAcceleration.ListOpenCLDevices();
            GpuAcceleration.PrecompileKernels();

            Window mainWindow = new(1000, 625, new OpenClMandelbrot());
            mainWindow.Run();
        }
    }
}