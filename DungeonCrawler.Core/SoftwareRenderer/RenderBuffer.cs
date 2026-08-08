using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using DungeonCrawler.Core.SoftwareRenderer.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonCrawler.Core.SoftwareRenderer;

public class RenderBuffer : IDisposable
{
    public int Width { get; }
    
    public int Height { get; }

    private readonly uint[] _pixels;

    private GraphicsDevice _graphicsDevice;
    
    private readonly Texture2D _target;

    private readonly Dictionary<Texture2D, uint[]> _textureColors = [];
    
    public RenderBuffer(GraphicsDevice graphicsDevice, int width, int height)
    {
        _graphicsDevice = graphicsDevice;
        
        Width = width;
        Height = height;
        
        _pixels = new uint[width * height];
        
        _target = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
    }
    
    public static uint Rgba(uint rgba) => BinaryPrimitives.ReverseEndianness(rgba);
    public static uint Rgb(uint rgb) => BinaryPrimitives.ReverseEndianness(rgb << 8) | 0xFF000000u;

    public static uint PackColor(byte r, byte g, byte b, byte a)
    {
        /// 0x AA RR GG BB
        return (uint) (r | (g << 8) | (b << 16) | (a << 24));
    }

    public void Clear(uint color)
    {
        _pixels.AsSpan().Fill(color);
    }

    public void SetPixel(int x, int y, uint color)
    {
        _pixels[x + y * Width] = color;
    }

    public void RenderSprite(int x, int y, Texture2D texture, Rectangle source, uint color = 0xFFFFFF)
    {
        int texWidth = texture.Width;
        int texHeight = texture.Height;
        
        // slow must cache soon
        if (!_textureColors.TryGetValue(texture, out uint[] colors))
        {
            colors = new uint[texWidth * texHeight];
            _textureColors.Add(texture, colors);
            texture.GetData(colors);
        }

        int dx = x;
        for (int xx = source.X; xx < source.X + source.Width; xx++)
        {
            int dy = y;
            for (int yy = source.Y; yy < source.Y + source.Height; yy++)
            {
                if (dx < 0 || dx >= Width || dy < 0 || dy >= Height) continue;
                
                var currColor = colors[xx + yy * texWidth];
                if (currColor >> 24 > 128)
                {
                    _pixels[dx + dy * Width] = currColor;
                }

                dy++;
            }
            dx++;
        }
    }
    
    public void Render(SpriteBatch spriteBatch)
    {
        _target.SetData(_pixels);

        var rect = FitRect(Engine.TargetWidth, Engine.TargetHeight, _graphicsDevice.Viewport.Bounds, false);
        spriteBatch.Draw(_target, rect, Color.White);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        _target?.Dispose();
    }
    
    private static Rectangle FitRect(int srcWidth, int srcHeight, Rectangle dest, bool integerScale)
    {
        int width, height;

        if (integerScale)
        {
            var scale = Math.Max(1, Math.Min(dest.Width / (float)srcWidth, dest.Height / (float)srcHeight));
            width = (int)(dest.Width * scale);
            height = (int)(dest.Height * scale);
        }
        else
        {
            var scale = Math.Min(1, Math.Min(dest.Width / (float)srcWidth, dest.Height / (float)srcHeight));
            width = (int)(dest.Width * scale);
            height = (int)(dest.Height * scale);
        }

        var widthDiff = (dest.Width - width) / 2;
        var heightDiff = (dest.Height - height) / 2;

        return new Rectangle(dest.X + widthDiff, dest.Y + heightDiff, width, height);
    }

    public static int FogAmount(double depth, double fogStart = 3.0f, double fogEnd = 14.0f)
    {
        double t = (depth - fogStart) / (fogEnd - fogStart);
        return (int)(Math.Clamp(t, 0.0f, 1.0f) * 256.0f);
    }

    public static uint Blend(uint src, uint dst, int amount)
    {
        int inv = 256 - amount;

        uint rb = ((src & 0x00FF00FFu) * (uint)inv + (dst & 0x00FF00FFu) * (uint)amount) >> 8;
        uint g = ((src & 0x0000FF00u) * (uint)inv + (dst & 0x0000FF00u) * (uint)amount) >> 8;
        
        return (rb & 0x00FF00FFu) | (g & 0x0000FF00u) | 0xFF000000u;
    }
    
    /// <summary>Multiplies a packed color by a 0..256 brightness.</summary>
    public static uint Shade(uint packed, uint brightness)
    {
        var r = (packed & 0xFFu) * (uint)brightness >> 8;
        var g  = (packed >> 8 & 0xFFu) * (uint)brightness >> 8;
        var b = (packed >> 16 & 0xFFu) * (uint)brightness >> 8; 
        return r | (g << 8) | (b << 16) | (packed & 0xFF000000u);
    }
    
    /// <summary>Multiplies a packed color by a 0..256 brightness.</summary>
    public static uint Shade(uint packed, in Shade3 light)
    {
        var r = (packed & 0xFFu) * (uint)light.R >> 8;
        var g  = (packed >> 8 & 0xFFu) * (uint)light.G >> 8;
        var b = (packed >> 16 & 0xFFu) * (uint)light.B >> 8; 
        return r | (g << 8) | (b << 16) | (packed & 0xFF000000u);
    }
}