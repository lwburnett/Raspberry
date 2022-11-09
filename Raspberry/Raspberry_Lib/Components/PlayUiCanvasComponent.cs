﻿using Microsoft.Xna.Framework;
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
            public static readonly RenderSetting DistanceLabelWidth = new(200);
            public static readonly RenderSetting DistanceLabelHeight = new(75);

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
            OnAddedToEntityInternal();
        }

        public int BeginPlayOrder => 97;
        public void OnBeginPlay()
        {
            BeginPlayInternal();
        }

        public void Update()
        {
            UpdateInternal();
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

        protected UICanvas Canvas;

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

        protected virtual void OnAddedToEntityInternal()
        {
            Canvas = Entity.AddComponent(new UICanvas());
            DistanceLabel = Canvas.Stage.AddElement(new Label("0 m"));
            DistanceLabel.SetBounds(
                (Screen.Width - Settings.DistanceLabelWidth.Value) / 2f, 
                Settings.Margin.Value / 2f,
                Settings.DistanceLabelWidth.Value,
                Settings.DistanceLabelHeight.Value);
            DistanceLabel.SetFontScale(Settings.FontScale);

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
                Screen.Height * .15f - Settings.IndicatorSizeY.Value / 2f,
                Settings.IndicatorSizeX.Value, 
                Settings.IndicatorSizeY.Value);
            _upIndicator.SetScaling(Scaling.Fill);
            _upIndicator.SetColor(drawColor);

            _downIndicator = Canvas.Stage.AddElement(new Image(_downDefaultIcon));
            _downIndicator.SetBounds(
                Settings.Margin.Value, 
                Screen.Height * .85f - Settings.IndicatorSizeY.Value / 2f,
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

            // This needs to match the render layer of the ScreenSpaceRenderer in SceneBase ctor
            Canvas.SetRenderLayer(-1);
        }

        protected virtual void UpdateInternal()
        {
            if (MovementComponent == null)
                return;

            // Handle Distance Display
            var distanceTraveled = (int)Mathf.Round(MovementComponent.TotalDistanceTraveled / Settings.DistanceToMetersFactor.Value);
            DistanceLabel.SetText($"{distanceTraveled} m");

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
    }
}
