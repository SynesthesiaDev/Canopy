// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Serilog;

namespace Canopy.Storage;

public class DirectoryAssetStorage(string dirPath) : AssetStorage
{
    private readonly string rootFullPath = Path.GetFullPath(dirPath);

    public override string[] GetAllFiles()
    {
        return Directory.EnumerateFiles(rootFullPath, "*", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(rootFullPath, p))
            .ToArray();
    }

    public override bool FileExists(string path)
    {
        var fullPath = getSafeFullPath(path);
        return fullPath != null && File.Exists(fullPath);
    }

    public override byte[]? GetOrNull(string path)
    {
        var fullPath = getSafeFullPath(path);
        if (fullPath == null || !File.Exists(fullPath)) return null;

        try
        {
            return File.ReadAllBytes(fullPath);
        }
        catch (Exception)
        {
            Log.Warning("file {path} exists but couldn't be read", path);
            return null;
        }
    }

    /// combines the root path with the relative path and normalizes separators
    /// and ensures the  file doesn't escape the root directory (../../../).
    private string? getSafeFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return null;

        string combinedPath = Path.Combine(rootFullPath, relativePath);
        string fullPath = Path.GetFullPath(combinedPath);

        return !fullPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase) ? null : fullPath;
    }
}
