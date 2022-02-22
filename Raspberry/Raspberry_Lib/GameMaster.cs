using Nez;
using Raspberry_Lib.Scenes;

namespace Raspberry_Lib
{
    public class GameMaster : Core
    {
        public GameMaster() : base(windowTitle: "Raspberry")
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

#if DEBUG
            Window.AllowUserResizing = true;
            ExitOnEscapeKeypress = true;
            
            // Dumb work around to make it so the debug window isn't so annoyingly obtrusive
            Screen.SetSize(1910, 1075);
#else
            Window.AllowUserResizing = false;
            ExitOnEscapeKeypress = false;

            Screen.IsFullscreen = true;
            var windowBounds = Window.ClientBounds;
            Screen.SetSize(windowBounds.Width, windowBounds.Height);
            Screen.ApplyChanges();
#endif

            Scene = new MainMenuScene(() => { }, Exit);
        }
    }
}
