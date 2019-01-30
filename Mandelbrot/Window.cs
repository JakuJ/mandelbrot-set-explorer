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
        private readonly double zoomFactor;
        /// <summary>
        /// The ratio of the resolution of the generated image to the resolution of the actual window.
        /// </summary>
        double resolution;
        /// <summary>
        /// The base title for the window.
        /// </summary>
        readonly string BaseTitle;
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mandelbrot.Window"/> class.
        /// </summary>
        /// <param name="_width">Window width.</param>
        /// <param name="_height">Window height.</param>
        public Window(int _width, int _height) : base(_width, _height)
        {
            mandelbrot = new MandelbrotSet();
            Title = BaseTitle = "Mandelbrot Set";

            zoomFactor = 8;
            resolution = 1;

            GL.Enable(EnableCap.Texture2D);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;
            texture = mandelbrot.GenerateTexture((int)(Width * resolution), (int)(Height * resolution));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            double factor = (e.Button == MouseButton.Left) ? zoomFactor : 1 / zoomFactor;

            double dx = mandelbrot.xMax - mandelbrot.xMin;
            double mX = mandelbrot.xMin + dx * e.X / Width;

            double dy = mandelbrot.yMax - mandelbrot.yMin;
            double mY = mandelbrot.yMin + dy * e.Y / Height;

            mandelbrot.xMin = mX - dx / factor;
            mandelbrot.xMax = mX + dx / factor;
            mandelbrot.yMin = mY - dy / factor;
            mandelbrot.yMax = mY + dy / factor;

            RedrawTexture();
        }
        /// <summary>
        /// Generates new texture using the <see cref="mandelbrot"/> <see cref="MandelbrotSet"/> instance.
        /// </summary>
        private void RedrawTexture()
        {
            GL.DeleteTexture(texture);

            DateTime start = DateTime.UtcNow;
            texture = mandelbrot.GenerateTexture((int)(Width * resolution), (int)(Height * resolution));
            DateTime end = DateTime.UtcNow;

            Title = $"{BaseTitle} – Resolution: {100 * resolution:0.##}% - Render speed: {(end - start).TotalSeconds:0.###}s";
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            resolution = Math.Max(resolution - 0.1D * e.Delta, 0.1);
            RedrawTexture();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindTexture(TextureTarget.Texture2D, texture);
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
    }
}
