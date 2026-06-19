// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Faster.Map.Core;
using Serilog;
using Silk.NET.OpenGL;
using Synesthesia.Utils.Extensions;

namespace Canopy.Rendering.Shaders;

public class Shader : IDisposable
{
    public uint Program { get; private set; }

    private readonly DenseMap<string, int> uniformCache = new();

    public bool IsCompiled { get; private set; }

    public bool CompileQueued { get; private set; }

    public bool UploadImmediately { get; }

    public readonly string? Vertex;
    public readonly string? Fragment;

    private GL? gl;

    public Shader(string? vertexCode, string? fragmentCode, bool uploadImmediately)
    {
        if (vertexCode == null && fragmentCode == null) throw new OpenGLException("Cannot have both Vertex and Fragment shaders empty");

        Vertex = vertexCode;
        Fragment = fragmentCode;
        UploadImmediately = uploadImmediately;

        if(UploadImmediately) EnqueueCompile();
    }


    public void EnqueueCompile()
    {
        Log.Verbose("Shader upload enqueued");
        OpenGLRenderer.SHADER_COMPILE_QUEUE.Enqueue(this);
        CompileQueued = true;
    }

    public void Compile(GL opengl)
    {
        gl = opengl;
        CompileQueued = false;

        uint? vertexShader = null;
        uint? fragmentShader = null;

        if (Vertex != null) vertexShader = compileShader(ShaderType.VertexShader, Vertex);
        if (Fragment != null) fragmentShader = compileShader(ShaderType.FragmentShader, Fragment);

        Program = gl.CreateProgram();

        if (vertexShader != null) gl.AttachShader(Program, vertexShader.Value);
        if (fragmentShader != null) gl.AttachShader(Program, fragmentShader.Value);
        gl.LinkProgram(Program);

        gl.GetProgram(Program, ProgramPropertyARB.LinkStatus, out int success);

        if (success == 0) throw new OpenGLException($"Shader linking failed: {gl.GetProgramInfoLog(Program)}");

        if (vertexShader != null) gl.DeleteShader(vertexShader.Value);
        if (fragmentShader != null) gl.DeleteShader(fragmentShader.Value);

        IsCompiled = true;

        Log.Verbose("Compiled shader {this}", this);
    }

    private void assertGlInitialized()
    {
        if (gl == null) throw new InvalidOperationException("Shader is not compiled/being compiled");
    }

    public int GetUniformLocation(string uniform)
    {
        assertGlInitialized();

        if (uniformCache.Get(uniform, out int cached))
            return cached;

        var location = gl!.GetUniformLocation(Program, uniform);
        return location != -1 ? location : throw new OpenGLException($"Failed to get shader uniform '{uniform}'");
    }

    private uint compileShader(ShaderType shaderType, string code)
    {
        assertGlInitialized();

        uint shader = gl!.CreateShader(shaderType);
        gl.ShaderSource(shader, code);
        gl.CompileShader(shader);

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);

        var shaderId = success == 0 ? throw new OpenGLException($"Shader compilation failed: ({shaderType}): {gl.GetShaderInfoLog(shader)}") : shader;
        Log.Verbose("Compiled {Replace} shader with id {ShaderId}", shaderType.ToString().Replace("Shader", string.Empty), shaderId);
        return shaderId;
    }

    public void SetMatrix4(int location, Matrix4x4 matrix)
    {
        assertGlInitialized();

        unsafe
        {
            gl!.UniformMatrix4(location, 1, false, (float*)&matrix);
        }
    }

    public void SetMatrix4(string name, Matrix4x4 matrix)
    {
        int location = GetUniformLocation(name);
        SetMatrix4(location, matrix);
    }

    public void SetFloat(int location, float value)
    {
        assertGlInitialized();
        gl!.Uniform1(location, value);
    }

    public void SetFloat(string name, float value)
    {
        var location = GetUniformLocation(name);
        SetFloat(location, value);
    }

    public void SetDouble(int location, double value)
    {
        assertGlInitialized();
        gl!.Uniform1(location, value);
    }

    public void SetDouble(string name, double value)
    {
        var location = GetUniformLocation(name);
        SetDouble(location, value);
    }

    public void SetInt(int location, int value)
    {
        assertGlInitialized();
        gl!.Uniform1(location, value);
    }

    public void SetInt(string name, int value)
    {
        var location = GetUniformLocation(name);
        SetInt(location, value);
    }

    public void SetBool(int location, bool value) => SetInt(location, value.ToInt());

    public void SetBool(string name, bool value) => SetInt(name, value.ToInt());

    public void SetVector2(int location, Vector2 value)
    {
        assertGlInitialized();
        gl!.Uniform2(location, value);
    }

    public void SetVector2(string name, Vector2 value)
    {
        var location = GetUniformLocation(name);
        SetVector2(location, value);
    }

    public void SetVector3(int location, Vector3 value)
    {
        assertGlInitialized();
        gl!.Uniform3(location, value);
    }

    public void SetVector3(string name, Vector3 value)
    {
        var location = GetUniformLocation(name);
        SetVector3(location, value);
    }

    public void SetVector4(int location, Vector4 value)
    {
        assertGlInitialized();
        gl!.Uniform4(location, value);
    }

    public void SetVector4(string name, Vector4 value)
    {
        var location = GetUniformLocation(name);
        SetVector4(location, value);
    }

    public void Use()
    {
        assertGlInitialized();

        gl!.UseProgram(Program);
        Log.Verbose("Bound shader program {id}", Program);
    }

    public override string ToString() => $"Shader(Handle={Program}, Vertex={Vertex != null}, Fragment={Fragment != null}, IsCompiled={IsCompiled}, CompileQueued={CompileQueued})";

    public void Dispose()
    {
        Log.Verbose("Disposed shader program {h}", Program);
        gl?.DeleteProgram(Program);
        uniformCache.Clear();
    }
}
