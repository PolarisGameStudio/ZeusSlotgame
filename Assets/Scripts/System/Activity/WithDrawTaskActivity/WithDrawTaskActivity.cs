using System;
using System.Collections.Generic;
using Libs;
using UnityEngine;
using UnityEngine.Localization;
using Utils;

namespace Activity
{
    public class WithDrawTaskActivity: BaseActivity
    {
        private const string WithDrawTaskProgress = "WithDrawTaskProgress";
        public CollectCardTypeCountTask CollectCardTypeCountTask;
        public CollectSpinCountTask CollectSpinCountTask;
        public CollectADCountTask CollectADCountTask;
        public AccumulateTotalCashTask AccumulateTotalCashTask;
        public CollectNewCardTypeCountTask CollectNewCardTypeCountTask;
        public CollectCashFromZeroTask CollectCashFromZeroTask;

        private List<object> taskData = new List<object>();
        public LocalizedString CollectNumCards;
        public LocalizedString SpinNumTimes;
        public LocalizedString WatchNumVideo;
        public LocalizedString CollectNumCash;
        private BaseAwardItem _baseAwardItem;
        public int CurrentTaskIndex
        {
            set
            {
                SharedPlayerPrefs.SetPlayerPrefsIntValue(WithDrawTaskProgress, value);
            }
            get
            {
                return SharedPlayerPrefs.GetPlayerPrefsIntValue(WithDrawTaskProgress, 0);
            }
        } //当前执行的任务下标索引
        //当前执行的任务索引
        public WithDrawTaskActivity(Dictionary<string, object> data) : base(data)
        {   
            ParseTaskData();
        }

        protected override void ParseTaskData()
        {
            base.ParseTaskData();
            taskData = Utilities.GetValue<List<object>>(Data, ActivityConstants.TASKS, null);
            if (taskData== null|| taskData.Count==0)
            {
                return;
            }

            if (taskData.Count>CurrentTaskIndex)
            {
                ChangeTask();
            }
        }

        /// <summary>
        /// 切换任务
        /// </summary>
        public void ChangeTask()
        {
            Dictionary<string,object> taskInfoDict = taskData[CurrentTaskIndex] as Dictionary<string, object>;
            if (taskInfoDict == null)
            {
                return;
            }
            int taskId = Utilities.GetInt(taskInfoDict, TaskConstants.TaskId_Key, 0);
            RemoveListener();
            Task = TaskManager.Instance.RegisterTask(taskId, taskInfoDict);
            if (Task == null)
            {
                Debug.LogError("WithDrawTaskActivity ParseTaskData error, taskId: " + taskId);
                return;
            }
            if (Task is CollectCardTypeCountTask)
            {
                CollectCardTypeCountTask = Task as CollectCardTypeCountTask;
            }
            else if (Task is CollectSpinCountTask)
            {
                CollectSpinCountTask = Task as CollectSpinCountTask;
            }
            else if (Task is CollectADCountTask)
            {
                CollectADCountTask = Task as CollectADCountTask;
            }
            else if (Task is AccumulateTotalCashTask)
            {
                AccumulateTotalCashTask = Task as AccumulateTotalCashTask;
            }
            else if (Task is CollectNewCardTypeCountTask)
            {
                CollectNewCardTypeCountTask = Task as CollectNewCardTypeCountTask;
            }else if (Task is CollectCashFromZeroTask)
            {
                CollectCashFromZeroTask = Task as CollectCashFromZeroTask;
            }
            Messenger.AddListener(Task.UpdateTaskDataMsg, UpdateProgress);
            List<BaseAwardItem> baseAwardItems = GetTaskAward();
            if (baseAwardItems==null && baseAwardItems.Count == 0)
            {
                Debug.LogError("WithDrawTaskActivity GetTaskAward is null, activityId: " + id);
                return;
            }
            _baseAwardItem = baseAwardItems[0];
            SendMsgToPlatform();
        }
        void RemoveListener()
        {
            if (Task != null)
            {
                Messenger.RemoveListener(Task.UpdateTaskDataMsg, UpdateProgress);
            }
        }
        
        void SendMsgToPlatform()
        {
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint, "TaskNum",CurrentTaskIndex);
        }
        
        /// <summary>
        /// 更新任务进度
        /// </summary>
        void UpdateProgress()
        {
            if (Task.IsTaskConditionOK && Task.State == (int)TaskState.ONGOING)
            {
                //更新进度
                if (icon != null && icon is WithDrawTaskIcon withDrawTaskIcon)
                {
                    //发放奖励
                    withDrawTaskIcon.RefreshProgress(GetProgress(),GetProgressText());
                    //切换至关闭状态
                    Task.State = (int)TaskState.CLOSE;
                    withDrawTaskIcon.PlayAwardAnim(_baseAwardItem);
                }
            }
            else
            {
                //更新进度
                if (icon != null && icon is WithDrawTaskIcon withDrawTaskIcon)
                {
                    withDrawTaskIcon.RefreshProgress(GetProgress(),GetProgressText());
                }
            }
        }
        
        /// <summary>
        /// 用在任务切换时，上个任务完成，领奖表现播放完毕，刷新下一个任务
        /// </summary>
        public void DelayChangeTask()
        {
            if (Task == null)
            {
                Debug.LogError("WithDrawTaskActivity DelayChangeTask error, Task is null");
                return;
            }
            //任务完成，切换到下一个任务
            CurrentTaskIndex++;
            ChangeTask();
            //icon需要做表现
            (icon as WithDrawTaskIcon).DelayChangeTask();
        }

        ///检查任务状态
        /// <summary>
        /// 在创建任务通知 icon刷新 ui后，检测当前任务状态是否已完成，若完成，广播消息
        /// </summary>
        public void CheckTaskState()
        {
            //主要是针对收集金钱的任务，此任务有可能提前完成
            if (Task.IsTaskConditionOK && Task.State == (int)TaskState.ONGOING)
            {
                Messenger.Broadcast(Task.UpdateTaskDataMsg);
            }
        }
        
        public override BaseIcon RegisterIcon(GameObject go)
        {
            icon = go.GetComponent<WithDrawTaskIcon>();
            if (icon==null)
            {
                icon = go.AddComponent<WithDrawTaskIcon>();
            }
            icon.OnInit(id,iconData);
            return icon;
        }

        public float GetProgress()
        {
            if (Task!=null)
            {
                float f = Task.HasCollectNum * 1.0f / Task.TargetNum;
                return f;
            }
            return 0;
        }
        
        public string GetProgressInfo()
        {
            string info = string.Empty;
            if (Task!= null)
            {
                if (Task.TaskType ==TaskConstants.AccumulateCashTask_Key || Task.TaskType==TaskConstants.CollectCashFromZeroTask_Key)
                {
                    info = string.Format("<color=#FFFD3A>{0}</color>",OnLineEarningMgr.Instance.GetMoneyStr((int)Task.TargetNum,0,false,true));
                }
                else
                {
                    info = string.Format("<color=#FFFD3A>{0}</color>",Task.TargetNum);
                }
            }
            return info;
        }

        public string GetProgressText()
        {
            string info = string.Empty;
            if (Task!= null)
            {
                info = Task.GetProgressDesc();
            }
            return info;
        }

        public int GetTaskIconId()
        {
            int iconId = 0;
            if (Task!= null)
            {
                if (Task is CollectCardTypeCountTask|| Task is CollectNewCardTypeCountTask)
                {
                    iconId = 3;
                }
                else if (Task is CollectSpinCountTask)
                {
                    iconId = 0;                
                }
                else if (Task is CollectADCountTask)
                {
                    iconId = 1;
                }
                else if (Task is AccumulateTotalCashTask || Task is CollectCashFromZeroTask)
                {
                    iconId = 2;
                }
            }
            return iconId;
        }

        public List<BaseAwardItem> GetTaskAward()
        {
            if (Task != null)
            {
                return RewardManager.Instance.CreateRewardByStr(Task.RewardList);
            }
            return null;
        }

        public string GetTaskAwardCountDesc()
        {
            List<BaseAwardItem> baseAwardItems = GetTaskAward();
            if (baseAwardItems==null && baseAwardItems.Count == 0)
            {
                Debug.LogError("WithDrawTaskActivity GetTaskAward is null, activityId: " + id);
                return string.Empty;
            }
            BaseAwardItem baseAwardItem = baseAwardItems[0];
            if (baseAwardItem != null)
            {
                if (baseAwardItem is CashAwardItem cashAwardItem)
                {
                    return cashAwardItem.GetAwardCountDesc();
                }
                else
                {
                    return baseAwardItem.GetAwardCountDesc();
                }
            }
            return string.Empty;
        }

        ///用于 withdrawdialog 显示当前任务面板信息
        public string GetIconTaskInfo()
        {
            string info = GetTaskInfoDesc();
            info = info.Replace("FFFD3A", "FFFF00");
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
            return info+". "+progressInfo;
        }

        public string GetTaskInfoDesc()
        {
            string info = "";
            string key = Task.GetDesc();
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("WithDrawTaskActivity GetTaskInfoDesc error, key is null or empty, taskId: " + Task.TaskId);
                return info;
            }
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,key);
            if (localizedString!=null)
            {
                localizedString.Arguments = new object[] { GetProgressInfo() };
                info = localizedString.GetLocalizedString();
            }

            return info;
        }

        public string GetTaskInfoDescForTip()
        {
            string info = "";
            string key = "";
            string arg1 = "";
            if (Task is CollectSpinCountTask)
            {
                key = "MoreSpinTimes";
                arg1 = string.Format("<color=#D800D9>{0}</color>",Task.TargetNum - Task.HasCollectNum);
            }else if (Task is CollectADCountTask)
            {
                key = "MoreVideoAds";
                arg1 = string.Format("<color=#D800D9>{0}</color>",Task.TargetNum - Task.HasCollectNum);
            }
            else if (Task is CollectCashFromZeroTask)
            {
                key = "MoreWinCash";
                int leftCash = (int)(Task.TargetNum - Task.HasCollectNum);
                arg1 = string.Format("<color=#D800D9>{0}</color>",OnLineEarningMgr.Instance.GetMoneyStr(leftCash, 2, false, true));
            }else if (Task is CollectCardTypeCountTask)
            {
                key = "MoreCollectCards";
                arg1 = string.Format("<color=#D800D9>{0}</color>",Task.TargetNum - Task.HasCollectNum);
            }
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,key);
            if (localizedString!=null)
            {
                
                localizedString.Arguments = new object[] { arg1};
                info = localizedString.GetLocalizedString();
            }
            return info;
        }
        
        
        
        public override void OnClickIcon()
        {
            base.OnClickIcon();
            WithDrawManager.Instance.ShowWithDrawDialog();
        }
    }
}