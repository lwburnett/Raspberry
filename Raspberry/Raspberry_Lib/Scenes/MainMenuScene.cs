using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
using Raspberry_Lib.Components;
using Raspberry_Lib.Content;

namespace Raspberry_Lib.Scenes
{
    internal class MainMenuScene : SceneBase
    {
        private static class Settings
        {
            public static readonly RenderSetting TitlePositionY = new(100);

            public static readonly RenderSetting LabelTopPadding = new(20);
            public static readonly RenderSetting FontScale = new(5);
            public static readonly RenderSetting MinButtonHeight = new(80);

            public const string Version = "Version 0.7.0";
            public static readonly RenderSetting VersionInsetX = new(225);
            public static readonly RenderSetting VersionInsetY = new(50);
            public static readonly RenderSetting VersionFontScale = new(3);

            public static readonly RenderSetting MenuWidth = new(900);
            public static readonly RenderSetting MenuHeight = new(500);

            public static readonly Color TextBoxBackgroundTextureColor = new(112, 128, 144, 200);

            public static readonly RenderSetting MinButtonWidth = new(300);

            public const float MinimumSecondBetweenButtonClicks = .5f;
        }

        public MainMenuScene(Action<int?> iOnStart, Action iOnTutorial, Action iOnCredits)
        {
            _onStart = iOnStart;
            _onTutorial = iOnTutorial;
            _onCredits = iOnCredits;
            _secondsSinceButtonClickAction = 0;
        }

        public override void Initialize()
        {
            base.Initialize();
            
            var backgroundEntity = CreateEntity("background", Screen.Center);
            var texture = Content.LoadTexture(ContentData.AssetPaths.TitleScreenBackground);
            var backgroundSprite = new Sprite(texture);
            backgroundEntity.AddComponent(new SpriteRenderer(backgroundSprite) { RenderLayer = 1 });
            backgroundEntity.SetScale(GetBackgroundScale(texture.Bounds));

            var particleEntity = CreateEntity("particle", Screen.Center);
            particleEntity.AddComponent(new SandParticleRenderer(Screen.Size){ RenderLayer = 0 });

            var canvas = CreateEntity("UiCanvas", Screen.Center).AddComponent(new UICanvas());
            canvas.IsFullScreen = true;

            var titleLabel = new Label("Concurrent Streams", Skin.CreateDefaultSkin());
            canvas.Stage.
                AddElement(titleLabel).
                SetFontScale(Settings.FontScale.Value * 2).
                SetFontColor(Color.Black).
                SetPosition(Screen.Center.X - titleLabel.PreferredWidth / 2, Settings.TitlePositionY.Value);

            var tableDimensions = new Vector2(Settings.MenuWidth.Value, Settings.MenuHeight.Value);

            _menuTable = CreateMenuTable(canvas, tableDimensions);
            _playTable = CreatePlayTable(canvas, tableDimensions);

            SetBackgroundSong(ContentData.AssetPaths.TitleScreenMusic, .6f);

            if (!Input.Touch.IsConnected)
            {
                _inputButton = new VirtualButton(
                    new VirtualButton.GamePadButton(0, Buttons.A),
                    new VirtualButton.MouseLeftButton(),
                    new VirtualButton.KeyboardKey(Keys.Space));
            }
        }

        public override void Update()
        {
            _secondsSinceButtonClickAction += Time.DeltaTime;

            if (!_menuTable.IsVisible() && !_playTable.IsVisible())
            {
                if (Input.Touch.IsConnected)
                {
                    if (Input.Touch.CurrentTouches.Any())
                    {
                        ShowMenu();
                    }
                }
                else
                {
                    if (_inputButton.IsPressed)
                    {
                        ShowMenu();
                    }
                }
            }

            base.Update();
        }

        private Table _menuTable;
        private Table _playTable;
        private VirtualButton _inputButton;
        private readonly Action<int?> _onStart;
        private readonly Action _onTutorial;
        private readonly Action _onCredits;
        private float _secondsSinceButtonClickAction;

        private Vector2 GetBackgroundScale(Rectangle iTextureBounds)
        {
            var scaleX = Screen.Size.X / iTextureBounds.Width;
            var scaleY = Screen.Size.Y / iTextureBounds.Height;

            return new Vector2(scaleX, scaleY);
        }

        private void OnPlayClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _menuTable.SetIsVisible(false);
            _playTable.SetIsVisible(true);

            _secondsSinceButtonClickAction = 0;
        }

        private void OnTutorialClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _onTutorial();

            _secondsSinceButtonClickAction = 0;
        }

        private void OnCreditsClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _onCredits();

            _secondsSinceButtonClickAction = 0;
        }

        private void OnMenuBackClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _menuTable.SetIsVisible(false);
            _playTable.SetIsVisible(false);

            _secondsSinceButtonClickAction = 0;
        }

        private void OnDistanceChallengeClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _onStart(SeedUtils.GetDistanceChallengeSeedForToday());

            _secondsSinceButtonClickAction = 0;
        }

        private void OnTimeChallengeClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _onStart(SeedUtils.GetTimeChallengeSeedForToday());

            _secondsSinceButtonClickAction = 0;
        }

        private void OnEndlessClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _onStart(null);

            _secondsSinceButtonClickAction = 0;
        }

        private void OnPlayBackClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _menuTable.SetIsVisible(true);
            _playTable.SetIsVisible(false);

            _secondsSinceButtonClickAction = 0;
        }

        private Texture2D CreateMenuTexture()
        {
            var backgroundWidth = (int)Settings.MenuWidth.Value;
            var backgroundHeight = (int)Settings.MenuHeight.Value;

            var textureData = new Color[backgroundWidth * backgroundHeight];
            for (var ii = 0; ii < backgroundWidth * backgroundHeight; ii++)
            {
                textureData[ii] = Settings.TextBoxBackgroundTextureColor;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, backgroundWidth, backgroundHeight);
            texture.SetData(textureData);

            return texture;
        }

        private Table CreateMenuTable(UICanvas iUiCanvas, Vector2 iDimensions)
        {
            var table = iUiCanvas.Stage.AddElement(new Table());
            table.SetBounds(
                Screen.Center.X - iDimensions.X / 2,
                Screen.Center.Y - iDimensions.Y / 2,
                iDimensions.X,
                iDimensions.Y);
            table.SetBackground(new SpriteDrawable(CreateMenuTexture()));

            var playButton = new TextButton("Play", Skin.CreateDefaultSkin());
            playButton.OnClicked += OnPlayClicked;
            playButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(playButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var tutorialButton = new TextButton("Tutorial", Skin.CreateDefaultSkin());
            tutorialButton.OnClicked += OnTutorialClicked;
            tutorialButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(tutorialButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var creditsButton = new TextButton("Credits", Skin.CreateDefaultSkin());
            creditsButton.OnClicked += OnCreditsClicked;
            creditsButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(creditsButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var backButton = new TextButton("Back", Skin.CreateDefaultSkin());
            backButton.OnClicked += OnMenuBackClicked;
            backButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(backButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value);

            iUiCanvas.Stage.
                AddElement(new Label(Settings.Version)).
                SetFontScale(Settings.VersionFontScale.Value).
                SetFontColor(Color.Black).
                SetPosition(
                    Screen.Width - Settings.VersionInsetX.Value,
                    Screen.Height - Settings.VersionInsetY.Value);

            iUiCanvas.SetRenderLayer(-1);

            table.SetIsVisible(false);

            return table;
        }

        private Table CreatePlayTable(UICanvas iUiCanvas, Vector2 iDimensions)
        {
            var table = iUiCanvas.Stage.AddElement(new Table());
            table.SetBounds(
                Screen.Center.X - iDimensions.X / 2,
                Screen.Center.Y - iDimensions.Y / 2,
                iDimensions.X,
                iDimensions.Y);
            table.SetBackground(new SpriteDrawable(CreateMenuTexture()));

            var distButton = new TextButton("Dist Challenge", Skin.CreateDefaultSkin());
            distButton.OnClicked += OnDistanceChallengeClicked;
            distButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(distButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var timeButton = new TextButton("Time Challenge", Skin.CreateDefaultSkin());
            timeButton.OnClicked += OnTimeChallengeClicked;
            timeButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(timeButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var endlessButton = new TextButton("Endless", Skin.CreateDefaultSkin());
            endlessButton.OnClicked += OnEndlessClicked;
            endlessButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(endlessButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value);
            table.Row().SetPadTop(Settings.LabelTopPadding.Value);

            var backButton = new TextButton("Back", Skin.CreateDefaultSkin());
            backButton.OnClicked += OnPlayBackClicked;
            backButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(backButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value);

            iUiCanvas.Stage.
                AddElement(new Label(Settings.Version)).
                SetFontScale(Settings.VersionFontScale.Value).
                SetFontColor(Color.Black).
                SetPosition(
                    Screen.Width - Settings.VersionInsetX.Value,
                    Screen.Height - Settings.VersionInsetY.Value);

            iUiCanvas.SetRenderLayer(-1);

            table.SetIsVisible(false);

            return table;
        }

        private void ShowMenu()
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _menuTable.SetIsVisible(true);
            _playTable.SetIsVisible(false);

            _secondsSinceButtonClickAction = 0;

            // _menuTable.SetColor(new Color(0, 0, 0, 0));
            // var tween = _menuTable.Tween("color", Color.White, .5f);
            // tween.Start();
            //
            // var buttons = _menuTable.GetChildren().OfType<TextButton>();
            // foreach (var button in buttons)
            // {
            //     var labelStyle = button.GetLabel().GetStyle();
            //     labelStyle.FontColor = new Color(0, 0, 0, 0);
            //     var labelTween = labelStyle.Tween("FontColor", Color.White, .5f);
            //     labelTween.Start();
            // }
        }
    }
}
