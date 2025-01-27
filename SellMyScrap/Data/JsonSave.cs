using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace com.github.zehsteam.SellMyScrap.Data;

internal class JsonSave
{
    public string DirectoryPath { get; private set; }
    public string FileName { get; private set; }
    public string FilePath => Path.Combine(DirectoryPath, FileName);

    private JObject _data;

    public JsonSave(string directoryPath, string fileName)
    {
        DirectoryPath = directoryPath;
        FileName = fileName;
        _data = ReadFile();
    }

    public bool KeyExists(string key)
    {
        if (_data == null)
        {
            Plugin.Logger.LogError($"KeyExists: Data is null. Ensure the save file is properly loaded.");
            return false;
        }

        return _data.ContainsKey(key);
    }

    public T LoadValue<T>(string key, T defaultValue = default)
    {
        if (TryLoadValue(key, out T value))
        {
            return value;
        }

        return defaultValue;
    }

    public bool TryLoadValue<T>(string key, out T value)
    {
        value = default;

        if (_data == null)
        {
            Plugin.Logger.LogError($"LoadValue: Data is null. Returning default value for key: {key}.");
            return false;
        }

        if (_data.TryGetValue(key, out JToken jToken))
        {
            try
            {
                value = jToken.ToObject<T>();
                return true;
            }
            catch (JsonException ex)
            {
                Plugin.Logger.LogError($"LoadValue: JSON Conversion Error for key: {key}. {ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                Plugin.Logger.LogError($"LoadValue: Argument Null Error for key: {key}. {ex.Message}");
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"LoadValue: Unexpected Error for key: {key}. {ex.Message}");
            }

            return false;
        }

        Plugin.Instance.LogWarningExtended($"LoadValue: Key '{key}' does not exist. Returning default value.");
        return false;
    }

    public bool SaveValue<T>(string key, T value)
    {
        if (_data == null)
        {
            Plugin.Logger.LogError($"SaveValue: Data is null. Cannot save key: {key}.");
            return false;
        }

        try
        {
            JToken jToken = JToken.FromObject(value);

            if (_data.ContainsKey(key))
            {
                _data[key] = jToken;
            }
            else
            {
                _data.Add(key, jToken);
            }

            return WriteFile(_data);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"SaveValue: Error saving key: {key}. {ex.Message}");
            return false;
        }
    }

    private JObject ReadFile()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                Plugin.Logger.LogWarning($"ReadFile: Save file does not exist at \"{FilePath}\". Initializing with an empty file.");
                return new JObject();
            }

            using FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(fs, Encoding.UTF8);

            return JObject.Parse(reader.ReadToEnd());
        }
        catch (JsonException ex)
        {
            Plugin.Logger.LogError($"ReadFile: JSON Parsing Error for file: \"{FilePath}\". {ex.Message}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"ReadFile: Unexpected Error for file: \"{FilePath}\". {ex.Message}");
        }

        return new JObject();
    }

    private bool WriteFile(JObject data)
    {
        try
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            File.WriteAllText(FilePath, data.ToString(), Encoding.UTF8);

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"WriteFile: Unexpected Error for file: \"{FilePath}\". {ex.Message}");
        }

        return false;
    }
}
