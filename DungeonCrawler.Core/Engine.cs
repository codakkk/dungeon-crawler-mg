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

    private Level _currentLevel;
    private Player _currentPlayer;
    private Entity _spider;
    private ViewModel _viewModel;
    
    private RenderBuffer _renderBuffer;
    private EasyRenderer _renderer;
    private BillboardRenderer _billboardRenderer;
    private HealthBarRenderer _healthBarRenderer;

    private readonly List<Entity> _entities = [];
    
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
        Graphics.PreferredBackBufferWidth = TargetWidth;
        Graphics.PreferredBackBufferHeight = TargetHeight;
        
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

        _spider = new Spider
        {
            Position = new Vec2(10, 2),
            Target = _currentPlayer,
        };
        _viewModel = new ViewModel(_currentPlayer);
        
        _entities.Add(_currentPlayer);
        _entities.Add(_spider);
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
        
        for (int i = 0; i < _entities.Count; ++i)
        {
            _entities[i].Update(_currentLevel, deltaTime);
        }
        
        _viewModel.Update(deltaTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _renderer.Render(_currentLevel, _currentPlayer);

        _billboardRenderer.Render(_renderBuffer, _currentPlayer,[new Sprite
        {
            Position = _spider.Position,
            Texture = SpriteManager.Get(Sprites.SpiderSheet).Texture,
            SourceRectangle = new Rectangle(0, 0, 256, 256),
            Entity = _spider,
        }], _renderer.WallDepth);
        
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