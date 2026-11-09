
using System;
using UnityEngine;
using System.IO;

namespace WData
{
    public static  class DataFileHandler
    {
        private const string RootFolder = "WData";

        #region  Private Methods

        private static string GetDevicePath(string filePath)
        {
            return Path.Combine(Application.persistentDataPath, RootFolder, filePath);
        }
        private static void Save<T>(T data, string filePath) where T : class
        {
            try
            {
                byte[] bytes = DataSerializer.Serialize<T>(data);
                {
                    string path = Path.GetDirectoryName(filePath);

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }

                File.WriteAllBytes(filePath, bytes);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Save failed: {e}");
            }
        }
        private static T Load<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning(typeof(DataFileHandler) +
                        $" Can't load, file {filePath} does not exist! creating new file");
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(filePath);

                return DataSerializer.Deserialize<T>(bytes);
            }
            catch (Exception e)
            {
                Debug.Log(typeof(DataFileHandler) + $" Load failed: {e}");

                return null;
            }
        }

        private static void Delete(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.Log(typeof(DataFileHandler) + $" Can't delete, file {filePath} does not exist!");
                    return;
                }

                File.Delete(filePath);
            }
            catch (Exception e)
            {
                Debug.Log(typeof(DataFileHandler) + $" Delete failed: {e}");
            }
        }
        #endregion

        #region Public Methods

        public static void SaveToDevice<T>(T data, string filePath) where T : class
        {
            Save(data, GetDevicePath(filePath));
        }
        

        public static T LoadFromDevice<T>(string filePath) where T : class
        {
            return Load<T>(GetDevicePath(filePath));
        }



        public static void DeleteInDevice(string filePath)
        {
            Delete(GetDevicePath(filePath));
        }

        public static void DeleteAllInDevice()
        {
            string path = Path.Combine(Application.persistentDataPath, RootFolder);

            var info = new DirectoryInfo(path);

            if (!info.Exists)
                return;

            var files = info.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                files[i].Delete();
            }
        }

        #endregion
    }
}