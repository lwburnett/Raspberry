using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class RiverParticleManager : PausableRenderableComponent, IBeginPlay
    {
        private static class Settings
        {
            public const int TextureSize = 2;
            public const int NumParticles = 20;

            public const float SpawnYPercentWindow = .9f;
            public static readonly RenderSetting DeltaPhasePerUnit = new(2f);
            public static readonly RenderSetting OscillationAmplitude = new(10f);
        }

        private class RiverParticle
        {
            public Vector2 Position { get; set; }
            public float OscillationPhase { get; set; }
            public Vector2 OscillationDirection { get; set; }
            public float RiverWidthPercent { get; set; }
        }

        public RiverParticleManager()
        {
            _particles = new List<RiverParticle>();

            var textureData = new Color[Settings.TextureSize * Settings.TextureSize];
            for (var ii = 0; ii < Settings.TextureSize * Settings.TextureSize; ii++)
            {
                textureData[ii] = Color.White;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, Settings.TextureSize, Settings.TextureSize);
            texture.SetData(textureData);
            _sprite = new Sprite(texture);

            RenderLayer = 5;
        }

        public override float Width => float.MaxValue;
        public override float Height => float.MaxValue;
        public int BeginPlayOrder => 90;

        public override void OnAddedToEntity()
        {
            _camera = Entity.Scene.Camera;
            _generator = Entity.GetComponent<ProceduralGeneratorComponent>();

            _character = Entity.Scene.FindEntity("character");

            System.Diagnostics.Debug.Assert(_camera != null);
            System.Diagnostics.Debug.Assert(_generator != null);
            System.Diagnostics.Debug.Assert(_character != null);
        }

        public void OnBeginPlay()
        {
            var rng = new System.Random();
            for (var ii = 0; ii < Settings.NumParticles; ii++)
            {
                var thisX = _camera.Bounds.Left + (float)rng.NextDouble() * _camera.Bounds.Width;
                var thisYPercent = Settings.SpawnYPercentWindow * (2 * (float)rng.NextDouble() - 1);

                var block = _generator.GetBlockForPosition(thisX);
                var thisRiverWidth = block.GetRiverWidth(thisX);
                var thisRiverY = block.Function.GetYForX(thisX);
                var thisY = thisRiverY + thisYPercent * thisRiverWidth / 2;
                var thisPosition = new Vector2(thisX, thisY);

                var thisPhase = MathHelper.TwoPi * (float)rng.NextDouble();

                var particle = new RiverParticle
                {
                    Position = thisPosition,
                    OscillationPhase = thisPhase,
                    OscillationDirection = Vector2.Zero,
                    RiverWidthPercent = thisYPercent
                };

                _particles.Add(particle);
            }

            _playerProximityComponent = _character.GetComponent<PlayerProximityComponent>();
            System.Diagnostics.Debug.Assert(_playerProximityComponent != null);
        }

        protected override void OnUpdate(float iTotalPlayableTime)
        {
            foreach (var particle in _particles)
            {
                var originalPosition = particle.Position;
                var riverVelocity = _generator.GetRiverVelocityAt(particle.Position);

                particle.Position += riverVelocity * Time.DeltaTime;

                float? teleportX = null;
                if (_camera.Bounds.Left > particle.Position.X)
                {
                    teleportX = _camera.Bounds.Right;
                }
                else if (_camera.Bounds.Right < particle.Position.X)
                {
                    teleportX = _camera.Bounds.Left;
                }

                if (teleportX.HasValue)
                {
                    var newBlock = _generator.GetBlockForPosition(teleportX.Value);
                    var riverWidth = newBlock.GetRiverWidth(teleportX.Value);
                    var newY = newBlock.Function.GetYForX(teleportX.Value) + riverWidth * particle.RiverWidthPercent / 2;

                    particle.Position = new Vector2(teleportX.Value, newY);

                    var riverFlowDirection = new Vector2(1f, newBlock.Function.GetYPrimeForX(teleportX.Value));
                    riverFlowDirection.Normalize();
                    particle.OscillationDirection = riverFlowDirection;
                }
                else
                {
                    var oldRiverWidth = _generator.GetBlockForPosition(particle.Position).GetRiverWidth(originalPosition.X);
                    var newRiverWidth = _generator.GetBlockForPosition(particle.Position).GetRiverWidth(particle.Position.X);

                    var riverWidthDiffFactorY = Math.Abs(newRiverWidth - oldRiverWidth) * -particle.RiverWidthPercent / 2;
                    particle.Position += new Vector2(0f, riverWidthDiffFactorY);

                    particle.OscillationPhase += Settings.DeltaPhasePerUnit.Value * Time.DeltaTime;

                    riverVelocity.Normalize();
                    particle.OscillationDirection = new Vector2(-riverVelocity.Y, riverVelocity.X);
                }
            }
        }

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            foreach (var particle in _particles)
            {
                var modifiedPosition = particle.Position + (float)Math.Cos(particle.OscillationPhase) * Settings.OscillationAmplitude.Value * particle.OscillationDirection;


                if (Vector2.Distance(modifiedPosition, _character.Position) < _playerProximityComponent.Radius)
                {
                    iBatcher.Draw(
                        _sprite,
                        modifiedPosition,
                        Color.White,
                        0f,
                        Vector2.Zero,
                        Entity.Scale,
                        SpriteEffects.None,
                        0);
                }
            }
        }

        private readonly List<RiverParticle> _particles;
        private readonly Sprite _sprite;
        private Camera _camera;
        private ProceduralGeneratorComponent _generator;
        private Entity _character;
        private PlayerProximityComponent _playerProximityComponent;
    }
}
