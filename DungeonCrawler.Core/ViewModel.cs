using System;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.Maths;
using DungeonCrawler.Core.SoftwareRenderer;
using DungeonCrawler.Core.SoftwareRenderer.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonCrawler.Core;

public class ViewModel(Player currentPlayer)
{

    private double _bobPhase;
    private double _swayX, _swayY;

    private double _animTime;
    private double _attackAnimTime;
    

    public static Rectangle[] HandFireballTextures = [
        new (0, 0, 256, 256),
        new (256, 0, 256, 256),
        new (512, 0, 256, 256),
        new (768, 0, 256, 256),
    ];

    public static Rectangle[] HandTorchTextures = [
        new (0, 0, 256, 256),
        new (256, 0, 256, 256),
        new (512, 0, 256, 256),
        new (768, 0, 256, 256),
    ];

    public int OffsetX => (int)(Math.Sin(_bobPhase) * 4.0f + _swayX);
    
    public int OffsetY => (int)(Math.Abs(Math.Cos(_bobPhase)) * 3.0f);

    public void Update(double deltaTime)
    {
        var player = currentPlayer;
        
        _animTime +=  deltaTime;

        _attackAnimTime = player.AttackTime;
        _bobPhase += player.MovementSpeed * deltaTime * 0.8f;

        var k = 1.0 - Math.Exp(-10.0 * deltaTime);
        _swayX += (-player.Turn * 80.0f - _swayX) * k;
    }
    
    public void Render(RenderBuffer buffer, LightMap lightMap, Texture2D leftTexture, Texture2D rightTexture)
    {
        const int spacing = 30;
        var rightX = buffer.Width / 2 - spacing;
        var leftX = spacing;
        var screenY = buffer.Height / 2;
        
        var brightness = lightMap.Illumination(currentPlayer.Position, [currentPlayer.Torch]);

        var index = 1+(int)(_animTime / 0.5f) % 3;
        buffer.RenderSprite(leftX + OffsetX, screenY + OffsetY, leftTexture,  HandTorchTextures[index], shade: brightness);

        var rightIndex = (int)(_attackAnimTime / 0.3f) % 4;
        buffer.RenderSprite(rightX + OffsetX, screenY + OffsetY, rightTexture,  HandFireballTextures[rightIndex], shade: brightness);
    }
}