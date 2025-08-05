using Libs;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Classic;
using Ads;
public class WesternTreasureMiniDialog : UIDialog
{

    [Header("LinkGame结束按钮")]
    public Button EndBtn;
    [Header("FreeGame结算奖励")]
    public UIText FreeGameWinCoins;
    [Header("Cash结算奖励")]
    public UIText BonusGameWinCash;
    [Header("看广告按钮")]
    public Button WatchADBtn;
    private Tween tween = null;
    private double curCoins = 0;
    private long totalCoins = 0;
    private bool isStop = false;
    private int totalCash = 0;
    private Tween Cashtween = null;
    private int curCash = 0;

    public Transform cashFlyPosition;
    public Transform coinFlyPosition;
    protected override void Awake()
    {
        base.Awake();
        AudioManager.Instance.StopMusicAudio("Jackpot");
        if (EndBtn != null)
        {
            EndBtn.onClick.AddListener(OnNotWatchADButtonClick); 
        }
        this.bResponseBackButton = false;
        if (WatchADBtn!=null)
        {
            WatchADBtn.onClick.AddListener(OnWatchADButtonClick);
        }
    }

    public void OnStart(double coins,int jackpotType = 0)
    {
        totalCoins = Utils.Utilities.CastValueLong(coins);
        //根据 jackpot类型获取奖励值
        totalCash = OnLineEarningMgr.Instance.GetJackpotGameWinReward(jackpotType);
        // BonusGameWinCash.gameObject.SetActive(totalCash>0);
        // if (totalCash>0)
        // {
        //     BonusGameWinCash.SetText(OnLineEarningMgr.Instance.GetMoneyStr(totalCash));
        // }
        
        AudioEntity.Instance.PlayRollUpEffect();
        tween = Utils.Utilities.AnimationTo(this.curCoins, coins, 3f, UpdateTextUI, null, () =>
        {
            AudioEntity.Instance.StopRollingUpEffect();
            tween = null;
        }).SetUpdate(true);
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            //金币滚动
            Cashtween = Utils.Utilities.AnimationTo(curCash, totalCash, 2f, SetCashCoins, null, () =>
            {
                SetCashCoins(totalCash);
                Cashtween = null;
            });
        }
        BonusGameWinCash.gameObject.SetActive(!PlatformManager.Instance.IsWhiteBao() && totalCash > 0);
    }
    
    protected override void Start()
    {
        base.Start();
        DelayShowClaimBtn();
    }
    
    void DelayShowClaimBtn()
    {
        if (EndBtn!=null)
        {
            EndBtn.enabled = false;
            EndBtn.gameObject.SetActive(false);
            // EndBtn.transform.localScale = Vector3.zero;
            new DelayAction(1f, null, () =>
            {
                EndBtn.gameObject.SetActive(true);
                EndBtn.enabled = true;
                // EndBtn.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
                // {
                //     EndBtn.enabled = true;
                // });
            }).Play();
        }
    }
    
    private void SetCashCoins(int cash)
    {
        this.curCash = cash;
        this.BonusGameWinCash.SetText(OnLineEarningMgr.Instance.GetMoneyStr(cash));
    }
    void OnEnable()
    {
        Messenger.AddListener<int>(ADConstants.PlayJackPotGameAD,AdIsPlaySuccessful);
        Messenger.AddListener<int>(ADConstants.PlayJackPotGameADFailed,AdIsPlayFailed);
        Messenger.AddListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);
    }
    void OnDisable()
    {
        Messenger.RemoveListener<int>(ADConstants.PlayJackPotGameAD,AdIsPlaySuccessful);
        Messenger.RemoveListener<int>(ADConstants.PlayJackPotGameADFailed,AdIsPlayFailed);
        Messenger.RemoveListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);

    }
    void AdIsPlaySuccessful(int type)
    {
        //激励广告
        if (type == 0)
        {
            RewardADIsPlaySuccess();
        }
        //插屏广告
        else if (type == 1)
        {
            DoneADCallBack();
        }
    }

    void AdIsPlayFailed(int type)
    {
        AdIsPlaySuccessful(type);
    }
    void HandleNotMeetConditionMsg(string msg)
    {
        if (msg == ADEntrances.Interstitial_Entrance_JACKPOTEND)
        {
            //插屏广告未满足条件，直接关闭
            DoneADCallBack();
        }
    }
    void RewardADIsPlaySuccess()
    {
        int multiple = ADManager.Instance.GetADRewardMultiple(ADEntrances.REWARD_VIDEO_ENTRANCE_JACKPOT);
        totalCash *= multiple;
        //钱已经加过一次了，所以需要倍数减1
        totalCoins *= (multiple-1);
        DoneADCallBack();
    }
    private void DoneADCallBack()
    {
        bool needFly = true;
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            //加钱动画
            FlyCash(needFly);
        }
        
        //加金币动画
        FlyCoins(false);
        Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
        Libs.AudioEntity.Instance.StopAllEffect();
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            Libs.AudioEntity.Instance.PlayCoinCollectionEffect();
        }
        new DelayAction( .8f, null, () =>
        {
            if (!PlatformManager.Instance.IsWhiteBao())
            {
                Messenger.Broadcast(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL);
            }
            this.Close();
        }).Play();
    }

    private bool isPlayAd;
    //是否选择过按钮，要么点击关闭，要么点击看广告
    private bool HasClicked = false;
    private void OnWatchADButtonClick()
    {
        if (!WatchADBtn.enabled)
        {
            return;
        }
        WatchADBtn.enabled = false;
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
        this.OnClickStopUpdate();
        Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance, ADEntrances.REWARD_VIDEO_ENTRANCE_JACKPOT);
        // bool rewardADIsReady = ADManager.Instance.RewardAdIsOk(ADEntrances.REWARD_VIDEO_ENTRANCE_BONUSGAMEWIN);
        // //广告未加载好
        // if (!rewardADIsReady)
        // {
        //     //展示未加载好广告的提示,给看过广告成功的奖励
        //     ADManager.Instance.ShowLoadingADsUI(endCallBack:this.RewardADIsPlaySuccess);
        // }
        // else
        // {
        //     ADManager.Instance.PlayRewardVideo(ADEntrances.REWARD_VIDEO_ENTRANCE_BONUSGAMEWIN);
        // }
        SendMsg(2);
    }

    private void OnNotWatchADButtonClick()
    {
        if (!EndBtn.enabled)
        {
            return;
        }
        EndBtn.enabled = false;
        if (HasClicked)
        {
            return;
        }
        HasClicked = true;
        this.OnClickStopUpdate();
        //不看广告
        Messenger.Broadcast(ADConstants.JackpotGameEndMsg);
        Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance, ADEntrances.Interstitial_Entrance_JACKPOTEND);
        SendMsg();
    }
    
    void ShowAD()
    {
        Debug.Log("[WesternTreasureReelManager] [ShowAD]");
        OnLineEarningMgr.Instance.AddADNum(2);
        if (OnLineEarningMgr.Instance.CheckCanPopAD(2))
        {
            OnLineEarningMgr.Instance.ResetADNum(2);
            OnLineEarningMgr.Instance.ResetSpinTime();
            bool interstitialADIsReady = ADManager.Instance.InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSEBONUSGAMEEND);
            //广告未加载好
            if (!interstitialADIsReady)
            {
                //展示未加载好广告的提示,直接给奖励
                ADManager.Instance.ShowLoadingADsUI();
            }
            else
            {
                //播放广告
                Messenger.Broadcast(ADEntrances.Interstitial_Entrance_CLOSEBONUSGAMEEND);
            }
        }
    }

    public void Close()
    {
        base.Close();
        tween?.Kill();
    }
    
    private void FlyCash(bool showAni = true)
    {
        //此处直接加钱
        OnLineEarningMgr.Instance.IncreaseCash(totalCash);
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,OnLineEarningMgr.Instance.GetCashTime());
        if (showAni)
        {
            Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action, CoinsBezier.BezierObjectType>(
                GameConstants.CollectBonusWithType, cashFlyPosition, Libs.CoinsBezier.BezierType.DailyBonus, null,
                CoinsBezier.BezierObjectType.Cash);
        }
        Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
    }
    
    private void FlyCoins(bool showAni=true)
    {
        //金币已经在GetResultAward()计算奖励时加过了，看完广告之后因为翻倍所以需要再加一次
        if (isPlayAd)
        {
            UserManager.GetInstance().IncreaseBalance(totalCoins);
        }
        if (showAni)
        {
            Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
                GameConstants.CollectBonusWithType, coinFlyPosition.transform, Libs.CoinsBezier.BezierType.DailyBonus, null);
        }
        Messenger.Broadcast(SlotControllerConstants.OnBlanceChangeForDisPlay);
    }
    
    private void UpdateTextUI(double num)
    {
        this.curCoins = num;
        FreeGameWinCoins.SetText(string.Format("<sprite name=\"coin\">{0}",Utils.Utilities.ThousandSeparatorNumber(curCoins)));
    }

    
    public void OnClickStopUpdate()
    {
        if (tween == null) return;
        isStop = true;
        tween.Kill(true);
        AudioEntity.Instance.StopRollingUpEffect();
        this.UpdateTextUI(totalCoins);
        if (Cashtween!=null)
        {
            Cashtween.Kill(true);
            this.SetCashCoins(totalCash);
        }
    }
    private void SendMsg(int multiple = 1)
    {
        string msgName = "JackPot";
        msgName +=multiple.ToString();
        //发送消息给平台
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,msgName);
    }
}
