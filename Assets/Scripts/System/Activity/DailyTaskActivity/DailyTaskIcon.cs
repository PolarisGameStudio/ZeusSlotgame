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
        private TextMeshProUGUI _mainTaskRewadCount;
        private Image _rewardIcon;

        private bool _firstTaskShow = true;

        private BaseTask _mainTask;

        private DailyTaskActivity _activity;

        public override void OnInit(int id, Dictionary<string, object> data)
        {
            base.OnInit(id, data);
            _clickButton = GetComponent<Button>();
            _finishIcon = transform.RealFindObj<Transform>("Tips/FinishIcon");
            _tipsTxt = transform.RealFindObj<TextMeshProUGUI>("Tips/Layout/TipsTxt");
            _redPoint = transform.RealFindObj<Transform>("RedPoint");
            _tip = transform.RealFindObj<Transform>("Tips");
            _rewardIcon = transform.RealFindObj<Image>("Tips/Layout/RewardIcon");
            _mainTaskRewadCount = transform.RealFindObj<TextMeshProUGUI>("Tips/Layout/RewardIcon/RewardText");
            if (_clickButton != null)
            {
                _clickButton.onClick.AddListener(OnButtonClick);
            }
            if (ActivityManager.Instance.GetActivityByID(DailyTaskActivity.ActivityId) is DailyTaskActivity activity)
            {
                _activity = activity;
                _activity.InitMainTask();
                _redPoint.gameObject.SetActive(activity.CheckHasFinishTask());
                foreach (var dailyTaskData in activity.TaskList)
                {
                    if(dailyTaskData.IsMainTask) continue;
                    Messenger.AddListener(dailyTaskData.Task.UpdateTaskDataMsg,UpdateRedPoint);
                }
            }
            
            ChangeMainTask();
        }
        private void UpdateRedPoint()
        {
            _redPoint.gameObject.SetActive(_activity.CheckHasFinishTask());
        }
        
        private void SetCoins()
        {
            var baseAwardItem = RewardManager.Instance.CreateRewardByStr(_mainTask.RewardList);
            if (baseAwardItem[0] is CashAwardItem cashAwardItem) 
            {
                OnLineEarningMgr.Instance.IncreaseCash(cashAwardItem.count,true);
                Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
                    GameConstants.CollectBonusWithType, _rewardIcon.transform, Libs.CoinsBezier.BezierType.JShape, null);
                Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay); 
            }
            new DelayAction(0.8f,null, () =>
            {
                Messenger.Broadcast(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL);
            }).Play();
        }
  
        
        private void UpdateMainTaskDes()
        {
            if (ActivityManager.Instance.GetActivityByID(WithDrawTaskActivity.ActivityId) is not WithDrawTaskActivity withDrawTaskActivity)
                return;
            
            if (withDrawTaskActivity.ShowPanel)
            {
                ChangeMainTask();
            }
            else
            {
                if (_mainTask.IsTaskConditionOK)
                {
                    _finishIcon.gameObject.SetActive(true);
                    if (!_firstTaskShow)
                    {
                        ShowBubble();
                    }
                    DOVirtual.DelayedCall(0.3f, SetCoins);
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
                
                Action<string> ac = (info)=>
                {
                    if (_tipsTxt!=null)
                    {
                        _tipsTxt.text = info;
                    }
                    
                    if (startCor!=null)
                    {
                        StopCoroutine(startCor);
                        startCor = null;
                    }
                };
            
                startCor = StartCoroutine(activity.GetTaskInfoDescAsync(ac));
                
                _mainTask = activity.Task;

                _mainTaskRewadCount.text = activity.GetTaskAwardCountDesc();
                
                _eventString = _mainTask.UpdateTaskDataMsg;
                
                Messenger.AddListener(_eventString, UpdateMainTaskDes);

                if (activity.ShowPanel)
                {
                    _tip.gameObject.SetActive(false);
                }
                else
                {
                    ShowBubble();
                }
            }
           
        }
        private Coroutine startCor;
       
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