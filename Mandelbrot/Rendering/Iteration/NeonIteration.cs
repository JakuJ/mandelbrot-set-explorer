using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;

namespace Mandelbrot.Rendering.Iteration;

public class NeonIteration : IIterationAlgorithm
{
    public int Offset => 1;

    public unsafe void Iterate(uint* ptr, double re, double dRe, double im, double rSq, uint n)
    {
        var dataRe = stackalloc[] { re, re + dRe };
        var dataIm = stackalloc[] { im, im };

        var vRe = AdvSimd.LoadVector128(dataRe);
        var vIm = AdvSimd.LoadVector128(dataIm);

        var vZre = Vector128<double>.Zero;
        var vZim = Vector128<double>.Zero;
        var vZreSq = Vector128<double>.Zero;
        var vZimSq = Vector128<double>.Zero;

        var dataTwo = stackalloc[] { 2.0 };
        var vTwo = AdvSimd.LoadVector64(dataTwo);

        var radiusData = stackalloc[] { rSq, rSq };
        var vRad = AdvSimd.LoadVector128(radiusData);

        var mask = stackalloc ulong[2];
        var vMask = Vector128<ulong>.Zero;

        *ptr = 0;
        *(ptr + 1) = 0;

        for (uint i = 0; i < n; i++)
        {
            var zSqSum = AdvSimd.Arm64.Add(vZreSq, vZimSq);
            var vCmp = AdvSimd.Arm64.CompareGreaterThan(zSqSum, vRad).AsUInt64();
            vMask = AdvSimd.Or(vMask, vCmp);

            AdvSimd.Store(mask, vMask);

            if (*ptr == 0 && (mask[0] & 1) == 1)
            {
                *ptr = i;
            }

            if (*(ptr + 1) == 0 && (mask[1] & 1) == 1)
            {
                *(ptr + 1) = i;
            }

            if ((mask[0] & mask[1]) != 0)
            {
                return;
            }

            var vZRe2 = AdvSimd.Arm64.MultiplyByScalar(vZre, vTwo);
            vZim = AdvSimd.Arm64.FusedMultiplyAdd(vIm, vZim, vZRe2);

            vZre = AdvSimd.Arm64.Subtract(vZreSq, vZimSq);
            vZre = AdvSimd.Arm64.Add(vZre, vRe);

            vZreSq = AdvSimd.Arm64.Multiply(vZre, vZre);
            vZimSq = AdvSimd.Arm64.Multiply(vZim, vZim);
        }
    }
}
