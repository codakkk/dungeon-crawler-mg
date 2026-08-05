using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonCrawler.Core;

public class WorldMesh : IDisposable
{

    public void Build(GraphicsDevice graphicsDevice, int[] tiles, int width, int height)
    {
        
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}