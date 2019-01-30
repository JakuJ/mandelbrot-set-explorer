using System;

namespace Mandelbrot
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Window mainWindow = new Window(640, 360)
            {
                VSync = OpenTK.VSyncMode.On
            };
            mainWindow.Run();
        }
    }
}
