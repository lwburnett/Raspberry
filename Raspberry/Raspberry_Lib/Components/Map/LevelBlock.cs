using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Raspberry_Lib.Maths;

namespace Raspberry_Lib.Components
{
    internal class LevelBlock
    {
        private static class Settings
        {
            public static readonly RenderSetting RiverWidthTransitionPeriod = new(400f);
        }

        public LevelBlock(IFunction iFunction, IReadOnlyCollection<Vector2> iObstacles, float iRiverWidth, float? iPreviousBlockWidth = null)
        {
            Function = iFunction;
            Obstacles = iObstacles;
            _riverWidth = iRiverWidth;
            _previousBlockWidth = iPreviousBlockWidth ?? iRiverWidth;
        }

        public IFunction Function { get; }
        public IReadOnlyCollection<Vector2> Obstacles { get; }

        public float GetRiverWidth(float iXPos)
        {
            if (iXPos <= Function.DomainStart)
            {
                return _previousBlockWidth;
            }
            else if (iXPos >= Function.DomainEnd)
            {
                return _riverWidth;
            }
            else
            {
                var lerpValue = MathHelper.Clamp(
                    (iXPos - Function.DomainStart) / Settings.RiverWidthTransitionPeriod.Value,
                    0f, 1f);

                return MathHelper.Lerp(_previousBlockWidth, _riverWidth, lerpValue);
            }
        }

        private readonly float _riverWidth;
        private readonly float _previousBlockWidth;
    }
}