using System;

namespace DungeonCrawler.Core.Entities;

public class Spider : Entity
{
    public Entity Target { get; set; }

    public double Speed { get; set; } = 1.0f;

    public Spider()
    {
        Health = MaxHealth = 10;
    }
    
    public override void Update(Level level, double deltaTime)
    {
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