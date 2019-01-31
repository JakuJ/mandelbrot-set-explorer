using System;

namespace Mandelbrot
{
    public class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            Window mainWindow = new Window(1000, 625)
            {
                VSync = OpenTK.VSyncMode.On
            };
            mainWindow.Run(30, 30);
        }
    }
}
