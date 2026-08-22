// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Providers;

public interface IProvider<out T> : IDisposable
{
    T Get();
}
