// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.Numerics;
using Canopy.Dependency;
using Canopy.Rendering;
using Canopy.Rendering.Textures;
using Canopy.Utils;

namespace Canopy.Graphics;

public class Box2D : Drawable2D
{
    public override BoxDrawInfo DrawInfo { get; } = new BoxDrawInfo
    {
        UvCoords = RectangleF.Empty,
        DrawSize = Vector2.One,
        DrawOffset = Vector2.One,
        PackedColor = Color.White.ToRgba32(),
        CornerRadius = 0f
    };

    public float CornerRadius
    {
        get;
        set
        {
            if (Precision.IsSame(field, value)) return;
            field = value;
            DrawInfo.Invalidate();
        }
    }

    public Texture? Texture
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            DrawInfo.Invalidate();
        }
    } = null;

    public TextureFillMode TextureFillMode
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            DrawInfo.Invalidate();
        }
    } = TextureFillMode.Stretch;

    public Color Color
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            DrawInfo.Invalidate();
        }
    } = Color.White;

    [Resolved]
    private OpenGLRenderer renderer = null!;


    protected void UpdateDrawInfo()
    {
        var drawSize = Size;
        var drawOffset = Vector2.Zero;
        var uvCoords = new RectangleF(0, 0, 1, 1);

        if (Texture != null && TextureFillMode != TextureFillMode.Stretch)
        {
            var textureRatio = (float)Texture.Width / Texture.Height;
            var boxRatio = Size.X / Size.Y;

            switch (TextureFillMode)
            {
                case TextureFillMode.Fit:
                    if (textureRatio > boxRatio)
                    {
                        drawSize = new Vector2(Size.X, Size.X / textureRatio);
                        drawOffset = new Vector2(0, (Size.Y - drawSize.Y) / 2f);
                    }
                    else
                    {
                        drawSize = new Vector2(Size.Y * textureRatio, Size.Y);
                        drawOffset = new Vector2((Size.X - drawSize.X) / 2f, 0);
                    }

                    break;
                case TextureFillMode.Fill:
                    var scaleX = 1f;
                    var scaleY = 1f;

                    if (textureRatio > boxRatio)
                    {
                        scaleX = boxRatio / textureRatio;
                    }
                    else
                    {
                        scaleY = textureRatio / boxRatio;
                    }

                    uvCoords = new RectangleF((1f - scaleX) / 2f, (1f - scaleY) / 2f, scaleX, scaleY);
                    break;
            }
        }

        DrawInfo.UvCoords = uvCoords;
        DrawInfo.DrawSize = drawSize;
        DrawInfo.DrawOffset = drawOffset;
        DrawInfo.PackedColor = Color.ToRgba32();
        DrawInfo.CornerRadius = Math.Clamp(CornerRadius, 0f, Math.Min(Width, Height) / 2f);
    }

    protected internal override void OnUpdate(FrameInfo frameInfo)
    {
        if (DrawInfo.IsDirty) UpdateDrawInfo();
    }

    protected override void OnDraw2d()
    {
        renderer.DrawQuad(
            new DrawMatrix(), //TODO
            position: DrawInfo.DrawOffset,
            size: DrawInfo.DrawSize,
            packedColor: DrawInfo.PackedColor,
            alpha: InheritedAlpha,
            cornerRadius: DrawInfo.CornerRadius,
            texture: Texture,
            textureCoord: DrawInfo.UvCoords
        );
    }

    public class BoxDrawInfo : IDrawInfo
    {
        public required RectangleF UvCoords { get; set; }
        public required Vector2 DrawSize { get; set; }
        public required Vector2 DrawOffset { get; set; }
        public required uint PackedColor { get; set; }
        public required float CornerRadius { get; set; }

        public bool IsDirty { get; private set; }

        public void Invalidate() => IsDirty = true;
    }
}
