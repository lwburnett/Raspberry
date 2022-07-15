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

            public const int MaxNumParticles = 50;
            public const float ParticleTtl = 1f;
            public static readonly RenderSetting MinimumVelocityForSpawn = new(10);

            public const int TextureSize = 2;
        }

        private class WakeParticle
        {
            public WakeParticle()
            {
                Sprite = null;
                Position = Vector2.Zero;
            }

            public Sprite Sprite { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public float SpawnTime { get; set; }
            public float TimeToLive { get; set; }
        }

        public WakeParticleEmitter(Func<Vector2> iGetCurrentVelocityFunc)
        {
            _particles = new List<WakeParticle>();
            _getEntityVelocityFunc = iGetCurrentVelocityFunc;
            Pool<WakeParticle>.WarmCache(Settings.MaxNumParticles);
            
            var textureData = new Color[Settings.TextureSize * Settings.TextureSize];
            for (int ii = 0; ii < Settings.TextureSize * Settings.TextureSize; ii++)
            {
                textureData[ii] = Color.White;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, Settings.TextureSize, Settings.TextureSize);
            texture.SetData(textureData);
            _sprite = new Sprite(texture);
        }

        public override void OnAddedToEntity()
        {
            _collider = Entity.GetComponent<Collider>();
            _proceduralGenerator = Entity.Scene.FindEntity("map").GetComponent<ProceduralGeneratorComponent>();

            System.Diagnostics.Debug.Assert(_collider != null);
            System.Diagnostics.Debug.Assert(_proceduralGenerator != null);
        }
        
        public override float Width => float.MaxValue;
        public override float Height => float.MaxValue;

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            foreach (var wakeParticle in _particles)
            {
                iBatcher.Draw(
                    wakeParticle.Sprite, 
                    Entity.Position + wakeParticle.Position, 
                    Color.White, 
                    0f, 
                    Vector2.Zero, 
                    Entity.Scale, 
                    SpriteEffects.None, 
                    0);
            }
        }

        public void Update()
        {
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

                thisParticle.Position += thisParticle.Velocity * Time.DeltaTime;
            }

            // Get current spawn points
            Vector2 upperPoint, lowerPoint, particleVelocity;
            switch (_collider.Shape)
            {
                case Circle circle:
                    GetWakePoints(circle, out upperPoint, out lowerPoint, out particleVelocity);
                    break;
                case Polygon polygon:
                    GetWakePoints(polygon, out upperPoint, out lowerPoint, out particleVelocity);
                    break;
                default:
                    System.Diagnostics.Debug.Fail($"Unknown type of {nameof(Shape)}");
                    return;
            }

            // Spawn particles if needed
            if (_particles.Count < Settings.MaxNumParticles)
            {
                if (_particles.Any())
                {
                    var tmp1 = Vector2.Distance(lowerPoint, Entity.Position + _lastParticleSpawned.Position);
                    var tmp2 = Settings.TextureSize * Entity.Scale.X * 2;

                    if (tmp1 > tmp2)
                    {
                        SpawnParticles(upperPoint, lowerPoint, particleVelocity);
                    }
                }
                else if (Math.Abs(particleVelocity.Length()) > Settings.MinimumVelocityForSpawn.Value)
                {
                    SpawnParticles(upperPoint, lowerPoint, particleVelocity);
                }
            }
        }

        private WakeParticle _lastParticleSpawned;
        private readonly List<WakeParticle> _particles;
        private Collider _collider;
        private ProceduralGeneratorComponent _proceduralGenerator;
        private readonly Func<Vector2> _getEntityVelocityFunc;
        private readonly Sprite _sprite;

        private void GetWakePoints(Circle iCircle, out Vector2 oUpperPoint, out Vector2 oLowerPoint, out Vector2 oParticleVelocity)
        {
            var entityVelocity = _getEntityVelocityFunc();
            var riverVelocity = GetRiverVelocityAt(Entity.Position);

            var velocityDiff = riverVelocity - entityVelocity;

            var orthogonalVec = new Vector2(-velocityDiff.Y, velocityDiff.X);
            orthogonalVec.Normalize();

            var point1 = Entity.Position + iCircle.Radius * orthogonalVec;
            var point2 = Entity.Position - iCircle.Radius * orthogonalVec;

            if (point1.Y > point2.Y)
            {
                oUpperPoint = point2;
                oLowerPoint = point1;
            }
            else
            {
                oUpperPoint = point1;
                oLowerPoint = point2;
            }

            oParticleVelocity = velocityDiff;
        }

        private void GetWakePoints(Polygon iPolygon, out Vector2 oUpperPoint, out Vector2 oLowerPoint, out Vector2 oParticleVelocity)
        {
            throw new NotImplementedException();
        }

        private Vector2 GetRiverVelocityAt(Vector2 iPos)
        {
            var flowSpeed = MathHelper.Lerp(
                Settings.FlowSpeedLower.Value,
                Settings.FlowSpeedUpper.Value,
                1 - _proceduralGenerator.PlayerScoreRating / _proceduralGenerator.PlayerScoreRatingMax);

            var block = _proceduralGenerator.GetBlockForPosition(Entity.Position);
            if (block == null)
                return Vector2.Zero;

            var yPrime = block.Function.GetYPrimeForX(iPos.X);
            var riverVelocity = new Vector2(1, yPrime);
            riverVelocity.Normalize();
            riverVelocity *= flowSpeed;

            return riverVelocity;
        }

        private void SpawnParticles(Vector2 iUpperSpawnPoint, Vector2 iLowerSpawnPoint, Vector2 iVelocity)
        {
            var spawnPointOffset = new Vector2(-Settings.TextureSize * Entity.Scale.X / 2f);

            var upperParticle = Pool<WakeParticle>.Obtain();
            var lowerParticle = Pool<WakeParticle>.Obtain();

            upperParticle.Sprite = _sprite;
            lowerParticle.Sprite = _sprite;

            upperParticle.Position = Entity.Position - iUpperSpawnPoint + spawnPointOffset;
            lowerParticle.Position = Entity.Position - iLowerSpawnPoint + spawnPointOffset;

            upperParticle.Velocity = iVelocity;
            lowerParticle.Velocity = iVelocity;

            upperParticle.SpawnTime = Time.TotalTime;
            lowerParticle.SpawnTime = Time.TotalTime;

            upperParticle.TimeToLive = Settings.ParticleTtl;
            lowerParticle.TimeToLive = Settings.ParticleTtl;

            _particles.Add(upperParticle);
            _particles.Add(lowerParticle);

            _lastParticleSpawned = lowerParticle;
        }
    }
}
