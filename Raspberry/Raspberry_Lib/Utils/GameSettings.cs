using Nez;

namespace Raspberry_Lib
{
    internal struct GameSettings
    {
        public GameSettings()
        {
            Vibrate = true;
            ScreenShake = true;
            Music = true;
            Sfx = true;
        }

        public GameSettings(bool iVibrate, bool iScreenShake, bool iMusic, bool iSfx)
        {
            Vibrate = iVibrate;
            ScreenShake = iScreenShake;
            Music = iMusic;
            Sfx = iSfx;
        }

        public bool Vibrate { get; }
        public bool ScreenShake { get; }
        public bool Music { get; }
        public bool Sfx { get; }
    }
}
