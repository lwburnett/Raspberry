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
            sSettingsFileReadWriteLock = new object();
        }

        private static readonly object sDataFileReadWriteLock;
        private static readonly object sSettingsFileReadWriteLock;

        public static Task<GameSettings> ReadSettingsFromDiskAsync()
        {
            return Task.Run(DoReadSettings);
        }

        public static Task WriteSettingsToDiskAsync(GameSettings iGameSettings)
        {
            return Task.Run(() => DoWriteSettings(iGameSettings));
        }

        public static Task<GameData> ReadDataFromDiskAsync()
        {
            return Task.Run(DoReadData);
        }

        public static Task WriteDataToDiskAsync(GameData iGameData)
        {
            return Task.Run(() => DoWriteData(iGameData));
        }

        private static GameSettings DoReadSettings()
        {
            return HandleRead(
                Settings.SettingsFileName, 
                sSettingsFileReadWriteLock,
                () => new GameSettings(),
                "Game Setting",
                "Game Settings");
        }

        private static GameData DoReadData()
        {
            return HandleRead(
                Settings.DataFileName,
                sDataFileReadWriteLock,
                () => new GameData(),
                "Game Data",
                "Game Data");
        }

        private static T HandleRead<T>(
            string iFileName, 
            object iLock,
            Func<T> iDefaultConstruction, 
            string iSingularDescriptor,
            string iPluralDescriptor)
        {
            var objectToReturn = iDefaultConstruction();

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
                        else if (thisProperty.PropertyType == typeof(DateTime?))
                        {
                            typeList.Add(typeof(DateTime?));
                            if (thisLine == "null")
                            {
                                valueList.Add(null);
                            }
                            else if (DateTime.TryParse(thisLine, out var val))
                            {
                                valueList.Add(val);
                            }
                            else
                            {
                                parseIsSuccessful = false;
                                System.Diagnostics.Debug.Fail($"Failed to parse DateTime {iSingularDescriptor}.");

                            }
                        }
                        else if (thisProperty.PropertyType == typeof(TimeSpan?))
                        {
                            typeList.Add(typeof(TimeSpan?));
                            if (thisLine == "null")
                            {
                                valueList.Add(null);
                            }
                            else if (TimeSpan.TryParse(thisLine, out var val))
                            {
                                valueList.Add(val);
                            }
                            else
                            {
                                parseIsSuccessful = false;
                                System.Diagnostics.Debug.Fail($"Failed to parse TimeSpan {iSingularDescriptor}.");

                            }
                        }
                        else if (thisProperty.PropertyType == typeof(float?))
                        {
                            typeList.Add(typeof(float?));
                            if (thisLine == "null")
                            {
                                valueList.Add(null);
                            }
                            else if (float.TryParse(thisLine, out var val))
                            {
                                valueList.Add(val);
                            }
                            else
                            {
                                parseIsSuccessful = false;
                                System.Diagnostics.Debug.Fail($"Failed to parse float {iSingularDescriptor}.");

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
                            objectToReturn = obj;
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
                    objectToReturn = obj;
                }
            }
            else
            {
                // Deciding not to debug.fail here because the file might not exist the first time
                var obj = iDefaultConstruction();
                objectToReturn = obj;
            }

            return objectToReturn;
        }

        private static bool TryReadFile(string iName, object iLock,  out List<string> oLines)
        {
            try
            {
                bool wasSuccess;
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
                        wasSuccess = true;
                    }
                    else
                    {
                        wasSuccess = false;
                    }
                }

                oLines = lines;
                return wasSuccess;
            }
            catch (Exception ex)
            {
                oLines = new List<string>();
                System.Diagnostics.Debug.Fail(ex.Message);
                return false;
            }
        }

        private static void DoWriteSettings(GameSettings iGameSettings)
        {
            HandleWrite(
                Settings.SettingsFileName,
                sSettingsFileReadWriteLock,
                iGameSettings);
        }

        private static void DoWriteData(GameData iGameData)
        {
            HandleWrite(
                Settings.DataFileName,
                sDataFileReadWriteLock,
                iGameData);
        }

        private static void HandleWrite<T>(
            string iFileName,
            object iLock,
            T iObject)
        {
            var tmpFileName = $"{Guid.NewGuid()}.txt";

            var successfullyOpenedTmpStream = FileManager.TryOpenStreamWriteSafe(tmpFileName, out var stream);

            var successfulTmpFileWrite = successfullyOpenedTmpStream;

            if (successfullyOpenedTmpStream)
            {
                using (stream)
                using (var writer = new StreamWriter(stream, Encoding.ASCII))
                {
                    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    var readableProperties = properties.Where(property => property.CanRead).ToList();
                    
                    var valueList = readableProperties.Select(p => p.GetValue(iObject)).ToList();
                    for (var ii = 0; ii < valueList.Count; ii++)
                    {
                        var thisValue = valueList[ii];

                        var stringToPrint = thisValue?.ToString() ?? "null";
                        writer.WriteLine(stringToPrint);
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.Fail("Failed to open temp file.");
            }

            if (successfulTmpFileWrite)
            {
                bool successfulRename;

                lock (iLock)
                {
                    successfulRename = FileManager.TryRenameFile(tmpFileName, iFileName, true);
                }

                if (!successfulRename)
                {
                    System.Diagnostics.Debug.Fail("Failed to rename temp file.");

                    var successfulTmpDelete = FileManager.TryDeleteFile(tmpFileName);

                    if (!successfulTmpDelete)
                        System.Diagnostics.Debug.Fail($"Failed to delete temp file {tmpFileName}.");
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

            public static bool TryRenameFile(string iSourceFile, string iDestFile, bool iOverwrite)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(sBaseDirectory))
                    {
                        var sourcePath = Path.Combine(sBaseDirectory, iSourceFile);

                        if (File.Exists(sourcePath))
                        {
                            var destinationPath = Path.Combine(sBaseDirectory, iDestFile);

                            if (File.Exists(destinationPath))
                            {
                                if (iOverwrite)
                                {
                                    File.Delete(destinationPath);
                                    File.Move(sourcePath, destinationPath);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.Fail($"Will not delete destination file {iDestFile}.");
                                    return false;
                                }
                            }
                            else
                            {
                                File.Move(sourcePath, destinationPath);
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.Fail($"Source file does not exist {sourcePath}.");
                            return false;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Fail($"Failed to rename file from {iSourceFile} to {iDestFile}.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Fail(ex.Message);
                    return false;
                }

                return true;
            }

            public static bool TryDeleteFile(string iFileName)
            {
                try
                {
                    var path = Path.Combine(sBaseDirectory, iFileName);
                    File.Delete(path);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Fail(ex.Message);
                    return false;
                }
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
