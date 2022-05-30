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
        public DFTFunction(IEnumerable<Vector2> iPoints, int iNumTerms)
        {
            System.Diagnostics.Debug.Assert(iPoints != null);

            var pointsList = iPoints.ToList();

            System.Diagnostics.Debug.Assert(pointsList.Any());

            _numPoints = pointsList.Count;
            _avgY = pointsList.Average(p => p.Y);
            _startingX = pointsList[0].X;

            PrecomputeCoefficients(pointsList, iNumTerms);
        }

        public float GetYForX(float iX)
        {
            var transformedX = iX - _startingX;
            var yValue = _avgY;

            for (var ii = 0; ii < _aCoefficients.Count; ii++)
            {
                var term1 = _aCoefficients[ii] * (float)Math.Cos(2 * Math.PI * ii * transformedX / _numPoints);
                var term2 = _bCoefficients[ii] * (float)Math.Sin(2 * Math.PI * ii * transformedX / _numPoints);
                yValue += term1 + term2;
            }

            return yValue;
        }

        public float GetYPrimeForX(float iX)
        {
            var transformedX = iX - _startingX;
            var yValue = 0f;

            for (var ii = 0; ii < _aCoefficients.Count; ii++)
            {
                var chainRuleTerm = 2 * Math.PI * ii / _numPoints;
                var term1 = _aCoefficients[ii] * (float)Math.Sin(chainRuleTerm * transformedX) * (float)chainRuleTerm;
                var term2 = _bCoefficients[ii] * (float)Math.Cos(chainRuleTerm * transformedX) * (float)chainRuleTerm;
                yValue += term2 - term1;
            }

            return yValue;
        }

        private readonly int _numPoints;
        private readonly float _avgY;
        private readonly float _startingX;
        private List<float> _aCoefficients;
        private List<float> _bCoefficients;

        private void PrecomputeCoefficients(List<Vector2> iPoints, int iNumTerms)
        {
            _aCoefficients = new List<float>();
            _bCoefficients = new List<float>();
            for (var ii = 0; ii < iNumTerms; ii++)
            {
                var aCoefficient = 1 / _avgY;
                var bCoefficient = 1 / _avgY;
                for (var jj = 0; jj < _numPoints - 1; jj++)
                {
                    aCoefficient += (iPoints[jj + 1].Y + iPoints[jj].Y) * (float)Math.Cos(2 * Math.PI * ii * (jj + .5) / _numPoints);
                    bCoefficient += (iPoints[jj + 1].Y + iPoints[jj].Y) * (float)Math.Sin(2 * Math.PI * ii * (jj + .5) / _numPoints);
                }

                _aCoefficients.Add(aCoefficient);
                _bCoefficients.Add(bCoefficient);
            }
        }
    }
}