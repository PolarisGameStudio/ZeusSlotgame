using System;
using Classic;
using Libs;

public class ADManager
{
    public static ADManager Instance => Singleton<ADManager>.Instance;
    private Action<bool> adCB = null;
    public string requestEntranceName = string.Empty;
    public IAcbAdsCallbackHandler adsCallbackHandler = null;

    public void Init()
    {
        adsCallbackHandler = new ADCallBack();
        
    }
    
    //构造函数
    ADManager()
    {
        Messenger.AddListener(ADEntrances.Interstitial_Entrance_CLOSELUCKYCASH, OnCloseLuckyCash);
        Messenger.AddListener(ADEntrances.Interstitial_Entrance_CLOSESPINWIN, OnCloseSpinWin);
        Messenger.AddListener(ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND, OnCloseFreeSpinEnd);
        Messenger.AddListener(ADEntrances.Interstitial_Entrance_CLOSEBONUSGAMEEND, OnCloseBonusGameEnd);
        Messenger.AddListener(ADEntrances.Interstitial_Entrance_BONUSGAMESTART, OnBonusGameStart);
        Messenger.AddListener(ADEntrances.Interstitial_Entrance_CARDLOTTERY, OnCardLotteryWatchAD);
        Messenger.AddListener(ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART, OnCloseFreeSpinStart);

    }
    //析构函数
    ~ADManager()
    {
        Messenger.RemoveListener(ADEntrances.Interstitial_Entrance_CLOSELUCKYCASH, OnCloseLuckyCash);
        Messenger.RemoveListener(ADEntrances.Interstitial_Entrance_CLOSESPINWIN, OnCloseSpinWin);
        Messenger.RemoveListener(ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND, OnCloseFreeSpinEnd);
        Messenger.RemoveListener(ADEntrances.Interstitial_Entrance_CLOSEBONUSGAMEEND, OnCloseBonusGameEnd);
        Messenger.RemoveListener(ADEntrances.Interstitial_Entrance_BONUSGAMESTART, OnBonusGameStart);
        Messenger.RemoveListener(ADEntrances.Interstitial_Entrance_CARDLOTTERY, OnCardLotteryWatchAD);
        Messenger.RemoveListener(ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART, OnCloseFreeSpinStart);
    }
    
    
    #region 激励视频
    /// <summary>
    /// 根据入口判定播放广告是否OK
    /// </summary>
    /// <param name="entranceName">
    /// 广告入口名称
    /// </param>
    /// <returns> true 表示有广告，false 无广告
    /// </returns>
    public bool RewardAdIsOk(string entranceName)
    {
        return PlatformManager.Instance.IsADReady(0);
    }

    /// <summary>
    /// 播放激励视频
    /// </summary>
    /// <param name="entranceName"> 广告入口名称 </param>
    /// <param name="callBack"> 播放结果回调，参数为true表示成功 </param>
    /// <param name="para"> ES需要的额外参数，默认为空 </param>
    public void PlayRewardVideo(string entranceName, Action<bool> callBack = null)
    {
        if (string.IsNullOrEmpty(entranceName))
        {
            return;
        }
        if (callBack !=null)
        {
            adCB = callBack;
        }
        this.requestEntranceName = entranceName;
#if UNITY_EDITOR
        HandlePlayVideoResult(0);
#else
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.ShowVideo,0);
#endif
    }
    #endregion

    #region 全屏广告

    /// <summary>
    /// 根据入口判定播放广告是否OK
    /// </summary>
    /// <param name="entranceName">
    /// 广告入口名称
    /// </param>
    /// <returns> true 表示有广告，false 无广告
    /// </returns>
    public bool InterstitialAdIsOk(string entranceName)
    {
        return PlatformManager.Instance.IsADReady(1);
    }

    /// <summary>
    /// 播放全屏广告
    /// </summary>
    /// <param name="entranceName"> 广告入口名称 </param>
    /// <param name="callBack"> 播放结果回调，参数为true表示成功 </param>
    /// <param name="para"> ES需要的额外参数，默认为空 </param>
    public void PlayInterstitialAd(string entranceName, Action<bool> callBack = null)
    {
        if (string.IsNullOrEmpty(entranceName))
        {
            return;
        }

        if (callBack !=null)
        {
            adCB = callBack;
        }
      
        this.requestEntranceName = entranceName;
#if UNITY_EDITOR
        HandlePlayVideoResult(1);
        adCB?.Invoke(true);
#else
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.ShowVideo,1);
#endif
    }
    #endregion

    //根据广告入口的类型来给出奖励的倍数。目前统一为 2 倍
    public int GetADRewardMultiple(string entranceName)
    {
        return OnLineEarningMgr.Instance.GetBigRewardMultiple();
    }
    
    
    #region ADCallBack
    
    public void OnRewardVideoRewarded(string entranceName)
    {
        if(this.requestEntranceName == entranceName) adCB?.Invoke(true);
        adCB = null;
    }

    public void OnRewardVideoFailed(string entranceName)
    {
        if(this.requestEntranceName == entranceName) adCB?.Invoke(false);
        adCB = null;
    }
    #endregion

    /// <summary>
    /// 广告sdk接在Android端，根据类型来处理不同广告的结果，现在结果都是成功
    /// </summary>
    /// <param name="type">暂时无用，通过看广告的入口来判断当前是哪种类型的广告</param>
    public void HandlePlayVideoResult(int type)
    {
        Messenger.Broadcast(ADConstants.OnPlayVedioEnd);
        adsCallbackHandler.OnVideoReward(requestEntranceName);
    }
    
    #region TriggerEvent
    private void OnCloseLuckyCash()
    {
        bool adIsReady = InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSELUCKYCASH);
        //广告未加载好
        if (!adIsReady)
        {
            ShowLoadingADsUI();
            return;
        }
        PlayInterstitialAd(ADEntrances.Interstitial_Entrance_CLOSELUCKYCASH);
    }
    private void OnCloseSpinWin()
    {
        bool adIsReady = InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSESPINWIN);
        //广告未加载好
        if (!adIsReady)
        {
            ShowLoadingADsUI();
            return;
        }
        PlayInterstitialAd(ADEntrances.Interstitial_Entrance_CLOSESPINWIN);
    }
    
    private void OnCloseFreeSpinEnd()
    {
        bool adIsReady = InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND);
        //广告未加载好
        if (!adIsReady)
        {
            ShowLoadingADsUI();
            return;
        }
        PlayInterstitialAd(ADEntrances.Interstitial_Entrance_CLOSEFREESPINEND);
    }
    private void OnCloseBonusGameEnd()
    {
        bool adIsReady = InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSEBONUSGAMEEND);
        //广告未加载好
        if (!adIsReady)
        {
            ShowLoadingADsUI();
            return;
        }
        PlayInterstitialAd(ADEntrances.Interstitial_Entrance_CLOSEBONUSGAMEEND);
    }
    private void OnBonusGameStart()
    {
        bool adIsReady = InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_BONUSGAMESTART);
        //广告未加载好
        if (!adIsReady)
        {
            ShowLoadingADsUI();
            return;
        }
        PlayInterstitialAd(ADEntrances.Interstitial_Entrance_BONUSGAMESTART);
    }

    private void OnCardLotteryWatchAD()
    {
        bool adIsReady = InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CARDLOTTERY);
        //广告未加载好
        if (!adIsReady)
        {
            ShowLoadingADsUI();
            return;
        }
        PlayInterstitialAd(ADEntrances.Interstitial_Entrance_CARDLOTTERY);
    }
    
    private void OnCloseFreeSpinStart()
    {
        bool adIsReady = InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART);
        //广告未加载好
        if (!adIsReady)
        {
            ShowLoadingADsUI();
            return;
        }
        PlayInterstitialAd(ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART);
    }
    public void ShowLoadingADsUI(float duration = 1.5f,Action endCallBack = null)
    {
        //展示未加载好广告的提示
        string info = "Loading ADs";
        UIManager.ShowLoadingUI(duration,info,endCB:endCallBack);
    }
    #endregion
}
