using System.Collections.Generic;
using System.Linq;
using Activity;
using Libs;
using UnityEngine;
namespace System.Activity.DailyTaskActivity
{
    public class DailyTaskData
    {
        public BaseTask Task;
        public bool ShowAd;
        public int RewardState;
        public int Reward;
        public bool IsMainTask;
        
        private const string RewardStateKey = "RewardStateKey";//是否领取奖励
        
        public void SetRewardState(int state)
        {
            RewardState = state;
            PlayerPrefs.SetInt(RewardStateKey + Task.TaskId, state);
        }

        public void GetRewardState()
        {
            RewardState = PlayerPrefs.GetInt(RewardStateKey + Task.TaskId, DailyTaskActivity.CanGetMoney);
        }
        
    }
    public class DailyTaskActivity : BaseActivity
    {
        public static int ActivityId;
        private readonly List<DailyTaskData> _taskList = new List<DailyTaskData>(4);

        public List<DailyTaskData> TaskList => _taskList;

        public const int GetMoneyFinish = 0;//领取完毕
        public const int CanGetMoney = 1;//可以点击领取
        
        private const string LastResetDateKey = "LastResetDate";

        public const string ChangeMainTask = "ChangeMainTask";
        public const string ChangeTime = "ChangeTime";
        public const string SetRedPointState = "SetRedPointState";

        public static int PlayAdTaskId;
        
        private float _leftTime;

        public float LeftTime => _leftTime;
        
        public DailyTaskActivity(Dictionary<string, object> data) : base(data)
        {
            ParseTaskData();
            CheckAndResetDailyTask();
        }

        protected override sealed void ParseTaskData()
        {
            ActivityId = id;

            List<object> taskData = Utils.Utilities.GetValue<List<object>>(Data, ActivityConstants.TASKS, null);
            if (taskData == null || taskData.Count == 0)
            {
                return;
            }

            foreach (var objectA in taskData)
            {
                Dictionary<string, object> taskInfoDict = objectA as Dictionary<string, object>;
                
                int taskId = Utils.Utilities.GetInt(taskInfoDict, TaskConstants.TaskId_Key, 0);
                
                var task = TaskManager.Instance.RegisterTask(taskId, taskInfoDict);
                var showAd = Utils.Utilities.GetBool(taskInfoDict, "ShowAd",false);
                var rewardCash = task.RewardList.Split(",")[1];
                int reward = int.Parse(rewardCash);
                var dailyTask = new DailyTaskData
                {
                    Task = task,
                    ShowAd = showAd,
                    Reward = reward * 100
                };
                dailyTask.GetRewardState();
                _taskList.Add(dailyTask);
            }
        }

        public void InitMainTask()
        {
            //构造主线任务
            if (ActivityManager.Instance.GetActivityByID(WithDrawTaskActivity.ActivityId) is  WithDrawTaskActivity activity)
            {
                var mainTask = activity.Task;
                var rewardCash = mainTask.RewardList.Split(",")[1];
                int reward = int.Parse(rewardCash);
                var task = new DailyTaskData
                {
                    Task = mainTask,
                    ShowAd = false,
                    RewardState = CanGetMoney,
                    Reward = reward * 100,
                    IsMainTask = true
                };
                _taskList.Add(task);
            }
        }
        
        
        private readonly List<DailyTaskData> _finishList = new List<DailyTaskData>();
        private readonly List<DailyTaskData> _canGetMoneyList = new List<DailyTaskData>();
        private readonly List<DailyTaskData> _unFinishList= new List<DailyTaskData>();
        private readonly List<DailyTaskData> _allList= new List<DailyTaskData>();

        public List<DailyTaskData> GetSortList()
        {
            _finishList.Clear();
            _canGetMoneyList.Clear();
            _unFinishList.Clear();
            _allList.Clear();
            
            foreach (var item in  _taskList)
            {
                if (item.Task.IsTaskConditionOK)
                {
                    if (item.RewardState == CanGetMoney)
                    {
                        _canGetMoneyList.Add(item);
                    }
                    else
                    {
                        _finishList.Add(item);
                    }
                }
                else
                {
                    _unFinishList.Add(item);
                }
            }
            
            _allList.AddRange(_canGetMoneyList);
            _allList.AddRange(_unFinishList);
            _allList.AddRange(_finishList);
            return _allList;
        }
        
        
        public override void OnClickIcon()
        {
            //base.OnClickIcon();
            ShowContinueSpinDialog();
        }
        
        public override BaseIcon RegisterIcon(GameObject go)
        {
            icon = go.AddComponent<DailyTaskIcon>();
            icon.OnInit(id, iconData);
            return icon;
        }

        
        private void ShowContinueSpinDialog()
        {
            //停止自动spin
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_SUSPEND);

            UIDialog dialog = UIManager.Instance.GetActiveDialog<DailyTaskDialog>();
            if (dialog != null)
            {
                return;
            }
            Debug.Log("ShowContinueSpinDialog");
            Messenger.Broadcast<int>(GameDialogManager.OpenDailyTaskDialogMsg, id);
        }

        public override void AddListener()
        {
            base.AddListener();
        }
        
       
        
        private void CheckAndResetDailyTask()
        {
            // 拿到上次重置日期
            string lastResetDate = PlayerPrefs.GetString(LastResetDateKey, "");
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            // 对比上次重置任务日期是不是今天
            if (lastResetDate != today)
            {
                // 重置任务
                ResetTasks();
                PlayerPrefs.SetString(LastResetDateKey, today);
                PlayerPrefs.Save();
            }

            // 计算今天到晚上12点还剩多少时间
            var now = DateTime.Now;
            var midnight = now.Date.AddDays(1); // 今天的午夜（明天的00:00）
            _leftTime = (float)(midnight - now).TotalSeconds;

            // 启动协程来处理倒计时逻辑
            CoroutineUtil.Instance.StartCoroutine(CountdownCoroutine());
        }

        
        private Collections.IEnumerator CountdownCoroutine()
        {
            while (_leftTime >= 0)
            {
                Messenger.Broadcast(ChangeTime);
                // 这里可以更新UI显示倒计时
                //Debug.Log("倒计时: " + timeString);

                // 不要每次new WaitForSeconds
                yield return new WaitForSeconds(1);

                _leftTime--;
            }

            CountDownFinish();
        }

        private void CountDownFinish()
        {
            ResetTasks();
            // 记录重置日期
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            PlayerPrefs.SetString(LastResetDateKey, today);
            PlayerPrefs.Save();

            // 重新计算到明天24点的时间
            var now = DateTime.Now;
            var midnight = now.Date.AddDays(1);
            _leftTime = (float)(midnight - now).TotalSeconds;

            CoroutineUtil.Instance.StartCoroutine(CountdownCoroutine());
        }

        public bool CheckHasFinishTask()
        {
            foreach (var dailyTaskData in _taskList)
            {
                if (dailyTaskData.Task.IsTaskConditionOK && dailyTaskData.RewardState == CanGetMoney) return true;
            }
            return false;
        }
        
        
        private void ResetTasks()
        {
           
            foreach (DailyTaskData dailyTaskData in _taskList)
            {
                if (dailyTaskData.IsMainTask)
                {
                    //主线任务不重置
                    continue;
                }
                dailyTaskData.Task.HasCollectNum = 0;
                dailyTaskData.Task.State = (int)TaskState.ONGOING;
                dailyTaskData.Task.IsTaskConditionOK = false;
                dailyTaskData.RewardState = CanGetMoney;
                Messenger.Broadcast(dailyTaskData.Task.UpdateTaskDataMsg);
            }
            Debug.Log("任务已重置！");
        }

    }
}