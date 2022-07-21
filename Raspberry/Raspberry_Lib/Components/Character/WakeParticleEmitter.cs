using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.PhysicsShapes;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class WakeParticleEmitter : RenderableComponent, IUpdatable
    {
        private static class Settings
        {
            public static readonly RenderSetting FlowSpeedLower = new(60);
            public static readonly RenderSetting FlowSpeedUpper = new(120);

            public const int MaxNumParticles = 150;
            public const float ParticleTtl = 1f;
            public static readonly RenderSetting MinimumVelocityForSpawn = new(10);

            public const int TextureSizeStart = 2;
            public const int TextureSizeEnd = 2;

            public const byte TextureAlphaStart = 255;
            public const byte TextureAlphaEnd = 0;

            public static readonly RenderSetting OrthogonalStartPositionalVariance = new(1);
            public const float OrthogonalEndPositionalVarianceAsPercentOfVelocityMag = .04f;
            public const float OrthogonalEndPositionVariancePercentOfTtlStart = .25f;
        }

        private class WakeParticle
        {
            public WakeParticle()
            {
                Sprite = null;
                Position = Vector2.Zero;
            }

            public Sprite Sprite { get; set; }
            public Vector2 SpawnPosition { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public float SpawnTime { get; set; }
            public float TimeToLive { get; set; }
            public float Size { get; set; }
            public byte ColorAlpha { get; set; }
            public Vector2 DeltaVelocityPerFrame { get; set; }
        }

        // Constructor for static objects that don't move
        public WakeParticleEmitter(Func<bool> iShouldUpdateFunc) :
            this(null, iShouldUpdateFunc, false)
        {
        }

        // Constructor for dynamic objects that need to be recalculated every frame
        public WakeParticleEmitter(Func<Vector2> iGetCurrentVelocityFunc, Func<bool> iShouldUpdateFunc, bool iIsPlayer)
        {
            _isPlayer = iIsPlayer;
            _particles = new List<WakeParticle>();
            _getEntityVelocityFunc = iGetCurrentVelocityFunc;
            _shouldUpdateFunc = iShouldUpdateFunc;
            Pool<WakeParticle>.WarmCache(Settings.MaxNumParticles);
            
            var textureData = new Color[Settings.TextureSizeStart * Settings.TextureSizeStart];
            for (int ii = 0; ii < Settings.TextureSizeStart * Settings.TextureSizeStart; ii++)
            {
                textureData[ii] = Color.White;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, Settings.TextureSizeStart, Settings.TextureSizeStart);
            texture.SetData(textureData);
            _sprite = new Sprite(texture);
            _rng = new System.Random();
            _staticSpawnPoints = new List<Vector2>();
        }

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            _proceduralGenerator = Entity.Scene.FindEntity("map").GetComponent<ProceduralGeneratorComponent>();

            System.Diagnostics.Debug.Assert(_collider != null);
            System.Diagnostics.Debug.Assert(_proceduralGenerator != null);

            if (_getEntityVelocityFunc == null)
            {
                Vector2 GetZeroVec() => Vector2.Zero;

                switch (_collider.Shape)
                {
                    case Circle circle:
                        GetWakePoints(circle, GetZeroVec, out _staticSpawnPoints, out _staticParticleVelocity);
                        break;
                    case Polygon polygon:
                        GetWakePoints(polygon, GetZeroVec, out _staticSpawnPoints, out _staticParticleVelocity);
                        break;
                    default:
                        System.Diagnostics.Debug.Fail($"Unknown type of {nameof(Shape)}");
                        return;
                }
            }
        }
        
        public override float Width => 100000000000f;
        public override float Height => 100000000000f;

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            foreach (var wakeParticle in _particles)
            {
                var thisColor = Color.White * (wakeParticle.ColorAlpha / 255f);

                iBatcher.Draw(
                    wakeParticle.Sprite, 
                    Entity.Position + wakeParticle.Position,
                    thisColor, 
                    0f, 
                    Vector2.Zero, 
                    wakeParticle.Size, 
                    SpriteEffects.None, 
                    0);
            }
        }

        public void Update()
        {
            // Skip update if need be
            if (!_shouldUpdateFunc())
                return;

            // Update current list of particles
            for (var ii = _particles.Count - 1; ii >= 0; ii--)
            {
                var thisParticle = _particles[ii];

                if (Time.TotalTime - thisParticle.SpawnTime > thisParticle.TimeToLive)
                {
                    Pool<WakeParticle>.Free(thisParticle);
                    _particles.RemoveAt(ii);
                    continue;
                }

                if (Time.TotalTime > thisParticle.SpawnTime + thisParticle.TimeToLive * Settings.OrthogonalEndPositionVariancePercentOfTtlStart)
                    thisParticle.Velocity += thisParticle.DeltaVelocityPerFrame * Time.DeltaTime;
                
                thisParticle.Position += thisParticle.Velocity * Time.DeltaTime;

                var lerpValue = (Time.TotalTime - thisParticle.SpawnTime) / thisParticle.TimeToLive;
                thisParticle.ColorAlpha = (byte)MathHelper.Lerp(Settings.TextureAlphaStart, Settings.TextureAlphaEnd, lerpValue);
                thisParticle.Size = MathHelper.Lerp(Settings.TextureSizeStart, Settings.TextureSizeEnd, lerpValue);
            }

            List<Vector2> spawnPoints;
            Vector2 particleVelocity;
            // Get current spawn points
            if (_getEntityVelocityFunc == null)
            {
                spawnPoints = _staticSpawnPoints;
                particleVelocity = _staticParticleVelocity;
            }
            else
            {
                switch (_collider.Shape)
                {
                    case Circle circle:
                        GetWakePoints(circle, _getEntityVelocityFunc, out spawnPoints, out particleVelocity);
                        break;
                    case Polygon polygon:
                        GetWakePoints(polygon, _getEntityVelocityFunc, out spawnPoints, out particleVelocity);
                        break;
                    default:
                        System.Diagnostics.Debug.Fail($"Unknown type of {nameof(Shape)}");
                        return;
                }
            }

            // Spawn particles if needed
            if (_particles.Count < Settings.MaxNumParticles)
            {
                if (_particles.Any())
                {
                    var distanceOfLastSpawnedParticle = Vector2.Distance(_lastParticleSpawned.SpawnPosition, _lastParticleSpawned.Position);
                    var widthOfParticleTexture = Settings.TextureSizeStart;

                    if (distanceOfLastSpawnedParticle > widthOfParticleTexture)
                    {
                        SpawnParticles(spawnPoints, particleVelocity);
                    }
                }
                else if (Math.Abs(particleVelocity.Length()) > Settings.MinimumVelocityForSpawn.Value)
                {
                    SpawnParticles(spawnPoints, particleVelocity);
                }
            }
        }

        private WakeParticle _lastParticleSpawned;
        private readonly List<WakeParticle> _particles;
        private Collider _collider;
        private ProceduralGeneratorComponent _proceduralGenerator;
        private readonly Sprite _sprite;
        private readonly Func<Vector2> _getEntityVelocityFunc;
        private readonly Func<bool> _shouldUpdateFunc;

        private List<Vector2> _staticSpawnPoints;
        private Vector2 _staticParticleVelocity;

        private readonly System.Random _rng;
        private readonly bool _isPlayer;

        private void GetWakePoints(Circle iCircle, Func<Vector2> iGetVelocityFunc, out List<Vector2> oSpawnPoints, out Vector2 oParticleVelocity)
        {
            var entityVelocity = iGetVelocityFunc();
            var riverVelocity = GetRiverVelocityAt(Entity.Position);

            var velocityDiff = riverVelocity - entityVelocity;

            var orthogonalVec = new Vector2(-velocityDiff.Y, velocityDiff.X);
            orthogonalVec.Normalize();

            var point1 = Entity.Position + iCircle.Radius * orthogonalVec;
            var point2 = Entity.Position - iCircle.Radius * orthogonalVec;

            oSpawnPoints = new List<Vector2>
            {
                point1,
                point2
            };

            oParticleVelocity = velocityDiff;
        }

        private void GetWakePoints(Polygon iPolygon, Func<Vector2> iGetVelocityFunc, out List<Vector2> oSpawnPoints, out Vector2 oParticleVelocity)
        {
            var entityVelocity = iGetVelocityFunc();
            var riverVelocity = GetRiverVelocityAt(Entity.Position);

            var velocityDiff = riverVelocity - entityVelocity;
            
            if (iPolygon is Box box && _isPlayer)
            {
                oSpawnPoints = new List<Vector2>
                {
                    Entity.Position + box.Points[1],
                    Entity.Position + box.Points[2]
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            oParticleVelocity = velocityDiff;
        }

        private Vector2 GetRiverVelocityAt(Vector2 iPos)
        {
            var flowSpeed = MathHelper.Lerp(
                Settings.FlowSpeedLower.Value,
                Settings.FlowSpeedUpper.Value,
                1 - _proceduralGenerator.PlayerScoreRating / _proceduralGenerator.PlayerScoreRatingMax);

            var block = _proceduralGenerator.GetBlockForPosition(iPos);
            if (block == null)
                return Vector2.Zero;

            var yPrime = block.Function.GetYPrimeForX(iPos.X);
            var riverVelocity = new Vector2(1, yPrime);
            riverVelocity.Normalize();
            riverVelocity *= flowSpeed;

            return riverVelocity;
        }

        private void SpawnParticles(List<Vector2> iSpawnPositions, Vector2 iVelocity)
        {
            var spawnPointOffset = new Vector2(-Settings.TextureSizeStart * Entity.Scale.X / 2f);

            foreach (var spawnPosition in iSpawnPositions)
            {
                var particle = Pool<WakeParticle>.Obtain();

                particle.Sprite = _sprite; 
                
                var orthogonalDirection = new Vector2(-iVelocity.Y, iVelocity.X);
                orthogonalDirection.Normalize();
                var rngOffsetMag = (float)(Settings.OrthogonalStartPositionalVariance.Value * (2 * _rng.NextDouble() - 1));
                var rngOffset = orthogonalDirection * rngOffsetMag;

                var spawnPositionFinal = Entity.Position - spawnPosition + spawnPointOffset + rngOffset;
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

                _particles.Add(particle);

                _lastParticleSpawned = particle;
            }
        }
    }
}
