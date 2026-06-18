// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.CompilerServices;
using Canopy.Utils;
using Synesthesia.Utils.Pooling;

namespace Canopy.Graphics;

public class DrawMatrix : IPooledObject, IEquatable<DrawMatrix>
{
    public Matrix4x4 Matrix { get; set; } = Matrix4x4.Identity;

    public Matrix4x4 InverseMatrix { get; set; } = Matrix4x4.Identity;

    public void Reset()
    {
        Matrix = Matrix4x4.Identity;
        InverseMatrix = Matrix4x4.Identity;
    }

    public bool IsPooled { get; set; }
    public Action<IPooledObject>? ReturnAction { get; set; }

    public void Translate(float x, float y, float z)
    {
        Matrix = Matrix4x4.CreateTranslation(x, y, z) * Matrix;
        InverseMatrix *= Matrix4x4.CreateTranslation(-x, -y, -z);
    }

    public void Scale(float x, float y, float z)
    {
        Matrix = Matrix4x4.CreateScale(x, y, z) * Matrix;
        InverseMatrix *= Matrix4x4.CreateScale(1 / x, 1 / y, 1 / z);
    }

    public void Rotate(float degrees, float x, float y, float z)
    {
        var rads = degrees.ToRads();
        var axis = Vector3.Normalize(new Vector3(x, y, z));

        Matrix = Matrix4x4.CreateFromAxisAngle(axis, rads) * Matrix;
        InverseMatrix *= Matrix4x4.CreateFromAxisAngle(axis, -rads);
    }

    public void RotateAround(Vector2 pivot, float degrees)
    {
        Translate(-pivot.X, -pivot.Y, 0);
        Rotate(degrees, 0, 0, 1);
        Translate(pivot.X, pivot.Y, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ScreenToLocal(Vector2 screenPos)
    {
        return Vector2.Transform(screenPos, InverseMatrix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ScreenToLocalDirection(Vector2 screenDelta)
    {
        return Vector2.TransformNormal(screenDelta, InverseMatrix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsPoint(Vector2 screenPos, Vector2 size)
    {
        var localPos = ScreenToLocal(screenPos);
        return localPos.X >= 0 && localPos.X <= size.X &&
               localPos.Y >= 0 && localPos.Y <= size.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsPoint(Vector2 screenPos, Vector2 offset, Vector2 size)
    {
        var localPos = ScreenToLocal(screenPos);
        return localPos.X >= offset.X && localPos.X <= offset.X + size.X &&
               localPos.Y >= offset.Y && localPos.Y <= offset.Y + size.Y;
    }

    public bool Equals(DrawMatrix? other) => Matrix.Equals(other?.Matrix);

    public override string ToString()
    {
        return $"DrawMatrix(Matrix={Matrix}, InverseMatrix={InverseMatrix}, IsPooled={IsPooled})";
    }
}
