using System;
using System.Collections.Generic;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Maths;
using DungeonCrawler.Core.SoftwareRenderer.Lights;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonCrawler.Core.SoftwareRenderer;

public class EasyRenderer
{
    public static bool ApplyDithering { get; set; } = false;

    public uint ClearColor { get; }

    private RenderBuffer _renderBuffer;

    private uint[][] _textureColors;

    public float[] WallDepth { get; }

    public EasyRenderer(RenderBuffer buffer, uint clearColor)
    {
        _renderBuffer = buffer;

        ClearColor = clearColor;

        const int BlockSize = 64;
        const int NoOfTextures = 8; // texture.Width / blockSize;
        const int TextureWidth = BlockSize * NoOfTextures;
        var texture = SpriteManager.Get("world_map");

        _textureColors = new uint[NoOfTextures][];

        WallDepth = new float[buffer.Width * buffer.Height];
        for (var i = 0; i < NoOfTextures; ++i)
        {
            var ctex = new uint[BlockSize * BlockSize];
            _textureColors[i] = ctex;
            for (var x = 0; x < BlockSize; ++x)
            for (var y = 0; y < BlockSize; ++y)
            {
                ctex[y + x * BlockSize] = texture.Data[(i * BlockSize + x) + y * TextureWidth];
            }
        }
    }

    public void Render(Level level, Player player, LightMap lightMap)
    {
        _renderBuffer.Clear(ClearColor);

        RenderCeilAndFloor(level, player, lightMap);

        RenderWalls(level, player, lightMap);
    }

    private void RenderWalls(Level level, Player player, LightMap lightMap)
    {
        var targetWidth = _renderBuffer.Width;
        var targetHeight = _renderBuffer.Height;

        for (var x = 0; x < _renderBuffer.Width; x++)
        {
            // Calculate ray position and direction
            var cameraX = 2f * x / targetWidth - 1f; // x-coordinate in camera space [-1, 1] with center on 0

            var rayDir = player.Direction + player.Plane * cameraX;

            var mapX = player.TileX;
            var mapZ = player.TileZ;

            var deltaDistX = rayDir.X == 0 ? float.PositiveInfinity : Math.Abs(1.0f / rayDir.X);
            var deltaDistY = rayDir.Z == 0 ? float.PositiveInfinity : Math.Abs(1.0f / rayDir.Z);

            float sideDistX, sideDistZ;

            // X/Z direction to step
            int stepX, stepZ;

            if (rayDir.X >= 0)
            {
                stepX = 1;
                sideDistX = (mapX + 1.0f - player.Position.X) * deltaDistX;
            }
            else
            {
                stepX = -1;
                sideDistX = (player.Position.X - mapX) * deltaDistX;
            }

            if (rayDir.Z >= 0)
            {
                stepZ = 1;
                sideDistZ = (mapZ + 1.0f - player.Position.Z) * deltaDistY;
            }
            else
            {
                stepZ = -1;
                sideDistZ = (player.Position.Z - mapZ) * deltaDistY;
            }

            var hitCount = 0;

            // NS or EW wall
            var side = 0;
            var totalSteps = 0;
            var tile = Level.Air;

            while (hitCount == 0 && totalSteps < 128)
            {
                totalSteps++;
                if (sideDistX >= sideDistZ)
                {
                    sideDistZ += deltaDistY;
                    mapZ += stepZ;

                    side = 1;
                }
                else
                {
                    sideDistX += deltaDistX;
                    mapX += stepX;

                    side = 0;
                }

                tile = level.At(mapX, mapZ);
                if (tile > Level.Air)
                {
                    hitCount++;
                }
            }

            var perpendicularWallDistance = 0.0f;

            if (side == 0) perpendicularWallDistance = sideDistX - deltaDistX;
            else perpendicularWallDistance = sideDistZ - deltaDistY;

            WallDepth[x] = perpendicularWallDistance;

            // h of line to draw
            var lineHeight = (int)(targetHeight / perpendicularWallDistance);

            var drawStart = -lineHeight / 2 + targetHeight / 2;
            if (drawStart < 0)
            {
                drawStart = 0;
            }

            var drawEnd = lineHeight / 2 + targetHeight / 2;
            if (drawEnd >= targetHeight)
            {
                drawEnd = targetHeight - 1;
            }

            var textureIdx = level.At(mapX, mapZ);

            var hit = player.Position + rayDir * perpendicularWallDistance;
            var wallX = side == 0 ? hit.Z : hit.X;
            wallX -= MathF.Floor(wallX);

            const int TexWidth = 64;
            const int TexHeight = 64;

            var texX = (int)(wallX * TexWidth);
            switch (side)
            {
                case 0 when rayDir.X > 0:
                case 1 when rayDir.Z < 0:
                    texX = TexWidth - texX - 1;
                    break;
            }

            var step = 1.0 * TexHeight / lineHeight;
            var texPos = (drawStart - (double)targetHeight / 2 + (double)lineHeight / 2) * step;
            
            var brightness = lightMap.Illumination(hit, [player.Torch]);
            // int fog = RenderBuffer.FogAmount(perpendicularWallDistance);

            for (var y = drawStart; y < drawEnd; y++)
            {
                if (ApplyDithering)
                {
                    var dither = ((x ^ y) & 1) * 8;
                    // fog = RenderBuffer.FogAmount(perpendicularWallDistance + dither, 3.0f, 8.0f) & ~15;
                }

                var texY = (int)texPos & (TexHeight - 1);
                texPos += step;
                var texture = _textureColors[textureIdx - 1]; // -1 so we can use all the sheet's image
                var texel = texture[texY + texX * TexHeight];

                // if (side == 1) texel = (texel >> 1) & 8355711;

                texel = RenderBuffer.Shade(texel, brightness);
                _renderBuffer.SetPixel(x, y, texel);
            }
        }
    }

    private void RenderCeilAndFloor(Level level, Player player, LightMap lightMap)
    {
        var halfHeight = _renderBuffer.Height / 2;

        // Eye height in pixels: 0.5 world units, scaled by pixels-per-unit.
        var camHeightPixels = 0.5f * _renderBuffer.Height;

        // Rays through the left and right edges of the screen.
        var rayDirX0 = player.Direction.X - player.Plane.X;
        var rayDirZ0 = player.Direction.Z - player.Plane.Z;
        var rayDirX1 = player.Direction.X + player.Plane.X;
        var rayDirZ1 = player.Direction.Z + player.Plane.Z;

        // Sample lighting every N pixels and hold it. Light varies slowly across a
        // floor row, so this is visually indistinguishable from per-pixel and costs
        // a quarter as much. Drop to 1 if we see banding on a torch edge.
        const int LightStep = 4;
        const int TexSize = 64;
        const int FloorIdx = 3;
        const int CeilIdx = 4;
        
        for (var y = halfHeight + 1; y < _renderBuffer.Height; y++)
        {
            var p = y - halfHeight;
            var rowDistance = camHeightPixels / p;

            var stepX = rowDistance * (rayDirX1 - rayDirX0) / _renderBuffer.Width;
            var stepZ = rowDistance * (rayDirZ1 - rayDirZ0) / _renderBuffer.Width;

            var worldX = player.Position.X + rowDistance * rayDirX0;
            var worldZ = player.Position.Z + rowDistance * rayDirZ0;
            
            var brightness = Shade3.Full;
            var playerTorch = new Light
            {
                Enabled = true,
                Position = player.Position,
                Color = LightColor.Torch,
                Intensity = 1f,
                Radius = 4.0f,
            };
            for (var x = 0; x < _renderBuffer.Width; x++)
            {
                if((x & (LightStep - 1)) == 0)
                    brightness = lightMap.Illumination(new Vec2(worldX, worldZ), [playerTorch]);

                var cellX = (int)worldX;
                var cellZ = (int)worldZ;

                var tx = (int)(TexSize * (worldX - cellX)) & (TexSize - 1);
                var ty = (int)(TexSize * (worldZ - cellZ)) & (TexSize - 1);

                worldX += stepX;
                worldZ += stepZ;
                
                // Textures are stored column-major by the constructor, so the
                // index is ty + tx * TexSize — the same order the wall pass uses.
                var idx = ty + tx * TexSize;

                // Floor is the BOTTOM half — this loop's y is below the horizon.
                _renderBuffer.SetPixel(x, y, RenderBuffer.Shade(_textureColors[FloorIdx][idx], brightness));
                _renderBuffer.SetPixel(x, _renderBuffer.Height - y - 1, RenderBuffer.Shade(_textureColors[CeilIdx][idx], brightness));
            }
        }
    }
}