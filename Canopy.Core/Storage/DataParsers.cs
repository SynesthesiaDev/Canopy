// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Text;
using Canopy.Rendering.Textures;
using Silk.NET.OpenGL;
using StbImageSharp;
using Texture = Canopy.Rendering.Textures.Texture;

namespace Canopy.Storage;

public static class DataParsers
{

    public static Texture LoadTexture(byte[] data, bool uploadImmediately = false)
    {
        var image = ImageResult.FromMemory(data, ColorComponents.RedGreenBlueAlpha);
        var textureData = new TextureData(image.Width, image.Height, image.Data, PixelFormat.Rgba);
        return new Texture(textureData, uploadImmediately);
    }

    public static Texture LoadTexture(byte[] data, string _)
    {
        return LoadTexture(data, true);
    }

    public static string LoadString(byte[] data, string _)
    {
        return Encoding.UTF8.GetString(data);
    }
}
