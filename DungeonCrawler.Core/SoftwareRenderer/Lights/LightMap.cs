using System;
using System.Collections.Generic;
using DungeonCrawler.Core.Maths;

namespace DungeonCrawler.Core.SoftwareRenderer.Lights;

public class LightMap
{
    public LightColor AmbientLight { get; set; } = LightColor.DungeonAmbient;

    private int _stride;
    private int _width;
    private int _depth;
    
    private LightColor[] _corners;


    public void Bake(Level level, IReadOnlyList<Light> lights)
    {
        _width = level.Width;
        _depth = level.Depth;
        _stride = _width + 1;
        
        _corners = new LightColor[_stride * (_depth + 1)];

        for (var z = 0; z < _depth; z++)
        {
            for (var x = 0; x < _width; x++)
            {
                var total = LightColor.Black;
                var corner = new Vec2(x, z);

                for (var i = 0; i < lights.Count; i++)
                {
                    var light = lights[i];

                    var contribution =  light.ContributionAt(light.Position);
                    if (contribution == LightColor.Black) continue;
                    
                    if (IsOccluded(level, light.Position, corner)) continue;
                    
                    total += contribution;
                }

                _corners[x + z * _stride] = total;
            }
        }
    }

    public LightColor SampleStatic(Vec2 worldPosition)
    {
        if(_corners.Length == 0) return LightColor.Black;
        
        var tx = (int)worldPosition.X;
        var tz = (int)worldPosition.Z;
        
        if(tx < 0 || tx >= _width || tz < 0 || tz >= _depth) return LightColor.Black;

        // we take only the decimal part to interpolate (how much worldPosition.X is in the tile tx)?
        // Example: if worldPosition.X = 5.7 and tx = 5 -> fx = 0.7
        var fx = worldPosition.X - tx;
        var fz = worldPosition.Z - tz;

        var i = tx + tz * _stride;
        
        var top = _corners[i] * (1-fx) +  _corners[i + 1] * fx;
        var bottom = _corners[i + _stride] * (1-fx) +  _corners[i + _stride + 1] * fx;
        
        return top * (1-fz) + bottom * fz;
    }

    public Shade3 Illumination(Vec2 position, ReadOnlySpan<Light> lights)
    {
        var lit = AmbientLight + SampleStatic(position);

        for (var i = 0; i < lights.Length; ++i)
            lit += lights[i].ContributionAt(position);
        
        return lit.Clamped().ToShade3();
    }
    
    public Shade3 Illumination(Vec2 position, in Light light)
    {
        return (AmbientLight + SampleStatic(position) + light.ContributionAt(position)).Clamped().ToShade3();
    }

    public bool IsOccluded(Level level, Vec2 from, Vec2 to)
    {
        var delta = to - from;
        var steps = (int)(delta.Length * 4.0);
        
        if (steps <= 1) return false;

        for (var step = 1; step < steps; ++step)
        {
            var t = step / (float)steps;
            var cx = (int)(from.X + delta.X * t);
            var cz = (int)(from.Z + delta.Z * t);

            if (level.IsSolid(cx, cz)) return true;
        }
        return false;
    }
}