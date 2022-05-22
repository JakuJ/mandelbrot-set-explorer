using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering
{
    public abstract class Renderer : IDisposable
    {
        public double XMin = -2.5;

        public double XMax = 1.5;

        protected double YMin = -1.25;

        protected double YMax = 1.25;

        public int N = 200;

        public float R = 2;

        public abstract Shader Shader { get; protected set; }

        public virtual void Initialize()
        {
        }

        protected double Width => XMax - XMin;

        protected double Height => YMax - YMin;

        public void Zoom(double dx, double dy, double factor = 0)
        {
            factor = 1 + factor * 0.01;

            var newX = XMin + Width * dx;
            var newY = YMin + Height * dy;

            dx = Width / (2 * factor);
            dy = Height / (2 * factor);

            XMin = newX - dx;
            XMax = newX + dx;
            YMin = newY - dy;
            YMax = newY + dy;
        }

        public abstract void Render(int width, int height);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Shader.Dispose();
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
