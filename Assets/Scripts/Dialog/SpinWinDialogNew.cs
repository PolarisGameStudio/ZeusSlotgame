using System.Collections.Generic;
using Classic;
using DG.Tweening;
using Libs;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;


public class SpinWinDialogNew : UIDialog
{
    [Header("SpinWinDialog配置")] public List<Sprite> winType;
    public List<Image> winImage;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI cashText;
    public SkeletonGraphic skeleton;
    private Tween tween = null;
    private Tween Cashtween = null;

    private long curCoins = 0;
    [Header("看广告的按钮")]
    public Button WatchAdBtn;
    [Header("有广告时的关闭按钮")]
    public Button CloseBtnOnAd;
    [Header("点击停止动画按钮")]
    public Button StopBtn;
    [Header("无广告时领奖按钮")]
    public Button CollectBtn;
    private long totalCoins = 0;
    private int curCash = 0;
    private int totalCash = 0;
    private SpinWinType spinWinType = SpinWinType.NONE;
    public float time = 2.3f;
    private bool m_HasClosed = false;
    bool isplayAds;
    private bool isCollect;
    private bool isPlayAd;
    //是否选择过按钮，要么点击关闭，要么点击看广告
    private bool HasClicked = false;
    //-1 不播广告，0播激励广告，1播插屏广告
    private int showAdType = -1;
    private SkeletonDataAsset skeletonDataAsset = null;

    public Transform cashFlyPosition;
    public Transform coinFlyPosition;
    private bool isFirstTime = true;
    private bool isSecondTime = false;
    protected override void Awake()
    {
        base.Awake();
        UGUIEventListener.Get(this.CloseBtnOnAd.gameObject).onClick = this.OnNotWatchADButtonClick;
        UGUIEventListener.Get(this.WatchAdBtn.gameObject).onClick = this.OnWatchADButtonClick;
        UGUIEventListener.Get(this.StopBtn.gameObject).onClick = this.OnButtonClickHandler;
        UGUIEventListener.Get(this.CollectBtn.gameObject).onClick = this.OnCollectBtnClick;
    }
    void OnEnable()
    {
        Messenger.AddListener<int>(ADConstants.PlaySpinWinAD,AdIsPlaySuccessful);
    }
    void OnDisable()
    {
        Messenger.RemoveListener<int>(ADConstants.PlaySpinWinAD,AdIsPlaySuccessful);
    }

    public void OnStart(long coins, SpinWinType type)
    {
        if (coins <= 0)
        {
            this.Close();
            return;
        }

        spinWinType = type;
        //设置文本
        totalCoins = coins;
        
        // SetCashCoins(totalCash);
        // UpdateTextUI(totalCoins);
        //设置图片
        // winImage[0].sprite = winType[(int)type];
        // foreach (var item in winImage) item.SetNativeSize();
        //播放spine动画
        PlaySpineShowAni();
        this.PlayWinTypeEffect();
        AudioEntity.Instance.PlayRollUpEffect(0.7f);
        time *= ((int)spinWinType+1);
        //bool haveAd = false;
        //金币滚动
        new DelayAction(0.4f, null,() =>
        {
            tween = Utils.Utilities.AnimationTo(this.curCoins, coins, time, UpdateTextUI, null, () =>
            {
                AudioEntity.Instance.StopRollingUpEffect();
                UpdateTextUI(coins);
                tween = null;
            });
        }).Play();
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            totalCash = OnLineEarningMgr.Instance.GetSpinWinReward((int)spinWinType);
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
        cashText.gameObject.SetActive(!PlatformManager.Instance.IsWhiteBao());
        int popCount = OnLineEarningMgr.Instance.AddPopSpinWinCount();
        isFirstTime = popCount==1;
        isSecondTime = popCount==2;
        if (isFirstTime)
        {
            Debug.Log("SpinWinDialogNew OnStart isFirstTime");
            //第一次免费，不显示广告按钮
            WatchAdBtn.gameObject.SetActive(false);
            CloseBtnOnAd.gameObject.SetActive(false);
            //显示免费的收集按钮
            CollectBtn.gameObject.SetActive(true);
        }else if (isSecondTime)
        {
            Debug.Log("SpinWinDialogNew OnStart isSecondTime");
            //第一次免费，不显示广告按钮
            WatchAdBtn.gameObject.SetActive(false);
            CloseBtnOnAd.gameObject.SetActive(false);
            //显示免费的收集按钮
            CollectBtn.gameObject.SetActive(true);
            Transform x2Image = Utilities.RealFindObj<Transform>(CollectBtn.transform, "x2");
            x2Image.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("SpinWinDialogNew OnStart isThirdTime");
            //第三次之后有广告和插屏
            WatchAdBtn.gameObject.SetActive(true);
            //显示免费的收集按钮
            CollectBtn.gameObject.SetActive(false);
            DelayShowCloseOnAdBtn();
        }
    }
    void DelayShowCloseOnAdBtn()
    {
        if (CloseBtnOnAd!=null)
        {
            Debug.Log("Scale animation complete111");
            CloseBtnOnAd.enabled = false;
            CloseBtnOnAd.gameObject.SetActive(false);
            // CloseBtnOnAd.transform.localScale = Vector3.zero;
            new DelayAction(1f, null, () =>
            {
                CloseBtnOnAd.gameObject.SetActive(true); 
                CloseBtnOnAd.enabled = true;
                //此段代码出现 bug,点击按钮后会缩回去
                // Tween tween = CloseBtnOnAd.transform.DOScale(Vector3.one, 0.3f).OnComplete(() =>
                // {
                //     CloseBtnOnAd.enabled = true;
                //     CloseBtnOnAd.transform.localScale = Vector3.one;
                //     Debug.Log("Scale animation complete");
                //     tween = null;
                // });
            }).Play();
        }
    }
    void PlaySpineShowAni()
    {
        if (skeleton!=null)
        {
            int id = 1;
            skeleton.Skeleton.SetToSetupPose();
            skeleton.AnimationState.ClearTracks();
            switch (spinWinType)
            {
                case SpinWinType.EPIC:
                    id = 5;
                    break;
                case SpinWinType.MEGA:
                    id = 3;
                    break;
                case SpinWinType.BIG:
                    id = 1;
                    break;
            }

            skeleton.AnimationState.Complete += OnComplete;
            skeleton.AnimationState.SetAnimation(0, GetAniName(id), false);
        }
    }
    
    void OnComplete(Spine.TrackEntry entry)
    {
        if (skeleton!=null)
        {
            if ((int)spinWinType==0)
            {
                skeleton.AnimationState.SetAnimation(0, GetAniName(2), true);
            }
            else if ((int)spinWinType == 1)
            {
                skeleton.AnimationState.SetAnimation(0, GetAniName(4), true);
            }
            else if ((int)spinWinType == 2)
            {
                skeleton.AnimationState.SetAnimation(0, GetAniName(6), true);
            }
        }
    }
    
    
    private string GetAniName(int animationId)
    {
        string name = "";
        if (animationId==1)
        {
            name= "bigwinshow";
        }
        else if (animationId==2)
        {
            name= "bigwinidle"; 
        }
        else if (animationId==3)
        {
            name= "megawinshow";
        }else if (animationId==4)
        {
            name= "megawinidle";
        }else if (animationId==5)
        {
            name= "epicwinshow";
        }else if (animationId==6)
        {
            name= "epicwinidle";
        }

        return name;
    }
    
    private void OnWatchADButtonClick(GameObject go)
    {
        if (!WatchAdBtn.enabled)
        {
            return;
        }
        WatchAdBtn.enabled = false;
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
        //看广告跟弹窗关闭解绑，弹窗关闭不影响广告播放，只负责加钱操作。无论成功与失败都加钱
        // AdIsPlaySuccessful(0);
        bool rewardADIsReady = ADManager.Instance.RewardAdIsOk(ADEntrances.REWARD_VIDEO_ENTRANCE_SPINWIN);
        //广告未加载好
        if (!rewardADIsReady)
        {
            //展示未加载好广告的提示，直接给看广告成功的奖励
            ADManager.Instance.ShowLoadingADsUI(endCallBack:this.RewardADIsPlaySuccess);
        }
        else
        {
            //观看激励视频后，累计插屏的计数进度重置
            OnLineEarningMgr.Instance.ResetADNum();
            OnLineEarningMgr.Instance.ResetSpinTime();
            ADManager.Instance.PlayRewardVideo(ADEntrances.REWARD_VIDEO_ENTRANCE_SPINWIN);
        }
        SendMsg(spinWinType,2);
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
            DoneADCallBack();
        }
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
        bool needFly = true;
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            FlyCash(needFly);
        }
        FlyCoins(false);
        Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
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
    
    //点击了关闭按钮
    private void OnNotWatchADButtonClick(GameObject go)
    {
        if (!CloseBtnOnAd.enabled)
        {
            return;
        }
        CloseBtnOnAd.enabled = false;
        if (HasClicked)
        {
            return;
        }
        HasClicked = true;
        OnClickStopUpdate();
        // DoneADCallBack();
        //只有通过 claim点击关闭的弹窗才计入插屏广告的累计次数
        OnLineEarningMgr.Instance.AddADNum();
        if (OnLineEarningMgr.Instance.CheckCanPopAD())
        {
            //加钱之后重置计数
            OnLineEarningMgr.Instance.ResetADNum();
            OnLineEarningMgr.Instance.ResetSpinTime();
            bool interstitialADIsReady = ADManager.Instance.InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSESPINWIN);
            //广告未加载好
            if (!interstitialADIsReady)
            {
                //展示未加载好广告的提示,直接给奖励
                ADManager.Instance.ShowLoadingADsUI(endCallBack:this.DoneADCallBack);
            }
            else
            {
                //播放广告
                Messenger.Broadcast(ADEntrances.Interstitial_Entrance_CLOSESPINWIN);
            }
        }
        else
        {
            //不播广告直接加钱
            DoneADCallBack();
        }
        SendMsg(spinWinType);
    }

    public void OnCollectBtnClick(GameObject go)
    {
        if (!CollectBtn.enabled)
        {
            return;
        }
        CollectBtn.enabled = false;
        if (HasClicked)
        {
            return;
        }
        HasClicked = true;
        OnClickStopUpdate();
        //增加插屏广告的检测条件
        OnLineEarningMgr.Instance.AddADNum();
        if (isSecondTime)
        {
            //奖励乘以倍数
            RewardADIsPlaySuccess();
        }else if (isFirstTime)
        {
            //免费领奖不翻倍
            DoneADCallBack();
        }
    }
    
    //领奖的按钮点击，nothanks 按钮点击
    public override void OnButtonClickHandler(GameObject go)
    {
        base.OnButtonClickHandler(go);
        this.OnClickStopUpdate();
    }

    private void UpdateTextUI(long num)
    {
        this.curCoins = num;
        winText.SetText(string.Format("<sprite name=\"coin\">{0}",Utils.Utilities.ThousandSeparatorNumber(curCoins)));
    }
    private void SetCashCoins(int cash)
    {
        this.curCash = cash;
        this.cashText.SetText(OnLineEarningMgr.Instance.GetMoneyStr(cash));
    }
    
    
    private void PlayWinTypeEffect()
    {
        // AudioEntity.Instance.PlayEffect("big_win");
        switch (spinWinType)
        {
            case SpinWinType.EPIC:
                AudioEntity.Instance.PlayEffect("epic_win");
                break;
            case SpinWinType.MEGA:
                AudioEntity.Instance.PlayEffect("mega_win");
                break;
            case SpinWinType.BIG:
                AudioEntity.Instance.PlayEffect("big_win");
                break;
        }
    }
    public void Close()
    {
        base.Close();
        AudioEntity.Instance.StopRollingUpEffect();
        tween?.Kill();
        Cashtween?.Kill();
    }
    
    public void OnClickStopUpdate()
    {
        if (tween == null) return;
        tween.Kill(true);
        AudioEntity.Instance.StopRollingUpEffect();
        this.UpdateTextUI(totalCoins);
        if (Cashtween!=null)
        {
            Cashtween.Kill(true);
            this.SetCashCoins(totalCash);
        }

        if (skeleton!=null)
        {
            skeleton.AnimationState.ClearTracks();
            if ((int)spinWinType ==0)
            {
                skeleton.AnimationState.SetAnimation(0, GetAniName(2), true);
            }else if ((int)spinWinType ==1)
            {
                skeleton.AnimationState.SetAnimation(0, GetAniName(4), true);
            }else if ((int)spinWinType ==2)
            {
                skeleton.AnimationState.SetAnimation(0, GetAniName(6), true);
            }
        }
    }
    public void SendMsg(SpinWinType type,int multiple = 1)
    {
        string msgName = "";
        if (type == SpinWinType.BIG)
        {
            msgName = "BigWin";
        }
        else if (type == SpinWinType.MEGA)
        {
            msgName = "MegaWin";
        }
        else if (type == SpinWinType.EPIC)
        {
            msgName = "EpicWin";
        }
        msgName +=multiple.ToString();
        //发送消息给平台
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,msgName);
    }
}