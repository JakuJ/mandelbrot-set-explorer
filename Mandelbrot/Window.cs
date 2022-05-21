using System;
using System.Diagnostics;
using System.IO;
using Mandelbrot.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Mandelbrot
{
    public sealed class Window : GameWindow
    {
        private const string BaseTitle = "Mandelbrot Set";
        private readonly MandelbrotSet mandelbrot;
        private int resolution = 100;
        private int vertexBufferObject;
        private int vertexArrayObject;
        private readonly Shader shader;
        private bool render = true;

        private int ImageWidth => Size.X * resolution / 100;

        private int ImageHeight => Size.Y * resolution / 100;

        public Window(int width, int height, MandelbrotSet mandelbrot)
            : base(GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    Profile = ContextProfile.Core,
                    APIVersion = Version.Parse("3.3"),
                    Size = new Vector2i(width, height),
                    Flags = ContextFlags.ForwardCompatible,
                })
        {
            this.mandelbrot = mandelbrot;
            shader = new Shader("shader.vert", "shader.frag");

            var handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X * 2, Size.Y * 2);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            shader.Use();
            GL.LoadIdentity();

            GL.ClearColor(0, 0, 0, 1);

            float[] vertices =
            {
                -1f, -1f, 0f, 0f, 0f,
                1f, -1f, 0f, 1f, 0f,
                1f, 1f, 0f, 1f, 1f,
                -1f, 1f, 0f, 0f, 1f
            };

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            var positionLocation = shader.GetAttribLocation("aPosition");
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(positionLocation);

            var texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            Render();
            base.OnLoad();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (MouseState.IsButtonDown(MouseButton.Button1))
            {
                mandelbrot.Zoom(0.5 - e.DeltaX / Size.X, 1 - (0.5 - e.DeltaY / Size.Y));
                render = true;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var delta = MathF.Sign(MouseState.ScrollDelta.Y);

            if (IsKeyDown(Keys.D1))
            {
                mandelbrot.R = (int) MathF.Min(32768, MathF.Max(2, mandelbrot.R + delta));
            }
            else if (IsKeyDown(Keys.D2))
            {
                mandelbrot.N = Math.Max(25, mandelbrot.N + 25 * delta);
            }
            else
            {
                var factor = MouseState.ScrollDelta.Y * 0.01f;

                var dx = MousePosition.X / Size.X - .5f;
                var dy = .5f - MousePosition.Y / Size.Y;
                var vec = new Vector2(dx, dy);

                vec *= 1 - 1 / MathF.Pow(2, factor);
                vec *= 1.3333f;

                mandelbrot.Zoom(.5 + vec.X, .5 + vec.Y, MouseState.ScrollDelta.Y);
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
            GL.DeleteProgram(shader.Handle);

            base.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (render)
            {
                Render();
                render = false;
            }

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(vertexArrayObject);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            SwapBuffers();
        }

        private void SaveImage()
        {
            Directory.CreateDirectory("Captured");

            var img = mandelbrot.RenderToImage(ImageWidth, ImageHeight);
            img.Mutate(x => x.Flip(FlipMode.Vertical));
            img.SaveAsBmp($"Captured/{DateTime.Now.ToLongTimeString()}.bmp");
        }

        private unsafe void GenerateTexture()
        {
            var image = mandelbrot.Render(ImageWidth, ImageHeight);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                ImageWidth,
                ImageHeight,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                (IntPtr) image);
        }

        private void Render()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            GenerateTexture();

            UpdateTitle(stopwatch.ElapsedMilliseconds);
        }

        private void UpdateTitle(double timeElapsed = 0)
        {
            var zoom = Math.Log10(mandelbrot.XMax - mandelbrot.XMin);
            Title = $"{BaseTitle} – Res: {resolution}% - Zoom: 10^{zoom:F1} - Speed: {timeElapsed}ms - N: {mandelbrot.N} - R: {mandelbrot.R}";
        }
    }
}
