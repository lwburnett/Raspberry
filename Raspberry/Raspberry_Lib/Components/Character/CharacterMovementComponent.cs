using System;
using Microsoft.Xna.Framework;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class CharacterMovementComponent : Component, IUpdatable
    {
        public CharacterMovementComponent(Action<PrototypeCharacterComponent.State> iOnStateChangedCallback)
        {
            _stateChangedCallback = iOnStateChangedCallback;
            _currentState = PrototypeCharacterComponent.State.Idle;
            _moveSpeed = 500f;
            _subPixelV2 = new SubpixelVector2();
        }

        public override void OnAddedToEntity()
        {
            _mover = Entity.AddComponent(new Mover());

            base.OnAddedToEntity();
        }

        public void Update()
        {
            var previousState = _currentState;
            Vector2 moveDir;

            switch (_currentInput)
            {
                case CharacterInputController.InputAction.Nothing:
                    _currentState = PrototypeCharacterComponent.State.Idle;
                    moveDir = Vector2.Zero;
                    break;
                case CharacterInputController.InputAction.Left:
                    _currentState = PrototypeCharacterComponent.State.RunLeft;
                    moveDir = -1.0f * Vector2.UnitX;
                    break;
                case CharacterInputController.InputAction.Right:
                    _currentState = PrototypeCharacterComponent.State.RunRight;
                    moveDir = Vector2.UnitX;
                    break;
                default:
                    moveDir = Vector2.Zero;
                    System.Diagnostics.Debug.Fail($"Unknown value of enum type {typeof(CharacterInputController.InputAction)}: {_currentInput}");
                    break;
            }

            if (_currentState != previousState)
                _stateChangedCallback(_currentState);

            var movement = moveDir * _moveSpeed * Time.DeltaTime;
            _mover.CalculateMovement(ref movement, out _);
            _subPixelV2.Update(ref movement);
            _mover.ApplyMovement(movement);
        }

        public void OnPlayerInput(CharacterInputController.InputAction iInput)
        {
            _currentInput = iInput;
        }


        private readonly Action<PrototypeCharacterComponent.State> _stateChangedCallback;
        private CharacterInputController.InputAction _currentInput;
        private PrototypeCharacterComponent.State _currentState;
        private readonly float _moveSpeed;
        private Mover _mover;
        SubpixelVector2 _subPixelV2;
    }
}