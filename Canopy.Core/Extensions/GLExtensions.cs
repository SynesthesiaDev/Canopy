// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using Serilog;
using Silk.NET.OpenGL;

namespace Canopy.Extensions;

public static class GLExtensions
{
    [Conditional("DEBUG")]
    public static void CheckError(this GL gl, string location)
    {
        var error = gl.GetError();
        if (error != GLEnum.NoError)
        {
            Log.Error("OpenGL Error at {Location}: {GLEnum}", location, error);
        }
    }
}
