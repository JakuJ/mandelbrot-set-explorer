using Mandelbrot;
using Mandelbrot.Rendering;

MandelbrotSet mandelbrot = new ParallelMandelbrot();

using var window = new Window(1200, 800, mandelbrot);
window.Run();
