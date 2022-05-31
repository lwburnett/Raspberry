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
            _walkSpeed = 300f;
            _sprintSpeed = 400f;
            _gravityForce = 75f;
            _jumpHeight = 3f;
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
            var moveVec = Vector2.Zero;

            if (_currentInput.MovementInput == CharacterInputController.MovementInputAction.Nothing)
            {
                moveVec = Vector2.Zero;
                _currentState = PrototypeCharacterComponent.State.Idle;
            }
            else if (_currentInput.MovementInput == CharacterInputController.MovementInputAction.Left)
            {
                if (_currentInput.SprollInput == CharacterInputController.SprollInputAction.Nothing)
                {
                    moveVec = new Vector2(-1 * _walkSpeed, 0);
                    _currentState = PrototypeCharacterComponent.State.WalkLeft;
                }
                else if (_currentInput.SprollInput == CharacterInputController.SprollInputAction.Roll)
                {
                    moveVec = new Vector2(-1 * _walkSpeed, 0);
                    _currentState = PrototypeCharacterComponent.State.WalkLeft;
                }
                else if (_currentInput.SprollInput == CharacterInputController.SprollInputAction.Sprint)
                {
                    moveVec = new Vector2(-1 * _sprintSpeed, 0);
                    _currentState = PrototypeCharacterComponent.State.RunLeft;
                }
                else
                {
                    System.Diagnostics.Debug.Fail(
                        $"Unknown value of enum type {typeof(CharacterInputController.SprollInputAction)}: {_currentInput.SprollInput}");
                }
            }
            else if (_currentInput.MovementInput == CharacterInputController.MovementInputAction.Right)
            {
                if (_currentInput.SprollInput == CharacterInputController.SprollInputAction.Nothing)
                {
                    moveVec = new Vector2(_walkSpeed, 0);
                    _currentState = PrototypeCharacterComponent.State.WalkRight;
                }
                else if (_currentInput.SprollInput == CharacterInputController.SprollInputAction.Roll)
                {
                    moveVec = new Vector2(_walkSpeed, 0);
                    _currentState = PrototypeCharacterComponent.State.WalkRight;
                }
                else if (_currentInput.SprollInput == CharacterInputController.SprollInputAction.Sprint)
                {
                    moveVec = new Vector2(_sprintSpeed, 0);
                    _currentState = PrototypeCharacterComponent.State.RunRight;
                }
                else
                {
                    System.Diagnostics.Debug.Fail(
                        $"Unknown value of enum type {typeof(CharacterInputController.SprollInputAction)}: {_currentInput.SprollInput}");
                }
            }
            else
            {
                System.Diagnostics.Debug.Fail(
                    $"Unknown value of enum type {typeof(CharacterInputController.MovementInputAction)}: {_currentInput.MovementInput}");
            }

            _currentVelocity.X = moveVec.X * Time.DeltaTime;

            if (_currentCollision.Collider != null && _currentCollision.Normal.X < .1f && _currentCollision.Normal.Y < 0f && _currentInput.JumpInput)
            {
                _currentVelocity.Y = -Mathf.Sqrt(2f * _jumpHeight * _gravityForce);
            }

            //_currentVelocity.Y += _gravityForce * Time.DeltaTime;

            if (_currentState != previousState)
                _stateChangedCallback(_currentState);

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
        private readonly float _walkSpeed;
        private readonly float _sprintSpeed;
        private readonly float _gravityForce;
        private readonly float _jumpHeight;
        private Vector2 _currentVelocity;
        private Mover _mover;
        private SubpixelVector2 _subPixelV2;
    }
}