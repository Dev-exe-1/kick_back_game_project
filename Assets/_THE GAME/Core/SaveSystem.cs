using UnityEngine;
using System.IO;
using System;

namespace Core
{
    public static class SaveSystem
    {
        private const string ENCRYPTION_KEY = "k1cK_b4cK_s3cR3t";

        public static void Save<T>(T data, string fileName = "savedata.dat")
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);
            try
            {
                string json = JsonUtility.ToJson(data);
                string encryptedJson = EncryptDecrypt(json);
                File.WriteAllText(path, encryptedJson);
                Debug.Log($"[SaveSystem] Successfully saved to {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Error saving data: {e.Message}");
            }
        }

        public static T Load<T>(string fileName = "savedata.dat") where T : new()
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(path))
            {
                try
                {
                    string encryptedJson = File.ReadAllText(path);
                    string decryptedJson = EncryptDecrypt(encryptedJson);
                    T data = JsonUtility.FromJson<T>(decryptedJson);
                    
                    if (data == null)
                    {
                        return new T();
                    }
                    
                    return data;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Error loading data: {e.Message}");
                }
            }
            return new T();
        }

        private static string EncryptDecrypt(string data)
        {
            char[] result = new char[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (char)(data[i] ^ ENCRYPTION_KEY[i % ENCRYPTION_KEY.Length]);
            }
            return new string(result);
        }
    }
}
