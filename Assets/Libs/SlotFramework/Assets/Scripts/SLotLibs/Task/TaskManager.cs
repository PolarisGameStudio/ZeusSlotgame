using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugins;
using UnityEngine;
using Utils;

namespace Libs
{
    public class TaskManager:MonoSingleton<TaskManager>
    {
        public TaskDataProgress taskDataProgress = new TaskDataProgress();
        public Dictionary<int, BaseTask> taskDict = new Dictionary<int, BaseTask>();
        public void OnInit()
        {
            LoadProgressData();
        }

        void LoadProgressData()
        {
            TaskDataProgress  data = StoreManager.Instance.LoadDataJson<TaskDataProgress>(taskDataProgress.fileName);
            if (data!=null)
            {
                taskDataProgress.LoadData(data);
            }
        }
        
        public void SaveProgressData()
        {
            taskDataProgress.SaveData();
        }

        public List<Dictionary<string,object>> ConvertTaskDataToJson()
        {
            List<Dictionary<string,object>> taskDataList = new List<Dictionary<string, object>>();
            foreach (var item in taskDict)
            {
                Dictionary<string,object> dict = item.Value.GetSaveDataDict();
                taskDataList.Add(dict);
            }
            return taskDataList;
        }
        
        /// <summary>
        /// 新创建的任务需要调用此方法克隆本地保存的任务进度
        /// </summary>
        /// <param name="task"></param>
        public void CloneTaskProgress(BaseTask task)
        {
            //再创建本地保存的任务
            if (taskDataProgress.taskDataList == null || taskDataProgress.taskDataList.Count == 0)
            {
                return;
            }
            foreach (var taskDictItem in taskDataProgress.taskDataList)
            {
                int taskId = Utilities.GetInt(taskDictItem, TaskConstants.TaskId_Key, 0);
                if (taskDict.ContainsKey(taskId))
                {
                    //已经存在的任务不再创建
                    continue;
                }

                if (taskId != task.TaskId)
                {
                    continue;
                }
                //加载本地保存的任务数据
                task.LoadSaveDataDict(taskDictItem);
                taskDict[taskId] = task;
                break;
            }
        }
        
        /// <summary>
        /// 外部系统通过此方法获取任务并注册，已缓存的直接获取，未缓存的直接创建
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        public BaseTask RegisterTask(int taskId,Dictionary<string,object> dict,BaseTask parentTask=null)
        {
            //创建对象，主要是走一遍构造函数
            BaseTask task = TaskFactory.CreateTask(dict, parentTask);
            CloneTaskProgress(task);
            taskDict[taskId] = task;
            return task;
        }

        /// <summary>
        /// 为了特定业务新增的接口，使用慎重，违反了以 taskid为唯一标识的原则
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public BaseTask GetTaskByType(int type)
        {
            foreach (var taskItem in taskDict)
            {
                if (taskItem.Value.TaskType == type)
                {
                    return taskItem.Value;
                }
            }
            return null;
        }
        
        private void CreatePlistTask()
        {
            Dictionary<string, object> taskInfoDict =
                Configuration.GetInstance().GetValue<Dictionary<string,object>>(TaskConstants.PlistTask_Key,null);
            if (taskInfoDict == null)
            {
                return;
            }
            foreach (var taskItem in taskInfoDict)
            {
                int taskId = Int32.Parse(taskItem.Key);
                if (taskDict.ContainsKey(taskId))
                {
                    continue;
                }
                Dictionary<string, object> taskInfos = taskItem.Value as Dictionary<string, object>;
                if (taskInfos == null || taskInfos.Count == 0)
                {
                    Debug.LogError("[TaskManager][CreatePlistTask] taskInfos is null");
                    continue;
                }

                BaseTask task = TaskFactory.CreateTask(taskInfos);
                taskDict.Add(taskId,task);
            }
        }
        
        private BaseTask GetTaskById(int taskId)
        {
            if (!taskDict.ContainsKey(taskId))
            {
                return null;
            }

            return taskDict[taskId];
        }
    }
}