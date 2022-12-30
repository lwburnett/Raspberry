using System.Threading.Tasks;

namespace Raspberry_Lib
{
    internal static class DataManager
    {
        static DataManager()
        {
            sHasBeenInitialized = false;
        }

        public static void Initialize()
        {
            System.Diagnostics.Debug.Assert(!sHasBeenInitialized, "We've already initialized.");

            sCurrentReadTask = StorageUtils.ReadDataFromDiskAsync().ContinueWith(t => SetGameDataQuiet(t.Result));
            sHasBeenInitialized = true;
        }

        public static GameData GetGameData()
        {
            sCurrentReadTask?.Wait();

            return sGameData;
        }

        public static void SetGameData(GameData iGameData)
        {
            sCurrentReadTask?.Wait();

            SetGameDataQuiet(iGameData);
        }

        public static void SaveData()
        {
            sCurrentReadTask?.Wait();
            sCurrentWriteTask?.Wait();

            sCurrentWriteTask = StorageUtils.WriteDataToDiskAsync(sGameData);
        }

        private static bool sHasBeenInitialized;
        private static Task sCurrentReadTask;
        private static Task sCurrentWriteTask;

        private static GameData sGameData;

        private static void SetGameDataQuiet(GameData iGameData)
        {
            sGameData = iGameData;
        }
    }
}
