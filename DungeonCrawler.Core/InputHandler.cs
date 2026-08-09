using DungeonCrawler.Core.Maths;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DungeonCrawler.Core;

public sealed class InputHandler(GameWindow window)
{
    public static float MouseSensitivity { get; set; } = 0.0025f;

    public bool IsMouseVisible { get; private set; }
    
    private bool _isMouseCaptured = true;
    
    private KeyboardState _lastKeyboardState;
    private MouseState _lastMouseState;

    public float MouseTurn
    {
        get
        {
            if (_isMouseCaptured == false) return 0.0f;
            
            var mouse = Mouse.GetState();
            
            var cx = window.ClientBounds.Width / 2;
            var cy = window.ClientBounds.Width / 2;

            var deltaX = mouse.X - cx;

            if (deltaX != 0) Mouse.SetPosition(cx, cy);

            return -deltaX * MouseSensitivity;
        }
    }
    
    public void Update(float deltaTime)
    {
        var mouseState = Mouse.GetState();
        
        if (IsKeyDown(Keys.Escape))
        {
            _isMouseCaptured = false;
            IsMouseVisible = true;
        }
        else if (_isMouseCaptured == false && JustClickedLeftButton() && !ImGui.IsAnyItemHovered() && !ImGui.IsAnyItemActive() && !ImGui.IsAnyItemFocused())
        {
            IsMouseVisible = false;
            _isMouseCaptured = true;
            
            var cx = window.ClientBounds.Width / 2;
            var cy = window.ClientBounds.Width / 2;
            
            Mouse.SetPosition(cx, cy);
        }
        _lastKeyboardState = Keyboard.GetState();
    }

    public int Forward => IsKeyDown(Keys.W) ? 1 : IsKeyDown(Keys.S) ? -1 : 0;

    public int Lateral => IsKeyDown(Keys.A) ? 1 : IsKeyDown(Keys.D) ? -1 : 0;

    public bool IsKeyDown(Keys key)
    {
        return _lastKeyboardState.IsKeyDown(key);
    }
    
    public bool IsKeyJustPressed(Keys key)
    {
        return !_lastKeyboardState.IsKeyDown(key) && Keyboard.GetState().IsKeyDown(key);
    }

    public bool JustClickedLeftButton()
    {
        return _lastMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed;
    }
}