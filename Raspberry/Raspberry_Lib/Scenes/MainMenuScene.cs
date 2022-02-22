using Nez;
using Nez.UI;

namespace Raspberry_Lib.Scenes
{
    public class MainMenuScene : SceneBase
    {
        public MainMenuScene(System.Action iOnStart, System.Action iOnExit)
        {
            _onStart = iOnStart;
            _onExit = iOnExit;
        }

        public override void Initialize()
        {
            base.Initialize();

            var canvas = CreateEntity("UiCanvas").AddComponent(new UICanvas());
            canvas.IsFullScreen = true;
            var table = canvas.Stage.AddElement(new Table());

            table.SetFillParent(true).Center();
            table.Add(new Label("Main Menu").SetFontScale(5));
            table.Row().SetPadTop(20);
            
            var playButton = new TextButton("Play", Skin.CreateDefaultSkin());
            playButton.OnClicked += OnPlayClicked;
            playButton.GetLabel().SetFontScale(2.5f);
            table.Add(playButton).SetFillX().SetMinHeight(30);
            table.Row().SetPadTop(20);

            var exitButton = new TextButton("Exit", Skin.CreateDefaultSkin());
            exitButton.GetLabel().SetFontScale(2.5f);
            exitButton.OnClicked += OnExitClicked;
            table.Add(exitButton).SetFillX().SetMinHeight(30);
        }

        private readonly System.Action _onStart;
        private readonly System.Action _onExit;

        private void OnPlayClicked(Button iButton)
        {
            _onStart();
        }

        private void OnExitClicked(Button iButton)
        {
            _onExit();
        }
    }
}
