using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
using Nez.UI;

namespace Raspberry_Lib.Components
{
    internal class PlayUiCanvasComponent : Component, IUpdatable, IBeginPlay
    {
        private static class Settings
        {
            public static readonly RenderSetting DistanceToMetersFactor = new(40);
            public static readonly RenderSetting Margin = new(100);
            public const int FontScale = 6;

            public static readonly RenderSetting IndicatorSizeX = new(150);
            public static readonly RenderSetting IndicatorSizeY = new(150);

            public const float RowTransition1 = .5f;
            public const float RowTransition2 = .9f;
            public const float RowTransition3 = 1.25f;
        }

        public PlayUiCanvasComponent()
        {
            _upPressed = false;
            _downPressed = false;
            _rowColor = RowColor.White;
        }

        public override void OnAddedToEntity()
        {
            var canvas = Entity.AddComponent(new UICanvas());
            _distanceLabel = canvas.Stage.AddElement(new Label("0 m"));
            _distanceLabel.SetPosition(Screen.Width / 2f, Settings.Margin.Value);
            _distanceLabel.SetFontScale(Settings.FontScale);

            var textureAtlas = Entity.Scene.Content.LoadTexture(Content.ContentData.AssetPaths.IconsTileset, true);
            var spriteList = Sprite.SpritesFromAtlas(textureAtlas, 32, 32);

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
            
            _upIndicator = canvas.Stage.AddElement(new Image(_upDefaultIcon));
            _upIndicator.SetPosition(Settings.Margin.Value, Screen.Height * .15f - Settings.IndicatorSizeY.Value / 2f);
            _upIndicator.SetSize(Settings.IndicatorSizeX.Value, Settings.IndicatorSizeY.Value);
            _upIndicator.SetScaling(Scaling.Fill);
            _upIndicator.SetColor(drawColor);

            _downIndicator = canvas.Stage.AddElement(new Image(_downDefaultIcon));
            _downIndicator.SetPosition(Settings.Margin.Value, Screen.Height * .85f - Settings.IndicatorSizeY.Value / 2f);
            _downIndicator.SetSize(Settings.IndicatorSizeX.Value, Settings.IndicatorSizeY.Value);
            _downIndicator.SetScaling(Scaling.Fill);
            _downIndicator.SetColor(drawColor);

            _rowIndicator = canvas.Stage.AddElement(new Image(spriteList[4]));
            _rowIndicator.SetPosition(Settings.Margin.Value, Screen.Height * .5f - Settings.IndicatorSizeY.Value / 2f);
            _rowIndicator.SetSize(Settings.IndicatorSizeX.Value, Settings.IndicatorSizeY.Value);
            _rowIndicator.SetScaling(Scaling.Fill);
            _rowIndicator.SetColor(drawColor);

            // This needs to match the render layer of the ScreenSpaceRenderer in SceneBase ctor
            canvas.SetRenderLayer(-1);
        }

        public int BeginPlayOrder => 97;
        public void OnBeginPlay()
        {
            _movementComponent = Entity.Scene.FindEntity("character").GetComponent<CharacterMovementComponent>();

            System.Diagnostics.Debug.Assert(_movementComponent != null);
        }

        public void Update()
        {
            if (_movementComponent == null)
                return;

            // Handle Distance Display
            var distanceTraveled = (int)Mathf.Round(_movementComponent.TotalDistanceTraveled / Settings.DistanceToMetersFactor.Value);
            _distanceLabel.SetText($"{distanceTraveled} m");

            // Handle Turning Indicators
            var input = _movementComponent.CurrentInput;

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
            var rowTime = _movementComponent.LastRowTimeSecond;
            var timeDiff = Time.TotalTime - rowTime;

            if (timeDiff < Settings.RowTransition1)
            {
                if (_rowColor != RowColor.Red)
                {
                    _rowIndicator.SetDrawable(_rowRedIcon);
                    _rowColor = RowColor.Red;
                }
            }
            else if (timeDiff < Settings.RowTransition2)
            {
                if (_rowColor != RowColor.Yellow)
                {
                    _rowIndicator.SetDrawable(_rowYellowIcon);
                    _rowColor = RowColor.Yellow;
                }
            }
            else if (timeDiff < Settings.RowTransition3)
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

        private enum RowColor
        {
            White,
            Red,
            Yellow,
            Green
        }

        private CharacterMovementComponent _movementComponent;
        private Label _distanceLabel;

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
    }
}
