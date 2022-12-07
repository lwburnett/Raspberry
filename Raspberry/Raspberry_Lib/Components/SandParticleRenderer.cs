using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class SandParticleRenderer : RenderableComponent, IUpdatable
    {
        private static class Settings
        {
            public const int NumParticles = 200;

            public const int TextureSize = 5;
            public static readonly Color ParticleColor = Color.Tan;

            public static readonly RenderSetting VelocityX = new(10);
        }

        public SandParticleRenderer(Vector2 iSize)
        {
            _particles = new List<Vector2>();

            var rng = new System.Random();
            for (var ii = 0; ii < Settings.NumParticles; ii++)
            {
                var thisPos = GetRandomPositionInRange(rng, iSize);
                _particles.Add(thisPos);
            }

            _size = iSize;

            var textureData = new Color[Settings.TextureSize * Settings.TextureSize];
            for (var ii = 0; ii < Settings.TextureSize * Settings.TextureSize; ii++)
            {
                textureData[ii] = Settings.ParticleColor;
            }
            var texture = new Texture2D(Graphics.Instance.Batcher.GraphicsDevice, Settings.TextureSize, Settings.TextureSize);
            texture.SetData(textureData);
            _sprite = new Sprite(texture);

            _velocity = new Vector2(Settings.VelocityX.Value, 0f);
        }

        public void Update()
        {
            for (var ii = 0; ii < _particles.Count; ii++)
            {
                var newPos = _particles[ii] + _velocity;

                if (newPos.X > _size.X / 2)
                {
                    newPos = new Vector2(-_size.X / 2, _particles[ii].Y);
                }

                _particles[ii] = newPos;
            }
        }

        public override float Width => 10000;
        public override float Height => 10000;

        public override void Render(Batcher iBatcher, Camera iCamera)
        {
            foreach (var position in _particles)
            {
                iBatcher.Draw(
                    _sprite,
                    Entity.Position + LocalOffset + position,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Entity.Scale,
                    SpriteEffects.None,
                    0);
            }
        }
        
        private readonly List<Vector2> _particles;
        private readonly Sprite _sprite;
        private readonly Vector2 _size;
        private readonly Vector2 _velocity;

        private static Vector2 GetRandomPositionInRange(System.Random iRng, Vector2 iSize)
        {
            var lowerBoundX = -iSize.X / 2;
            var lowerBoundY = -iSize.Y / 2;

            return new Vector2(
                lowerBoundX + (float)iRng.NextDouble() * iSize.X,
                lowerBoundY + (float)iRng.NextDouble() * iSize.Y);
        }
    }
}
