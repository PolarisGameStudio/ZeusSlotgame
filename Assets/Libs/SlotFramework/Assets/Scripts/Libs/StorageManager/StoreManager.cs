using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using Classic;
using Newtonsoft.Json;

namespace Libs
{
    public class StoreManager
    {
        public static StoreManager Instance
        {
            get { return Singleton<StoreManager>.Instance; }
        }

        StoreManager()
        {
            
        }
        public static string DataFolderName = "StoreData";
        private static string JsonData = "";
        public static bool HasRestoreException = false;

        public void SaveDataJson<T>(string fileName, T progressData) where T : ProgressDataBase<T>
        {
            if (HasRestoreException)
            {
                HasRestoreException = false;
                return;
            }

            string folderPath = Path.Combine(Application.persistentDataPath, DataFolderName);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string path = Path.Combine(folderPath, fileName + ".json");

            string json = JsonConvert.SerializeObject(progressData,new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All});

            if (Debug.isDebugBuild)
            {
                Debug.Log(path);
            }

            try
            {
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public T LoadDataJson<T>(string fileName) where T:ProgressDataBase<T>
        {
            string localPath = Path.Combine(DataFolderName, fileName + ".json");
            string path = Path.Combine(Application.persistentDataPath, localPath);

            if (File.Exists(path))
            {
                JsonData = File.ReadAllText(path);
                T result = null;
                try
                {
                    result = JsonConvert.DeserializeObject<T>(JsonData,new JsonSerializerSettings {
                        TypeNameHandling = TypeNameHandling.All});
                    HasRestoreException = false;
                }
                catch (Exception e)
                {
                    Debug.LogError(fileName + "解析json数据报错" + e.Message);
                }

                return result;
            }

            return null;
        }

        public string GetSaveData(string fileName)
        {
            string localPath = Path.Combine(DataFolderName, fileName + ".json");
            string path = Path.Combine(Application.persistentDataPath, localPath);
            if (File.Exists(path))
            {
                JsonData = File.ReadAllText(path);
                return JsonData;
            }

            return "";
        }

        public void DeleteProgress(string fileName, ReelManager reelManager)
        {
            if (string.IsNullOrEmpty(fileName) || reelManager == null)
                return;

            string localPath = Path.Combine(DataFolderName, fileName + ".json");
            string path = Path.Combine(Application.persistentDataPath, localPath);
            if (File.Exists(path))
            {
                try
                {
                    FileUtils.DeleteFile(path);
                    // 恢复时，如果有feature，将json内容上报ES, 便于查询卡死
                    if (reelManager.IsInBonusGame || reelManager.isFreespinBonus || reelManager.FreespinCount > 0
                        || BaseSlotMachineController.Instance.onceMore)
                    {
                        Dictionary<string, object> para = new Dictionary<string, object>();
                        para.Add("JsonData", JsonData);
                        para.Add("Exception", HasRestoreException);
                        JsonData = "";
                        BaseGameConsole.ActiveGameConsole().LogBaseEvent(Analytics.RestoreLocalProgress, para);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(fileName + "删除json数据报错" + e.Message);
                }
            }
        }
    }
}