using System;
using System.Collections.Generic;

namespace DungeonCrawler.Core.SoftwareRenderer;

public struct Light
{
    public Vec2 Position { get; init; }
    public double Intensity { get; init; }
    public double Radius { get; init; }

    public double ContributionAt(Vec2 worldPosition)
    {
        if(Radius <= 0.0f) return 0.0f;
        
        var dist = (worldPosition - Position).LengthSquared;
        if (dist >= Radius * Radius) return 0.0f;
        var falloff = 1.0 - Math.Sqrt(dist) / Radius;
        return Intensity * falloff * falloff;
    }
}

public class LightMap
{
    public double AmbientLight { get; set; }

    private int _stride;
    private int _width;
    private int _depth;
    
    private double[] _corners;


    public void Bake(Level level, IReadOnlyList<Light> lights)
    {
        _width = level.Width;
        _depth = level.Depth;
        _stride = _width + 1;
        
        _corners = new double[_stride * (_depth + 1)];

        for (var z = 0; z < _depth; z++)
        {
            for (var x = 0; x < _width; x++)
            {
                double total = 0.0f;
                var corner = new Vec2(x, z);

                for (var i = 0; i < lights.Count; i++)
                {
                    var light = lights[i];

                    var contribution =  light.ContributionAt(light.Position);
                    if (contribution <= 0.00f) continue;
                    
                    if (IsOccluded(level, light.Position, corner)) continue;
                    
                    total += contribution;
                }

                _corners[x + z * _stride] = total > 1 ? 1 : total;
            }
        }
    }

    public double SampleStatic(Vec2 worldPosition)
    {
        if(_corners.Length == 0) return 0;
        
        var tx = (int)worldPosition.X;
        var tz = (int)worldPosition.Z;
        
        if(tx < 0 || tx >= _width || tz < 0 || tz >= _depth) return 0;

        // we take only the decimal part to interpolate (how much worldPosition.X is in the tile tx)?
        // Example: if worldPosition.X = 5.7 and tx = 5 -> fx = 0.7
        var fx = worldPosition.X - tx;
        var fz = worldPosition.Z - tz;

        var i = tx + tz * _stride;
        
        var top = _corners[i] * (1-fx) +  _corners[i + 1] * fx;
        var bottom = _corners[i + _stride] * (1-fx) +  _corners[i + _stride + 1] * fx;
        
        return top * (1-fz) + bottom * fz;
    }

    public int Brightness(Vec2 worldPosition, ReadOnlySpan<Light> dynamicLights)
    {
        double lit = SampleStatic(worldPosition);

        for (int i = 0; i < dynamicLights.Length; ++i)
            lit += dynamicLights[i].ContributionAt(worldPosition);
        
        lit = Math.Clamp(AmbientLight + lit, 0.0f, 1.0f);
        return (int)lit * 256;
    }
    
    public int Brightness(Vec2 worldPosition, in Light light)
    {
        var lit = SampleStatic(worldPosition) + light.ContributionAt(worldPosition);
        
        lit = Math.Clamp(AmbientLight + lit, 0.0f, 1.0f);
        return (int)lit * 256;
    }

    public bool IsOccluded(Level level, Vec2 from, Vec2 to)
    {
        var delta = to - from;
        var steps = (int)(delta.Length * 4.0);
        
        if (steps <= 1) return false;

        for (var step = 1; step < steps; ++step)
        {
            var t = step / (double)steps;
            var cx = (int)(from.X + delta.X * t);
            var cz = (int)(from.Z + delta.Z * t);

            if (level.IsSolid(cx, cz)) return true;
        }
        return false;
    }
}