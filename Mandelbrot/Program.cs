using Mandelbrot;
using Mandelbrot.Rendering;

using var mandelbrot = new ParallelMandelbrot();

using var window = new Window(1200, 800, mandelbrot);
window.Run();
