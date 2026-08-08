using System;
using System.Collections.Generic;
using System.Numerics;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Maths;

namespace DungeonCrawler.Core;

public class Level
{

    public static int Air = 0;
    
    public int Width { get; }
    
    public int Depth { get; }

    public int[] _tiles;

    private List<Entity> _entities = [];

    public IReadOnlyList<Entity> Entities => _entities;

    public Level(int width, int depth)
    {
        Width = width;
        Depth = depth;
        
        _tiles = new int[width * depth];
        _tiles.AsSpan().Fill(Air);   
    }

    public int At(int x, int z)
    {
        return _tiles[x + z * Width];
    }

    public void SetAt(int x, int z, int tile)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Depth)
        {
            return;
        }
        
        _tiles[x + z * Width] = tile;
    }

    public bool IsSolid(int x, int z)
    {
        return _tiles[x + z * Width] > Air;
    }

    public void Set(int[,] tiles)
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int z = 0; z < tiles.GetLength(1); z++)
            {
                var tile = tiles[x, z];
                
                // torch
                if (tile == 6)
                {
                    Spawn(new Torch
                    {
                        Position = new Vec2(x + 0.5f, z + 0.5f),
                        Radius = 0.3f,
                    });
                }

                else
                {
                    _tiles[x + z * Width] = tiles[x, z];
                }
            }
        }
    }

    public bool IsBlocked(Vec2 position, double radius)
    {
        var minX = (int)Math.Floor(position.X - radius);
        var maxX = (int)Math.Floor(position.X + radius);
        var minZ = (int)Math.Floor(position.Z - radius);
        var maxZ = (int)Math.Floor(position.Z + radius);

        for (int zz = minZ; zz <= maxZ; ++zz)
        for (int xx = minX; xx <= maxX; ++xx)
        {
            var isSolid = _tiles[xx + zz * Width] > Air;
            if (isSolid) return true;
        }

        return false;
    }
    
    public bool Spawn(Entity entity)
    {
        if (IsBlocked(entity.Position, entity.Radius))
        {
            return false;
        }

        _entities.Add(entity);
        return true;
    }

    public bool Remove(Entity entity)
    {
        return _entities.Remove(entity);
    }
}