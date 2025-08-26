using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Core;
using DG.Tweening;
using Libs;
using Classic;
using UnityEngine.UI;
using SlotFramework.AutoQuest;

public class SlotBottomPanel : MonoBehaviour 
{
    [SerializeField]
    private UIText win;
    [SerializeField]
    private UIText cash;
    [SerializeField]
    private GameObject WinEffect;

    [SerializeField]
    private TextMeshProUGUI bet;
    [SerializeField]
    private TextMeshProUGUI bet_300;
   
    [SerializeField]
    private Button SubBetButton;

    [SerializeField]
    private Button AddBetButton;

    [SerializeField]
    private Button MaxBetButton;

    [SerializeField]
    private Button FastSpinButton;

	[SerializeField]
	private Button PayTableButton;

    [SerializeField]
    public SpinButtonStyle BtnStyle;
    
    [SerializeField]
    public Image BgImage;
    
    [SerializeField]
    public Sprite BgSpriteNormal;
    
    [SerializeField]
    public Sprite BgSpriteCash;
    
    public GameObject disableSpin;

    public GameObject BetPanel;
    public GameObject BetPanel_300;

    public CoinsPanelBottom CoinsPanelBottom;
    
    public GameObject AverageBet;
    public Animator spinNumPanel;
    public CanvasGroup RewardSpin;

    private SpinButtonStyle.SpinButtonState lastState;

    private List<bool> betMiuns = new List<bool>();

    private List<bool> betPlus = new List<bool>();

    public static SlotBottomPanel Instance;
    
    public void PlayMinusBet()
    {
        for (int i = 6; i >= 0; i--)
        {
            if(betMiuns[i])
            {
                if(i == 0)
                {
                    AudioManager.Instance.AsyncPlayEffectAudio("bet_sound_6");
                    betMiuns[i] = false;
                    betMiuns[6] = true;

                    return;
                }

                AudioManager.Instance.AsyncPlayEffectAudio("bet_sound_" + (i-1).ToString());
                betMiuns[i] = false;
                betMiuns[i-1] = true;


                return;
            }
        }
 
        AudioManager.Instance.AsyncPlayEffectAudio("bet_sound_6");
        betMiuns[6] = true;
    }

    public void PlayPlusBet()      
    {
        for (int i = 0; i < 7; i++)
        {
            if(betPlus[i])
            {
                if(i == 6)
                {
                    AudioManager.Instance.AsyncPlayEffectAudio("bet_sound_0");
                    betPlus[i] = false;
                    betPlus[0] = true;
                    return;
                }

                AudioManager.Instance.AsyncPlayEffectAudio("bet_sound_" + (i+1).ToString());
                betPlus[i] = false;
                betPlus[i+1] = true;

                return;
            }
        }

        AudioManager.Instance.AsyncPlayEffectAudio("bet_sound_0");
        betPlus[0] = true;
    }



    private long lastBalance = -1;

    void Awake()
    {
        Instance = this;
        Messenger.AddListener(GameConstants.OnSceneInit, Init);
        
        Messenger.AddListener<long> (SlotControllerConstants.OnBetChange, SetBetText);
        Messenger.AddListener(SlotControllerConstants.OnBetRefresh, RefreshBetUI);
        Messenger.AddListener<bool> (SlotControllerConstants.OnAverageBet, OnAverageBet);
        Messenger.AddListener<bool> (SlotControllerConstants.RefreshCashState, RefreshCashState);
        Messenger.AddListener<float,long,long> (SlotControllerConstants.OnChangeWinText, OnWinAnimation);
        Messenger.AddListener<long>(SlotControllerConstants.OnChangeWinTextSilence, SetWinSilenceText);
        Messenger.AddListener<float,int,int> (SlotControllerConstants.OnChangeCashText, OnWinCashAnimation);
        Messenger.AddListener<int>(SlotControllerConstants.OnChangeCashTextSilence, SetWinCashSilenceText);


        Messenger.AddListener (SlotControllerConstants.OnEnterFreespin, OnEnterFreespin);
		Messenger.AddListener (SlotControllerConstants.OnQuitFreespin, OnQuitFreespin);
		Messenger.AddListener<int,int,int> (SlotControllerConstants.OnFreespinTimesChanged,OnFreespinTimeChange);
        
        Messenger.AddListener<bool> (SlotControllerConstants.ActiveButtons, ActiveButtons);
        Messenger.AddListener<bool> (SlotControllerConstants.DisactiveButtons, DisactiveButtons);

        Messenger.AddListener<bool> (SlotControllerConstants.DisableSpinButton, DisableSpinButton);
        
        Messenger.AddListener(SlotControllerConstants.DisableBetButtons,DisableBetBtns);
        Messenger.AddListener(SlotControllerConstants.EnableBetButtons,EnableBetBtns);
        Messenger.AddListener(SlotControllerConstants.OnBlanceChangeForDisPlay, BalanceChange);


        for (int i = 0; i < 7; i++)
        {
            betMiuns.Add(false);
            betPlus.Add(false);
        }
    }

    void Start()
    {
        SubBetButton.onClick.AddListener(OnSubBet);

        AddBetButton.onClick.AddListener(OnAddBet);

        MaxBetButton.onClick.AddListener(OnMaxBet);

        PayTableButton.onClick.AddListener(OpenPayTable);

        FastSpinButton.onClick.AddListener(OnFastSpin);
    }

    public  Vector3  GetButtomTextPosition()
    {
        return win.transform.position;
    }

    void OnDestroy() 
    {
        Instance = null;
        Messenger.RemoveListener (GameConstants.OnSceneInit, Init);

        Messenger.RemoveListener<long> (SlotControllerConstants.OnBetChange, SetBetText);
        Messenger.RemoveListener (SlotControllerConstants.OnBetRefresh, RefreshBetUI);
        Messenger.RemoveListener<bool> (SlotControllerConstants.RefreshCashState, RefreshCashState);

        Messenger.RemoveListener<bool> (SlotControllerConstants.OnAverageBet, OnAverageBet);
        Messenger.RemoveListener<float,long,long> (SlotControllerConstants.OnChangeWinText, OnWinAnimation);
        Messenger.RemoveListener<long>(SlotControllerConstants.OnChangeWinTextSilence, SetWinSilenceText);
        
        Messenger.RemoveListener<float,int,int> (SlotControllerConstants.OnChangeCashText, OnWinCashAnimation);
        Messenger.RemoveListener<int>(SlotControllerConstants.OnChangeCashTextSilence, SetWinCashSilenceText);
        
        Messenger.RemoveListener (SlotControllerConstants.OnEnterFreespin, OnEnterFreespin);
		Messenger.RemoveListener (SlotControllerConstants.OnQuitFreespin, OnQuitFreespin);
		Messenger.RemoveListener<int,int,int> (SlotControllerConstants.OnFreespinTimesChanged,OnFreespinTimeChange);
        
        Messenger.RemoveListener<bool> (SlotControllerConstants.ActiveButtons, ActiveButtons);
        Messenger.RemoveListener<bool> (SlotControllerConstants.DisactiveButtons, DisactiveButtons);

        Messenger.RemoveListener<bool> (SlotControllerConstants.DisableSpinButton, DisableSpinButton);
		
        Messenger.RemoveListener(SlotControllerConstants.DisableBetButtons,DisableBetBtns);
        Messenger.RemoveListener(SlotControllerConstants.EnableBetButtons,EnableBetBtns);
        Messenger.RemoveListener(SlotControllerConstants.OnBlanceChangeForDisPlay, BalanceChange);
        
    }

    public void Init()
    {
		bet.text = Utils.Utilities.ThousandSeparatorNumber (BaseSlotMachineController.Instance.currentBetting); 
        bet_300.text = Utils.Utilities.ThousandSeparatorNumber (BaseSlotMachineController.Instance.currentBetting);
        win.SetText("0");
        RefreshCashState(false);
        BetPanel.SetActive(OnLineEarningMgr.Instance.isInfiniteOpen());
        if (BetPanel_300!=null)
        {
            BetPanel_300.SetActive(!OnLineEarningMgr.Instance.isInfiniteOpen());
        }
        if (!OnLineEarningMgr.Instance.isInfiniteOpen())
        {
            if (CoinsPanelBottom!=null)
            {
                BaseGameConsole.singletonInstance.SlotMachineController.CoinsTransform = CoinsPanelBottom.coinTarget;
                BalanceChange();
            }
        }
        // MaxBetShow();
    }

	private void SetBetText(long value)
    {
		bet.text = Utils.Utilities.ThousandSeparatorNumber (value);
        if (bet_300!=null)
        {
            bet_300.text = Utils.Utilities.ThousandSeparatorNumber (value);
        }
        MaxBetShow();
    }

    void BalanceChange()
    {
        lastBalance = UserManager.GetInstance().UserProfile().Balance();
        CoinsPanelBottom.SetCoinsNumber((lastBalance));
    }
    private void RefreshBetUI()
    {
        MaxBetShow();
    }
    private void OnAverageBet(bool state)
    {
        if(state)
        {
            if(BetPanel!=null) BetPanel.SetActive(false);
            if(AverageBet!=null) AverageBet.SetActive(true);
        }else
        {
            if(BetPanel!=null) BetPanel.SetActive(true);
            if(AverageBet!=null) AverageBet.SetActive(false);
        }
    }

    private void RefreshCashState(bool isShow)
    {
        if (isShow)
        {
            ShowCashUI();
        }
        else
        {
            HideCashUI();
        }
    }
    
    private void SetWinSilenceText(long value)
    {
        DOTween.Kill(this);
        win.SetText(Utils.Utilities.ThousandSeparatorNumber(value));
    }

    private void SetWinText(float value)
    {
		win.SetText(Utils.Utilities.ThousandSeparatorNumber(value));
    }
    private void SetWinCashSilenceText(int value)
    {
        DOTween.Kill(this);
        SetCashText(value);
    }
    private void SetCashText(int value)
    {
        cash.SetText(OnLineEarningMgr.Instance.GetMoneyStr(value));
    }
	private long winStep;
    private long freespinwin;
    private void OnWinAnimation(float duration,long from,long win)
    {
        OnWinAnimation (duration,from,win,true);
    }
   
    private void OnWinAnimation(float duration,long from,long value,bool EnableAnimation)
    {
        if (isFreeSpin && freespinwin == value && from == 0) return;
        freespinwin = value + from;   
        if (value > 0) {
            if (!EnableAnimation) 
            {
				SetWinText(value);
                return;
            }

            CheckWinEffect(value);
            this.winStep = from;
            DOTween.To(()=>this.winStep,x=>this.winStep = x,(from + value),duration).OnUpdate(()=>{
                win.SetText(Utils.Utilities.ThousandSeparatorNumber(this.winStep));
            }).OnComplete(()=>{
                win.SetText(Utils.Utilities.ThousandSeparatorNumber(from + value));
            }).SetUpdate(true);
        } else {
            win.SetText(Utils.Utilities.ThousandSeparatorNumber(from));
        }
    }
    
    // 赢钱数量大于totalbet的多少倍时，显示特效
    private const int ShowEffectMultiple = 1;
    private void CheckWinEffect(long value)
    {
        if (value >= ShowEffectMultiple * BaseSlotMachineController.Instance.currentBetting)
        {
            if (WinEffect != null)
            {
                WinEffect.SetActive(true);
                new DelayAction(1.0f, null, (() =>
                {
                    WinEffect.SetActive(false);
                })).Play();
            }
        }
    }

    #region 展示赢钱的特效
    private int winCashStep;
    //freespi过程中累加的钱数
    private int freespincash;
    private void OnWinCashAnimation(float duration,int from,int win)
    {
        OnWinCashAnimation (duration,from,win,true);
    }
   
    private void OnWinCashAnimation(float duration,int from,int value,bool EnableAnimation)
    {
        if (isFreeSpin && freespincash == value && from == 0) return;
        freespincash = value + from;   
        
        if (value > 0) {
            if (!EnableAnimation) 
            {
                SetCashText(value);
                return;
            }

            CheckWinEffect(value);
            this.winCashStep = from;
            DOTween.To(()=>this.winCashStep,x=>this.winCashStep = x,(from + value),duration).OnUpdate(()=>{
                SetCashText(winCashStep);
            }).OnComplete(()=>{
                SetCashText(from + value);
            }).SetUpdate(true);
        } else {
            SetCashText(value);
        }
    }
    #endregion
    

    public void OpenPayTable()
    {
        Messenger.Broadcast (GameDialogManager.OpenPaytablePanel);
        AudioEntity.Instance.PlayEffectAudio("paytable_open");
    }

    public void OnFastSpin()
    {
        ReelManager rm = BaseSlotMachineController.Instance.reelManager;
        rm.SetFastMode(!rm.GetFastMode());
        Messenger.Broadcast(GameConstants.PressedFastButton);
    }

    public void OnSubBet()
    {
        if (BaseSlotMachineController.Instance.MinusBetClicked())
        {
            PlayMinusBet();
            Messenger.Broadcast(GameConstants.ON_BET_CHANGE_SUCCEED);
        }

        MaxBetShow();
    }

    public void OnAddBet()
    {
        if (BaseSlotMachineController.Instance.PlusBetClicked())
        {
            PlayPlusBet();
            Messenger.Broadcast(GameConstants.ON_BET_CHANGE_SUCCEED);
        }

        MaxBetShow();
    }

    public void OnMaxBet()
    {
        BaseSlotMachineController.Instance.MaxBetClicked ();
        AudioManager.Instance.AsyncPlayEffectAudio("bet_max");
        Messenger.Broadcast(GameConstants.ON_BET_CHANGE_SUCCEED);
        MaxBetShow();
    }

    private void MaxBetShow()
    {
        bool isMaxBet = BaseSlotMachineController.Instance.IsMaxBet();
        bool isMinBet = BaseSlotMachineController.Instance.IsMinBet();
        if (isMaxBet && isMinBet)
        {
            AddBetButton.interactable = false;
            MaxBetButton.interactable = false;
            SubBetButton.interactable = false;
        }
        else if(isMaxBet)
        {
            AddBetButton.interactable = false;
            MaxBetButton.interactable = false;
            SubBetButton.interactable = true;
        }
        else if (isMinBet)
        {
            AddBetButton.interactable = true;
            MaxBetButton.interactable = true;
            SubBetButton.interactable = false;
        }
        else
        {
            AddBetButton.interactable = true;
            MaxBetButton.interactable = true;
            SubBetButton.interactable = true;
        }
    }
    private void DisableBetBtns()
    {
        DisactiveButtons();
    }
    
    private void EnableBetBtns()
    {
        ActiveButtons();
    }
    //禁用按钮
    public void DisactiveButtons(bool state = false)
    {
        SubBetButton.interactable = false;
        MaxBetButton.interactable = false;
        AddBetButton.interactable = false;
        FastSpinButton.interactable = false;
		PayTableButton.interactable = false;
    }

    //激活按钮
	public void ActiveButtons(bool ignoreSpin = false)
    {
        SubBetButton.interactable = true;
        MaxBetButton.interactable = true;
        AddBetButton.interactable = true;
        FastSpinButton.interactable = true;
		PayTableButton.interactable = true;
        MaxBetShow();
    }

    public void DisableSpinButton(bool state)
    {
        if(disableSpin != null) disableSpin.SetActive(state);
    }

    private bool isFreeSpin = false;

    private void OnEnterFreespin()
    {
        isFreeSpin = true;
        lastState = BtnStyle.state;
        BtnStyle.state = SpinButtonStyle.SpinButtonState.FREE;
        if (!BaseSlotMachineController.Instance.isUseMachineFreespinBtn)
        {
            BtnStyle.SpinText.gameObject.SetActive(false);
        }
        BtnStyle.numOfMenuObj.gameObject.SetActive(false);
        RewardSpin.alpha = 0;
    }

	private void OnFreespinTimeChange(int leftFreespinTimes,int totalFreespinTimes,int multiple)
    {
        //BtnStyle.isStartSpin = true;
        BtnStyle.FreeSpinText.SetText(leftFreespinTimes.ToString()+"/"+totalFreespinTimes);
    }

    private void OnQuitFreespin()
    {
        isFreeSpin = false;
        freespinwin = 0;
        freespincash = 0;
        BtnStyle.state = lastState;
        BtnStyle.SpinText.gameObject.SetActive(true);
        RewardSpin.alpha = 1;
        if (spinNumPanel!=null) {
            spinNumPanel.SetTrigger ("Show");
        }
    }

    public void HideCashUI()
    {
        Transform xian = win.transform.parent.Find("xian");
        xian.gameObject.SetActive(false);
        RectTransform winTr = win.GetComponent<RectTransform>();
        winTr.anchoredPosition = new Vector3(0, 60);
        winTr.sizeDelta = new Vector2(800,100);
        TextMeshProUGUI tmp_win =  win.GetComponent<TextMeshProUGUI>();
        tmp_win.fontSizeMax = 105;
        cash.gameObject.SetActive(false);
    }

    public void ShowCashUI()
    {
        Transform xian = win.transform.parent.Find("xian");
        xian.gameObject.SetActive(true);
        SetCashText(0);
        RectTransform winTr = win.GetComponent<RectTransform>();
        winTr.anchoredPosition = new Vector3(-200, 54);
        winTr.sizeDelta = new Vector2(400,100);
        cash.gameObject.SetActive(true);
        win.GetComponent<TextMeshProUGUI>().fontSizeMax = 56.0f;
    }
}
