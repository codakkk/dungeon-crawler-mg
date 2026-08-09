using DungeonCrawler.Core;
using DungeonCrawler.Core.Entities;

public class Projectile : Entity
{
    public double Lifetime = 3.0;
    public DamageInfo DamageInfo { get; set; }
    public Entity Owner { get; init; }
    
    

    public Projectile()
    {
        Radius = 0.1f;
        Health = MaxHealth = 1;
    }

    public override void Update(Level level, float dt)
    {
        Lifetime -= dt;
        if (Lifetime <= 0) 
        { 
            Health = 0; 
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
            if ((e.Position - next).LengthSquared < (Radius + e.Radius) * (Radius + e.Radius))
            {
                e.Damage(DamageInfo with { Direction = Velocity.Normalized });
                Health = 0;
                return;
            }
        }

        Position = next;
    }
}