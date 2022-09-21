using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class CharacterAnimationComponent : RenderableComponent, IUpdatable
    {
        private static class Settings
        {
            public const int CellWidth = 80;
            public const int CellHeight = 30;

            public const float Amplitude = 11.49f;
            public const float WaveLengthSec = 2f;
        }

        public CharacterAnimationComponent()
        {
            RenderLayer = 4;
            _omega = MathHelper.TwoPi / Settings.WaveLengthSec;
        }

        public override void OnAddedToEntity()
        {
            var texture = Entity.Scene.Content.LoadTexture(Content.ContentData.AssetPaths.CharacterSpriteSheet);
            _sprites = Sprite.SpritesFromAtlas(texture, Settings.CellWidth, Settings.CellHeight);

            _currentSprite = _sprites[0];
            _spriteEffect = SpriteEffects.None;

            //_movementComponent = Entity.GetComponent<CharacterMovementComponent>();
        }

        public void Update()
        {
            //var input = _movementComponent.CurrentInput;

            var rawVal = DampedOscillationFunction(Time.TotalTime);
            var index = (int)Math.Round(rawVal);

            if (index >= 0)
            {
                _currentSprite = _sprites[index];
                _spriteEffect = SpriteEffects.None;
            }
            else
            {
                _currentSprite = _sprites[Math.Abs(index)];
                _spriteEffect = SpriteEffects.FlipVertically;
            }
        }

        public override float Width => 600f;
        public override float Height => 600f;

        public override void Render(Batcher batcher, Camera camera)
        {
            batcher.Draw(_currentSprite, Entity.Transform.Position + LocalOffset, Color,
                Entity.Transform.Rotation, _currentSprite.Origin, Entity.Transform.Scale, _spriteEffect, _layerDepth);
        }

        private List<Sprite> _sprites;
        private Sprite _currentSprite;
        private SpriteEffects _spriteEffect;
        private readonly float _omega;

        //private CharacterMovementComponent _movementComponent;

        private float DampedOscillationFunction(float iTimeSec)
        {
            var value = Settings.Amplitude * (float)Math.Cos(iTimeSec * _omega);
            return value;
        }
    }
}