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
            _currentInput = new CharacterInputController.InputDescription();
            _currentState = PrototypeCharacterComponent.State.Idle;
            _currentCollision = new CollisionResult();
            _moveSpeed = 300f;
            _gravityForce = 75f;
            _jumpHeight = 4f;
            _currentVelocity = Vector2.Zero;
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

            switch (_currentInput.MovementInput)
            {
                case CharacterInputController.MovementInputAction.Nothing:
                    _currentState = PrototypeCharacterComponent.State.Idle;
                    moveDir = Vector2.Zero;
                    break;
                case CharacterInputController.MovementInputAction.Left:
                    _currentState = PrototypeCharacterComponent.State.RunLeft;
                    moveDir = -1.0f * Vector2.UnitX;
                    break;
                case CharacterInputController.MovementInputAction.Right:
                    _currentState = PrototypeCharacterComponent.State.RunRight;
                    moveDir = Vector2.UnitX;
                    break;
                default:
                    moveDir = Vector2.Zero;
                    System.Diagnostics.Debug.Fail(
                        $"Unknown value of enum type {typeof(CharacterInputController.MovementInputAction)}: {_currentInput}");
                    break;
            }

            if (_currentCollision.Collider != null && _currentCollision.Normal.X < .1 && _currentInput.JumpInput)
            {
                _currentVelocity.Y = -Mathf.Sqrt(2f * _jumpHeight * _gravityForce);
            }

            if (_currentState != previousState)
                _stateChangedCallback(_currentState);

            _currentVelocity.X = moveDir.X * _moveSpeed * Time.DeltaTime;
            _currentVelocity.Y += _gravityForce * Time.DeltaTime;

            _mover.CalculateMovement(ref _currentVelocity, out _currentCollision);
            _subPixelV2.Update(ref _currentVelocity);
            _mover.ApplyMovement(_currentVelocity);
        }

        public void OnPlayerInput(CharacterInputController.InputDescription iInput)
        {
            _currentInput = iInput;
        }


        private readonly Action<PrototypeCharacterComponent.State> _stateChangedCallback;
        private CharacterInputController.InputDescription _currentInput;
        private PrototypeCharacterComponent.State _currentState;
        private CollisionResult _currentCollision;
        private readonly float _moveSpeed;
        private readonly float _gravityForce;
        private readonly float _jumpHeight;
        private Vector2 _currentVelocity;
        private Mover _mover;
        private SubpixelVector2 _subPixelV2;
    }
}