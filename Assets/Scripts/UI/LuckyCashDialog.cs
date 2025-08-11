using System;
using System.Collections.Generic;
using Classic;
using DG.Tweening;
using Libs;
using RealYou.Utility.Message;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ads;
using Utils;

public class LuckyCashDialog : UIDialog
{
    private TextMeshProUGUI TMP_Money;
    private Button BtnWatch;
    private Button BtnNotWatch;
    public Transform cashFlyPosition;

    //PopRewardState exit
    private Action endStateCallBack;
    private int cash;
    private int cashAdd;
    private int totalCash;
   

    private bool isPlayAd;
    //是否选择过按钮，要么点击关闭看全屏广告，要么点击看激励广告
    private bool HasClicked = false;
    private bool closeShowAd = false;
    private TaskTipPanel _taskTipPanel;
    protected override void Awake()
    {
        AudioManager.Instance.AsyncPlayEffectAudio("Bonus_win");
        TMP_Money = Util.FindObject<TextMeshProUGUI>(transform, "Anchor/Animation/TMP_Money");
        BtnWatch = Util.FindObject<Button>(transform, "Anchor/Animation/BtnWatch");
        BtnNotWatch= Util.FindObject<Button>(transform, "Anchor/Animation/BtnNotWatch");
        BtnWatch.onClick.AddListener(OnWatchADButtonClick);
        BtnNotWatch.onClick.AddListener(OnButtonCloseClick);
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"LuckyCash");
        _taskTipPanel = Utilities.RealFindObj<TaskTipPanel>(transform,"Anchor/Animation/TaskTipsPanel");
        if (_taskTipPanel!=null)
        {
            _taskTipPanel.RefreshInfo(TaskConstants.CollectTriggerSpinWinCountTask_Key);
        }
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        DelayShowClaimBtn();
    }

    void DelayShowClaimBtn()
    {
        if (BtnNotWatch!=null)
        {
            BtnNotWatch.enabled = false;
            BtnNotWatch.gameObject.SetActive(false);
            // BtnNotWatch.transform.localScale = Vector3.zero;
            new DelayAction(1f, null, () =>
            {
                BtnNotWatch.gameObject.SetActive(true);
                BtnNotWatch.enabled = true;
                // BtnNotWatch.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
                // {
                //     BtnNotWatch.enabled = true;
                // });
            }).Play();
        }
    }
    
    void OnEnable()
    {
        Messenger.AddListener<int>(ADConstants.PlayLuckyCashAD,AdIsPlaySuccessful);
        Messenger.AddListener<int>(ADConstants.PlayLuckyCashADFailed,AdIsPlayFailed);
    }
    void OnDisable()
    {
        Messenger.RemoveListener<int>(ADConstants.PlayLuckyCashAD,AdIsPlaySuccessful);
        Messenger.RemoveListener<int>(ADConstants.PlayLuckyCashADFailed,AdIsPlayFailed);
    }
    
    public void SetUIData(int money)
    {
        this.totalCash = money;
        CashRollUp();
    }

    private Tween Cashtween = null;
    private int curCash = 0;
    public float time = 2.3f;
    void CashRollUp()
    {
        //金币滚动
        new DelayAction(0.4f, null,() =>
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
        if (Cashtween!=null)
        {
            Cashtween.Kill(true);
            this.SetCashCoins(totalCash);
        }
    }
    
    private void SetCashCoins(int cash)
    {
        this.curCash = cash;
        this.TMP_Money.SetText(OnLineEarningMgr.Instance.GetMoneyStr(cash));
    }
    
    void AdIsPlaySuccessful(int type)
    {
        Debug.Log("LuckyCashDialog  AdIsPlaySuccessful  type========="+type);
        int multiple = 1;
        if (type == (int)ADType.RewardAD)
        {
            multiple = ADManager.Instance.GetADRewardMultiple(ADEntrances.REWARD_VIDEO_ENTRANCE_LUCKYCASH);
        }else if (type == (int)ADType.InterstitialAD)
        {
            multiple = ADManager.Instance.GetADRewardMultiple(ADEntrances.Interstitial_Entrance_CLOSELUCKYCASH);
        }
        totalCash *= multiple;
        SetCoins();
    }
    void AdIsPlayFailed(int type)
    {
        AdIsPlaySuccessful(type);
    }
    //激励广告
    public void OnWatchADButtonClick()
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
        OnClickStopUpdate();
        Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance,ADEntrances.REWARD_VIDEO_ENTRANCE_LUCKYCASH);
    }

    private void OnButtonCloseClick()
    {
        if (!BtnNotWatch.enabled)
        {
            return;
        }
        BtnNotWatch.enabled = false;
        if (HasClicked)
        {
            return;
        }
        HasClicked = true;
        OnClickStopUpdate();
        Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance,ADEntrances.Interstitial_Entrance_CLOSELUCKYCASH);
    }
    
    public void SetCoins()
    {
        //加钱动画播放完毕
        OnLineEarningMgr.Instance.IncreaseCash(totalCash);
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
            this.Close();
            Messenger.Broadcast(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL);
        }).Play();
    }
}