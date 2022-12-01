using System;
using Nez;
using Nez.Textures;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raspberry_Lib.Components
{
    internal class EnergyAnimationComponent: RenderableComponent, IUpdatable
    {
        private static class Settings
        {
            public const float Frequency = 8f;

            public static readonly List<Color> Colors = new()
            {
                Color.LightYellow,
                Color.Orange,
                Color.Violet,
                Color.Aqua,
                Color.Salmon
            };
        }

        public EnergyAnimationComponent(int iColorIndex)
        {
            RenderLayer = 4;

            _sprites = new List<Sprite>();
            _spriteEffect = SpriteEffects.None;

            _indexTracker = 0f;
            _isIndexIncreasing = true;

            var colorIndexToUse = Math.Abs(iColorIndex + 1);
            _colorToUse = Settings.Colors[colorIndexToUse];
        }

        public override void OnAddedToEntity()
        {
            var textureAtlas = Entity.Scene.Content.LoadTexture(Content.ContentData.AssetPaths.ObjectsTileset, true);

            _sprites.AddRange(new[]
            {
                new Sprite(textureAtlas, new Rectangle(144, 72, 36, 36)),
                new Sprite(textureAtlas, new Rectangle(180, 72, 36, 36)),
                new Sprite(textureAtlas, new Rectangle(216, 72, 36, 36)),
                new Sprite(textureAtlas, new Rectangle(252, 72, 36, 36)),
                new Sprite(textureAtlas, new Rectangle(144, 108, 36, 36)),

            });

            _amplitude = _sprites.Count - 1;

            _currentSprite = _sprites[0];
        }

        public void Update()
        {
            var step = Settings.Frequency * Time.DeltaTime;

            int index;
            if (_isIndexIncreasing)
            {
                var potentialIndex = _indexTracker + step;

                if (potentialIndex <= _amplitude)
                {
                    _indexTracker = potentialIndex;
                    index = (int)Math.Round(Math.Abs(potentialIndex));
                }
                else
                {
                    _indexTracker = _amplitude;
                    index = _amplitude;
                    _isIndexIncreasing = false;
                }
            }
            else
            {
                var potentialIndex = _indexTracker - step;

                if (potentialIndex >= -_amplitude)
                {
                    _indexTracker = potentialIndex;
                    index = (int)Math.Round(Math.Abs(potentialIndex));
                }
                else
                {
                    _indexTracker = -_amplitude;
                    index = _amplitude;
                    _isIndexIncreasing = true;
                }
            }

            if (_indexTracker >= 0f && _isIndexIncreasing)
            {
                _spriteEffect = SpriteEffects.None;
            }
            else if (_indexTracker >= 0f && !_isIndexIncreasing)
            {
                _spriteEffect = SpriteEffects.FlipHorizontally;
            }
            else if (_indexTracker < 0f && !_isIndexIncreasing)
            {
                _spriteEffect = SpriteEffects.FlipVertically;
            }
            else
            {
                _spriteEffect = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }

            _currentSprite = _sprites[index];
        }

        public override float Width => 600f;
        public override float Height => 600f;

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            iBatcher.Draw(_currentSprite, Entity.Transform.Position + LocalOffset, _colorToUse,
                Entity.Transform.Rotation, _currentSprite.Origin, Entity.Transform.Scale, _spriteEffect, _layerDepth);
        }

        private readonly List<Sprite> _sprites;
        private Sprite _currentSprite;
        private SpriteEffects _spriteEffect;

        private int _amplitude;
        private float _indexTracker;
        private bool _isIndexIncreasing;

        private readonly Color _colorToUse;
    }
}
