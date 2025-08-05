using System;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ads;
namespace Classic
{
    public class GetMoreCashDialog:UIDialog
    {
        private TextMeshProUGUI TMP_Money;
        private Button BtnWatch;
        public Transform cashFlyPosition;
        private Action endStateCallBack;
        private int cash;
        private int cashAdd;
        private int initCash;
        private bool isPlayAd;
        //是否选择过按钮，要么点击关闭，要么点击看广告
        private bool HasClicked = false;
        protected override void Awake()
        {
            TMP_Money = Util.FindObject<TextMeshProUGUI>(transform, "Anchor/Animation/qiankuang/TMP_Money");
            BtnWatch = Util.FindObject<Button>(transform, "Anchor/Animation/BtnWatch");
            BtnWatch.onClick.AddListener(OnWatchADButtonClick);
            base.Awake();
        }
        void OnEnable()
        {
            Messenger.AddListener<int>(ADConstants.PlayGetMoreCashAD,AdIsPlaySuccessful);
        }
        void OnDisable()
        {
            Messenger.RemoveListener<int>(ADConstants.PlayGetMoreCashAD,AdIsPlaySuccessful);
        }
        
        void AdIsPlaySuccessful(int type)
        {
            AddCashAndFly();
        }
        
        private void AddCashAndFly()
        {
            int multiple =ADManager.Instance.GetADRewardMultiple(ADEntrances.REWARD_VIDEO_ENTRANCE_MORECASH);
            initCash *= multiple;
            SetCoins();
        }

        protected override void BtnCloseClick(GameObject closeBtnObject)
        {
            base.BtnCloseClick(closeBtnObject);
            Messenger.Broadcast(GameConstants.ExitPopRewardState);
        }
        public void SetUIData(int money)
        {
            this.initCash = money;
            this.TMP_Money.SetText(OnLineEarningMgr.Instance.GetMoneyStr(money));
        }

        private void OnWatchADButtonClick()
        {
            if (!BtnWatch.enabled)
            {
                return;
            }
            BtnWatch.enabled = false;
            if (HasClicked)
            {
                return;
            }

            HasClicked = true;
            if (isPlayAd)
            {
                return;
            }
            isPlayAd = true;
            //看广告跟弹窗关闭解绑，弹窗关闭不影响广告播放，只负责加钱操作。无论成功与失败都加钱
            // AdIsPlaySuccessful(0);
            bool rewardADIsReady = ADManager.Instance.RewardAdIsOk(ADEntrances.REWARD_VIDEO_ENTRANCE_MORECASH);
            //广告未加载好
            if (!rewardADIsReady)
            {
                //展示未加载好广告的提示,给看过广告成功的奖励
                ADManager.Instance.ShowLoadingADsUI(endCallBack:this.AddCashAndFly);
            }
            else
            {
                ADManager.Instance.PlayRewardVideo(ADEntrances.REWARD_VIDEO_ENTRANCE_MORECASH);
            }
        }
        
        public void SetCoins()
        {
            //加钱动画播放完毕
            OnLineEarningMgr.Instance.IncreaseCash(initCash);
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,OnLineEarningMgr.Instance.GetCashTime());
            Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action, CoinsBezier.BezierObjectType>(
                GameConstants.CollectBonusWithType, cashFlyPosition, Libs.CoinsBezier.BezierType.DailyBonus, null,
                CoinsBezier.BezierObjectType.Cash);
            Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            Libs.AudioEntity.Instance.StopAllEffect();
            Libs.AudioEntity.Instance.PlayCoinCollectionEffect();
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
            new DelayAction(0.8f, null, () =>
            {
                this.Close();
                Messenger.Broadcast(GameConstants.ExitPopRewardState);
            }).Play();
        }
    }
}