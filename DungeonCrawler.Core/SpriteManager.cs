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
    public const string WorldMap = "world_map";
    public const string SpiderSheet = "spider";
    public const string KnifeHandSheet = "knife_hand";
}

public static class SpriteManager
{
    private static readonly Dictionary<string, SpriteData> Sprites = [];

    public static void Initialize(ContentManager content)
    {
        RegisterFromContent(content, Core.Sprites.WorldMap, "wolftextures");
        RegisterFromContent(content, Core.Sprites.SpiderSheet, "spider_sheet");
        RegisterFromContent(content, Core.Sprites.KnifeHandSheet, "knife_hand_sheet");
    }

    public static void RegisterFromContent(ContentManager content, string name, string resourceName)
    {
        var texture = content.Load<Texture2D>(resourceName);
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