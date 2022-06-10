using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raspberry_Lib.Maths;

namespace Raspberry_Lib.Components
{
    internal class LevelBlock
    {
        public LevelBlock(IFunction iFunction, float iRiverWidth, IReadOnlyCollection<Vector2> iObstacles)
        {
            Function = iFunction;
            RiverWidth = iRiverWidth;
            Obstacles = iObstacles;
        }

        public IFunction Function { get; }
        public float RiverWidth { get; }
        public IReadOnlyCollection<Vector2> Obstacles { get; }
    }
}