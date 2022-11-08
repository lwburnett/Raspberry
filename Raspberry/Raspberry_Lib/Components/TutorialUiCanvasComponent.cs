using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.UI;

namespace Raspberry_Lib.Components
{
    internal class TutorialUiCanvasComponent : PlayUiCanvasComponent
    {
        private static class Settings
        {
            public static readonly RenderSetting MarginBottom = new(50);
            public static readonly RenderSetting CellPadding = new(0);
            public static readonly RenderSetting BackgroundTextureWidth = new(1500);
            public static readonly RenderSetting BackgroundTextureHeight = new(200);
            public static readonly Color TextBoxBackgroundTextureColor = new(112, 128, 144, 200);
            public static readonly Color DistanceDisplayBackgroundTextureColor = new(136, 8, 8, 200);

            public const float BlinkPeriodSeconds = 1f;
        }

        public enum State
        {
            Welcome = 0,
            StoryIntro1,
            StoryIntro2,
            StoryIntro3,
            GameGoal1,
            DistanceDisplay,
            EndPlay
        }

        public TutorialUiCanvasComponent()
        {
            _currentState = 0;
            _timeSinceLastBlinkToggle = null;
        }

        private State _currentState;
        private PlayerProximityComponent _characterProximity;
        private CharacterInputController _characterInputController;
        private TutorialNavigationInputController _navigationInputController;

        private Table _textBox;
        private Label _text;

        private float? _timeSinceLastBlinkToggle;
        private bool _blinkToggle;

        private Nez.UI.IDrawable _distanceDisplayBackground;

        protected sealed override void OnAddedToEntityInternal()
        {
            base.OnAddedToEntityInternal();
            
            _navigationInputController = Entity.AddComponent(new TutorialNavigationInputController(OnNavigation));

            var textBoxBackgroundTexture = CreateTextBoxBackground();
            var spriteDrawable = new SpriteDrawable(textBoxBackgroundTexture);

            _textBox = Canvas.Stage.AddElement(new Table());
            _textBox.SetSize(Settings.BackgroundTextureWidth.Value, Settings.BackgroundTextureHeight.Value);
            _textBox.SetBounds(
                (Screen.Width - Settings.BackgroundTextureWidth.Value) / 2f, 
                Screen.Height - Settings.MarginBottom.Value - Settings.BackgroundTextureHeight.Value,
                Settings.BackgroundTextureWidth.Value,
                Settings.BackgroundTextureHeight.Value);
            _textBox.SetBackground(spriteDrawable);
            _text = new Label(string.Empty).
                SetFontScale(5).
                SetFontColor(Color.White).
                SetAlignment(Align.TopLeft);
            _textBox.Add(_text).Pad(Settings.CellPadding.Value);

            var distanceDisplayBackgroundTexture = CreateDistanceDisplayBackgroundTexture();
            _distanceDisplayBackground = new SpriteDrawable(distanceDisplayBackgroundTexture);
        }

        protected sealed override void BeginPlayInternal()
        {
            base.BeginPlayInternal();

            var characterEntity = Entity.Scene.FindEntity("character");
            _characterProximity = characterEntity.GetComponent<PlayerProximityComponent>();
            _characterInputController = characterEntity.GetComponent<CharacterInputController>();

            System.Diagnostics.Debug.Assert(_characterProximity != null);

            OnNavigationChanged(State.Welcome, State.Welcome);
        }

        protected sealed override void UpdateInternal()
        {
            base.UpdateInternal();

            if (!_timeSinceLastBlinkToggle.HasValue)
                return;

            _timeSinceLastBlinkToggle += Time.DeltaTime;

            if (_currentState == State.DistanceDisplay)
            {
                if (_timeSinceLastBlinkToggle.Value >= Settings.BlinkPeriodSeconds)
                {
                    _blinkToggle = !_blinkToggle;
                    _timeSinceLastBlinkToggle = 0;
                    var drawable = _blinkToggle ? _distanceDisplayBackground : null;
                    DistanceLabel.SetBackground(drawable);
                }
            }
        }

        private Texture2D CreateTextBoxBackground()
        {
            var width = (int)Math.Round(Settings.BackgroundTextureWidth.Value);
            var height = (int)Math.Round(Settings.BackgroundTextureHeight.Value);

            return CreateSolidColorTexture(width, height, Settings.TextBoxBackgroundTextureColor);
        }

        private Texture2D CreateDistanceDisplayBackgroundTexture()
        {
            var width = (int)DistanceLabel.PreferredWidth;
            var height = (int)DistanceLabel.PreferredHeight;
            return CreateSolidColorTexture(width, height, Settings.DistanceDisplayBackgroundTextureColor);
        }

        private Texture2D CreateSolidColorTexture(int iWidth, int iHeight, Color iColor)
        {
            var textureData = new Color[iWidth * iHeight];
            for (var ii = 0; ii < iWidth * iHeight; ii++)
            {
                textureData[ii] = iColor;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, iWidth, iHeight);
            texture.SetData(textureData);

            return texture;
        }

        private void OnNavigation()
        {
            OnNavigationChanged(_currentState, _currentState + 1);
            _currentState++;
        }

        private void OnNavigationChanged(State iOldState, State iNewState)
        {
            switch (iOldState)
            {
                case State.DistanceDisplay:
                    DistanceLabel.SetBackground(null);
                    _timeSinceLastBlinkToggle = null;
                    break;
                case State.Welcome:
                case State.StoryIntro1:
                case State.StoryIntro2:
                case State.StoryIntro3:
                case State.GameGoal1:
                case State.EndPlay:
                    break;
                default:
                    System.Diagnostics.Debug.Fail($"Unknown type of {nameof(State)}");
                    return;
            }

            switch (iNewState)
            {
                case State.Welcome:
                    HandleGenericTextChange("Welcome to Concurrent Streams");
                    break;
                case State.StoryIntro1:
                    HandleGenericTextChange("In a parallel world, this desolate wasteland\nis a beautiful meadow. In the meadow\nis a pristine stream.");
                    break;
                case State.StoryIntro2:
                    HandleGenericTextChange("A mysterious energy has ripped a hole\ninto this parallel world.");
                    break;
                case State.StoryIntro3:
                    HandleGenericTextChange("The boat must collect energy to stay in\nthe meadow. Otherwise be trapped in the desert.");
                    break;
                case State.GameGoal1:
                    HandleGenericTextChange("The goal of the game is to make it as far as\npossible along the stream before\nthe energy runs out.");
                    break;
                case State.DistanceDisplay:
                    HandleDistanceDisplay();
                    break;
                case State.EndPlay:
                    HandleEndPlay();
                    break;
                default:
                    System.Diagnostics.Debug.Fail($"Unknown type of {nameof(State)}");
                    return;
            }
        }

        private void SetPauseState(bool iValue)
        {
            MovementComponent.IsPaused = iValue;
            _characterProximity.IsPaused = iValue;
            _characterInputController.IsPaused = iValue;
            _navigationInputController.IsPaused = !iValue;
        }

        private void HandleGenericTextChange(string iText)
        {
            SetPauseState(true);
            _textBox.SetIsVisible(true);
            _text.SetText(iText);
        }

        private void HandleDistanceDisplay()
        {
            _blinkToggle = true;
            _timeSinceLastBlinkToggle = 0;
            DistanceLabel.SetBackground(_distanceDisplayBackground);
            HandleGenericTextChange("Distance is shown at the top of the screen.");
        }

        private void HandleEndPlay()
        {
            SetPauseState(false);
            _textBox.SetIsVisible(false);
        }
    }
}
