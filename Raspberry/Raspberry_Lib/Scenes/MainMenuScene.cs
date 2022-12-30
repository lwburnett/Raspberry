using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.UI;
using Raspberry_Lib.Components;
using Raspberry_Lib.Components.UI;
using Raspberry_Lib.Content;

namespace Raspberry_Lib.Scenes
{
    internal class MainMenuScene : SceneBase
    {
        private static class Settings
        {
            public static readonly RenderSetting TitlePositionY = new(100);
            
            public static readonly RenderSetting FontScale = new(5);

            public const string Version = "Version 0.7.0";
            public static readonly RenderSetting VersionInsetX = new(225);
            public static readonly RenderSetting VersionInsetY = new(50);
            public static readonly RenderSetting VersionFontScale = new(3);

            public static readonly RenderSetting MenuWidth = new(1050);
            public static readonly RenderSetting MenuHeight = new(500);

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

            var menuBounds = new RectangleF(Screen.Center.X - tableDimensions.X / 2,
                Screen.Center.Y - tableDimensions.Y / 2,
                tableDimensions.X,
                tableDimensions.Y);

            var mainMenu = new TitleScreenMainMenu(
                menuBounds, 
                OnPlayClicked, 
                OnCreditsClicked, 
                OnSettingsClicked,
                OnMenuBackClicked);
            mainMenu.Initialize();
            mainMenu.SetIsVisible(false);

            var playMenu = new TitleScreenPlayMenu(
                menuBounds, 
                OnDistanceChallengeClicked,
                OnTimeChallengeClicked,
                OnEndlessClicked,
                OnTutorialClicked,
                OnPlayBackClicked);
            playMenu.Initialize();
            playMenu.SetIsVisible(false);

            var settingsMenu = new SettingsMenu(
                menuBounds,
                OnPlayBackClicked);
            settingsMenu.Initialize();
            settingsMenu.SetIsVisible(false);

            _mainMenu = canvas.Stage.AddElement(mainMenu);
            _playMenu = canvas.Stage.AddElement(playMenu);
            _settingsMenu = canvas.Stage.AddElement(settingsMenu);


            canvas.Stage.
                AddElement(new Label(Settings.Version)).
                SetFontScale(Settings.VersionFontScale.Value).
                SetFontColor(Color.Black).
                SetPosition(
                    Screen.Width - Settings.VersionInsetX.Value,
                    Screen.Height - Settings.VersionInsetY.Value);

            canvas.SetRenderLayer(-1);

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

            if (!_mainMenu.IsVisible() && !_playMenu.IsVisible() && !_settingsMenu.IsVisible())
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

        private MenuBase _mainMenu;
        private MenuBase _playMenu;
        private MenuBase _settingsMenu;
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

            _mainMenu.SetIsVisible(false);
            _playMenu.SetIsVisible(true);
            _settingsMenu.SetIsVisible(false);

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
        
        private void OnSettingsClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _mainMenu.SetIsVisible(false);
            _playMenu.SetIsVisible(false);
            _settingsMenu.SetIsVisible(true);

            _secondsSinceButtonClickAction = 0;
        }

        private void OnMenuBackClicked(Button iButton)
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _mainMenu.SetIsVisible(false);
            _playMenu.SetIsVisible(false);
            _settingsMenu.SetIsVisible(false);

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

            _mainMenu.SetIsVisible(true);
            _playMenu.SetIsVisible(false);
            _settingsMenu.SetIsVisible(false);

            _secondsSinceButtonClickAction = 0;
        }
        private void ShowMenu()
        {
            if (_secondsSinceButtonClickAction < Settings.MinimumSecondBetweenButtonClicks)
                return;

            _mainMenu.SetIsVisible(true);
            _playMenu.SetIsVisible(false);
            _settingsMenu.SetIsVisible(false);

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
