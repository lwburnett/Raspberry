using System;
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

        public static void OnNewDistanceChallengeData(float iDistance, float iTime)
        {
            var gameData = GetGameData();

            if (gameData.DistChallengeRecordDateTime.HasValue &&
                gameData.DistChallengeRecord.HasValue &&
                DateTime.Today.Date == gameData.DistChallengeRecordDateTime.Value.Date)
            {
                if (iDistance > gameData.DistChallengeRecord.Value)
                {
                    var newGameData = new GameData(
                        DateTime.Now,
                        iDistance,
                        gameData.TimeChallengeRecordDateTime,
                        gameData.TimeChallengeRecord,
                        gameData.EndlessChallengeRecord);

                    SetGameData(newGameData);
                    SaveData();
                }
            }
            else
            {
                var newGameData = new GameData(
                    DateTime.Now,
                    iDistance,
                    gameData.TimeChallengeRecordDateTime,
                    gameData.TimeChallengeRecord,
                    gameData.EndlessChallengeRecord);

                SetGameData(newGameData);
                SaveData();
            }
        }

        public static void OnNewTimeChallengeData(float iDistance, float iTime)
        {
            var gameData = GetGameData();

            var timeSpan = TimeSpan.FromSeconds(iTime);

            if (gameData.TimeChallengeRecordDateTime.HasValue &&
                gameData.TimeChallengeRecord.HasValue &&
                DateTime.Today.Date == gameData.TimeChallengeRecordDateTime.Value.Date)
            {
                if (timeSpan < gameData.TimeChallengeRecord.Value)
                {
                    var newGameData = new GameData(
                        gameData.DistChallengeRecordDateTime,
                        gameData.DistChallengeRecord,
                        DateTime.Now,
                        timeSpan,
                        gameData.EndlessChallengeRecord);

                    SetGameData(newGameData);
                    SaveData();
                }
            }
            else
            {
                var newGameData = new GameData(
                    gameData.DistChallengeRecordDateTime,
                    gameData.DistChallengeRecord,
                    DateTime.Now,
                    timeSpan,
                    gameData.EndlessChallengeRecord);

                SetGameData(newGameData);
                SaveData();
            }

        }

        public static void OnNewEndlessData(float iDistance, float iTime)
        {
            sCurrentReadTask?.Wait();

            var gameData = GetGameData();

            if (!gameData.EndlessChallengeRecord.HasValue ||
                iDistance > gameData.EndlessChallengeRecord.Value)
            {
                var newGameData = new GameData(
                    gameData.DistChallengeRecordDateTime,
                    gameData.DistChallengeRecord,
                    gameData.TimeChallengeRecordDateTime,
                    gameData.TimeChallengeRecord,
                    iDistance);

                SetGameData(newGameData);
                SaveData();
            }
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
