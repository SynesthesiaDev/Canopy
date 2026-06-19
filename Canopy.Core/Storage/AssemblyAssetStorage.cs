// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Reflection;

namespace Canopy.Storage;

public class AssemblyAssetStorage(Assembly assembly) : AssetStorage
{
    public override string[] GetAllFiles() => assembly.GetManifestResourceNames();

    public override bool FileExists(string path) => assembly.GetManifestResourceNames().Contains(path);

    public override byte[]? GetOrNull(string path)
    {
        using var stream = assembly.GetManifestResourceStream(path);
        return stream == null ? null : ReadFully(stream);
    }

    public static byte[] ReadFully(Stream input)
    {
        byte[] buffer = new byte[16*1024];
        using MemoryStream ms = new MemoryStream();

        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
    }
}
