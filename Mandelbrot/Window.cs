using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Mandelbrot
{
    public sealed class Window : GameWindow
    {
        readonly MandelbrotSet mandelbrot;
        int texture;
        readonly double scale;

        public Window(int _width, int _height) : base(_width, _height,
            GraphicsMode.Default,
            "Mandelbrot Set Zoom")
        {
            mandelbrot = new MandelbrotSet();
            scale = 8;
            GL.Enable(EnableCap.Texture2D);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;
            texture = mandelbrot.GenerateTexture(Width, Height);
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

            double factor = (e.Button == MouseButton.Left) ? scale : 1 / scale;

            double dx = mandelbrot.xMax - mandelbrot.xMin;
            double mX = mandelbrot.xMin + dx * e.X / Width;

            double dy = mandelbrot.yMax - mandelbrot.yMin;
            double mY = mandelbrot.yMin + dy * e.Y / Height;

            mandelbrot.xMin = mX - dx / factor;
            mandelbrot.xMax = mX + dx / factor;
            mandelbrot.yMin = mY - dy / factor;
            mandelbrot.yMax = mY + dy / factor;

            GL.DeleteTexture(texture);
            texture = mandelbrot.GenerateTexture(Width, Height);
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
