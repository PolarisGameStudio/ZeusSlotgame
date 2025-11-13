using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugins;
using UnityEngine;
using UnityEngine.Localization;
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
        
        
        
        private BaseTask GetTaskById(int taskId)
        {
            if (!taskDict.ContainsKey(taskId))
            {
                return null;
            }

            return taskDict[taskId];
        }
        
        public string GetTaskInfo(BaseTask Task)
        {
            string info = GetTaskInfos(Task);
            if (string.IsNullOrEmpty(info))
            {
                return info;
            }
            //去除info字符串中”()“之间包含的字符串的内容：譬如"info(fjsdffs)cds"处理后变为"infocds"
            info = info.Substring(0, info.IndexOf("(")) + info.Substring(info.IndexOf(")") + 1);
            //info中有‘.’字符，需要替换为‘.’
            // info = info.Replace(".", "");
            string progressInfo = "";
            if (Task is CollectCashFromZeroTask)
            {
                progressInfo = string.Format("<color=#FFFF00>({0}/{1})</color>",OnLineEarningMgr.Instance.GetMoneyStr((int)Task.HasCollectNum,0,false,true),
                    OnLineEarningMgr.Instance.GetMoneyStr((int)Task.TargetNum,0,false,true));
            }
            else
            {
                progressInfo = string.Format("<color=#FFFF00>({0}/{1})</color>",Task.HasCollectNum,Task.TargetNum);
            }

            //info最后结束于'.'字符，则去除这个字符
            if (info.EndsWith("."))
            {
                info = info.Substring(0, info.Length - 1);
            }
            return info+". "+progressInfo;
        }


        //获取任务信息描述
        public string GetTaskInfos(BaseTask task)
        {
            string info = "";
            LocalizedString title = null;
            switch (task.TaskType)
            {
                case TaskConstants.CollectSpinCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawspin");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectNewCardTypeCountTask_Key:
                case TaskConstants.CollectNewCardCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawcard");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectCashFromZeroTask_Key:
                case TaskConstants.AccumulateCashTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawcash");
                    title.Arguments = new object[] {OnLineEarningMgr.Instance.GetMoneyStr((int)task.TargetNum, needIcon: false)};
                    break;
                case TaskConstants.WatchADTimeTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawad");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectFreeGameTriggerCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawfreegame");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectWildSymbolCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdraww01");
                    title.Arguments = new object[] {task.TargetNum,"<sprite=0>"};
                    break;
                case TaskConstants.CollectSymbolCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdraws01");
                    title.Arguments = new object[] {task.TargetNum,"<sprite=1>"};
                    break;
                case TaskConstants.CollectTriggerSpinWinCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawwin");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectJackpotGameCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawjackpot");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectLoginDaysTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawlogin");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectLuckyGiftAdTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "CollectLuckyGift");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
            }
            info = title.GetLocalizedString();
            return info;
        }
    }
}