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
        }

        public override void OnAddedToEntity()
        {
            var canvas = Entity.AddComponent(new UICanvas());
            _distanceLabel = canvas.Stage.AddElement(new Label("0 m"));
            _distanceLabel.SetPosition(Screen.Width / 2f, Settings.Margin.Value);
            _distanceLabel.SetFontScale(Settings.FontScale);

            var textureAtlas = Entity.Scene.Content.LoadTexture(Content.ContentData.AssetPaths.LevelTileset, true);
            var spriteList = Sprite.SpritesFromAtlas(textureAtlas, 32, 32);

            _upDefaultIcon = new SpriteDrawable(spriteList[8]);
            _upPressedIcon = new SpriteDrawable(spriteList[9]);

            _downDefaultIcon = new SpriteDrawable(spriteList[12]);
            _downPressedIcon = new SpriteDrawable(spriteList[13]);

            var drawColor = Color.White;
            drawColor.A = 127;
            
            _upIndicator = canvas.Stage.AddElement(new Image(_upDefaultIcon));
            _upIndicator.SetPosition(Settings.Margin.Value, Screen.Height * .25f);
            _upIndicator.SetSize(Settings.IndicatorSizeX.Value, Settings.IndicatorSizeY.Value);
            _upIndicator.SetScaling(Scaling.Fill);
            _upIndicator.SetColor(drawColor);

            _downIndicator = canvas.Stage.AddElement(new Image(_downDefaultIcon));
            _downIndicator.SetPosition(Settings.Margin.Value, Screen.Height * .75f);
            _downIndicator.SetSize(Settings.IndicatorSizeX.Value, Settings.IndicatorSizeY.Value);
            _downIndicator.SetScaling(Scaling.Fill);
            _downIndicator.SetColor(drawColor);

            _rowIndicator = canvas.Stage.AddElement(new Image(spriteList[4]));
            _rowIndicator.SetPosition(Screen.Width - Settings.Margin.Value * 2, Screen.Height * .5f);
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

            var distanceTraveled = (int)Mathf.Round(_movementComponent.TotalDistanceTraveled / Settings.DistanceToMetersFactor.Value);
            _distanceLabel.SetText($"{distanceTraveled} m");

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
    }
}
