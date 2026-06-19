// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;
using Canopy.Rendering;
using Canopy.Rendering.Shaders;
using Canopy.Rendering.Textures;
using Canopy.Storage;
using Canopy.Utils.Future;
using Synesthesia.Utils.Extensions;

namespace Canopy.Graphics;

public class Wallpaper : IDrawable
{
    // null means it uses default texture shader
    public Shader? Shader { get; private set; }

    // null means not ready for rendering yet, probably uploading.
    // transitions should wait until loaded and swapped
    public Texture? Texture { get; private set; }

    public TextureFillMode TextureFillMode { get; set; } = TextureFillMode.Fill;

    public string TextureResourceName { get; }

    public float Alpha { get; set; } = 1f;

    public float CornerRadius { get; set; } = 1f;

    private RectangleF uvCoords = new(0, 0, 1, 1);
    private Vector2 drawSize = Vector2.One;
    private Vector2 drawOffset = Vector2.Zero;

    public Vector2 Size { get; private set; }

    public AssetStorage AssetStorage { get; }

    public Wallpaper(string textureResourceName, AssetStorage assetStorage, string? sharedResourceName = null)
    {
        TextureResourceName = textureResourceName;
        AssetStorage = assetStorage;

        Tasks.RunAsync(() => assetStorage.GetResolved(textureResourceName, DataParsers.LoadTexture)).Then(tex =>
        {
            Texture = tex;
            if (Size != Vector2.Zero) updateAxis(Size);
        });

        if (sharedResourceName != null)
        {
            Tasks.RunAsync(() =>
            {
                var normalizedShaderName = sharedResourceName.RemoveSuffix(".vert").RemoveSuffix(".frag");
                var vert = assetStorage.GetResolved($"{normalizedShaderName}.vert", DataParsers.LoadString);
                var frag = assetStorage.GetResolved($"{normalizedShaderName}.frag", DataParsers.LoadString);

                return new Shader(vert, frag, true);
            }).Then(shader =>
            {
                Shader = shader;
            });
        }
    }

    public void Draw(OpenGLRenderer gl)
    {
        var currentSize = new Vector2(gl.BackBufferWidth, gl.BackBufferHeight);

        // Dont recalc every frame
        if (Size != currentSize)
        {
            Size = currentSize;
            updateAxis(currentSize);
        }

        var shaderReady = Shader is { IsCompiled: true };
        if (shaderReady) gl.BindShader(Shader!);

        gl.DrawQuad(drawOffset, drawSize, 0xFFFFFFFF, Alpha, CornerRadius, Texture, uvCoords);

        if (shaderReady) gl.UnbindShader();
    }

    public void Dispose()
    {
        Shader?.Dispose();
        Texture?.Dispose();
    }

    private void updateAxis(Vector2 size)
    {
        if (Texture == null || TextureFillMode == TextureFillMode.Stretch) return;

        var textureRatio = (float)Texture.Width / Texture.Height;
        var boxRatio = size.X / size.Y;

        drawSize = size;
        drawOffset = Vector2.Zero;
        uvCoords = new RectangleF(0, 0, 1, 1);

        switch (TextureFillMode)
        {
            case TextureFillMode.Fit:
                if (textureRatio > boxRatio)
                {
                    drawSize = new Vector2(size.X, size.X / textureRatio);
                    drawOffset = new Vector2(0, (size.Y - drawSize.Y) / 2f);
                }
                else
                {
                    drawSize = new Vector2(size.Y * textureRatio, size.Y);
                    drawOffset = new Vector2((size.X - drawSize.X) / 2f, 0);
                }

                break;
            case TextureFillMode.Fill:
                var scaleX = 1f;
                var scaleY = 1f;

                if (textureRatio > boxRatio)
                {
                    scaleX = boxRatio / textureRatio;
                }
                else
                {
                    scaleY = textureRatio / boxRatio;
                }

                uvCoords = new RectangleF((1f - scaleX) / 2f, (1f - scaleY) / 2f, scaleX, scaleY);
                break;
        }
    }
}
