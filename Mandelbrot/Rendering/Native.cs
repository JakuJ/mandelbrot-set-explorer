using System;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering;

public class NativeRenderer : Renderer, IDisposable
{
    private Image<Rgba32> image = new(1, 1);

    protected override Shader? Shader { get; set; }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

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
            -1f, 1f, 0f, 0f, 1f
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

    public override void Render(int width, int height)
    {
        if (Shader is null) return;

        GL.Uniform1(Shader.GetUniformLocation("N"), N);
        GL.Uniform1(Shader.GetUniformLocation("M"), M);

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

    private unsafe IntPtr RenderTexture(int width, int height)
    {
        if (image.Width != width || image.Height != height)
        {
            image.Dispose();
            image = Helpers.ContiguousImage<Rgba32>(width, height);
        }

        var dx = Width / image.Width;
        var dy = Height / image.Height;

        using var pinHandle = Helpers.GetImageMemory(image);
        var ptr = (uint*) pinHandle.Pointer;

        Parallel.For(0,
            image.Height,
            y =>
            {
                var row = ptr + y * image.Width;
                var clrY = YMin + y * dy;

                for (var x = 0; x < image.Width; ++x)
                {
                    *row++ = ParallelColoring(XMin + x * dx, clrY);
                }
            });

        return (IntPtr) ptr;
    }

    private uint ParallelColoring(double cRe, double cIm)
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

    protected override void Dispose(bool disposing)
    {
        if (disposing) image.Dispose();
        base.Dispose(disposing);
    }
}
