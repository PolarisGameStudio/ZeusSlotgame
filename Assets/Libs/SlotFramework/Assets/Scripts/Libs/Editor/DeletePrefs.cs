using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;
using Libs;
using Classic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class DeletePrefs
{
    
    [MenuItem("Libs/删除/删除所有数据")]
    public static void DeleteAll()
    {
        DeleteAllPrefs();
        DeleteAllDownloadData();
        DeleteAllDownloadData();
        DeleteSceneProgressData();
        DeleteSystemProgressData();
        DeletePackUselessData();
        Debug.Log("delete all success");
    }
    [MenuItem("Libs/删除/删除本地pref数据")]
    public static void DeleteAllPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("delete prefs success");
    }

    [MenuItem("Libs/删除/删除App下载数据")]
    public static void DeleteAllDownloadData()
    {
        string path = Application.persistentDataPath;
//        if (Directory.Exists(path))
//        {
//            DirectoryInfo dir = new DirectoryInfo(path);
//            dir.Delete(true);
//        }
        Debug.Log("delete download data success" + path);
    }
    [MenuItem("Libs/删除/删除关卡保存数据 #&d")]
    public static void DeleteSceneProgressData()
    {
	    string folderPath = Path.Combine (Application.persistentDataPath, SceneProgressManager.DataFolderName);
	    if (Directory.Exists(folderPath))
	    {
		    DirectoryInfo dir = new DirectoryInfo(folderPath);

		    foreach (FileInfo file in dir.GetFiles())
		    {
			    file.Delete();
		    }
	    }
	    Debug.Log("delete scene data success");
    }
    [MenuItem("Libs/删除/删除系统保存数据 #&d")]
    public static void DeleteSystemProgressData()
    {
        string folderPath = Path.Combine (Application.persistentDataPath, StoreManager.DataFolderName);
        if (Directory.Exists(folderPath))
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);

            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }
        }
        Debug.Log("delete system data success");
    }
    
	private const string WILD_DEFAULT_NAME = "GameConfig.plist";
	private const string CLASSIC_DEFAULT_NAME = "Config_classicIOS.plist";

	public static List<string> GetSlotMachinesInPackage(){
		List<string> InPackageSlotMachines = new List<string> ();

		Dictionary<string,object> config = Plugins.Configuration.ReadConfigInUnityEditor (CLASSIC_DEFAULT_NAME);
		if (config==null) config = Plugins.Configuration.ReadConfigInUnityEditor (WILD_DEFAULT_NAME);
		Dictionary<string,object> slotInfoDict = CSharpUtil.GetValueWithPath<Dictionary<string,object>> (config,SlotMachineConfigParse.SlotInfoDict_Key,null);
		List<object> nameArr = CSharpUtil.GetValueWithPath<List<object>> (config,SlotMachineConfigParse.SlotList_Key,null);
		List<string> SlotNameList = new List<string>();

		if (nameArr!=null)
		{
		    List<object> slotList = nameArr[0] as List<object>;
			foreach (object item in slotList) {
				string slot = item as string;
				if (slot!=null) {
					SlotNameList.Add(slot);
				}
			}
		}

		if (slotInfoDict != null)
		{
			foreach (string key in slotInfoDict.Keys)
			{
				if(string.IsNullOrEmpty(key)) continue;

				Dictionary<string,object> slotinfo = slotInfoDict[key] as Dictionary<string,object>;
				if (slotinfo == null) continue;

				bool InPackage = Utils.Utilities.GetValue<bool> (slotinfo,SlotMachineConfigParse.SLOT_MACHINE_IN_PACKAGE_KEY,false);
				if (InPackage && SlotNameList.Contains(key))
				{
                    string localScenePath = Path.Combine(Application.dataPath, ("Scenes/" + key + ".unity"));
                    if (File.Exists(localScenePath)) {
                        Debug.Log("localScenePath:"+ localScenePath);
                        InPackageSlotMachines.Add(key);
                    }
                    else
                    {
                        Debug.Log("NolocalScenePath");
                    }
					
				}		
			}
		}
		return InPackageSlotMachines;
	}
    [MenuItem("Libs/删除/删除Pack无用数据")]
    public static void DeletePackUselessData()
    {
		List<string> dirsUsefulData = FileUtils.ReadFile(Path.Combine(System.Environment.CurrentDirectory, GameConstants.USEFUL_DATA_PATH));
        for (int i = 0; i < dirsUsefulData.Count; i++)
        {
            dirsUsefulData[i] = Path.Combine(Application.dataPath, dirsUsefulData[i]);
        }

		List<string> dirsUselessData =FileUtils.ReadFile( Path.Combine(System.Environment.CurrentDirectory, GameConstants.USELESS_DATA_PATH));
       
        for (int i = 0; i < dirsUselessData.Count; i++) {
            if (string.IsNullOrEmpty(dirsUselessData[i])) {
                continue;
            }
            if (dirsUselessData[i].Equals("\\") || dirsUselessData[i].Equals("/"))
            {
                continue;
            }
            string dirPath = Path.Combine(Application.dataPath, dirsUselessData[i]);

            if (File.Exists(dirPath))
            {
                FileUtils.DeleteFile(dirPath);
                continue;
            }
            List<string> dirsList = new List<string>();
            GetDirectionariesRecursion(dirPath, ref dirsList);
            //Debug.Log(MiniJSON.Json.Serialize(dirsList));
            for (int j = 0; j < dirsList.Count; j++)
            {
                if (CheckPathIsInValid(dirsList[j], dirsUsefulData))
                {
                    continue;
                }
                if (Directory.Exists(dirsList[j]))
                {
                    //Debug.Log("DeleteDir:"+dirsList[j]);
                    DirectoryInfo dir = new DirectoryInfo(dirsList[j]);
                    dir.Delete(true);
                }
            }
            //如果获取
            List<string> filesList = new List<string>();
            GetFilesRecursion(dirPath, ref filesList);
            for (int k = 0; k < filesList.Count; k++)
            {
                if (CheckPathIsInValid(filesList[k], dirsUsefulData))
                {
                    continue;
                }
                FileUtils.DeleteFile(filesList[k]);
            }
        }
       
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        Debug.Log("Delete PackUselessData success!");
    }

    public static bool CheckPathIsInValid(string path,List<string> list){
        if (list==null)
        {
            return false;
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Contains(path)||path.Contains(list[i]+"/"))
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 递归遍历
    /// </summary>
    /// <param name="pathname"></param>
    /// <param name="list"></param>
    public static void GetFilesRecursion(string pathname, ref List<string> list)
    {
        if (!Directory.Exists(pathname))
        {
            return;
        }
        string[] subFiles = Directory.GetFiles(pathname);
        foreach (string subFile in subFiles)
        {
            //Console.WriteLine(subFile);
            list.Add(subFile);
        }

        string[] subDirs = Directory.GetDirectories(pathname);
        foreach (string subDir in subDirs)
        {
            GetFilesRecursion(subDir, ref list);
        }
    }

    /// <summary>
    /// 递归遍历
    /// </summary>
    /// <param name="pathname"></param>
    /// <param name="list"></param>
    public static void GetDirectionariesRecursion(string pathname, ref List<string> list)
    {
        list.Add(pathname);
        if (!Directory.Exists(pathname))
        {
            return;
        }
        string[] subDirs = Directory.GetDirectories(pathname);
        foreach (string subDir in subDirs)
        {
            GetDirectionariesRecursion(subDir, ref list);
        }
    }
}
