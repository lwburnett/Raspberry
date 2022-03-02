using System;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class CharacterInputController: Component, IUpdatable
    {
        public CharacterInputController(Action<PrototypeCharacterComponent.State> iOnStateChangedCallback)
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
                _onStateChangedCallback(PrototypeCharacterComponent.State.Idle);
            else if (inputX > 0)
                _onStateChangedCallback(PrototypeCharacterComponent.State.RunRight);
            else
                _onStateChangedCallback(PrototypeCharacterComponent.State.RunLeft);
        }

        private VirtualIntegerAxis _xAxisInput;
        private readonly Action<PrototypeCharacterComponent.State> _onStateChangedCallback;
    }
}