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

            public InputDescription(MovementInputAction iMovementInput, bool iJumpInput, SprollInputAction iSprollInput)
            {
                MovementInput = iMovementInput;
                JumpInput = iJumpInput;
                SprollInput = iSprollInput;
            }

            public MovementInputAction MovementInput { get; }
            public bool JumpInput { get; }
            public SprollInputAction SprollInput { get; }
        }

        public enum MovementInputAction
        {
            Nothing,
            Left,
            Right
        }

        public enum SprollInputAction
        {
            Nothing,
            Sprint,
            Roll
        }

        public CharacterInputController(Action<InputDescription> iOnStateChangedCallback)
        {
            _onStateChangedCallback = iOnStateChangedCallback;
            _lastSprollPressTime = null;
        }

        public override void OnAddedToEntity()
        {
            _xAxisInput = new VirtualIntegerAxis(
                new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D),
                new VirtualAxis.GamePadLeftStickX());
            _jumpInput = new VirtualButton(
                new VirtualButton.KeyboardKey(Keys.Space),
                new VirtualButton.GamePadButton(0, Buttons.A));

            _sprollInput = new VirtualButton(
                new VirtualButton.KeyboardKey(Keys.LeftShift),
                new VirtualButton.GamePadButton(0, Buttons.B));
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

            var jumpPressed = _jumpInput.IsPressed;

            var sprollInput = SprollInputAction.Nothing;
            if (_sprollInput.IsDown)
            {
                if (_lastSprollPressTime.HasValue)
                {
                    if (Time.TotalTime - _lastSprollPressTime.Value > CSecondsTillSprint)
                    {
                        sprollInput = SprollInputAction.Sprint;
                    }
                }
                else
                {
                    _lastSprollPressTime = Time.TotalTime;
                }
            }
            else
            {
                if (_lastSprollPressTime.HasValue && Time.TotalTime - _lastSprollPressTime.Value < CSecondsTillSprint)
                {
                    sprollInput = SprollInputAction.Roll;
                }
                _lastSprollPressTime = null;
            }

            _onStateChangedCallback(new InputDescription(movementInput, jumpPressed, sprollInput));
        }

        private VirtualIntegerAxis _xAxisInput;
        private VirtualButton _jumpInput;
        private VirtualButton _sprollInput;
        private float? _lastSprollPressTime;
        private readonly Action<InputDescription> _onStateChangedCallback;

        private const float CSecondsTillSprint = .5f;
    }
}