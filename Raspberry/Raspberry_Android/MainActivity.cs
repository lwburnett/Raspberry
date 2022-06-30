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
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        Theme="@android:style/Theme.NoTitleBar.Fullscreen",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class MainActivity : AndroidGameActivity
    {
        private GameMaster _game;
        private View _view;

        protected override void OnCreate(Bundle iBundle)
        {
            base.OnCreate(iBundle);

            Platform.Init(this, iBundle);

            Window?.AddFlags(WindowManagerFlags.Fullscreen);

            _game = new GameMaster(true);
            _view = _game.Service.GetService(typeof(View)) as View;

            SetContentView(_view);
            _game.Run();
        }
    }
}
