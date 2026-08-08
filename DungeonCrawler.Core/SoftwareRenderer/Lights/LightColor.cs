using System;

namespace DungeonCrawler.Core.SoftwareRenderer.Lights;

public readonly record struct LightColor(float R, float G, float B)
{
    public static readonly LightColor White = new(1.0f, 1.0f, 1.0f);
    public static readonly LightColor Black = new(0.0f, 0.0f, 0.0f);
    
    public static readonly LightColor Torch = new(1.0f, 0.82f, 0.60f);

    public static readonly LightColor DungeonAmbient = new(0.10f, 0.12f, 0.18f);
    
    public static LightColor operator+(LightColor a, LightColor b) => new(a.R + b.R, a.G + b.G, a.B + b.B);
    public static LightColor operator-(LightColor a, LightColor b) => new(a.R - b.R, a.G - b.G, a.B - b.B);
    
    public static LightColor operator*(LightColor a, float b) => new(a.R * b, a.G * b, a.B * b);
    public static LightColor operator*(float b, LightColor a) => new(a.R * b, a.G * b, a.B * b);

    public readonly LightColor Clamped()
    {
        var peak = MathF.Max(R, Math.Max(G, B));

        if (peak <= 1.0f)
        {
            return new LightColor(MathF.Max(0, R), MathF.Max(0, G), MathF.Max(0, B));
        }

        var scale = 1.0f / peak;
        return new LightColor(R * scale, G * scale, B * scale);
    }

    public readonly Shade3 ToShade3() => new(
        (int)(Math.Clamp(R, 0.0f, 1.0f) * 256.0f),
        (int)(Math.Clamp(G, 0.0f, 1.0f) * 256.0f),
        (int)(Math.Clamp(B, 0.0f, 1.0f) * 256.0f)
    );
    
    public readonly System.Numerics.Vector3 ToNumerics() => new(R, G, B);
    
    public static LightColor FromNumerics(in System.Numerics.Vector3 vector) => new(vector.X, vector.Y, vector.Z);
}

public readonly record struct Shade3(int R, int G, int B)
{
    public static readonly Shade3 Full = new Shade3(256, 256, 256);
}