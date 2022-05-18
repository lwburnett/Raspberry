using Microsoft.Xna.Framework;
using Nez;
using Raspberry_Lib.Scenes;

namespace Raspberry_Lib
{
    public class GameMaster : Core
    {
        private static class Settings
        {
            public static readonly Vector2 TargetScreenSize = new(2340, 1080);
        }

        public GameMaster(bool iFullScreen) : base(windowTitle: "Raspberry")
        {
            _fullScreen = iFullScreen;
        }
        
        private readonly bool _fullScreen;

        protected override void Initialize()
        {
            base.Initialize();

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
                renderScaleFactor = windowBounds.Width / Settings.TargetScreenSize.X;
            }
            else
            {
                Window.AllowUserResizing = true;
                ExitOnEscapeKeypress = true;
                PauseOnFocusLost = false;

                // Dumb work around to make it so the debug window isn't so annoyingly obtrusive
                Screen.SetSize((int)(renderScaleFactor * Settings.TargetScreenSize.X), (int)(renderScaleFactor * Settings.TargetScreenSize.Y));
            }

            Scene = new MainMenuScene(() => { Scene = new PrototypeScene(); }, Exit);
        }
    }
}
