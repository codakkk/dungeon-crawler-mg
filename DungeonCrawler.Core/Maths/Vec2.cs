
namespace DungeonCrawler.Core.Maths;

/// <summary>
/// A Vector2 representation with X and Z instead of X/Y to avoid confusion (there's no real Y in world logic)
/// </summary>
/// <param name="X"></param>
/// <param name="Z"></param>
public readonly record struct Vec2(float X, float Z)
{
    public static readonly Vec2 Zero = new Vec2(0.0f, 0.0f);
    public static readonly Vec2 One = new Vec2(1.0f, 1.0f);
    public static readonly Vec2 UnitX = new Vec2(1.0f, 0.0f);
    public static readonly Vec2 UnitZ = new Vec2(0.0f, 1.0f);

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Z + b.Z);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Z - b.Z);
    public static Vec2 operator -(Vec2 a) => new(-a.X, -a.Z);
    public static Vec2 operator *(Vec2 a, float b) => new(a.X * b, a.Z * b);
    public static Vec2 operator *(float b, Vec2 a) => a * b;
    public static Vec2 operator /(Vec2 a, float b) => new(a.X / b, a.Z / b);

    public float LengthSquared => X * X + Z * Z;
    
    public float Length => float.Sqrt(LengthSquared);

    public Vec2 Normalized => this / Length;
    
    public Vec2 Perpendicular => new (Z, -X);
    
    public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Z * b.Z;
    
    /// <summary>
    /// 2D cross product - equals to |a||b|*sin(theta).
    /// Sign of return tells which side b lies on relative to a
    /// </summary>
    public static float Cross(Vec2 a, Vec2 b) => a.X * b.Z - a.Z * b.X;

    public Vec2 Rotated(float radians)
    {
        var cos = float.Cos(radians);
        var sin = float.Sin(radians);
        return new Vec2(X * cos - Z * sin, X * sin + Z * cos);
    }
    
    public override string ToString() => $"({X:0.###}, {Z:0.###})";

    public System.Numerics.Vector2 ToNumerics() => new([X, Z]);
    
    public Microsoft.Xna.Framework.Vector2 ToVector2() => new(X, Z);
}