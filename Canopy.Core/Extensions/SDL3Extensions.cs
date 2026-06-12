// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.CompilerServices;
using SDL3;
using Serilog;

namespace Canopy.Extensions;

public static class SDL3Extensions
{

    public static int LogErrorIfFailed(this int returnValue, [CallerArgumentExpression("returnValue")] string? expression = null)
    {
        if (returnValue == -1) logError(expression);
        return returnValue;
    }

    extension(bool returnValue)
    {
        public void ThrowIfFailed([CallerArgumentExpression("returnValue")] string? expression = null)
        {
            if (!returnValue) throwError(expression);
        }


        public bool LogErrorIfFailed([CallerArgumentExpression("returnValue")] string? expression = null)
        {
            if (!returnValue) logError(expression);
            return returnValue;
        }
    }

    private static void throwError(string? expression) => throw new SDLPlatformException("SDL Error", expression);

    private static void logError(string? expression)
    {
        Log.Error("SDL error: {GetError}", SDL.GetError());
        if (!string.IsNullOrEmpty(expression))
            Log.Error("at {Expression}", expression);
    }
}

