using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace Raspberry_Lib.Components
{
    internal class CharacterAnimationComponent : RenderableComponent, IUpdatable
    {
        private static class Settings
        {
            public const int CellWidth = 80;
            public const int CellHeight = 30;

            public const float AmplitudeMax = 11.49f;

            public const float WaveLengthSec = 2f;
            public const float DampingFactor = .5f;
            
            public const float RowTransition1 = .5f;
            public const float RowTransition2 = .9f;
            public const float RowTransition3 = 1.25f;

            public const float RowTime = .5f;
            public const float RowTorque = 10f;
        }

        public CharacterAnimationComponent()
        {
            RenderLayer = 4;
            _omega = MathHelper.TwoPi / Settings.WaveLengthSec;

            _rowStartTime = null;
            _turnStartTime = null;
            _turnIsClockwise = null;

            _rng = new System.Random();
            _amplitude = 0f;
            _phase = 0f;

            _timeOfFreedom = 0f;
        }

        public override void OnAddedToEntity()
        {
            var texture = Entity.Scene.Content.LoadTexture(Content.ContentData.AssetPaths.CharacterSpriteSheet);
            _sprites = Sprite.SpritesFromAtlas(texture, Settings.CellWidth, Settings.CellHeight);

            _currentSprite = _sprites[0];
            _spriteEffect = SpriteEffects.None;

            _movementComponent = Entity.GetComponent<CharacterMovementComponent>();
            _proceduralGenerator = Entity.Scene.FindEntity("map").GetComponent<ProceduralGeneratorComponent>();
        }

        public void Update()
        {
            var input = _movementComponent.CurrentInput;

            // Set row values
            if (input.Row)
            {
                if (!_rowStartTime.HasValue)
                {
                    _rowStartTime = Time.TotalTime;
                    _rowForceDirection = _rng.Next() % 2 == 0 ? 1 : -1;
                }
            }

            // Clear out rowing values if not rowing
            if (_rowStartTime.HasValue && Time.TotalTime - _rowStartTime.Value > Settings.RowTime)
                _rowStartTime = null;

            // Set turning values
            if (input.Rotation > 0)
            {
                if (!_turnIsClockwise.HasValue || !_turnIsClockwise.Value)
                {
                    _turnStartTime = Time.TotalTime;
                    _turnIsClockwise = true;
                }
            }
            else if (input.Rotation < 0)
            {
                if (!_turnIsClockwise.HasValue || _turnIsClockwise.Value)
                {
                    _turnStartTime = Time.TotalTime;
                    _turnIsClockwise = false;
                }
            }
            else
            {
                // Clear out turning values if not turning
                _turnStartTime = null;
                _turnIsClockwise = null;
            }
            
            // Accumulate torque
            var torque = 0f;

            // Apply torque if rowing
            if (_rowStartTime.HasValue)
                torque += Settings.RowTorque * _rowForceDirection;

            // Apply torque if turning
            // todo

            // Calculate new values given a non-zero torque
            if (torque != 0)
            {
                _phase = CalculatePhase(torque);
                _amplitude = CalculateAmplitude(torque, _phase);
                _timeOfFreedom = Time.TotalTime;
            }

            // Apply torque to boat
            var deltaT = Time.TotalTime - _timeOfFreedom;

            var oscillationValue = DampedOscillationFunction(deltaT, _amplitude, _phase);
            var clampedOscillation = MathHelper.Clamp(oscillationValue, -Settings.AmplitudeMax, Settings.AmplitudeMax);

            var index = (int)Math.Round(clampedOscillation);

            if (index >= 0)
            {
                _currentSprite = _sprites[index];
                _spriteEffect = SpriteEffects.None;
            }
            else
            {
                _currentSprite = _sprites[-index];
                _spriteEffect = SpriteEffects.FlipVertically;
            }
        }

        public override float Width => 600f;
        public override float Height => 600f;

        public override void Render(Batcher batcher, Camera camera)
        {
            batcher.Draw(_currentSprite, Entity.Transform.Position + LocalOffset, Color,
                Entity.Transform.Rotation, _currentSprite.Origin, Entity.Transform.Scale, _spriteEffect, _layerDepth);
        }

        private List<Sprite> _sprites;
        private Sprite _currentSprite;
        private SpriteEffects _spriteEffect;
        private readonly System.Random _rng;
        private readonly float _omega;

        private float _amplitude;
        private float _phase;

        private CharacterMovementComponent _movementComponent;
        private ProceduralGeneratorComponent _proceduralGenerator;

        private float? _rowStartTime;
        private float _rowForceDirection;
        private float? _turnStartTime;
        private bool? _turnIsClockwise;

        private float _timeOfFreedom;

        private float DampedOscillationFunction(float iTimeSec, float iAmplitude, float iPhase)
        {
            var cos = (float)Math.Cos(iTimeSec * _omega + iPhase);
            var egt = (float)Math.Pow(MathHelper.E, -Settings.DampingFactor * iTimeSec);

            return iAmplitude * egt * cos;
        }

        private float DampedOscillationDoublePrimeFunction(float iTimeSec, float iAmplitude, float iPhase)
        {
            var sin = (float)Math.Sin(iTimeSec * _omega + iPhase);
            var cos = (float)Math.Cos(iTimeSec * _omega + iPhase);
            var egt = (float)Math.Pow(MathHelper.E, -Settings.DampingFactor * iTimeSec);

            var term1 = Settings.DampingFactor * Settings.DampingFactor * egt * sin;
            var term2 = 2 * Settings.DampingFactor * _omega * egt * cos;
            var term3 = -_omega * _omega * egt * sin;

            return iAmplitude * (term1 + term2 + term3);
        }

        private float CalculateAmplitude(float iTorque, float iPhase)
        {
            var t0 = Time.TotalTime - _timeOfFreedom;

            // ReSharper disable InconsistentNaming
            var f0t0 = DampedOscillationFunction(t0, _amplitude, _phase);
            var f0t0PrimePrime = DampedOscillationDoublePrimeFunction(t0, _amplitude, _phase);
            // ReSharper restore InconsistentNaming
            var forceSum = f0t0PrimePrime + iTorque;

            var numerator = f0t0 + Time.DeltaTime * Time.DeltaTime * forceSum;
            var denominator = (float)Math.Cos(iPhase);

            return numerator / denominator;
        }

        private float CalculatePhase(float iTorque)
        {
            var t0 = Time.TotalTime - _timeOfFreedom;

            // ReSharper disable InconsistentNaming
            var f0t0 = DampedOscillationFunction(t0, _amplitude, _phase);
            var f0t0PrimePrime = DampedOscillationDoublePrimeFunction(t0, _amplitude, _phase);
            // ReSharper restore InconsistentNaming
            var forceSum = f0t0PrimePrime + iTorque;

            var nTerm1 = Settings.DampingFactor * f0t0;
            var nTerm2 = Settings.DampingFactor * Time.DeltaTime * Time.DeltaTime * forceSum;
            var nTerm3 = Time.DeltaTime * forceSum;

            var dTerm1 = _omega * f0t0;
            var dTerm2 = _omega * Time.DeltaTime * Time.DeltaTime * forceSum;

            var numerator = nTerm1 + nTerm2 + nTerm3;
            var denominator = dTerm1 + dTerm2;

            var fraction = -numerator / denominator;

            return (float)Math.Atan(fraction);
        }

        private Vector2 GetRotationAsDirectionVector()
        {
            var rotation = Entity.Transform.Rotation;

            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }

        private static float ScalarProject(Vector2 iVecA, Vector2 iVecB) => Vector2.Dot(iVecA, iVecB) / iVecB.Length();
    }
}