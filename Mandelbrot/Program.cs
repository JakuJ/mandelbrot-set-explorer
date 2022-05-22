using Mandelbrot;
using Mandelbrot.Rendering;

using var renderer = new OpenGl();
using Window window = new(1200, 1000, renderer);

window.Run();
