using System;
using DungeonCrawler.Core.Maths;

namespace DungeonCrawler.Core.SoftwareRenderer.Lights;

public readonly struct Light
{
    public bool Enabled { get; init; }
    public Vec2 Position { get; init; }
    public float Intensity { get; init; }
    public float Radius { get; init; }
    public LightColor Color { get; init; }
    

    public readonly LightColor ContributionAt(Vec2 worldPosition)
    {
        if(Radius <= 0.0f || !Enabled) return LightColor.Black;
        
        var dist = (worldPosition - Position).LengthSquared;
        if (dist >= Radius * Radius) return LightColor.Black;
        var falloff = 1.0f - MathF.Sqrt(dist) / Radius;
        
        var color = Color == default ? LightColor.White : Color;
        return color * (Intensity * falloff * falloff);
    }
}