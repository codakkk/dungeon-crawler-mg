using System;
using DungeonCrawler.Core.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonCrawler.Core.SoftwareRenderer;

public class EasyRenderer
{
    public uint ClearColor { get; }
    
    private RenderBuffer _renderBuffer;
    
    private uint[][] _textureColors;
    
    public double[] WallDepth { get; }
    
    public EasyRenderer(RenderBuffer buffer, uint clearColor)
    {
        _renderBuffer = buffer;
        
        ClearColor = clearColor;

        const int blockSize = 64;
        const int noOfTextures = 8; // texture.Width / blockSize;
        const int textureWidth = blockSize * noOfTextures;
        var texture = SpriteManager.Get("world_map");
        
        _textureColors = new uint[noOfTextures][];
        
        WallDepth = new double[buffer.Width * buffer.Height];
        for (int i = 0; i < noOfTextures; ++i)
        {
            var ctex = new uint[blockSize * blockSize];
            _textureColors[i] = ctex;
            for(int x = 0; x < blockSize; ++x)
            for(int y = 0; y < blockSize; ++y)
            {
                ctex[y + x * blockSize] = texture.Data[(i * blockSize + x) + y * textureWidth];
            }
        }
    }
    
    public void Render(Level level, Player player)
    {
        PrepareBuffer(level, player);
    }
    
    private void PrepareBuffer(Level level, Player player)
    {
        _renderBuffer.Clear(ClearColor);
        
        RenderCeilAndFloor(level, player);
        
        RenderWalls(level, player);
    }

    private void RenderWalls(Level level, Player player)
    {
        int targetWidth = _renderBuffer.Width;
        int targetHeight = _renderBuffer.Height;
        
        for (int x = 0; x < _renderBuffer.Width; x++)
        {
            // Calculate ray position and direction
            double cameraX = 2 * x / (double)targetWidth - 1; // x-coordinate in camera space [-1, 1] with center on 0
            
            double rayDirX = player.Direction.X + player.Plane.X * cameraX;
            double rayDirY = player.Direction.Z + player.Plane.Z * cameraX;

            int mapX = player.TileX;
            int mapZ = player.TileZ;
            
            double deltaDistX = rayDirX == 0 ? double.PositiveInfinity : Math.Abs(1.0f / rayDirX);
            double deltaDistY = rayDirY == 0 ? double.PositiveInfinity : Math.Abs(1.0f / rayDirY);

            double sideDistX, sideDistZ;
            
            // X/Z direction to step
            int stepX, stepZ;

            if (rayDirX >= 0)
            {
                stepX = 1;
                sideDistX = (mapX + 1.0f - player.Position.X) * deltaDistX;
            }
            else
            {
                stepX = -1;
                sideDistX = (player.Position.X - mapX) * deltaDistX;
            }
            
            if (rayDirY >= 0)
            {
                stepZ = 1;
                sideDistZ = (mapZ + 1.0f - player.Position.Z) * deltaDistY;
            }
            else
            {
                stepZ = -1;
                sideDistZ = (player.Position.Z - mapZ) * deltaDistY;
            }

            int hitCount = 0;
            
            // NS or EW wall
            int side = 0;
            int totalSteps = 0;
            int tile = Level.Air;

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

            double perpendicularWallDistance = 0.0f;

            if (side == 0) perpendicularWallDistance = sideDistX - deltaDistX;
            else perpendicularWallDistance = sideDistZ - deltaDistY;
            
            WallDepth[x] = perpendicularWallDistance;
            
            // h of line to draw
            int lineHeight = (int)(targetHeight / perpendicularWallDistance);

            int drawStart = -lineHeight / 2 + targetHeight / 2;
            if(drawStart < 0)
            {
                drawStart = 0;
            }
            
            int drawEnd = lineHeight / 2 + targetHeight / 2;
            if (drawEnd >= targetHeight)
            {
                drawEnd = targetHeight - 1;
            }
            
            int textureIdx = level.At(mapX, mapZ);
            double wallX = 0;

            if (side == 0) wallX = player.Position.Z + perpendicularWallDistance * rayDirY;
            else wallX = player.Position.X + perpendicularWallDistance * rayDirX;

            wallX -= Math.Floor(wallX);

            const int texWidth = 64;
            const int texHeight = 64;
            
            int texX = (int)(wallX * texWidth);
            switch (side)
            {
                case 0 when rayDirX > 0:
                case 1 when rayDirY < 0:
                    texX = texWidth - texX - 1;
                    break;
            }

            double step = 1.0 * texHeight / lineHeight;
            double texPos = (drawStart - (double)targetHeight / 2 + (double)lineHeight / 2) * step;

            for (int y = drawStart; y < drawEnd; y++)
            {
                var dither = ((x ^ y) & 1) * 8;
                int fog = RenderBuffer.FogAmount(perpendicularWallDistance + dither, 1.0f, 5.0f) & ~15;
                
                int texY = (int)texPos & (texHeight - 1);
                texPos += step;
                var texture = _textureColors[textureIdx-1]; // -1 so we can use all the sheet's image
                var color = texture[texY + texX * texHeight];

                if (side == 1) color = (color >> 1) & 8355711;
                _renderBuffer.SetPixel(x, y, RenderBuffer.Blend(color, Colors.Fog, fog));
            }
        }
    }
    
    private void RenderCeilAndFloor(Level level, Player player)
    {
        int halfHeight = _renderBuffer.Height / 2;

        // Eye height in pixels: 0.5 world units, scaled by pixels-per-unit.
        double camHeightPixels = 0.5 * _renderBuffer.Height;

        // Rays through the left and right edges of the screen.
        double rayDirX0 = player.Direction.X - player.Plane.X;  
        double rayDirZ0 = player.Direction.Z - player.Plane.Z;
        double rayDirX1 = player.Direction.X + player.Plane.X;  
        double rayDirZ1 = player.Direction.Z + player.Plane.Z;

        for (int y = halfHeight + 1; y < _renderBuffer.Height; y++)
        {
            int p = y - halfHeight;
            double rowDistance = camHeightPixels / p;

            double stepX = rowDistance * (rayDirX1 - rayDirX0) / _renderBuffer.Width;
            double stepZ = rowDistance * (rayDirZ1 - rayDirZ0) / _renderBuffer.Width;

            double worldX = player.Position.X + rowDistance * rayDirX0;
            double worldZ = player.Position.Z + rowDistance * rayDirZ0;

            for (int x = 0; x < _renderBuffer.Width; x++)
            {
                int cellX = (int)worldX;
                int cellZ = (int)worldZ;

                int tx = (int)(64 * (worldX - cellX)) & (64 - 1);
                int ty = (int)(64 * (worldZ - cellZ)) & (64 - 1);

                const int floorIdx = 3;
                const int ceilIdx = 4;

                worldX += stepX;
                worldZ += stepZ;

                _renderBuffer.SetPixel(x, y, _textureColors[ceilIdx][tx + ty * 64]);
                _renderBuffer.SetPixel(x, _renderBuffer.Height - y - 1, _textureColors[floorIdx][tx + ty * 64]);
            }
        }
    }
    
}