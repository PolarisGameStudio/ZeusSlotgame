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
using UnityEngine.Localization;
using Utils;
using System.SliderMultiplier;

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

    // 滑块倍率组件
    [Header("Slider Multiplier")]
    [SerializeField] private SliderMultiplierComponent sliderMultiplierComponent;

    // 最终倍率显示节点（盖章效果）
    [Header("Multiplier Stamp")]
    [SerializeField] private GameObject multiplierStampObject;
    [SerializeField] private Text multiplierStampText;

    private bool isPlayAd;
    //是否选择过按钮，要么点击关闭看全屏广告，要么点击看激励广告
    private bool HasClicked = false;
    private bool closeShowAd = false;
    private TaskTipPanel _taskTipPanel;
    protected override void Awake()
    {
        Messenger.Broadcast<float,float>(GameConstants.SetBackGroundAudio,0,0.3f);
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

        // 初始化倍率盖章节点（隐藏）
        if (multiplierStampObject != null)
        {
            multiplierStampObject.SetActive(false);
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
            TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(BtnNotWatch.transform, "claim");
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"Claim");
            if (!PlatformManager.Instance.IsWhiteBao())
            {
                int rewardRate = (int)(OnLineEarningMgr.Instance.GetClaimRewardRate()*100);
                claim.text = localizedString.GetLocalizedString()+" "+rewardRate+"%";
            }
            else
            {
                claim.text = localizedString.GetLocalizedString();
            }
            // BtnNotWatch.transform.localScale = Vector3.zero;
            new DelayAction(0.7f, null, () =>
            {
                BtnNotWatch.gameObject.SetActive(true);
                BtnNotWatch.enabled = true;
            }).Play();
        }
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();

        Messenger.AddListener<int>(ADConstants.PlayLuckyCashAD,AdIsPlaySuccessful);
        Messenger.AddListener<int>(ADConstants.PlayLuckyCashADFailed,AdIsPlayFailed);
        Messenger.AddListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);

        // 每次启用时初始化滑块（会自动检查提现状态）
        if (sliderMultiplierComponent != null)
        {
            sliderMultiplierComponent.OnInit();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Messenger.RemoveListener<int>(ADConstants.PlayLuckyCashAD,AdIsPlaySuccessful);
        Messenger.RemoveListener<int>(ADConstants.PlayLuckyCashADFailed,AdIsPlayFailed);
        Messenger.RemoveListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);
    }
    void HandleNotMeetConditionMsg(string msg)
    {
        if (msg == ADEntrances.Interstitial_Entrance_CLOSELUCKYCASH)
        {
            totalCash  =(int)(totalCash* OnLineEarningMgr.Instance.GetClaimRewardRate());
            SetCoins();
        }
    }
    private int RewardAdMultiple = 1;
    public void SetUIData(int money)
    {
        this.totalCash = money;
        // RewardAdMultiple = ADManager.Instance.GetADRewardMultiple(ADEntrances.REWARD_VIDEO_ENTRANCE_LUCKYCASH);
        // Text adMultiple = Util.FindObject<Text>(BtnWatch.transform, "num");
        // adMultiple.text = "" + RewardAdMultiple;
        CashRollUp();
        TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(BtnWatch.transform, "Claim");
        claim.text = OnLineEarningMgr.Instance.GetMoneyStr(totalCash*RewardAdMultiple,needIcon:false,needBigNum:true);
    }

    private Tween Cashtween = null;
    private int curCash = 0;
    public float time = 1.2f;
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
            multiple = RewardAdMultiple;
            totalCash *= multiple;

            // 广告播放成功后，推进配置索引
            if (sliderMultiplierComponent != null)
            {
                SliderMultiplierManager.Instance.AdvanceToNextConfig();
                Debug.Log("[LuckyCashDialog] Configuration advanced after successful ad play");
            }

            // 播放倍率盖章动画
            PlayMultiplierStampAnimation(multiple);
        }else if (type == (int)ADType.InterstitialAD)
        {
            totalCash  =(int)(totalCash* OnLineEarningMgr.Instance.GetClaimRewardRate());
            SetCoins();
        }
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

        // 停止滑块移动并计算倍率
        if (sliderMultiplierComponent != null)
        {
            int finalMultiplier = sliderMultiplierComponent.CalculateFinalMultiplier();
            Debug.Log($"[LuckyCashDialog] Slider final multiplier: {finalMultiplier}");

            // 直接使用滑块倍率，不再与 ADMultiple 相乘
            RewardAdMultiple = finalMultiplier;

            Debug.Log($"[LuckyCashDialog] Final RewardAdMultiple: {RewardAdMultiple}");
        }

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
        Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
            GameConstants.CollectBonusWithType, cashFlyPosition, Libs.CoinsBezier.BezierType.DailyBonus, null);
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

    /// <summary>
    /// 播放倍率盖章动画
    /// 顺序：盖章出现（从大到小）-> 金币数值滚动 -> 飞钱动画
    /// </summary>
    private void PlayMultiplierStampAnimation(int multiplier)
    {
        if (multiplierStampObject == null)
        {
            Debug.LogWarning("[LuckyCashDialog] multiplierStampObject is null, skipping stamp animation");
            // 直接播放金币滚动和飞钱动画
            PlayCashRollUpWithMultiplier();
            return;
        }

        // 1. 设置倍率文本
        if (multiplierStampText != null)
        {
            multiplierStampText.text = $"{multiplier}";
        }

        // 2. 显示对象并设置初始缩放（从大到小的盖章效果）
        multiplierStampObject.SetActive(true);
        multiplierStampObject.transform.localScale = Vector3.one * 2.0f;

        // 3. 播放盖章动画（从2.0倍缩放到1.0倍，带回弹效果）
        multiplierStampObject.transform.DOScale(Vector3.one*0.7f, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                // 盖章动画完成后，延迟一点时间开始金币滚动
                new DelayAction(0.3f, null, () =>
                {
                    // 播放金币滚动动画
                    PlayCashRollUpWithMultiplier();
                }).Play();
            });

        // 4. 播放音效（如果需要的话）
        // AudioManager.Instance.AsyncPlayEffectAudio("Stamp_Sound");
    }

    /// <summary>
    /// 播放金币滚动到最终倍率后的数值，然后播放飞钱动画
    /// </summary>
    private void PlayCashRollUpWithMultiplier()
    {
        // 停止之前的滚动动画（如果有）
        if (Cashtween != null)
        {
            Cashtween.Kill();
            Cashtween = null;
        }

        // 计算最终金额（已经在 AdIsPlaySuccessful 中计算过了）
        int finalCash = totalCash;

        // 从当前显示的金额滚动到最终金额
        Cashtween = Utils.Utilities.AnimationTo(curCash, finalCash, time, SetCashCoins, null, () =>
        {
            SetCashCoins(finalCash);
            Cashtween = null;

            // 金币滚动完成后，隐藏倍率盖章节点
            if (multiplierStampObject != null)
            {
                multiplierStampObject.SetActive(false);
            }

            // 播放飞钱动画
            SetCoins();
        });
    }
}