using System;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Mandelbrot.Rendering;

public sealed class Shader : IDisposable
{
    private readonly int handle;

    public Shader(string vertexPath, string fragmentPath)
    {
        using var vertexReader = new StreamReader(vertexPath, Encoding.UTF8);
        var vertexShaderSource = vertexReader.ReadToEnd();

        using var fragmentReader = new StreamReader(fragmentPath, Encoding.UTF8);
        var fragmentShaderSource = fragmentReader.ReadToEnd();

        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);

        GL.CompileShader(vertexShader);

        var infoLogVert = GL.GetShaderInfoLog(vertexShader);
        if (infoLogVert != string.Empty)
            Console.WriteLine(infoLogVert);

        GL.CompileShader(fragmentShader);

        var infoLogFrag = GL.GetShaderInfoLog(fragmentShader);
        if (infoLogFrag != string.Empty)
            Console.WriteLine(infoLogFrag);

        handle = GL.CreateProgram();

        GL.AttachShader(handle, vertexShader);
        GL.AttachShader(handle, fragmentShader);

        GL.LinkProgram(handle);

        GL.DetachShader(handle, vertexShader);
        GL.DetachShader(handle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);
    }

    public void Use() => GL.UseProgram(handle);

    public int GetAttribLocation(string name) => GL.GetAttribLocation(handle, name);

    public int GetUniformLocation(string name) => GL.GetUniformLocation(handle, name);

    public void Dispose()
    {
        GL.DeleteProgram(handle);
        GC.SuppressFinalize(this);
    }

    ~Shader()
    {
        GL.DeleteProgram(handle);
    }
}
