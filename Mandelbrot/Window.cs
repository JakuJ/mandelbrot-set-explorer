using System;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Mandelbrot
{
    /// <summary>
    /// Main rendering window class
    /// </summary>
    public sealed class Window : GameWindow
    {
        /// <summary>
        /// Mouse wheel mode.
        /// </summary>
        private enum MouseWheelMode
        {
            Resolution,
            ZoomFactor,
            Iterations,
            EscapeRadius
        };
        /// <summary>
        /// A <see cref="MouseWheelMode"/> instance directing which rendering parameter will the mouse wheel change.
        /// </summary>
        private MouseWheelMode mode;
        /// <summary>
        /// The <see cref="MandelbrotSet"/> instance used to render the set.
        /// </summary>
        private readonly MandelbrotSet mandelbrot;
        /// <summary>
        /// Current image <see cref="TextureTarget.Texture2D"/> id.
        /// </summary>
        private int texture;
        /// <summary>
        /// The click zoom factor.
        /// </summary>
        private double zoomFactor;
        /// <summary>
        /// The ratio of the resolution of the generated image to the resolution of the actual window.
        /// </summary>
        private int resolution;
        /// <summary>
        /// The base title for the window.
        /// </summary>
        readonly string BaseTitle;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mandelbrot.Window"/> class.
        /// </summary>
        /// <param name="width">Window width.</param>
        /// <param name="height">Window height.</param>
        public Window(int width, int height, MandelbrotSet mandelbrot) : base(width, height)
        {
            this.mandelbrot = mandelbrot;
            BaseTitle = "Mandelbrot Set";
            mode = MouseWheelMode.ZoomFactor;
            zoomFactor = 2;
            resolution = 100;

            UpdateTitle();
            GL.Enable(EnableCap.Texture2D);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;
            texture = mandelbrot.GenerateTexture(Width * resolution / 100, Height * resolution / 100);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            double factor = (e.Button == MouseButton.Left) ? zoomFactor : 1 / zoomFactor;

            mandelbrot.Zoom(e.X / (double)Width, e.Y / (double)Height, factor);
            RedrawTexture();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            switch (mode)
            {
                case MouseWheelMode.Resolution:
                    resolution = Math.Max(25, resolution - (25 * e.Delta));
                    break;
                case MouseWheelMode.EscapeRadius:
                    try
                    {
                        int newRadius = (int)(mandelbrot.R / Math.Pow(2, e.Delta));
                        mandelbrot.R = Math.Min(32768, Math.Max(2, newRadius));
                    }
                    catch (ArithmeticException) { }
                    break;
                case MouseWheelMode.Iterations:
                    mandelbrot.N = Math.Max(25, mandelbrot.N - (25 * e.Delta));
                    break;
                case MouseWheelMode.ZoomFactor:
                    zoomFactor = Math.Max(1, zoomFactor - e.Delta);
                    UpdateTitle();
                    return;
            }

            RedrawTexture();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Escape:
                    Exit();
                    break;
                case Key.Space:
                    mode = mode.Next();
                    UpdateTitle();
                    break;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex2(-1, 1);

            GL.TexCoord2(1, 0);
            GL.Vertex2(1, 1);

            GL.TexCoord2(1, 1);
            GL.Vertex2(1, -1);

            GL.TexCoord2(0, 1);
            GL.Vertex2(-1, -1);

            GL.End();

            SwapBuffers();
        }

        /// <summary>
        /// Generates new texture using the <see cref="mandelbrot"/> <see cref="MandelbrotSet"/> instance.
        /// </summary>
        private void RedrawTexture()
        {
            GL.DeleteTexture(texture);

            DateTime start = DateTime.UtcNow;
            texture = mandelbrot.GenerateTexture(Width * resolution / 100, Height * resolution / 100);
            DateTime end = DateTime.UtcNow;

            UpdateTitle((end - start).TotalSeconds);
        }
        /// <summary>
        /// Updates the window title.
        /// </summary>
        /// <param name="timeElapsed">Last rendering time.</param>
        private void UpdateTitle(double timeElapsed = 0)
        {
            Title = string.Format("{0} – Res: {1}% - Zoom: {2}x, 10^{7:F1} - Speed: {3:F3}s - N: {4} - R: {5} - Mode: {6}",
                BaseTitle,
                resolution,
                zoomFactor,
                timeElapsed,
                mandelbrot.N,
                mandelbrot.R,
                Enum.GetName(typeof(MouseWheelMode), mode),
                Math.Log10(mandelbrot.xMax - mandelbrot.xMin));
        }
    }
}
