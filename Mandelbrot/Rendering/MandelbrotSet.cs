using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering
{
    /// <summary>
    /// Represents the Mandelbrot set renderer
    /// </summary>
    public abstract class MandelbrotSet
    {
        /// <summary>
        /// The lower bound of the real axis.
        /// </summary>
        public double XMin = -2.5;

        /// <summary>
        /// The upper bound of the real axis.
        /// </summary>
        public double XMax = 1.5;

        /// <summary>
        /// The lower bound of the imaginary axis.
        /// </summary>
        protected double YMin = -1.25;

        /// <summary>
        /// The upper bound of the imaginary axis.
        /// </summary>
        protected double YMax = 1.25;

        /// <summary>
        /// The number of iterations before announcing a complex number non-divergent
        /// </summary>
        public int N = 200;

        /// <summary>
        /// The escape radius.
        /// </summary>
        public int R = 2;

        /// <summary>
        /// Gets the window width.
        /// </summary>
        /// <value>The width.</value>
        protected double Width => XMax - XMin;

        /// <summary>
        /// Gets the window height.
        /// </summary>
        /// <value>The height.</value>
        protected double Height => YMax - YMin;

        /// <summary>
        /// Zooms in or out at a specified location by a given factor.
        /// </summary>
        /// <param name="dx">Real part of the new location as a fraction of the window width.</param>
        /// <param name="dy">Imaginary part of the new location as a fraction of the window height.</param>
        /// <param name="factor">Zooming factor.</param>
        public void Zoom(double dx, double dy, double factor)
        {
            double newX = XMin + Width * dx;
            double newY = YMin + Height * dy;

            dx = Width / (2 * factor);
            dy = Height / (2 * factor);

            XMin = newX - dx;
            XMax = newX + dx;
            YMin = newY - dy;
            YMax = newY + dy;
        }

        /// <summary>
        /// Render the image with specified width and height.
        /// </summary>
        /// <returns>The rendered image object.</returns>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public abstract Image<Rgba32> Render(int width, int height);
    }
}
