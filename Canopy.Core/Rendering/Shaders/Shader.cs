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
    private readonly GL gl;
    public readonly uint Program;

    private readonly DenseMap<string, int> uniformCache = new();

    public Shader(GL openGl, string? vertexCode, string? fragmentCode)
    {
        // ThreadSafety.AssertRunningOnRenderThread();

        gl = openGl;

        if (vertexCode == null && fragmentCode == null) throw new OpenGLException("Cannot have both Vertex and Fragment shaders empty");

        uint? vertexShader = null;
        uint? fragmentShader = null;

        if (vertexCode != null) vertexShader = compileShader(ShaderType.VertexShader, vertexCode);
        if (fragmentCode != null) fragmentShader = compileShader(ShaderType.FragmentShader, fragmentCode);

        Program = gl.CreateProgram();

        if (vertexShader != null) gl.AttachShader(Program, vertexShader.Value);
        if (fragmentShader != null) gl.AttachShader(Program, fragmentShader.Value);
        gl.LinkProgram(Program);

        gl.GetProgram(Program, ProgramPropertyARB.LinkStatus, out int success);

        if (success == 0) throw new OpenGLException($"Shader linking failed: {gl.GetProgramInfoLog(Program)}");

        if (vertexShader != null) gl.DeleteShader(vertexShader.Value);
        if (fragmentShader != null) gl.DeleteShader(fragmentShader.Value);
    }

    public int GetUniformLocation(string uniform)
    {
        // ThreadSafety.AssertRunningOnRenderThread();

        if (uniformCache.Get(uniform, out int cached))
            return cached;

        var location = gl.GetUniformLocation(Program, uniform);
        return location != -1 ? location : throw new OpenGLException($"Failed to get shader uniform '{uniform}'");
    }

    private uint compileShader(ShaderType shaderType, string code)
    {
        uint shader = gl.CreateShader(shaderType);
        gl.ShaderSource(shader, code);
        gl.CompileShader(shader);

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);

        var shaderId = success == 0 ? throw new OpenGLException($"Shader compilation failed: ({shaderType}): {gl.GetShaderInfoLog(shader)}") : shader;
        Log.Verbose("Compiled {Replace} shader with id {ShaderId}", shaderType.ToString().Replace("Shader", string.Empty), shaderId);
        return shaderId;
    }

    public void SetMatrix4(int location, Matrix4x4 matrix)
    {
        unsafe
        {
            gl.UniformMatrix4(location, 1, false, (float*)&matrix);
        }
    }

    public void SetMatrix4(string name, Matrix4x4 matrix)
    {
        int location = GetUniformLocation(name);
        SetMatrix4(location, matrix);
    }

    public void SetFloat(int location, float value) => gl.Uniform1(location, value);

    public void SetFloat(string name, float value)
    {
        var location = GetUniformLocation(name);
        SetFloat(location, value);
    }

    public void SetDouble(int location, double value) => gl.Uniform1(location, value);

    public void SetDouble(string name, double value)
    {
        var location = GetUniformLocation(name);
        SetDouble(location, value);
    }

    public void SetInt(int location, int value) => gl.Uniform1(location, value);

    public void SetInt(string name, int value)
    {
        var location = GetUniformLocation(name);
        SetInt(location, value);
    }

    public void SetBool(int location, bool value) => SetInt(location, value.ToInt());

    public void SetBool(string name, bool value) => SetInt(name, value.ToInt());

    public void SetVector2(int location, Vector2 value) => gl.Uniform2(location, value);

    public void SetVector2(string name, Vector2 value)
    {
        var location = GetUniformLocation(name);
        SetVector2(location, value);
    }

    public void SetVector3(int location, Vector3 value) => gl.Uniform3(location, value);

    public void SetVector3(string name, Vector3 value)
    {
        var location = GetUniformLocation(name);
        SetVector3(location, value);
    }

    public void SetVector4(int location, Vector4 value) => gl.Uniform4(location, value);

    public void SetVector4(string name, Vector4 value)
    {
        var location = GetUniformLocation(name);
        SetVector4(location, value);
    }

    public void Use()
    {
        gl.UseProgram(Program);
        Log.Verbose("Shader {id} bind", Program);
    }

    public void Dispose()
    {
        gl.DeleteProgram(Program);
    }
}
