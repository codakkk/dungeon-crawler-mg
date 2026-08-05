using System;

namespace DungeonCrawler.Core.SoftwareRenderer;

public class HealthBarRenderer
{
    private BillboardRenderer _renderer;
    
    public HealthBarRenderer(BillboardRenderer renderer)
    {
        _renderer = renderer;
    }

    public void Render(RenderBuffer buffer, double[] wallDepth)
    {
        var data = _renderer.RenderData;

        for (int i = 0; i < data.Count; ++i)
        {
            var item = data[i];
            
            var entity = item.Entity;
            var size = item.SpriteSize;
            
            if(size <= 0)
            {
                continue;
            }
            
            var left = item.ScreenX - size / 2;
            var top = buffer.Height / 2 - size / 2;
            
            var barWidth = (int)(size * 0.6);
            var barHeight = Math.Max(1, size / 24);
            var barX = item.ScreenX - barWidth / 2;
            var barY = top - barHeight - Math.Max(2, size / 20);
            
            var frac = Math.Clamp(entity.Health / (double)entity.MaxHealth, 0, 1);
            int fillW = (int)(barWidth * frac);

            for (int y = barY; y < barY + barHeight; ++y)
            {
                if(y < 0 || y >= buffer.Height) continue;
                for (int x = barX; x < barX + barWidth; ++x)
                {
                    if(x < 0 || x >= buffer.Width) continue;
                    
                    if (item.Depth >= wallDepth[x]) continue;
                    
                    buffer.SetPixel(x, y, Colors.EnemyHealth);
                }
            }
        }
    }
}