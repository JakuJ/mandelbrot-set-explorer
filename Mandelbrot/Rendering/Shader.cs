using System;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;
using static System.GC;

namespace Mandelbrot;

public sealed class Shader : IDisposable
{
    private readonly int handle;

    public Shader(string vertexPath, string fragmentPath)
    {
        string vertexShaderSource;

        using (var reader = new StreamReader(vertexPath, Encoding.UTF8))
        {
            vertexShaderSource = reader.ReadToEnd();
        }

        string fragmentShaderSource;

        using (var reader = new StreamReader(fragmentPath, Encoding.UTF8))
        {
            fragmentShaderSource = reader.ReadToEnd();
        }

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);

        GL.CompileShader(vertexShader);

        string infoLogVert = GL.GetShaderInfoLog(vertexShader);
        if (infoLogVert != string.Empty)
            Console.WriteLine(infoLogVert);

        GL.CompileShader(fragmentShader);

        string infoLogFrag = GL.GetShaderInfoLog(fragmentShader);

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

    public void Use()
    {
        GL.UseProgram(handle);
    }

    public int GetAttribLocation(string name) => GL.GetAttribLocation(handle, name);
    public int GetUniformLocation(string name) => GL.GetUniformLocation(handle, name);

    private bool disposedValue;

    private void Dispose(bool disposed)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(handle);
            disposedValue = true;
        }
    }

    ~Shader()
    {
        GL.DeleteProgram(handle);
    }

    public void Dispose()
    {
        Dispose(true);
        SuppressFinalize(this);
    }
}
