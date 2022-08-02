using Microsoft.Xna.Framework;
using Nez;
using Raspberry_Lib.Scenes;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Raspberry_Lib
{
    public class GameMaster : Core
    {
        private static class Settings
        {
            public static readonly Vector2 TargetScreenSize = new(2340, 1080);
        }

        public GameServiceContainer Service => Core.Services;

        public GameMaster(bool iFullScreen, bool iIsTouch) : base(windowTitle: "Raspberry")
        {
            _fullScreen = iFullScreen;
            _isTouch = iIsTouch;
            IsMouseVisible = !iIsTouch;
        }
        
        private readonly bool _fullScreen;
        private readonly bool _isTouch;

        protected override void Initialize()
        {
            base.Initialize();

            if (_isTouch)
                Input.Touch.EnableTouchSupport();

#if VERBOSE
            DebugRenderEnabled = true;
#endif

            var windowBounds = GraphicsDevice.DisplayMode;
            var renderScaleFactor = windowBounds.Width / Settings.TargetScreenSize.X;
            PlatformUtils.SetRenderScale(renderScaleFactor);

            if (_fullScreen)
            {
                Window.AllowUserResizing = false;
                ExitOnEscapeKeypress = false;

                Screen.IsFullscreen = true;
                Screen.SetSize(windowBounds.Width, windowBounds.Height);
                Screen.ApplyChanges();
            }
            else
            {
                Window.AllowUserResizing = true;
                ExitOnEscapeKeypress = true;
                PauseOnFocusLost = false;

                // Dumb work around to make it so the debug window isn't so annoyingly obtrusive
                Screen.SetSize((int)(renderScaleFactor * Settings.TargetScreenSize.X), (int)(renderScaleFactor * Settings.TargetScreenSize.Y));
            }

            Batcher.UseFnaHalfPixelMatrix = true;
            Scene = new MainMenuScene(() => { Scene = new PrototypeScene(OnMainMenu); }, Exit);
        }

        private void OnMainMenu()
        {
            Scene = new MainMenuScene(OnPlay, Exit);
        }

        private void OnPlay()
        {
            Scene = new PrototypeScene(OnMainMenu);
        }
    }
}
