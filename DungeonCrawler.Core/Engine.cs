using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.ImmediateGui;
using DungeonCrawler.Core.Maths;
using DungeonCrawler.Core.SoftwareRenderer;
using DungeonCrawler.Core.SoftwareRenderer.Lights;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DungeonCrawler.Core;

public class Engine : Game
{

    public readonly GraphicsDeviceManager Graphics;

    public readonly SpriteBatch SpriteBatch;

    /// <summary>  
    /// Gets the ImGui renderer used for debug UIs.  
    /// </summary>  
    public static ImGuiRenderer ImGuiRenderer { get; private set; }
    
    public const int TargetWidth = 640;
    public const int TargetHeight = 480;
    
    private Level _currentLevel;
    private Player _currentPlayer;
    private Entity _spider;
    private ViewModel _viewModel;
    
    private RenderBuffer _renderBuffer;
    private EasyRenderer _renderer;
    private BillboardRenderer _billboardRenderer;
    private HealthBarRenderer _healthBarRenderer;
    
    private InputHandler _inputHandler;
    
    private bool _wasMouseVisible = false;
    
    private LightMap _lightMap;
    
    private List<Light> _staticLights = [];
    
    private bool _isResizing;

    private readonly int[,] _worldMap = new int[24,24]
    {
        {1,1,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,1,1,1,1,0,0,0,0,1,0,1,0,1,0,0,0,1},
        {1,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,0,6,6,1,0,0,0,0,1,0,0,0,1,0,0,0,1},
        {1,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,1,0,1,1,0,0,0,0,1,0,1,0,1,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,0,1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,0,0,0,0,5,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,0,1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,0,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,2,2,2,2,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
    };
    
    public Engine()
    {
        Window.AllowUserResizing = true;
        
        Graphics = new GraphicsDeviceManager(this);
        Graphics.PreferredBackBufferWidth = TargetWidth * 2;
        Graphics.PreferredBackBufferHeight = TargetHeight * 2;
        
        Graphics.ApplyChanges();
        
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        
        Content.RootDirectory = "Content";
        IsMouseVisible = _wasMouseVisible;

        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _inputHandler = new InputHandler(Window);
        
        _renderBuffer = new RenderBuffer(GraphicsDevice, TargetWidth, TargetHeight);
        _renderer = new EasyRenderer(_renderBuffer, 0x000000);
        
        _billboardRenderer = new BillboardRenderer();
        _healthBarRenderer = new HealthBarRenderer(_billboardRenderer);

        
        _currentLevel = new Level(24, 24);
        _currentLevel.Set(_worldMap);
        
        _currentPlayer = new Player(_inputHandler)
        {
            Position = new Vec2(10, 5),
            Direction = new Vec2(-1, 0),
        };
        _viewModel = new ViewModel(_currentPlayer);
        _currentLevel.Spawn(_currentPlayer);
        _spider = new Spider
        {
            Position = new Vec2(10, 2),
            Target = _currentPlayer,
        };
        _currentLevel.Spawn(_spider);

        for (int i = 0; i < _currentLevel.Entities.Count; ++i)
        {
            var entity = _currentLevel.Entities[i];
            
            if (entity is not Torch) continue;
            
            _staticLights.Add(new Light
            {
                Enabled = true,
                Position = entity.Position,
                Color = LightColor.Torch,
                Intensity = 1.0f,
                Radius = 1.0f,
            });
        }
        
        _lightMap = new LightMap();
        _lightMap.Bake(_currentLevel, _staticLights);
        
        // Create the ImGui renderer.
        ImGuiRenderer = new ImGuiRenderer(this);
        ImGuiRenderer.RebuildFontAtlas();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        SpriteManager.Initialize(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        var keyboardState = Keyboard.GetState();
        
        
        if (_inputHandler.IsKeyJustPressed(Keys.Escape))
        {
            IsMouseVisible = !_wasMouseVisible;
        }

        _wasMouseVisible = IsMouseVisible;
        

        var player = _currentPlayer;

        if (keyboardState.IsKeyDown(Keys.F1))
        {
            var damageInfo = new DamageInfo(1, 0.25f, 0.5f, player.Direction);
            _spider.Damage(damageInfo);
        }
        
        if (keyboardState.IsKeyDown(Keys.F))
        {
            var damageInfo = new DamageInfo(1, 0.25f, 0.5f, -player.Direction);
            _currentPlayer.Damage(damageInfo);
        }
        
        for (int i = _currentLevel.Entities.Count-1; i >= 0; --i)
        {
            var entity = _currentLevel.Entities[i];
            if (entity.Health <= 0)
            {
                _currentLevel.Remove(entity);
                continue;
            }
            _currentLevel.Entities[i].Update(_currentLevel, deltaTime);
        }
        
        _viewModel.Update(deltaTime);

        if (IsActive)
        {
            _inputHandler.Update(deltaTime);
            IsMouseVisible = _inputHandler.IsMouseVisible;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _renderer.Render(_currentLevel, _currentPlayer, _lightMap);

        List<Sprite> sprites = [];
        for(int i = 0; i < _currentLevel.Entities.Count; ++i)
        {
            var entity = _currentLevel.Entities[i];

            if(entity is Player || entity.Health <= 0) continue;
            
            // we have to find a better way soon lol
            var texture = entity switch
            {
                Projectile => SpriteManager.Get(Sprites.FireballSheet).Texture,
                Spider => SpriteManager.Get(Sprites.SpiderSheet).Texture,
                Torch => SpriteManager.Get(Sprites.TorchSheet).Texture,
            };

            var index = entity is Torch torch ? torch.AnimIndex : 0; 
            
            sprites.Add(new Sprite
            {
                Position = entity.Position,
                Texture = texture,
                SourceRectangle = new Rectangle(index * 256, 0, 256, 256),
                Entity = entity,
            });
        }

        _billboardRenderer.Render(_renderBuffer, _currentPlayer, sprites, _renderer.WallDepth);
        
        _healthBarRenderer.Render(_renderBuffer, _renderer.WallDepth);
        
        _viewModel.Render(_renderBuffer, SpriteManager.GetTexture(Sprites.KnifeHandSheet));
        
        SpriteBatch.Begin(
            SpriteSortMode.Deferred, 
            BlendState.Opaque, 
            SamplerState.PointClamp, 
            DepthStencilState.None, 
            RasterizerState.CullNone // We handle culling in software mode
        );
        _renderBuffer.Render(SpriteBatch);
        
        SpriteBatch.End();
        
        // Draw debug UI
        ImGuiRenderer.BeforeLayout(gameTime);
        Player_DebugUi();
        Lights_DebugUi();
        ImGuiRenderer.AfterLayout();
        
        base.Draw(gameTime);
    }
    
    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        if (_isResizing) return;
        
        _isResizing = true;
        
        Graphics.PreferredBackBufferWidth = Math.Max(1, Window.ClientBounds.Width);
        Graphics.PreferredBackBufferHeight = Math.Max(1, Window.ClientBounds.Height);
        Graphics.ApplyChanges();
        
        _currentPlayer?.UpdateAspect(Window.ClientBounds.Width, Window.ClientBounds.Height);
        
        _isResizing = false;
    }

    private int _selected = -1;

    private void Player_DebugUi()
    {
        ImGui.BeginGroup();
        
        var player = _currentPlayer;
        var health = player.Health;
        var maxHealth = player.MaxHealth;

        if (ImGui.DragInt("Health", ref health, 1, maxHealth))
        {
            player.Health = health;
        }
        
        if (ImGui.DragInt("Max Health", ref maxHealth, 1, maxHealth))
        {
            player.MaxHealth = maxHealth;
        }
        
        var invulnerableTime = player.InvulnerableTime;
        var invincible = invulnerableTime > 0.0f;
        if (ImGui.Checkbox("Invincible", ref invincible))
        {
            player.InvulnerableTime = invincible ? float.PositiveInfinity : 0.0f;
        }
        
        var radius = player.Radius;
        if (ImGui.DragFloat("Radius", ref radius, 0.1f, 0.01f))
        {
            player.Radius = radius;
        }
        ImGui.Text($"Position: {player.Position} - Tile: ({player.TileX}, {player.TileZ}) ");
        ImGui.Text($"Velocity: {player.Velocity}");
        ImGui.Text($"FOV: {player.Plane.Z} - Half: {player.PlaneHalfWidth}");
        
        ImGui.EndGroup();   
    }
    
    [Conditional("DEBUG")]
    private void Lights_DebugUi()
    {
        var lights = _staticLights;
        ImGui.Begin("Lights");

        var applyDithering = EasyRenderer.ApplyDithering;
        if (ImGui.Checkbox("Apply Dithering", ref applyDithering))
        {
            EasyRenderer.ApplyDithering = applyDithering;
        }
        
        
        
        // ---- Left column: list + footer button ----
        ImGui.BeginGroup();
        float footer = ImGui.GetFrameHeightWithSpacing();       // button height + item spacing
        
        int toDelete = -1;
        ImGui.BeginChild("Lights", new System.Numerics.Vector2(180, -footer), ImGuiChildFlags.Borders);
        for (int i = 0; i < lights.Count; ++i)
        {
            ImGui.PushID(i);
            if (ImGui.Selectable($"Light {i}", _selected == i)) _selected = i;
            ImGui.PopID();
        }
        ImGui.EndChild();
        if (ImGui.Button("Bake", new Vector2(180, 0).ToNumerics()))
        {
            _lightMap.Bake(_currentLevel, _staticLights);;
        }
        ImGui.EndGroup();
        
        ImGui.SameLine();
        
        // ---- Right column: inspector ----
        ImGui.BeginChild("inspector", Vector2.Zero.ToNumerics());
        Lights_Inspector(lights);
        ImGui.EndChild();
        
        ImGui.End();

        // deferred mutation, after the loop
        if (toDelete >= 0)
        {
            _staticLights.RemoveAt(toDelete);
            if (_selected >= _staticLights.Count) _selected = _staticLights.Count - 1;
        }
    }

    private void Lights_Inspector(List<Light> lights)
    {
        var span = CollectionsMarshal.AsSpan(lights);
        if (_selected < 0 || _selected >= lights.Count)
        {
            ImGui.TextDisabled("No light selected");
            return;
        }
        
        ref var light = ref span[_selected];
        if (Lights_DrawItem(ref light))
        {
            lights[_selected] = light;
        }
    }
    
    private bool Lights_DrawItem(ref Light light)
    {
        var enabled = light.Enabled;
        var position = light.Position.ToNumerics();
        var intensity = light.Intensity;
        var radius = light.Radius;
        var color = light.Color.ToNumerics();
        
        bool changed = false;
        changed |= ImGui.Checkbox("Enabled", ref enabled);
        changed |= ImGui.DragFloat2("Position", ref position, 0.05f);
        changed |= ImGui.DragFloat3("Color", ref color, 0.05f);
        changed |= ImGui.DragFloat("Intensity", ref intensity, 0.1f, 0f, 100f);
        changed |= ImGui.DragFloat("Radius", ref radius, 0.1f, 0f, 100f);
        
        if (changed)
        {
            light = new Light
            {
                Enabled = enabled,
                Position = new Vec2(position.X, position.Y),
                Color = LightColor.FromNumerics(color),
                Intensity = intensity,
                Radius = radius
            };
        }

        if (ImGui.Button("Teleport To"))
        {
            _currentPlayer.Position = new Vec2(position.X, position.Y);
        }
        
        return changed;
    }

}