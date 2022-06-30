using System;
using Microsoft.Xna.Framework;
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
                Rotation = 0.0f;
                Row = false;
            }

            public InputDescription(float iRotationInputAction, bool iJumpInput)
            {
                Rotation = iRotationInputAction;
                Row = iJumpInput;
            }

            // Left thumb with domain [-1, 1] for how high or low their thumb is on the Y axis of the touch screen
            public float Rotation { get; }
            public bool Row { get; }
        }

        public CharacterInputController(Action<InputDescription> iOnStateChangedCallback)
        {
            _onStateChangedCallback = iOnStateChangedCallback;
        }

        public override void OnAddedToEntity()
        {
            if (!Input.Touch.IsConnected)
            {
                _rotationInput = new VirtualAxis(new VirtualAxis.GamePadLeftStickY());
                _rowInput = new VirtualButton(new VirtualButton.GamePadButton(0, Buttons.A));
            }
        }

        public void Update()
        {
            if (Input.Touch.IsConnected)
            {
                var rotation = 0f;
                var row = false;

                var touchCollection = Input.Touch.CurrentTouches;
                var screenSize = Screen.Size;

                foreach (var touch in touchCollection)
                {
                    var touchPosRatioX = touch.Position.X / screenSize.X;

                    if (touchPosRatioX <= .33f)
                    {
                        var touchPosRatioY = touch.Position.Y / screenSize.Y;
                        
                        rotation = MathHelper.Clamp((touchPosRatioY - .5f) * 4, -1f, 1f);
                    }
                    else if (touchPosRatioX >= .66f)
                    {
                        row = true;
                    }
                }

                _onStateChangedCallback(new InputDescription(rotation, row));
            }
            else
            {
                _onStateChangedCallback(new InputDescription(_rotationInput.Value, _rowInput.IsPressed));
            }
        }

        private VirtualAxis _rotationInput;
        private VirtualButton _rowInput;
        private readonly Action<InputDescription> _onStateChangedCallback;
    }
}