using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using Raspberry_Lib;
using Xamarin.Essentials;

namespace Raspberry_Android
{
    [Activity(
        MainLauncher = true,
        Icon = "@mipmap/ic_launcher",
        Theme="@android:style/Theme.NoTitleBar.Fullscreen",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    // ReSharper disable once UnusedMember.Global
    public class MainActivity : AndroidGameActivity
    {
        private GameMaster _game;
        private View _view;
        private Vibrator _vibrator;

        protected override void OnCreate(Bundle iBundle)
        {
            base.OnCreate(iBundle);

            Platform.Init(this, iBundle);

            Window?.AddFlags(WindowManagerFlags.Fullscreen);

            _game = new GameMaster(true, true);
            _view = _game.Service.GetService(typeof(View)) as View;

            _vibrator = (Vibrator)GetSystemService(VibratorService);
            PlatformUtils.SetVibrateCallback(Vibrate);

            SetContentView(_view);
            _game.Run();
        }

        private void Vibrate(long iMilliseconds, byte? iAmplitude = null)
        {
            if (!SettingsManager.GetGameSettings().Vibrate)
                return;

            var amplitude = iAmplitude ?? VibrationEffect.DefaultAmplitude;
            _vibrator.Vibrate(VibrationEffect.CreateOneShot(iMilliseconds, amplitude));
        }
    }
}
