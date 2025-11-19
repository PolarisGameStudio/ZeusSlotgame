using Libs;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Ads;
using Classic;
using TMPro;
using UnityEngine.Localization;
using Utils;

public class SpinWinDialog : UIDialog 
{
	[Header("SpinWinDialog配置")]
	public List<Sprite> winType;
	public List<Sprite> winTypeMask;
	public List<Sprite> win;
	public List<Image> winImage;
	public UIText winText;
	public UIText cashText;
	public Button collect;
	public SpinWinAnimationEvent animationEvent;
	private Tween tween = null;
	private Tween Cashtween = null;

    private long curCoins = 0;
    private long totalCoins = 0;
    private int curCash = 0;
    private int totalCash = 0;
	private SpinWinType spinWinType = SpinWinType.NONE;
	public float time = 5f;
    private bool m_HasClosed = false;
	public Button WatchAdBtn;
	public Button CloseBtnOnAd; //有广告时的关闭按钮
	public Button CollectBtn;
	public Button StopBtn;

	bool isplayAds;
	string entranceName;
	public const string PlayBigWinAD = "PlayBigWinAD";
	public const string PlayGBigWinADDefeated = "PlayBigWinADDefeated";
	private bool isBigWin;
	private bool isShowWatchBtn;
	//public UIText multi;
	public int multiNum;
	//public TextMeshProUGUI WatchAdBtnText;
	private double winBetLimit_Min = 100;
	private double winBetLimit_Max = 1000000;
	public float winMultiplesLimit_Max = 16;
	public float winMultiplesLimit_Min = 5;
	private bool isCollect;
	private TaskTipPanel _taskTipPanel;
	private bool isFirstTime = true;
	private bool isSecondTime = false;
	
	//激励视频广告倍数
	private int RewardAdMultiple = 1;
	
	protected override void Awake()
	{
		base.Awake();
		UGUIEventListener.Get(this.WatchAdBtn.gameObject).onClick = this.OnWatchADButtonClick;
		// UGUIEventListener.Get(this.StopBtn.gameObject).onClick = this.OnButtonClickHandler;
		UGUIEventListener.Get(this.CollectBtn.gameObject).onClick = this.OnCollectBtnClick;
		isShowWatchBtn = false;
		SetWatchBtn(false);
		this.CollectBtn.gameObject.SetActive(false);
		UGUIEventListener.Get(this.CloseBtnOnAd.gameObject).onClick = this.OnNotWatchADButtonClick;
		_taskTipPanel = Utilities.RealFindObj<TaskTipPanel>(transform,"SpinWin/TaskTipsPanel");
		if (_taskTipPanel!=null)
		{
			_taskTipPanel.RefreshInfo(TaskConstants.CollectTriggerSpinWinCountTask_Key);
		}
	}

	public void OnStart(long coins, SpinWinType type)
	{
		if (coins <= 0) { this.Close(); return; }
		isBigWin = type == SpinWinType.BIG;
		spinWinType = type;
		totalCoins = coins;
		animationEvent.index = (int)spinWinType;
		winImage[0].sprite = winType[(int)type];
		winImage[1].sprite = winTypeMask[(int)type];
		winImage[2].sprite = win[(int)type];
		foreach (var item in winImage) item.SetNativeSize();
		this.PlayWinTypeEffect();
		AudioEntity.Instance.PlayRollUpEffect(0.7f);
		bool haveAd = false;
		tween = Utils.Utilities.AnimationTo(this.curCoins, coins, time, UpdateTextUI, null, () =>
		{
			AudioEntity.Instance.StopRollingUpEffect();
			UpdateTextUI(coins);
			tween = null;
		});
		int popCount = OnLineEarningMgr.Instance.AddPopSpinWinCount();
		isFirstTime = popCount==1;
		isSecondTime = popCount==2;
		if (!PlatformManager.Instance.IsWhiteBao())
		{
			totalCash = OnLineEarningMgr.Instance.GetSpinWinReward((int)spinWinType);
			//金币滚动
			Cashtween = Utils.Utilities.AnimationTo(curCash, totalCash, time, SetCashCoins, null, () =>
			{
				SetCashCoins(totalCash);
				Cashtween = null;
			});
		}
		cashText.gameObject.SetActive(!PlatformManager.Instance.IsWhiteBao());
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
			Transform numImage = Utilities.RealFindObj<Transform>(CollectBtn.transform, "number");
			numImage.gameObject.SetActive(true);
		}
		else
		{
			//Debug.Log("SpinWinDialogNew OnStart isThirdTime");
			//第三次之后有广告和插屏的按钮，此时插屏的广告进度并未累计完毕，所以不能看插屏
			//增加插屏广告的检测进度
			Messenger.Broadcast(ADConstants.CloseSpinWinMsg);
			//若不满足间隔，并且配置有不显示广告
			if (ADManager.Instance.CheckHideSpinWin())
			{
				WatchAdBtn.gameObject.SetActive(false);
				//显示免费的收集按钮
				CollectBtn.gameObject.SetActive(true);
				CloseBtnOnAd.gameObject.SetActive(false);
			}
			else
			{
				RewardAdMultiple = ADManager.Instance.GetADRewardMultiple(ADEntrances.REWARD_VIDEO_ENTRANCE_SPINWIN);
				Text adMultiple = Utilities.RealFindObj<Text>(WatchAdBtn.transform, "number");
				adMultiple.text = "" + RewardAdMultiple;
				WatchAdBtn.gameObject.SetActive(true);
				//显示免费的收集按钮
				CollectBtn.gameObject.SetActive(false);
				DelayShowCloseOnAdBtn();
			}
		}
	}
	
	void DelayShowCloseOnAdBtn()
	{
		if (CloseBtnOnAd!=null)
		{
			Debug.Log("Scale animation complete111");
			CloseBtnOnAd.enabled = false;
			CloseBtnOnAd.gameObject.SetActive(false);
			TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(CloseBtnOnAd.transform, "claim");
			LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"Claim");
			int rewardRate = (int)(OnLineEarningMgr.Instance.GetClaimRewardRate()*100);
			claim.text = localizedString.GetLocalizedString()+" "+rewardRate+"%";
			// CloseBtnOnAd.transform.localScale = Vector3.zero;
			new DelayAction(0.7f, null, () =>
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
	
	void SetWatchBtn(bool iswatchBtn = false)
	{
		this.CollectBtn.gameObject.SetActive(!iswatchBtn);
		this.WatchAdBtn.gameObject.SetActive(iswatchBtn);
		this.CloseBtnOnAd.gameObject.SetActive(iswatchBtn);
	}

	void OnEnable()
	{
		Messenger.AddListener<int>(ADConstants.PlaySpinWinAD,AdIsPlaySuccessful);
		Messenger.AddListener<int>(ADConstants.PlaySpinWinADFailed,AdIsPlayFailed);
		Messenger.AddListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);
	}
	void OnDisable()
	{
		Messenger.RemoveListener<int>(ADConstants.PlaySpinWinAD,AdIsPlaySuccessful);
		Messenger.RemoveListener<int>(ADConstants.PlaySpinWinADFailed,AdIsPlayFailed);
		Messenger.RemoveListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);
	}
	
	public override void OnButtonClickHandler (GameObject go)
    {
        base.OnButtonClickHandler(go);
        collect.interactable = false;
        this.OnClickStopUpdate();
		isCollect = true;
		Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
			GameConstants.CollectBonusWithType, go.transform, Libs.CoinsBezier.BezierType.DailyBonus, null);
		Messenger.Broadcast(SlotControllerConstants.OnBlanceChangeForDisPlay);
		Libs.AudioEntity.Instance.StopAllEffect();
		Libs.AudioEntity.Instance.PlayCoinCollectionEffect();
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
		if (isFirstTime||isSecondTime||ADManager.Instance.CheckHideSpinWin())
		{
			TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(CollectBtn.transform, "Claim");
			claim.text = OnLineEarningMgr.Instance.GetMoneyStr(cash,needIcon:false,needBigNum:true);
		}
		else
		{
			TextMeshProUGUI claim = Utilities.RealFindObj<TextMeshProUGUI>(WatchAdBtn.transform, "Claim");
			claim.text = OnLineEarningMgr.Instance.GetMoneyStr(cash,needIcon:false,needBigNum:true);
		}
	}
	
	private void PlayWinTypeEffect()
	{
		switch(spinWinType)
		{
			case SpinWinType.EPIC :
				AudioEntity.Instance.PlayEffect("epic_win");
				break;
			case SpinWinType.MEGA :
				AudioEntity.Instance.PlayEffect("mega_win");
				break;
			case SpinWinType.BIG :
				AudioEntity.Instance.PlayEffect("big_win");
				break;
		}
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
        AudioEntity.Instance.StopRollingUpEffect();
    }

	private void CloseDialog()
	{
		if(this.m_HasClosed)
        {
            return;
        }
        m_HasClosed = true;
        this.Close ();

		Messenger.Broadcast(GameConstants.CLOSE_SPIN_WIN_DIALOG);

		switch(spinWinType)
		{
			case SpinWinType.EPIC :
				CommandTriggerManager.Instance.CheckMomentConditions (GameConstants.CLOSE_EPIC_WIN_DIALOG);
				Messenger.Broadcast(GameConstants.CLOSE_EPIC_WIN_DIALOG);
				break;
			case SpinWinType.MEGA :
				CommandTriggerManager.Instance.CheckMomentConditions (GameConstants.CLOSE_MEGA_WIN_DIALOG);
				Messenger.Broadcast(GameConstants.CLOSE_MEGA_WIN_DIALOG);
				break;
			case SpinWinType.BIG :
				CommandTriggerManager.Instance.CheckMomentConditions (GameConstants.CLOSE_BIG_WIN_DIALOG);
				Messenger.Broadcast(GameConstants.CLOSE_BIG_WIN_DIALOG);
				break;
		}
	}
	private bool isPlayAd;
	//是否选择过按钮，要么点击关闭，要么点击看广告
	private bool HasClicked = false;
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
		Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance,ADEntrances.REWARD_VIDEO_ENTRANCE_SPINWIN);
		// SendMsg(spinWinType,RewardAdMultiple);
	}
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
		//广告时机
		// Messenger.Broadcast(ADConstants.CloseSpinWinMsg);
		//检查是否可以播放广告
		Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance,ADEntrances.Interstitial_Entrance_CLOSESPINWIN);
		// SendMsg(spinWinType);
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
		Messenger.Broadcast(ADConstants.CloseSpinWinMsg);
		if (isSecondTime)
		{
			//奖励乘以倍数
			RewardADIsPlaySuccess();
		}else
		{
			//免费领奖不翻倍
			DoneADCallBack();
		}
	}
	 //广告播放成功
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

    //广告播放失败
    void AdIsPlayFailed(int type)
    {
	    AdIsPlaySuccessful(1);
    }
    
    void RewardADIsPlaySuccess()
    {
	    totalCash *= RewardAdMultiple;
	    //钱已经加过一次了，所以需要倍数减1
	    totalCoins *= (RewardAdMultiple-1);
        DoneADCallBack();
    }

    void HandleNotMeetConditionMsg(string msg)
    {
        if (msg == ADEntrances.Interstitial_Entrance_CLOSESPINWIN)
        {
	        totalCash  =(int)(totalCash* OnLineEarningMgr.Instance.GetClaimRewardRate());
            this.DoneADCallBack();
        }
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
            Messenger.Broadcast<Transform, Libs.CoinsBezier.BezierType, System.Action>(
                GameConstants.CollectBonusWithType, CollectBtn.transform, Libs.CoinsBezier.BezierType.DailyBonus,null);
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
                GameConstants.CollectBonusWithType, CollectBtn.transform, Libs.CoinsBezier.BezierType.Purchase, null);
        }
        Messenger.Broadcast(SlotControllerConstants.OnBlanceChangeForDisPlay);
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
public enum SpinWinType
{
	BIG = 0,
	MEGA,
	EPIC,
	NONE
}

