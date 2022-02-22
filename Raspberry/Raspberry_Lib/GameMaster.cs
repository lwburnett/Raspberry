using Nez;
using Raspberry_Lib.Scenes;

namespace Raspberry_Lib
{
    public class GameMaster : Core
    {
        public GameMaster() :
            base(isFullScreen: true, windowTitle: "Raspberry")
        {
            
        }

        protected override void Initialize()
        {
            base.Initialize();

            Window.AllowUserResizing = false;
            ExitOnEscapeKeypress = true;

            Scene = new MainMenuScene(() => { }, Exit);
        }
    }
}
