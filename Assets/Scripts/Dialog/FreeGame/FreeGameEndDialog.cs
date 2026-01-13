using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Classic;
using Libs;
using TMPro;
using DG.Tweening;
using Ads;
using UnityEngine.Localization;
using Utils;

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
    private TaskTipPanel _taskTipPanel;
    protected override void Awake()
    {
        base.Awake ();
        AudioEntity.Instance.StopAllAudio();
        AudioEntity.Instance.PlayFreeGameEndDialogMusic();
        if(EndBtn != null) {UGUIEventListener.Get(this.EndBtn.gameObject).onClick = this.OnButtonClickHandler;}
        if(WatchADBtn != null) {UGUIEventListener.Get(this.WatchADBtn.gameObject).onClick = this.OnWatchADButtonClick;}
        _taskTipPanel = Utilities.RealFindObj<TaskTipPanel>(transform,"Animation/TaskTipsPanel");
        if (_taskTipPanel!=null)
        {
            _taskTipPanel.RefreshInfo(TaskConstants.CollectFreeGameTriggerCountTask_Key);
        }
        // this.bResponseBackButton = false;
        // this.AutoQuit = true;
        // this.DisplayTime = 4; 
    }

    void OnEnable()
    {
        Messenger.AddListener<int>(ADConstants.PlayFreeSpinEndAD,AdIsPlaySuccessful);
        Messenger.AddListener<int>(ADConstants.PlayFreeSpinEndADFailed,AdIsPlayFailed);
        Messenger.AddListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);
    }
    void OnDisable()
    {
        Messenger.RemoveListener<int>(ADConstants.PlayFreeSpinEndAD,AdIsPlaySuccessful);
        Messenger.RemoveListener<int>(ADConstants.PlayFreeSpinEndADFailed,AdIsPlayFailed);
        Messenger.RemoveListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);
    }

    private int RewardAdMultiple = 1;
    protected override void Start()
    {
        base.Start();
        if(!PlatformManager.Instance.IsWhiteBao())
        {
            DelayShowClaimBtn();
        }
    }

    void DelayShowClaimBtn()
    {
        if (EndBtn!=null)
        {
            EndBtn.enabled = false;
            EndBtn.gameObject.SetActive(false);
            TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(EndBtn.transform, "claim");
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"Claim");
            int rewardRate = (int)(OnLineEarningMgr.Instance.GetClaimRewardRate()*100);
            claim.text = localizedString.GetLocalizedString()+" "+rewardRate+"%";
            // EndBtn.transform.localScale = Vector3.zero;
            new DelayAction(0.7f, null, () =>
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
        //不播广告直接加钱
        if (totalCash>0)
        {
            //广播条件累计
            Messenger.Broadcast(ADConstants.CloseFreeGameEndMsg);
            //通知播放广告
            Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance, ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND);
        }
        else
        {
            this.Close();
        }
           
        // }
        SendMsg();
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
        Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance, ADEntrances.REWARD_VIDEO_ENTRANCE_FREESPINEND);
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
        else if (type == 1)
        {
            totalCash  =(int)(totalCash* OnLineEarningMgr.Instance.GetClaimRewardRate());
            DoneADCallBack();
        }
    }
    
    void AdIsPlayFailed(int type)
    {
        AdIsPlaySuccessful(type);
    }

    void HandleNotMeetConditionMsg(string msg)
    {
        if (msg == ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND)
        {
            totalCash  =(int)(totalCash* OnLineEarningMgr.Instance.GetClaimRewardRate());
            this.DoneADCallBack();
        }
    }
    
    void RewardADIsPlaySuccess()
    {
        totalCash *= RewardAdMultiple;
        //钱已经加过一次了，所以需要倍数减1
        totalCoins *= (RewardAdMultiple-1);
        DoneADCallBack();
    }
    
    //加钱动画
    private void DoneADCallBack()
    {
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
        // PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,OnLineEarningMgr.Instance.GetCashTime());
        if (showAni)
        {
            Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
                GameConstants.CollectBonusWithType, FreeGameWinCash.transform, Libs.CoinsBezier.BezierType.DailyBonus, null);
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

    public float time =1.5f;
    public void OnStart(long coins, int count,int cash)
    {
        totalCoins = coins;
        totalCash = !OnLineEarningMgr.Instance.isInfiniteOpen()?OnLineEarningMgr.Instance.GetRewardsByName(OnLineEarningConstants.REWARD_FREEGAMEEND):cash;
        if(FreeGameCount != null) 
            FreeGameCount.SetText(count.ToString());
        if (FreeGameWinCash!=null)
        {
            FreeGameWinCash.gameObject.SetActive(!PlatformManager.Instance.IsWhiteBao() && totalCash > 0);
        }
        AudioEntity.Instance.PlayRollUpEffect();
        tween = Utils.Utilities.AnimationTo (this.curCoins, coins, time, UpdateTextUI, null,()=>
        {
            AudioEntity.Instance.StopRollingUpEffect();
            tween = null;
		}).SetUpdate(true);
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            //金币滚动
            Cashtween = Utils.Utilities.AnimationTo(curCash, totalCash, time, SetCashCoins, null, () =>
            {
                SetCashCoins(totalCash);
                Cashtween = null;
            });
            RewardAdMultiple = ADManager.Instance.GetADRewardMultiple(ADEntrances.REWARD_VIDEO_ENTRANCE_SPINWIN);
            Text adMultiple = Util.FindObject<Text>(WatchADBtn.transform, "num");
            adMultiple.text = "" + RewardAdMultiple;
            TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(WatchADBtn.transform, "Claim");
            claim.text = OnLineEarningMgr.Instance.GetMoneyStr(totalCash*RewardAdMultiple,needIcon:false,needBigNum:true);
        }
        else
        {
            //白包不显示广告按钮
            Text adMultiple = Util.FindObject<Text>(WatchADBtn.transform, "num");
            adMultiple.gameObject.SetActive(false);
            TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(WatchADBtn.transform, "Claim");
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"Claim");
            claim.text = localizedString.GetLocalizedString();
            TextMeshProUGUI ad = Utilities.RealFindObj<TextMeshProUGUI>(WatchADBtn.transform, "ad");
            ad.gameObject.SetActive(false);
            TextMeshProUGUI x2 = Utilities.RealFindObj<TextMeshProUGUI>(WatchADBtn.transform, "x2");
            x2.gameObject.SetActive(false);
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
   

