using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
using System;
using System.Collections.Generic;

namespace Raspberry_Lib.Components
{
    internal class OarPairComponent : RenderableComponent, IUpdatable
    {
        private static class Settings
        {
            public const int MaxNumParticles = 150;
            public const int TextureSize = 2;

            public const byte TextureAlphaStart = 255;
            public const byte TextureAlphaEnd = 0;

            //public const float RowDurationSec = 1f;
            public const float ParticleTtl = 2f;

            public const float RowTransition1 = .5f;
            public const float RowTransition2 = .9f;
            public const float RowTransition3 = 1.25f;

            //public const float RowSpeedBadAsPercentOfCurrentSpeed = 0.0f;
            public const float RowSpeedMediumAsPercentOfCurrentSpeed = -0.25f;
            public const float RowSpeedGoodAsPercentOfCurrentSpeed = -.75f;
            public const float RowSpeedNeutralAsPercentOfCurrentSpeed = -0.50f;

            public static readonly RenderSetting RowStartPerpendicularPosition = new(30);
            public static readonly RenderSetting RowStartParticleRadius = new(3);

            //public static readonly RenderSetting RowParticleRadiusChangePerSecond = new(10);

            public const int NumParticles = 12;
        }

        private class OarParticle
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public float SpawnTime { get; set; }
            public float TimeToLive { get; set; }
            public Vector2 DeltaVelocityPerFrame { get; set; }
            public byte ColorAlpha { get; set; }
        }

        public override float Width => 2000;
        public override float Height => 2000;

        public OarPairComponent()
        {
            Pool<OarParticle>.WarmCache(Settings.MaxNumParticles);

            var textureData = new Color[Settings.TextureSize * Settings.TextureSize];
            for (var ii = 0; ii < Settings.TextureSize * Settings.TextureSize; ii++)
            {
                textureData[ii] = Color.White;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, Settings.TextureSize, Settings.TextureSize);
            texture.SetData(textureData);
            _sprite = new Sprite(texture);
            _particles = new List<OarParticle>();
        }

        public override void OnAddedToEntity()
        {

            _proximityComponent = Entity.GetComponent<PlayerProximityComponent>();
            _movementComponent = Entity.GetComponent<CharacterMovementComponent>();
        }

        public void Update()
        {
            // Update current list of particles
            for (var ii = _particles.Count - 1; ii >= 0; ii--)
            {
                var thisParticle = _particles[ii];

                if (Time.TotalTime - thisParticle.SpawnTime > thisParticle.TimeToLive)
                {
                    Pool<OarParticle>.Free(thisParticle);
                    _particles.RemoveAt(ii);
                    continue;
                }

                if (Time.TotalTime > thisParticle.SpawnTime + thisParticle.TimeToLive)
                    thisParticle.Velocity += thisParticle.DeltaVelocityPerFrame * Time.DeltaTime;

                thisParticle.Position += thisParticle.Velocity * Time.DeltaTime;

                var lerpValue = (Time.TotalTime - thisParticle.SpawnTime) / thisParticle.TimeToLive;
                thisParticle.ColorAlpha = (byte)MathHelper.Lerp(Settings.TextureAlphaStart, Settings.TextureAlphaEnd, lerpValue);
            }

            // Spawn more row particles if need be
            var input = _movementComponent.CurrentInput;
            if (input.Row)
            {
                var timeDiff = Time.TotalTime - _movementComponent.LastRowTimeSecond;
                float? rowVelocityAsPercent;
                if (timeDiff < Settings.RowTransition1)
                    rowVelocityAsPercent = null;
                else if (timeDiff < Settings.RowTransition2)
                    rowVelocityAsPercent = Settings.RowSpeedMediumAsPercentOfCurrentSpeed;
                else if (timeDiff < Settings.RowTransition3)
                    rowVelocityAsPercent = Settings.RowSpeedGoodAsPercentOfCurrentSpeed;
                else
                    rowVelocityAsPercent = Settings.RowSpeedNeutralAsPercentOfCurrentSpeed;

                if (rowVelocityAsPercent.HasValue)
                {
                    var rowVelocity = rowVelocityAsPercent.Value * _movementComponent.CurrentVelocity;

                    var parallelDirection = GetRotationAsDirectionVector();
                    parallelDirection.Normalize();
                    var orthogonalDirection = new Vector2(-parallelDirection.Y, parallelDirection.X);
                    orthogonalDirection.Normalize();
                    var dTheta = MathHelper.Pi / Settings.NumParticles;
                    var radius = Settings.RowStartParticleRadius.Value;
                    if (input.Rotation <= 0f)
                    {
                        var centerPointLeft = Entity.Position +
                                               orthogonalDirection * Settings.RowStartPerpendicularPosition.Value;

                        for (var ii = 0; ii < Settings.NumParticles; ii++)
                        {
                            var thisParticle = Pool<OarParticle>.Obtain();
                            var angle = dTheta * ii;
                            thisParticle.Position = 
                                centerPointLeft + radius * 
                                ((float)Math.Cos(angle) * orthogonalDirection + (float)Math.Sin(angle) * parallelDirection);

                            thisParticle.Velocity = rowVelocity;
                            thisParticle.SpawnTime = Time.TotalTime;
                            thisParticle.TimeToLive = Settings.ParticleTtl;
                            thisParticle.DeltaVelocityPerFrame = Vector2.Zero;
                            thisParticle.ColorAlpha = Settings.TextureAlphaStart;

                            _particles.Add(thisParticle);
                        }
                    }

                    if (input.Rotation >= 0f)
                    {
                        var centerPointRight = Entity.Position -
                                               orthogonalDirection * Settings.RowStartPerpendicularPosition.Value;

                        for (var ii = 0; ii < Settings.NumParticles; ii++)
                        {
                            var thisParticle = Pool<OarParticle>.Obtain();
                            var angle = dTheta * ii;
                            thisParticle.Position =
                                centerPointRight + radius *
                                ((float)Math.Cos(angle) * orthogonalDirection + (float)Math.Sin(angle) * parallelDirection);

                            thisParticle.Velocity = rowVelocity;
                            thisParticle.SpawnTime = Time.TotalTime;
                            thisParticle.TimeToLive = Settings.ParticleTtl;
                            thisParticle.DeltaVelocityPerFrame = Vector2.Zero;
                            thisParticle.ColorAlpha = Settings.TextureAlphaStart;

                            _particles.Add(thisParticle);
                        }
                    }
                }
            }
        }

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            foreach (var oarParticle in _particles)
            {
                var thisPos = oarParticle.Position;

                if (Vector2.Distance(thisPos, Entity.Position) < _proximityComponent.Radius)
                {
                    var thisColor = Color.White * (oarParticle.ColorAlpha / 255f);

                    iBatcher.Draw(
                        _sprite,
                        thisPos,
                        thisColor,
                        0f,
                        _sprite.Origin,
                        Entity.Transform.Scale,
                        SpriteEffects.None,
                        0);
                }
            }
        }

        private readonly Sprite _sprite;
        private readonly List<OarParticle> _particles;
        private CharacterMovementComponent _movementComponent;
        private PlayerProximityComponent _proximityComponent;

        private Vector2 GetRotationAsDirectionVector()
        {
            var rotation = Entity.Transform.Rotation;

            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }
    }
}
