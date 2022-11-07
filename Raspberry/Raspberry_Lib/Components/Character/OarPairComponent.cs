using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raspberry_Lib.Components
{
    internal class OarPairComponent : RenderableComponent, IUpdatable
    {
        private static class Settings
        {
            public const int MaxNumParticles = 75;
            public const int TextureSize = 2;

            public const byte TextureAlphaStart = 255;
            public const byte TextureAlphaEnd = 0;

            public static readonly RenderSetting RowLength = new(100);

            public const float ParticleTtl = 1.4f;

            public const float RowTransition1 = .5f;
            public const float RowTransition2 = .9f;
            public const float RowTransition3 = 1.25f;
            
            public const float RowSpeedMediumAsPercentOfCurrentSpeed = -0.25f;
            public const float RowSpeedGoodAsPercentOfCurrentSpeed = -0.75f;
            public const float RowSpeedNeutralAsPercentOfCurrentSpeed = -0.50f;

            public static readonly RenderSetting RowStartPerpendicularPosition = new(45);
            public static readonly RenderSetting RowStartParticleRadius = new(3);
            public static readonly RenderSetting RowEndParticleRadius = new(13);

            public const int NumParticles = 8;

            public const float OrthogonalEndPositionVariancePercentOfTtlStart = .25f;
            
            public static readonly RenderSetting MinimumVelocityForWakeSpawn = new(10);
            public static readonly RenderSetting OrthogonalStartPositionalVariance = new(1);
            public const float OrthogonalEndPositionalVarianceAsPercentOfVelocityMag = .04f;
        }

        private class RowGroup
        {
            public List<OarParticle> Particles { get; set; }
            public Vector2 CenterPoint { get; set; }
            public Vector2 CenterVelocity { get; set; }
            public float? FreedomTime { get; set; }
            public float TimeToLive { get; set; }
            public float ColorAlpha { get; set; }
            public Vector2 PostRowVelocity { get; set; }
        }

        private class OarParticle
        {
            public Vector2 Position { get; set; }
            public Vector2 RadialDirection { get; set; }
        }

        private class FreeParticle
        {
            public Vector2 Position { get; set; }
            public Vector2 SpawnPosition { get; set; }
            public Vector2 Velocity { get; set; }
            public Vector2 DeltaVelocityPerFrame { get; set; }
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
            _rng = new System.Random();
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
            var parallelDirection = GetRotationAsDirectionVector();
            parallelDirection.Normalize();
            var orthogonalDirection = new Vector2(-parallelDirection.Y, parallelDirection.X);
            orthogonalDirection.Normalize();

            for (var ii = _rowGroups.Count - 1; ii >= 0; ii--)
            {
                var thisRowGroup = _rowGroups[ii];
                
                if (thisRowGroup.FreedomTime.HasValue && Time.TotalTime - thisRowGroup.FreedomTime.Value > thisRowGroup.TimeToLive)
                {
                    foreach (var particle in _rowGroups[ii].Particles)
                    {
                        Pool<OarParticle>.Free(particle);
                    }
                    _rowGroups.RemoveAt(ii);
                    continue;
                }

                var entityToGroupCenter = thisRowGroup.CenterPoint - Entity.Position;
                var groupCenterProjection = Math.Abs(Vector2.Dot(parallelDirection, entityToGroupCenter));

                if (groupCenterProjection < Settings.RowLength.Value)
                {
                    thisRowGroup.CenterPoint += thisRowGroup.CenterVelocity * Time.DeltaTime;

                    var radialLerpValue = 1 - (Settings.RowLength.Value - groupCenterProjection) / Settings.RowLength.Value;
                    var radius = MathHelper.Lerp(Settings.RowStartParticleRadius.Value, Settings.RowEndParticleRadius.Value, radialLerpValue);

                    foreach (var particle in thisRowGroup.Particles)
                    {
                        particle.Position = thisRowGroup.CenterPoint + particle.RadialDirection * radius;
                    }
                }
                else
                {
                    thisRowGroup.FreedomTime ??= Time.TotalTime;

                    foreach (var particle in thisRowGroup.Particles)
                    {
                        particle.Position += thisRowGroup.PostRowVelocity * Time.DeltaTime;
                    }

                    var alphaLerpValue = (Time.TotalTime - thisRowGroup.FreedomTime.Value) / thisRowGroup.TimeToLive;
                    thisRowGroup.ColorAlpha = (byte)MathHelper.Lerp(Settings.TextureAlphaStart, Settings.TextureAlphaEnd, alphaLerpValue);
                }
            }

            // Handle Input
            var input = _movementComponent.CurrentInput;

            // Spawn more row groups if need be
            if (input.Row)
            {
                var timeDiff = _movementComponent.SecondsSinceLastRow;
                float? rowVelocityAsPercent;
                if (timeDiff < Settings.RowTransition1)
                {
                    rowVelocityAsPercent = null;
                }
                else if (timeDiff < Settings.RowTransition2)
                {
                    rowVelocityAsPercent = Settings.RowSpeedMediumAsPercentOfCurrentSpeed;
                }
                else if (timeDiff < Settings.RowTransition3)
                {
                    rowVelocityAsPercent = Settings.RowSpeedGoodAsPercentOfCurrentSpeed;
                }
                else
                {
                    rowVelocityAsPercent = Settings.RowSpeedNeutralAsPercentOfCurrentSpeed;
                }
                
                if (rowVelocityAsPercent.HasValue)
                {
                    var playerParallelSpeed = Vector2.Dot(_movementComponent.CurrentVelocity, parallelDirection) /
                                              parallelDirection.Length();
                    var playerLateralSpeed = Vector2.Dot(_movementComponent.CurrentVelocity, orthogonalDirection) /
                                                orthogonalDirection.Length();
                    var rowVelocity = (playerLateralSpeed * orthogonalDirection) + 
                                      (rowVelocityAsPercent.Value * playerParallelSpeed * parallelDirection);
                    
                    var dTheta = MathHelper.Pi / Settings.NumParticles;
                    var radius = Settings.RowStartParticleRadius.Value;
                    if (input.Rotation <= 0f)
                    {
                        var centerPointLeft = Entity.Position +
                                               orthogonalDirection * Settings.RowStartPerpendicularPosition.Value;

                        var rowGroup = CreateRowGroup(
                            centerPointLeft,
                            dTheta,
                            radius,
                            orthogonalDirection,
                            parallelDirection,
                            rowVelocity);

                        _rowGroups.Add(rowGroup);
                    }

                    if (input.Rotation >= 0f)
                    {
                        var centerPointRight = Entity.Position -
                                               orthogonalDirection * Settings.RowStartPerpendicularPosition.Value;

                        var rowGroup = CreateRowGroup(
                            centerPointRight,
                            dTheta,
                            radius,
                            orthogonalDirection,
                            parallelDirection,
                            rowVelocity);

                        _rowGroups.Add(rowGroup);
                    }
                }
            }

            // Handle left oar wake if turning
            if (input.Rotation < 0)
            {
                var oarPosition = Entity.Position -
                                 orthogonalDirection * Settings.RowStartPerpendicularPosition.Value;
                HandleWake(oarPosition);
            }

            // Handle right oar wake if turning
            if (input.Rotation > 0)
            {
                var oarPosition = Entity.Position +
                                  orthogonalDirection * Settings.RowStartPerpendicularPosition.Value;
                HandleWake(oarPosition);
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
        private readonly System.Random _rng;
        private CharacterMovementComponent _movementComponent;
        private PlayerProximityComponent _proximityComponent;
        private ProceduralGeneratorComponent _proceduralGenerator;
        private FreeParticle _lastParticleSpawned;

        private RowGroup CreateRowGroup(
            Vector2 iCenterPoint,
            float iDTheta, 
            float iRadius,
            Vector2 iOrthogonalDirection,
            Vector2 iParallelDirection,
            Vector2 iRowVelocity)
        {
            var rowGroup = new RowGroup
            {
                Particles = new List<OarParticle>(),
                CenterPoint = iCenterPoint,
                CenterVelocity = iRowVelocity,
                TimeToLive = Settings.ParticleTtl,
                ColorAlpha = Settings.TextureAlphaStart,
                PostRowVelocity = _proceduralGenerator.GetRiverVelocityAt(Entity.Position)
            };

            for (var ii = 0; ii < Settings.NumParticles; ii++)
            {
                var thisParticle = Pool<OarParticle>.Obtain();
                var angle = -iDTheta * ii;
                thisParticle.RadialDirection = (float)Math.Cos(angle) * iOrthogonalDirection +
                                               (float)Math.Sin(angle) * iParallelDirection;
                thisParticle.Position =
                    iCenterPoint + iRadius * thisParticle.RadialDirection;
                
                rowGroup.Particles.Add(thisParticle);
            }

            return rowGroup;
        }

        private void SpawnParticles(Vector2 iSpawnPosition, Vector2 iVelocity)
        {
            var spawnPointOffset = new Vector2(-Settings.TextureSize * Entity.Scale.X / 2f);
        
            var particle = Pool<FreeParticle>.Obtain();
            
            var orthogonalDirection = new Vector2(-iVelocity.Y, iVelocity.X);
            orthogonalDirection.Normalize();
            var rngOffsetMag = (float)(Settings.OrthogonalStartPositionalVariance.Value * (2 * _rng.NextDouble() - 1));
            var rngOffset = orthogonalDirection * rngOffsetMag;

            var spawnPositionFinal = iSpawnPosition + spawnPointOffset + rngOffset;
            particle.Position = spawnPositionFinal;
            particle.SpawnPosition = spawnPositionFinal;

            particle.Velocity = iVelocity;
            particle.SpawnTime = Time.TotalTime;
            particle.TimeToLive = Settings.ParticleTtl;

            var timeToVarySquared = Settings.ParticleTtl * Settings.ParticleTtl *
                                    Settings.OrthogonalEndPositionVariancePercentOfTtlStart * Settings.OrthogonalEndPositionVariancePercentOfTtlStart;
            var endVarianceMag = 2 * ((float)_rng.NextDouble() * 2 - 1f) *
                                 Settings.OrthogonalEndPositionalVarianceAsPercentOfVelocityMag * iVelocity.Length() /
                                 timeToVarySquared;
            var endVariance = endVarianceMag * orthogonalDirection;

            particle.DeltaVelocityPerFrame = endVariance;

            _freeParticles.Add(particle);

            _lastParticleSpawned = particle;
        }

        void HandleWake(Vector2 iOarPosition)
        {
            var riverVelocity = _proceduralGenerator.GetRiverVelocityAt(Entity.Position);

                // Spawn particles if needed
            if (_freeParticles.Count < Settings.MaxNumParticles)
            {
                if (_freeParticles.Any())
                {
                    var distanceOfLastSpawnedParticle = Vector2.Distance(_lastParticleSpawned.SpawnPosition, _lastParticleSpawned.Position);
                    var widthOfParticleTexture = Settings.TextureSize;

                    if (distanceOfLastSpawnedParticle > widthOfParticleTexture)
                    {
                        SpawnParticles(iOarPosition, riverVelocity);
                    }
                }
                else if (Math.Abs(riverVelocity.Length()) > Settings.MinimumVelocityForWakeSpawn.Value)
                {
                    SpawnParticles(iOarPosition, riverVelocity);
                }
            }
        }

        private Vector2 GetRotationAsDirectionVector()
        {
            var rotation = Entity.Transform.Rotation;

            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }
    }
}
