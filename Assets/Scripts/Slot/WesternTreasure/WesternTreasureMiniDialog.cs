using Libs;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Classic;
using Ads;
using TMPro;
using UnityEngine.Localization;
using Utils;
using System.SliderMultiplier;

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
    private TaskTipPanel _taskTipPanel;
    public Transform cashFlyPosition;
    public Transform coinFlyPosition;
    private int RewardADMultiple = 1;

    [Header("Slider Multiplier")]
    [SerializeField] private SliderMultiplierComponent sliderMultiplierComponent;

    [Header("Multiplier Stamp")]
    [SerializeField] private GameObject multiplierStampObject;
    [SerializeField] private Text multiplierStampText;

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
        _taskTipPanel = Utilities.RealFindObj<TaskTipPanel>(transform,"Animation/TaskTipsPanel");
        if (_taskTipPanel!=null)
        {
            _taskTipPanel.RefreshInfo(TaskConstants.CollectJackpotGameCountTask_Key);
        }

        // 隐藏金币显示（保留数值逻辑）
        if (FreeGameWinCoins != null)
        {
            FreeGameWinCoins.gameObject.SetActive(false);
        }

        // 初始化倍率盖章节点（隐藏）
        if (multiplierStampObject != null)
        {
            multiplierStampObject.SetActive(false);
        }
    }

    public float time = 1.5f;

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

        // 注释掉原有的倍率显示（改为使用滑块倍率）
        // RewardADMultiple = ADManager.Instance.GetADRewardMultiple(ADEntrances.REWARD_VIDEO_ENTRANCE_JACKPOT);
        // Text adMultiple = Util.FindObject<Text>(WatchADBtn.transform, "num");
        // adMultiple.text = "" + RewardADMultiple;

        // 注释：隐藏金币显示，保留数值逻辑
        // AudioEntity.Instance.PlayRollUpEffect();
        // tween = Utils.Utilities.AnimationTo(this.curCoins, coins, time, UpdateTextUI, null, () =>
        // {
        //     AudioEntity.Instance.StopRollingUpEffect();
        //     tween = null;
        // }).SetUpdate(true);

        // 直接设置金币数值（不显示动画）
        this.curCoins = coins;

        if (!PlatformManager.Instance.IsWhiteBao())
        {
            //现金滚动
            Cashtween = Utils.Utilities.AnimationTo(curCash, totalCash, time, SetCashCoins, null, () =>
            {
                SetCashCoins(totalCash);
                Cashtween = null;
            });

            // 设置 WatchADBtn 按钮文本为 "Claim" 多语言
            TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(WatchADBtn.transform, "Claim");
            if (claim != null)
            {
                LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName, "Claim");
                claim.text = localizedString.GetLocalizedString();
            }
        }
        else
        {
            // 白包不显示广告相关元素
            Text adMultiple = Util.FindObject<Text>(WatchADBtn.transform, "num");
            if (adMultiple != null) adMultiple.gameObject.SetActive(false);

            TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(WatchADBtn.transform, "Claim");
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"Claim");
            claim.text = localizedString.GetLocalizedString();
            TextMeshProUGUI ad = Utilities.RealFindObj<TextMeshProUGUI>(WatchADBtn.transform, "ad");
            if (ad != null) ad.gameObject.SetActive(false);
            TextMeshProUGUI x2 = Utilities.RealFindObj<TextMeshProUGUI>(WatchADBtn.transform, "x2");
            if (x2 != null) x2.gameObject.SetActive(false);
        }
        BonusGameWinCash.gameObject.SetActive(!PlatformManager.Instance.IsWhiteBao() && totalCash > 0);
    }
    
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

        // 每次启用时初始化滑块（会自动检查提现状态）
        if (sliderMultiplierComponent != null)
        {
            sliderMultiplierComponent.OnInit();
        }
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

            // 广告播放成功后，推进配置索引（只针对激励广告）
            if (sliderMultiplierComponent != null)
            {
                SliderMultiplierManager.Instance.AdvanceToNextConfig();
                Debug.Log("[WesternTreasureMiniDialog] Configuration advanced after successful reward ad");
            }

            // 播放倍率盖章动画（只针对现金，金币不显示）
            PlayMultiplierStampAnimation(RewardADMultiple);
        }
        //插屏广告（不推进配置索引，不使用滑块倍率）
        else if (type == 1)
        {
            totalCash = (int)(totalCash * OnLineEarningMgr.Instance.GetClaimRewardRate());
            DoneADCallBack();

            Debug.Log("[WesternTreasureMiniDialog] Interstitial ad completed, no config advance");
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
            totalCash  =(int)(totalCash* OnLineEarningMgr.Instance.GetClaimRewardRate());
            //插屏广告未满足条件，直接关闭
            DoneADCallBack();
        }
    }
    void RewardADIsPlaySuccess()
    {
        totalCash *= RewardADMultiple;

        // 金币倍率逻辑保留（不显示，但数值要对）
        // 钱已经加过一次了，所以需要倍数减1
        totalCoins *= (RewardADMultiple - 1);

        // 不调用 DoneADCallBack，改为在盖章动画完成后调用
        // DoneADCallBack();
    }
    private void DoneADCallBack()
    {
        bool needFly = true;
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            //加钱动画
            FlyCash(needFly);
        }

        // 注释：金币不显示飞行动画，改为无动画发放
        // FlyCoins(false);
        FlyCoinsWithoutAnimation();

        Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
        Libs.AudioEntity.Instance.StopAllEffect();
        if (!PlatformManager.Instance.IsWhiteBao())
        {
            Libs.AudioEntity.Instance.PlayCoinCollectionEffect();
        }
        new DelayAction(.8f, null, () =>
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

        // 停止滑块移动并计算倍率
        if (sliderMultiplierComponent != null)
        {
            int finalMultiplier = sliderMultiplierComponent.CalculateFinalMultiplier();
            Debug.Log($"[WesternTreasureMiniDialog] Slider final multiplier: {finalMultiplier}");

            // 使用滑块倍率
            RewardADMultiple = finalMultiplier;

            Debug.Log($"[WesternTreasureMiniDialog] Final RewardADMultiple: {RewardADMultiple}");
        }

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
        // SendMsg(2);
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

        // 停止滑块移动（EndBtn 不使用滑块倍率）
        if (sliderMultiplierComponent != null)
        {
            sliderMultiplierComponent.PauseMovement();
            Debug.Log("[WesternTreasureMiniDialog] Slider paused when EndBtn clicked");
        }

        //不看广告
        Messenger.Broadcast(ADConstants.JackpotGameEndMsg);
        Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance, ADEntrances.Interstitial_Entrance_JACKPOTEND);
        // SendMsg();
    }

    public void Close()
    {
        base.Close();
        tween?.Kill();

        // 清理滑块状态
        if (sliderMultiplierComponent != null)
        {
            sliderMultiplierComponent.PauseMovement();
        }
    }
    
    private void FlyCash(bool showAni = true)
    {
        //此处直接加钱
        OnLineEarningMgr.Instance.IncreaseCash(totalCash);
        // PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,OnLineEarningMgr.Instance.GetCashTime());
        if (showAni)
        {
            Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
                GameConstants.CollectBonusWithType, cashFlyPosition, Libs.CoinsBezier.BezierType.DailyBonus, null);
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
    
    // 注释：金币不再显示
    // private void UpdateTextUI(double num)
    // {
    //     this.curCoins = num;
    //     FreeGameWinCoins.SetText(string.Format("<sprite name=\"coin\">{0}",Utils.Utilities.ThousandSeparatorNumber(curCoins)));
    // }

    public void OnClickStopUpdate()
    {
        // 注释：金币不再显示滚动动画
        // if (tween == null) return;
        // isStop = true;
        // tween.Kill(true);
        // AudioEntity.Instance.StopRollingUpEffect();
        // this.UpdateTextUI(totalCoins);

        // 保留现金的停止滚动
        if (Cashtween != null)
        {
            Cashtween.Kill(true);
            this.SetCashCoins(totalCash);
        }
    }
    // private void SendMsg(int multiple = 1)
    // {
    //     string msgName = "JackPot";
    //     msgName +=multiple.ToString();
    //     //发送消息给平台
    //     PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,msgName);
    // }

    /// <summary>
    /// 发放金币奖励（不显示动画）
    /// </summary>
    private void FlyCoinsWithoutAnimation()
    {
        // 金币已经在GetResultAward()计算奖励时加过了，看完广告之后因为翻倍所以需要再加一次
        if (isPlayAd)
        {
            UserManager.GetInstance().IncreaseBalance(totalCoins);
        }
        // 不播放飞行动画，只发放奖励和更新显示
        Messenger.Broadcast(SlotControllerConstants.OnBlanceChangeForDisPlay);
    }

    /// <summary>
    /// 播放倍率盖章动画（只针对现金）
    /// 顺序：盖章出现（从大到小）-> 现金数值滚动 -> 飞钱动画
    /// </summary>
    private void PlayMultiplierStampAnimation(int multiplier)
    {
        if (multiplierStampObject == null)
        {
            Debug.LogWarning("[WesternTreasureMiniDialog] multiplierStampObject is null, skipping stamp animation");
            PlayCashRollUpWithMultiplier();
            return;
        }

        // 1. 设置倍率文本
        if (multiplierStampText != null)
        {
            multiplierStampText.text = $"{multiplier}";
        }

        // 2. 显示对象并设置初始缩放
        multiplierStampObject.SetActive(true);
        multiplierStampObject.transform.localScale = Vector3.one * 2.0f;

        // 3. 播放盖章动画
        multiplierStampObject.transform.DOScale(Vector3.one * 0.7f, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                new DelayAction(0.3f, null, () =>
                {
                    PlayCashRollUpWithMultiplier();
                }).Play();
            });
    }

    /// <summary>
    /// 播放现金滚动到最终倍率后的数值，然后播放飞钱动画
    /// </summary>
    private void PlayCashRollUpWithMultiplier()
    {
        // 停止之前的滚动动画
        if (Cashtween != null)
        {
            Cashtween.Kill();
            Cashtween = null;
        }

        // 计算最终金额（已经在 RewardADIsPlaySuccess 中计算过了）
        int finalCash = totalCash;

        // 从当前显示的金额滚动到最终金额
        Cashtween = Utils.Utilities.AnimationTo(curCash, finalCash, time, SetCashCoins, null, () =>
        {
            SetCashCoins(finalCash);
            Cashtween = null;

            // 隐藏倍率盖章节点
            if (multiplierStampObject != null)
            {
                multiplierStampObject.SetActive(false);
            }

            // 播放飞钱动画
            DoneADCallBack();
        });
    }
}
