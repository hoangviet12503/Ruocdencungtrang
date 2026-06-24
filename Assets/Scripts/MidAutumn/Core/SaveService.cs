using System;
using System.IO;
using UnityEngine;

namespace MidAutumn.Gameplay
{
    /// <summary>
    /// Local JSON persistence for SaveData. WebGL falls back to PlayerPrefs
    /// (Application.persistentDataPath isn't reliably writable/persistent
    /// across sessions in browser sandboxes); native mobile uses a file.
    /// </summary>
    public static class SaveService
    {
        private const string FileName = "midautumn_save.json";
        private const string PlayerPrefsKey = "MidAutumn_SaveData";

        public static SaveData Load()
        {
            try
            {
                string json = ReadRaw();
                if (string.IsNullOrEmpty(json)) return SaveData.CreateNew();

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                return data ?? SaveData.CreateNew();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] Load failed, starting fresh: {e.Message}");
                return SaveData.CreateNew();
            }
        }

        public static void Save(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                WriteRaw(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Save failed: {e.Message}");
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private static string ReadRaw() => PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);

        private static void WriteRaw(string json)
        {
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }
#else
        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        private static string ReadRaw() => File.Exists(FilePath) ? File.ReadAllText(FilePath) : string.Empty;

        private static void WriteRaw(string json) => File.WriteAllText(FilePath, json);
#endif
    }
}
