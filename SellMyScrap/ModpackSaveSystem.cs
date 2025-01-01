using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace com.github.zehsteam.SellMyScrap;

internal static class ModpackSaveSystem
{
    public const string FileName = $"{MyPluginInfo.PLUGIN_NAME}_SaveData.json";

    public static string FilePath
    {
        get
        {
            string folderPath = Path.GetDirectoryName(Plugin.Instance.Config.ConfigFilePath);
            return Path.Combine(folderPath, FileName);
        }
    }

    public static bool FileExists => File.Exists(FilePath);

    private static JObject _saveFileObject;

    static ModpackSaveSystem()
    {
        try
        {
            if (!FileExists)
            {
                WriteFile(JObject.Parse("{}"));
            }

            _saveFileObject = ReadFile() ?? JObject.Parse("{}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Failed to initialize ModpackSaveSystem: {ex}");
            _saveFileObject = JObject.Parse("{}"); // Fallback to empty object
        }
    }

    public static bool ContainsKey(string key)
    {
        return _saveFileObject.ContainsKey(key);
    }

    public static T ReadValue<T>(string key, T defaultValue = default)
    {
        if (_saveFileObject.TryGetValue(key, out JToken jToken))
        {
            try
            {
                // Attempt to convert the JToken to the specified type T
                return jToken.ToObject<T>();
            }
            catch (JsonException ex)
            {
                // Handle JSON conversion errors
                Plugin.Logger.LogError($"JSON Conversion Error: {ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                // Handle cases where the JToken is null
                Plugin.Logger.LogError($"Argument Null Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                Plugin.Logger.LogError($"Unexpected Error: {ex.Message}");
            }

            // Return the default value of T if conversion fails
            return defaultValue;
        }

        return defaultValue;
    }

    public static void WriteValue<T>(string key, T value)
    {
        JToken jToken = JToken.FromObject(value);

        if (_saveFileObject.ContainsKey(key))
        {
            _saveFileObject[key] = jToken;
        }
        else
        {
            _saveFileObject.Add(key, jToken);
        }

        WriteFile(_saveFileObject);
    }

    #region Read/Write File
    private static JObject ReadFile()
    {
        try
        {
            using FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
            using StreamReader reader = new StreamReader(fs, Encoding.UTF8);

            return JObject.Parse(reader.ReadToEnd());
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to read save file.\n\n{e}");
        }

        return null;
    }

    private static bool WriteFile(JObject jObject)
    {
        try
        {
            using FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter writer = new StreamWriter(fs, Encoding.UTF8);

            writer.WriteLine(jObject.ToString());

            return true;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Failed to write save file.\n\n{e}");
        }

        return false;
    }
    #endregion
}
