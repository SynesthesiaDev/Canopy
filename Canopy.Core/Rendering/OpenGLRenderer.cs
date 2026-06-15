// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using Canopy.Extensions;
using Canopy.Rendering.Shaders;
using Serilog;
using Silk.NET.OpenGL;
using Synesthesia.Utils.Extensions;
using Shader = Canopy.Rendering.Shaders.Shader;
using Texture = Canopy.Rendering.Textures.Texture;

namespace Canopy.Rendering;

public class OpenGLRenderer : IDisposable
{
    private const ClearFlags default_clear_flags = ClearFlags.ColorBuffer | ClearFlags.DepthBuffer | ClearFlags.StencilBuffer;

    private readonly float[] vertices =
    [
        0.0f, 0.0f, 0.0f, 0.0f,
        0.0f, 1.0f, 0.0f, 1.0f,
        1.0f, 1.0f, 1.0f, 1.0f,
        1.0f, 0.0f, 1.0f, 0.0f
    ];

    private readonly uint[] indices =
    [
        0, 1, 2,
        0, 2, 3
    ];

    private const string shader_uniform_texture = "u_texture";
    private const string shader_uniform_use_texture = "u_use_texture";
    private const string shader_uniform_transform_matrix = "u_transform";
    private const string shader_uniform_alpha = "u_alpha";

    // Cache so we don't query with string every frame. That's expensive on gc allocations!!
    private int textureShaderLocation, useTextureShaderLocation, transformShaderLocation, alphaShaderLocation;
    private uint vao, vbo, ebo;

    private bool openGlInitialized;
    private Matrix4x4 projectionMatrix;

    public Shader DefaultShader = null!;

    public required IWindowSurface Surface { get; init; }

    public GL OpenGL
    {
        get
        {
            EnsureInitialized();
            return field;
        }

        private set;
    } = null!;

    public bool CanDraw => BackBufferWidth > 0 && BackBufferHeight > 0;

    public int BackBufferWidth { get; private set; }

    public int BackBufferHeight { get; private set; }

    public ClearFlags ClearFlags = default_clear_flags;

    public Matrix4x4 Matrix { get; private set; } = Matrix4x4.Identity;

    public Matrix4x4 InverseMatrix { get; private set; } = Matrix4x4.Identity;

    public Texture? CurrentTexture { get; private set; }
    public Shader CurrentShader { get; private set; } = null!;

    public static readonly ConcurrentQueue<Texture> TEXTURE_UPLOAD_QUEUE = new ConcurrentQueue<Texture>();

    public void Initialize()
    {
        if (openGlInitialized) throw new InvalidOperationException("OpenGL is already initialized");
        Log.Verbose("Initializing OpenGL Renderer");

        var gl = GL.GetApi(name =>
        {
            var ptr = Surface.NativeContext.GetProcAddress(name);
            return ptr;
        });

        OpenGL = gl ?? throw new InvalidOperationException("Silk.NET could not bind to OpenGL");

        BackBufferHeight = (int)Surface.GetScreenSize().X;
        BackBufferWidth = (int)Surface.GetScreenSize().Y;

        openGlInitialized = true;
        Resize(BackBufferWidth, BackBufferHeight);

        gl.GenVertexArrays(1, out vao);
        gl.BindVertexArray(vao);
        gl.CheckError("Generate and bind vao array");

        gl.GenBuffers(1, out vbo);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        gl.CheckError("Generate and bind vbo buffer");

        unsafe
        {
            fixed (float* pointer = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), pointer, BufferUsageARB.StaticDraw);
            }
        }

        gl.GenBuffers(1, out ebo);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        gl.CheckError("Generate and bind ebo buffer");

        unsafe
        {
            fixed (uint* pointer = indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), pointer, BufferUsageARB.StaticDraw);
            }
        }

        unsafe
        {
            //TODO move to Vertex2d class with proper attributes like
            // [VertexInfo(4, 1, VertexAttribPointerType.Float)]
            // public readonly float Alpha = alpha;

            // 0 - Position (Vector2)
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)0);
            // 1 - TexCoord (Vector2)
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float)));
            gl.CheckError("Setup vertex attributes");

            gl.BindVertexArray(0);
            gl.CheckError("unbind");
        }

        var version = OpenGL.GetStringS(GLEnum.Version);
        var shadingLanguageVersion = OpenGL.GetStringS(GLEnum.ShadingLanguageVersion);
        var vendor = OpenGL.GetStringS(GLEnum.Vendor);
        var renderer = OpenGL.GetStringS(GLEnum.Renderer);

        Console.WriteLine(" ");
        Log.Debug("OpenGL Initialized");
        Log.Debug($"- Version:   {version}");
        Log.Debug($"- Vendor:    {vendor}");
        Log.Debug($"- Renderer   {renderer}");
        Log.Debug($"- GLSL:      {shadingLanguageVersion}");
    }

    public void DrawQuad(Vector2 position, Vector2 size, float alpha, Texture? texture = null)
    {
        unsafe
        {
            if (texture is { IsUploaded: false }) return;

            EnsureInitialized();
            if (texture != CurrentTexture)
            {
                BindTexture(texture);
            }

            Matrix4x4 modelMatrix = Matrix4x4.CreateScale(size.X, size.Y, 1.0f) * Matrix4x4.CreateTranslation(position.X, position.Y, 0.0f);
            Matrix4x4 finalTransform = modelMatrix * Matrix * projectionMatrix;

            CurrentShader.SetMatrix4(transformShaderLocation, finalTransform);
            CurrentShader.SetFloat(alphaShaderLocation, alpha);
            CurrentShader.SetBool(useTextureShaderLocation, texture != null);

            OpenGL.BindVertexArray(vao);
            OpenGL.CheckError("bind vao");
            OpenGL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
            OpenGL.CheckError("draw to vao");
            OpenGL.BindVertexArray(0);
            OpenGL.CheckError("unbind vao");
        }
    }

    public void BindTexture(Texture? texture)
    {
        if (CurrentTexture == texture) return;
        CurrentTexture = texture;
        if (texture != null && texture.Bind(OpenGL))
        {
            CurrentShader.SetInt(textureShaderLocation, 0);
        }
        else
        {
            OpenGL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    public void Resize(int width, int height)
    {
        Log.Verbose("(OpenGL Renderer) Viewport resize to {width}x{height}", width, height);
        EnsureInitialized();
        pushViewport();
    }

    public void BindShader(Shader shader)
    {
        // ThreadSafety.AssertRunningOnRenderThread();

        if (CurrentShader == shader) return;

        CurrentShader = shader;
        shader.Use();
        cacheShaderUniformLocations();
    }

    public void UnbindShader()
    {
        BindShader(DefaultShader);
    }

    public void BeginDrawing()
    {
        EnsureInitialized();
        ClearBufferMask mask = ClearBufferMask.None;

        if (ClearFlags.HasFlagFast(ClearFlags.ColorBuffer))
            mask |= ClearBufferMask.ColorBufferBit;
        if (ClearFlags.HasFlagFast(ClearFlags.DepthBuffer))
            mask |= ClearBufferMask.DepthBufferBit;
        if (ClearFlags.HasFlagFast(ClearFlags.StencilBuffer))
            mask |= ClearBufferMask.StencilBufferBit;

        if (mask != ClearBufferMask.None) OpenGL.Clear(mask);

        while (!TEXTURE_UPLOAD_QUEUE.IsEmpty)
        {
            TEXTURE_UPLOAD_QUEUE.TryDequeue(out var texture);
            texture?.Upload(OpenGL);
        }

        pushViewport();
    }

    public void EndDrawing()
    {
        EnsureInitialized();

        Surface.SwapBuffers();
        BindTexture(null);

        ClearFlags = default_clear_flags;
    }

    private void pushViewport()
    {
        var size = Surface.GetScreenSize();
        BackBufferWidth = (int)size.X;
        BackBufferHeight = (int)size.Y;

        projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, BackBufferWidth, BackBufferHeight, 0, -1, 1);

        OpenGL.Viewport(0, 0, (uint)BackBufferWidth, (uint)BackBufferHeight);
    }

    public void CompileDefaultShaders()
    {
        DefaultShader = new Shader(OpenGL, ShaderSources.DEFAULT_VERTEX, ShaderSources.DEFAULT_FRAGMENT);
        BindShader(DefaultShader);
    }

    private void cacheShaderUniformLocations()
    {
        textureShaderLocation = CurrentShader.GetUniformLocation(shader_uniform_texture);
        useTextureShaderLocation = CurrentShader.GetUniformLocation(shader_uniform_use_texture);
        transformShaderLocation = CurrentShader.GetUniformLocation(shader_uniform_transform_matrix);
        alphaShaderLocation = CurrentShader.GetUniformLocation(shader_uniform_alpha);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureInitialized()
    {
        if (!openGlInitialized) throw new InvalidOperationException("OpenGL is not initialized yet");
    }

    public void Dispose()
    {
        Log.Verbose("Disposing OpenGLRenderer");
        openGlInitialized = false;

        OpenGL.DeleteBuffer(vbo);
        OpenGL.DeleteBuffer(ebo);
        OpenGL.DeleteVertexArray(vao);

        OpenGL.Dispose();
    }
}
