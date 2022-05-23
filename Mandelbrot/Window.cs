using System;
using System.Diagnostics;
using System.IO;
using Mandelbrot.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Mandelbrot;

public sealed class Window : GameWindow
{
    private bool changed = true;
    private Renderer renderer;
    private int resolution = 100;
    private int vertexArrayObject;
    private int vertexBufferObject;

    public Window(int width, int height)
        : base(GameWindowSettings.Default,
            new NativeWindowSettings
            {
                Profile = ContextProfile.Core,
                APIVersion = Version.Parse("4.1"),
                Size = new Vector2i(width, height),
                Flags = ContextFlags.ForwardCompatible,
            })
    {
        renderer = new OpenGl();
    }

    private int ImageWidth => ClientSize.X * resolution / 100;

    private int ImageHeight => ClientSize.Y * resolution / 100;

    private void SwitchRenderer()
    {
        var old = renderer;
        old.Dispose();

        renderer = old is OpenGl ? new NativeRenderer() : new OpenGl();

        renderer.CopyParams(old);
        renderer.Initialize(out vertexBufferObject, out vertexArrayObject);
        renderer.OnChange();
        renderer.Render(ImageWidth, ImageHeight);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
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
            renderer.Zoom(0.5 - e.DeltaX / ClientSize.X, 1 - (0.5 - e.DeltaY / ClientSize.Y));
            changed = true;
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        var delta = MouseState.ScrollDelta.Y;

        if (IsKeyDown(Keys.D1))
        {
            renderer.R = MathF.Min(32768f, MathF.Max(2f, renderer.R + delta));
        }
        else if (IsKeyDown(Keys.D2))
        {
            renderer.M = MathF.Min(1, MathF.Max(.01f, renderer.M + delta / 100f));
        }
        else if (IsKeyDown(Keys.D3))
        {
            renderer.N = Math.Max(25, renderer.N + (uint) (25 * MathF.Sign(delta)));
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

        changed = true;

        base.OnMouseWheel(e);
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        switch (e.Key)
        {
            case Keys.Escape:
                Close();
                break;
            case Keys.R:
                SwitchRenderer();
                break;
            case Keys.S:
                SaveImage();
                break;
            case Keys.D0:
                resolution = resolution switch
                {
                    100 => 50,
                    50 => 25,
                    _ => 100,
                };
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

        if (changed)
        {
            renderer.OnChange();
            changed = false;
        }

        renderer.Render(ImageWidth, ImageHeight);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.BindVertexArray(vertexArrayObject);
        GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

        SwapBuffers();

        UpdateTitle(stopwatch.ElapsedMilliseconds);
    }

    private void SaveImage()
    {
        using var image = new ContiguousImage<Rgba32>(ImageWidth, ImageHeight);

        unsafe
        {
            GL.ReadPixels(0, 0, ImageWidth, ImageHeight, PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr) image.Pointer);
        }

        Directory.CreateDirectory("Captured");
        image.Image.Mutate(x => x.Flip(FlipMode.Vertical));
        image.Image.SaveAsBmp($"Captured/capture-{DateTime.Now.ToLongTimeString()}.bmp");
    }

    private void UpdateTitle(double timeElapsed)
    {
        var zoom = Math.Log10(renderer.XMax - renderer.XMin);
        var rendering = renderer is OpenGl ? "OpenGL" : "Native";
        Title = $"{rendering} – Res: {resolution}% - Zoom: 1e{zoom:F1} - Speed: {timeElapsed:000}ms - N: {renderer.N} - R: {renderer.R:F1} - M: {renderer.M:F2}";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) renderer.Dispose();
        base.Dispose(disposing);
    }
}
