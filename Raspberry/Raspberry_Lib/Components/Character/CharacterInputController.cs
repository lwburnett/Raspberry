using System;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class CharacterInputController: Component, IUpdatable
    {
        public class InputDescription
        {
            public InputDescription()
            {
                MovementInput = MovementInputAction.Nothing;
                JumpInput = false;
            }

            public InputDescription(MovementInputAction iMovementInput, bool iJumpInput)
            {
                MovementInput = iMovementInput;
                JumpInput = iJumpInput;
            }

            public MovementInputAction MovementInput { get; }
            public bool JumpInput { get; }
        }

        public enum MovementInputAction
        {
            Nothing,
            Left,
            Right
        }

        public CharacterInputController(Action<InputDescription> iOnStateChangedCallback)
        {
            _onStateChangedCallback = iOnStateChangedCallback;
        }

        public override void OnAddedToEntity()
        {
            _xAxisInput = new VirtualIntegerAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));
            _jumpInput = new VirtualButton(new VirtualButton.KeyboardKey(Keys.Up));
        }

        public void Update()
        {
            MovementInputAction movementInput;

            var inputX = _xAxisInput.Value;

            if (inputX == 0)
                movementInput = MovementInputAction.Nothing;
            else if (inputX > 0)
                movementInput = MovementInputAction.Right;
            else
                movementInput = MovementInputAction.Left;

            var jumpPressed = _jumpInput.IsDown;

            _onStateChangedCallback(new InputDescription(movementInput, jumpPressed));
        }

        private VirtualIntegerAxis _xAxisInput;
        private VirtualButton _jumpInput;
        private readonly Action<InputDescription> _onStateChangedCallback;
    }
}