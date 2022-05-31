using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raspberry_Lib.Maths;

namespace Raspberry_Lib
{
    internal class ProceduralGenerator
    {
        private static class Settings
        {
            public const int NumPointsPerBlock = 100;

            public const int NumDTFTerms = 5;
        }

        public ProceduralGenerator(Vector2 iCharacterStartingPos, float iScale)
        {
            _scale = iScale;

            Functions = new List<IFunction>
            {
                LeadingPoints(iCharacterStartingPos),
                RandomWalk(iCharacterStartingPos)
            };

            // Write function to give Y based on passed-in X point
            //      Use this function in ProceduralRenderer to render tiles
        }

        public List<IFunction> Functions { get; }

        private readonly float _scale;

        private IFunction LeadingPoints(Vector2 iStartingPoint)
        {
            var leadingPoints = new List<Vector2>();
            for (var ii = 0; ii < 4; ii++)
            {
                var thisPoint = new Vector2(iStartingPoint.X - ((4 - ii) * _scale), iStartingPoint.Y);
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
