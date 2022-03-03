using System;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class CharacterInputController: Component, IUpdatable
    {
        public enum InputAction
        {
            Nothing,
            Left,
            Right
        }

        public CharacterInputController(Action<InputAction> iOnStateChangedCallback)
        {
            _onStateChangedCallback = iOnStateChangedCallback;
        }

        public override void OnAddedToEntity()
        {
            _xAxisInput = new VirtualIntegerAxis(new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right));
        }

        public void Update()
        {
            var inputX = _xAxisInput.Value;

            if (inputX == 0)
                _onStateChangedCallback(InputAction.Nothing);
            else if (inputX > 0)
                _onStateChangedCallback(InputAction.Right);
            else
                _onStateChangedCallback(InputAction.Left);
        }

        private VirtualIntegerAxis _xAxisInput;
        private readonly Action<InputAction> _onStateChangedCallback;
    }
}