using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
using Raspberry_Lib.Maths;
using Random = System.Random;

namespace Raspberry_Lib.Components
{
    internal class ProceduralGeneratorComponent : Component, IBeginPlay
    {
        private static class Settings
        {
            public const int NumPointsPerBlock = 100;

            public const int NumDTFTerms = 5;
        }

        public ProceduralGeneratorComponent()
        {
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            var textureAtlas = Entity.Scene.Content.LoadTexture("Levels/PrototypeSpriteSheet");
            var texture = Sprite.SpritesFromAtlas(textureAtlas, 16, 16)[15];

            _scale = texture.SourceRect.Width * Entity.Transform.Scale.X;
        }

        public List<IFunction> Functions { get; private set; }

        public int BeginPlayOrder => 0;

        public void OnBeginPlay()
        {
            var characterEntity = Entity.Scene.Entities.FindEntity("character");
            var startingPos = characterEntity.Position;

            Functions = new List<IFunction>
            {
                LeadingPoints(startingPos),
                RandomWalk(startingPos)
            };
        }

        private float _scale;

        private IFunction LeadingPoints(Vector2 iStartingPoint)
        {
            var leadingPoints = new List<Vector2>();
            for (var ii = 0; ii < 4; ii++)
            {
                var thisPoint = new Vector2(iStartingPoint.X - (4 - ii) * _scale, iStartingPoint.Y);
                leadingPoints.Add(thisPoint);
            }

            return new LinearFunction(leadingPoints);
        }

        private IFunction RandomWalk(Vector2 iStartingPoint)
        {
            var rng = new Random();
            var walkPoints = new List<Vector2>();

            for (var ii = 0; ii < Settings.NumPointsPerBlock; ii++)
            {
                float dy;
                var randomNumber = rng.Next() % 2;
                if (randomNumber == 0)
                {
                    dy = -1;
                }
                else
                {
                    dy = 1;
                }

                var thisPoint = new Vector2(ii, dy);
                walkPoints.Add(thisPoint);
            }

            return new DFTFunction(walkPoints, Settings.NumDTFTerms, iStartingPoint, _scale);
        }
    }
}
