using System;
using Microsoft.Xna.Framework;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class CharacterMovementComponent : Component, IUpdatable, IBeginPlay
    {
        private static class Settings
        {
            public static readonly RenderSetting SpeedDifMax = new(20);

            public const float MinimumSpeedAsPercentOfFlowSpeed = .5f;
            public static readonly RenderSetting Acceleration = new(20);

            public const float RotationRateDegreesPerSecondMin = 30f;
            public const float RotationRateDegreesPerSecondMax = 60f;
            public const float RowTime = .5f;
            public static readonly RenderSetting RotationDragGrowthSlope = new(.125f);

            public static readonly RenderSetting DragCoefficient = new(.005f);

            public const float RowTransition1 = .5f;
            public const float RowTransition2 = .9f;
            public const float RowTransition3 = 1.25f;

            public static readonly RenderSetting RowForceBad = new(40);
            public static readonly RenderSetting RowForceMedium = new(65);
            public static readonly RenderSetting RowForceGood = new(90);
            public static readonly RenderSetting RowForceNeutral = new(75);
        }

        public CharacterMovementComponent(Action<PrototypeCharacterComponent.State> iOnStateChangedCallback)
        {
            _stateChangedCallback = iOnStateChangedCallback;
            CurrentInput = new CharacterInputController.InputDescription();
            _currentState = PrototypeCharacterComponent.State.Idle;
            _currentVelocity = new Vector2(.01f, 0.0f);
            _thisIterationMotion = Vector2.Zero;
            _mover = new Mover();
            _subPixelV2 = new SubpixelVector2();
            LastRowTimeSecond = float.MinValue;
            _globalMaxPositionXAchievedSoFar = null;
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

            var thisBlock = _generator.GetBlockForPosition(Entity.Position);

            if (thisBlock == null)
                return;

            var previousState = _currentState;
            var forceVec = Vector2.Zero;

            var riverFlow = _generator.GetRiverVelocityAt(Entity.Position);

            var flowSpeed = riverFlow.Length();

            var flowDirectionVector = riverFlow;
            flowDirectionVector.Normalize();

            var flowPerpendicularDirection = GetClockwisePerpendicularUnitVector(flowDirectionVector);

            var playerVelocityToWaterSpeedDiffInPlayerFrame = _currentVelocity.Length() - ScalarProject(flowSpeed * flowDirectionVector, _currentVelocity);

            // Apply rotation input
            var lerpValue = MathHelper.Clamp(playerVelocityToWaterSpeedDiffInPlayerFrame / Settings.SpeedDifMax.Value, 0, 1);
            float rotationSpeed = MathHelper.Lerp(Settings.RotationRateDegreesPerSecondMin, Settings.RotationRateDegreesPerSecondMax, lerpValue);
            var rotationDegreesToApply = CurrentInput.Rotation * rotationSpeed * Time.DeltaTime;

            Entity.Transform.SetRotationDegrees(Entity.Transform.RotationDegrees + rotationDegreesToApply);

            var directionVector = GetRotationAsDirectionVector();
            directionVector.Normalize();

            if (CurrentInput.Rotation > 0.01f)
                _currentState = PrototypeCharacterComponent.State.TurnCw;
            else if (CurrentInput.Rotation < -0.01f)
                _currentState = PrototypeCharacterComponent.State.TurnCcw;
            else
                _currentState = PrototypeCharacterComponent.State.Idle;

            // Apply row input
            if (Time.TotalTime - LastRowTimeSecond < Settings.RowTime)
            {
                System.Diagnostics.Debug.Assert(_rowForceForCurrentRow.HasValue);

                var potentialRowForce = directionVector * _rowForceForCurrentRow.Value;
                var potentialVelocity = _currentVelocity + potentialRowForce * Time.DeltaTime;

                var currentVelocityProjectionOntoRiver = Vector2.Dot(_currentVelocity, flowDirectionVector) / flowDirectionVector.Length();
                var potentialVelocityProjectionOntoRiver = Vector2.Dot(potentialVelocity, flowDirectionVector) / flowDirectionVector.Length();

                if (potentialVelocityProjectionOntoRiver >= 0 ||
                    potentialVelocityProjectionOntoRiver > currentVelocityProjectionOntoRiver)
                {
                    forceVec += potentialRowForce;
                }
                else
                {
                    var parallelProjectionForceMag = Vector2.Dot(potentialRowForce, flowDirectionVector) / flowDirectionVector.Length();
                    var perpProjectionForce = potentialRowForce - parallelProjectionForceMag * flowDirectionVector / flowDirectionVector.Length();

                    forceVec += perpProjectionForce;
                }

                _currentState = PrototypeCharacterComponent.State.Row;
            }
            else
            {
                _rowForceForCurrentRow = null;

                if (previousState == PrototypeCharacterComponent.State.Row)
                    _currentState = PrototypeCharacterComponent.State.Idle;
                if (CurrentInput.Row)
                {
                    var timeDiff = Time.TotalTime - LastRowTimeSecond;
                    if (timeDiff < Settings.RowTransition1) 
                        _rowForceForCurrentRow = Settings.RowForceBad.Value;
                    else if (timeDiff < Settings.RowTransition2)
                        _rowForceForCurrentRow = Settings.RowForceMedium.Value;
                    else if (timeDiff < Settings.RowTransition3)
                        _rowForceForCurrentRow = Settings.RowForceGood.Value;
                    else
                        _rowForceForCurrentRow = Settings.RowForceNeutral.Value;

                    LastRowTimeSecond = Time.TotalTime; 
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
                
                forceVec += -flowDirectionVector * rotationDragForceParallel * Math.Abs(CurrentInput.Rotation);

                var playerPerpendicularVelocityInRiverFrame = ScalarProject(_currentVelocity, flowPerpendicularDirection);
                var rotationDragForcePerpendicular = Settings.RotationDragGrowthSlope.Value * playerPerpendicularVelocityInRiverFrame;
                forceVec += -flowPerpendicularDirection * rotationDragForcePerpendicular * Math.Abs(CurrentInput.Rotation);
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
            var dragForcePerpMag = .5f * Settings.DragCoefficient.Value * (1 - dotProductPerp) * perpendicularSpeed * perpendicularSpeed;

            Vector2 dragForcePerpVec;
            if (perpendicularSpeed > 0f)
                dragForcePerpVec = -flowPerpendicularDirection * dragForcePerpMag;
            else
                dragForcePerpVec = flowPerpendicularDirection * dragForcePerpMag;

            forceVec += dragForcePerpVec;

            // Apply river flow force
            forceVec += Settings.Acceleration.Value * flowDirectionVector * Math.Abs(dotProductParallel);

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
            if (_globalMaxPositionXAchievedSoFar.HasValue)
            {
                var diffX = Entity.Position.X - _globalMaxPositionXAchievedSoFar.Value;

                if (diffX > 0)
                {

                    var lastIterationFlowScalar = thisBlock.Function.GetYPrimeForX(_globalMaxPositionXAchievedSoFar.Value);

                    var arcLength = (float)Math.Sqrt(1 + lastIterationFlowScalar * lastIterationFlowScalar) * diffX;

                    TotalDistanceTraveled += arcLength;

                    _globalMaxPositionXAchievedSoFar = Entity.Position.X;
                }
            }
            else
            {
                _globalMaxPositionXAchievedSoFar = Entity.Position.X;
            }
        }

        public void OnPlayerInput(CharacterInputController.InputDescription iInput)
        {
            CurrentInput = iInput;
        }

        public float TotalDistanceTraveled { get; private set; }
        public Vector2 CurrentVelocity => _currentVelocity;
        public CharacterInputController.InputDescription CurrentInput { get; private set; }
        public float LastRowTimeSecond { get; private set; }

        private readonly Action<PrototypeCharacterComponent.State> _stateChangedCallback;
        private PrototypeCharacterComponent.State _currentState;
        private Vector2 _currentVelocity;
        private Vector2 _thisIterationMotion;
        private readonly Mover _mover;
        private SubpixelVector2 _subPixelV2;
        private ProceduralGeneratorComponent _generator;
        private CharacterCollisionComponent _collisionComponent;
        private float? _globalMaxPositionXAchievedSoFar;
        private float? _rowForceForCurrentRow;

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