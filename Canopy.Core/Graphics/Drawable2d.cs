// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Canopy.Utils;

namespace Canopy.Graphics;

public abstract class Drawable2D : Drawable
{
    public abstract IDrawInfo DrawInfo { get; }

    public bool IsLoaded => LoadState >= DrawableLoadState.Loaded;

    public float X
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            DrawInfo.Invalidate();
        }
    }

    public float Y
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            DrawInfo.Invalidate();
        }
    }

    public bool Visible { get; set; } = true;

    public float Width
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            DrawInfo.Invalidate();
        }
    }

    public float Height
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            DrawInfo.Invalidate();
        }
    }

    public Vector2 Size
    {
        get => new Vector2(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public Vector2 Position
    {
        get => new Vector2(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public float Rotation { get; set; }

    public Vector2 Scale
    {
        get;
        set
        {
            if(field == value) return;
            field = value;
            DrawInfo.Invalidate();
        }
    } = Vector2.One;

    public Drawable2D? Parent { get; set; } = null;

    public Vector2 InheritedScale => Parent == null ? Scale : Parent.InheritedScale * Scale;

    // ReSharper disable once FunctionRecursiveOnAllPaths
    protected float InheritedAlpha => Alpha * (Parent?.InheritedAlpha ?? 1f);

    protected abstract void OnDraw2d();

    protected internal override void OnDraw()
    {
        if (!Visible || InheritedAlpha <= 0.001f || !IsLoaded) return;
        OnDraw2d();
    }
}
