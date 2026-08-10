using System;
using System.Collections.Generic;
using System.Diagnostics;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Maths;
using DungeonCrawler.Core.SoftwareRenderer.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonCrawler.Core.SoftwareRenderer;

public struct BillboardRenderInfo
{
    public int SpriteSize { get; init; }
    
    public float Depth { get; init; }
    
    public float Lateral { get; init; }
    
    public int ScreenX { get; init; }
    
    public int SpriteWidth { get; set; }
    public int SpriteHeight { get; set; }
    public uint[] TextureData { get; init; }
    public Entity Entity { get; init; }
}

public class BillboardRenderer
{
    private const int AlphaThreshold = 128;
    private const int MaxSpriteSize = 4096;

    // per-frame — reset at top of frame, incremented in the raster loops
    private long _fSprite, _fIters, _fWall, _fVm;

    // per-second — reset only when printing
    private long _accSprite, _accIters, _accWall, _accVm, _maxSprite;
    private int _frames;
    private float _statTimer;
    
    private static readonly Comparison<BillboardRenderInfo> FarToNear =
        (a, b) => b.Depth.CompareTo(a.Depth);
    

    private readonly List<BillboardRenderInfo> _renderData = [];

    public IReadOnlyList<BillboardRenderInfo> RenderData => _renderData;
    private readonly Light[] _lights = new Light[1];
    private int[] _rowOffset = new int[1];
    

    private void BeginFrame()
    {
        _fSprite = _fIters = _fWall = _fVm = 0;
    }

    private void EndFrame(float dt)
    {
        _accSprite += _fSprite;
        _accIters  += _fIters;
        _accWall   += _fWall;
        _accVm     += _fVm;
        _maxSprite  = Math.Max(_maxSprite, _fSprite);
        _frames++;

        _statTimer += dt;
        if (_statTimer < 1f) return;

        long f = Math.Max(1, _frames);
        long waste = 100 - (_accSprite * 100 / Math.Max(1, _accIters));
        Debug.WriteLine(
            $"{_frames}fps | sprite {_accSprite/f/1000}k/f (peak {_maxSprite/1000}k) | " +
            $"iters {_accIters/f/1000}k | waste {waste}% | " +
            $"wall {_accWall/f/1000}k | vm {_accVm/f/1000}k");

        _accSprite = _accIters = _accWall = _accVm = _maxSprite = 0;
        _frames = 0;
        _statTimer = 0f;
    }
    
    public void Render(RenderBuffer buffer, Player player, LightMap lightMap, List<Sprite> sprites, float[] wallDepth)
    {
        BeginFrame();
        
        _renderData.Clear();
        
        for (var i = 0; i < sprites.Count; i++)
        {
            var item = ProcessItem(buffer, player, sprites[i]);
            _renderData.Add(item);
        }
        
        _renderData.Sort(FarToNear);
        _lights[0] = player.Torch;
        
        for (var i = 0; i < _renderData.Count; i++)
        {
            var item = _renderData[i];
            
            // is behind player/camera ?
            if (item.Depth <= 0) continue;
            
            var size = item.SpriteSize;
            if(size <= 0) continue;

            var left = item.ScreenX - size / 2;
            var top = buffer.Height / 2 - size / 2;

            var startX = Math.Max(0, left);
            var endX = Math.Min(buffer.Width, left + size);
            var startY = Math.Max(0, top);
            var endY = Math.Min(buffer.Height, top + size);
            
            if (startX >= endX || startY >= endY) continue;
            
            var brightness = lightMap.Illumination(item.Entity.Position, _lights);
            var flash   = item.Entity.FlashTime > 0f;
            var texData = item.TextureData;
            var texW    = item.SpriteWidth;
            var texH    = item.SpriteHeight;
            var depth   = item.Depth;
            
            var rows = endY - startY;
            if (_rowOffset.Length < rows) _rowOffset = new int[rows];
            for (var k = 0; k < rows; k++)
                _rowOffset[k] = (startY + k - top) * texH / size * texW;
            
            int iters = 0, drawn = 0;
            
            for (var stripe = startX; stripe < endX; stripe++)
            {
                if (item.Depth >= wallDepth[stripe])  continue;
                
                var texX = (stripe - left) * item.SpriteWidth / size;
                for (var y = startY; y < endY; y++)
                {
                    iters++;

                    var texel = texData[texX + _rowOffset[y - startY]];
                    if (texel >> 24 < AlphaThreshold) continue;

                    drawn++;
                    buffer.SetPixel(stripe, y, flash ? 0xFFFFFFFF
                        : RenderBuffer.Shade(texel, brightness));
                    // var texY = (y - top) * item.SpriteHeight / size;
                    // var texel = item.TextureData[texX + texY * item.SpriteWidth];
                    //
                    // if (texel >> 24 < AlphaThreshold)
                    // {
                    //     continue;
                    // }
                    //
                    // // var dither = ((stripe ^ y) & 1) * 8;
                    // texel = RenderBuffer.Shade(texel, brightness);
                    // if (item.Entity.FlashTime > 0.0f) texel = 0xFFFFFFFF;
                    //
                    // drawn++;
                    // buffer.SetPixel(stripe, y, texel);
                }
            }

            _fIters += iters;
            _fSprite += drawn;
        }
        
        EndFrame(Engine.DeltaTime);
    }

    private BillboardRenderInfo ProcessItem(RenderBuffer buffer, Player player, Sprite item)
    {
        // worldOffset = lateral * plane + depth * dir
        // this can be done in a single pass using vectors -> ma sono un cane
        var dir = player.Direction;
        var plane = player.Plane;
        
        var offset = item.Position - player.Position;

        // The camera matrix has plane and dir as its columns:
        //
        //   [ planeX  dirX ] [ lateral ]   [ offsetX ]
        //   [ planeZ  dirZ ] [  depth  ] = [ offsetZ ]
        //
        // We have the offset and want lateral/depth, so invert it: swap the
        // diagonal, negate the off-diagonal, divide by the determinant.
        // offset = lateral * plane + depth * dir
        // offsetX = lateral * planeX + depth * dirX
        // offsetZ = lateral * planeZ + depth * dirZ
        // (depth * dirX - offsetX)/planeX = lateral
        var invDet = 1.0f / Vec2.Cross(plane, dir);
        // var invDet = 1.0 / (planeX * dirZ - dirX * planeZ);
        
        var lateral = invDet * (dir.Z * offset.X - dir.X * offset.Z);
        var depth = invDet * (-plane.Z * offset.X + plane.X * offset.Z);
        
        // same as blocks
        var screenX = 0; 
        var spriteSize = 0;

        if (depth > 0)
        {
            screenX = (int)((float)buffer.Width / 2 * (1 + lateral / depth));
            var rawSize = buffer.Height / depth;
            
            spriteSize = rawSize >= MaxSpriteSize ? MaxSpriteSize : (int)rawSize;
        }
        
        return new BillboardRenderInfo
        {
            Lateral = lateral,
            Depth = depth,
            ScreenX = screenX,
            SpriteSize = spriteSize,
            TextureData = item.TextureData,
            SpriteWidth = item.Width,
            SpriteHeight = item.Height,
            Entity = item.Entity,
        };
    }
}