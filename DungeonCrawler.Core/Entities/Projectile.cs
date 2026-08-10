using System;
using DungeonCrawler.Core;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.SoftwareRenderer;
using Microsoft.Xna.Framework;

public class Projectile : Entity
{
    protected override bool ApplyDefaultMovement => false;
    public DamageInfo DamageInfo { get; set; }
    public Entity Owner { get; init; }

    public int AnimIndex { get; private set; }
    
    private float _animTime;

    public Projectile()
    {
        Radius = 0.1f;
        Health = MaxHealth = 1;
        Sprite = new Sprite
        {
            Entity = this,
            Position = Position,
            Texture = SpriteManager.GetTexture(Sprites.FireballSheet),
            SourceRectangle = new Rectangle(0, 0, 256, 256),
        };
    }

    public override void OnDie()
    {
        base.OnDie();
        _animTime = 0.0f;
        AnimIndex = 0;
    }

    public override void Update(Level level, float dt)
    {
        base.Update(level, dt);
        
        _animTime += dt;

        Sprite.Position = Position;
        AnimIndex = (int)((IsAlive ? _animTime / 0.1f : (1-DeathTime) / 0.25) ) % 4;

        Sprite.SourceRectangle = new Rectangle(AnimIndex * 256, IsAlive ? 0 : 256, 256, 256);
        
        if (IsAlive == false)
        {
            return;
        }

        var next = Position + Velocity * dt;

        if (level.IsBlocked(next, Radius)) 
        { 
            Health = 0;
            return; 
        }

        foreach (var e in level.Entities)
        {
            if (e == Owner || e is Projectile) continue;
            
            var distance = (e.Position - next).LengthSquared; 
            if (distance >= (Radius + e.Radius) * (Radius + e.Radius))
            {
                continue;
            }
            
            e.Damage(DamageInfo with { Direction = Velocity.Normalized });
            Health = 0;
            return;
        }

        Position = next;
    }
}