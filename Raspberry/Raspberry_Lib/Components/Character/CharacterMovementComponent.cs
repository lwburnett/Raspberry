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
            public static readonly RenderSetting FlowSpeed = new(50);
            public const float MinimumSpeedAsPercentOfFlowSpeed = .5f;
            public static readonly RenderSetting Acceleration = new(50);

            public const float RotationRateDegreesPerSecond = 90f;
            public static readonly RenderSetting RowForce = new(100);
        }

        public CharacterMovementComponent(Action<PrototypeCharacterComponent.State> iOnStateChangedCallback)
        {
            _stateChangedCallback = iOnStateChangedCallback;
            _currentInput = new CharacterInputController.InputDescription();
            _currentState = PrototypeCharacterComponent.State.Idle;
            _currentCollision = new CollisionResult();
            _currentVelocity = new Vector2(.01f, 0.0f);
            _subPixelV2 = new SubpixelVector2();
        }

        public int BeginPlayOrder => 98;

        public void OnBeginPlay()
        {
            _generator = Entity.Scene.FindEntity("map")?.GetComponent<ProceduralGeneratorComponent>();

            System.Diagnostics.Debug.Assert(_generator != null);
        }

        public void Update()
        {
            var previousState = _currentState;
            var forceVec = Vector2.Zero;
            
            if (_generator == null)
                return;
            
            var thisFunction = _generator.Functions.
                FirstOrDefault(f => 
                    f.DomainStart < Entity.Position.X &&
                    Entity.Position.X <= f.DomainEnd);
            
            if (thisFunction == null)
                return;
            
            var flowDirectionScalar = thisFunction.GetYPrimeForX(Entity.Position.X);
            
            var flowDirectionVector = new Vector2(1f, flowDirectionScalar);
            flowDirectionVector.Normalize();

            var directionVector = GetRotationAsDirectionVector();
            directionVector.Normalize();

            var dotProduct = Vector2.Dot(directionVector, flowDirectionVector);
            
            var currentTopSpeedParallel = (Settings.FlowSpeed.Value * Settings.MinimumSpeedAsPercentOfFlowSpeed) * (dotProduct + 1f);
            
            var currentParallelSpeed = Vector2.Dot(_currentVelocity, flowDirectionVector);
            if (currentParallelSpeed < currentTopSpeedParallel)
            {
                forceVec += Settings.Acceleration.Value * flowDirectionVector;
            }
            else
            {
                forceVec += -Settings.Acceleration.Value * flowDirectionVector;
            }
            
            _currentVelocity += forceVec * Time.DeltaTime;
            
            if (_currentState != previousState)
                _stateChangedCallback(_currentState);
            
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

        private Vector2 GetRotationAsDirectionVector()
        {
            var rotation = Entity.Transform.Rotation;

            return new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));
        }
    }
}