using System;
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

            public static readonly RenderSetting RiverWidthUpper = new(800);
            public static readonly RenderSetting RiverWidthLower = new(400);

            public static readonly RenderSetting ObstacleXGapMinUpper = new(800);
            public static readonly RenderSetting ObstacleXGapMinLower = new(200);

            public static readonly RenderSetting ObstacleXGapMaxUpper = new(1600);
            public static readonly RenderSetting ObstacleXGapMaxLower = new(800);
        }

        public ProceduralGeneratorComponent()
        {
            _playerScoreRating = 0f;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            var textureAtlas = Entity.Scene.Content.LoadTexture("Levels/PrototypeSpriteSheet");
            var texture = Sprite.SpritesFromAtlas(textureAtlas, 16, 16)[15];

            _scale = texture.SourceRect.Width * Entity.Transform.Scale.X;
        }

        public List<LevelBlock> Blocks { get; private set; }

        public int BeginPlayOrder => 0;

        public void OnBeginPlay()
        {
            _playerCharacter = Entity.Scene.Entities.FindEntity("character");
            System.Diagnostics.Debug.Assert(_playerCharacter != null);

            var startingPos = _playerCharacter.Position;

            var riverWidth = Lerp(Settings.RiverWidthLower.Value, Settings.RiverWidthUpper.Value, 1 - _playerScoreRating / 9f);

            var randomWalk = RandomWalk(startingPos);
            Blocks = new List<LevelBlock>
            {
                new (LeadingPoints(startingPos), riverWidth, new List<Vector2>()),
                new (randomWalk, riverWidth, GetObstaclesForBlock(randomWalk, randomWalk.DomainStart + Settings.ObstacleXGapMaxUpper.Value))
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
                Blocks.RemoveAt(0);
                var lastBlock = Blocks.Last();
                var nextStartingPoint = new Vector2(lastBlock.Function.DomainEnd, lastBlock.Function.GetYForX(lastBlock.Function.DomainEnd));

                var riverWidth = Lerp(Settings.RiverWidthLower.Value, Settings.RiverWidthUpper.Value, 1 - _playerScoreRating / 9f);
                
                var newWalk = RandomWalk(nextStartingPoint);
                var newBlock = new LevelBlock(newWalk, riverWidth, GetObstaclesForBlock(newWalk));

                Blocks.Add(newBlock);

                _nextGenerationPointX = (newWalk.DomainEnd + newWalk.DomainStart) / 2f;

                _renderer.OnNewGeneration(newBlock);

                if (_playerScoreRating < 9f)
                    _playerScoreRating += 1f;
                else
                    _playerScoreRating = 9f;
            }
        }

        private float _scale;
        private ProceduralRenderer _renderer;
        private Entity _playerCharacter;
        private float _nextGenerationPointX;
        private float _playerScoreRating;

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

            var numTerms = _playerScoreRating < 5f ? Settings.NumDTFTerms : Settings.NumDTFTerms + 1;
            return new DFTFunction(walkPoints, numTerms, iStartingPoint, _scale);
        }

        private List<Vector2> GetObstaclesForBlock(IFunction iFunction, float? iStartingPointX = null)
        {
            var rng = new System.Random();
            var obstacles = new List<Vector2>();

            var lastPointX = iStartingPointX ?? iFunction.DomainStart;

            var gapMin = Lerp(Settings.ObstacleXGapMinLower.Value, Settings.ObstacleXGapMinUpper.Value, 1 - _playerScoreRating / 9f);
            var gapMax = Lerp(Settings.ObstacleXGapMaxLower.Value, Settings.ObstacleXGapMaxUpper.Value, 1 - _playerScoreRating / 9f);
            var riverWidth = Lerp(Settings.RiverWidthLower.Value, Settings.RiverWidthUpper.Value, 1 - _playerScoreRating / 9f);

            while (lastPointX < iFunction.DomainEnd)
            {
                var thisPointX = lastPointX + gapMin + 
                                 (float)rng.NextDouble() * (gapMax - gapMin);

                var thisPointY = iFunction.GetYForX(thisPointX) - (riverWidth / 2f) + _scale + ((float)rng.NextDouble() * (riverWidth - 2 * _scale));

                obstacles.Add(new Vector2(thisPointX, thisPointY));

                lastPointX = thisPointX;
            }

            return obstacles;
        }

        private static float Lerp(float iLower, float iUpper, float iVal)
        {
            System.Diagnostics.Debug.Assert(iLower < iUpper);
            System.Diagnostics.Debug.Assert(0 <= iVal && iVal <= 1f);
            var clampedVal = Math.Clamp(iVal, 0f, 1f);
            return iLower + (iUpper - iLower) * clampedVal;
        }
    }
}
