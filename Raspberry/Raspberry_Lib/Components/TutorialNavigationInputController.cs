using System.Linq;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class TutorialNavigationInputController : Component, IBeginPlay, IUpdatable, IPausable
    {
        private static class Settings
        {
            public const float TimeBetweenNavigationSeconds = .5f;
        }

        public TutorialNavigationInputController(System.Action iOnNavigationClick)
        {
            IsPaused = false;
            _onNavigation = iOnNavigationClick;
        }

        public bool IsPaused { get; set; }

        public override void OnAddedToEntity()
        {
            if (!Input.Touch.IsConnected)
            {
                _navigationInput = new VirtualButton(
                    new VirtualButton.GamePadButton(0, Buttons.A),
                    new VirtualButton.MouseLeftButton(),
                    new VirtualButton.KeyboardKey(Keys.Space));
            }
        }

        public int BeginPlayOrder => 96;
        public void OnBeginPlay()
        {
            _lastNavigationTime = Time.TotalTime;
            _timeSpentPaused = 0;
        }

        public void Update()
        {
            if (IsPaused)
            {
                _timeSpentPaused += Time.DeltaTime;
                return;
            }

            var adjustedTime = Time.TotalTime - _timeSpentPaused;
            if (adjustedTime - _lastNavigationTime < Settings.TimeBetweenNavigationSeconds)
            {
                return;
            }

            if (Input.Touch.IsConnected)
            {
                if (Input.Touch.CurrentTouches.Any())
                {
                    _lastNavigationTime = adjustedTime;
                    _onNavigation();
                }
            }
            else
            {
                if (_navigationInput.IsPressed)
                {
                    _lastNavigationTime = adjustedTime;
                    _onNavigation();
                }
            }
        }

        private readonly System.Action _onNavigation;
        private VirtualButton _navigationInput;
        private float _lastNavigationTime;
        private float _timeSpentPaused;
    }
}
