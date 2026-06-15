// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using Canopy.Dependency;
using Canopy.Utils.Timing;
using Serilog;

namespace Canopy.Graphics;

public abstract class Drawable : IDisposable
{
    private static readonly StopwatchClock performance_watch = new(true);
    protected internal bool IsDisposed { get; private set; }
    internal readonly object LoadLock = new();

    public float Alpha { get; set; } = 1f;

    public BlendMode BlendMode { get; set; } = BlendMode.Alpha;

    public DrawableLoadState LoadState { get; protected set; }

    protected internal abstract void OnDraw();

    protected internal virtual void OnUpdate(FrameInfo frameInfo)
    {
    }

    internal void Load()
    {
        lock (LoadLock)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (LoadState != DrawableLoadState.NotLoaded) return;

            Trace.Assert(LoadState == DrawableLoadState.NotLoaded);
            LoadState = DrawableLoadState.Loading;

            load();

            LoadState = DrawableLoadState.Ready;

            loadComplete();
        }
    }

    private void load()
    {
        var timeBefore = performance_watch.CurrentTime;

        Reflection.ResolveDependencies(this);

        OnLoading();

        // if (this is Drawable2d drawable)
        // {
        // drawable.Invalidate(Invalidation.All);
        // drawable.Parent?.Invalidate(Invalidation.All);
        // }

        if (!(timeBefore > 1000)) return;

        var loadDuration = performance_watch.CurrentTime - timeBefore;

        if (!(loadDuration > 16)) return;

        Log.Warning("{this} took {time}ms to load (and blocked the thread)", GetType().Name, $"{loadDuration:0.00}");
    }


    protected virtual void OnLoading()
    {
    }

    protected virtual void LoadComplete()
    {
    }


    private bool loadComplete()
    {
        if (LoadState < DrawableLoadState.Ready) return false;

        LoadState = DrawableLoadState.Loaded;

        // if (this is Drawable2d drawable2d)
        // {
            // drawable2d.Invalidate(Invalidation.All);
        // }

        LoadComplete();
        return true;
    }


    public void Dispose()
    {
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (IsDisposed) return;
        IsDisposed = true;
        // Scheduler.Value.Dispose(); //TODO
    }

    public enum DrawableLoadState
    {
        NotLoaded,
        Loading,
        Ready,
        Loaded
    }

}
