using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
using Nez.UI;
using Raspberry_Lib.Components.UI;
using Raspberry_Lib.Scenes;

namespace Raspberry_Lib.Components
{
    internal class PlayUiCanvasComponent : Component, IUpdatable, IBeginPlay
    {
        private static class Settings
        {
            public static readonly RenderSetting Margin = new(100);
            public const int FontScale = 6;
            public static readonly RenderSetting DistanceLabelWidth = new(200);
            public static readonly RenderSetting DistanceLabelHeight = new(75);

            public static readonly RenderSetting IndicatorSizeX = new(150);
            public static readonly RenderSetting IndicatorSizeY = new(150);

            public const float RowTransition1 = .5f;
            public const float RowTransition2 = .9f;
            public const float RowTransition3 = 1.25f;

            public static readonly RenderSetting MenuWidth = new(1050);
            public static readonly RenderSetting MenuHeight = new(500);

            public static readonly RenderSetting PauseButtonFontScale = new(2.5f);

        }

        public PlayUiCanvasComponent(
            Action iOnPlayAgain,
            Action iOnMainMenu,
            Action iOnPause,
            Action iOnResume,
            Scenario iScenario)
        {
            _onPlayAgain = iOnPlayAgain;
            _onMainMenu = iOnMainMenu;
            _onPause = iOnPause;
            _onResume = iOnResume;
            _scenario = iScenario;

            _upPressed = false;
            _downPressed = false;
            _rowColor = RowColor.White;
        }

        public override void OnAddedToEntity()
        {
            OnAddedToEntityInternal();
        }

        public int BeginPlayOrder => 97;
        public void OnBeginPlay()
        {
            BeginPlayInternal();

            if (_introMenu != null)
            {
                _introMenu.SetIsVisible(true);
                _pauseButton.SetIsVisible(false);
            }
        }

        public void Update()
        {
            UpdateInternal();
        }

        public void OnPlayEnd(bool iLost, float iRunTime)
        {
            _statsMenu.SetData(iLost, DistanceLabel.GetText(), iRunTime);
            _statsMenu.SetIsVisible(true);
            _pauseButton.SetIsVisible(false);
            _introMenu?.SetIsVisible(false);
        }

        public void SetPlayTime(float iTime)
        {
            _runTime = iTime;
        }

        public virtual bool ShouldBeAggregatingTime()
        {
            return true;
        }

        private enum RowColor
        {
            White,
            Red,
            Yellow,
            Green
        }

        protected CharacterMovementComponent MovementComponent;
        protected Label DistanceLabel;
        protected Label TimeLabel;

        protected UICanvas Canvas;

        private readonly Action _onPlayAgain;
        private readonly Action _onMainMenu;
        private readonly Action _onPause;
        private readonly Action _onResume;
        private readonly Scenario _scenario;

        private Image _upIndicator;
        private Image _downIndicator;
        private Image _rowIndicator;

        private bool _upPressed;
        private bool _downPressed;

        private Nez.UI.IDrawable _upDefaultIcon;
        private Nez.UI.IDrawable _upPressedIcon;

        private Nez.UI.IDrawable _downDefaultIcon;
        private Nez.UI.IDrawable _downPressedIcon;

        private RowColor _rowColor;

        private Nez.UI.IDrawable _rowWhiteIcon;
        private Nez.UI.IDrawable _rowRedIcon;
        private Nez.UI.IDrawable _rowYellowIcon;
        private Nez.UI.IDrawable _rowGreenIcon;

        protected CharacterInputController.InputDescription InputOverride;
        private float? _lastRowTimeOverride;

        private PlayScreenStatsMenu _statsMenu;
        private Element _pauseMenu;
        private PlayScreenIntroMenu _introMenu;
        private TextButton _pauseButton;
        private float _runTime;

        protected virtual void OnAddedToEntityInternal()
        {
            Canvas = Entity.AddComponent(new UICanvas());
            DistanceLabel = Canvas.Stage.AddElement(new Label("0 m"));
            DistanceLabel.SetBounds(
                (Screen.Width - Settings.DistanceLabelWidth.Value) / 2f,
                Settings.Margin.Value / 4f,
                Settings.DistanceLabelWidth.Value,
                Settings.DistanceLabelHeight.Value);
            DistanceLabel.SetFontScale(Settings.FontScale);

            TimeLabel = Canvas.Stage.AddElement(new Label("0:00"));
            TimeLabel.SetBounds(
                (Screen.Width - Settings.DistanceLabelWidth.Value) / 2f,
                Settings.Margin.Value / 4f + Settings.DistanceLabelHeight.Value,
                Settings.DistanceLabelWidth.Value,
                Settings.DistanceLabelHeight.Value);
            TimeLabel.SetFontScale(Settings.FontScale);

            const int cellSize = 32;
            var textureAtlas = Entity.Scene.Content.LoadTexture(Content.ContentData.AssetPaths.IconsTileset, true);
            var spriteList = Sprite.SpritesFromAtlas(textureAtlas, cellSize, cellSize);

            _upDefaultIcon = new SpriteDrawable(spriteList[4]);
            _upPressedIcon = new SpriteDrawable(spriteList[5]);

            _downDefaultIcon = new SpriteDrawable(spriteList[8]);
            _downPressedIcon = new SpriteDrawable(spriteList[9]);

            _rowWhiteIcon = new SpriteDrawable(spriteList[0]);
            _rowRedIcon = new SpriteDrawable(spriteList[1]);
            _rowYellowIcon = new SpriteDrawable(spriteList[2]);
            _rowGreenIcon = new SpriteDrawable(spriteList[3]);

            var drawColor = Color.White;
            drawColor.A = 127;

            _upIndicator = Canvas.Stage.AddElement(new Image(_upDefaultIcon));
            _upIndicator.SetBounds(
                Settings.Margin.Value,
                Screen.Height * .20f - Settings.IndicatorSizeY.Value / 2f,
                Settings.IndicatorSizeX.Value,
                Settings.IndicatorSizeY.Value);
            _upIndicator.SetScaling(Scaling.Fill);
            _upIndicator.SetColor(drawColor);

            _downIndicator = Canvas.Stage.AddElement(new Image(_downDefaultIcon));
            _downIndicator.SetBounds(
                Settings.Margin.Value,
                Screen.Height * .80f - Settings.IndicatorSizeY.Value / 2f,
                Settings.IndicatorSizeX.Value,
                Settings.IndicatorSizeY.Value);
            _downIndicator.SetScaling(Scaling.Fill);
            _downIndicator.SetColor(drawColor);

            _rowIndicator = Canvas.Stage.AddElement(new Image(spriteList[0]));
            _rowIndicator.SetBounds(
                Screen.Width - Settings.Margin.Value - Settings.IndicatorSizeX.Value,
                Screen.Height * .5f - Settings.IndicatorSizeY.Value / 2f,
                Settings.IndicatorSizeX.Value,
                Settings.IndicatorSizeY.Value);
            _rowIndicator.SetScaling(Scaling.Fill);
            _rowIndicator.SetColor(drawColor);

            _pauseButton = Canvas.Stage.AddElement(new TextButton("Pause", SkinManager.GetGameUiSkin()));
            _pauseButton.OnClicked += OnPause;
            _pauseButton.GetLabel().SetFontScale(Settings.PauseButtonFontScale.Value);
            _pauseButton.SetBounds(
                Screen.Width - Settings.Margin.Value - Settings.DistanceLabelWidth.Value / 4,
                Settings.Margin.Value / 2f,
                Settings.DistanceLabelWidth.Value / 2,
                Settings.DistanceLabelHeight.Value / 2);

            // Intro, pause, and end menus
            var menuBounds = new RectangleF((Screen.Width - Settings.MenuWidth.Value) / 2f,
                (Screen.Height - Settings.MenuHeight.Value) / 2f,
                Settings.MenuWidth.Value,
                Settings.MenuHeight.Value);

            if (_scenario.IntroLines.Any())
            {
                _pauseButton.SetIsVisible(false);

                var introMenu = new PlayScreenIntroMenu(
                    (SceneBase)Entity.Scene,
                    menuBounds,
                    _scenario.Title,
                    _scenario.IntroLines,
                    OnResume,
                    OnMainMenu);
                introMenu.Initialize();
                _introMenu = Canvas.Stage.AddElement(introMenu);
                _introMenu.SetIsVisible(false);
            }

            var pauseMenu = new PlayScreenPauseMenu(
                (SceneBase)Entity.Scene,
                menuBounds, 
                OnResume, 
                OnPlayAgain, 
                OnMainMenu);
            pauseMenu.Initialize();
            _pauseMenu = Canvas.Stage.AddElement(pauseMenu);
            SetPauseMenuVisibility(false);

            var statsMenu = new PlayScreenStatsMenu(
                (SceneBase)Entity.Scene,
                menuBounds,
                _scenario.Title,
                _scenario.LoseLines,
                _scenario.EndLines,
                OnPlayAgain,
                OnMainMenu);
            statsMenu.Initialize();
            _statsMenu = Canvas.Stage.AddElement(statsMenu);
            _statsMenu.SetIsVisible(false);

            // This needs to match the render layer of the ScreenSpaceRenderer in SceneBase ctor
            Canvas.SetRenderLayer(-1);
        }

        protected virtual void UpdateInternal()
        {
            if (MovementComponent == null)
                return;

            // Handle Distance Display
            var distanceTraveled = (int)Mathf.Round(MovementComponent.TotalDistanceTraveled);
            DistanceLabel.SetText($"{distanceTraveled} m");

            // Handle Time Display
            TimeLabel.SetText(TimeSpan.FromSeconds(_runTime).ToString("mm':'ss"));

            // Handle Turning Indicators
            var input = InputOverride ?? MovementComponent.CurrentInput;

            var upPressed = input.Rotation < 0f;
            if (_upPressed != upPressed)
            {
                _upPressed = upPressed;
                _upIndicator.SetDrawable(_upPressed ? _upPressedIcon : _upDefaultIcon);
            }

            var downPressed = input.Rotation > 0f;
            if (_downPressed != downPressed)
            {
                _downPressed = downPressed;
                _downIndicator.SetDrawable(_downPressed ? _downPressedIcon : _downDefaultIcon);
            }

            // Handle Row Indicator
            float secondsSinceLastRow;
            if (InputOverride == null)
            {
                _lastRowTimeOverride = null;

                secondsSinceLastRow = MovementComponent.SecondsSinceLastRow;
            }
            else
            {
                if (InputOverride.Row)
                {
                    _lastRowTimeOverride = 0f;
                }
                else
                {
                    if (_lastRowTimeOverride != null)
                        _lastRowTimeOverride += Time.DeltaTime;
                    else
                        _lastRowTimeOverride = Settings.RowTransition3;
                }

                secondsSinceLastRow = _lastRowTimeOverride.Value;
            }

            if (secondsSinceLastRow < Settings.RowTransition1)
            {
                if (_rowColor != RowColor.Red)
                {
                    _rowIndicator.SetDrawable(_rowRedIcon);
                    _rowColor = RowColor.Red;
                }
            }
            else if (secondsSinceLastRow < Settings.RowTransition2)
            {
                if (_rowColor != RowColor.Yellow)
                {
                    _rowIndicator.SetDrawable(_rowYellowIcon);
                    _rowColor = RowColor.Yellow;
                }
            }
            else if (secondsSinceLastRow < Settings.RowTransition3)
            {
                if (_rowColor != RowColor.Green)
                {
                    _rowIndicator.SetDrawable(_rowGreenIcon);
                    _rowColor = RowColor.Green;
                }
            }
            else
            {
                if (_rowColor != RowColor.White)
                {
                    _rowIndicator.SetDrawable(_rowWhiteIcon);
                    _rowColor = RowColor.White;
                }
            }
        }

        protected virtual void BeginPlayInternal()
        {
            MovementComponent = Entity.Scene.FindEntity("character").GetComponent<CharacterMovementComponent>();

            System.Diagnostics.Debug.Assert(MovementComponent != null);
        }

        private void OnPlayAgain(Button iButton)
        {
            PlatformUtils.VibrateForUiNavigation();
            _onPlayAgain();
        }

        private void OnMainMenu(Button iButton)
        {
            PlatformUtils.VibrateForUiNavigation();
            _onMainMenu();
        }

        protected virtual void OnPause(Button iButton)
        {
            PlatformUtils.VibrateForUiNavigation();
            _onPause();
            SetPauseMenuVisibility(true);
        }

        protected virtual void OnResume(Button iButton)
        {
            PlatformUtils.VibrateForUiNavigation();
            _onResume();
            SetPauseMenuVisibility(false);
            _introMenu?.SetIsVisible(false);
        }

        protected void SetPauseMenuVisibility(bool iVisibility)
        {
            _pauseMenu.SetIsVisible(iVisibility);
            _pauseButton.SetIsVisible(!iVisibility);
        }
    }
}
