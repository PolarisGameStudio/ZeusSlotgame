using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Classic;
using Libs;
using TMPro;
using DG.Tweening;

public class FreeGameEndDialog : UIDialog 
{
    [Header("FreeGame结束按钮")]
    public Button EndBtn;
    [Header("FreeGame结束按钮")]
    public Button WatchADBtn;
    [Header("FreeGame次数")]
    public UIText FreeGameCount;
    [Header("FreeGame结算奖励")]
    public UIText FreeGameWinCoins;
    [Header("FreeGame金钱奖励")]
    public UIText FreeGameWinCash;
    private Tween tween = null;
    private Tween Cashtween = null;
    private long curCoins = 0;
    private long totalCoins = 0;
    private int totalCash = 0;
    private int curCash = 0;

    private bool isStop = false;
    private bool HasClicked = false;
    private bool isPlayAd = false;
    
    protected override void Awake()
    {
        base.Awake ();
        AudioEntity.Instance.StopAllAudio();
        AudioEntity.Instance.PlayFreeGameEndDialogMusic();
        if(EndBtn != null) {UGUIEventListener.Get(this.EndBtn.gameObject).onClick = this.OnButtonClickHandler;}
        if(WatchADBtn != null) {UGUIEventListener.Get(this.WatchADBtn.gameObject).onClick = this.OnWatchADButtonClick;}

        // this.bResponseBackButton = false;
        // this.AutoQuit = true;
        // this.DisplayTime = 4; 
    }

    void OnEnable()
    {
        Messenger.AddListener<int>(ADConstants.PlayFreeSpinEndAD,AdIsPlaySuccessful);
    }
    void OnDisable()
    {
        Messenger.RemoveListener<int>(ADConstants.PlayFreeSpinEndAD,AdIsPlaySuccessful);
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
    
    public override void OnButtonClickHandler(GameObject go)
    {
        if(!EndBtn.interactable) return;
        EndBtn.interactable = false;
        base.OnButtonClickHandler (go);
        if (HasClicked)
        {
            return;
        }
        HasClicked = true;
        OnClickStopUpdate();
        //
        // //只有通过 claim点击关闭的弹窗才计入插屏广告的累计次数
        // OnLineEarningMgr.Instance.AddADNum();
        // if (OnLineEarningMgr.Instance.CheckCanPopAD())
        // {
        //     //加钱之后重置计数
        //     OnLineEarningMgr.Instance.ResetADNum();
        //     bool interstitialADIsReady = ADManager.Instance.InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND);
        //     //广告未加载好
        //     if (!interstitialADIsReady)
        //     {
        //         //展示未加载好广告的提示,直接给奖励
        //         ADManager.Instance.ShowLoadingADsUI(endCallBack:this.DoneADCallBack);
        //     }
        //     else
        //     {
        //         //播放广告
        //         Messenger.Broadcast(ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND);
        //     }
        // }
        // else
        // {
            //不播广告直接加钱
            if (totalCash>0)
            {
               
                if (OnLineEarningMgr.Instance.CheckCanShowFreeStartAD())
                {
                    Debug.Log("FreeGameStartDialog OnStartButtonClick ShowAD");
                    ShowAD();
                }
                else
                {
                    DoneADCallBack();
                }
            }
            else
            {
                EndBtn.interactable = true;
                this.Close();
            }
           
        // }
        SendMsg();
    }
    
    void ShowAD()
    {
        OnLineEarningMgr.Instance.AddADNum(1);
        if (OnLineEarningMgr.Instance.CheckCanPopAD(1))
        {
            OnLineEarningMgr.Instance.ResetADNum(1);
            bool interstitialADIsReady = ADManager.Instance.InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART);
            //广告未加载好
            if (!interstitialADIsReady)
            {
                //展示未加载好广告的提示,直接给奖励
                ADManager.Instance.ShowLoadingADsUI(endCallBack:this.DoneADCallBack);
            }
            else
            {
                //播放广告
               
                ADManager.Instance.PlayInterstitialAd(ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART,DoneADCallBack);
            }
            OnLineEarningMgr.Instance.ResetSpinTime();
        }
        else
        {
            this.DoneADCallBack();
        }
    }

    private void DoneADCallBack(bool res)
    {
        DoneADCallBack();
    }
    
   private void OnWatchADButtonClick(GameObject go)
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
        
        //看广告跟弹窗关闭解绑，弹窗关闭不影响广告播放，只负责加钱操作。无论成功与失败都加钱
        bool rewardADIsReady = ADManager.Instance.RewardAdIsOk(ADEntrances.REWARD_VIDEO_ENTRANCE_FREESPINEND);
        //广告未加载好
        if (!rewardADIsReady)
        {
            //展示未加载好广告的提示，直接给看广告成功的奖励
            ADManager.Instance.ShowLoadingADsUI(endCallBack:this.RewardADIsPlaySuccess);
        }
        else
        {
            ADManager.Instance.PlayRewardVideo(ADEntrances.REWARD_VIDEO_ENTRANCE_FREESPINEND);
        }
        OnLineEarningMgr.Instance.ResetSpinTime();
        SendMsg(2);
    }
    void AdIsPlaySuccessful(int type)
    {
        //激励广告
        if (type == 0)
        {
            RewardADIsPlaySuccess();
        }
        // //插屏广告
        // else if (type == 1)
        // {
        //     DoneADCallBack();
        // }
    }

    void RewardADIsPlaySuccess()
    {
        int multiple = ADManager.Instance.GetADRewardMultiple(ADEntrances.REWARD_VIDEO_ENTRANCE_SPINWIN);
        totalCash *= multiple;
        //钱已经加过一次了，所以需要倍数减1
        totalCoins *= (multiple-1);
        DoneADCallBack();
    }
    
    //加钱动画
    private void DoneADCallBack()
    {
        EndBtn.interactable = true;
        bool needFly = true;
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            FlyCash(needFly);
        }
       
        FlyCoins(false);
        Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
        // AudioEntity.Instance.StopFreeGameEndDialogMusic();
        // AudioEntity.Instance.PlayFeatureBtnEffect();
        Libs.AudioEntity.Instance.StopAllEffect();
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            Libs.AudioEntity.Instance.PlayCoinCollectionEffect();
        }
        new DelayAction( .8f, null, () =>
        {
            this.Close();
            if (!PlatformManager.Instance.IsWhiteBao())
            {
                Messenger.Broadcast(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL);
            }
        }).Play();
    }
    private void FlyCash(bool showAni = true)
    {
        //此处直接加钱
        OnLineEarningMgr.Instance.IncreaseCash(totalCash);
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,OnLineEarningMgr.Instance.GetCashTime());
        if (showAni)
        {
            Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action, CoinsBezier.BezierObjectType>(
                GameConstants.CollectBonusWithType, FreeGameWinCash.transform, Libs.CoinsBezier.BezierType.DailyBonus, null,
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
                GameConstants.CollectBonusWithType, WatchADBtn.transform, Libs.CoinsBezier.BezierType.DailyBonus, null);
        }
        Messenger.Broadcast(SlotControllerConstants.OnBlanceChangeForDisPlay);
    }
    
    public void OnStart(long coins, int count,int cash)
    {
        totalCoins = coins;
        totalCash = cash;
        if(FreeGameCount != null) 
            FreeGameCount.SetText(count.ToString());
        if (FreeGameWinCash!=null)
        {
            FreeGameWinCash.gameObject.SetActive(!PlatformManager.Instance.IsWhiteBao() && totalCash > 0);
        }
        AudioEntity.Instance.PlayRollUpEffect();
        tween = Utils.Utilities.AnimationTo (this.curCoins, coins, 2f, UpdateTextUI, null,()=>
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
    }
    
    public void Close()
    {
        base.Close();
        Libs.AudioEntity.Instance.StopCoinCollectionEffect();
        tween?.Kill();
    }
    
    private void UpdateTextUI(long num)
    {
        this.curCoins = num;
        FreeGameWinCoins.SetText(string.Format("<sprite=0>{0}",Utils.Utilities.ThousandSeparatorNumber(curCoins)));
    }
    
    private void SetCashCoins(int cash)
    {
        this.curCash = cash;
        this.FreeGameWinCash.SetText(OnLineEarningMgr.Instance.GetMoneyStr(cash));
    }
    public void OnClickStopUpdate()
    {
        if (tween == null) return;
        isStop = true;
        tween?.Kill(true);
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
        string msgName = "FreeGame";
        msgName +=multiple.ToString();
        //发送消息给平台
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,msgName);
    }
}
   

