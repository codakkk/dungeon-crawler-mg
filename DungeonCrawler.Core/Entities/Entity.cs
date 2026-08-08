using System;
using DungeonCrawler.Core.Maths;
using Microsoft.Xna.Framework;

namespace DungeonCrawler.Core.Entities;

public readonly record struct DamageInfo(
    int Amount,
    float Stagger = 0.25f,
    float Knockback = 0.0f,
    Vec2 Direction = default);

public abstract class Entity
{
    public Vec2 Position { get; set; }
    
    public Vec2 Velocity { get; set; }
    
    public int Health { get; set; }
    public int MaxHealth { get; set; }

    public bool IsAlive => Health > 0;
    
    public float Radius { get; set; } = 0.3f;

    public int TileX => (int)Position.X;
    public int TileZ => (int)Position.Z;

    public float FlashTime { get; protected set; }
    public float StaggerTime { get; protected set; }
    public float InvulnerableTime { get; set; }
    
    public float StaggerResistance { get; protected set; }
    
    public virtual void Update(Level level, float deltaTime)
    {
        if (Velocity.LengthSquared > 0.00001)
        {
            Move(level, Velocity * deltaTime);
            Velocity *= MathF.Pow(0.02f, deltaTime);
        }
        else Velocity = Vec2.Zero;
        
        if (FlashTime > 0.0f) FlashTime = Math.Max(0.0f, FlashTime - deltaTime);
        if (StaggerTime > 0.0f) StaggerTime = Math.Max(0.0f, StaggerTime - deltaTime);
        if (InvulnerableTime > 0.0f) InvulnerableTime = Math.Max(0.0f, InvulnerableTime - deltaTime);
    }

    public bool Move(Level level, Vec2 delta, bool canIgnoreWalls = false)
    {
        bool didMove = false;
        
        if (canIgnoreWalls)
        {
            Position += delta;
            return true;
        }

        var fx = level.IsBlocked(new Vec2(Position.X + delta.X, Position.Z), Radius); 
        if (fx == false)
        {
            Position = Position with { X = Position.X + delta.X };
            didMove = true;
        }
        
        fx = level.IsBlocked(new Vec2(Position.X, Position.Z + delta.Z), Radius); 
        if (fx == false)
        {
            Position = Position with { Z = Position.Z + delta.Z };
            didMove = true;
        }

        return didMove;
    }

    public virtual void OnDie() {}
    
    public bool Damage(DamageInfo hit)
    {
        if (InvulnerableTime > 0.0f) return false;
        
        Health -= hit.Amount;
        FlashTime = 0.25f;
        StaggerTime = Math.Max(StaggerTime, hit.Stagger * StaggerResistance);
        
        Velocity += hit.Direction * hit.Knockback;
        
        if (Health < 0)
        {
            OnDie();
        }

        return true;
    }
}