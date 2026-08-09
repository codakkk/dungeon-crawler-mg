using System;
using DungeonCrawler.Core.SoftwareRenderer;
using Microsoft.Xna.Framework;

namespace DungeonCrawler.Core.Entities;

public class Spider : Entity
{
    public Entity Target { get; set; }

    public float Speed { get; set; } = 1.0f;

    private float _animTime;
    
    public Spider()
    {
        Health = MaxHealth = 10;
        Sprite = new Sprite
        {
            Position = Position,
            Entity = this,
            Texture = SpriteManager.GetTexture(Sprites.SpiderSheet),
        };
    }
    
    public override void Update(Level level, float deltaTime)
    {
        _animTime += deltaTime;
        var index = (int)(_animTime / Speed*2) % 4;
        Sprite.Position = Position;
        Sprite.SourceRectangle = new Rectangle(index * 256, 0, 256, 256);
        
        if (Target == null)
        {
            return;
        }
        
        var toTarget = Target.Position - Position;
        
        if (toTarget.LengthSquared > 1.0f)
        {
            var dir = toTarget.Normalized * Speed * deltaTime;
            Move(level, dir, false);
        }
        
        base.Update(level, deltaTime);
    }

    public override void OnDie()
    {
        Console.WriteLine("Spider just died lol");
        base.OnDie();
    }
}