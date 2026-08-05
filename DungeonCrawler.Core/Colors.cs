using DungeonCrawler.Core.SoftwareRenderer;

namespace DungeonCrawler.Core;

public static class Colors
{
    public static readonly uint Flash = RenderBuffer.Rgb(0xFFFFFF);
    public static readonly uint EnemyHealth = RenderBuffer.Rgb(0xFF0000);
    
    public static readonly uint Fog = RenderBuffer.Rgb(0x0E0C14);
}