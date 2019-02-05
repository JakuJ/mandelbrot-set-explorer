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
            //float bitsPerPixel = System.Drawing.Image.GetPixelFormatSize(format);
            int bitsPerPixel = ((int)pxFormat >> 8) & 0xFF;
            //Number of bits used to store the image data per line (only the valid data)
            int validBitsPerLine = width * bitsPerPixel;
            //4 bytes for every int32 (32 bits)
            int stride = ((validBitsPerLine + 31) / 32) * 4;
            return stride;
        }
    }
}
