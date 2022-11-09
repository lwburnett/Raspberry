using System;
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
            public static readonly RenderSetting MinButtonHeight = new(80);

            public static readonly RenderSetting TablePositionX = new(300);
            public static readonly RenderSetting TablePositionY = new(300);

            public const string Version = "Version 0.5.0";
            public static readonly RenderSetting VersionInsetX = new(225);
            public static readonly RenderSetting VersionInsetY = new(50);
            public static readonly RenderSetting VersionFontScale = new(3);
        }

        public MainMenuScene(Action iOnStart, Action iOnTutorial, Action iOnCredits, Action iOnExit)
        {
            _onStart = iOnStart;
            _onTutorial = iOnTutorial;
            _onCredits = iOnCredits;
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
            table.Add(new Label("Concurrent Streams").
                SetFontScale(Settings.FontScale.Value).
                SetFontColor(Color.Black));
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);
            
            var playButton = new TextButton("Play", Skin.CreateDefaultSkin());
            playButton.OnClicked += OnPlayClicked;
            playButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(playButton).SetFillX().SetMinHeight(Settings.MinButtonHeight.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var tutorialButton = new TextButton("Tutorial", Skin.CreateDefaultSkin());
            tutorialButton.OnClicked += OnTutorialClicked;
            tutorialButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(tutorialButton).SetFillX().SetMinHeight(Settings.MinButtonHeight.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var creditsButton = new TextButton("Credits", Skin.CreateDefaultSkin());
            creditsButton.OnClicked += OnCreditsClicked;
            creditsButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(creditsButton).SetFillX().SetMinHeight(Settings.MinButtonHeight.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var exitButton = new TextButton("Exit", Skin.CreateDefaultSkin());
            exitButton.OnClicked += OnExitClicked;
            exitButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(exitButton).SetFillX().SetMinHeight(Settings.MinButtonHeight.Value);

            canvas.Stage.
                AddElement(new Label(Settings.Version)).
                SetFontScale(Settings.VersionFontScale.Value).
                SetFontColor(Color.Black).
                SetPosition(
                    Screen.Width - Settings.VersionInsetX.Value, 
                    Screen.Height - Settings.VersionInsetY.Value);

            SetBackgroundSong(ContentData.AssetPaths.TitleScreenMusic, .6f);
        }

        private readonly Action _onStart;
        private readonly Action _onTutorial;
        private readonly Action _onCredits;
        private readonly Action _onExit;

        private void OnPlayClicked(Button iButton)
        {
            _onStart();
        }

        private void OnTutorialClicked(Button iButton)
        {
            _onTutorial();
        }

        private void OnCreditsClicked(Button iButton)
        {
            _onCredits();
        }

        private void OnExitClicked(Button iButton)
        {
            _onExit();
        }
    }
}
