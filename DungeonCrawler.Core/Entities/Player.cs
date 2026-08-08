using System;
using System.Numerics;
using DungeonCrawler.Core.Maths;
using Microsoft.Xna.Framework.Input;

namespace DungeonCrawler.Core.Entities;

public class Player : Entity
{
    public Vec2 Direction { get; set; }
    
    public Vec2 Plane { get; set; } = new(0, 0.66);

    private double _planeHalfWidth = 0.66; // FOV 66

    public double FieldOfViewInDegrees
    {
        get => 2.0 * Math.Atan(_planeHalfWidth) * MathUtils.RadToDeg;
        set
        {
            _planeHalfWidth = Math.Tan(value * 0.5 * MathUtils.DegToRad);
            Plane = Direction.Perpendicular * _planeHalfWidth;
        }
    }
    
    public double CurrentSpeed { get; set; }
    public double MovementSpeed { get; set; } = 5.0f;
    public double RunMultiplier { get; set; } = 1.1f;

    public double TurnSpeed { get; set; } = 5.0f;
    public double Turn { get; set; }

    public int LightRadius { get; set; } = 0;
    
    public Player()
    {
        Radius = 0.5;
        Health = MaxHealth = 10;
    }
    
    public override void Update(Level level, double deltaTime)
    {
        var keyboardState = Keyboard.GetState();
        
        UpdateMovement(level, keyboardState, deltaTime);

        if(keyboardState.IsKeyDown(Keys.Space))
        {
            level.Spawn(new Projectile
            {
                Position = Position + Direction * (Radius + 0.1),
                Velocity = Direction * 10,
                DamageInfo = new DamageInfo(1, 0.0f, 0.5f, Direction),
                Owner = this
            });
        }

        if (keyboardState.IsKeyDown(Keys.T))
        {
            LightRadius++;
        }
        
        if (keyboardState.IsKeyDown(Keys.Y))
        {
            Engine.AmbientLevel++;
        }
        
        base.Update(level, deltaTime);
    }

    private void UpdateMovement(Level level, KeyboardState keyboardState, double deltaTime)
    {   
        bool isRunning = keyboardState.IsKeyDown(Keys.LeftShift);
        double speed = (isRunning ? RunMultiplier : 1f) * MovementSpeed *  deltaTime;
        double rotationSpeed = TurnSpeed * deltaTime;
        
        var forward = keyboardState.IsKeyDown(Keys.W) ? 1 : keyboardState.IsKeyDown(Keys.S) ? -1 : 0;

        var prevPosition = Position;
        
        Move(level, Direction * (forward * speed), false);

        CurrentSpeed = deltaTime > 0 ? (Position - prevPosition).Length / deltaTime : 0;
        
        var rotDir = keyboardState.IsKeyDown(Keys.A) ? 1 : keyboardState.IsKeyDown(Keys.D) ? -1 : 0;
        Turn = rotDir * rotationSpeed;

        if (Turn != 0)
        {
            // rot matrix should be: 
            // [Cos, Sin]
            // [Sin, Cos]
            // can probably be rewritten as: [rotMatrix] * [dirX, dirY] (column vector)
            
            Direction = Direction.Rotated(Turn).Normalized;
            Plane = Direction.Perpendicular * _planeHalfWidth;
        }
    }
}