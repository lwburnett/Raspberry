using System.Threading.Tasks;

namespace Raspberry_Lib
{
    public static class SettingsManager
    {
        static SettingsManager()
        {
            sHasBeenInitialized = false;
        }

        public static void Initialize()
        {
            System.Diagnostics.Debug.Assert(!sHasBeenInitialized, "We've already initialized.");

            sCurrentReadTask = StorageUtils.ReadSettingsFromDiskAsync().ContinueWith(t => SetGameSettingsQuiet(t.Result));
            sHasBeenInitialized = true;
        }

        public static GameSettings GetGameSettings()
        {
            sCurrentReadTask?.Wait();

            return sGameSettings;
        }

        public static void SetGameSettings(GameSettings iGameSettings)
        {
            sCurrentReadTask?.Wait();

            SetGameSettingsQuiet(iGameSettings);
        }

        public static void SaveSettings()
        {
            sCurrentReadTask?.Wait();
            sCurrentWriteTask?.Wait();

            sCurrentWriteTask = StorageUtils.WriteSettingsToDiskAsync(sGameSettings);
        }

        private static bool sHasBeenInitialized;
        private static Task sCurrentReadTask;
        private static Task sCurrentWriteTask;

        private static GameSettings sGameSettings;

        private static void SetGameSettingsQuiet(GameSettings iGameSettings)
        {
            sGameSettings = iGameSettings;
        }
    }
}
