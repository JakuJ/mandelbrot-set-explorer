using System;

namespace Mandelbrot.Rendering.Iteration;

public class NaiveIteration : IIterationAlgorithm
{
    public int Offset => 0;

    public unsafe void Iterate(uint* ptr, double cRe, double _, double cIm, double rSq, uint n)
    {
        double zRe = 0, zIm = 0;

        double zReSqr = 0;
        double zImSqr = 0;

        uint i = 1;
        for (; i < n; i++)
        {
            zIm = Math.FusedMultiplyAdd(zIm, zRe + zRe, cIm);

            zRe = zReSqr - zImSqr + cRe;
            zReSqr = zRe * zRe;
            zImSqr = zIm * zIm;

            if (zReSqr + zImSqr > rSq)
            {
                break;
            }
        }

        *ptr = i;
    }
}
