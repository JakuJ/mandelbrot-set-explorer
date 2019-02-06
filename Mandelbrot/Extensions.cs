using System;
using System.Drawing.Imaging;

namespace Mandelbrot
{
    public static class Extensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int i = Array.IndexOf(Arr, src) + 1;
            return (Arr.Length == i) ? Arr[0] : Arr[i];
        }

        public static int GetStride(int width, PixelFormat pxFormat)
        {
            int bitsPerPixel = ((int)pxFormat >> 8) & 0xFF;
            int validBitsPerLine = width * bitsPerPixel;
            int stride = (validBitsPerLine + 31) / 32 * 4;
            return stride;
        }
    }
}
