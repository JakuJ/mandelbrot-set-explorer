using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering
{
    public abstract class MandelbrotSet
    {
        public double XMin = -2.5;

        public double XMax = 1.5;

        protected double YMin = -1.25;

        private double yMax = 1.25;

        public int N = 200;

        public int R = 2;

        protected double Width => XMax - XMin;

        protected double Height => yMax - YMin;

        public void Zoom(double dx, double dy, double factor)
        {
            double newX = XMin + Width * dx;
            double newY = YMin + Height * dy;

            dx = Width / (2 * factor);
            dy = Height / (2 * factor);

            XMin = newX - dx;
            XMax = newX + dx;
            YMin = newY - dy;
            yMax = newY + dy;
        }

        public abstract Image<Rgba32> Render(int width, int height);
    }
}
