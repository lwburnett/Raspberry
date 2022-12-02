using System;
using System.Linq;
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
            public static readonly Color HighlightDisplayBackgroundTextureColor = new(136, 8, 8, 200);
            public static readonly RenderSetting EnergyDisplayThickness = new(10);
            public const float EnergyDecayPercentLossPerSecond = .15f;
            public const float EnergyDecayMinimumScale = .33f;
            public static readonly RenderSetting ObstacleAlertRadius = new(1000);

            public static readonly RenderSetting ObstacleAlertThickness = new(7.5f);
            public const float ObstacleCircleSizeMultiplier = .5f;

            public const float BlinkPeriodSeconds = 1f;
            public const float RowPeriodSeconds = 2f;
        }

        public enum State
        {
            Welcome = 0,
            StoryIntro1,
            StoryIntro2,
            StoryIntro3,
            GameGoal1,
            DistanceDisplay,
            EnergyDisplay,
            EnergyDecay1,
            EnergyDecay2,
            TurningUp,
            TurningDown,
            Rowing1,
            Rowing2,
            Rowing3,
            Rowing4,
            FirstPlayInstructions,
            FirstPlay,
            Collision1,
            Collision2,
            SecondPlayInstructions,
            SecondPlay,
            Energy,
            GoodLuck,
            EndPlay
        }

        public TutorialUiCanvasComponent(Action iOnPlayAgain, Action iOnMainMenu) :
            base(iOnPlayAgain, iOnMainMenu)
        {
            _currentState = 0;
            _timeSinceLastBlinkToggle = null;
            _blinkToggle = false;
            _energyDisplaySizeMultiplier = 1f;
        }

        private State _currentState;
        private PlayerProximityComponent _characterProximity;
        private CharacterInputController _characterInputController;
        private TutorialNavigationInputController _navigationInputController;

        private Table _textBox;
        private Label _text;

        private float? _timeSinceLastBlinkToggle;
        private bool _blinkToggle;

        private float _energyDisplaySizeMultiplier;

        private Nez.UI.IDrawable _distanceDisplayBackground;
        private Image _energyDisplay;
        private Image _obstacleAlert;
        private Vector2 _firstRockLocation;
        private Vector2 _firstEnergyLocation;

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
            System.Diagnostics.Debug.Assert(characterEntity != null);

            _characterProximity = characterEntity.GetComponent<PlayerProximityComponent>();
            _characterInputController = characterEntity.GetComponent<CharacterInputController>();
            System.Diagnostics.Debug.Assert(_characterProximity != null);
            System.Diagnostics.Debug.Assert(_characterInputController != null);

            var energyDisplayTexture = CreateCircleDisplayTexture(_characterProximity.Radius, Settings.EnergyDisplayThickness.Value);
            _energyDisplay = Canvas.Stage.AddElement(new Image(energyDisplayTexture));
            _energyDisplay.SetIsVisible(false);

            var obstacleAlertTexture = CreateCircleDisplayTexture(
                _characterProximity.Radius * Settings.ObstacleCircleSizeMultiplier,
                Settings.ObstacleAlertThickness.Value);
            _obstacleAlert = Canvas.Stage.AddElement(new Image(obstacleAlertTexture));
            _obstacleAlert.SetIsVisible(false);

            var map = Entity.Scene.FindEntity("map");
            System.Diagnostics.Debug.Assert(map != null);

            var proceduralGenerator = map.GetComponent<ProceduralGeneratorComponent>();
            System.Diagnostics.Debug.Assert(proceduralGenerator != null);

            _firstRockLocation = proceduralGenerator.Blocks[1].Obstacles[0].Position;
            _firstEnergyLocation = proceduralGenerator.Blocks[1].Obstacles[1].Position;

            OnNavigationChanged(State.Welcome, State.Welcome);
        }

        protected sealed override void UpdateInternal()
        {
            base.UpdateInternal();

            if (_timeSinceLastBlinkToggle.HasValue)
            {
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
                else if (_currentState == State.EnergyDisplay)
                {
                    var characterPositionInScreenSpace =
                        Entity.Scene.Camera.WorldToScreenPoint(_characterProximity.Entity.Position);

                    _energyDisplay.SetPosition(
                        characterPositionInScreenSpace.X - _characterProximity.Radius,
                        characterPositionInScreenSpace.Y - _characterProximity.Radius);

                    if (_timeSinceLastBlinkToggle.Value >= Settings.BlinkPeriodSeconds)
                    {
                        _blinkToggle = !_blinkToggle;
                        _timeSinceLastBlinkToggle = 0;
                        _energyDisplay.SetIsVisible(_blinkToggle);
                    }
                }
                else if (_currentState == State.TurningUp)
                {
                    if (_timeSinceLastBlinkToggle.Value >= Settings.BlinkPeriodSeconds)
                    {
                        _blinkToggle = !_blinkToggle;
                        _timeSinceLastBlinkToggle = 0;
                        var rotation = _blinkToggle ? -1 : 0;
                        InputOverride = new CharacterInputController.InputDescription(rotation, false);
                    }
                }
                else if (_currentState == State.TurningDown)
                {
                    if (_timeSinceLastBlinkToggle.Value >= Settings.BlinkPeriodSeconds)
                    {
                        _blinkToggle = !_blinkToggle;
                        _timeSinceLastBlinkToggle = 0;
                        var rotation = _blinkToggle ? 1 : 0;
                        InputOverride = new CharacterInputController.InputDescription(rotation, false);
                    }
                }
                else if (_currentState is State.Rowing1 or State.Rowing2 or State.Rowing3 or State.Rowing4)
                {
                    if (_timeSinceLastBlinkToggle.Value >= Settings.RowPeriodSeconds)
                    {
                        _timeSinceLastBlinkToggle = 0;
                        InputOverride = new CharacterInputController.InputDescription(0, true);
                    }
                    else
                    {
                        InputOverride = new CharacterInputController.InputDescription(0, false);
                    }
                }
                else if (_currentState == State.Collision1 || _currentState == State.Collision2)
                {
                    var rockPositionInScreenSpace =
                        Entity.Scene.Camera.WorldToScreenPoint(_firstRockLocation);

                    var radius = _characterProximity.Radius * Settings.ObstacleCircleSizeMultiplier;
                    _obstacleAlert.SetBounds(
                        rockPositionInScreenSpace.X - radius,
                        rockPositionInScreenSpace.Y - radius,
                        radius * 2,
                        radius * 2);

                    if (_timeSinceLastBlinkToggle.Value >= Settings.BlinkPeriodSeconds)
                    {
                        _blinkToggle = !_blinkToggle;
                        _timeSinceLastBlinkToggle = 0;
                        _obstacleAlert.SetIsVisible(_blinkToggle);
                    }
                }
                else if (_currentState == State.Energy)
                {
                    var energyPositionInScreenSpace =
                        Entity.Scene.Camera.WorldToScreenPoint(_firstEnergyLocation);

                    var radius = _characterProximity.Radius * Settings.ObstacleCircleSizeMultiplier;
                    _obstacleAlert.SetBounds(
                        energyPositionInScreenSpace.X - radius,
                        energyPositionInScreenSpace.Y - radius,
                        radius * 2,
                        radius * 2);

                    if (_timeSinceLastBlinkToggle.Value >= Settings.BlinkPeriodSeconds)
                    {
                        _blinkToggle = !_blinkToggle;
                        _timeSinceLastBlinkToggle = 0;
                        _obstacleAlert.SetIsVisible(_blinkToggle);
                    }
                }
            }
            
            if (_currentState == State.EnergyDecay1 || _currentState == State.EnergyDecay2)
            {
                var currentScale = _energyDisplay.GetWidth() / (_characterProximity.Radius * 2); 
                var potentialNewScale = currentScale - Settings.EnergyDecayPercentLossPerSecond * Time.DeltaTime;
                _energyDisplaySizeMultiplier = potentialNewScale >= Settings.EnergyDecayMinimumScale ? potentialNewScale : 1f;
                
                var boatPosition = Entity.Scene.Camera.WorldToScreenPoint(_characterProximity.Entity.Position);
                _energyDisplay.SetBounds(
                    boatPosition.X - _characterProximity.Radius * _energyDisplaySizeMultiplier,
                    boatPosition.Y - _characterProximity.Radius * _energyDisplaySizeMultiplier,
                    2 * _characterProximity.Radius * _energyDisplaySizeMultiplier,
                    2 * _characterProximity.Radius * _energyDisplaySizeMultiplier);
            }
            else if (_currentState == State.FirstPlay)
            {
                if (Vector2.Distance(_characterProximity.Entity.Position, _firstRockLocation) <=
                    Settings.ObstacleAlertRadius.Value)
                {
                    OnNavigation();
                }
            }
            else if (_currentState == State.SecondPlay)
            {
                if (Vector2.Distance(_characterProximity.Entity.Position, _firstEnergyLocation) <=
                    Settings.ObstacleAlertRadius.Value)
                {
                    OnNavigation();
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
            return CreateSolidColorTexture(width, height, Settings.HighlightDisplayBackgroundTextureColor);
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

        private Texture2D CreateCircleDisplayTexture(float iRadius, float iLineThickness)
        {
            var radiusInt = (int)iRadius;

            var diameter = 2 * radiusInt;

            var center = new Vector2(radiusInt);
            var textureData = new Color[diameter * diameter];
            for (var ii = 0; ii < diameter; ii++)
            for (var jj = 0; jj < diameter; jj++)
            {
                var thisCoordinate = new Vector2(ii, jj);

                var distance = Math.Abs((center - thisCoordinate).Length());

                var diff = iRadius - distance;

                if (0 < diff && diff < iLineThickness)
                    textureData[jj * diameter + ii] = Settings.HighlightDisplayBackgroundTextureColor;
                else
                    textureData[jj * diameter + ii] = Color.Transparent;
            }

            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, diameter, diameter);
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
                case State.EnergyDisplay:
                    _energyDisplay.SetIsVisible(false);
                    _timeSinceLastBlinkToggle = null;
                    break;
                case State.EnergyDecay2:
                    _energyDisplay.SetIsVisible(false);
                    var radius = _characterProximity.Radius;
                    var boatPosition = Entity.Scene.Camera.WorldToScreenPoint(_characterProximity.Entity.Position);
                    _energyDisplay.SetBounds(boatPosition.X - radius, boatPosition.Y - radius, radius * 2, radius * 2);
                    break;
                case State.TurningUp:
                case State.TurningDown:
                case State.Rowing4:
                    _timeSinceLastBlinkToggle = null;
                    InputOverride = null;
                    break;
                case State.Collision2:
                case State.Energy:
                    _obstacleAlert.SetIsVisible(false);
                    _timeSinceLastBlinkToggle = null;
                    break;
                case State.Welcome:
                case State.GameGoal1:
                case State.StoryIntro1:
                case State.StoryIntro2:
                case State.StoryIntro3:
                case State.EnergyDecay1:
                case State.Rowing1:
                case State.Rowing2:
                case State.Rowing3:
                case State.FirstPlayInstructions:
                case State.FirstPlay:
                case State.Collision1:
                case State.SecondPlayInstructions:
                case State.SecondPlay:
                case State.GoodLuck:
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
                case State.EnergyDisplay:
                    HandleEnergyDisplay();
                    break;
                case State.EnergyDecay1:
                    HandleEnergyDecay1();
                    break;
                case State.EnergyDecay2:
                    HandleGenericTextChange("Careful! The further you get, the faster\nthe energy will decay.");
                    break;
                case State.TurningUp:
                    HandleTurningUp();
                    break;
                case State.TurningDown:
                    HandleTurningDown();
                    break;
                case State.Rowing1:
                    HandleRowing1();
                    break;
                case State.Rowing2:
                    HandleGenericTextChange("After rowing, the row indicator will cycle\nthrough levels of power.");
                    break;
                case State.Rowing3:
                    HandleGenericTextChange("Rowing again on green yields the most power.\nWhite yields a little less power.");
                    break;
                case State.Rowing4:
                    HandleGenericTextChange("Yellow yields very little power.\nRed yields almost no power.");
                    break;
                case State.FirstPlayInstructions:
                    HandleFirstPlayInstruction();
                    break;
                case State.FirstPlay:
                    HandlePlay();
                    break;
                case State.Collision1:
                    HandleCollision1();
                    break;
                case State.Collision2:
                    HandleGenericTextChange("Careful! The further you get, the more\nenergy will be lost by collisions.");
                    break;
                case State.SecondPlayInstructions:
                    HandleSecondPlayInstruction();
                    break;
                case State.SecondPlay:
                    HandlePlay();
                    break;
                case State.Energy:
                    HandleEnergy();
                    break;
                case State.GoodLuck:
                    HandleGenericTextChange("And that's it! Press row to continue playing.\nGood luck!");
                    break;
                case State.EndPlay:
                    HandlePlay();
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

        private void HandleEnergyDisplay()
        {
            _blinkToggle = false;
            _timeSinceLastBlinkToggle = .5f;
            _energyDisplay.SetIsVisible(false);
            HandleGenericTextChange("Energy level is shown by the size of the\nbubble around the boat.");
        }

        private void HandleEnergyDecay1()
        {
            _timeSinceLastBlinkToggle = null;
            _energyDisplay.SetIsVisible(true);
            _energyDisplaySizeMultiplier = 1f;
            HandleGenericTextChange("Over time your energy level will decay,\nmaking the bubble smaller. If the bubble\nencloses around the boat, you lose.");
        }

        private void HandleTurningUp()
        {
            _blinkToggle = false;
            _timeSinceLastBlinkToggle = Settings.BlinkPeriodSeconds;

            if (Input.Touch.IsConnected)
            {
                HandleGenericTextChange("Press and hold this button to turn\ncounterclockwise.");
            }
            else if (Input.GamePads.Any(gp => gp.IsConnected()))
            {
                HandleGenericTextChange("Press up or left with the left stick\nto turn counterclockwise.");
            }
            else
            {
                HandleGenericTextChange("Press W or A with the keyboard to turn\ncounterclockwise.");
            }
        }

        private void HandleTurningDown()
        {
            _blinkToggle = false;
            _timeSinceLastBlinkToggle = Settings.BlinkPeriodSeconds;

            if (Input.Touch.IsConnected)
            {
                HandleGenericTextChange("Press and hold this button to turn\nclockwise.");
            }
            else if (Input.GamePads.Any(gp => gp.IsConnected()))
            {
                HandleGenericTextChange("Press down or right with the left stick\nto turn clockwise.");
            }
            else
            {
                HandleGenericTextChange("Press S or D with the keyboard to turn\nclockwise.");
            }
        }

        private void HandleRowing1()
        {
            _timeSinceLastBlinkToggle = Settings.RowPeriodSeconds;

            if (Input.Touch.IsConnected)
            {
                HandleGenericTextChange("Tap anywhere on the right half of the\nscreen to row.");
            }
            else if (Input.GamePads.Any(gp => gp.IsConnected()))
            {
                HandleGenericTextChange("Tap the A/X face button to row.");
            }
            else
            {
                HandleGenericTextChange("Press left click or space to row.");
            }
        }

        private void HandleFirstPlayInstruction()
        {
            if (Input.Touch.IsConnected)
            {
                HandleGenericTextChange("Why don't you give it a try.\nTap the screen to begin.");
            }
            else if (Input.GamePads.Any(gp => gp.IsConnected()))
            {
                HandleGenericTextChange("Why don't you give it a try.\nPress the A/X face button to begin.");
            }
            else
            {
                HandleGenericTextChange("Why don't you give it a try.\nPress left click or space to begin.");
            }
        }

        private void HandleCollision1()
        {
            _timeSinceLastBlinkToggle = 0;
            _obstacleAlert.SetIsVisible(true);
            HandleGenericTextChange("You lose energy when colliding with rocks\nand the shoreline. The faster your speed\nat impact, the more energy is lost.");
        }

        private void HandleSecondPlayInstruction()
        {
            if (Input.Touch.IsConnected)
            {
                HandleGenericTextChange("Tap the screen to continue.");
            }
            else if (Input.GamePads.Any(gp => gp.IsConnected()))
            {
                HandleGenericTextChange("Press the A/X face button to continue.");
            }
            else
            {
                HandleGenericTextChange("Press left click or space to continue.");
            }
        }

        private void HandleEnergy()
        {
            _timeSinceLastBlinkToggle = 0;
            _obstacleAlert.SetIsVisible(true);
            HandleGenericTextChange("Pick up this energy to give yourself more time!");
        }

        private void HandlePlay()
        {
            SetPauseState(false);
            _textBox.SetIsVisible(false);
        }
    }
}
