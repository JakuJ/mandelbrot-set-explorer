using OpenTK.Graphics.OpenGL4;

namespace Mandelbrot.Rendering;

public class OpenGl : Renderer
{
    protected override Shader? Shader { get; set; }

    public override void Initialize(out int vbo, out int vao)
    {
        Shader = new Shader("Shaders/mandelbrot.vert", "Shaders/mandelbrot.frag");
        Shader.Use();

        float[] vertices =
        {
            -1f, -1f, 0f,
            1f, -1f, 0f,
            1f, 1f, 0f,
            -1f, 1f, 0f,
        };

        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        var positionLocation = Shader.GetAttribLocation("aPosition");
        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);
        GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(positionLocation);
    }

    public override void Render(int width, int height)
    {
        if (Shader is null) return;

        GL.Uniform1(Shader.GetUniformLocation("R"), R);
        GL.Uniform1(Shader.GetUniformLocation("N"), N);
        GL.Uniform1(Shader.GetUniformLocation("M"), M);
        GL.Uniform1(Shader.GetUniformLocation("xMin"), (float) XMin);
        GL.Uniform1(Shader.GetUniformLocation("xMax"), (float) XMax);
        GL.Uniform1(Shader.GetUniformLocation("yMin"), (float) YMin);
        GL.Uniform1(Shader.GetUniformLocation("yMax"), (float) YMax);
    }
}
