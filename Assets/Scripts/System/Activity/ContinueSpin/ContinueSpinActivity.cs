using System;
using System.Collections.Generic;
using Libs;
using UnityEngine;
using Random = UnityEngine.Random;
namespace Activity
{
    public class ContinueSpinActivity:BaseActivity
    {
        
        private static int _activeSpinCount;
        
        public static bool IsActive = false;
        public static bool IsFirstPop = false;
        
        public static bool CanOpen = false;

        public static bool IsCloseReward = false;
        
        private const string BASE_RESULT_CHANGE_SPIN_TIMES = "BASE_RESULT_CHANGE_SPIN_TIMES";
        private const string CONTINUE_SPIN_IS_FRIST_POP = "CONTINUE_SPIN_IS_FRIST_POP"; //是否是第一次
        
        private int _minReward = 0;
        private int _maxReward = 100;
        
        private int _closeMinReward = 0;
        private int _closeMaxReward = 100;
        
        private static int _autoPopCount;

        private int _canOpenSpinCount = 0;//可以打开活动面板时，旋转的次数，需要自动打开
        
        public ContinueSpinActivity(Dictionary<string, object> data) : base(data)
        {
            ParseTaskData();
            
            //int curCount = SharedPlayerPrefs.GetPlayerPrefsIntValue(SPIN_COUNT_KEY, 0);
            
            //IsActive =  curCount > spinCount;
            CheckActive();
            
            UpdateProgress();
        }

        private void CheckActive()
        {
            var allSpinCount =  SharedPlayerPrefs.GetPlayerPrefsIntValue(BASE_RESULT_CHANGE_SPIN_TIMES,0);
                
            IsActive =  allSpinCount >= _activeSpinCount;
        }
        
        protected override void ParseTaskData()
        {
            //ActiveSpinCount =  Utils.Utilities.GetInt(Data,"ActiveSpinCount", 0);
            Dictionary<string, object> taskData = Utils.Utilities.GetValue<Dictionary<string,object>>(Data, ActivityConstants.TASKS, null);
            if (taskData==null || taskData.Count==0)
            {
                return;
            }
            int taskId =  Utils.Utilities.GetInt(taskData,TaskConstants.TaskId_Key, 0);
            
            _activeSpinCount = Utils.Utilities.GetInt(taskData,"activeSpinCount", 0);
            
            _autoPopCount = Utils.Utilities.GetInt(taskData,"autoPopCount", 2);
            
            IsFirstPop = SharedPlayerPrefs.GetPlayerBoolValue(CONTINUE_SPIN_IS_FRIST_POP,true);
            
            Task = TaskManager.Instance.RegisterTask(taskId,taskData);
            string[] rewards = Task.RewardList.Split(",");
            _minReward = int.Parse(rewards[0]);
            _maxReward = int.Parse(rewards[1]);
            
            string closeRewardString = Utils.Utilities.GetString(taskData,"closeRewardList", "100,200");
            string[] closeRewards = closeRewardString.Split(",");
            _closeMinReward = int.Parse(closeRewards[0]);
            _closeMaxReward = int.Parse(closeRewards[1]);
        }

        public override void AddListener()
        {
            base.AddListener();
            Messenger.AddListener(Task.UpdateTaskDataMsg,UpdateProgress);
            Messenger.AddListener(SlotControllerConstants.OnSpinEnd,UpdateSpinCount);
        }
        public void UpdateProgress()
        {
            if (IsActive)
            {
                if (icon!=null)
                {
                    float  value = 0;
                    if (Task!=null)
                    {
                        value = Task.HasCollectNum* 1.0f / Task.TargetNum ;
                    }
                    icon.RefreshProgress(value);
                    CanOpen = Mathf.Approximately(value, 1);
                    if (IsFirstPop && CanOpen)
                    {
                        ShowContinueSpinDialog();
                    }
                }
            }
            else
            {
                CheckActive();

                ResetTask();
            }
            
        }
        public override void OnClickIcon()
        {
            //base.OnClickIcon();
            ShowContinueSpinDialog();
        }

        public void ResetTask()
        {
            Task.HasCollectNum = 0;
            Task.IsTaskConditionOK = false;
            if (icon != null)
            {
                icon.RefreshProgress(0);
            }
           
        }

        public static void SaveData()
        {
            IsFirstPop = false;
            SharedPlayerPrefs.SetPlayerPrefsBoolValue(CONTINUE_SPIN_IS_FRIST_POP,IsFirstPop);
        }
        

        public override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener(Task.UpdateTaskDataMsg,UpdateProgress);
            Messenger.AddListener(SlotControllerConstants.OnSpinEnd,UpdateSpinCount);
        }
        private void UpdateSpinCount()
        {
            if(!CanOpen) return;

            _canOpenSpinCount++;

            if (_canOpenSpinCount >= _autoPopCount)
            {
                ShowContinueSpinDialog();
            }
            
        }

        public override BaseIcon RegisterIcon(GameObject go)
        {
            icon = go.AddComponent<ContinueSpinTaskIcon>();
            icon.OnInit(id,iconData);
            return icon;
        }
        
        private void ShowContinueSpinDialog()
        {
            _canOpenSpinCount = 0;
            Messenger.Broadcast<int>(GameDialogManager.OpenContinueSpinDialogMsg,id);
        }

        public int GetReward()
        {
            int randomReward;
            if (IsCloseReward)
            {
                randomReward = Random.Range(_closeMinReward,_closeMaxReward + 1);
            }
            else
            {
                randomReward = Random.Range(_minReward,_maxReward + 1);
            }
            return randomReward;
        }
        

    }
}