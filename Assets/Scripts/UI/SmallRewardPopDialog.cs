using System;
using System.Collections.Generic;
using Classic;
using Libs;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class SmallRewardPopDialog : UIDialog
{
    private UIText TMP_Money;
    private int cash;
    Transform cashFlyPosition;
    private Action callBack;
    private SkeletonGraphic skeletonGraphic;

    protected override void Awake()
    {
        base.Awake();
        skeletonGraphic= Util.FindObject<SkeletonGraphic>(transform, "Anchor/spine");
        TMP_Money = Util.FindObject<UIText>(transform,"Anchor/TMP_Money");
        cashFlyPosition = Util.FindObject<Transform>(transform,"Anchor/cashFlyPosition");
    }

    protected override void Start()
    {
        base.Start();
        if (skeletonGraphic!=null)
        {
            skeletonGraphic.AnimationState.ClearTracks();
            skeletonGraphic.AnimationState.SetAnimation(0, "animation", true);
        }
    }

    public void SetUIData(int money,Action closeCallBack)
    {
        //整型传递
        this.cash = money;
        string symbol = OnLineEarningMgr.Instance.GetCashSymbol();
        double showCash = OnLineEarningMgr.Instance.ConvertMoneyToDouble(money);
        TMP_Money.SetText(string.Format(symbol+"{0}",showCash));
        callBack = closeCallBack;
    }

    public override void Refresh()
    {
        //播放飞钱动画
        if (cash>0)
        {
            OnLineEarningMgr.Instance.IncreaseCash(cash);
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,OnLineEarningMgr.Instance.GetCashTime());
            new DelayAction(0.5f, null, () =>
            {
                Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
                    GameConstants.CollectBonusWithType, cashFlyPosition, Libs.CoinsBezier.BezierType.DailyBonus, null);
                Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
                DelayAction ac = new DelayAction(2f, null,() =>
                {
                    this.Close();
                    callBack?.Invoke();
                });
                ac.Play();
            }).Play();
        }
    }
}