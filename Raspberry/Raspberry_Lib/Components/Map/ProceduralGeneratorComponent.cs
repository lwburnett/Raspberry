using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
using Raspberry_Lib.Maths;

namespace Raspberry_Lib.Components
{
    internal class ProceduralGeneratorComponent : Component, IUpdatable, IBeginPlay
    {
        private static class Settings
        {
            public const int NumPointsPerBlock = 100;

            public const int NumDTFTerms = 5;

            public const int NumLeadingPoints = 20;
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
            _playerCharacter = Entity.Scene.Entities.FindEntity("character");
            System.Diagnostics.Debug.Assert(_playerCharacter != null);

            var startingPos = _playerCharacter.Position;

            var randomWalk = RandomWalk(startingPos);
            Functions = new List<IFunction>
            {
                LeadingPoints(startingPos),
                randomWalk
            };

            _nextGenerationPointX = (randomWalk.DomainEnd + randomWalk.DomainStart) / 2f;

            _renderer = Entity.GetComponent<ProceduralRenderer>();
            System.Diagnostics.Debug.Assert(_renderer != null);
        }

        public void Update()
        {
            if (_playerCharacter == null)
                return;

            if (_playerCharacter.Position.X > _nextGenerationPointX)
            {
                Functions.RemoveAt(0);
                var lastFunc = Functions.Last();
                var nextStartingPoint = new Vector2(lastFunc.DomainEnd, lastFunc.GetYForX(lastFunc.DomainEnd));

                var newWalk = RandomWalk(nextStartingPoint);

                Functions.Add(newWalk);

                _nextGenerationPointX = (newWalk.DomainEnd + newWalk.DomainStart) / 2f;

                _renderer.OnNewGeneration(newWalk);
            }
        }

        private float _scale;
        private ProceduralRenderer _renderer;
        private Entity _playerCharacter;
        private float _nextGenerationPointX;

        private IFunction LeadingPoints(Vector2 iStartingPoint)
        {
            var leadingPoints = new List<Vector2>();
            for (var ii = 0; ii < Settings.NumLeadingPoints; ii++)
            {
                var thisPoint = new Vector2(iStartingPoint.X - (Settings.NumLeadingPoints - ii - 1) * _scale, iStartingPoint.Y);
                leadingPoints.Add(thisPoint);
            }

            return new LinearFunction(leadingPoints);
        }

        private IFunction RandomWalk(Vector2 iStartingPoint)
        {
            var rng = new System.Random();
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
