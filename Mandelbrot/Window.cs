using System;
using System.Diagnostics;
using SixLabors.ImageSharp;
using System.IO;
using Mandelbrot.Rendering;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Mandelbrot
{
    public sealed class Window : GameWindow
    {
        private enum MouseWheelMode
        {
            Resolution,
            ZoomFactor,
            Iterations,
            EscapeRadius
        }

        private readonly float[] vertices =
        {
            -1f, -1f, 0f, 0f, 0f,
            1f, -1f, 0f, 1f, 0f,
            1f, 1f, 0f, 1f, 1f,
            -1f, 1f, 0f, 0f, 1f
        };

        private const string BaseTitle = "Mandelbrot Set";
        private readonly MandelbrotSet mandelbrot;
        private MouseWheelMode mode = MouseWheelMode.ZoomFactor;
        private double zoomFactor = 2;
        private int resolution = 200;
        private int vertexBufferObject;
        private int vertexArrayObject;
        private readonly Shader shader;
        private byte[] imageBuffer = Array.Empty<byte>();

        private int ImageWidth => Size.X * resolution / 100;

        private int ImageHeight => Size.Y * resolution / 100;

        public Window(int width, int height, MandelbrotSet mandelbrot)
            : base(GameWindowSettings.Default,
                new NativeWindowSettings
                {
                    Profile = ContextProfile.Core,
                    APIVersion = Version.Parse("3.3"),
                    Size = new Vector2i(width, height),
                    Flags = ContextFlags.ForwardCompatible
                })
        {
            this.mandelbrot = mandelbrot;
            shader = new Shader("shader.vert", "shader.frag");

            UpdateTitle();

            var handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);

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

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);

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

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var factor = e.Button == MouseButton.Left ? zoomFactor : 1 / zoomFactor;

            mandelbrot.Zoom(MousePosition.X / (double) Size.X, MousePosition.Y / (double) Size.Y, factor);
            Render();

            base.OnMouseDown(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var delta = MathF.Sign(MouseState.ScrollDelta.Y);

            switch (mode)
            {
                case MouseWheelMode.Resolution:
                    resolution = Math.Max(25, resolution - 25 * delta);
                    break;
                case MouseWheelMode.EscapeRadius:
                    try
                    {
                        mandelbrot.R = Math.Min(32768, Math.Max(2, mandelbrot.R + delta));
                    }
                    catch (ArithmeticException)
                    {
                    }

                    break;
                case MouseWheelMode.Iterations:
                    mandelbrot.N = Math.Max(25, mandelbrot.N - 25 * delta);
                    break;
                case MouseWheelMode.ZoomFactor:
                    zoomFactor = Math.Max(1, zoomFactor - delta);
                    UpdateTitle();
                    return;
            }

            Render();
            base.OnMouseWheel(e);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Space:
                    mode = mode.Next();
                    UpdateTitle();
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

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BindVertexArray(vertexArrayObject);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            SwapBuffers();
        }

        private void SaveImage()
        {
            Directory.CreateDirectory("Captured");
            var img = mandelbrot.Render(ImageWidth, ImageHeight);
            img.SaveAsBmp($"Captured/{DateTime.Now.ToLongTimeString()}.bmp");
        }

        private void GenerateTexture()
        {
            var image = mandelbrot.Render(ImageWidth, ImageHeight);

            if (imageBuffer.Length < ImageWidth * ImageHeight * 4)
            {
                imageBuffer = new byte[4 * ImageWidth * ImageHeight];
            }

            image.ProcessPixelRows(access =>
            {
                var i = 0;
                for (var y = image.Height - 1; y >= 0; --y)
                {
                    var row = access.GetRowSpan(y);
                    foreach (var color in row)
                    {
                        imageBuffer[i++] = color.R;
                        imageBuffer[i++] = color.G;
                        imageBuffer[i++] = color.B;
                        imageBuffer[i++] = color.A;
                    }
                }
            });

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                image.Width,
                image.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                imageBuffer);
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
            Title = string.Format(
                "{0} – Res: {1}% - Zoom: {2}x, 10^{7:F1} - Speed: {3}ms - N: {4} - R: {5} - Mode: {6}",
                BaseTitle,
                resolution,
                zoomFactor,
                timeElapsed,
                mandelbrot.N,
                mandelbrot.R,
                Enum.GetName(typeof(MouseWheelMode), mode),
                Math.Log10(mandelbrot.XMax - mandelbrot.XMin));
        }
    }
}
