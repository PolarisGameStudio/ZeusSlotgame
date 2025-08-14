using System.Collections.Generic;
using Activity;
using UnityEngine;
using UnityEngine.Localization;

namespace Libs
{
    public class TaskTipManager : MonoSingleton<TaskTipManager>
    {
        public bool isOpen = false;
        public int StartSpinLimit = 0;
        public int EndSpinLimit = 0;
        public int CurrentSpinCount = 0;

        private TaskTipProgress taskTipProgress = new TaskTipProgress();

        public Dictionary<int, bool> TaskTipStatus = new Dictionary<int, bool>(); //任务提示状态，key为任务类型，value为是否展示提示

        public void Init()
        {
            ParseConfig();
            AddListener();
            LoadProgressData();
        }

        void LoadProgressData()
        {
            TaskTipProgress data = StoreManager.Instance.LoadDataJson<TaskTipProgress>(taskTipProgress.fileName);
            if (data != null)
            {
                taskTipProgress.LoadData(data);
                CurrentSpinCount = taskTipProgress.spinCount;
                TaskTipStatus = taskTipProgress.taskTipStatus;
            }
        }

        public void SaveProgressData()
        {
            taskTipProgress.SaveData();
        }

        void ParseConfig()
        {
            Dictionary<string, object> config = Plugins.Configuration.GetInstance()
                .GetValue<Dictionary<string, object>>(TaskTipConstants.TaskTipConfigKey, null);
            if (config == null)
            {
                Debug.LogError("TaskTipManager: ParseConfig failed, config is null");
                return;
            }

            isOpen = Utils.Utilities.GetBool(config, "IsOpen", false);
            if (!isOpen)
            {
                Debug.Log("TaskTipManager: Task tips are disabled");
                return;
            }

            if (config.TryGetValue(TaskTipConstants.StartSpinLimit, out object startSpinLimitObj) &&
                startSpinLimitObj is int startSpinLimit)
            {
                StartSpinLimit = startSpinLimit;
            }
            else
            {
                Debug.LogError("TaskTipManager: StartSpinLimit not found or invalid type");
            }

            if (config.TryGetValue(TaskTipConstants.EndSpinLimit, out object endSpinLimitObj) &&
                endSpinLimitObj is int endSpinLimit)
            {
                EndSpinLimit = endSpinLimit;
            }
            else
            {
                Debug.LogError("TaskTipManager: EndSpinLimit not found or invalid type");
            }

            if (StartSpinLimit < 0 || EndSpinLimit < 0)
            {
                Debug.LogError("TaskTipManager: StartSpinLimit or EndSpinLimit cannot be negative");
            }
            else if (StartSpinLimit >= EndSpinLimit)
            {
                Debug.LogError("TaskTipManager: StartSpinLimit must be less than EndSpinLimit");
            }

            Debug.Log(
                $"TaskTipManager initialized with StartSpinLimit: {StartSpinLimit}, EndSpinLimit: {EndSpinLimit}");
        }

        public override void Dispose()
        {
            base.Dispose();
            RemoveListener();
        }

        void AddListener()
        {
            Messenger.AddListener(SlotControllerConstants.OnSpinEnd, OnSpinEnd);
        }

        void RemoveListener()
        {
            Messenger.RemoveListener(SlotControllerConstants.OnSpinEnd, OnSpinEnd);
        }

        void OnSpinEnd()
        {
            if (!isOpen)
            {
                return;
            }
            CurrentSpinCount++;
        }

        public bool CheckShowWithDrawTaskTip(int taskType)
        {
            if (!isOpen)
            {
                return false;
            }

            if (CurrentSpinCount < StartSpinLimit)
            {
                return false;
            } 
            
            if (CurrentSpinCount > EndSpinLimit)
            {
                return true;
            }

            if (!TaskTipStatus.ContainsKey(taskType))
            {
                TaskTipStatus[taskType] = true;
                return true;
            }

            bool isShow = TaskTipStatus[taskType];
            if (!isShow)
            {
                BaseTask task = TaskManager.Instance.GetTaskByType(taskType);
                if (task.IsConditionOK())
                {
                    return false;
                }
                SetTaskTipStatus(taskType, true);
                return true;
            }
            SetTaskTipStatus(taskType, false);
            return false;
        }

        public bool CheckShowWithDrawActivityTip()
        {
            if (!isOpen)
            {
                return false;
            }

            if (CurrentSpinCount < StartSpinLimit)
            {
                return true;
            }

            if (CurrentSpinCount >= EndSpinLimit)
            {
                return false;
            }

            return true;
        }

        bool isWithDrawDialogShow = false;
        bool isWithDrawActivityShow = false;

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns></returns>
        public BaseTask GetTask(int taskType)
        {
            isWithDrawDialogShow = false;
            isWithDrawActivityShow = false;
            //withdrawdialog展示的任务
            if (CheckShowWithDrawTaskTip(taskType))
            {
                Debug.Log("TaskTipManager isWithDrawDialogShow");
                isWithDrawDialogShow = true;
                return TaskManager.Instance.GetTaskByType(taskType);
            }

            // withdrawtaskactivity展示的任务
            if (CheckShowWithDrawActivityTip())
            {
                WithDrawTaskActivity acti =
                    ActivityManager.Instance.GetActivityByType(ActivityType.WithDrawTask) as WithDrawTaskActivity;
                if (acti == null)
                {
                    Debug.LogError("GetTask: WithDrawTaskActivity is null");
                    return null;
                }

                Debug.Log("TaskTipManager isWithDrawActivityShow");
                isWithDrawActivityShow = true;
                return acti.Task;
            }

            return null;
        }

        public string GetTaskTipText(BaseTask task)
        {
            if (task == null)
            {
                Debug.LogError("GetTaskTipText: Task is null");
                return string.Empty;
            }

            // 获取奖励项
            BaseAwardItem awardItem = null;
            if (!string.IsNullOrEmpty(task.RewardList))
            {
                var awardItems = RewardManager.Instance.CreateRewardByStr(task.RewardList);
                if (awardItems.Count > 0) awardItem = awardItems[0];
            }

            // 处理奖励文本
            string GetRewardText(BaseAwardItem item)
            {
                if (item == null) return string.Empty;
                var value = item.type == AwardType.Cash
                    ? OnLineEarningMgr.Instance.GetCashStr(item.count, 0, false, true)
                    : item.GetAwardCountDesc();
                return string.Format("<color=#{0}>{1}</color>",
                    task is CollectSpinCountTask || task is CollectADCountTask ||
                    task is CollectCashFromZeroTask || task is CollectCardTypeCountTask
                        ? "FDFF2F"
                        : "FF0000",
                    value);
            }

            string key;
            string arg1;
            string arg2 = GetRewardText(awardItem);

            // 确定任务类型和参数
            if (task is CollectSymbolCountTask || task is CollectWildSymbolCountTask ||
                task is CollectTriggerBigWinCountTask || task is CollectFreeGameTriggerCountTask ||
                task is CollectBonusGameCountTask)
            {
                key = task.GetDesc();
                arg1 = string.Format("<color=#118D1D>{0}</color>", task.TargetNum);
            }
            else
            {
                int remaining = (int)(task.TargetNum - task.HasCollectNum);
                key = task switch
                {
                    CollectSpinCountTask _ => "MoreSpinTimes",
                    CollectADCountTask _ => "MoreVideoAds",
                    CollectCashFromZeroTask _ => "MoreWinCash",
                    CollectCardTypeCountTask _ => "MoreCollectCards",
                    _ => string.Empty
                };

                arg1 = string.Format("<color=#29f706>{0}</color>",
                    task is CollectCashFromZeroTask
                        ? OnLineEarningMgr.Instance.GetMoneyStr(remaining, 2, false, true)
                        : remaining.ToString());
            }

            // 本地化处理
            if (string.IsNullOrEmpty(key)) return string.Empty;

            var localizedString = new LocalizedString(LocalizationManager.Instance.tableName, key);
            if (localizedString == null) return string.Empty;

            localizedString.Arguments = task is CollectSpinCountTask || task is CollectADCountTask ||
                                        task is CollectCashFromZeroTask || task is CollectCardTypeCountTask
                ? new object[] { arg1 }
                : new object[] { arg1, arg2 };

            return task is CollectSpinCountTask || task is CollectADCountTask ||
                   task is CollectCashFromZeroTask || task is CollectCardTypeCountTask
                ? string.Format("{0} {1}.", localizedString.GetLocalizedString(), arg2)
                : localizedString.GetLocalizedString();
        }


        public void SetTaskTipStatus(int taskType, bool isShow)
        {
            if (!TaskTipStatus.ContainsKey(taskType))
            {
                TaskTipStatus[taskType] = isShow;
            }
            else
            {
                TaskTipStatus[taskType] = isShow;
            }

            SaveProgressData();
        }
    }
}