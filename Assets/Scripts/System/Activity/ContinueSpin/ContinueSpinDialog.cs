using System;
using DG.Tweening;
using Libs;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
namespace Activity
{
    public class ContinueSpinDialog:UIDialog
    {
        public Transform cashFlyPosition;
        private SkeletonGraphic _skeletonGraphic;
        private TextMeshProUGUI _cashCount;
        private Button _adButton;
        private Button _closeButton;
        private Button _getButton;

       
        private const string CONTINUE_SPIN_COUNT = "CONTINUE_SPIN_COUNT"; //开启次数
        private const string CONTINUE_SPIN_CLOSE = "CONTINUE_SPIN_CLOSE"; //关闭次数
        
        ContinueSpinActivity _activity;
        public void SetUIData(int activityId)
        {
            _activity = ActivityManager.Instance.GetActivityByID(activityId) as ContinueSpinActivity;
            OpenUI();
        }

        private void OpenUI()
        {
            Debug.Log("打开ui");
            if (ContinueSpinActivity.IsFirstPop)
            {
                _adButton.gameObject.SetActive(false);
                DOVirtual.DelayedCall(1f, () =>
                {
                    ShowVideoCallBack(0);
                    ContinueSpinActivity.SaveData();
                });
            }
            else
            {
                _adButton.gameObject.SetActive(true);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            _skeletonGraphic = Utilities.RealFindObj<SkeletonGraphic>(transform,"Anchor/Animation/House");
            
            _cashCount = Utilities.RealFindObj<TextMeshProUGUI>(transform,"Anchor/Animation/TMP_Money");
            
            _adButton = Utilities.RealFindObj<Button>(transform,"Anchor/Animation/AdBtn");
            
            _getButton = Utilities.RealFindObj<Button>(transform,"Anchor/Animation/GetBtn");
            
            _closeButton = Utilities.RealFindObj<Button>(transform,"Anchor/Animation/CloseBtn");
            
            _adButton.onClick.AddListener(OnClickAdBtn);
            
            _closeButton.onClick.AddListener(OnClickCloseBtn);
            
            _getButton.onClick.AddListener(OnClickGetBtn);
            
            _cashCount.gameObject.SetActive(false);
            _getButton.gameObject.SetActive(false);
            
            _cashCount.text = "0";
           
            
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Messenger.AddListener<int>(ADConstants.PlayContinueSpinAD, ShowVideoCallBack);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Messenger.RemoveListener<int>(ADConstants.PlayContinueSpinAD, ShowVideoCallBack);
        }

        private void ShowVideoCallBack(int res)
        {
            if (ContinueSpinActivity.IsCloseReward)
            {
                int count = SharedPlayerPrefs.GetPlayerPrefsIntValue(CONTINUE_SPIN_CLOSE);
                count++;
                SharedPlayerPrefs.SetPlayerPrefsIntValue(CONTINUE_SPIN_CLOSE,count);
                PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"continue_spin_close",count);
            }
            else
            {
                int count = SharedPlayerPrefs.GetPlayerPrefsIntValue(CONTINUE_SPIN_COUNT);
                count++;
                SharedPlayerPrefs.SetPlayerPrefsIntValue(CONTINUE_SPIN_COUNT,count);
                PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"continue_spin_count",count);
            }
            
            PlayAnim();
            //SetCoins();
            _adButton.gameObject.SetActive(false);
            _closeButton.gameObject.SetActive(false);
            _activity.ResetTask();
        }

        private void SetCoins()
        {
            var randomReward = _activity.GetReward();
            OnLineEarningMgr.Instance.IncreaseCash(randomReward);
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,OnLineEarningMgr.Instance.GetCashTime());
            Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action, CoinsBezier.BezierObjectType>(
                GameConstants.CollectBonusWithType, cashFlyPosition, Libs.CoinsBezier.BezierType.DailyBonus, null,
                CoinsBezier.BezierObjectType.Cash);
            Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            Libs.AudioEntity.Instance.StopAllEffect();
            Libs.AudioEntity.Instance.PlayCoinCollectionEffect();
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
            new DelayAction(0.8f,null, () =>
            {
                Close();
                Messenger.Broadcast(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL);
                _closeButton.gameObject.SetActive(true);
            }).Play();
        }
        
        private void OnClickGetBtn()
        {
            SetCoins();
        }
        private void OnClickCloseBtn()
        {
            bool rewardADIsReady = ADManager.Instance.InterstitialAdIsOk(ADEntrances.REWARD_VIDEO_CONTINUE_SPIN);
              
            if (rewardADIsReady)
            {
                ContinueSpinActivity.IsCloseReward = true;
                ADManager.Instance.PlayInterstitialAd(ADEntrances.REWARD_VIDEO_CONTINUE_SPIN);
            }
            else
            {
                //广告未加载好
                //展示未加载好广告的提示,直接给看广告成功的奖励
                _cashCount.gameObject.SetActive(false);
                _cashCount.text = "0";
                Close();
            }
        }
        private TrackEntry _trackEntry = null;

        private const float AnimTime = 0.3f;

        private void OnClickAdBtn()
        {
            bool rewardADIsReady = ADManager.Instance.RewardAdIsOk(ADEntrances.REWARD_VIDEO_CONTINUE_SPIN);
              
            if (rewardADIsReady)
            {
                ContinueSpinActivity.IsCloseReward = false;
                ADManager.Instance.PlayRewardVideo(ADEntrances.REWARD_VIDEO_CONTINUE_SPIN);
            }
            else
            {
                //广告未加载好
                //展示未加载好广告的提示,直接给看广告成功的奖励
                ADManager.Instance.ShowLoadingADsUI();
            }
        }

        private void PlayAnim()
        {
            //_skeletonGraphic.AnimationState.SetAnimation(0, "kai", false);
            //_skeletonGraphic.AnimationState.AddAnimation(0, "idle", true, 0);
            
            if (_trackEntry != null)
            {
                _trackEntry.Complete -= OnKaiAnimationComplete;
            }
            
            // 播放 "kai" 动画，只播放一次
            _trackEntry = _skeletonGraphic.AnimationState.SetAnimation(0, "kai", false);
            
            // 添加播放完成回调
            _trackEntry.Complete += OnKaiAnimationComplete;

            // 在 "kai" 播放完成后，播放循环的 "idle"
            _skeletonGraphic.AnimationState.AddAnimation(0, "idle", true, 0);
            
        }
        
        private void OnKaiAnimationComplete(TrackEntry trackEntry)
        {
            _cashCount.gameObject.SetActive(true);
            _cashCount.transform.localScale = Vector3.zero;
            _cashCount.transform.DOScale(1, AnimTime).OnComplete(
                ()=>_getButton.gameObject.SetActive(true));

            var randomReward = _activity.GetReward();
            SetCoinsNum(randomReward);
           
        }
       
        private Tweener _tweener; //DOTween动画
        private int _initNum;
        private void SetCoinsNum(int number)
        {
            if (_tweener != null)
            {
                _tweener.Kill(true);
            }

            _tweener = DOTween.To(() => _initNum, x =>_initNum = x, number, AnimTime)
                .OnUpdate(CalculateTxt);
        }
        private void CalculateTxt()
        {
            _cashCount.text = OnLineEarningMgr.Instance.GetMoneyStr(_initNum,needIcon:false);
        }
    }
}