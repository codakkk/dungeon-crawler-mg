using System;

namespace DungeonCrawler.Core.Entities;

public sealed class Torch : Entity
{
    private const int FrameCount = 4;
    
    private static readonly int[] TorchHold = [120, 90, 145, 80];
    
    private double _animTime;
    private double _randomRate;
    
    public int AnimIndex { get; private set; }
    
    public Torch()
    {
        Health = MaxHealth = 9999;
        InvulnerableTime = float.PositiveInfinity;
        
        var rng = new Random(HashCode.Combine(Position.X, Position.Z));
        _randomRate = 0.9f + (float)rng.NextDouble() * 0.2f;
        AnimIndex = rng.Next(FrameCount);
    }
    
    public override void Update(Level level, float deltaTime)
    {
        _animTime += deltaTime * 1000.0f * _randomRate;
        
        while (_animTime >= TorchHold[AnimIndex]) {
            _animTime -= TorchHold[AnimIndex];
            AnimIndex = (AnimIndex + 1) % FrameCount;
        }
        
        // AnimIndex = (int) ((_animTime / AnimationDuration) % FrameCount);
        
        base.Update(level, deltaTime);
    }
}