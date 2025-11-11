using Activity;
using Ads;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
namespace System.Activity.DailyTaskActivity
{
    public class DailyTaskItem : MonoBehaviour
    {
        public TextMeshProUGUI rewardCount;

        public TextMeshProUGUI taskDes;

        public Image taskProgress;

        public TextMeshProUGUI taskProgressTxt;

        public GameObject finishIcon;
        
        public GameObject maskObj;
        
        public Button getBtn;

        private DailyTaskData _dailyTaskData;

        
        private void Start()
        {
            getBtn.onClick.AddListener(OnTaskBtnClick);
        }
        private void OnTaskBtnClick()
        {
            DailyTaskActivity.PlayAdTaskId = _dailyTaskData.Task.TaskId;
            if (_dailyTaskData.ShowAd)
            {
                Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance,ADEntrances.REWARD_VIDEO_DAILY_TASK);
            }
            else
            {
                ShowVideoCallBack(1);
            }
        }

        public void SetData(DailyTaskData dailyTaskData)
        {
            _dailyTaskData = dailyTaskData;
            taskProgress.fillAmount = GetProgress();
            taskProgressTxt.text = GetProgressText();
            taskDes.text = TaskManager.Instance.GetTaskInfo(_dailyTaskData.Task);
            rewardCount.text = OnLineEarningMgr.Instance.GetMoneyStr(_dailyTaskData.Reward, needIcon: false);
            RefreshMask();
            Messenger.AddListener(_dailyTaskData.Task.UpdateTaskDataMsg, UpdateProgress);
        }
        
        private void RefreshMask()
        {
            if (_dailyTaskData.Task.State == (int)TaskState.ONGOING)
            {
                getBtn.interactable = false;
                finishIcon.gameObject.SetActive(false);
                maskObj.gameObject.SetActive(false);
                return;
            }
            
            if (_dailyTaskData.RewardState == DailyTaskActivity.GetMoneyFinish)
            {
                getBtn.interactable = false;
                maskObj.gameObject.SetActive(true);
                finishIcon.gameObject.SetActive(false);
            }
            else
            {
                getBtn.interactable = true;
                finishIcon.gameObject.SetActive(true);
                maskObj.gameObject.SetActive(false);
            }
            
        }
     
        private void UpdateProgress()
        {
            taskProgress.fillAmount = GetProgress();
            taskProgressTxt.text = GetProgressText();
            RefreshMask();
        }

        public void OnDisable()
        {
            Messenger.RemoveListener(_dailyTaskData.Task.UpdateTaskDataMsg, UpdateProgress);
            Messenger.RemoveListener<int>(ADConstants.PlayDailyTaskAD, ShowVideoCallBack);
        }
        
        private void OnEnable()
        {
            Messenger.AddListener<int>(ADConstants.PlayDailyTaskAD, ShowVideoCallBack);
        }
        private void ShowVideoCallBack(int arg0)
        {
            if( DailyTaskActivity.PlayAdTaskId != _dailyTaskData.Task.TaskId) return;
            _dailyTaskData.SetRewardState(DailyTaskActivity.GetMoneyFinish);
            getBtn.interactable = false;
            finishIcon.gameObject.SetActive(false);
            maskObj.gameObject.SetActive(true);
            SetCoins();
            
            if (ActivityManager.Instance.GetActivityByID(DailyTaskActivity.ActivityId) is DailyTaskActivity activity)
            {
                Messenger.Broadcast(DailyTaskActivity.SetRedPointState,activity.CheckHasFinishTask());
            }
            
        }

        private string GetTaskInfoDesc()
        {
            string info = "";
            string key = _dailyTaskData.Task.GetDesc();
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("WithDrawTaskActivity GetTaskInfoDesc error, key is null or empty, taskId: " + _dailyTaskData.Task.TaskId);
                return info;
            }
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName, key)
            {
                Arguments = new object[]
                {
                    GetProgressInfo()
                }
            };
            info = localizedString.GetLocalizedString();

            return info;
        }

        private string GetProgressText()
        {
            string info = string.Empty;
            if (_dailyTaskData != null)
            {
                info = _dailyTaskData.Task.GetProgressDesc();
            }
            return info;
        }

        private float GetProgress()
        {
            if (_dailyTaskData == null) return 0;
            
            float f = _dailyTaskData.Task.HasCollectNum * 1.0f / _dailyTaskData.Task.TargetNum;

            if (_dailyTaskData.Task.HasCollectNum != _dailyTaskData.Task.TargetNum) return f;
            
            if (_dailyTaskData.Task.State != (int)TaskState.ONGOING) return f;

            if (!_dailyTaskData.IsMainTask)
            {
                _dailyTaskData.SetRewardState(DailyTaskActivity.CanGetMoney);
                _dailyTaskData.Task.CompleteTask();
                Messenger.Broadcast(DailyTaskActivity.SetRedPointState,true);
            }
            
            return f;
        }

        private string GetProgressInfo()
        {
            string info = string.Empty;
            if (_dailyTaskData == null)
                return info;
            if (_dailyTaskData.Task.TaskType is TaskConstants.AccumulateCashTask_Key or TaskConstants.CollectCashFromZeroTask_Key)
            {
                var money = OnLineEarningMgr.Instance.GetMoneyStr((int)_dailyTaskData.Task.TargetNum, 2, false, true);
                info = $"<color=#FFFD3A>{money}</color>";
            }
            else
            {
                info = $"<color=#FFFD3A>{_dailyTaskData.Task.TargetNum}</color>";
            }
            return info;
        }
        
        
        private void SetCoins()
        {
            var randomReward = _dailyTaskData.Reward;
            OnLineEarningMgr.Instance.IncreaseCash(randomReward);
            Messenger.Broadcast<Transform,CoinsBezier.BezierType, Action>(
                GameConstants.CollectBonusWithType, transform, CoinsBezier.BezierType.DailyBonus, null);
            Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            //Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
            new DelayAction(0.8f,null, () =>
            {
                Messenger.Broadcast(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL);
            }).Play();
        }
    }


}