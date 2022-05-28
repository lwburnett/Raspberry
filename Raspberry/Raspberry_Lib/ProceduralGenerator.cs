using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Raspberry_Lib
{
    internal class ProceduralGenerator
    {
        private static class Settings
        {
            public static readonly RenderSetting DeltaX = new(16f);
            public static readonly RenderSetting DeltaY = new(16f);
            public static readonly int NumPointsPerBlock = 50;
        }

        public ProceduralGenerator(Vector2 iCharacterStartingPos)
        {
            _points = new List<List<Vector2>>();

            var leadingPoints = new List<Vector2>();
            for (var ii = 0; ii < 4; ii++)
            {
                var thisPoint = new Vector2(iCharacterStartingPos.X - ((4 - ii) * Settings.DeltaX.Value), iCharacterStartingPos.Y);
                leadingPoints.Add(thisPoint);
            }

            _points.Add(leadingPoints);

            _points.Add(RandomWalk(iCharacterStartingPos));

            // TODO: Finish this class
            // Fit line to points

            // Write function to give Y based on passed-in X point
            //      Use this function in ProceduralRenderer to render tiles
        }

        private List<List<Vector2>> _points;

        private static List<Vector2> RandomWalk(Vector2 iStartingPoint)
        {
            var rng = new Random();
            var walkPoints = new List<Vector2>();

            for (var ii = 0; ii < Settings.NumPointsPerBlock; ii++)
            {
                var relativeYPoint = ii == 0 ? iStartingPoint.Y : walkPoints[ii - 1].Y;

                float dy;
                var randomNumber = rng.Next() % 3;
                if (randomNumber == 0)
                {
                    dy = -Settings.DeltaY.Value;
                }
                else if (randomNumber == 1)
                {
                    dy = 0;
                }
                else
                {
                    dy = Settings.DeltaY.Value;
                }

                var thisPoint = new Vector2(iStartingPoint.X + (ii * Settings.DeltaX.Value), relativeYPoint + dy);
                walkPoints.Add(thisPoint);
            }

            return walkPoints;
        }
    }
}
