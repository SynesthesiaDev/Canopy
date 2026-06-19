// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Rendering;

namespace Canopy.Graphics;

public interface IDrawable : IDisposable
{
    void Draw(OpenGLRenderer gl);
}
