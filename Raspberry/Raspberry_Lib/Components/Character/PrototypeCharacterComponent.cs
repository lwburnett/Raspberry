using System;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class PrototypeCharacterComponent : Component
    {
        public enum State
        {
            Idle,
            RunLeft,
            RunRight
        }

        public override void OnAddedToEntity()
        {
            _animationComponent = Entity.AddComponent<CharacterAnimationComponent>();
            _movementComponent = Entity.AddComponent(new CharacterMovementComponent(OnCharacterStateChanged));
            Entity.AddComponent(new CharacterInputController(OnPlayerInput));
        }

        private CharacterAnimationComponent _animationComponent;
        private CharacterMovementComponent _movementComponent;

        private void OnPlayerInput(CharacterInputController.InputAction iInput)
        {
            _movementComponent.OnPlayerInput(iInput);
        }

        private void OnCharacterStateChanged(State iNewState)
        {
            _animationComponent.SetState(iNewState);
        }
    }

    internal class CharacterMovementComponent : Component, IUpdatable
    {
        public CharacterMovementComponent(Action<PrototypeCharacterComponent.State> iOnStateChangedCallback)
        {
            _stateChangedCallback = iOnStateChangedCallback;
            _currentState = PrototypeCharacterComponent.State.Idle;
        }

        public void Update()
        {
            var previousState = _currentState;

            switch (_currentInput)
            {
                case CharacterInputController.InputAction.Nothing:
                    _currentState = PrototypeCharacterComponent.State.Idle;
                    break;
                case CharacterInputController.InputAction.Left:
                    _currentState = PrototypeCharacterComponent.State.RunLeft;
                    break;
                case CharacterInputController.InputAction.Right:
                    _currentState = PrototypeCharacterComponent.State.RunRight;
                    break;
                default:
                    System.Diagnostics.Debug.Fail($"Unknown value of enum type {typeof(CharacterInputController.InputAction)}: {_currentInput}");
                    break;
            }

            if (_currentState != previousState)
                _stateChangedCallback(_currentState);
        }

        public void OnPlayerInput(CharacterInputController.InputAction iInput)
        {
            _currentInput = iInput;
        }


        private readonly Action<PrototypeCharacterComponent.State> _stateChangedCallback;
        private CharacterInputController.InputAction _currentInput;
        private PrototypeCharacterComponent.State _currentState;
    }
}
