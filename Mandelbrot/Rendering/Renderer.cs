using System;

namespace Mandelbrot.Rendering;

public abstract class Renderer : IDisposable
{
    public uint N = 200;
    public float M = 0.25f;
    public float R = 2;

    public double XMax = 1.5;
    public double XMin = -2.5;

    protected double YMax = 1.25;
    protected double YMin = -1.25;

    protected abstract Shader? Shader { get; set; }

    public double Width => XMax - XMin;

    protected double Height => YMax - YMin;

    public abstract void Initialize(out int vbo, out int vao);

    public virtual void Render(int width, int height)
    {
    }

    public virtual void OnChange()
    {
    }

    public void Zoom(double dx, double dy, double factor = 0)
    {
        factor = Math.Abs(1 + factor * 0.01);

        var newX = XMin + Width * dx;
        var newY = YMin + Height * dy;

        dx = Width / (2 * factor);
        dy = Height / (2 * factor);

        XMin = newX - dx;
        XMax = newX + dx;
        YMin = newY - dy;
        YMax = newY + dy;
    }

    public void CopyParams(Renderer other)
    {
        XMin = other.XMin;
        XMax = other.XMax;
        YMin = other.YMin;
        YMax = other.YMax;
        N = other.N;
        M = other.M;
        R = other.R;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing) Shader?.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
