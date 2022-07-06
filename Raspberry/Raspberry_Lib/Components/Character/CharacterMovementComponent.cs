using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class CharacterMovementComponent : Component, IUpdatable, IBeginPlay
    {
        private static class Settings
        {
            public static readonly RenderSetting FlowSpeedLower = new(40);
            public static readonly RenderSetting FlowSpeedUpper = new(100);
            public static readonly RenderSetting SpeedDifMax = new(25);

            public const float MinimumSpeedAsPercentOfFlowSpeed = .5f;
            public static readonly RenderSetting Acceleration = new(10);

            public const float RotationRateDegreesPerSecondMin = 30f;
            public const float RotationRateDegreesPerSecondMax = 60f;
            public static readonly RenderSetting RowForce = new(75);
            public static readonly TimeSpan RowTime = TimeSpan.FromSeconds(.5);
            public static readonly RenderSetting RotationDragGrowthSlope = new(.25f);

            public static readonly RenderSetting DragCoefficient = new(.01f);
        }

        public CharacterMovementComponent(Action<PrototypeCharacterComponent.State> iOnStateChangedCallback)
        {
            _stateChangedCallback = iOnStateChangedCallback;
            _currentInput = new CharacterInputController.InputDescription();
            _currentState = PrototypeCharacterComponent.State.Idle;
            _currentVelocity = new Vector2(.01f, 0.0f);
            _thisIterationMotion = Vector2.Zero;
            _mover = new Mover();
            _subPixelV2 = new SubpixelVector2();
            _lastRowTimeSeconds = float.MinValue;
            _lastIterationPositionX = null;
            TotalDistanceTraveled = 0;

#if VERBOSE
            Verbose.TrackMetric(() => _currentVelocity.Length(), v => $"SpeedT: {v:G6}");
            Verbose.TrackMetric(() => _currentVelocity.X, v => $"SpeedX: {v:G6}");
            Verbose.TrackMetric(() => _currentVelocity.Y, v => $"SpeedY: {v:G6}");
#endif
        }

        public int BeginPlayOrder => 98;

        public override void OnAddedToEntity()
        {
            Entity.AddComponent(_mover);
        }

        public void OnBeginPlay()
        {
            _generator = Entity.Scene.FindEntity("map")?.GetComponent<ProceduralGeneratorComponent>();

            System.Diagnostics.Debug.Assert(_generator != null);

            _collisionComponent = Entity.GetComponent<CharacterCollisionComponent>();
        }

        public void Update()
        {
            if (_generator == null)
                return;

            var thisBlock = _generator.Blocks.
                FirstOrDefault(f =>
                    f.Function.DomainStart < Entity.Position.X &&
                    Entity.Position.X <= f.Function.DomainEnd);

            if (thisBlock == null)
                return;

            var previousState = _currentState;
            var forceVec = Vector2.Zero;

            var flowDirectionScalar = thisBlock.Function.GetYPrimeForX(Entity.Position.X);

            var flowDirectionVector = new Vector2(1f, flowDirectionScalar);
            flowDirectionVector.Normalize();

            var flowPerpendicularDirection = GetClockwisePerpendicularUnitVector(flowDirectionVector);

            var flowSpeed = MathHelper.Lerp(
                Settings.FlowSpeedLower.Value,
                Settings.FlowSpeedUpper.Value,
                1 - _generator.PlayerScoreRating / _generator.PlayerScoreRatingMax);

            var playerVelocityToWaterSpeedDiffInPlayerFrame = _currentVelocity.Length() - ScalarProject(flowSpeed * flowDirectionVector, _currentVelocity);

            // Apply rotation input
            var lerpValue = MathHelper.Clamp(playerVelocityToWaterSpeedDiffInPlayerFrame / Settings.SpeedDifMax.Value, 0, 1);
            float rotationSpeed = MathHelper.Lerp(Settings.RotationRateDegreesPerSecondMin, Settings.RotationRateDegreesPerSecondMax, lerpValue);
            var rotationDegreesToApply = _currentInput.Rotation * rotationSpeed * Time.DeltaTime;

            Entity.Transform.SetRotationDegrees(Entity.Transform.RotationDegrees + rotationDegreesToApply);

            var directionVector = GetRotationAsDirectionVector();
            directionVector.Normalize();

            if (_currentInput.Rotation > 0.01f)
                _currentState = PrototypeCharacterComponent.State.TurnCw;
            else if (_currentInput.Rotation < -0.01f)
                _currentState = PrototypeCharacterComponent.State.TurnCcw;
            else
                _currentState = PrototypeCharacterComponent.State.Idle;

            // Apply row input
            if (Time.TotalTime - _lastRowTimeSeconds < Settings.RowTime.TotalSeconds)
            {
                forceVec += directionVector * Settings.RowForce.Value;

                _currentState = PrototypeCharacterComponent.State.Row;
            }
            else
            {
                if (previousState == PrototypeCharacterComponent.State.Row)
                    _currentState = PrototypeCharacterComponent.State.Idle;
                if (_currentInput.Row)
                {
                    _lastRowTimeSeconds = Time.TotalTime; 
                    if (previousState == PrototypeCharacterComponent.State.Idle)
                        _currentState = PrototypeCharacterComponent.State.Row;
                }
            }

            // Apply rotation drag force
            if (Math.Abs(rotationSpeed) > 0f)
            {
                var playerParallelVelocityToWaterSpeedDiffInRiverFrame = ScalarProject(_currentVelocity, flowDirectionVector) - flowSpeed;

                float rotationDragForceParallel;
                if (playerParallelVelocityToWaterSpeedDiffInRiverFrame <= 0)
                {
                    rotationDragForceParallel = 0;
                }
                else
                {
                    rotationDragForceParallel = Settings.RotationDragGrowthSlope.Value * playerParallelVelocityToWaterSpeedDiffInRiverFrame;
                }
                
                forceVec += -flowDirectionVector * rotationDragForceParallel * Math.Abs(_currentInput.Rotation);

                var playerPerpendicularVelocityInRiverFrame = ScalarProject(_currentVelocity, flowPerpendicularDirection);
                var rotationDragForcePerpendicular = Settings.RotationDragGrowthSlope.Value * playerPerpendicularVelocityInRiverFrame;
                forceVec += -flowPerpendicularDirection * rotationDragForcePerpendicular * Math.Abs(_currentInput.Rotation);
            }

            // Apply river drag force
            var dotProductParallel = Vector2.Dot(directionVector, flowDirectionVector);

            var currentTopSpeedParallel = (flowSpeed * Settings.MinimumSpeedAsPercentOfFlowSpeed) * (dotProductParallel + 1f);

            var currentParallelSpeed = Vector2.Dot(_currentVelocity, flowDirectionVector);
            var parallelSpeedDif = currentParallelSpeed - currentTopSpeedParallel;

            if (parallelSpeedDif > 0f)
            {
                var dragForceMag = .5f * Settings.DragCoefficient.Value * (1 - dotProductParallel / 2) * parallelSpeedDif * parallelSpeedDif;

                var dragForceVec = -dragForceMag * flowDirectionVector;

                forceVec += dragForceVec;
            }

            var dotProductPerp = Vector2.Dot(directionVector, flowPerpendicularDirection);
            var perpendicularSpeed = Vector2.Dot(_currentVelocity, flowPerpendicularDirection);
            var dragForcePerpMag = Settings.DragCoefficient.Value * (1 - dotProductPerp) * perpendicularSpeed * perpendicularSpeed;

            Vector2 dragForcePerpVec;
            if (perpendicularSpeed > 0f)
                dragForcePerpVec = -flowPerpendicularDirection * dragForcePerpMag;
            else
                dragForcePerpVec = flowPerpendicularDirection * dragForcePerpMag;

            forceVec += dragForcePerpVec;

            // Apply river flow force
            forceVec += Settings.Acceleration.Value * flowDirectionVector * dotProductParallel;

            if (dotProductPerp > 0)
            {
                forceVec += .5f * Settings.Acceleration.Value * flowPerpendicularDirection * dotProductPerp;
            }
            else
            {
                forceVec += .5f * Settings.Acceleration.Value * -flowPerpendicularDirection * dotProductPerp;
            }

            // Apply accumulated forces
            _currentVelocity += forceVec * Time.DeltaTime;
            
            if (_currentState != previousState)
                _stateChangedCallback(_currentState);

            _thisIterationMotion = _currentVelocity * Time.DeltaTime;
            _mover.CalculateMovement(ref _thisIterationMotion, out var collisionResult);
            _subPixelV2.Update(ref _thisIterationMotion);
            _mover.ApplyMovement(_thisIterationMotion);
            
            _collisionComponent.HandleCollision(collisionResult);

            // Publish distance traveled results
            if (_lastIterationPositionX.HasValue)
            {
                var diffX = Entity.Position.X - _lastIterationPositionX.Value;

                if (diffX > 0)
                {

                    var lastIterationFlowScalar = thisBlock.Function.GetYPrimeForX(_lastIterationPositionX.Value);

                    var arcLength = (float)Math.Sqrt(1 + lastIterationFlowScalar * lastIterationFlowScalar) * diffX;

                    TotalDistanceTraveled += arcLength;
                }
            }

            _lastIterationPositionX = Entity.Position.X;
        }

        public void OnPlayerInput(CharacterInputController.InputDescription iInput)
        {
            _currentInput = iInput;
        }

        public float TotalDistanceTraveled { get; private set; }

        private readonly Action<PrototypeCharacterComponent.State> _stateChangedCallback;
        private CharacterInputController.InputDescription _currentInput;
        private PrototypeCharacterComponent.State _currentState;
        private Vector2 _currentVelocity;
        private Vector2 _thisIterationMotion;
        private readonly Mover _mover;
        private SubpixelVector2 _subPixelV2;
        private ProceduralGeneratorComponent _generator;
        private float _lastRowTimeSeconds;
        private CharacterCollisionComponent _collisionComponent;
        private float? _lastIterationPositionX;

        private Vector2 GetRotationAsDirectionVector()
        {
            var rotation = Entity.Transform.Rotation;

            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }

        private static float ScalarProject(Vector2 iVecA, Vector2 iVecB) => Vector2.Dot(iVecA, iVecB) / iVecB.Length();

        private static Vector2 GetClockwisePerpendicularUnitVector(Vector2 iVec)
        {
            var newVec = new Vector2(-iVec.Y, iVec.X);
            newVec.Normalize();
            return newVec;
        }
    }
}