using System;
using Mandelbrot.Rendering;

namespace Mandelbrot
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            MandelbrotSet mandelbrot;

            try
            {
                OpenClInterop.InitializeOpenCl();
                mandelbrot = new OpenClMandelbrot();
            }
            catch
            {
                Console.WriteLine("Failed to initialize OpenCL-based renderer.");
                Console.WriteLine("Using Parallel.For as fallback.");
                mandelbrot = new ParallelMandelbrot();
            }

            new Window(1000, 625, mandelbrot).Run();
        }
    }
}