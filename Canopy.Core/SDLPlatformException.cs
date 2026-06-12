// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy;


public class SDLPlatformException(string message, string? expression = null) : Exception
{
    public override string Message => expression != null ? $"{message} at {expression}" : message;
}
