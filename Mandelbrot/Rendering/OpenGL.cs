using OpenTK.Graphics.OpenGL4;

namespace Mandelbrot.Rendering;

public class OpenGl : Renderer
{
    public override Shader Shader { get; protected set; }

    public override void Initialize()
    {
        Shader = new Shader("Shaders/mandelbrot.vert", "Shaders/mandelbrot.frag");
        Shader.Use();
    }

    public override void Render(int width, int height)
    {
        GL.Uniform1(Shader.GetUniformLocation("R"), R);
        GL.Uniform1(Shader.GetUniformLocation("N"), N);
        GL.Uniform1(Shader.GetUniformLocation("xMin"), (float) XMin);
        GL.Uniform1(Shader.GetUniformLocation("xMax"), (float) XMax);
        GL.Uniform1(Shader.GetUniformLocation("yMin"), (float) YMin);
        GL.Uniform1(Shader.GetUniformLocation("yMax"), (float) YMax);
    }
}
