using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Classic;
using Newtonsoft.Json;
using System.Security.Cryptography;

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
        
        // 添加版本控制
        private const int MaxBackupVersions = 3;
        private const string TempFileSuffix = ".tmp";
        private const string BackupFileSuffix = ".bak";

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

            string finalPath = Path.Combine(folderPath, fileName + ".json");
            string tempPath = finalPath + TempFileSuffix;
            string backupPath = finalPath + BackupFileSuffix;

            string json = JsonConvert.SerializeObject(progressData, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented // 更易读的格式，便于调试
            });

            if (Debug.isDebugBuild)
            {
                Debug.Log($"Saving to path: {finalPath}");
                Debug.Log($"JSON content: {json}");
            }

            try
            {
                // 1. 先写入临时文件
                File.WriteAllText(tempPath, json);
                
                // 2. 计算校验和并验证写入是否完整
                string tempChecksum = CalculateFileChecksum(tempPath);
                string writtenChecksum = CalculateChecksum(json);
                
                if (tempChecksum != writtenChecksum)
                {
                    throw new IOException("Checksum verification failed after writing to temp file");
                }

                // 3. 原子性替换操作
                if (File.Exists(finalPath))
                {
                    // 创建备份
                    File.Replace(tempPath, finalPath, backupPath, false);
                    
                    // 清理旧备份，保留最多MaxBackupVersions个版本
                    CleanupOldBackups(folderPath, fileName, MaxBackupVersions);
                }
                else
                {
                    // 没有原文件，直接移动临时文件到目标位置
                    File.Move(tempPath, finalPath);
                }
                
                Debug.Log($"Successfully saved data to {finalPath}");
            }
            catch (Exception e)
            {
                // 清理可能残留的临时文件
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { /* 忽略删除错误 */ }
                }
                
                Debug.LogError($"Failed to save data to {finalPath}. Error: {e.Message}");
                throw new IOException($"Failed to save data to {finalPath}", e);
            }
        }

        public T LoadDataJson<T>(string fileName) where T: ProgressDataBase<T>
        {
            string localPath = Path.Combine(DataFolderName, fileName + ".json");
            string path = Path.Combine(Application.persistentDataPath, localPath);

            if (!File.Exists(path))
            {
                // 尝试从备份恢复
                string backupPath = path + BackupFileSuffix;
                if (File.Exists(backupPath))
                {
                    Debug.LogWarning($"Main file {fileName} not found, attempting to restore from backup");
                    try
                    {
                        File.Copy(backupPath, path, true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to restore {fileName} from backup: {e.Message}");
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            try
            {
                JsonData = File.ReadAllText(path);
                
                // 验证文件完整性
                string fileChecksum = CalculateChecksum(JsonData);
                string storedChecksum = CalculateFileChecksum(path);
                
                if (fileChecksum != storedChecksum)
                {
                    Debug.LogWarning($"Checksum mismatch for {fileName}, attempting to use backup");
                    return TryLoadFromBackup<T>(fileName, path);
                }

                T result = JsonConvert.DeserializeObject<T>(JsonData, new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.All
                });
                
                HasRestoreException = false;
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"{fileName} JSON parsing error: {e.Message}");
                return TryLoadFromBackup<T>(fileName, path);
            }
        }

        private T TryLoadFromBackup<T>(string fileName, string originalPath) where T : ProgressDataBase<T>
        {
            string backupPath = originalPath + BackupFileSuffix;
            if (!File.Exists(backupPath))
            {
                Debug.LogError($"No backup available for {fileName}");
                return null;
            }

            try
            {
                Debug.LogWarning($"Attempting to load {fileName} from backup");
                JsonData = File.ReadAllText(backupPath);
                
                T result = JsonConvert.DeserializeObject<T>(JsonData, new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.All
                });
                
                // 尝试自动修复主文件
                try
                {
                    File.Copy(backupPath, originalPath, true);
                    Debug.Log($"Successfully restored {fileName} from backup");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to restore main file from backup: {e.Message}");
                }
                
                HasRestoreException = true;
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load {fileName} from backup: {e.Message}");
                return null;
            }
        }

        private void CleanupOldBackups(string folderPath, string baseFileName, int keepCount)
        {
            try
            {
                string pattern = baseFileName + ".json.bak*";
                var backupFiles = Directory.GetFiles(folderPath, pattern);
                
                if (backupFiles.Length > keepCount)
                {
                    // 按创建时间排序，保留最新的
                    var sortedFiles = backupFiles
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.CreationTime)
                        .Skip(keepCount);
                    
                    foreach (var file in sortedFiles)
                    {
                        try { file.Delete(); }
                        catch { /* 忽略删除错误 */ }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to cleanup old backups: {e.Message}");
            }
        }

        private string CalculateChecksum(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

        private string CalculateFileChecksum(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        public void DeleteProgress(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            string localPath = Path.Combine(DataFolderName, fileName + ".json");
            string path = Path.Combine(Application.persistentDataPath, localPath);
            if (File.Exists(path))
            {
                try
                {
                    FileUtils.DeleteFile(path);
                }
                catch (Exception e)
                {
                    Debug.LogError(fileName + "删除json数据报错" + e.Message);
                }
            }
        }
    }
}