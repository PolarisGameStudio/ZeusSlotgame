using System;
using DG.Tweening;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Classic
{
    public class RewardCashDialog : UIDialog
    {
        private TextMeshProUGUI TMP_Money;
        public Button BtnWatch;
        public Transform cashFlyPosition;

        //PopRewardState exit
        private Action endStateCallBack;
        private int cash;
        private int cashAdd;
        private int totalCash;
        private bool HasClicked = false;
        protected override void Awake()
        {
            AudioManager.Instance.AsyncPlayEffectAudio("Bonus_win");
            TMP_Money = Util.FindObject<TextMeshProUGUI>(transform, "Anchor/Animation/TMP_Money");
            BtnWatch.onClick.AddListener(OnButtonClick);
            base.Awake();
        }


        public void SetUIData(int money)
        {
            this.totalCash = money;
            CashRollUp();
            TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(BtnWatch.transform, "Claim");
            claim.text = OnLineEarningMgr.Instance.GetMoneyStr(totalCash,needIcon:false,needBigNum:true);
        }

        private Tween Cashtween = null;
        private int curCash = 0;
        public float time = 2.3f;

        void CashRollUp()
        {
            //金币滚动
            new DelayAction(0.4f, null, () =>
            {
                Cashtween = Utils.Utilities.AnimationTo(curCash, totalCash, time, SetCashCoins, null, () =>
                {
                    SetCashCoins(totalCash);
                    Cashtween = null;
                });
            }).Play();
        }

        public void OnClickStopUpdate()
        {
            AudioEntity.Instance.StopRollingUpEffect();
            if (Cashtween != null)
            {
                Cashtween.Kill(true);
                this.SetCashCoins(totalCash);
            }
        }

        private void SetCashCoins(int cash)
        {
            this.curCash = cash;
            this.TMP_Money.SetText(OnLineEarningMgr.Instance.GetMoneyStr(cash,needBigNum:true));
        }

        //激励广告
        public void OnButtonClick()
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
            
            OnClickStopUpdate();
            SetCoins();
        }

        public override void Close()
        {
            base.Close();
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
        }

        public void SetCoins()
        {
            //加钱动画播放完毕
            OnLineEarningMgr.Instance.IncreaseCash(curCash);
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,
                OnLineEarningMgr.Instance.GetCashTime());
            Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
                GameConstants.CollectBonusWithType, BtnWatch.transform, Libs.CoinsBezier.BezierType.DailyBonus, null);
            Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            Libs.AudioEntity.Instance.StopAllEffect();
            Libs.AudioEntity.Instance.PlayCoinCollectionEffect();
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
            new DelayAction(1f, null, () => { this.Close(); }).Play();
        }
    }
}