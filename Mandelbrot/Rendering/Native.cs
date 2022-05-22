using System;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mandelbrot.Rendering
{
    public class NativeRenderer : Renderer, IDisposable
    {
        private Image<Rgba32> image = new(1, 1);

        protected override Shader? Shader { get; set; }

        public override void Initialize(out int vbo, out int vao)
        {
            Shader = new Shader("Shaders/texture.vert", "Shaders/texture.frag");
            Shader.Use();

            var handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

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

        public override unsafe void Render(int width, int height)
        {
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                (IntPtr) RenderTexture(width, height));
        }

        private unsafe Rgba32* RenderTexture(int width, int height)
        {
            if (image.Width != width || image.Height != height)
            {
                image.Dispose();
                image = Helpers.ContiguousImage(width, height);
            }

            var dx = Width / image.Width;
            var dy = Height / image.Height;

            if (!image.DangerousTryGetSinglePixelMemory(out var memory))
            {
                throw new Exception("This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
            }

            using var pinHandle = Helpers.GetImageMemory(image);
            var ptr = (Rgba32*) pinHandle.Pointer;

            Parallel.For(0,
                image.Height,
                y =>
                {
                    var row = ptr + y * image.Width;
                    var clrY = YMin + y * dy;

                    for (var x = 0; x < image.Width; ++x)
                    {
                        *row++ = DeepColoring(XMin + x * dx, clrY);
                    }
                });

            return ptr;
        }

        private Rgba32 DeepColoring(double cRe, double cIm)
        {
            double zRe = 0, zIm = 0;

            double zReSqr = 0;
            double zImSqr = 0;

            double radius = R * R;

            for (var i = 0; i < N; i++)
            {
                if (zReSqr + zImSqr > radius)
                {
                    return GetColor(i, (float) (zReSqr + zImSqr));
                }

                zIm = Math.FusedMultiplyAdd(zIm, zRe * 2, cIm);

                zRe = zReSqr - zImSqr + cRe;
                zReSqr = zRe * zRe;
                zImSqr = zIm * zIm;
            }

            return Color.Black;
        }

        private Rgba32 GetColor(int i, float zs)
        {
            const float b = 0.23570226f, c = 0.124526508f;
            var x = M * (i + i + MathF.Log2(zs));

            var red = .5f * (1 - MathF.Cos(x));
            var green = .5f * (1 - MathF.Cos(b * x));
            var blue = .5f * (1 - MathF.Cos(c * x));

            return new Rgba32(red, green, blue);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                image.Dispose();
            }

            base.Dispose(disposing);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
