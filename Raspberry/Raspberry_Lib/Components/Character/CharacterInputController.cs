﻿using System;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class CharacterInputController: PausableComponent
    {
        public class InputDescription
        {
            public InputDescription()
            {
                Rotation = 0.0f;
                Row = false;
            }

            public InputDescription(float iRotationInputAction, bool iRowInput)
            {
                Rotation = iRotationInputAction;
                Row = iRowInput;
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
                _rotationInput = new VirtualAxis(
                    new VirtualAxis.GamePadLeftStickY(),
                    new VirtualAxis.GamePadLeftStickX(),
                    new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.W, Keys.S),
                    new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D));
                _rowInput = new VirtualButton(
                    new VirtualButton.GamePadButton(0, Buttons.A),
                    new VirtualButton.MouseLeftButton(),
                    new VirtualButton.KeyboardKey(Keys.Space));
            }
        }

        protected override void OnUpdate(float iTotalPlayableTime)
        {
            if (IsPaused)
                return;

            if (Input.Touch.IsConnected)
            {
                var rotation = 0f;
                var row = false;

                var touchCollection = Input.Touch.CurrentTouches;
                var screenSize = Screen.Size;

                foreach (var touch in touchCollection)
                {
                    var touchPosRatioX = touch.Position.X / screenSize.X;
                    var touchPosRatioY = touch.Position.Y / screenSize.Y;

                    if (touchPosRatioX <= .33f)
                    {
                        if (touchPosRatioY >= .55f)
                            rotation = 1f;
                        else if (touchPosRatioY <= .45f)
                            rotation = -1f;
                    }
                    else
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