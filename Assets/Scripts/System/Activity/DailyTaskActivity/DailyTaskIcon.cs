using System.Collections.Generic;
using Activity;
using DG.Tweening;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
namespace System.Activity.DailyTaskActivity
{
    public class DailyTaskIcon : BaseIcon
    {
        private Button _clickButton;
        private Transform _finishIcon;
        private TextMeshProUGUI _tipsTxt;
        private Transform _tip;
        private Transform _redPoint;

        private bool _firstTaskShow = true;

        private BaseTask _mainTask;

        private DailyTaskActivity _activity;

        public override void OnInit(int id, Dictionary<string, object> data)
        {
            base.OnInit(id, data);
            _clickButton = GetComponent<Button>();
            _finishIcon = transform.RealFindObj<Transform>("Tips/Layout/FinishIcon");
            _tipsTxt = transform.RealFindObj<TextMeshProUGUI>("Tips/Layout/TipsTxt");
            _redPoint = transform.RealFindObj<Transform>("RedPoint");
            _tip = transform.RealFindObj<Transform>("Tips");
            if (_clickButton != null)
            {
                _clickButton.onClick.AddListener(OnButtonClick);
            }
            ChangeMainTask();
            
            if (ActivityManager.Instance.GetActivityByID(DailyTaskActivity.ActivityId) is DailyTaskActivity activity)
            {
                _activity = activity;
                _redPoint.gameObject.SetActive(activity.CheckHasFinishTask());
                foreach (var dailyTaskData in activity.TaskList)
                {
                    if(dailyTaskData.IsMainTask) continue;
                    Messenger.AddListener(dailyTaskData.Task.UpdateTaskDataMsg,UpdateRedPoint);
                }
            }
        }
        private void UpdateRedPoint()
        {
            _redPoint.gameObject.SetActive(_activity.CheckHasFinishTask());
        }
        private void PlayMoneyAnim()
        {
            var rewardCount = _mainTask.RewardList.Split(",")[0];
            
            var reward = int.Parse(rewardCount) * 100;
            
            OnLineEarningMgr.Instance.IncreaseCash(reward);
            
            Messenger.Broadcast<Transform,CoinsBezier.BezierType, Action>(
                GameConstants.CollectBonusWithType, transform, CoinsBezier.BezierType.DailyBonus, null);
            Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            //Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
            new DelayAction(0.8f,null, () =>
            {
                Messenger.Broadcast(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL);
            }).Play();
        }
        
        private void UpdateMainTaskDes()
        {
            if (_mainTask.IsTaskConditionOK)
            {
                _finishIcon.gameObject.SetActive(true);
                if (!_firstTaskShow)
                {
                    ShowBubble();
                }
                DOVirtual.DelayedCall(0.3f, PlayMoneyAnim);
                DOVirtual.DelayedCall(1f, () =>
                {
                    HiddenBubble();
                    DOVirtual.DelayedCall(0.3f, ChangeMainTask);
                });
            }
            else
            {
                _finishIcon.gameObject.SetActive(false);
                if (_firstTaskShow)
                {
                    HiddenBubble();
                    _firstTaskShow = false;
                }
            }
        }

        private string _eventString = "";
        //
        //private string _oldTaskDes;
        protected override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener(_eventString, UpdateMainTaskDes);
            Messenger.RemoveListener<bool>(DailyTaskActivity.SetRedPointState,SetRedPointState);
            foreach (var dailyTaskData in _activity.TaskList)
            {
                if(dailyTaskData.IsMainTask) continue;
                Messenger.RemoveListener(dailyTaskData.Task.UpdateTaskDataMsg,UpdateRedPoint);
            }
        }

        protected override void AddListener()
        {
            base.AddListener();
            Messenger.AddListener<bool>(DailyTaskActivity.SetRedPointState,SetRedPointState);
        }
        private void SetRedPointState(bool isShow)
        {
            _redPoint.gameObject.SetActive(isShow);
        }
        private void ChangeMainTask()
        {
            _firstTaskShow = true;
            _finishIcon.gameObject.SetActive(false);
            //_eventString会在更新任务的时候变更，需要先移除旧的，在注册新的
            if (_eventString != "")
            {
                Messenger.RemoveListener(_eventString, UpdateMainTaskDes);
            }
            if (ActivityManager.Instance.GetActivityByID(WithDrawTaskActivity.ActivityId) is WithDrawTaskActivity activity)
            {
                _tipsTxt.text = activity.GetTaskInfoDesc();
                
                _mainTask = activity.Task;
                
                _eventString = _mainTask.UpdateTaskDataMsg;
                
                Messenger.AddListener(_eventString, UpdateMainTaskDes);
            }
            ShowBubble();
        }

        private void OnButtonClick()
        {
            ActivityManager.Instance.OnClickIcon(activityId);
        }

        
        public float tipShowTime = 0.3f;

        
        private void ShowBubble()
        {
            _tip.transform.DOKill();
            _tip.transform.localScale = Vector3.zero;
            _tip.transform.DOScale(new Vector3(1, 1, 1), tipShowTime).SetEase(Ease.OutBack);
        }

        private void HiddenBubble()
        {
            _tip.transform.DOKill();
            _tip.transform.localScale = Vector3.one;
            _tip.transform.DOScale(new Vector3(0, 0, 0), tipShowTime).SetEase(Ease.InBack);
        }
    }
}