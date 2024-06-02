using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace com.github.zehsteam.SellMyScrap;

internal class SaveSystem
{
    private const string _fileName = "sellmyscrap_savedata.json";

    public static bool SetScrapEaterChance
    {
        get
        {
            SaveData saveData = ReadSaveData();
            return saveData.SetScrapEaterChance;
        }
        set
        {
            SaveData saveData = ReadSaveData();
            saveData.SetScrapEaterChance = value;
            WriteSaveData(saveData);
        }
    }

    private static SaveData ReadSaveData()
    {
        try
        {
            return JsonConvert.DeserializeObject<SaveData>(ReadFile());
        }
        catch (Exception e)
        {
            Plugin.logger.LogError($"[SaveSystem] Error: Failed to read save data.\n\n{e}");
        }

        return null;
    }

    private static bool WriteSaveData(SaveData saveData)
    {
        try
        {
            return WriteFile(JsonConvert.SerializeObject(saveData, Formatting.Indented));
        }
        catch (Exception e)
        {
            Plugin.logger.LogError($"[SaveSystem] Error: Failed to write save data.\n\n{e}");
        }

        return false;
    }

    private static string CreateSaveDataFile()
    {
        try
        {
            if (WriteSaveData(new SaveData()))
            {
                return ReadFile();
            }
        }
        catch (Exception e)
        {
            Plugin.logger.LogError($"[SaveSystem] Error: Failed to create save data file.\n\n{e}");
        }

        return string.Empty;
    }

    private static string ReadFile()
    {
        try
        {
            string filePath = GetFilePath();

            if (!File.Exists(filePath))
            {
                return CreateSaveDataFile();
            }

            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            using StreamReader reader = new StreamReader(fs, Encoding.UTF8);

            return reader.ReadToEnd();
        }
        catch (Exception e)
        {
            Plugin.logger.LogError($"[SaveSystem] Error: Failed to read save file.\n\n{e}");
        }

        return string.Empty;
    }

    private static bool WriteFile(string text)
    {
        try
        {
            using FileStream fs = new FileStream(GetFilePath(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            using StreamWriter writer = new StreamWriter(fs, Encoding.UTF8);

            writer.WriteLine(text);

            return true;
        }
        catch (Exception e)
        {
            Plugin.logger.LogError($"[SaveSystem] Error: Failed to write save file.\n\n{e}");
        }

        return false;
    }

    private static string GetFilePath()
    {
        string folderPath = Path.GetDirectoryName(Plugin.Instance.Info.Location);
        return Path.Combine(folderPath, _fileName);
    }
}

[Serializable]
public class SaveData
{
    public bool SetScrapEaterChance = false;
}
