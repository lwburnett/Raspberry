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
            public static readonly RenderSetting CellPadding = new(15);
            public static readonly RenderSetting BackgroundTextureWidth = new(1500);
            public static readonly RenderSetting BackgroundTextureHeight = new(200);
            public static readonly Color BackgroundTextureColor = new(112, 128, 144, 200);
        }

        public enum State
        {
            Welcome = 0,
            EndPlay
        }

        public TutorialUiCanvasComponent()
        {
            _currentState = 0;
        }

        private State _currentState;
        private PlayerProximityComponent _characterProximity;
        private CharacterInputController _characterInputController;
        private TutorialNavigationInputController _navigationInputController;

        private Table _textBox;
        private Label _text;

        protected sealed override void OnAddedToEntityInternal()
        {
            base.OnAddedToEntityInternal();
            
            _navigationInputController = Entity.AddComponent(new TutorialNavigationInputController(OnNavigation));

            var textureWidthPixels = (int)Math.Round(Settings.BackgroundTextureWidth.Value);
            var textureHeightPixels = (int)Math.Round(Settings.BackgroundTextureHeight.Value);
            var textureData = new Color[textureWidthPixels * textureHeightPixels];
            for (var ii = 0; ii < textureWidthPixels * textureHeightPixels; ii++)
            {
                textureData[ii] = Settings.BackgroundTextureColor;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, textureWidthPixels, textureHeightPixels);
            texture.SetData(textureData);
            var spriteDrawable = new SpriteDrawable(texture);

            _textBox = Canvas.Stage.AddElement(new Table());
            _textBox.SetSize(Settings.BackgroundTextureWidth.Value, Settings.BackgroundTextureHeight.Value);
            _textBox.SetBounds(
                (Screen.Width - Settings.BackgroundTextureWidth.Value) / 2f, 
                Screen.Height - Settings.MarginBottom.Value - Settings.BackgroundTextureHeight.Value,
                Settings.BackgroundTextureWidth.Value,
                Settings.BackgroundTextureHeight.Value);
            _textBox.SetBackground(spriteDrawable);
            _text = new Label("Test message 123").SetFontScale(5).SetFontColor(Color.White).SetAlignment(Align.TopLeft);
            _textBox.Add(_text).Pad(Settings.CellPadding.Value);
        }

        protected sealed override void BeginPlayInternal()
        {
            base.BeginPlayInternal();

            var characterEntity = Entity.Scene.FindEntity("character");
            _characterProximity = characterEntity.GetComponent<PlayerProximityComponent>();
            _characterInputController = characterEntity.GetComponent<CharacterInputController>();

            System.Diagnostics.Debug.Assert(_characterProximity != null);

            OnNavigationChanged();
        }

        // protected sealed override void UpdateInternal()
        // {
        //     base.UpdateInternal();
        //
        //
        // }

        private void OnNavigation()
        {
            _currentState++;
            OnNavigationChanged();
        }

        private void OnNavigationChanged()
        {
            void SetPauseStuff(bool iValue)
            {
                MovementComponent.IsPaused = iValue;
                _characterProximity.IsPaused = iValue;
                _characterInputController.IsPaused = iValue;
                _navigationInputController.IsPaused = !iValue;
            }

            switch (_currentState)
            {
                case State.Welcome:
                    SetPauseStuff(true);
                    _textBox.SetIsVisible(true);
                    break;
                case State.EndPlay:
                    SetPauseStuff(false);
                    _textBox.SetIsVisible(false);
                    break;
                default:
                    System.Diagnostics.Debug.Fail($"Unknown type of {nameof(State)}");
                    return;
            }
        }
    }
}
