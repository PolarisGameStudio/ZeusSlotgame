using UnityEngine;
using UnityEngine.UI;
using Libs;

public class FreeGameStartDialog : UIDialog
{
    [Header("FreeGame开始按钮")]
    public Button StartBtn;

    [Header("FreeGame关闭按钮")] 
    Button CloseBtn;

    [Header(header: "广告按钮")] 
    Button AdStartBtn;

    string entranceName;
    protected long normalTimes;
    [Header("FreeGame次数")]
    public UIText FreeSpinCount;
    [Header("界面打开音效音量(0-1)")]
    public float open_volume = 1.0f;

    private bool isPlayAd = false;
    private bool hasClicked = false;
    protected override void Awake ()
    {
        base.Awake ();
        
        // Messenger.AddListener(ADConstants.PlayFreeSpinADDefeated, this.OnAdIsPlayDefeated);//关闭广告时调用
        AudioEntity.Instance.StopAllAudio();
        AudioEntity.Instance.PlayFreeGameStartDialogMusic(open_volume);
        // CloseBtn = transform.Find("Animation/closeBtn").GetComponent<Button>();
        // AdStartBtn = transform.Find("Animation/StartBtn").GetComponent<Button>();
        // if (AdStartBtn!=null)
        // {
        //     UGUIEventListener.Get(this.AdStartBtn.gameObject).onClick = this.OnWatchADButtonClick;
        // }
        // if (BaseSlotMachineController.Instance.slotMachineConfig.IsOpenAdFreespinCountAdd)
        // {
        //     if (!FreespinGame.isAdFreeSpinCountAdd)
        //     {
        //         if (AdStartBtn != null)AdStartBtn.gameObject.SetActive(false);
        //         if (CloseBtn!=null)CloseBtn.gameObject.SetActive(false);
        //         if(StartBtn != null) {UGUIEventListener.Get(this.StartBtn.gameObject).onClick = this.OnButtonClickHandler;
        //             return;
        //         }
        //     }
        //     else
        //     {
        //         RestoreAdsDialog();
        //         return;
        //     }
        //     
        // }
        // else
        // {
        //     if(StartBtn != null) 
        //     {
        //         UGUIEventListener.Get(this.StartBtn.gameObject).onClick = this.OnButtonClickHandler;
        //         return;
        //     }
        // }
        //
        // this.bResponseBackButton = false;
        // this.AutoQuit = true;
        // this.DisplayTime = 4;
    }

    protected override void OnEnable()
    {
        Messenger.AddListener<int>(ADConstants.PlayFreeSpinAD, this.OnAdIsPlaySuccessful);
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        Messenger.RemoveListener<int>(ADConstants.PlayFreeSpinAD, this.OnAdIsPlaySuccessful);
        base.OnDisable();
    }

    public void OnStart (long times)
    {
        normalTimes = times;
        if(FreeSpinCount != null)FreeSpinCount.SetText(times.ToString());
        DelayShowStartBtn(1.5f);
    }
    protected void DelayShowStartBtn(float delaytime)
    {
        if (StartBtn != null)
        {
            StartBtn.onClick.AddListener(this.OnStartButtonClick);
            StartBtn.gameObject.SetActive(false);
            new Libs.DelayAction(delaytime, null, () =>
            {
                if (StartBtn != null) StartBtn.gameObject.SetActive(true);
            }).Play();
        }
    }

    private void OnWatchADButtonClick(GameObject go)
    {
        if (!AdStartBtn.enabled)
        {
            return;
        }
        AdStartBtn.enabled = false;
        if (isPlayAd)
        {
            return;
        }
        isPlayAd = true;
        if (hasClicked)
        {
            return;
        }
        hasClicked = true;
        bool adIsReady = ADManager.Instance.InterstitialAdIsOk(ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART);
        //广告未加载好
        if (!adIsReady)
        {
            isPlayAd = true;
            //展示未加载好广告的提示
            ADManager.Instance.ShowLoadingADsUI(endCallBack:this.ADDoneCallBack);
        }
        else
        {
            ADManager.Instance.PlayInterstitialAd(ADEntrances.Interstitial_Entrance_CLOSEFREESPINSTART);
        }
        OnLineEarningMgr.Instance.ResetSpinTime();
    }
    protected bool adRewarded = false;
    //广告播放成功，关闭弹板进入下一个操作
    public virtual void OnAdIsPlaySuccessful(int type)
    {
        adRewarded = true;
        ADDoneCallBack();
    }
    private void OnStartButtonClick()
    {
        if (!StartBtn.interactable) return;
        StartBtn.interactable = false;
        if (hasClicked)
        {
            return;
        }
        hasClicked = true;
        AudioEntity.Instance.StopFreeGameStartDialogMusic();
        AudioEntity.Instance.PlayFeatureBtnEffect();
        Close();
    }

    public void ADDoneCallBack()
    {
        if (isPlayAd)
        {
            //广播看过广告了
            Messenger.Broadcast(BaseSlotMachineController.CALCULATE_CASH,true);
        }
        AudioEntity.Instance.StopFreeGameStartDialogMusic();
        AudioEntity.Instance.PlayFeatureBtnEffect();
        this.Close();
    }

    
}

