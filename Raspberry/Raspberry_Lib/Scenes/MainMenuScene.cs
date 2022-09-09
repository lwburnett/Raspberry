using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using Raspberry_Lib.Content;

namespace Raspberry_Lib.Scenes
{
    internal class MainMenuScene : SceneBase
    {
        private static class Settings
        {
            public static readonly RenderSetting LabelTopPadding = new(20);
            public static readonly RenderSetting FontScale = new(5);
            public static readonly RenderSetting MinButtonHeight = new(50);

            public static readonly RenderSetting TablePositionX = new(300);
            public static readonly RenderSetting TablePositionY = new(200);
        }

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

            var background = new Image(Content.LoadTexture(ContentData.AssetPaths.TitleScreenBackground));
            background.SetScaling(Scaling.Stretch);
            background.SetWidth(canvas.Width);
            background.SetHeight(canvas.Height);
            canvas.Stage.AddElement(background);


            var table = canvas.Stage.AddElement(new Table());
            table.SetPosition(Settings.TablePositionX.Value, Settings.TablePositionY.Value);
            table.Add(new Label("Awesome Title").SetFontScale(5).SetFontColor(Color.Black));
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);
            
            var playButton = new TextButton("Play", Skin.CreateDefaultSkin());
            playButton.OnClicked += OnPlayClicked;
            playButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(playButton).SetFillX().SetMinHeight(Settings.MinButtonHeight.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var exitButton = new TextButton("Exit", Skin.CreateDefaultSkin());
            exitButton.OnClicked += OnExitClicked;
            exitButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(exitButton).SetFillX().SetMinHeight(Settings.MinButtonHeight.Value);
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
