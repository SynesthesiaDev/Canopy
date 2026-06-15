// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Canopy.Utils;
using Synesthesia.Utils.Extensions;

namespace Canopy.Graphics;

public abstract class Drawable2D : Drawable
{
    private Invalidation invalidatedFlags = Invalidation.All;

    public bool IsLoaded => LoadState >= DrawableLoadState.Loaded;

    public float X
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry);
        }
    } = 0;

    public float Y
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry);
        }
    } = 0;

    public bool Visible
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            Invalidate(Invalidation.Size | Invalidation.Layout | Invalidation.Size);
        }
    } = true;

    public float Width
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry | Invalidation.Layout | Invalidation.Size);
            Parent?.Invalidate(Invalidation.Layout);
            // invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry); //TODO
        }
    } = 0f;

    public float Height
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            Invalidate(Invalidation.Geometry | Invalidation.Layout | Invalidation.Size);
            Parent?.Invalidate(Invalidation.Layout);
            // invalidateChildrenIfComposite(Invalidation.Size | Invalidation.Geometry); //TODO
        }
    } = 0f;

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

    public float Rotation
    {
        get;
        set
        {
            if (Precision.IsSame(value, field)) return;
            if (!float.IsFinite(value)) throw new ArgumentException($@"{nameof(Rotation)} must be finite, but is {value}.", nameof(value));
            field = value;
            Invalidate(Invalidation.Geometry);
        }
    } = 0f;

    public Axes AutoSizeAxes
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Size | Invalidation.Layout);
        }
    } = Axes.None;

    public Axes RelativeSizeAxes
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Size | Invalidation.Layout);
        }
    } = Axes.None;

    public Vector2 Scale
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Invalidate(Invalidation.Geometry | Invalidation.Size);
        }
    } = Vector2.One;

    public Drawable2D? Parent
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            if (RelativeSizeAxes != Axes.None)
                Invalidate(Invalidation.Layout | Invalidation.Size);
        }
    } = null;

    public void Invalidate(Invalidation flags)
    {
        if ((invalidatedFlags & flags) == flags) return;

        invalidatedFlags |= flags;

        if ((flags & Invalidation.Geometry) != Invalidation.None)
        {
            // if (this is CompositeDrawable2d composite)
            // {
            //     for (int i = 0; i < composite.InternalChildren.Count; i++)
            //         composite.InternalChildren[i].Invalidate(Invalidation.Geometry);
            // }
        }

        if ((flags & Invalidation.Size) != Invalidation.None)
        {
            Parent?.Invalidate(Invalidation.Size);
        }
    }

    public Vector2 InheritedScale => Parent == null ? Scale : Parent.InheritedScale * Scale;

    // ReSharper disable once FunctionRecursiveOnAllPaths
    protected float InheritedAlpha => Alpha * (Parent?.InheritedAlpha ?? 1f);

    protected virtual void OnLayout(Invalidation dirty)
    {
        if (dirty.HasFlagFast(Invalidation.Size))
        {
            UpdateRelativeSize();
        }

        if (dirty.HasFlagFast(Invalidation.DrawNode))
        {
        }
    }

    protected void UpdateLayout()
    {
        if (invalidatedFlags == Invalidation.None) return;

        var dirty = invalidatedFlags;
        invalidatedFlags = Invalidation.None;

        OnLayout(dirty);
    }

    protected virtual void UpdateRelativeSize()
    {
        if (Parent == null) return;

        var targetWidth = RelativeSizeAxes.HasFlagFast(Axes.X)
            ? Parent.Size.X
            : Width;

        var targetHeight = RelativeSizeAxes.HasFlagFast(Axes.Y)
            ? Parent.Size.Y
            : Height;

        // Use size setter only if values actually changed to
        // avoid unnecessary child invalidations
        if (!Precision.IsSame(targetWidth, Width) || !Precision.IsSame(targetHeight, Height))
        {
            Size = new Vector2(targetWidth, targetHeight);
        }
    }

    protected abstract void OnDraw2d();

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Invalidate(Invalidation.All);
        UpdateLayout();
    }

    protected internal override void OnDraw()
    {
        if (!Visible || InheritedAlpha <= 0.001f || !IsLoaded) return;
        OnDraw2d();
    }
}
