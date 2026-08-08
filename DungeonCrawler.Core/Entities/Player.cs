using System;
using System.Numerics;
using DungeonCrawler.Core.Maths;
using Microsoft.Xna.Framework.Input;

namespace DungeonCrawler.Core.Entities;

public class Player : Entity
{
    public static float FOV = 0.889f;
    
    public Vec2 Direction { get; set; }
    
    public Vec2 Plane { get; set; } = new(0f, FOV);

    public float PlaneHalfWidth { get; private set; }

    public float FieldOfViewInDegrees
    {
        get => 2.0f * MathF.Atan(PlaneHalfWidth) * MathUtils.RadToDeg;
        set
        {
            PlaneHalfWidth = MathF.Tan(value * 0.5f * MathUtils.DegToRad);
            Plane = Direction.Perpendicular * PlaneHalfWidth;
        }
    }
    
    public float CurrentSpeed { get; set; }
    public float MovementSpeed { get; set; } = 5.0f;
    public float RunMultiplier { get; set; } = 1.1f;

    public float TurnSpeed { get; set; } = 5.0f;
    public float Turn { get; set; }
   
    private InputHandler _inputHandler;
    
    public Player(InputHandler inputHandler)
    {
        _inputHandler = inputHandler;
        Radius = 0.2f;
        Health = MaxHealth = 10;
    }
    
    public override void Update(Level level, float deltaTime)
    {
        UpdateMovement(level, deltaTime);

        if(_inputHandler.IsKeyJustPressed(Keys.Space))
        {
            level.Spawn(new Projectile
            {
                Position = Position + Direction * (Radius + 0.5f),
                Velocity = Direction * 10f,
                DamageInfo = new DamageInfo(1, 0.0f, 0.5f, Direction),
                Owner = this
            });
        }
        
        base.Update(level, deltaTime);
    }

    private void UpdateMovement(Level level, float deltaTime)
    {   
        bool isRunning = _inputHandler.IsKeyDown(Keys.LeftShift);
        float speed = (isRunning ? RunMultiplier : 1f) * MovementSpeed;
        
        var forward = _inputHandler.Forward;
        var lateral = _inputHandler.Lateral;

        var prevPosition = Position;

        var delta = Direction * (forward > 0 ? forward : forward * 0.6f) + Direction.Perpendicular * -lateral * 0.75f;
        if (delta.LengthSquared > 1.0f)
        {
            delta = delta.Normalized;
        }
        
        Move(level, delta * speed * deltaTime, false);

        CurrentSpeed = deltaTime > 0 ? (Position - prevPosition).Length / deltaTime : 0;
        
        // var rotDir = _inputHandler.IsKeyDown(Keys.A) ? 1 : _inputHandler.IsKeyDown(Keys.D) ? -1 : 0;
        // Turn = rotDir * rotationSpeed;
        Turn = _inputHandler.MouseTurn;

        if (Turn != 0)
        {
            // rot matrix should be: 
            // [Cos, Sin]
            // [Sin, Cos]
            // can probably be rewritten as: [rotMatrix] * [dirX, dirY] (column vector)
            
            Direction = Direction.Rotated(Turn).Normalized;
            Plane = Direction.Perpendicular * PlaneHalfWidth;
        }
    }

    public void UpdateAspect(int bufferWidth, int bufferHeight)
    {
        PlaneHalfWidth = bufferWidth / (2f * bufferHeight);
        Plane = Direction.Perpendicular * PlaneHalfWidth;
    }
}