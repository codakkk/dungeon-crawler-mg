using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonCrawler.Core;

public struct SpriteData
{
    public int Width { get; set; }
    
    public int Height { get; set; }
    
    public Texture2D Texture { get; set; }
    
    public uint[] Data { get; set; }
}

public static class Sprites
{
    public const string WorldMap = "wolftextures";
    public const string SpiderSheet = "spider_sheet";
    public const string KnifeHandSheet = "knife_hand_sheet";
    
    public const string HandTorchSheet = "hand_torch_sheet";

    public const string FireballSheet = "fireball_sheet";
    public const string TorchSheet = "torch_sheet";
    
    public const string HandFireballSheet = "hand_fireball_sheet";
    public const string HandFireballIdleSheet = "hand_fireball_idle_sheet";
}

public static class SpriteManager
{
    private static readonly Dictionary<string, SpriteData> Sprites = [];

    public static void Initialize(ContentManager content)
    {
        RegisterFromContent(content, Core.Sprites.WorldMap);
        RegisterFromContent(content, Core.Sprites.SpiderSheet);
        RegisterFromContent(content, Core.Sprites.KnifeHandSheet);
        RegisterFromContent(content, Core.Sprites.FireballSheet);
        RegisterFromContent(content, Core.Sprites.TorchSheet);
        RegisterFromContent(content, Core.Sprites.HandTorchSheet);
        RegisterFromContent(content, Core.Sprites.HandFireballSheet);
        RegisterFromContent(content, Core.Sprites.HandFireballIdleSheet);
    }

    public static void RegisterFromContent(ContentManager content, string name)
    {
        var texture = content.Load<Texture2D>(name);
        Register(name, texture);
    }
    
    public static void Register(string name, Texture2D texture)
    {
        if (Sprites.ContainsKey(name))
        {
            throw new Exception($"Sprite {name} has been registered twice. (Atleast you tried lol)");
        }
        
        var pixel = new uint[texture.Width * texture.Height];
        texture.GetData(pixel);
        
        Sprites.Add(name, new SpriteData
        {
            Texture =  texture,
            Data = pixel,
            Width = texture.Width,
            Height = texture.Height,
        });
    }
    
    public static SpriteData Get(string name)
    {
        return Sprites[name];
    }
    
    public static Texture2D GetTexture(string name)
    {
        return Sprites[name].Texture;
    }
}