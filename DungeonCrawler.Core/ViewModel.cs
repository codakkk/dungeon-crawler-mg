using System;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.SoftwareRenderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonCrawler.Core;

public class ViewModel
{
    private Player _currentPlayer;

    private double _bobPhase;
    private double _swayX, _swayY;

    private double _animTime;
    private int _currAnim;
    
    

    public static Rectangle[] _handTextures = [
        new (0, 0, 256, 256),
        new (256, 0, 256, 256),
        new (512, 0, 256, 256),
        new (768, 0, 256, 256),
    ];
    
    public int OffsetX => (int)(Math.Sin(_bobPhase) * 4.0f + _swayX);
    
    public int OffsetY => (int)(Math.Abs(Math.Cos(_bobPhase)) * 3.0f);
    
    public ViewModel(Player player)
    {
        _currentPlayer = player;
    }

    public void Update(double deltaTime)
    {
        var player = _currentPlayer;
        
        _bobPhase += player.MovementSpeed * deltaTime * 0.8f;

        double k = 1.0 - Math.Exp(-10.0 * deltaTime);
        _swayX += (-player.Turn * 80.0f - _swayX) * k;
    }
    
    public void Render(RenderBuffer buffer, Texture2D texture)
    {
        var screenX = Engine.TargetWidth / 2 - 90;
        var screenY = Engine.TargetHeight / 2;
        
        buffer.RenderSprite(screenX + OffsetX, screenY + OffsetY, texture,  _handTextures[0]);        
    }
}