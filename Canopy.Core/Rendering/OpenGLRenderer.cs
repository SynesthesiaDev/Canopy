// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Canopy.Graphics;
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

    private const string shader_uniform_texture = "u_texture";

    // Cache so we don't query with string every frame. That's expensive on gc allocations!!
    private int textureShaderLocation;

    private bool openGlInitialized;

    public Shader DefaultShader { get; private set; } = null!;

    public VertexBatch<Vertex2D> VertexBatch2D { get; private set; } = null!;

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

    public ClearFlags ClearFlags = default_clear_flags;

    public bool CanDraw => BackBufferWidth > 0 && BackBufferHeight > 0;
    public int BackBufferWidth { get; private set; }
    public int BackBufferHeight { get; private set; }
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

        BackBufferWidth = (int)Surface.GetScreenSize().X;
        BackBufferHeight = (int)Surface.GetScreenSize().Y;


        openGlInitialized = true;
        Resize(BackBufferWidth, BackBufferHeight);

        VertexBatch2D = new VertexBatch<Vertex2D>(OpenGL);

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

    public void DrawQuad(DrawMatrix drawMatrix, Vector2 position, Vector2 size, uint packedColor, float alpha, float cornerRadius, Texture? texture, RectangleF? textureCoord)
    {
        EnsureInitialized();
        if (texture is { IsUploaded: false }) return;

        if (texture != CurrentTexture)
        {
            BindTexture(texture);
        }

        var v0 = position;
        var v1 = position with { Y = position.Y + size.Y };
        var v2 = position + size;
        var v3 = position with { X = position.X + size.X };

        v0 = Vector2.Transform(v0, drawMatrix.Matrix);
        v1 = Vector2.Transform(v1, drawMatrix.Matrix);
        v2 = Vector2.Transform(v2, drawMatrix.Matrix);
        v3 = Vector2.Transform(v3, drawMatrix.Matrix);

        var tex = textureCoord ?? new Rectangle(0, 0, 1, 1);

        VertexBatch2D.PushVertex(new Vertex2D(
            position: v0,
            size: size,
            texCoord: new Vector2(tex.Left, tex.Top),
            color: packedColor,
            alpha: alpha,
            radius: cornerRadius,
            localUv: new Vector2(0, 0)
        ));

        VertexBatch2D.PushVertex(new Vertex2D(
            position: v1,
            size: size,
            texCoord: new Vector2(tex.Left, tex.Bottom),
            color: packedColor,
            alpha: alpha,
            radius: cornerRadius,
            localUv: new Vector2(0, 1)
        ));

        VertexBatch2D.PushVertex(new Vertex2D(
            position: v2,
            size: size,
            texCoord: new Vector2(tex.Right, tex.Bottom),
            color: packedColor,
            alpha: alpha,
            radius: cornerRadius,
            localUv: new Vector2(1, 1)
        ));

        VertexBatch2D.PushVertex(new Vertex2D(
            position: v3,
            size: size,
            texCoord: new Vector2(tex.Right, tex.Top),
            color: packedColor,
            alpha: alpha,
            radius: cornerRadius,
            localUv: new Vector2(1, 0)
        ));

        // old
        // Matrix4x4 modelMatrix = Matrix4x4.CreateScale(size.X, size.Y, 1.0f) * Matrix4x4.CreateTranslation(position.X, position.Y, 0.0f);
        // Matrix4x4 finalTransform = modelMatrix * Matrix * projectionMatrix;
        //
        // CurrentShader.SetMatrix4(transformShaderLocation, finalTransform);
        // CurrentShader.SetFloat(alphaShaderLocation, alpha);
        // CurrentShader.SetBool(useTextureShaderLocation, texture != null);
    }

    public void BindTexture(Texture? texture)
    {
        if (CurrentTexture == texture) return;
        VertexBatch2D.Flush();

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

        VertexBatch2D.Flush();

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
        VertexBatch2D.Flush();

        Surface.SwapBuffers();
        BindTexture(null);

        ClearFlags = default_clear_flags;
    }

    private void pushViewport()
    {
        var size = Surface.GetScreenSize();
        BackBufferWidth = (int)size.X;
        BackBufferHeight = (int)size.Y;

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

        VertexBatch2D.Dispose();
        OpenGL.Dispose();
    }
}
