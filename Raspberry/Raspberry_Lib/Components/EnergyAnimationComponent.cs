using System;
using Nez;
using Nez.Textures;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raspberry_Lib.Components
{
    internal class EnergyAnimationComponent: PausableRenderableComponent
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

            public const int ParticleTextureSize = 6;
            public const int NumCollisionParticles = 8;

            public const float AngleVarianceAsPercentageOfDTheta = .25f;

            public static readonly RenderSetting BlastParticleVelocity = new(400);
            public const float BlastDurationSeconds = .5f;
            public const float HoldDurationSeconds = .5f;
            public static readonly RenderSetting ChaseParticleVelocity = new(800);
            public static readonly RenderSetting DistanceToPlayerTolerance = new(5);
        }

        public EnergyAnimationComponent(int iColorIndex)
        {
            RenderLayer = 4;

            _energySprites = new List<Sprite>();
            _spriteEffect = SpriteEffects.None;

            _indexTracker = 0f;
            _isIndexIncreasing = true;

            var colorIndexToUse = Math.Abs(iColorIndex + 1);
            _colorToUse = Settings.Colors[colorIndexToUse];

            var texture = CreateParticleTexture();
            _particleSprite = new Sprite(texture);

            _collisionParticles = new List<CollisionParticle>();
            _hasCollidedWithPlayer = false;

            _rng = new System.Random();
        }

        public override void OnAddedToEntity()
        {
            var textureAtlas = Entity.Scene.Content.LoadTexture(Content.ContentData.AssetPaths.ObjectsTileset, true);

            _energySprites.AddRange(new[]
            {
                new Sprite(textureAtlas, new Rectangle(144, 72, 36, 36)),
                new Sprite(textureAtlas, new Rectangle(180, 72, 36, 36)),
                new Sprite(textureAtlas, new Rectangle(216, 72, 36, 36)),
                new Sprite(textureAtlas, new Rectangle(252, 72, 36, 36)),
                new Sprite(textureAtlas, new Rectangle(144, 108, 36, 36)),

            });

            _amplitude = _energySprites.Count - 1;
            _currentSprite = _energySprites[0];

            var angle = 0f;
            const float dTheta = MathHelper.Pi / Settings.NumCollisionParticles;
            const float thetaVariance = dTheta * Settings.AngleVarianceAsPercentageOfDTheta;
            for (var ii = 0; ii < Settings.NumCollisionParticles; ii++)
            {
                var angleVariance = (angle - thetaVariance) + ((float)_rng.NextDouble() * 2 * thetaVariance);
                var trueAngle = angle + angleVariance;
                var velocityDirection = new Vector2((float)Math.Cos(trueAngle), (float)Math.Sin(trueAngle));
                var velocity = Settings.BlastParticleVelocity.Value * velocityDirection;

                _collisionParticles.Add(new CollisionParticle(_particleSprite, Vector2.Zero, velocity));

                angle += dTheta;
            }

            _playerEntity = Entity.Scene.FindEntity("character");

            System.Diagnostics.Debug.Assert(_playerEntity != null);
        }

        protected override void OnUpdate(float iTotalPlayableTime)
        {
            if (!_hasCollidedWithPlayer)
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

                _currentSprite = _energySprites[index];
            }
            else
            {
                var timeSinceCollision = iTotalPlayableTime - _collisionTime;

                if (timeSinceCollision <= Settings.BlastDurationSeconds)
                {
                    foreach (var collisionParticle in _collisionParticles)
                    {
                        collisionParticle.Velocity = collisionParticle.InitialVelocity;
                    }
                }
                else if (timeSinceCollision <= Settings.BlastDurationSeconds + Settings.HoldDurationSeconds)
                {
                    foreach (var collisionParticle in _collisionParticles)
                    {
                        collisionParticle.Velocity = Vector2.Zero;
                    }
                }
                else
                {
                    foreach (var collisionParticle in _collisionParticles)
                    {
                        if (!collisionParticle.IsChasingPlayer)
                            continue;

                        var diffVector = _playerEntity.Position - Entity.Position - LocalOffset - collisionParticle.PositionOffset;

                        if (diffVector.Length() >= Settings.DistanceToPlayerTolerance.Value)
                        {
                            diffVector.Normalize();
                            collisionParticle.Velocity = diffVector * Settings.ChaseParticleVelocity.Value;
                        }
                        else
                        {
                            collisionParticle.Velocity = Vector2.Zero;
                            collisionParticle.IsChasingPlayer = false;
                        }
                    }
                }

                foreach (var collisionParticle in _collisionParticles)
                {
                    collisionParticle.PositionOffset += collisionParticle.Velocity * Time.DeltaTime;
                }
            }
        }

        public override float Width => 60000f;
        public override float Height => 60000f;

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            if (!_hasCollidedWithPlayer)
            {
                iBatcher.Draw(_currentSprite, Entity.Transform.Position + LocalOffset, _colorToUse,
                    Entity.Transform.Rotation, _currentSprite.Origin, Entity.Transform.Scale, _spriteEffect,
                    _layerDepth);
            }
            else
            {
                foreach (var collisionParticle in _collisionParticles.Where(cp => cp.IsChasingPlayer))
                {
                    iBatcher.Draw(collisionParticle.SpriteToUse, Entity.Transform.Position + LocalOffset + collisionParticle.PositionOffset, _colorToUse,
                        Entity.Transform.Rotation, _currentSprite.Origin, Entity.Transform.Scale, SpriteEffects.None,
                        _layerDepth);
                }
            }
        }

        public void OnPlayerHit()
        {
            _hasCollidedWithPlayer = true;
            _collisionTime = Time.TotalTime - TimeSpentPaused;
        }

        private class CollisionParticle
        {
            public CollisionParticle(Sprite iSprite, Vector2 iPositionOffset, Vector2 iVelocity)
            {
                SpriteToUse = iSprite;
                PositionOffset = iPositionOffset;
                InitialVelocity = iVelocity;
                Velocity = iVelocity;
                IsChasingPlayer = true;
            }

            public Sprite SpriteToUse { get; }
            public Vector2 PositionOffset { get; set; }
            public Vector2 InitialVelocity { get; }
            public Vector2 Velocity { get; set; }
            public bool IsChasingPlayer { get; set; }
        }

        private Entity _playerEntity;

        private readonly List<Sprite> _energySprites;
        private Sprite _currentSprite;
        private SpriteEffects _spriteEffect;

        private int _amplitude;
        private float _indexTracker;
        private bool _isIndexIncreasing;

        private readonly Color _colorToUse;

        private readonly Sprite _particleSprite;
        private readonly List<CollisionParticle> _collisionParticles;

        private bool _hasCollidedWithPlayer;
        private readonly System.Random _rng;

        private float _collisionTime;

        private Texture2D CreateParticleTexture()
        {
            var textureData = new Color[Settings.ParticleTextureSize * Settings.ParticleTextureSize];

            for (var jj = 0; jj < Settings.ParticleTextureSize; jj++)
            for(var ii = 0; ii < Settings.ParticleTextureSize; ii++)
            {
                var index = Settings.ParticleTextureSize * jj + ii;

                if (jj == 0 || jj == Settings.ParticleTextureSize - 1 ||
                    ii == 0 || ii == Settings.ParticleTextureSize - 1)
                {
                    textureData[index] = Color.Black;
                }
                else
                {
                    textureData[index] = Color.White;
                }
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, Settings.ParticleTextureSize, Settings.ParticleTextureSize);
            texture.SetData(textureData);

            return texture;
        }
    }
}
