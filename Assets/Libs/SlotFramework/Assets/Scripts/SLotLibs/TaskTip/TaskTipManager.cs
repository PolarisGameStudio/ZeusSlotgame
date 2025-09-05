using System;
using System.Collections.Generic;
using Activity;
using Classic;
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
        public int FreeSymbolNum = 0;
        public int S01SymbolNum = 0;
        private TaskTipProgress taskTipProgress = new TaskTipProgress();

        public Dictionary<int, bool> TaskTipStatus = new Dictionary<int, bool>(); //任务提示状态，key为任务类型，value为是否展示提示

        public void Init()
        {
            ParseConfig();
            if (!isOpen)
            {
                return;
            }
            AddListener();
            LoadProgressData();
        }

        void LoadProgressData()
        {
            TaskTipProgress data = StoreManager.Instance.LoadDataJson<TaskTipProgress>(taskTipProgress.fileName);
            if (data != null)
            {
                taskTipProgress.LoadData(data);
                CurrentSpinCount = data.spinCount;
                TaskTipStatus = data.taskTipStatus;
                FreeSymbolNum = data.FreeSymbolNum;
                S01SymbolNum = data.S01SymbolNum;
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
                // Debug.LogError("TaskTipManager: ParseConfig failed, config is null");
                return;
            }

            isOpen = Utils.Utilities.GetBool(config, "IsOpen", false);
            if (!isOpen)
            {
                // Debug.Log("TaskTipManager: Task tips are disabled");
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
            Messenger.AddListener<ReelManager, long>(GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
            Messenger.AddListener(SlotControllerConstants.OnSpinEnd, OnSpinEnd);
        }

        void RemoveListener()
        {
            Messenger.RemoveListener<ReelManager, long>(GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
            Messenger.RemoveListener(SlotControllerConstants.OnSpinEnd, OnSpinEnd);
        }

        void OnSpinAwardEnd(ReelManager reelManager, long totalWinCoins)
        {
            CheckShowW01TaskTip(reelManager, totalWinCoins);
            CheckShowS01TaskTip(reelManager, totalWinCoins);
        }

        void CheckShowW01TaskTip(ReelManager reelManager, long totalWinCoins)
        {
            BaseTask task = TaskManager.Instance.GetTaskByType(TaskConstants.CollectWildSymbolCountTask_Key);
            if (task == null)
            {
                Debug.LogError("TaskTipPanel task is null, taskType: " + TaskConstants.CollectWildSymbolCountTask_Key);
                return;
            }

            if (FreeSymbolNum >= task.TargetNum)
            {
                return;
            }

            // Debug.Log($"[WithDrawManager][OnSpinAwardEnd] FreeSymbolNum:{FreeSymbolNum}");
            int AddNumber = reelManager.GetSpecialCount(SymbolMap.IS_WILD);
            long remin = FreeSymbolNum % 10;
            FreeSymbolNum += AddNumber;
            if (remin + AddNumber < 10)
            {
                return;
            }

            int symbolIndex = reelManager.symbolMap.getSymbolIndex("W01");
            Sprite symbolSprite = reelManager.gameConfigs.elementResources[symbolIndex].staticSprite;
            Messenger.Broadcast<Sprite, int>(GameDialogManager.OpenTaskTipsDialogMsg, symbolSprite,
                TaskConstants.CollectWildSymbolCountTask_Key);
            //更新进度数据
            SaveProgressData();
        }

        void CheckShowS01TaskTip(ReelManager reelManager, long totalWinCoins)
        {
            BaseTask task = TaskManager.Instance.GetTaskByType(TaskConstants.CollectSymbolCountTask_Key);
            if (task == null)
            {
                Debug.LogError("TaskTipPanel task is null, taskType: " + TaskConstants.CollectSymbolCountTask_Key);
                return;
            }

            if (S01SymbolNum >= task.TargetNum)
            {
                return;
            }

            CollectSymbolCountTask collectSymbolCountTask = task as CollectSymbolCountTask;
            Debug.Log($"[WithDrawManager][OnSpinAwardEnd] S01SymbolNum:{S01SymbolNum}");
            List<BaseElementPanel> elementList =
                reelManager.GetElementsWithSymbolName(collectSymbolCountTask.symbolName);
            int AddNumber = (elementList == null || elementList.Count == 0) ? 0 : elementList.Count;
            long remin = S01SymbolNum % 10;
            S01SymbolNum += AddNumber;
            if (remin + AddNumber < 10)
            {
                return;
            }

            int symbolIndex = reelManager.symbolMap.getSymbolIndex(collectSymbolCountTask.symbolName);
            Sprite symbolSprite = reelManager.gameConfigs.elementResources[symbolIndex].staticSprite;
            Messenger.Broadcast<Sprite, int>(GameDialogManager.OpenTaskTipsDialogMsg, symbolSprite,
                TaskConstants.CollectSymbolCountTask_Key);
            //更新进度数据
            SaveProgressData();
        }

        void OnSpinEnd()
        {
            if (!isOpen)
            {
                return;
            }

            CurrentSpinCount++;
        }

        public bool CheckShow300CashTip()
        {
            if (!isOpen)
            {
                return false;
            }

            if (OnLineEarningMgr.Instance.isInfiniteOpen())
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

            if (!OnLineEarningMgr.Instance.isInfiniteOpen())
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

            //展示300模式的现金进度，跟CheckShowWithDrawActivityTip互斥
            if (CheckShow300CashTip())
            {
                return CreateCashTask();
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

        public string Get300CashStr()
        {
            string key = "MoreWinCash";
            string arg1 = string.Format("<color=#118D1D>{0}</color>", OnLineEarningMgr.Instance.GetMoneyStr(OnLineEarningMgr.Instance.Cash(), 0, false, true));
            string arg2 = string.Format("<color=#FF0000>{0}</color>", OnLineEarningMgr.Instance.GetCashStr(OnLineEarningMgr.Instance.GetMaxValue(), 0, false, true));
            var localizedString = new LocalizedString(LocalizationManager.Instance.tableName, key);
            localizedString.Arguments = new object[] { arg1 };
            return string.Format("{0} {1}.", localizedString.GetLocalizedString(), arg2);
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
                    task is CollectCashFromZeroTask || task is CollectCardTypeCountTask|| task is AccumulateTotalCashTask
                        ? "FF0000"
                        : "FDFF2F",
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
                arg1 = string.Format("<color=#29f706>{0}</color>", task.TargetNum);
            }
            else
            {
                int remaining = (int)(task.TargetNum - task.HasCollectNum);
                key = task switch
                {
                    CollectSpinCountTask _ => "MoreSpinTimes",
                    CollectADCountTask _ => "MoreVideoAds",
                    AccumulateTotalCashTask _ => "MoreWinCash",
                    CollectCashFromZeroTask _ => "MoreWinCash",
                    CollectCardTypeCountTask _ => "MoreCollectCards",
                    _ => string.Empty
                };

                arg1 = string.Format("<color=#118D1D>{0}</color>",
                    task is CollectCashFromZeroTask || task is AccumulateTotalCashTask
                        ? OnLineEarningMgr.Instance.GetMoneyStr(remaining, 2, false, true)
                        : remaining.ToString());
            }

            // 本地化处理
            if (string.IsNullOrEmpty(key)) return string.Empty;

            var localizedString = new LocalizedString(LocalizationManager.Instance.tableName, key);
            if (localizedString == null) return string.Empty;

            localizedString.Arguments = task is CollectSpinCountTask || task is CollectADCountTask ||
                                        task is CollectCashFromZeroTask || task is CollectCardTypeCountTask || task is AccumulateTotalCashTask
                ? new object[] { arg1 }
                : new object[] { arg1, arg2 };

            return task is CollectSpinCountTask || task is CollectADCountTask ||
                   task is CollectCashFromZeroTask || task is CollectCardTypeCountTask || task is AccumulateTotalCashTask
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
        BaseTask cashTask = null;
        private BaseTask CreateCashTask()
        {
            int maxValue = OnLineEarningMgr.Instance.GetMaxValue()*OnLineEarningMgr.Instance.GetCashMultiple();
            Dictionary<string,object> data = new Dictionary<string, object>
            {
                { TaskConstants.TaskId_Key, 80001},
                { TaskConstants.TargetNum_Key, OnLineEarningMgr.Instance.GetMaxValue()},
                { TaskConstants.RewardList_Key, string.Format("10001,{0}",OnLineEarningMgr.Instance.GetMaxValue()) },
                { TaskConstants.Type_Key, TaskConstants.CollectCashFromZeroTask_Key },
                { TaskConstants.CollectNumber_Key, OnLineEarningMgr.Instance.Cash()},
                { TaskConstants.TaskState_Key, (int)TaskState.ONGOING },
            };
            if (cashTask==null)
            {
                cashTask = new CollectCashFromZeroTask(data, null);
            }
            return cashTask;
        }
    }
}