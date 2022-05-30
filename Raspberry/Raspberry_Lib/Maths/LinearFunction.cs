using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Raspberry_Lib.Maths
{
    internal class LinearFunction : IFunction
    {
        public LinearFunction(IEnumerable<Vector2> iPoints)
        {
            System.Diagnostics.Debug.Assert(iPoints != null);

            var pointsList = iPoints.ToList();

            System.Diagnostics.Debug.Assert(pointsList.Count >= 2);

            var point1 = pointsList[0];
            var point2 = pointsList[1];
            _slope = (point2.Y - point1.Y) / (point2.X - point1.X);
            _intercept = point1.Y - _slope * point1.X;
        }

        public float GetYForX(float iX)
        {
            return _slope * iX + _intercept;
        }

        public float GetYPrimeForX(float iX)
        {
            return _slope;
        }

        private readonly float _slope;
        private readonly float _intercept;
    }
}
