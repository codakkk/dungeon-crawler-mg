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
        const int cellSize = 256;
        
        var rightX = buffer.Width / 2 - spacing;
        var leftX = spacing;
        var screenY = buffer.Height / 2;
        
        var brightness = lightMap.Illumination(currentPlayer.Position, [currentPlayer.Torch]);

        var leftHandFrameCount = leftTexture.Width / 256 - 1;
        var leftIndex = 0;// 1+(int)(_animTime / 0.1f) % leftHandFrameCount;
        buffer.RenderSprite(leftX + OffsetX, screenY + OffsetY, leftTexture, new Rectangle(leftIndex * cellSize, 0, cellSize, cellSize), shade: brightness);

        var rightHandFrameCount = rightTexture.Width / 256;
        var rightIndex = (_attackAnimTime > 0.0f ? (int)(_attackAnimTime / 0.6f) : (int)(_animTime / 0.1f))% rightHandFrameCount;
        buffer.RenderSprite(rightX + OffsetX, screenY + OffsetY, rightTexture,  new Rectangle(rightIndex * cellSize, 0, cellSize, cellSize), shade: brightness);
    }
}