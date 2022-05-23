namespace Mandelbrot.Rendering.Iteration;

public interface IIterationAlgorithm
{
    public int Offset { get; }

    unsafe void Iterate(uint* ptr, double re, double dRe, double im, double rSq, uint n);
}
