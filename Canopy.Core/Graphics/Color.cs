// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Faster.Map.Core;

namespace Canopy.Graphics;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Color : IEquatable<Color>
{
    public const double GAMMA = 2.4;

    private static readonly DenseMap<string, Color> hexColorCache = new();

    public readonly float R;
    public readonly float G;
    public readonly float B;
    public readonly float A;

    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(byte r, byte g, byte b, byte a)
    {
        R = (float)r / byte.MaxValue;
        G = (float)g / byte.MaxValue;
        B = (float)b / byte.MaxValue;
        A = (float)a / byte.MaxValue;
    }

    public int ToArgb()
    {
        return (int)(uint)(A * (double)byte.MaxValue) << 24 | (int)(uint)(R * (double)byte.MaxValue) << 16 /*0x10*/ | (int)(uint)((double)G * byte.MaxValue) << 8 | (int)(uint)((double)B * byte.MaxValue);
    }

    //abgr in memory (Little Endian) for shaders
    public uint ToRgba32()
    {
        uint r = (uint)(Math.Clamp(R, 0, 1) * 255);
        uint g = (uint)(Math.Clamp(G, 0, 1) * 255);
        uint b = (uint)(Math.Clamp(B, 0, 1) * 255);
        uint a = (uint)(Math.Clamp(A, 0, 1) * 255);

        return r | (g << 8) | (b << 16) | (a << 24);
    }

    public static bool operator ==(Color? left, Color? right) => left.Equals(right);

    public static bool operator !=(Color? left, Color? right) => !left.Equals(right);

    // Windows lacks a fast path for x == 1 in Math.Pow and given passing color
    // is 1 (White or Transparent) is very common, we add a fast path for it
    public static double ToLinear(double color)
    {
        if (color == 1)
            return 1;

        return color <= 0.04045 ? color / 12.92 : Math.Pow((color + 0.055) / 1.055, GAMMA);
    }

    public static float ToLinear(float color)
    {
        if (color == 1)
            return 1;

        return color <= 0.04045f ? color / 12.92f : MathF.Pow((color + 0.055f) / 1.055f, (float)GAMMA);
    }

    public static double ToSRGB(double color)
    {
        if (color == 1)
            return 1;

        return color < 0.0031308 ? 12.92 * color : 1.055 * Math.Pow(color, 1.0 / GAMMA) - 0.055;
    }

    public static float ToSRGB(float color)
    {
        if (color == 1)
            return 1;

        return color < 0.0031308f ? 12.92f * color : 1.055f * MathF.Pow(color, 1.0f / (float)GAMMA) - 0.055f;
    }

    public Color WithOpacity(float a) => new Color(R, G, B, a);

    public Color WithOpacity(byte a) => new Color(R, G, B, a / 255f);

    public Color ToLinear() =>
        new Color(
            ToLinear(R),
            ToLinear(G),
            ToLinear(B),
            A);

    public Color ToSRGB() =>
        new Color(
            ToSRGB(R),
            ToSRGB(G),
            ToSRGB(B),
            A);


    // fast paths for white cause multiplying any color by white results in white
    public static Color Multiply(Color first, Color second)
    {
        if (first.Equals(White))
            return second;

        if (second.Equals(White))
            return first;

        return new Color(
            first.R * second.R,
            first.G * second.G,
            first.B * second.B,
            first.A * second.A);
    }

    /// <summary>
    /// Returns a lightened version of the color.
    /// </summary>
    /// <param name="amount">Decimal light addition</param>
    public Color Lighten(float amount) => Multiply(1 + amount);

    /// <summary>
    /// Returns a darkened version of the color.
    /// </summary>
    /// <param name="amount">Percentage light reduction</param>
    public Color Darken(float amount) => Multiply(1 / (1 + amount));

    public Color Multiply(float scalar)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(scalar);

        return new Color(
            Math.Min(1, R * scalar),
            Math.Min(1, G * scalar),
            Math.Min(1, B * scalar),
            A);
    }

    public static Color FromVector(Vector4 vector) => new Color(vector.X, vector.Y, vector.Z, vector.W);

    public Vector4 ToVector() => new Vector4(R, G, B, A);


    public static Color FromHex(string hex)
    {
        if (hexColorCache.Get(hex, out var value)) return value;

        var hexSpan = hex[0] == '#' ? hex.AsSpan()[1..] : hex.AsSpan();

        var color = hexSpan.Length switch
        {
            3 => new Color(
                (byte)(byte.Parse(hexSpan.Slice(0, 1), NumberStyles.HexNumber) * 17),
                (byte)(byte.Parse(hexSpan.Slice(1, 1), NumberStyles.HexNumber) * 17),
                (byte)(byte.Parse(hexSpan.Slice(2, 1), NumberStyles.HexNumber) * 17),
                255),
            6 => new Color(
                byte.Parse(hexSpan.Slice(0, 2), NumberStyles.HexNumber),
                byte.Parse(hexSpan.Slice(2, 2), NumberStyles.HexNumber),
                byte.Parse(hexSpan.Slice(4, 2), NumberStyles.HexNumber),
                255),
            4 => new Color(
                (byte)(byte.Parse(hexSpan.Slice(0, 1), NumberStyles.HexNumber) * 17),
                (byte)(byte.Parse(hexSpan.Slice(1, 1), NumberStyles.HexNumber) * 17),
                (byte)(byte.Parse(hexSpan.Slice(2, 1), NumberStyles.HexNumber) * 17),
                (byte)(byte.Parse(hexSpan.Slice(3, 1), NumberStyles.HexNumber) * 17)),
            8 => new Color(
                byte.Parse(hexSpan.Slice(0, 2), NumberStyles.HexNumber),
                byte.Parse(hexSpan.Slice(2, 2), NumberStyles.HexNumber),
                byte.Parse(hexSpan.Slice(4, 2), NumberStyles.HexNumber),
                byte.Parse(hexSpan.Slice(6, 2), NumberStyles.HexNumber)),
            _ => throw new ArgumentException("Invalid hex string length!", nameof(hex)),
        };

        hexColorCache.Insert(hex, color);
        return color;
    }

    public string ToHex(bool alwaysOutputAlpha = false)
    {
        int argb = ToArgb();
        byte a = (byte)(argb >> 24);
        byte r = (byte)(argb >> 16);
        byte g = (byte)(argb >> 8);
        byte b = (byte)argb;

        if (!alwaysOutputAlpha && a == 255)
            return $"#{r:X2}{g:X2}{b:X2}";

        var hex = $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        hexColorCache.Insert(hex, this);
        return hex;
    }

    public System.Drawing.Color ToMicroslopColor()
    {
        return System.Drawing.Color.FromArgb(ToArgb());
    }

    // ReSharper disable CompareOfFloatsByEqualityOperator
    public bool Equals(Color other)
    {
        return R == (double)other.R && G == (double)other.G && B == (double)other.B && A == (double)other.A;
    }
    // ReSharper restore CompareOfFloatsByEqualityOperator

    public override bool Equals(object? obj) => obj is Color other && Equals(other);

    public override int GetHashCode() => ToArgb();

    public static Color White = new Color(255, 255, 255, 255);
    public static Color Black = new Color(0, 0, 0, 255);
    public static Color Transparent = new Color(0, 0, 0, 0);

    public static uint WhitePacked = White.ToRgba32();
    public static uint BlackPacked = Black.ToRgba32();
    public static uint TransparentPacked = Transparent.ToRgba32();

    public static Color Red = new Color(255, 0, 0, 255);
    public static Color Green = new Color(0, 255, 0, 255);
    public static Color Blue = new Color(0, 0, 255, 255);

    public static Color Yellow = new Color(255, 255, 0, 255);
    public static Color Cyan = new Color(0, 255, 255, 255);
    public static Color Magenta = new Color(255, 0, 255, 255);

    public static Color Gray = new Color(128, 128, 128, 255);
    public static Color LightGray = new Color(211, 211, 211, 255);
    public static Color DarkGray = new Color(64, 64, 64, 255);
    public static Color SlateGray = new Color(112, 128, 144, 255);

    public static Color Orange = new Color(255, 165, 0, 255);
    public static Color Brown = new Color(165, 42, 42, 255);
    public static Color Purple = new Color(128, 0, 128, 255);
    public static Color Gold = new Color(255, 215, 0, 255);
    public static Color SkyBlue = new Color(135, 206, 235, 255);

    public static Color Crimson = new Color(220, 20, 60, 255);
    public static Color ForestGreen = new Color(34, 139, 34, 255);
    public static Color RoyalBlue = new Color(65, 105, 225, 255);

    public override string ToString()
    {
        return $"Color({R}, {G}, {B}, {A})";
    }
}
