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
        }

        public MainMenuScene(Action iOnStart, Action iOnTutorial, Action iOnCredits)
        {
            _onStart = iOnStart;
            _onTutorial = iOnTutorial;
            _onCredits = iOnCredits;
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

            var menuWidth = Settings.MenuWidth.Value;
            var menuHeight = Settings.MenuHeight.Value;

            var table = canvas.Stage.AddElement(new Table());
            table.SetBounds(
                Screen.Center.X - menuWidth / 2, 
                Screen.Center.Y - menuHeight / 2,
                menuWidth,
                menuHeight);
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
            backButton.OnClicked += OnBackClicked;
            backButton.GetLabel().SetFontScale(Settings.FontScale.Value);
            table.Add(backButton).
                SetMinHeight(Settings.MinButtonHeight.Value).
                SetMinWidth(Settings.MinButtonWidth.Value);

            canvas.Stage.
                AddElement(new Label(Settings.Version)).
                SetFontScale(Settings.VersionFontScale.Value).
                SetFontColor(Color.Black).
                SetPosition(
                    Screen.Width - Settings.VersionInsetX.Value, 
                    Screen.Height - Settings.VersionInsetY.Value);

            canvas.SetRenderLayer(-1);

            table.SetIsVisible(false);
            _menuTable = table;

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
            base.Update();

            if (!_menuTable.IsVisible())
            {
                if (Input.Touch.IsConnected)
                {
                    if (Input.Touch.CurrentTouches.Any())
                    {
                        _menuTable.SetIsVisible(true);
                    }
                }
                else
                {
                    if (_inputButton.IsPressed)
                    {
                        _menuTable.SetIsVisible(true);
                    }
                }
            }
        }

        private Table _menuTable;
        private VirtualButton _inputButton;
        private readonly Action _onStart;
        private readonly Action _onTutorial;
        private readonly Action _onCredits;

        private Vector2 GetBackgroundScale(Rectangle iTextureBounds)
        {
            var scaleX = Screen.Size.X / iTextureBounds.Width;
            var scaleY = Screen.Size.Y / iTextureBounds.Height;

            return new Vector2(scaleX, scaleY);
        }

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

        private void OnBackClicked(Button iButton)
        {
            _menuTable.SetIsVisible(false);
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
    }
}
