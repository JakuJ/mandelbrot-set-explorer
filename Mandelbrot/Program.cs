using System;

namespace Mandelbrot
{
    public class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            GPUAcceleration.ListOpenCLDevices();
            GPUAcceleration.PrecompileKernels();

            Window mainWindow = new Window(1000, 625, new OpenCLMandelbrot());
            mainWindow.Run();
        }
    }
}