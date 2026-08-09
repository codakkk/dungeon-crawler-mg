using System;

namespace DungeonCrawler.Core.SoftwareRenderer;

public class HealthBarRenderer
{
    private BillboardRenderer _renderer;
    
    public HealthBarRenderer(BillboardRenderer renderer)
    {
        _renderer = renderer;
    }

    public void Render(RenderBuffer buffer, float[] wallDepth)
    {
        var data = _renderer.RenderData;

        for (var i = 0; i < data.Count; ++i)
        {
            var item = data[i];
            
            var entity = item.Entity;

            if(entity.Health == entity.MaxHealth) continue;
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
            
            var frac = Math.Clamp(entity.Health / (float)entity.MaxHealth, 0, 1);
            var fillW = (int)(barWidth * frac);

            for (var y = barY; y < barY + barHeight; ++y)
            {
                if(y < 0 || y >= buffer.Height) continue;
                for (var x = Math.Max(0, barX); x < Math.Min(buffer.Width, barX + barWidth); x++)
                {
                    if(x < 0 || x >= buffer.Width) continue;
                    
                    if (item.Depth >= wallDepth[x]) continue;
                    var filled = x - barX < fillW;
                    var color = filled ? Colors.EnemyHealth : Colors.EmptyEnemyHealth;
                    buffer.SetPixel(x, y, color);
                }
            }
        }
    }
}