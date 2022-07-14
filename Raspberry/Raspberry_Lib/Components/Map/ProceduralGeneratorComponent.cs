using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Raspberry_Lib.Maths;

namespace Raspberry_Lib.Components
{
    internal class ProceduralGeneratorComponent : Component, IUpdatable, IBeginPlay
    {
        private static class Settings
        {
            public const int NumPointsPerBlockLower = 60;
            public const int NumPointsPerBlockUpper = 120;

            public const int NumDTFTerms = 5;

            public const int NumLeadingPoints = 20;

            public const int PlayerScoreRatingMax = 10;

            public static readonly RenderSetting RiverWidthUpper = new(800);
            public static readonly RenderSetting RiverWidthLower = new(400);

            public static readonly RenderSetting ObstacleXGapMinUpper = new(800);
            public static readonly RenderSetting ObstacleXGapMinLower = new(200);

            public static readonly RenderSetting ObstacleXGapMaxUpper = new(1400);
            public static readonly RenderSetting ObstacleXGapMaxLower = new(600);

            public const float YScaleDivisorLower = 8f;
            public const float YScaleDivisorUpper = 4f;
        }

        public ProceduralGeneratorComponent()
        {
            PlayerScoreRating = 1f;
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _scale = 32 * Entity.Scale.X;
        }

        public List<LevelBlock> Blocks { get; private set; }

        public int BeginPlayOrder => 0;


        public float PlayerScoreRating { get; private set; }
        public float PlayerScoreRatingMax => Settings.PlayerScoreRatingMax;

        public void OnBeginPlay()
        {
            _playerCharacter = Entity.Scene.Entities.FindEntity("character");
            System.Diagnostics.Debug.Assert(_playerCharacter != null);

            var startingPos = _playerCharacter.Position;

            var riverWidth = MathHelper.Lerp(Settings.RiverWidthLower.Value, Settings.RiverWidthUpper.Value, 1 - PlayerScoreRating / Settings.PlayerScoreRatingMax);

            var randomWalk = RandomWalk(startingPos, Vector2.UnitX);
            Blocks = new List<LevelBlock>
            {
                new (LeadingPoints(startingPos), new List<Vector2>(), riverWidth),
                new (randomWalk, GetObstaclesForBlock(randomWalk, randomWalk.DomainStart + Settings.ObstacleXGapMaxUpper.Value), riverWidth)
            };

            _nextScorePointX = randomWalk.DomainEnd;
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

                var riverWidth = MathHelper.Lerp(Settings.RiverWidthLower.Value, Settings.RiverWidthUpper.Value, 1 - PlayerScoreRating / Settings.PlayerScoreRatingMax);

                var nextStartingSlope = new Vector2(1f, lastBlock.Function.GetYPrimeForX(lastBlock.Function.DomainEnd));
                var newWalk = RandomWalk(nextStartingPoint, nextStartingSlope);
                var newBlock = new LevelBlock(newWalk, GetObstaclesForBlock(newWalk, lastBlock.Obstacles.Last().X), riverWidth, lastBlock.GetRiverWidth(lastBlock.Function.DomainEnd));

                Blocks.Add(newBlock);

                _nextGenerationPointX = (newWalk.DomainEnd + newWalk.DomainStart) / 2f;

                _renderer.OnNewGeneration(newBlock);
            }

            if (_playerCharacter.Position.X > _nextScorePointX)
            {
                _nextScorePointX = Blocks.Last().Function.DomainEnd;

                var potentialScore = PlayerScoreRating + 1;

                PlayerScoreRating = potentialScore <= Settings.PlayerScoreRatingMax ? potentialScore : Settings.PlayerScoreRatingMax;
            }
        }

        private float _scale;
        private ProceduralRenderer _renderer;
        private Entity _playerCharacter;
        private float _nextGenerationPointX;
        private float _nextScorePointX;

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

        private IFunction RandomWalk(Vector2 iStartingPoint, Vector2 iStartingSlope)
        {
            var rng = new System.Random();
            var walkPoints = new List<Vector2>();

            var numPoints = (int)MathHelper.Lerp(Settings.NumPointsPerBlockLower, Settings.NumPointsPerBlockUpper, PlayerScoreRating / Settings.PlayerScoreRatingMax);

            float y = 0;
            for (var ii = 0; ii < numPoints; ii++)
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

                y += dy;

                var thisPoint = new Vector2(ii, y);
                walkPoints.Add(thisPoint);
            }


            var yScaleDivisor = MathHelper.Lerp(
                Settings.YScaleDivisorLower, 
                Settings.YScaleDivisorUpper, 
                PlayerScoreRating / Settings.PlayerScoreRatingMax);
            var numTerms = PlayerScoreRating < 5f ? Settings.NumDTFTerms : Settings.NumDTFTerms + 1;
            return new DFTFunction(
                walkPoints, 
                numTerms, 
                iStartingPoint, 
                iStartingSlope,
                new Vector2(_scale, _scale / yScaleDivisor));
        }

        private List<Vector2> GetObstaclesForBlock(IFunction iFunction, float? iStartingPointX = null)
        {
            var rng = new System.Random();
            var obstacles = new List<Vector2>();

            var lastPointX = iStartingPointX ?? iFunction.DomainStart;

            var gapMin = MathHelper.Lerp(Settings.ObstacleXGapMinLower.Value, Settings.ObstacleXGapMinUpper.Value, 1 - PlayerScoreRating / Settings.PlayerScoreRatingMax);
            var gapMax = MathHelper.Lerp(Settings.ObstacleXGapMaxLower.Value, Settings.ObstacleXGapMaxUpper.Value, 1 - PlayerScoreRating / Settings.PlayerScoreRatingMax);
            var riverWidth = MathHelper.Lerp(Settings.RiverWidthLower.Value, Settings.RiverWidthUpper.Value, 1 - PlayerScoreRating / Settings.PlayerScoreRatingMax);

            while (lastPointX < iFunction.DomainEnd)
            {
                var thisPointX = lastPointX + gapMin + 
                                 (float)rng.NextDouble() * (gapMax - gapMin);

                if (thisPointX > iFunction.DomainEnd)
                    break;

                var thisPointY = iFunction.GetYForX(thisPointX) - (riverWidth / 2f) + _scale + ((float)rng.NextDouble() * (riverWidth - 2 * _scale));

                obstacles.Add(new Vector2(thisPointX, thisPointY));

                lastPointX = thisPointX;
            }

            return obstacles;
        }
    }
}
