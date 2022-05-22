using System;
using System.Diagnostics;
using Mandelbrot.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Mandelbrot
{
    public sealed class Window : GameWindow
    {
        private const string BaseTitle = "Mandelbrot Set";
        private readonly Renderer renderer;
        private int resolution = 100;
        private int vertexBufferObject;
        private int vertexArrayObject;
        private bool render = true;

        private int ImageWidth => Size.X * resolution / 100;

        private int ImageHeight => Size.Y * resolution / 100;

        public Window(int width, int height, Renderer renderer)
            : base(GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    Profile = ContextProfile.Core,
                    APIVersion = Version.Parse("4.1"),
                    Size = new Vector2i(width, height),
                    Flags = ContextFlags.ForwardCompatible,
                })
        {
            this.renderer = renderer;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X * 2, Size.Y * 2);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0, 0, 0, 1);

            renderer.Initialize(out vertexBufferObject, out vertexArrayObject);

            base.OnLoad();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (MouseState.IsButtonDown(MouseButton.Button1))
            {
                renderer.Zoom(0.5 - e.DeltaX / Size.X, 1 - (0.5 - e.DeltaY / Size.Y));
                render = true;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var delta = MouseState.ScrollDelta.Y;
            var deltaI = MathF.Sign(delta);

            if (IsKeyDown(Keys.D1))
            {
                renderer.R = MathF.Min(32768f, MathF.Max(2f, renderer.R + delta));
            }
            else if (IsKeyDown(Keys.D2))
            {
                renderer.N = Math.Max(25, renderer.N + 25 * deltaI);
            }
            else
            {
                var factor = MouseState.ScrollDelta.Y * 0.01f;

                var dx = MousePosition.X / Size.X - .5f;
                var dy = .5f - MousePosition.Y / Size.Y;
                var vec = new Vector2(dx, dy);

                vec *= 1 - 1 / MathF.Pow(2, factor);
                vec *= 1.3333f;

                renderer.Zoom(.5 + vec.X, .5 + vec.Y, MouseState.ScrollDelta.Y);
            }

            render = true;

            base.OnMouseWheel(e);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Escape:
                    Close();
                    break;
                case Keys.D3:
                    resolution = resolution switch
                    {
                        100 => 200,
                        200 => 50,
                        _ => 100
                    };
                    render = true;
                    break;
                case Keys.S:
                    SaveImage();
                    break;
            }

            base.OnKeyDown(e);
        }

        protected override void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(vertexBufferObject);
            GL.DeleteVertexArray(vertexArrayObject);

            base.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            Stopwatch stopwatch = new();
            stopwatch.Start();

            if (render)
            {
                renderer.Render(ImageWidth, ImageHeight);
                render = false;
            }

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(vertexArrayObject);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            SwapBuffers();

            UpdateTitle(stopwatch.ElapsedMilliseconds);
        }

        private void SaveImage()
        {
            // TODO
            // Directory.CreateDirectory("Captured");
            //
            // img.Mutate(x => x.Flip(FlipMode.Vertical));
            // img.SaveAsBmp($"Captured/{DateTime.Now.ToLongTimeString()}.bmp");
        }

        private void UpdateTitle(double timeElapsed = 0)
        {
            var zoom = Math.Log10(renderer.XMax - renderer.XMin);
            Title = $"{BaseTitle} – Res: {resolution}% - Zoom: 1e{zoom:F1} - Speed: {timeElapsed}ms - N: {renderer.N} - R: {renderer.R:F1}";
        }
    }
}
