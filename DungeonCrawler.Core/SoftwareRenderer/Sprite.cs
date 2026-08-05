using DungeonCrawler.Core.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonCrawler.Core.SoftwareRenderer;

public class Sprite
{
    public Vec2 Position { get; set; }
    
    public int Width { get; set; }
    public int Height { get; set; }
    
    public Entity Entity { get; set; }

    private Texture2D _texture;
    public Texture2D Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            
            UpdateTextureData(value, _sourceRectangle);
        }
    }

    private Rectangle _sourceRectangle;

    public Rectangle SourceRectangle
    {
        get => _sourceRectangle;
        set
        {
            _sourceRectangle = value;
            UpdateTextureData(_texture, value);
        }
    }
    
    public uint[] TextureData { get; private set; }

    private void UpdateTextureData(Texture2D texture, Rectangle srcRect)
    {
        if (texture == null) return;
        
        var rect = srcRect.IsEmpty ? new Rectangle(0, 0, texture.Width, texture.Height) : srcRect;
        
        Width = rect.Width;
        Height = rect.Height;
        
        var newSz = Width * Height;
        
        TextureData = new uint[newSz];
        texture.GetData(0, rect, TextureData, 0, newSz);
    }
}