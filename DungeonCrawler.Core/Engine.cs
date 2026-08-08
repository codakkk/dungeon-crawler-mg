using System;
using System.Collections.Generic;
using DungeonCrawler.Core.Entities;
using DungeonCrawler.Core.SoftwareRenderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DungeonCrawler.Core;

public class Engine : Game
{

    public readonly GraphicsDeviceManager Graphics;

    public readonly SpriteBatch SpriteBatch;

    public const int TargetWidth = 640;
    public const int TargetHeight = 480;
    public static int AmbientLevel = 20;
    
    private Level _currentLevel;
    private Player _currentPlayer;
    private Entity _spider;
    private ViewModel _viewModel;
    
    private RenderBuffer _renderBuffer;
    private EasyRenderer _renderer;
    private BillboardRenderer _billboardRenderer;
    private HealthBarRenderer _healthBarRenderer;
    private LightMap _lightMap;
    
    private bool _isResizing;

    private readonly int[,] _worldMap = new int[24,24]
    {
        {1,1,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,1,1,1,1,0,0,0,0,1,0,1,0,1,0,0,0,1},
        {1,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1},
        {1,0,0,0,0,0,1,0,0,0,1,0,0,0,0,1,0,0,0,1,0,0,0,1},
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
        IsMouseVisible = false;

        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    protected override void LoadContent()
    {
        SpriteManager.Initialize(Content);

        _renderBuffer = new RenderBuffer(GraphicsDevice, TargetWidth, TargetHeight);
        _renderer = new EasyRenderer(_renderBuffer, 0x000000);
        
        _billboardRenderer = new BillboardRenderer();
        _healthBarRenderer = new HealthBarRenderer(_billboardRenderer);
        
        _currentLevel = new Level(24, 24);
        _currentLevel.Set(_worldMap);
        
        _currentPlayer = new Player
        {
            Position = new Vec2(10, 5),
            Direction = new Vec2(-1, 0),
        };
        _currentLevel.Spawn(_currentPlayer);

        _spider = new Spider
        {
            Position = new Vec2(10, 2),
            Target = _currentPlayer,
        };
        _currentLevel.Spawn(_spider);

        _viewModel = new ViewModel(_currentPlayer);
        
        _lightMap.Bake(_currentLevel, [
        new Light
        {
            Position = new Vec2(5, 2),
            Intensity = 1.0f,
            Radius = 2.0f,
        }]);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
            Exit();
        
        double deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var player = _currentPlayer;

        if (keyboardState.IsKeyDown(Keys.F1))
        {
            var damageInfo = new DamageInfo(1, 0.25, 0.5f, player.Direction);
            _spider.Damage(damageInfo);
        }
        
        if (keyboardState.IsKeyDown(Keys.F))
        {
            var damageInfo = new DamageInfo(1, 0.25, 0.5f, -player.Direction);
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
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _renderer.Render(_currentLevel, _currentPlayer);

        List<Sprite> sprites = [];
        for(int i = 0; i < _currentLevel.Entities.Count; ++i)
        {
            var entity = _currentLevel.Entities[i];

            if(entity.Health <= 0) continue;
            
            var texture = entity is Projectile ? SpriteManager.Get(Sprites.FireballSheet).Texture : SpriteManager.Get(Sprites.SpiderSheet).Texture;
            sprites.Add(new Sprite
            {
                Position = entity.Position,
                Texture = texture,
                SourceRectangle = new Rectangle(0, 0, 256, 256),
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
        
        base.Draw(gameTime);
    }
    
    private void OnClientSizeChanged(object sender, EventArgs e)
    {
        if (_isResizing) return;
        
        _isResizing = true;
        
        Graphics.PreferredBackBufferWidth = Math.Max(1, Window.ClientBounds.Width);
        Graphics.PreferredBackBufferHeight = Math.Max(1, Window.ClientBounds.Height);
        Graphics.ApplyChanges();
        
        _isResizing = false;
    }
}