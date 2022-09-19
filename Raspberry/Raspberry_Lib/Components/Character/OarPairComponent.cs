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

            public const float RowDurationSec = 1f;
            public const float ParticleTtl = 1f;

            public const float RowTransition1 = .5f;
            public const float RowTransition2 = .9f;
            public const float RowTransition3 = 1.25f;

            //public const float RowSpeedBadAsPercentOfCurrentSpeed = 0.0f;
            public const float RowSpeedMediumAsPercentOfCurrentSpeed = -0.25f;
            public const float RowSpeedGoodAsPercentOfCurrentSpeed = -0.75f;
            public const float RowSpeedNeutralAsPercentOfCurrentSpeed = -0.50f;

            public static readonly RenderSetting RowStartPerpendicularPosition = new(45);
            public static readonly RenderSetting RowStartParticleRadius = new(3);

            public static readonly RenderSetting RowParticleRadiusChangePerSecond = new(10);

            public const int NumParticles = 8;

            public const float OrthogonalEndPositionVariancePercentOfTtlStart = .25f;
        }

        private class RowGroup
        {
            public List<OarParticle> Particles { get; set; }
            public float SpawnTime { get; set; }
            public float RowDuration { get; set; }
            public float TimeToLive { get; set; }
            public float ColorAlpha { get; set; }
            public Vector2 PostRowVelocity { get; set; }
        }

        private class OarParticle
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public Vector2 DeltaVelocityPerFrame { get; set; }
        }

        private class FreeParticle : OarParticle
        {
            public float SpawnTime { get; set; }
            public float TimeToLive { get; set; }
            public byte ColorAlpha { get; set; }
        }

        public override float Width => 2000;
        public override float Height => 2000;

        public OarPairComponent()
        {
            Pool<FreeParticle>.WarmCache(Settings.MaxNumParticles);
            Pool<OarParticle>.WarmCache(Settings.MaxNumParticles);

            var textureData = new Color[Settings.TextureSize * Settings.TextureSize];
            for (var ii = 0; ii < Settings.TextureSize * Settings.TextureSize; ii++)
            {
                textureData[ii] = Color.White;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, Settings.TextureSize, Settings.TextureSize);
            texture.SetData(textureData);
            _sprite = new Sprite(texture);
            _freeParticles = new List<FreeParticle>();
            _rowGroups = new List<RowGroup>();
        }

        public override void OnAddedToEntity()
        {
            _proximityComponent = Entity.GetComponent<PlayerProximityComponent>();
            _movementComponent = Entity.GetComponent<CharacterMovementComponent>();
            _proceduralGenerator = Entity.Scene.FindEntity("map").GetComponent<ProceduralGeneratorComponent>();
        }

        public void Update()
        {
            // Update current list of free particles
            for (var ii = _freeParticles.Count - 1; ii >= 0; ii--)
            {
                var thisParticle = _freeParticles[ii];

                if (Time.TotalTime - thisParticle.SpawnTime > thisParticle.TimeToLive)
                {
                    Pool<FreeParticle>.Free(thisParticle);
                    _freeParticles.RemoveAt(ii);
                    continue;
                }

                if (Time.TotalTime > thisParticle.SpawnTime + thisParticle.TimeToLive * Settings.OrthogonalEndPositionVariancePercentOfTtlStart)
                    thisParticle.Velocity += thisParticle.DeltaVelocityPerFrame * Time.DeltaTime;

                thisParticle.Position += thisParticle.Velocity * Time.DeltaTime;

                var lerpValue = (Time.TotalTime - thisParticle.SpawnTime) / thisParticle.TimeToLive;
                thisParticle.ColorAlpha = (byte)MathHelper.Lerp(Settings.TextureAlphaStart, Settings.TextureAlphaEnd, lerpValue);
            }

            // Update current list of row groups
            for (var ii = _rowGroups.Count - 1; ii >= 0; ii--)
            {
                var thisRowGroup = _rowGroups[ii];

                var timeSinceSpawn = Time.TotalTime - thisRowGroup.SpawnTime;
                if (timeSinceSpawn > thisRowGroup.TimeToLive)
                {
                    foreach (var particle in _rowGroups[ii].Particles)
                    {
                        Pool<OarParticle>.Free(particle);
                    }
                    _rowGroups.RemoveAt(ii);
                    continue;
                }

                if (timeSinceSpawn < thisRowGroup.RowDuration)
                {
                    foreach (var particle in thisRowGroup.Particles)
                    {
                        particle.Velocity += particle.DeltaVelocityPerFrame * Time.DeltaTime;
                        particle.Position += particle.Velocity * Time.DeltaTime;
                    }
                }
                else
                {
                    foreach (var particle in thisRowGroup.Particles)
                    {
                        particle.Position += thisRowGroup.PostRowVelocity * Time.DeltaTime;
                    }

                    var lerpValue = (Time.TotalTime - thisRowGroup.RowDuration - thisRowGroup.SpawnTime) /
                                    (thisRowGroup.TimeToLive - thisRowGroup.RowDuration);
                    thisRowGroup.ColorAlpha = (byte)MathHelper.Lerp(Settings.TextureAlphaStart, Settings.TextureAlphaEnd, lerpValue);
                }
            }

            // Handle Input
            var input = _movementComponent.CurrentInput;

            // Spawn more row groups if need be
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
                    var parallelDirection = GetRotationAsDirectionVector();
                    parallelDirection.Normalize();
                    var orthogonalDirection = new Vector2(-parallelDirection.Y, parallelDirection.X);
                    orthogonalDirection.Normalize();

                    var playerLateralVelocity = Vector2.Dot(_movementComponent.CurrentVelocity, orthogonalDirection) /
                                                orthogonalDirection.Length();
                    var rowVelocity = (playerLateralVelocity * orthogonalDirection) + 
                                      (rowVelocityAsPercent.Value * _movementComponent.CurrentVelocity.Length() * parallelDirection);


                    var dTheta = MathHelper.Pi / Settings.NumParticles;
                    var radius = Settings.RowStartParticleRadius.Value;
                    if (input.Rotation <= 0f)
                    {
                        var centerPointLeft = Entity.Position +
                                               orthogonalDirection * Settings.RowStartPerpendicularPosition.Value;

                        var rowGroup = new RowGroup
                        {
                            Particles = new List<OarParticle>(),
                            SpawnTime = Time.TotalTime,
                            RowDuration = Settings.RowDurationSec,
                            TimeToLive = Settings.RowDurationSec + Settings.ParticleTtl,
                            ColorAlpha = Settings.TextureAlphaStart,
                            PostRowVelocity = _proceduralGenerator.GetRiverVelocityAt(Entity.Position)
                        };

                        for (var ii = 0; ii < Settings.NumParticles; ii++)
                        {
                            var thisParticle = Pool<OarParticle>.Obtain();
                            var angle = -dTheta * ii;
                            thisParticle.Position = 
                                centerPointLeft + radius * 
                                ((float)Math.Cos(angle) * orthogonalDirection + (float)Math.Sin(angle) * parallelDirection);

                            thisParticle.Velocity = rowVelocity;
                            thisParticle.DeltaVelocityPerFrame = Settings.RowParticleRadiusChangePerSecond.Value * (thisParticle.Position - centerPointLeft);
                            rowGroup.Particles.Add(thisParticle);
                        }

                        _rowGroups.Add(rowGroup);
                    }

                    if (input.Rotation >= 0f)
                    {
                        var centerPointRight = Entity.Position -
                                               orthogonalDirection * Settings.RowStartPerpendicularPosition.Value;

                        var rowGroup = new RowGroup
                        {
                            Particles = new List<OarParticle>(),
                            SpawnTime = Time.TotalTime,
                            RowDuration = Settings.RowDurationSec,
                            TimeToLive = Settings.RowDurationSec + Settings.ParticleTtl,
                            ColorAlpha = Settings.TextureAlphaStart,
                            PostRowVelocity = _proceduralGenerator.GetRiverVelocityAt(Entity.Position)
                        };

                        for (var ii = 0; ii < Settings.NumParticles; ii++)
                        {
                            var thisParticle = Pool<OarParticle>.Obtain();
                            var angle = -dTheta * ii;
                            thisParticle.Position =
                                centerPointRight + radius *
                                ((float)Math.Cos(angle) * orthogonalDirection + (float)Math.Sin(angle) * parallelDirection);

                            thisParticle.Velocity = rowVelocity;
                            thisParticle.DeltaVelocityPerFrame = Settings.RowParticleRadiusChangePerSecond.Value * (thisParticle.Position - centerPointRight);
                            rowGroup.Particles.Add(thisParticle);
                        }

                        _rowGroups.Add(rowGroup);
                    }
                }
            }
        }

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            foreach (var particle in _freeParticles)
            {
                var thisPos = particle.Position;

                if (Vector2.Distance(thisPos, Entity.Position) < _proximityComponent.Radius)
                {
                    var thisColor = Color.White * (particle.ColorAlpha / 255f);

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

            foreach (var rowGroup in _rowGroups)
            {
                foreach (var particle in rowGroup.Particles)
                {
                    var thisPos = particle.Position;

                    if (Vector2.Distance(thisPos, Entity.Position) < _proximityComponent.Radius)
                    {
                        var thisColor = Color.White * (rowGroup.ColorAlpha / 255f);

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
        }

        private readonly Sprite _sprite;
        private readonly List<FreeParticle> _freeParticles;
        private readonly List<RowGroup> _rowGroups;
        private CharacterMovementComponent _movementComponent;
        private PlayerProximityComponent _proximityComponent;
        private ProceduralGeneratorComponent _proceduralGenerator;

        private Vector2 GetRotationAsDirectionVector()
        {
            var rotation = Entity.Transform.Rotation;

            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }
    }
}
