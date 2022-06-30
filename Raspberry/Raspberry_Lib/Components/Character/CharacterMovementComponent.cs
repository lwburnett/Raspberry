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
            public static readonly RenderSetting FlowSpeedLower = new(100);
            public static readonly RenderSetting FlowSpeedUpper = new(200);
            public static readonly RenderSetting SpeedDifMax = new(150);

            public const float MinimumSpeedAsPercentOfFlowSpeed = .5f;
            public static readonly RenderSetting Acceleration = new(50);

            public const float RotationRateDegreesPerSecondMin = 30f;
            public const float RotationRateDegreesPerSecondMax = 60f;
            public static readonly RenderSetting RowForce = new(75);
            public static readonly TimeSpan RowTime = TimeSpan.FromSeconds(.5);
            public static readonly RenderSetting RotationDragGrowthSlope = new(2f);

            public static readonly RenderSetting DragCoefficient = new(.0005f);
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

            var thisFunction = _generator.Blocks.
                FirstOrDefault(f =>
                    f.Function.DomainStart < Entity.Position.X &&
                    Entity.Position.X <= f.Function.DomainEnd);

            if (thisFunction == null)
                return;

            var previousState = _currentState;
            var forceVec = Vector2.Zero;

            var flowDirectionScalar = thisFunction.Function.GetYPrimeForX(Entity.Position.X);

            var flowDirectionVector = new Vector2(1f, flowDirectionScalar);
            flowDirectionVector.Normalize();

            var flowSpeed = MathHelper.Lerp(
                Settings.FlowSpeedLower.Value,
                Settings.FlowSpeedUpper.Value,
                1 - _generator.PlayerScoreRating / 9f);

            var playerVelocityToWaterSpeedDiffInPlayerFrame = _currentVelocity.Length() - ScalarProject(flowDirectionVector, _currentVelocity);

            // Apply rotation input
            var lerpValue = Math.Clamp(playerVelocityToWaterSpeedDiffInPlayerFrame / Settings.SpeedDifMax.Value, 0, 1);
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
                var playerVelocityToWaterSpeedDiffInRiverFrame = ScalarProject(_currentVelocity, flowDirectionVector) - flowSpeed;

                float rotationDragForce;
                if (playerVelocityToWaterSpeedDiffInRiverFrame <= 0)
                {
                    rotationDragForce = 0;
                }
                else
                {
                    rotationDragForce = Settings.RotationDragGrowthSlope.Value * playerVelocityToWaterSpeedDiffInRiverFrame;
                }
                
                forceVec += -flowDirectionVector * rotationDragForce * Math.Abs(_currentInput.Rotation);
            }

            // Apply river flow force
            var dotProduct = Vector2.Dot(directionVector, flowDirectionVector);
            
            var currentTopSpeedParallel = (flowSpeed * Settings.MinimumSpeedAsPercentOfFlowSpeed) * (dotProduct + 1f);
            
            var currentParallelSpeed = Vector2.Dot(_currentVelocity, flowDirectionVector);
            if (currentParallelSpeed < currentTopSpeedParallel)
            {
                // Parallel
                forceVec += Settings.Acceleration.Value * directionVector * dotProduct;
            }
            else
            {
                var speedDif = currentParallelSpeed - currentTopSpeedParallel;
                var dragForceMag = .5f * Settings.DragCoefficient.Value * (1 - .75f * dotProduct) * speedDif * speedDif;

                var dragForceVec = - dragForceMag * flowDirectionVector;

                forceVec += dragForceVec;
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
        }

        public void OnPlayerInput(CharacterInputController.InputDescription iInput)
        {
            _currentInput = iInput;
        }


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

        private Vector2 GetRotationAsDirectionVector()
        {
            var rotation = Entity.Transform.Rotation;

            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }

        private static float ScalarProject(Vector2 iVecA, Vector2 iVecB) => Vector2.Dot(iVecA, iVecB) / iVecB.Length();
    }
}