// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Canopy;

public static class Utils
{
    public static void CopyEmbeddedFolder(Assembly assembly, string folderPrefix, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        string searchPrefix = folderPrefix.EndsWith('/') ? folderPrefix : folderPrefix + "/";

        foreach (string resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(searchPrefix, StringComparison.OrdinalIgnoreCase)) continue;

            string relativePath = resourceName[searchPrefix.Length..];

            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            string destinationFilePath = Path.Combine(destinationDirectory, relativePath);

            string? parentDir = Path.GetDirectoryName(destinationFilePath);
            if (!string.IsNullOrEmpty(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            using Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null) continue;

            using FileStream fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write);
            resourceStream.CopyTo(fileStream);
        }
    }

    public static void OpenFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", $"\"{folderPath}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", $"\"{folderPath}\"");
        }
    }
}
