using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Raspberry_Lib.Maths
{
    /// <summary>
    /// Discrete Fourier Transform class that handles the computation for the fit line of the random walks
    /// </summary>
    internal class DFTFunction : IFunction
    {
        public DFTFunction(IEnumerable<Vector2> iPoints, int iNumTerms, Vector2 iStartingPoint, float iScale)
        {
            _scale = iScale;

            System.Diagnostics.Debug.Assert(iPoints != null);

            var pointsList = iPoints.ToList();

            System.Diagnostics.Debug.Assert(pointsList.Any());

            _numPoints = pointsList.Count;
            _startingPoint = iStartingPoint;

            _avgY = pointsList.Average(p => p.Y);

            PrecomputeCoefficients(pointsList, iNumTerms);

            DomainStart = pointsList.First().X * _scale + _startingPoint.X;
            DomainEnd = pointsList.Last().X * _scale + _startingPoint.X;
        }


        public float GetYForX(float iX)
        {
            var transformedX = (iX - _startingPoint.X) / _scale;
            var yValue = _avgY;

            for (var ii = 0; ii < _aCoefficients.Count; ii++)
            {
                var term1 = _aCoefficients[ii] * (float)Math.Cos(2 * Math.PI * ii * transformedX / _numPoints);
                var term2 = _bCoefficients[ii] * (float)Math.Sin(2 * Math.PI * ii * transformedX / _numPoints);
                yValue += term1 + term2;
            }

            return 2 * yValue * _scale + _startingPoint.Y;
        }

        public float GetYPrimeForX(float iX)
        {
            var transformedX = iX - _startingPoint.X;
            var yValue = 0f;

            for (var ii = 0; ii < _aCoefficients.Count; ii++)
            {
                var chainRuleTerm = 2 * Math.PI * ii / _numPoints;
                var term1 = _aCoefficients[ii] * (float)Math.Sin(chainRuleTerm * transformedX) * (float)chainRuleTerm;
                var term2 = _bCoefficients[ii] * (float)Math.Cos(chainRuleTerm * transformedX) * (float)chainRuleTerm;
                yValue += term2 - term1;
            }

            return yValue * _scale;
        }

        public float DomainStart { get; }
        public float DomainEnd { get; }

        private readonly float _scale;
        private readonly int _numPoints;
        private readonly float _avgY;
        private readonly Vector2 _startingPoint;
        private List<float> _aCoefficients;
        private List<float> _bCoefficients;

        private void PrecomputeCoefficients(List<Vector2> iPoints, int iNumTerms)
        {
            _aCoefficients = new List<float>();
            _bCoefficients = new List<float>();
            for (var ii = 0; ii < iNumTerms; ii++)
            {
                var aCoefficient = 0f;
                var bCoefficient = 0f;
                for (var jj = 0; jj < _numPoints - 1; jj++)
                {
                    aCoefficient += (iPoints[jj + 1].Y + iPoints[jj].Y) * (float)Math.Cos(2 * Math.PI * ii * (jj + .5) / _numPoints);
                    bCoefficient += (iPoints[jj + 1].Y + iPoints[jj].Y) * (float)Math.Sin(2 * Math.PI * ii * (jj + .5) / _numPoints);
                }

                _aCoefficients.Add((1f / _numPoints) * aCoefficient);
                _bCoefficients.Add((1f / _numPoints) * bCoefficient);
            }
        }
    }
}