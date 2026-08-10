using System;
using DungeonCrawler.Core.Maths;
using DungeonCrawler.Core.SoftwareRenderer;
using Microsoft.Xna.Framework;

namespace DungeonCrawler.Core.Entities;

public readonly record struct DamageInfo(
    int Amount,
    float Stagger = 0.25f,
    float Knockback = 0.0f,
    Vec2 Direction = default);

public abstract class Entity
{
    protected virtual bool ApplyDefaultMovement => true;
    
    public Vec2 Position { get; set; }
    
    public Vec2 Velocity { get; set; }

    public Vec2 Knockback { get; set; }
    
    public int Health
    {
        get;
        set
        {
            field = value;

            if (field <= 0)
            {
                OnDie();
            }
        }
    }
    public int MaxHealth { get; set; }

    public bool IsAlive => Health > 0;
    
    public float Radius { get; set; } = 0.3f;

    public int TileX => (int)Position.X;
    public int TileZ => (int)Position.Z;

    /// <summary>
    ///  Death time is used by death animations
    ///  When an entity dies, IsAlive = false but DeathTime can be greater than 0 for animations
    ///  
    /// </summary>
    public float DeathTime { get; protected set; }
    public float FlashTime { get; protected set; }
    public float StaggerTime { get; protected set; }
    public float InvulnerableTime { get; set; }
    
    public float StaggerResistance { get; protected set; }
    
    public Sprite Sprite { get; set; }
    
    public virtual void Update(Level level, float deltaTime)
    {
        if (ApplyDefaultMovement)
        {
            var step = Velocity + Knockback;
            if (step.LengthSquared > 0.00001f)
                Move(level, step * deltaTime);
        }

        if (Knockback.LengthSquared > 0.00001f)
            Knockback *= MathF.Pow(0.02f, deltaTime);
        else
            Knockback = Vec2.Zero;
        
        if (DeathTime > 0.0f) DeathTime = Math.Max(0.0f, DeathTime - deltaTime * 4);
        if (FlashTime > 0.0f) FlashTime = Math.Max(0.0f, FlashTime - deltaTime);
        if (StaggerTime > 0.0f) StaggerTime = Math.Max(0.0f, StaggerTime - deltaTime);
        if (InvulnerableTime > 0.0f) InvulnerableTime = Math.Max(0.0f, InvulnerableTime - deltaTime);
    }

    public bool Move(Level level, Vec2 delta, bool canIgnoreWalls = false)
    {
        var didMove = false;
        
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

    public virtual void OnDie()
    {
        DeathTime = 1.0f;
    }
    
    public bool Damage(DamageInfo hit)
    {
        if (InvulnerableTime > 0.0f) return false;
        
        FlashTime = 0.25f;
        StaggerTime = Math.Max(StaggerTime, hit.Stagger * StaggerResistance);
        
        Knockback += hit.Direction * hit.Knockback;
        Health -= hit.Amount;

        return true;
    }
}