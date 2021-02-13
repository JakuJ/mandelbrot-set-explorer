using System;

namespace Mandelbrot
{
    public static class Extensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
            }

            var arr = (T[])Enum.GetValues(src.GetType());
            int i = Array.IndexOf(arr, src) + 1;
            return arr[i % arr.Length];
        }
    }
}