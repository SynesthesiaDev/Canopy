// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Canopy.Rendering.Shaders;
using Serilog;
using Silk.NET.OpenGL;
using Shader = Canopy.Rendering.Shaders.Shader;
using Texture = Canopy.Rendering.Textures.Texture;

namespace Canopy.Rendering;

public class OpenGLRenderer : IDisposable
{
    private const string shader_uniform_texture = "u_texture";

    // Cache so we don't query with string every frame. That's expensive on gc allocations!!
    private int textureShaderLocation;

    private uint vao, vbo, ebo;
    private int projectionUniformLocation;
    private int colorUniformLocation;
    private int alphaUniformLocation;
    private int useTextureUniformLocation;

    private bool openGlInitialized;

    public Shader DefaultShader { get; private set; } = null!;

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

    public int BackBufferWidth { get; private set; }
    public int BackBufferHeight { get; private set; }
    public Texture? CurrentTexture { get; private set; }
    public Shader? CurrentShader { get; private set; } = null!;

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

        initQuadGeometry();
        compileDefaultShaders();
        updateProjection();

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

    private void initQuadGeometry()
    {
        unsafe
        {
            uint[] indices = [0u, 1u, 2u, 0u, 2u, 3u];

            vao = OpenGL.GenVertexArray();
            vbo = OpenGL.GenBuffer();
            ebo = OpenGL.GenBuffer();

            OpenGL.BindVertexArray(vao);

            OpenGL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            OpenGL.BufferData(BufferTargetARB.ArrayBuffer, 64, null, BufferUsageARB.DynamicDraw);

            OpenGL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            OpenGL.BufferData(BufferTargetARB.ElementArrayBuffer, indices, BufferUsageARB.StaticDraw);

            const uint stride = 4 * sizeof(float);

            OpenGL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            OpenGL.EnableVertexAttribArray(0);

            OpenGL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            OpenGL.EnableVertexAttribArray(1);

            OpenGL.BindVertexArray(0);
        }
    }

    // Still need position and size because if you have multiple monitors, the window will span across ALL of them, so
    // you need to draw multiple wallpapers at different positions that match the monitors
    public void DrawQuad(Vector2 position, Vector2 size, uint packedColor, float alpha, float cornerRadius, Texture? texture, RectangleF? textureCoord)
    {
        EnsureInitialized();
        if (texture is { IsUploaded: false }) return;
        if (texture != CurrentTexture) BindTexture(texture);

        var tex = textureCoord ?? new RectangleF(0, 0, 1, 1);
        float x = position.X, y = position.Y, w = size.X, h = size.Y;

        // Interleaved: x, y, u, v per vertex
        ReadOnlySpan<float> vertices =
        [
            x, y, tex.Left, tex.Top,
            x, y + h, tex.Left, tex.Bottom,
            x + w, y + h, tex.Right, tex.Bottom,
            x + w, y, tex.Right, tex.Top,
        ];

        OpenGL.BindVertexArray(vao);
        OpenGL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        OpenGL.BufferSubData(BufferTargetARB.ArrayBuffer, 0, vertices);

        CurrentShader?.SetVector4(colorUniformLocation, new Vector4(
            (packedColor & 0xFF) / 255f,
            (packedColor >> 8 & 0xFF) / 255f,
            (packedColor >> 16 & 0xFF) / 255f,
            (packedColor >> 24 & 0xFF) / 255f));
        CurrentShader?.SetFloat(alphaUniformLocation, alpha);
        CurrentShader?.SetBool(useTextureUniformLocation, texture != null);

        unsafe
        {
            OpenGL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
        }

        OpenGL.BindVertexArray(0);
    }

    public void BindTexture(Texture? texture)
    {
        if (CurrentTexture == texture) return;

        CurrentTexture = texture;
        if (texture != null && texture.Bind(OpenGL))
        {
            CurrentShader?.SetInt(textureShaderLocation, 0);
        }
        else
        {
            OpenGL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    public void Resize(int width, int height)
    {
        BackBufferWidth = width;
        BackBufferHeight = height;

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

#if DEBUG
        OpenGL.ClearColor(1f, 0f, 1f, 1f);
#else
        OpenGL.ClearColor(0f, 0f, 0f, 1f);
#endif

        OpenGL.Clear(ClearBufferMask.ColorBufferBit);

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
    }

    private void pushViewport()
    {
        OpenGL.Viewport(0, 0, (uint)BackBufferWidth, (uint)BackBufferHeight);
        if(CurrentShader != null) updateProjection();
    }

    private void updateProjection()
    {
        var proj = Matrix4x4.CreateOrthographicOffCenter(
            left: 0,
            right: BackBufferWidth,
            bottom: BackBufferHeight,
            top: 0,
            zNearPlane: -1,
            zFarPlane: 1
        );

        CurrentShader.SetMatrix4(projectionUniformLocation, proj);
    }

    private void compileDefaultShaders()
    {
        DefaultShader = new Shader(OpenGL, ShaderSources.DEFAULT_VERTEX, ShaderSources.DEFAULT_FRAGMENT);
        BindShader(DefaultShader);
    }

    private void cacheShaderUniformLocations()
    {
        textureShaderLocation = CurrentShader.GetUniformLocation("u_texture");
        projectionUniformLocation = CurrentShader.GetUniformLocation("u_projection");
        colorUniformLocation = CurrentShader.GetUniformLocation("u_color");
        alphaUniformLocation = CurrentShader.GetUniformLocation("u_alpha");
        useTextureUniformLocation = CurrentShader.GetUniformLocation("u_use_texture");
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

        OpenGL.Dispose();
    }
}
