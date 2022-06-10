﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class CharacterMovementComponent : Component, IUpdatable, IBeginPlay
    {
        private static class Settings
        {
            public static readonly RenderSetting FlowSpeed = new(75);
            public const float MinimumSpeedAsPercentOfFlowSpeed = .5f;
            public static readonly RenderSetting Acceleration = new(50);

            public const float RotationRateDegreesPerSecond = 45f;
            public static readonly RenderSetting RowForce = new(75);
            public static readonly TimeSpan RowTime = TimeSpan.FromSeconds(.5);

            public static readonly RenderSetting DragCoefficient = new(.0005f);
        }

        public CharacterMovementComponent(Action<PrototypeCharacterComponent.State> iOnStateChangedCallback)
        {
            _stateChangedCallback = iOnStateChangedCallback;
            _currentInput = new CharacterInputController.InputDescription();
            _currentState = PrototypeCharacterComponent.State.Idle;
            _currentCollision = new CollisionResult();
            _currentVelocity = new Vector2(.01f, 0.0f);
            _subPixelV2 = new SubpixelVector2();
            _lastRowTimeSeconds = float.MinValue;
        }

        public int BeginPlayOrder => 98;

        public void OnBeginPlay()
        {
            _generator = Entity.Scene.FindEntity("map")?.GetComponent<ProceduralGeneratorComponent>();

            System.Diagnostics.Debug.Assert(_generator != null);
        }

        public void Update()
        {
            if (_generator == null)
                return;

            var previousState = _currentState;
            var forceVec = Vector2.Zero;

            // Apply rotation input
            var rotationDegreesToApply = _currentInput.Rotation * Settings.RotationRateDegreesPerSecond * Time.DeltaTime;
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
            
            // Apply river flow force
            var thisFunction = _generator.Blocks.
                FirstOrDefault(f => 
                    f.Function.DomainStart < Entity.Position.X &&
                    Entity.Position.X <= f.Function.DomainEnd);
            
            if (thisFunction == null)
                return;
            
            var flowDirectionScalar = thisFunction.Function.GetYPrimeForX(Entity.Position.X);
            
            var flowDirectionVector = new Vector2(1f, flowDirectionScalar);
            flowDirectionVector.Normalize();

            var dotProduct = Vector2.Dot(directionVector, flowDirectionVector);
            
            var currentTopSpeedParallel = (Settings.FlowSpeed.Value * Settings.MinimumSpeedAsPercentOfFlowSpeed) * (dotProduct + 1f);
            
            var currentParallelSpeed = Vector2.Dot(_currentVelocity, flowDirectionVector);
            if (currentParallelSpeed < currentTopSpeedParallel)
            {
                forceVec += Settings.Acceleration.Value * flowDirectionVector;
            }
            else
            {
                var speedDif = currentParallelSpeed - currentTopSpeedParallel;
                var dragForceMag = .5f * Settings.DragCoefficient.Value * (1 - .75f * dotProduct) * speedDif * speedDif;

                var dragForceVec = - dragForceMag * flowDirectionVector;

                _currentVelocity += dragForceVec;
            }
            
            // Apply accumulated forces
            _currentVelocity += forceVec * Time.DeltaTime;
            
            if (_currentState != previousState)
                _stateChangedCallback(_currentState);
            
            // Update position based on current velocity
            var newPosition = Entity.Position + _currentVelocity * Time.DeltaTime;
            Entity.SetPosition(newPosition);
            _subPixelV2.Update(ref _currentVelocity);
        }

        public void OnPlayerInput(CharacterInputController.InputDescription iInput)
        {
            _currentInput = iInput;
        }


        private readonly Action<PrototypeCharacterComponent.State> _stateChangedCallback;
        private CharacterInputController.InputDescription _currentInput;
        private PrototypeCharacterComponent.State _currentState;
        private CollisionResult _currentCollision;
        private Vector2 _currentVelocity;
        //private Mover _mover;
        private SubpixelVector2 _subPixelV2;
        private ProceduralGeneratorComponent _generator;
        private float _lastRowTimeSeconds;

        private Vector2 GetRotationAsDirectionVector()
        {
            var rotation = Entity.Transform.Rotation;

            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }
    }
}