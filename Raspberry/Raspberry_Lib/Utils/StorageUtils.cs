using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable HeuristicUnreachableCode

namespace Raspberry_Lib
{
    internal static class StorageUtils
    {
        private static class Settings
        {
            public const string SettingsFileName = "Settings.txt";
            public const string DataFileName = "Data.txt";
        }

        static StorageUtils()
        {
            FileManager.RegisterBaseDirectory(Nez.Storage.GetStorageRoot());
            sDataFileReadWriteLock = new object();
            sDataPropertyReadWriteLock = new object();
            sSettingsFileReadWriteLock = new object();
            sSettingsPropertyReadWriteLock = new object();

            sGameSettings = new GameSettings();
            sSettingsSet = false;
            sGameData = new GameData();
            sDataSet = false;
        }

        private static readonly object sDataFileReadWriteLock;
        private static readonly object sDataPropertyReadWriteLock;
        private static readonly object sSettingsFileReadWriteLock;
        private static readonly object sSettingsPropertyReadWriteLock;

        private static GameSettings sGameSettings;
        private static bool sSettingsSet;
        private static GameData sGameData;
        private static bool sDataSet;

        public static void ReadSettingsFromDiskAsync()
        {
            if (!sSettingsSet)
                System.Diagnostics.Debug.Fail("Already read settings from disk.");

            Task.Run(DoReadSettings);
        }

        public static void WriteSettingsToDiskAsync()
        {
            if (!sSettingsSet)
                System.Diagnostics.Debug.Fail("Writing default settings to disk.");

            Task.Run(DoWriteSettings);
        }

        public static void ReadDataFromDiskAsync()
        {
            if (!sDataSet)
                System.Diagnostics.Debug.Fail("Already read data from disk.");

            Task.Run(DoReadData);
        }

        public static void WriteDataToDiskAsync()
        {
            if (!sDataSet)
                System.Diagnostics.Debug.Fail("Writing default data to disk.");

            Task.Run(DoWriteData);
        }

        public static GameSettings GameSettings
        {
            get
            {
                lock (sSettingsPropertyReadWriteLock)
                {
                    return sGameSettings;
                }
            }
            private set
            {
                lock (sSettingsPropertyReadWriteLock)
                {
                    sGameSettings = value;
                    sSettingsSet = true;
                }
            }
        }

        public static GameData GameData
        {
            get
            {
                lock (sDataPropertyReadWriteLock)
                {
                    return sGameData;
                }
            }
            private set
            {
                lock (sDataPropertyReadWriteLock)
                {
                    sGameData = value;
                    sDataSet = true;
                }
            }
        }

        private static void DoReadSettings()
        {
            HandleRead(
                Settings.SettingsFileName, 
                sSettingsFileReadWriteLock,
                gs => GameSettings = gs,
                () => new GameSettings(),
                "Game Setting",
                "Game Settings");
        }

        private static void DoReadData()
        {
            HandleRead(
                Settings.DataFileName,
                sDataFileReadWriteLock,
                gs => GameData = gs,
                () => new GameData(),
                "Game Data",
                "Game Data");
        }

        private static void HandleRead<T>(
            string iFileName, 
            object iLock,
            Action<T> iPropertySetter, 
            Func<T> iDefaultConstruction, 
            string iSingularDescriptor,
            string iPluralDescriptor)
        {
            if (TryReadFile(iFileName, iLock, out var lines))
            {
                var parseIsSuccessful = true;

                var realLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim()).ToList();

                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var readableProperties = properties.Where(property => property.CanRead).ToList();

                if (realLines.Count == readableProperties.Count)
                {
                    var typeList = new List<Type>();
                    var valueList = new List<object>();

                    for (var ii = 0; ii < realLines.Count; ii++)
                    {
                        var thisLine = realLines[ii];
                        var thisProperty = readableProperties[ii];

                        if (thisProperty.PropertyType == typeof(bool))
                        {
                            typeList.Add(typeof(bool));
                            if (bool.TryParse(thisLine, out var val))
                            {
                                valueList.Add(val);
                            }
                            else
                            {
                                parseIsSuccessful = false;
                                System.Diagnostics.Debug.Fail($"Failed to parse boolean {iSingularDescriptor}.");
                                break;
                            }
                        }
                        else
                        {
                            parseIsSuccessful = false;
                            System.Diagnostics.Debug.Fail($"Unsupported {iSingularDescriptor} type.");
                            break;
                        }
                    }

                    if (parseIsSuccessful)
                    {
                        var constructor = typeof(T).GetConstructor(typeList.ToArray());
                        if (constructor != null)
                        {
                            var obj = (T)constructor.Invoke(valueList.ToArray());
                            iPropertySetter(obj);
                        }
                        else
                        {
                            parseIsSuccessful = false;
                            System.Diagnostics.Debug.Fail($"No matching constructor for type {typeof(T)}.");
                        }
                    }
                }
                else
                {
                    parseIsSuccessful = false;
                    System.Diagnostics.Debug.Fail($"Unexpected number of {iPluralDescriptor}.");
                }

                if (!parseIsSuccessful)
                {
                    var obj = iDefaultConstruction();
                    iPropertySetter(obj);
                }
            }
            else
            {
                // Deciding not to debug.fail here because the file might not exist the first time
                var obj = iDefaultConstruction();
                iPropertySetter(obj);
            }
        }

        private static bool TryReadFile(string iName, object iLock,  out List<string> oLines)
        {
            try
            {
                var lines = new List<string>();
                lock (iLock)
                {
                    if (FileManager.TryOpenStreamReadSafe(iName, out var stream))
                    {
                        using (stream)
                        using (var reader = new StreamReader(stream, Encoding.ASCII))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                                lines.Add(line);
                        }
                    }
                }

                oLines = lines;
                return true;
            }
            catch (Exception ex)
            {
                oLines = new List<string>();
                System.Diagnostics.Debug.Fail(ex.Message);
                return false;
            }
        }

        private static void DoWriteSettings()
        {

        }

        private static void DoWriteData()
        {

        }

        private static void HandleWrite<T>(
            string iFileName, 
            object iLock, 
            T iObject,
            string iSingularDescriptor,
            string iPluralDescriptor)
        {
            lock (iLock)
            {
                var successfullyOpenedStream = FileManager.TryOpenStreamWriteSafe(iFileName, out var stream);

                if (successfullyOpenedStream)
                {
                    using (stream)
                    {
                    }
                }
                else
                {
                }
            }
        }

        // Class taken from  https://github.com/lwburnett/WordGame/blob/main/WordGame/WordGame_Lib/FileManager.cs
        private static class FileManager
        {
            static FileManager()
            {
                sBaseDirectory = string.Empty;
            }

            public static void RegisterBaseDirectory(string iBaseDirectoryPath)
            {
                System.Diagnostics.Debug.Assert(string.IsNullOrWhiteSpace(sBaseDirectory), "Already registered a base directory?");

                sBaseDirectory = iBaseDirectoryPath;
            }
            
            public static bool TryOpenStreamReadSafe(string iFileName, out Stream oStream)
            {
                var stream = OpenStreamRead(iFileName);

                oStream = stream;

                return stream != null;
            }
            
            public static bool TryOpenStreamWriteSafe(string iFileName, out Stream oStream)
            {
                var stream = OpenStreamWrite(iFileName);

                oStream = stream;

                return stream != null;
            }

            private static string sBaseDirectory;

            private static Stream OpenStreamRead(string iFileName)
            {
                if (!string.IsNullOrWhiteSpace(sBaseDirectory))
                {
                    var path = Path.Combine(sBaseDirectory, iFileName);

                    if (File.Exists(path))
                        return File.OpenRead(path);
                    else
                        return null;
                }
                else
                {
                    System.Diagnostics.Debug.Fail($"Failed to open read stream at {iFileName}");
                    return null;
                }
            }

            private static Stream OpenStreamWrite(string iFileName)
            {
                if (!string.IsNullOrWhiteSpace(sBaseDirectory))
                {
                    var path = Path.Combine(sBaseDirectory, iFileName);

                    if (File.Exists(path))
                        return File.OpenWrite(path);
                    else
                    {
                        var parentDirectory = Path.GetDirectoryName(path);
                        if (!string.IsNullOrWhiteSpace(parentDirectory))
                        {
                            Directory.CreateDirectory(parentDirectory);
                            return File.Create(path);
                        }
                        else
                        {
                            System.Diagnostics.Debug.Fail($"Failed to open read stream at {iFileName}");
                            return null;
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Fail($"Failed to open read stream at {iFileName}");
                    return null;
                }
            }
        }
    }
}
