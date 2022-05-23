using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering;

public class NativeRenderer : Renderer, IDisposable
{
    private ContiguousImage<Rgba32>? image;
    private CancellationTokenSource cancellation = new();
    private readonly List<Task> tasks = new();

    private volatile bool filling;

    protected override Shader? Shader { get; set; }

    public override void Initialize(out int vbo, out int vao)
    {
        Shader = new Shader("Shaders/texture.vert", "Shaders/texture.frag");
        Shader.Use();

        var handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

        float[] vertices =
        {
            -1f, -1f, 0f, 0f, 0f,
            1f, -1f, 0f, 1f, 0f,
            1f, 1f, 0f, 1f, 1f,
            -1f, 1f, 0f, 0f, 1f,
        };

        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        var positionLocation = Shader.GetAttribLocation("aPosition");
        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(positionLocation);

        var texCoordLocation = Shader.GetAttribLocation("aTexCoord");
        GL.EnableVertexAttribArray(texCoordLocation);
        GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
    }

    private void SpawnRenderingThreads()
    {
        var n = Environment.ProcessorCount - 1;

        for (var i = 0; i < n; ++i)
        {
            tasks.Add(Task.Run(MonteCarlo, cancellation.Token));
        }

        tasks.Add(Task.Run(() => FillIn(0, 1, true), cancellation.Token));
    }

    public override void OnChange()
    {
        filling = false;

        if (Shader is null) return;

        GL.Uniform1(Shader.GetUniformLocation("N"), N);
        GL.Uniform1(Shader.GetUniformLocation("M"), M);

        base.OnChange();
    }

    public override void Render(int width, int height)
    {
        if (Shader is null) return;

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.R32ui,
            width,
            height,
            0,
            PixelFormat.RedInteger,
            PixelType.UnsignedInt,
            RenderTexture(width, height));
    }

    private unsafe void MonteCarlo()
    {
        Random random = new();

        while (!cancellation.IsCancellationRequested)
        {
            var dx = Width / image!.Image.Width;
            var dy = Height / image.Image.Height;

            for (var i = 0; i < 1000; i++)
            {
                var x = random.Next(image.Image.Width);
                var y = random.Next(image.Image.Height);

                var row = (uint*) image.Pointer + y * image.Image.Width + x;
                *row = ParallelIteration(
                    Math.FusedMultiplyAdd(x, dx, XMin),
                    Math.FusedMultiplyAdd(y, dy, YMin)
                );
            }
        }
    }

    private unsafe void FillIn(int index, int all, bool reverse = false)
    {
        var step = image!.Image.Height / all;

        var startY = step * index;
        var stopY = step * (index + 1) - (all - index == 1 ? 1 : 0);

        if (stopY >= image.Image.Height)
            throw new ArgumentException($"Height {stopY} out of bounds for image of height {image.Image.Height}");

        while (true)
        {
            for (var i = startY; i < stopY; ++i)
            {
                if (cancellation.IsCancellationRequested) return;
                if (!filling)
                {
                    Thread.Sleep(1000);
                    filling = true;
                    break;
                }

                var y = reverse ? stopY - (i - startY) : i;

                var dx = Width / image.Image.Width;
                var dy = Height / image.Image.Height;

                var yClr = Math.FusedMultiplyAdd(y, dy, YMin);

                var ptr = (uint*) image.Pointer + y * image.Image.Width;

                for (var x = 0; x < image.Image.Width; x++)
                {
                    *ptr++ = ParallelIteration(Math.FusedMultiplyAdd(x, dx, XMin), yClr);
                }
            }
        }
    }

    private unsafe IntPtr RenderTexture(int width, int height)
    {
        if (image?.Image.Width != width || image.Image.Height != height)
        {
            KillRenderingThreads();

            image?.Dispose();
            image = new ContiguousImage<Rgba32>(width, height);

            SpawnRenderingThreads();
        }

        return (IntPtr) image.Pointer;
    }

    private uint ParallelIteration(double cRe, double cIm)
    {
        double zRe = 0, zIm = 0;

        double zReSqr = 0;
        double zImSqr = 0;

        double radius = R * R;

        for (uint i = 0; i < N; i++)
        {
            if (zReSqr + zImSqr > radius)
            {
                return i;
            }

            zIm = Math.FusedMultiplyAdd(zIm, zRe * 2, cIm);

            zRe = zReSqr - zImSqr + cRe;
            zReSqr = zRe * zRe;
            zImSqr = zIm * zIm;
        }

        return 0;
    }

    private void KillRenderingThreads()
    {
        cancellation.Cancel(false);
        Task.WaitAll(tasks.ToArray());
        cancellation = new CancellationTokenSource();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            KillRenderingThreads();
            image?.Dispose();
        }

        base.Dispose(disposing);
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
