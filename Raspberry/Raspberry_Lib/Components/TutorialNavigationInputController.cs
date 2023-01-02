using System.Linq;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace Raspberry_Lib.Components
{
    internal class TutorialNavigationInputController : PausableComponent, IBeginPlay
    {
        private static class Settings
        {
            public const float TimeBetweenNavigationSeconds = 1f;
        }

        public TutorialNavigationInputController(System.Action iOnNavigationClick)
        {
            IsPaused = false;
            _onNavigation = iOnNavigationClick;
        }

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
            _lastNavigationTime = Time.TotalTime - TimeSpentPaused;
        }

        protected override void OnUpdate(float iTotalPlayableTime)
        {
            if (iTotalPlayableTime - _lastNavigationTime < Settings.TimeBetweenNavigationSeconds)
            {
                return;
            }

            if (Input.Touch.IsConnected)
            {
                if (Input.Touch.CurrentTouches.Any())
                {
                    PlatformUtils.VibrateForUiNavigation();
                    _lastNavigationTime = iTotalPlayableTime;
                    _onNavigation();
                }
            }
            else
            {
                if (_navigationInput.IsPressed)
                {
                    _lastNavigationTime = iTotalPlayableTime;
                    _onNavigation();
                }
            }
        }

        private readonly System.Action _onNavigation;
        private VirtualButton _navigationInput;
        private float _lastNavigationTime;
    }
}
