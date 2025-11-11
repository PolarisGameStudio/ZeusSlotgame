using System;
using System.Collections.Generic;
using Libs;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DelayAction = Libs.DelayAction;

namespace Activity
{
    public class WithDrawTaskIcon:BaseIcon
    {
        private Button clickButton;
        private Image bar;
        private TextMeshProUGUI tmp_info;
        private TextMeshProUGUI tmp_progress;
        private List<Image> icons = new List<Image>();
        private SkeletonGraphic _spine;
        public TextMeshProUGUI rewardCount;
        // private Animator _animator;
        WithDrawTaskActivity activity;
        private void Awake()
        {
            clickButton = GetComponent<Button>();
            tmp_info = Util.FindObject<TextMeshProUGUI>(transform, "tmp_info");
            tmp_progress = Util.FindObject<TextMeshProUGUI>(transform, "slider_bg/tmp_progress");
            bar = Util.FindObject<Image>(transform, "slider_bg/img_progress");
            _spine = Util.FindObject<SkeletonGraphic>(transform, "spinAni");
            if (_spine!= null)
            {
                _spine.Initialize(true);
                _spine.AnimationState.SetAnimation(0, "idle", true);
            }
            // _animator = GetComponent<Animator>();
            Transform img_icon = Util.FindObject<Transform>(transform, "img_icon");
            if (img_icon != null)
            {
                for (int i = 0; i < img_icon.childCount; i++)
                {
                    Image icon = img_icon.GetChild(i).GetComponent<Image>();
                    if (icon != null)
                    {
                        icons.Add(icon);
                    }
                }
            }
            if (clickButton!=null)
            {
                clickButton.onClick.AddListener(OnButtonClick);
            }
        }

        public override void OnInit(int Id, Dictionary<string, object> data)
        {
            base.OnInit(Id, data);
            activity =ActivityManager.Instance.GetActivityByID(activityId) as WithDrawTaskActivity;
            if (activity == null)
            {
                Debug.LogError("WithDrawTaskActivity is null, activityId: " + activityId);
                this.OnDestroy();
            }
            RefreshProgress(activity.GetProgress(),activity.GetProgressText());
            GetTaskInfoAsync();
            ShowTaskIcon();
            //新的任务显示已经切换完成，通知活动检测任务状态
            new DelayAction(1f, null, () =>
            {
                activity.CheckTaskState();
            }).Play();
        }

        private void ShowTaskIcon()
        {
            int id = activity.GetTaskIconId();
            if (id < 0)
            {
                Debug.LogError("WithDrawTaskActivity GetTaskIconId is error, activityId: " + activityId);
                return;
            }
            for (int i = 0; i < icons.Count; i++)
            {
                if (i==id)
                {
                    icons[i].gameObject.SetActive(true);
                }
                else
                {
                    icons[i].gameObject.SetActive(false);
                }
            }
        }

        private void Start()
        {
            
        }

        public override void RefreshProgress(float progress, string info = null)
        {
            bar.fillAmount = progress;
            if (tmp_progress!=null && info!=null)
            {
                tmp_progress.text = info;
            }
        }

        public void DelayChangeTask(float time = 0.6f)
        {
            if (_spine!= null)
            {
                _spine.AnimationState.Complete += OnComplete;
                _spine.AnimationState.SetAnimation(0, "show", false);
            }
            // _animator.SetTrigger("change");
            new Libs.DelayAction(time, null, ()=>
            {
                if (activity.Task == null)
                {
                    Debug.LogError("WithDrawTaskActivity Task is null, activityId: " + activityId);
                    return;
                }
                SetInfo();
                RefreshProgress(activity.GetProgress(),activity.GetProgressText());
                ShowTaskIcon();
                //新的任务显示已经切换完成，通知活动检测任务状态
                new DelayAction(0.5f, null, () =>
                {
                    activity.CheckTaskState();
                }).Play();
            }).Play();
            // //切换为 idle状态
            // new Libs.DelayAction(2f, null, ()=>
            // {
            //     // if (_animator != null)
            //     // {
            //     //     _animator.SetTrigger("idle");
            //     // }
            // }).Play();
        }

        void OnComplete(TrackEntry entry)
        {
            if (_spine != null)
            {
                if (entry.animation.name == "show")
                {
                    _spine.AnimationState.Complete -= OnComplete;
                    _spine.AnimationState.SetAnimation(0, "idle", true);
                }
            }
        }

        private Coroutine startCor;
        private void GetTaskInfoAsync()
        {
            Action<string> ac = (info)=>
            {
                if (tmp_info!=null)
                {
                    tmp_info.text = info;
                }
                if (startCor!=null)
                {
                    StopCoroutine(startCor);
                    startCor = null;
                }
            };
            
            startCor = StartCoroutine(activity.GetTaskInfoDescAsync(ac));
        }
        
        public void SetInfo()
        {
            if (activity!=null)
            {
                if (tmp_info!=null)
                {
                    tmp_info.text = activity.GetTaskInfoDesc();
                }
                if (rewardCount != null)
                {
                    rewardCount.text = activity.GetTaskAwardCountDesc();
                }
            }
        }

        void OnButtonClick()
        {
            ActivityManager.Instance.OnClickIcon(activityId);
        }

        public void PlayAwardAnim(BaseAwardItem baseAwardItem)
        {
            /*if (baseAwardItem is CashAwardItem cashAwardItem)
            {
                OnLineEarningMgr.Instance.IncreaseCash(cashAwardItem.count,true);
                Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
                    GameConstants.CollectBonusWithType, rewardCount.transform.parent, Libs.CoinsBezier.BezierType.DailyBonus, null);
                Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            }*/
            //领奖动画播放完毕
            new DelayAction(1f,null, () =>
            {
                //广播刷新任务
                activity.DelayChangeTask();
            }).Play();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            Debug.Log("SpinWithDrawItem Destroy GameObject");
        }
    }
}