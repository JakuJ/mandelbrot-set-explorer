using System;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;
using static System.GC;

namespace Mandelbrot;

public sealed class Shader
{
    public readonly int Handle;

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

        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);

        GL.LinkProgram(Handle);

        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }

    private bool disposedValue;

    private void Dispose(bool disposed)
    {
        if (!disposedValue)
        {
            GL.DeleteProgram(Handle);

            disposedValue = true;
        }
    }

    ~Shader()
    {
        GL.DeleteProgram(Handle);
    }

    public void Dispose()
    {
        Dispose(true);
        SuppressFinalize(this);
    }
}
