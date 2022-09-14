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
        public DFTFunction(IEnumerable<Vector2> iPoints, int iNumTerms, Vector2 iStartingPoint, Vector2 iStartingSlope, Vector2 iScale)
        {
            _scale = iScale;

            System.Diagnostics.Debug.Assert(iPoints != null);

            var pointsList = iPoints.ToList();

            System.Diagnostics.Debug.Assert(pointsList.Any());

            _numPoints = pointsList.Count;

            _avgY = pointsList.Average(p => p.Y);

            PrecomputeCoefficients(pointsList, iNumTerms);

            var dftStartingSlope = DFTGetYPrimeForXUntransformed(0f);
            ComputeTransitionValues(iStartingPoint, iStartingSlope, dftStartingSlope);

            _dftDomainStart = _transitionDomainEnd;
            _dftDomainEnd = pointsList.Last().X * _scale.X + _dftDomainStart;
            
            var transitionEndPointY = TransitionGetYForX(_transitionDomainEnd);
            var transitionEndPointSlope = TransitionGetYPrimeForX(_transitionDomainEnd);
            var currentStartPointY = DFTGetYForXUntransformed(0f);
            _dftRangeStart = transitionEndPointY - currentStartPointY + (transitionEndPointSlope / 8);
        }


        public float GetYForX(float iX)
        {
            if (iX < _dftDomainStart)
                return TransitionGetYForX(iX);
            else
                return DFTGetYForXUntransformed(iX - _dftDomainStart) + _dftRangeStart;
        }

        public float GetYPrimeForX(float iX)
        {
            if (iX < _dftDomainStart)
                return TransitionGetYPrimeForX(iX);
            else
                return DFTGetYPrimeForXUntransformed(iX - _dftDomainStart);
        }

        public float DomainStart => _transitionDomainStart;
        public float DomainEnd => _dftDomainEnd;

        private readonly Vector2 _scale;
        private readonly int _numPoints;
        private readonly float _avgY;
        private List<float> _aCoefficients;
        private List<float> _bCoefficients;
        private readonly float _dftRangeStart;
        private readonly float _dftDomainStart;
        private readonly float _dftDomainEnd;

        private List<float> _transitionPolynomialCoefficients;
        private float _transitionDomainStart;
        private float _transitionDomainEnd;

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

        private void ComputeTransitionValues(Vector2 iStartingPoint, Vector2 iStartingSlope, float iEndingSlope)
        {
            _transitionPolynomialCoefficients = new List<float>();

            var startingSlope = (iStartingSlope / iStartingSlope.X).Y;
            _transitionPolynomialCoefficients.Add(iStartingPoint.Y + startingSlope / 8);
            _transitionPolynomialCoefficients.Add(startingSlope);

            var deltaSlopePerUnit = .001f / PlatformUtils.GetRenderScale();
            var leadingCoefficient = iEndingSlope > startingSlope ? deltaSlopePerUnit / 2 : -deltaSlopePerUnit / 2;
            _transitionPolynomialCoefficients.Add(leadingCoefficient);

            _transitionDomainStart = iStartingPoint.X;
            _transitionDomainEnd = iStartingPoint.X + Math.Abs(iEndingSlope - startingSlope) / deltaSlopePerUnit;
        }

        private float DFTGetYForXUntransformed(float iX)
        {
            var transformedX = iX / _scale.X;
            var yValue = _avgY;

            for (var ii = 0; ii < _aCoefficients.Count; ii++)
            {
                var term1 = _aCoefficients[ii] * (float)Math.Cos(2 * Math.PI * ii * transformedX / _numPoints);
                var term2 = _bCoefficients[ii] * (float)Math.Sin(2 * Math.PI * ii * transformedX / _numPoints);
                yValue += term1 + term2;
            }

            return yValue * _scale.Y;
        }

        private float DFTGetYPrimeForXUntransformed(float iX)
        {
            var transformedX = iX / _scale.X;
            var yValue = 0f;

            for (var ii = 0; ii < _aCoefficients.Count; ii++)
            {
                var chainRuleTerm = 2 * Math.PI * ii / _numPoints;
                var term1 = _aCoefficients[ii] * (float)Math.Sin(chainRuleTerm * transformedX) * (float)chainRuleTerm;
                var term2 = _bCoefficients[ii] * (float)Math.Cos(chainRuleTerm * transformedX) * (float)chainRuleTerm;
                yValue += term2 - term1;
            }

            return yValue  * _scale.Y / _scale.X;
        }

        private float TransitionGetYForX(float iX)
        {
            var x = iX - _transitionDomainStart;

            var numTerms = _transitionPolynomialCoefficients.Count;

            var sum = 0f;
            for (var ii = 0; ii < numTerms; ii++)
            {
                var coefficient = _transitionPolynomialCoefficients[ii];

                var product = 1f;
                for (var jj = 0; jj < ii; jj++)
                {
                    product *= x;
                }

                sum += coefficient * product;
            }

            return sum;
        }

        private float TransitionGetYPrimeForX(float iX)
        {
            var x = iX - _transitionDomainStart;

            var numTerms = _transitionPolynomialCoefficients.Count;

            var sum = 0f;
            for (var ii = numTerms - 1; ii > 0; ii--)
            {
                var coefficient = _transitionPolynomialCoefficients[ii];

                var product = 1f;
                for (var jj = 0; jj < ii - 1; jj++)
                {
                    product *= x;
                }

                sum += ii * coefficient * product;
            }

            return sum;
        }
    }
}