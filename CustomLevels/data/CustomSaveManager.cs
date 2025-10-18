using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

[Serializable]
public class LevelData
{
    public string entityData;
    public int moveCount;
    public bool reversed;
    public string uuid;
    public string code;
    public string description;
    public string title;

    public LevelData()
    {
        uuid = null;
        code = null;
        description = null;
        title = null;
    }
}

[Serializable]
public class StringBoolPair
{
    public string Key;
    public bool Value;
}

[Serializable]
public class StringLevelDataPair
{
    public string Key;
    public LevelData Value;
}

[Serializable]
public class PlayerData
{
    public string playerName;
    public string privateKey;
    public string publicKey;
    public List<StringBoolPair> levelCompletionStatus = new List<StringBoolPair>();
    public List<StringLevelDataPair> levelsData = new List<StringLevelDataPair>();

    public Dictionary<string, bool> GetLevelCompletionDictionary()
    {
        var dict = new Dictionary<string, bool>();
        foreach (var pair in levelCompletionStatus)
            dict[pair.Key] = pair.Value;
        return dict;
    }

    public Dictionary<string, LevelData> GetLevelsDataDictionary()
    {
        var dict = new Dictionary<string, LevelData>();
        foreach (var pair in levelsData)
            dict[pair.Key] = pair.Value;
        return dict;
    }

    public void SetLevelCompletionDictionary(Dictionary<string, bool> dict)
    {
        levelCompletionStatus.Clear();
        foreach (var kv in dict)
            levelCompletionStatus.Add(new StringBoolPair { Key = kv.Key, Value = kv.Value });
    }

    public void SetLevelsDataDictionary(Dictionary<string, LevelData> dict)
    {
        levelsData.Clear();
        foreach (var kv in dict)
            levelsData.Add(new StringLevelDataPair { Key = kv.Key, Value = kv.Value });
    }
}

public static class SaveManagerCustom
{
    private static readonly string savePath = Path.Combine(Application.persistentDataPath, "playerSave.json");
    private static PlayerData _currentData;
    private static Dictionary<string, LevelData> _levelsDict = new Dictionary<string, LevelData>();
    private static Dictionary<string, bool> _levelCompletionDict = new Dictionary<string, bool>();

    public static void InitializeSave(string playerName = "")
    {
        _currentData = LoadFromFile();
        if (_currentData == null)
        {
            _currentData = new PlayerData { playerName = playerName };
            GenerateKeys(out string privateKey, out string publicKey);
            _currentData.privateKey = privateKey;
            _currentData.publicKey = publicKey;

            _levelsDict = new Dictionary<string, LevelData>();
            LevelData defaultLevel = new LevelData
            {
                title = "Default Level",
                description = "A basic starting level.",
                entityData = "{}",
                moveCount = 0,
                reversed = false
            };
            _levelsDict["level_1"] = defaultLevel;

            _levelCompletionDict = new Dictionary<string, bool> { { "level_1", false } };

            SaveToFile();
        }
        else
        {
            _levelsDict = _currentData.GetLevelsDataDictionary();
            _levelCompletionDict = _currentData.GetLevelCompletionDictionary();
            if (string.IsNullOrEmpty(_currentData.privateKey) || string.IsNullOrEmpty(_currentData.publicKey))
            {
                GenerateKeys(out string privateKey, out string publicKey);
                _currentData.privateKey = privateKey;
                _currentData.publicKey = publicKey;
                SaveToFile();
            }
        }
    }

    private static void GenerateKeys(out string privateKey, out string publicKey)
    {
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.PersistKeyInCsp = false;
            privateKey = rsa.ToXmlString(true);
            publicKey = rsa.ToXmlString(false);
        }
    }

    public static void SaveToFile()
    {
        if (_currentData == null) return;
        _currentData.SetLevelCompletionDictionary(_levelCompletionDict);
        _currentData.SetLevelsDataDictionary(_levelsDict);
        string json = JsonUtility.ToJson(_currentData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Save file written.");
    }

    private static PlayerData LoadFromFile()
    {
        if (!File.Exists(savePath)) return null;
        string json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<PlayerData>(json);
    }

    public static PlayerData CurrentData => _currentData;

    public static void UpdateLevelData(string levelId, LevelData newData)
    {
        if (_currentData == null) return;
        _levelsDict[levelId] = newData;
        SaveToFile();
    }

    public static void UpdateLevelCompletion(string levelId, bool completed)
    {
        if (_currentData == null) return;
        _levelCompletionDict[levelId] = completed;
        SaveToFile();
    }

    public static void UpdatePlayerName(string newName)
    {
        if (_currentData == null)
        {
            Debug.LogWarning("No save file found to update player name.");
            return;
        }
        _currentData.playerName = newName;
        SaveToFile();
        Debug.Log("Player name updated to: " + newName);
    }

    public static void DeleteSave()
    {
        _currentData = null;
        _levelsDict.Clear();
        _levelCompletionDict.Clear();
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save deleted.");
        }
        else
        {
            Debug.LogWarning("No save to delete.");
        }
    }

    public static bool SaveExists()
    {
        return File.Exists(savePath);
    }

    public static string SignData(string data, string privateKeyXml)
    {
        using (var rsa = new RSACryptoServiceProvider())
        {
            rsa.FromXmlString(privateKeyXml);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signedBytes = rsa.SignData(dataBytes, CryptoConfig.MapNameToOID("SHA256"));
            return Convert.ToBase64String(signedBytes);
        }
    }
}
