// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Storage;

public abstract class AssetStorage
{
    private readonly Dictionary<string, object> cacheTyped = new();

    public abstract string[] GetAllFiles();

    public abstract bool FileExists(string path);

    public abstract byte[]? GetOrNull(string path);

    public byte[] Get(string path, string exception = "File with that path was not found")
    {
        return GetOrNull(path) ?? throw new FileNotFoundException(exception, path);
    }

    public T? GetResolvedOrNull<T>(string path, Func<byte[], string, T> dataParser) where T: class
    {
        if (cacheTyped.TryGetValue(path, out var cachedObj) && cachedObj is T typed)
            return typed;

        var item = GetOrNull(path);
        if (item == null) return null;

        var resolved = dataParser.Invoke(item, path);
        cacheTyped.TryAdd(path, resolved);

        return resolved;
    }

    public T GetResolved<T>(string path, Func<byte[], string, T> dataParser,  string exception = "File with that path was not found") where T: class
    {
        return GetResolvedOrNull(path, dataParser) ?? throw new FileNotFoundException(exception, path);
    }
}
