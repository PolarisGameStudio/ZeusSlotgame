using DG.Tweening;
using Libs;
using UnityEngine;

namespace Core.UI
{
    public class ExtraRewardCashDialog:UIDialog
    {
        public Transform Img_cashBg;
        public UIText tmp_cashCount;
        int curCash = 0;
        int cash = 0;
        private Tween Cashtween = null;
        private float time = 0.5f;
        private float idletime = 0.5f;

        public void SetUIData(int cash)
        {
            AudioEntity.Instance.PlayRollUpEffect(0.7f);
            Cashtween = Utils.Utilities.AnimationTo(curCash, cash, time, SetCashCoins, null, () =>
            {
                AudioEntity.Instance.StopRollingUpEffect();
                SetCashCoins(cash);
                Cashtween = null;
                new DelayAction(idletime, null, () =>
                {
                    FlyCash();
                }).Play();
            });
        }

        private void SetCashCoins(int cash)
        {
            this.curCash = cash;
            this.tmp_cashCount.SetText("+"+OnLineEarningMgr.Instance.GetMoneyStr(cash,needIcon:false));
        }
        
        private void FlyCash(bool showAni = true)
        {
            //此处直接加钱
            OnLineEarningMgr.Instance.IncreaseCash(cash);
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,OnLineEarningMgr.Instance.GetCashTime());
            if (showAni)
            {
                Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
                    GameConstants.CollectBonusWithType, Img_cashBg, Libs.CoinsBezier.BezierType.DailyBonus,null);
            }
            Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            Libs.AudioEntity.Instance.PlayCoinCollectionEffect();
            new DelayAction( .8f, null, () =>
            {
                
                Libs.AudioEntity.Instance.StopCoinCollectionEffect();
                this.Close();
                Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
            }).Play();
        }
    }
}