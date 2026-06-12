// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Serilog;
using Silk.NET.OpenGL;

namespace Canopy.Rendering.Textures;

public class Texture : IDisposable
{
    public uint Handle { get; private set; }

    public TextureData TextureData { get; private set; }

    public int Width => TextureData.Width;
    public int Height => TextureData.Height;
    public PixelFormat PixelFormat => TextureData.PixelFormat;

    public bool IsUploaded { get; private set; }

    public bool UploadQueued { get; private set; }

    public bool UploadImmediately { get; }

    private GL? gl;

    public Texture(TextureData textureData, bool uploadImmediately)
    {
        UploadImmediately = uploadImmediately;
        TextureData = textureData;
        IsUploaded = false;

        if (UploadImmediately) EnqueueUpload();
    }

    public void EnqueueUpload()
    {
        Log.Verbose("Texture upload enqueued");
        OpenGLRenderer.TEXTURE_UPLOAD_QUEUE.Enqueue(this);
        UploadQueued = true;
    }

    public void Upload(GL opengl)
    {
        // ThreadSafety.AssertRunningOnRenderThread();

        if (IsUploaded) return;
        if (TextureData.Data.Length == 0) throw new OpenGLException("No pixel data");

        gl = opengl;
        Handle = gl.GenTexture();
        opengl.BindTexture(TextureTarget.Texture2D, Handle);

        gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        unsafe
        {
            fixed (void* ptr = TextureData.Data)
            {
                gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)Width, (uint)Height, 0, TextureData.PixelFormat, PixelType.UnsignedByte, ptr);
            }
        }

        IsUploaded = true;
        UploadQueued = false;
        Log.Verbose("Uploaded texture {this}", ToString());
    }

    public bool Bind(GL opengl, TextureUnit unit = TextureUnit.Texture0)
    {
        // ThreadSafety.AssertRunningOnRenderThread();

        switch (IsUploaded)
        {
            case false when !UploadQueued:
                EnqueueUpload();
                return false;
            case false:
                return false;
        }

        gl!.ActiveTexture(unit);
        gl.BindTexture(TextureTarget.Texture2D, Handle);
        return true;
    }

    public override string ToString() => $"Texture(Handle={Handle}, TextureData={TextureData}, IsUploaded={IsUploaded}, UploadQueued={UploadQueued})";

    public void Dispose()
    {
    }
}
