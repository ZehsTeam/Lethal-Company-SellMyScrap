using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace com.github.zehsteam.SellMyScrap;

internal class ModpackSaveSystem
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
    
    public static void Initialize()
    {
        if (!FileExists)
        {
            WriteFile(JObject.Parse("{}"));
        }

        _saveFileObject = ReadFile();
    }

    public static bool ContainsKey(string key)
    {
        return _saveFileObject.ContainsKey(key);
    }

    public static JToken ReadValue(string key, JToken defaultValue)
    {
        if (TryReadValue(key, out JToken value))
        {
            return value;
        }

        return defaultValue;
    }

    public static bool TryReadValue(string key, out JToken value)
    {
        value = string.Empty;

        if (_saveFileObject.TryGetValue(key, out JToken jToken))
        {
            value = jToken;
            return true;
        }

        return false;
    }

    public static bool WriteValue(string key, JToken value)
    {
        if (_saveFileObject.ContainsKey(key)) return false;

        _saveFileObject.Add(key, value);
        WriteFile(_saveFileObject);

        return true;
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
            Plugin.logger.LogError($"Error: Failed to read save file.\n\n{e}");
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
            Plugin.logger.LogError($"Error: Failed to write save file.\n\n{e}");
        }

        return false;
    }
    #endregion
}
