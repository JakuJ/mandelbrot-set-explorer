using System;
using Mandelbrot.Rendering;

namespace Mandelbrot
{
    public static class Program
    {
        public static void Main()
        {
            MandelbrotSet mandelbrot;

            try
            {
                // OpenClInterop.InitializeOpenCl();
                mandelbrot = new ParallelMandelbrot();
            }
            catch
            {
                Console.WriteLine("Failed to initialize OpenCL-based renderer.");
                Console.WriteLine("Using Parallel.For as fallback.");
                mandelbrot = new ParallelMandelbrot();
            }

            using var window = new Window(1000, 625, mandelbrot);
            window.Run();
        }
    }
}
