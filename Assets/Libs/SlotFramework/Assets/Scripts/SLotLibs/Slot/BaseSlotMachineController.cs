using UnityEngine;
using System.Collections;
using Core;
using Utils;
using System.Collections.Generic;
using System;
using RealYou.Core.UI;
using Classic;
using DG.Tweening;
using Libs;

using DelayAction = Libs.DelayAction;

public class BaseSlotMachineController : MonoBehaviour
{

//    public  int DEVIDE_EXP = 100;
    //exp devide when spin,exp = bet/DEVIDE_EXP
    public SlotConsole slotConsole;
    [HideInInspector] public TestBoardDataPanel TestBoardData;

    [HideInInspector] public Camera ToCamera;
	public int MachineSpinTime = 0;
	public int currentLevelSpinCount = 0;
	public DateTime currentLevelTotalTime;
	public DateTime startTimeInSlot;
    public enum WIN_TYPE{
        NONE      = -1,
        SMALL_WIN = 0,
        MID_WIN   = 1,
        BIG_WIN   = 2,
        TOP_WIN   = 3,
    }
	public Transform CoinsTransform {
		get;
		set;
	}
	public Transform CashTransform {
		get;
		set;
	}
	public Transform MenuTransform {
		get;
		set;
	}
    //classic机器播放奖励动画专用
    private const int SmallAnimationTag = 1;
    private const int MiddleAnimationTag = 5;
    private const int BigAnimationTag = 10;

    #region Statistics RTP
    public StatisticsManager statisticsManager = new StatisticsManager();
	#endregion
	public static bool Is_Quit = true;

	public const int ENTER = 1;
	public const int EXIT = 2;

	private float oldMissionProgress = 0f;
	int missionProgressShowNum = 0;
	bool needShowMissionProgress = false;
	protected static bool bFirstStart = true;
	public bool Force_FreeRun = false;
	//记录前端机器相关数据 ,目前用于上发事件
	public long ReSpinTotalAward;
	public long FreeSpinTotalAward;
    public FreespinGame freespinGame {
        get;
        set;
    }

    public bool isFreeRun {
		get { return  Force_FreeRun || reelManager.isFreespinBonus || onceMore; }
    }

    public bool onceMore {
        get;
        set;
    }

    public bool isUseMachineFreespinBtn
    {
	    get;
	    set;
    }

    public ReelManager reelManager {
        get;
        set;
    }
	//public  float currentBetting = 0f;
	///用于EventLog 实际使用的bet值记录
	public long preBettingForEventLog = 0;
	
	public long currentCacheBetting;

    public long currentBetting
    {
        get
        {
            currentCacheBetting = Utilities.CastValueLong(PlayerPrefs.GetString("Current_Bet", "0"));
            //currentCacheBetting = Utilities.CastValueLong(PlayerPrefs.GetInt("Current_Bet", 0));
            return currentCacheBetting;
        }
        set
        {
	        currentCacheBetting = value;
	        PlayerPrefs.SetString("Current_Bet", currentCacheBetting.ToString());
            //PlayerPrefs.SetInt("Current_Bet", Utilities.CastValueInt(currentCacheBetting));
            UserManager.GetInstance().SetCurrentBet(currentCacheBetting);
        }
    }

	[HideInInspector]
	public Dictionary<string,bool> AutoPilotflagInSession =new Dictionary<string, bool>(){
		{"spin",true},
		{"buy",true},
		{"out_of_coins",true},
		{"time",true},
		{"nearly_out_of_coins",true}
	};//11111b 控制autopilot在当前session内只发送一次，因为每次切换机器都会重新实例化baseslotMachineController所以只要执行过就设置标记为false即可

	public long currentBettingTemp;

	#region Restore SlotsMachineStateData
	public void RestoreSlotsMachinesGameStateData(long currentBet){
		currentBetting = currentBet;
		currentBettingTemp = currentBetting;
		if (reelManager.extraAward != null)
			reelManager.extraAward.OnBetChanger(currentBetting);
		Messenger.Broadcast<long> (SlotControllerConstants.OnBetChange, currentBetting);//恢复赌资
		Messenger.Broadcast<bool> (SlotControllerConstants.DisactiveButtons,reelManager.gameConfigs.EnableSpinBtnInFreeSpin);//禁用赌资调整按钮
	}
	//恢复bet但是不禁用赌资
	public void RestoreSlotsMachinesBet(long currentBet){
		currentBetting = currentBet;
		currentBettingTemp = currentBetting;
		if (reelManager.extraAward != null)
			reelManager.extraAward.OnBetChanger(currentBetting);
		Messenger.Broadcast<long> (SlotControllerConstants.OnBetChange, currentBetting);//恢复赌资
	}
	#endregion
	public SlotMachineConfig slotMachineConfig {
        get;
        private set;
    }

    public static BaseSlotMachineController Instance {
        get {
	        if (BaseGameConsole.ActiveGameConsole()==null)
	        {
		        return null;
	        }
			return BaseGameConsole.ActiveGameConsole().SlotMachineController as BaseSlotMachineController;
        }
    }

    public bool addLastWinCoinsToDisplay {
        get;
        set;
    }

	public long winCoinsForDisplay {
        get;
        set;
    }
	public int winCashForDisplay {
		get;
		set;
	}
	
	public long RewardSpinWinCoinsByLast {
		get;
		set;
	}

//    public bool needCheckResult {
//        get;
//        set;
//    }

    public bool spinning {
        get;
        set;
    }

	public double totalAward {
        get;
        set;
    }
	public int totalCash {
		get;
		set;
	}
	/// <summary>
	///totalAward - featureWin
	/// </summary>
	public double baseAward
	{
		get;
		set;
	}

	public bool isReturnFromFreespin {
		get;
		set;
	}

    public  AverageFrame averageFrame = new AverageFrame (100);

    public bool hasLevelUp;

    public bool hasPopReward;
    
	//应对bigwin的时候弹出其他形式的框
	public bool temporaryHideBigWin =false;

	public bool isEpicWin {
		get;
		set;
	}
	public bool isMegaWin {
        get;
        set;
    }
    public bool isBigWin {
        get;
        set;
    }
    public bool isMiddleWin
    {
	    get;
	    set;
    }
    public bool isSmallWin
    {
	    get;
	    protected set;// 13/12/2019 before is private
    }
    public bool isTinyWin
    {
	    get;
	    set;
    }
	public bool hasAnticipation {
		get;
		set;
	}
	[HideInInspector]
	public bool lastOnceMore = false;

	[HideInInspector]
	public long PreBet = 0;
	
	[HideInInspector]
	public int JackpotHitProbability = 0;

    public ConfigRuleData RuleConfig { set; get; }

    public bool ShowCash = true;
    
    public bool IsCalculateCash = false;
	public bool IsAutopliotFeatureMachine(){
		// string featureMachineName = AutopilotManager.GetInstance ().GetXValueInTopicByKey<string> (SlotMachineConfig.FEATURED_MACHINE, "name", "");
		// if (slotMachineConfig.Name ().Equals (featureMachineName)) {
		// 	return true;
		// }
		return false;
	}

    #region LifeCycle

    void Awake ()
    {
#if !UNITY_EDITOR
	    PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.EnterGame);
#endif

        OnSlotMachineControllerAwake ();
		BaseSlotMachineController.Is_Quit = false;
		ToCamera = this.GetComponent<Camera> ();
    }
  
    void Start ()
    {
		if (BaseGameConsole.IsConfigInit) {
			OnSlotMachineControllerStart ();
		}
    }

    void OnDestroy ()
    {
        OnSlotMachineControllerDestroy ();
    }

    void Update ()
    {
        OnSlotMachineControllerUpdate ();
    }

    #region debug board

    private void InitDebugBoardPanel()
    {
#if DEBUG
		    GameObject obj = Resources.Load("TestBoardDataPanel") as GameObject;
		    
		    GameObject realObj = Instantiate(obj,slotConsole.transform.parent.transform,false) as GameObject;
		    this.TestBoardData = realObj.GetComponent<TestBoardDataPanel>();
		    RectTransform rectTransform = realObj.transform as RectTransform;
		    rectTransform.anchorMin = new Vector2(0,0);
		    rectTransform.anchorMax = new Vector2(1,1);
		    rectTransform.localScale = Vector3.one;
		    rectTransform.offsetMin = Vector2.zero;
		    rectTransform.offsetMax = Vector2.one;
#endif
    }
    #endregion

    protected virtual void OnSlotMachineControllerAwake ()
    {
	    Debug.Log("OnSlotMachineControllerAwake");
		GetGameConsole ();
		
        FixedFull.Scaler = Screen.width * 9f / (Screen.height * 16f);
        onceMore = false;
        addLastWinCoinsToDisplay = false;
        ResultStateManager.InitInstane (this);

        Messenger.AddListener<Transform, Libs.CoinsBezier.BezierType, System.Action>(GameConstants.CollectBonusWithType, CreateCoinBezierFrom);
        Messenger.AddListener<Transform, Transform, Libs.CoinsBezier.BezierType, System.Action> (GameConstants.CollectBonusWithTypeAndTarget, CreateCoinBezierFromTo);
		Messenger.AddListener(GameConstants.CollectBonusFromInputMouse, CreateCoinBezierFromInputMouse);		
		Messenger.AddListener<float,float>(GameConstants.SetBackGroundAudio, SetBackGroundAudio);


		InitAnimationActions ();
		BeginStay ();
		InitDebugBoardPanel();
		RegisterMB();
    }
    
    private IMachineBehaviour machineBehaviour;
    private void RegisterMB()
    {
	    if (machineBehaviour == null)
	    {
		    machineBehaviour = new BetTipsMB();
		    MachineUtility.Instance.RegisterMachineEvent(machineBehaviour);
	    }
    }

    private void UnregisterMB()
    {
	    if (machineBehaviour != null)
	    {
		    MachineUtility.Instance.UnRegisterMachineEvent(machineBehaviour);
	    }
    }
	protected virtual void GetGameConsole(){
		 BaseGameConsole.ActiveGameConsole ();
	}

    protected virtual void OnSlotMachineControllerStart ()
    {
        try
        {
            BaseGameConsole.ActiveGameConsole().EnterSlotMachine(this);
            //SequencePaidResultsManager.Reset();
            slotMachineConfig = BaseGameConsole.singletonInstance.SlotMachineConfig(SwapSceneManager.Instance.GetLogicSceneName()) as SlotMachineConfig;
            

            if (slotMachineConfig == null)
            {
                slotMachineConfig = new SlotMachineConfig();
                slotMachineConfig.SetName(SwapSceneManager.Instance.GetLogicSceneName());
            }
            //单机游戏解析配置文件
            slotMachineConfig.ParseDict();

			SendEnterSlotAutopilotLog();

            // 上报提现奖励状态（进入游戏时）
            OnLineEarningMgr.Instance.ReportWithdrawRewardStatusOnEnterGame();

            //初始化 bet 数据
            DataManager.GetInstance().InitBetData(slotMachineConfig);
            currentBetting = UserManager.GetInstance().getBetByBalance(slotMachineConfig);
            preBettingForEventLog = currentBetting;
			
            //跳转至SlotMiddlePanel 进行ReelManager 初始化， MiddlePannel创建
            slotConsole.Init(slotMachineConfig, OnRollEnd, OnRollStart);
            Messenger.Broadcast(GameConstants.OnSceneInit);
            
            reelManager.SetFastModeOnStart();
            reelManager.SetPlistFastTimeScale(ApplicationConfig.GetInstance().FastTimeScale,slotMachineConfig.FastTimeScale);
            Messenger.Broadcast(GameConstants.OnSlotMachineSceneInit);
            // 竖版unity广告会奔溃，临时屏蔽
            if(!slotMachineConfig.IsPortrait)
				CommandTriggerManager.Instance.CheckMomentConditions(GameConstants.ENTER_MACHINE);
            reelManager.isFreespinBonus = false;
            if (reelManager.extraAward != null)
            {
                reelManager.extraAward.Init(slotMachineConfig.extroInfos.infos, animationAction.PlayNextChildAction);
            }

            MachineSpinTime = 0;
            
            startTimeInSlot = DateTime.Now;

            currentLevelSpinCount = UserManager.GetInstance().UserProfile().GetSpinCountForLevelUpInterval();
            //freespinSuspend freespinResume监听要在《对保存数据进行恢复》之前，因为恢复的可能会发送这几个消息
            Messenger.AddListener(SUPSPEND_FREESPIN, freespinSuspend);
            Messenger.AddListener(RESUME_FREESPIN, freespinResume);
            
            Messenger.AddListener<bool>(CALCULATE_CASH, CalculateCash);
            
            Messenger.AddListener(SlotControllerConstants.AUTO_SPIN_SUSPEND, AutoSpinSuspendHandle);
            Messenger.AddListener(SlotControllerConstants.AUTO_SPIN_RESUME, AutoSpinResumeHandle);
            
			Messenger.AddListener(SlotControllerConstants.HIGH_ROLLER_CHECK_KEY,CheckPopupHighRollerDialog);
			
			//播放背景音乐
			AudioEntity.Instance.UnPauseBackGroundAudio(Libs.AudioEntity.audioSpin);
			
            //对保存数据进行恢复
			if (reelManager.NeedSavePlayerSlotMachineStateData && (!RewardSpinManager.Instance.hasFreeSpin || reelManager.SpinUseNetwork))
			{
				reelManager.RecoveryPlayerPreviousSlotMachineStateData();
			}
			reelManager.PlayBGAudio();
			
	        this.RuleConfig = new ConfigRuleData(this.reelManager);
            SpinClickedMsgMgr.Instance.OnInitReels();
            CoinsIn = 0;
            
            Messenger.Broadcast(GameConstants.SLOT_CONTROLLER_START_OVER);
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint, "EnterGame");
        }
        catch (Exception ex)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("reason", "InitSlotError:" + ex.Message+" StackTrace:"+ex.StackTrace);
            BaseGameConsole.ActiveGameConsole().LogBaseEvent(Analytics.SCENE_EXCHANGE_EXCEPTION, dict);

			#if UNITY_EDITOR
				Debug.LogError("machineError:"+ex.Message +":"+ex.StackTrace);
			#endif

            SwapSceneManager.Instance.ResetState();
            Libs.UIManager.Instance.CloseAll(true);
            
            // string sceneName = GameConstants.LOBBY_NAME;
            // SwapSceneManager.Instance.SwapGameScene(sceneName, false,finishCB: () =>
            // {
	           //  Libs.UIManager.Instance.CloseAll(true);
            // });
        }

    }
    
    private void SendSceneLoadSucceedES(string machineName)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("MachineName", machineName);
        BaseGameConsole.ActiveGameConsole().LogBaseEvent(Analytics.SCENE_LOAD_SUCCEED, dict);
    }

	#region High Roller
	private const string strLastHighRollerTriggerimeKey = "LastHighRollerTriggerTime";
	private const string strHasTriggerHighRollerCountKey = "TriggerHighRollerCount";
	public long nLastPopupHighRollerDialogTime{
		get{ return SharedPlayerPrefs.LoadPlayerPrefsLong(SwapSceneManager.Instance.GetLogicSceneName()+"_"+strLastHighRollerTriggerimeKey, 0);}
		set{ SharedPlayerPrefs.SavePlayerPrefsLong(SwapSceneManager.Instance.GetLogicSceneName()+"_"+strLastHighRollerTriggerimeKey, value); }
	}
	public int nCurrentTriggerHighRollerCount{
		get{ return SharedPlayerPrefs.GetPlayerPrefsIntValue(SwapSceneManager.Instance.GetLogicSceneName()+"_"+strHasTriggerHighRollerCountKey, 0);}
		set{ SharedPlayerPrefs.SetPlayerPrefsIntValue(SwapSceneManager.Instance.GetLogicSceneName()+"_"+strHasTriggerHighRollerCountKey, value); }
	}

	private void CheckPopupHighRollerDialog(){
		//必须放到发送场景初始化消息之前，因为要求HighRoller弹出优先级最高，防止其他地方弹框影响HighRoller弹框
		if (slotMachineConfig.IsHighRollerMachine) {
			DateTime now = System.DateTime.Now;
			double seconds = (now -TimeUtils.ConvertLongToDate(nLastPopupHighRollerDialogTime)).TotalSeconds;
			if (seconds >= slotMachineConfig.HighRollerDisplayInterval) {
				nCurrentTriggerHighRollerCount = 0;
				nLastPopupHighRollerDialogTime = TimeUtils.ConvertDateTimeLong(now);
			}

			if (nCurrentTriggerHighRollerCount < slotMachineConfig.HighRollerDisPlayCount) {
				Messenger.Broadcast<SlotMachineConfig>(GameDialogManager.OPEN_HIGH_ROLLER_DIALOG, slotMachineConfig);
			}
		}
	}
	#endregion

	#region MyRegion
	void SendEnterSlotAutopilotLog()
	{
		string slotName = slotMachineConfig.Name();
		if (string.IsNullOrEmpty(slotName)) return;
		if (BaseGameConsole.ActiveGameConsole().IsFirstTriggerFeatureInThisSession(slotName))
		{
			BaseGameConsole.ActiveGameConsole().AddFirstTriggerFeatureInSessionList(slotName);
		}
	}

	public void SendSlotAutopilotLog(string eventName)
	{
		// AutopilotManager.GetInstance ().SlotCommonEventLog (slotMachineConfig.Name(),eventName,1);
	}

	public void SendSlotAutopilotLog_Test()
	{
		// AutopilotManager.GetInstance ().SlotTestEventLog (slotMachineConfig.Name());
	}

	public void SendSlotAutopilot_Time(float costTime)
	{
		// AutopilotManager.GetInstance ().SlotCommonEventLog (slotMachineConfig.Name(),GameConstants.Time_Key,costTime);
	}

	public void SendSlotAutopilotCoinsIn()
	{
		// AutopilotManager.GetInstance().SlotCommonEventLog(slotMachineConfig.Name(),GameConstants.Coins_hold_Key,CoinsIn);
		// CoinsIn = 0;
	}
	#endregion

	public void SendFlurry(int type)
	{
		Dictionary<string, object> analyticsParameters = new Dictionary<string, object>();

		analyticsParameters["type"] = type;
		analyticsParameters ["currentBet"] = this.currentBettingTemp;
		Analytics.GetInstance().LogEvent(Analytics.EnterExitMachine, analyticsParameters);
	}

	private void UpdateFrame()
	{

		averageFrame.ChangeFrame (Time.deltaTime);
	}


    protected virtual void OnSlotMachineControllerUpdate ()
    {
//		#if UNITY_ANDROID
//        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.OSXEditor) {
//            if (Input.GetKeyUp (KeyCode.Escape)) {
//                if (Libs.UIManager.Instance.OnKeyBack ()) {
//                } else {
//                    BackGameLobbyClicked ();
//                    //Libs.SoundEntity.Instance.Click ();
//                    Libs.AudioEntity.Instance.PlayClickEffect ();
//                }
//            }
//        }
//		#endif
		UpdateFrame ();
    }

    protected virtual void OnSlotMachineControllerDestroy ()
	{
		Messenger.RemoveListener (SUPSPEND_FREESPIN, freespinSuspend);
		Messenger.RemoveListener (RESUME_FREESPIN, freespinResume);
		Messenger.RemoveListener<bool> (CALCULATE_CASH, CalculateCash);
//		if(reelManager.SpinUseNetwork)
//			Messenger.RemoveListener(GameConstants.NetworkSpinResponse, OnSpinNetResponse);
		
		Messenger.RemoveListener(SlotControllerConstants.AUTO_SPIN_SUSPEND, AutoSpinSuspendHandle);
		Messenger.RemoveListener (SlotControllerConstants.AUTO_SPIN_RESUME, AutoSpinResumeHandle);
		Messenger.RemoveListener(SlotControllerConstants.HIGH_ROLLER_CHECK_KEY,CheckPopupHighRollerDialog);
        Messenger.RemoveListener<Transform, Libs.CoinsBezier.BezierType, System.Action>(GameConstants.CollectBonusWithType, CreateCoinBezierFrom);
		Messenger.RemoveListener(GameConstants.CollectBonusFromInputMouse, CreateCoinBezierFromInputMouse);
		Messenger.RemoveListener<Transform, Transform, Libs.CoinsBezier.BezierType, System.Action> (GameConstants.CollectBonusWithTypeAndTarget, CreateCoinBezierFromTo);
		Messenger.RemoveListener<float,float>(GameConstants.SetBackGroundAudio, SetBackGroundAudio);

        //UnregisterPaymentListeners ();
        Libs.AudioEntity.Instance.StopAllAudio();//LQ 清空所有的Audioclip

        UnregisterMB();
	}

	private bool isFreespinSupend = false;
	void freespinSuspend()
	{
		if (isFreeRun) {
			isFreespinSupend = true;
		}
	}

	void freespinResume()
	{
		if (isFreespinSupend) {
			isFreespinSupend = false;
			//只有freerun有效，在进行恢复
			if (isFreeRun) {
				DoSpin ();
			}
		}
	}

	void CalculateCash(bool showCash = false)
	{
		IsCalculateCash = showCash;
		freespinGame.NeedShowCash = IsCalculateCash;
	}

    #endregion
    
    public string BackGameLobbyClickedFrom;
	public float StartBackGameLobbyTime{set; get;}
	long CoinsIn;
    public void BackGameLobbyClicked (Action finishedCB = null, string from = null)
    {
	    BackGameLobbyClickedFrom = from;
	    
	    Action machineQuestCB = null;
	  
	    // BackToLobby(false, () =>
	    // {
		   //  finishedCB?.Invoke();
	    // });
	    SendSlotAutopilotCoinsIn();
	    StartBackGameLobbyTime = Time.realtimeSinceStartup;
    }

    // private void BackToLobby(bool isFromMachineQuest, Action finishedCB = null)
    // {
	   //  SwapSceneManager.Instance.SwapGameScene (GameConstants.LOBBY_NAME,false,
		  //   ()=>{
			 //    DOTween.KillAll();
			 //    SaveDataBeforeSwitchScene ();
			 //    if (!isFromMachineQuest)
			 //    {
				//     if (!string.IsNullOrEmpty(BackGameLobbyClickedFrom) && BackGameLobbyClickedFrom == "ServerError")
				//     {
				// 	    Log.Trace("ServerError导致返回大厅，不展示全屏广告");
				// 	    BackGameLobbyClickedFrom = null;
				//     }
			 //    }
    //
			 //    if(this.slotMachineConfig != null)
			 //    {
				//     this.slotMachineConfig.ClearSpineData();
				//     this.slotMachineConfig.ClearStaticData();
			 //    } 
			 //    Messenger.Broadcast(GameConstants.OnSlotMachineSceneQuit);
			 //    UserManager.GetInstance().LastPlayMachine = slotMachineConfig.Name();
			 //    UserManager.GetInstance().LastPlayMachineSpinNum = MachineSpinTime;
			 //    UserManager.GetInstance().IsLastPlayMachineFeature = slotMachineConfig.IsFeatureSlot;
			 //    ResultStateManager.Instante.RemoveAll(); //20210708 by xxb
		  //   },()=>{
			 //    Libs.UIManager.Instance.CloseAll();
			 //    Libs.AudioEntity.Instance.StopSlotsBackGroundMusic();
			 //    
			 //    if (!isFromMachineQuest)
			 //    {
				//     CommandTriggerManager.Instance.CheckMomentConditions(GameConstants.BACK_TO_LOBBY);
			 //    }
    //
			 //    // 回到大厅时检测，有没有Tournament的奖励可以弹出
			 //    Dictionary<string,object> parameters = new Dictionary<string, object>();
			 //    parameters.Add("duration",Time.realtimeSinceStartup - StartBackGameLobbyTime);
			 //    BaseGameConsole.ActiveGameConsole().LogBaseEvent("BackLobbyEvent",parameters);
			 //    if (!isFromMachineQuest)
			 //    {
				//     if (CheckRateMachine()){
				// 	    Messenger.Broadcast<SlotMachineConfig>(GameDialogManager.OpenRateMachine,slotMachineConfig);
				//     }
			 //    }
			 //    // AutopilotManager.GetInstance().RemoveCurrentSlotTopicDict();
			 //    if (finishedCB != null) finishedCB();
		  //   });
    // }

    //Rate Machine pop out flag
	private bool CheckRateMachine()
	{
		float popPercent = Plugins.Configuration.GetInstance().GetValueWithPath<float>("Module/RateMachineConfig/PopOutChancePercentage", 50);
		int maxPopTime = Plugins.Configuration.GetInstance().GetValueWithPath<int>("Module/RateMachineConfig/MaxPopOutTime", 1);
		int spinRequirement = Plugins.Configuration.GetInstance().GetValueWithPath<int>("Module/RateMachineConfig/MinSpinTime", 50);
		#if UNITY_EDITOR
		//return true;
		#endif
		return (Application.internetReachability != NetworkReachability.NotReachable
			&& (UnityEngine.Random.Range(0.0f, 100.0f) < popPercent || slotMachineConfig.IsFeatureSlot)
			&& UserManager.GetInstance().UserProfile().GetRatingDialogPopedTime(slotMachineConfig.Name()) < maxPopTime
			&& MachineSpinTime >= spinRequirement);
	}

	public void SaveDataBeforeSwitchScene(){
		PreBet = 0;
        if (reelManager!=null)
        {
            reelManager.SavePlayerCurrentSlotMachineStateData();
        }
		Is_Quit = true;

		UserManager.GetInstance ().UserProfile ().SaveSpinCountForLevelUpInterval (currentLevelSpinCount);
		statisticsManager.Send_Machine_Real_RTP (startTimeInSlot,"BacktoLobby");
		UserManager.GetInstance ().SetFirstPlayedSlotMachineSpinFlag ();
		DataManager.GetInstance().DataMining.SaveDataToFile();
		RewardSpinManager.Instance.SendRewardSpinLogEvent ();
	}

	private void OpenSpecialOffer (int type,string subSourceType){}

    //private Payment.NotifyCoinPaymentSucceeded CoinPaymentSucceededListener;
	//private Payment.NotifyCoinPaymentFailed CoinPaymentFailedListener;
    //private Payment.NotifyCoinPurchaseResumed CoinPurchaseResumedListener;
    public bool IsJackpotWin{
        get;
        set;
    }

    public double JackpotAward {
        get;
        set;
    }

    private Action<bool> OnRollStartSystemCB;
	public string JackpotName {
		get;
		set;
	}

    public void SetJackpotData(bool isJackpot = false , double awardValue = 0 , string name = "Normal")
    {
        //Debug.Log ("awardValue  " + awardValue);
        IsJackpotWin = isJackpot;
        JackpotAward = awardValue;
		JackpotName = name;
    }

	public void ResetJackpotData()
	{
		IsJackpotWin = false;
		JackpotAward = 0;
		JackpotName = string.Empty;
	}
	
    protected virtual void OnRollStart ()
    {

	    if (!reelManager.AutoRun && !isFreeRun) {
            Libs.AudioEntity.Instance.StopAllEffect();
        }

        StopAllAnimation();
        InitParameter ();

	    ChangeUserData();

        try
        {
	        OnRollStartSystemCB?.Invoke(!isFreeRun);
        }
        catch (Exception e)
        {
	        BaseGameConsole.ActiveGameConsole().SendCatchExceptionToServer(e);
        }
		lastOnceMore = onceMore;
		onceMore = false; // Free Spin Game 会判断，如果是 onceMore，不扣除 LeftTime
		Messenger.Broadcast(GameConstants.OnSpinCloseSetting);
		//PiggyBankMgr.Instance.AddPiggy(currentBetting);
    }
    
    private Libs.DelayAction totalWinDelay;

     void InitParameter(){
        spinning = true;
		isBigWin = false;
		isMegaWin = false;
		isEpicWin = false;
//        needCheckResult = true;
		isReturnFromFreespin = false;
        totalAward = 0;//LQ 每次在reel之前需要清空，这样，不会影响当前判断（bonus trigger的判定，不然会瘦上一次的影响）
        JackpotTotalAward = 0;
        if (!isFreeRun) {
			ReSpinTotalAward = 0;
			FreeSpinTotalAward = 0;
            winCoinsForDisplay = 0;
            winCashForDisplay = 0;
			RewardSpinWinCoinsByLast = 0;
			// if (IsAutopliotFeatureMachine ()) {
			// 	AutopilotManager.GetInstance ().LogTopicEventBykey (SlotMachineConfig.FEATURED_MACHINE, "spin", 1);
			// }
		}

        //LQ 当轮子转停时，数值清零。
		if ((!addLastWinCoinsToDisplay) && (!isFreeRun)) {

            float delayClearWinCoins = (slotMachineConfig != null && slotMachineConfig.EnableRollingUp) ? 0.5f : 0f;
			totalWinDelay = new Libs.DelayAction(delayClearWinCoins, null, () =>
			{
				Messenger.Broadcast<float, long, long>(SlotControllerConstants.OnChangeWinText, 1f, 0, 0);
				Messenger.Broadcast<float, int, int>(SlotControllerConstants.OnChangeCashText, 1f, 0, 0);
				Messenger.Broadcast<bool>(SlotControllerConstants.RefreshCashState,false);
			});
            totalWinDelay.Play();
            winCashForDisplay = 0;
            winCoinsForDisplay = 0;
			RewardSpinWinCoinsByLast = 0;
        }
        if (reelManager.extraAward != null) {
            reelManager.extraAward.OnSpin ();
        }
        if (reelManager.isFreespinBonus) {
            freespinGame.OnSpin ();
        }

        if (isFreeRun == false) {
            Messenger.Broadcast(SlotControllerConstants.OnRechargeableSpinWillStart);
        }
        Messenger.Broadcast(SlotControllerConstants.OnSpinStart);

		if (reelManager.AutoRun || isFreeRun) {
			if (!isFreeRun) {
                reelManager.ChangeMiddleMessage ("AUTO SPIN");
			}
		} else {
            reelManager.ChangeMiddleMessage ("GOOD LUCK");
		}
		#region Restore Data
		if (!reelManager.isFreespinBonus) {
			reelManager.freespinFinished = false;
		}
		
		#endregion
     }

	public void ReSetSpinAwardState()
	{
		isBigWin = false;
		isMegaWin = false;
		isEpicWin = false;
		reelManager.resultContent.awardResult.awardValue = 0;
	}

	private bool extraAwardIsAccumulatedAlone(){
		return reelManager.extraAwardIsAccumulatedAlone ();
	}

	private int OldUserLevel = 0;
	
	public void ChangeUserData(){
		
		UserManager.GetInstance().UserProfile ().IncreaseSpinCounter();
		PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"Spin",UserManager.GetInstance().UserProfile().GetTotalSpinCounter());
		if (!isFreeRun) {
			UserManager.GetInstance ().IncreaseBalanceAndSendMessage (-(currentBetting));
			reelManager.IsSpinCostCoins = true;
			
			#region Statistics RTP
			statisticsManager.CostCoinsNum += currentBetting;
			statisticsManager.SpinNum++;
			
			StatDataAdapter.Instance.GlobalSpinNum++;
			if(reelManager.AutoRun)
			{
				statisticsManager.AutoSpinNum++;
			}
			#endregion
			Messenger.Broadcast<bool> (SlotControllerConstants.DisactiveButtons,reelManager.gameConfigs.EnableSpinBtnInFreeSpin);
			hasPopReward = OnLineEarningMgr.Instance.CheckCanPopReward();
			currentBettingTemp = currentBetting;
        } 
		else {
			if (RewardSpinManager.Instance.RewardIsValid()) {
				currentBettingTemp = currentBetting;
			}
		}
    }

    protected virtual void OnRollEnd ()
    {
		if (reelManager.gameConfigs.ReelEndWaitDuration > 0) {
			new Libs.DelayAction (reelManager.gameConfigs.ReelEndWaitDuration, null, delegate() {
                reelEndHandler ();
			}).Play(); 
		} else {
			reelEndHandler ();
		}
       
    }

	private void reelEndHandler()
	{
		reelManager.OnRollEndHandler (()=>
		{
			// reelManager.PauseBackGroundAudio();
			if(totalWinDelay != null) totalWinDelay.Stop();
			Messenger.Broadcast(SlotControllerConstants.OnSpinEnd);
			reelManager.HandleGameProcedureState();
		});
       
	}

    public virtual void ProcessSoundOnSpinEnd(){

    }

    public bool PlusBetClicked ()
    {
		long bet = currentBetting;
        UserManager userManager = UserManager.GetInstance ();
		userManager.UserProfile ().SetChangeBet ();
        currentBetting = userManager.NextBetting (currentBetting, userManager.UserProfile (),slotMachineConfig);
		ChangeBet ();
		if(bet == this.currentBetting) return false;
		return true;
    }

    public bool MinusBetClicked ()
    {
		long bet = currentBetting;
        UserManager userManager = UserManager.GetInstance ();
		userManager.UserProfile ().SetChangeBet ();
        currentBetting = userManager.PrevBetting (currentBetting, userManager.UserProfile (),slotMachineConfig);
		ChangeBet ();
		if(bet == this.currentBetting) return false;
		return true;
    }
		
    public void MaxBetClicked ()
    {
	    Upgrade2MaxBet();
        UserManager userManager = UserManager.GetInstance ();
		userManager.PlayBetEffect (-1);
    }
    
    public void Upgrade2MaxBet()
    {
	    List<long> betList = DataManager.GetInstance().MachineBetDatas;
	    if (betList.Count == 0) return;
	    long maxBet = betList[betList.Count - 1];
        bool isChangeBet = Mathf.Abs(currentBetting - maxBet) > 0.001f;
        if (!isChangeBet) return;
        
        currentBetting = maxBet;
        ChangeBet ();
    }
    
//现在是否是最大bet
	public bool IsMaxBet()
	{
		long maxBet = GetMaxBet();
		if(maxBet == this.currentBetting) return true;
		return false;
	}
//现在是否是最小bet
	public bool IsMinBet()
	{
		UserManager userManager = UserManager.GetInstance ();
		userManager.UserProfile ().SetChangeBet ();
		long bet = userManager.PrevBetting (currentBetting, userManager.UserProfile (),slotMachineConfig,false);
		if(bet == this.currentBetting) return true;
		return false;
	}
//当前最大bet是否等于最小bet
	public bool MaxIsMin()
	{
		UserManager userManager = UserManager.GetInstance ();
		long minBet = userManager.PrevBetting (0, userManager.UserProfile (),slotMachineConfig);
		long maxBet = GetMaxBet();
		return maxBet == minBet;
	}

	public long GetMaxBet()
    {
	    UserManager userManager = UserManager.GetInstance ();
	    return userManager.MaximumBetting (userManager.UserProfile ().Level ());
    }
    private void ChangeBet ()
    {
		Messenger.Broadcast<long> (SlotControllerConstants.OnBetChange, currentBetting);
        if (reelManager.extraAward != null) {
            reelManager.extraAward.OnBetChanger (currentBetting);
        }
//        UserManager userManager = UserManager.GetInstance ();
        bool needChangeBet = true; //maxBetting != currentBetting;
		SharedPlayerPrefs.SetPlayerPrefsBoolValue ("BetChange", needChangeBet);
		SharedPlayerPrefs.SavePlayerPreference ();
//		Messenger.Broadcast<long> (SlotControllerConstants.OnBetChange, currentBetting);
	}

    #region JackPotSystem

    public void ChangeBet(long bet)
    {
	    currentBetting = bet;
	    ChangeBet();
    }
    

    #endregion
    
	public bool GetIsSpining()
	{
		return spinning;
	}

    public bool DoSpin ()
    {
		if(this.reelManager != null)
		{
			if (!this.reelManager.IsStartSpin())
			{
				return false;
			}
		}

		if (spinning||isFreespinSupend) {
            return false;
		}
        Messenger.Broadcast(SpinButtonStyle.SOPTSPININFREE);
        if (UserManager.GetInstance ().UserProfile ().Balance () < currentBetting && (!isFreeRun)) {
            // Libs.AudioEntity.Instance.PlayClickEffect ();
			// if (HSAccountMgr.Instance.CheckAccountValidAndNetwork() && ReliefCoinsManager.Instance.IsMeetTheConditions())
			// {
			// 	UIManager.ShowLoadingUI(5f);//TODO 阻塞后续弹出  层级移植后需要修改
			// 	HSAccountMgr.Instance.OutOfCoinsEventTriggerRequest(() => { OutOfCoinsCallback(); });
			// }
			// else
			// {
			// 	OutOfCoinsCallback();
			// }
			return false;
		} 
		else if(!isFreeRun)
		{
			//非FreeSpin状态下进行的计算，原有的统计计算存在漏洞(未经过FreeSpin过滤)，考虑其他像OnceMore等特定奖励转会不会执行此方法
			MachineSpinTime++;
			currentLevelSpinCount++;
			DataManager.GetInstance().DataMining.AddSpinTime(slotMachineConfig.Name());
			SendSlotAutopilotLog(GameConstants.Spin_Key);
			CoinsIn += currentBetting;
			UserManager.GetInstance().UserProfile().AllSpinTimes += MachineSpinTime;
		}

		Messenger.Broadcast (GameConstants.DO_SPIN);
		// Messenger.Broadcast (GameDialogManager.HIDE_JACKPOT_BET_CHANGE_DIALOG);
		return reelManager.StartRun ();
    }

	public void OutOfCoinsCallback()
	{
		//TODO层级移植后需要修改
		//UIManager.HideLoadingUI();
		CommandTriggerManager.Instance.CheckMomentConditions(GameConstants.OUTOF_COINS);

		SendSlotAutopilotLog(GameConstants.OutOfCoins_Key);
		Messenger.Broadcast(GameConstants.SPINNOMONEY);
		Messenger.Broadcast<bool>(SlotControllerConstants.ActiveButtons, false);

		//Messenger.Broadcast<int> (GameDialogManager.OpenPiggy,2);
		if (!UserManager.GetInstance().UserProfile().HasSendAllSpinTimes)
		{
			int allTimes = UserManager.GetInstance().UserProfile().AllSpinTimes + MachineSpinTime;
			UserManager.GetInstance().UserProfile().AllSpinTimes = allTimes;

			Dictionary<string, object> analyticsParameters = new Dictionary<string, object>();

			analyticsParameters[SlotControllerConstants.FlurryOnSpinKey] = allTimes;
			Analytics.GetInstance().LogEvent(Analytics.SendAllSpinTimeNoMoney, analyticsParameters);
		}

		if (!UserManager.GetInstance().UserProfile().HasSendFirstNoMoneySessionTime)
		{
			Analytics.GetInstance().LogEvent(Analytics.FirstNoMoneyGameSession, SlotControllerConstants.FlurryNoMonedyKey, UserManager.GetInstance().UserProfile().CurrentGameTime());
			UserManager.GetInstance().UserProfile().HasSendFirstNoMoneySessionTime = true;
		}

		BaseGameConsole.ActiveGameConsole().LogBaseEvent(Analytics.OutOfCoins);
	}




	public void ForAutoSpin ()
    {
	    GoldGameConfig config = this.reelManager.GetComponent<GoldGameConfig>(); //FindObjectOfType<GoldGameConfig>();
	    if (config == null)
	    {
		    Debug.LogError("GoldGameConfig 为空");
	    }
		if (reelManager.AutoRun|| isFreeRun) {
            if (isBigWin || isEpicWin || isMegaWin) {
//                StartCoroutine (Delay (reelManager.gameConfigs.TileAnimationDuration , AutoRun));
				//classic 1.1.2 ，时间暂时固定了
				float time = config == null ? 1f : config.isBigWinWaitTime;
				StartCoroutine (Delay (time , AutoRun));
            } else {
                // 判定当前中奖延迟
                if (reelManager.resultContent.awardResult.isAwarded) {
                    //StartCoroutine (Delay (reelManager.gameConfigs.TileAnimationDuration, AutoRun));
					float time = config == null ? 1f : config.isAwardedWaitTime;
					StartCoroutine (Delay (time, AutoRun));//会影响freespin弹次数框，暂停的时间长度，因为上面的Delay方法为1s+1s，所以再此也要对其进行缩短
                } else {
                    AutoRun ();
                }
            }
        }
    }

    private void AutoRun ()
    {
        DoSpin ();
    }

    public double GetBetWeight ()
    {
        if (reelManager.isFreespinBonus) {
			return  freespinGame.multiplier * currentBettingTemp / reelManager.gameConfigs.ScalFactor;
        } else {
			return currentBettingTemp / reelManager.gameConfigs.ScalFactor;
        }
	}

	public const string BET_KEY = "bet";
	public const string Balance_KEY = "balance";
	public const string Machine_KEY = "machine";
	public const string KEY_TIME_STAMP = "timestamp";
	public const string KEY_EVENTS = "events";
	public const string KEY_TOURNAMENT = "tournament";
	public const string IS_POWER_MACHINE_KEY = "is_power_machine";
	public const string COLLECTION_ID_KEY = "collection_id";
	public const string MAX_BET_KEY = "max_bet";
	public const string UID_KEY = "uid";
	double currentBetting_X;
	string ADWinconsReson = string.Empty;
    Dictionary<string, string> AdsToESDic = new Dictionary<string, string>();
    public void SetADWinconsReson(string reason, string ToEsName)
	{
        ToEsName += "_Ads";
        if (AdsToESDic.ContainsKey(ToEsName))
        {
            AdsToESDic[ToEsName] = reason;
            return;
        }
        AdsToESDic.Add(ToEsName, reason);
	}

	/// <summary>
    /// 检查所中的jackpot是否有效
    /// </summary>
    /// <returns></returns>

	[Obsolete("方法即将移除，请勿在此添加任何逻辑")]
    public virtual void CheckResult ()
	{
		double spinTotalAward = 0;
		//检查结果前，将所有的悬念动画停止掉
		Messenger.Broadcast (SlotControllerConstants.STOP_ANTICIPATION_EFFECT);
		currentBetting_X = (double)1 / currentBetting;
		reelManager.extraAward?.GetAwardResult ();
        AwardResult.CurrentResultBet = GetBetWeight ();
		reelManager.CheckAward (slotMachineConfig.wildAccumulation,slotMachineConfig.extroInfos.infos);

		reelManager.resultContent.awardResult.awardValue = Math.Round(reelManager.resultContent.awardResult.awardValue * currentBettingTemp / reelManager.gameConfigs.ScalFactor);
		totalAward = reelManager.resultContent.awardResult.awardValue;
		baseAward = Math.Round(baseAward * currentBettingTemp / reelManager.gameConfigs.ScalFactor);
		
		// Debug.LogAssertion("min.wang : baseAward : "+baseAward+" total : "+totalAward);
		if (reelManager.extraAward != null) {
			totalAward += reelManager.extraAward.AwardInfo.awardValue;
        }

        if (reelManager.isFreespinBonus) {
            totalAward *= freespinGame.multiplier;
            baseAward *= freespinGame.multiplier;
            freespinGame.AwardInfo.awardValue += totalAward;
			reelManager.resultContent.awardResult.ChangedAwardValue (freespinGame.multiplier);
			SpinClickedMsgMgr.Instance.SetMultiplier(freespinGame.multiplier);
        }
		reelManager.PostCheckAwardEvent ();
		if (freespinGame != null) {
			freespinGame.AddAwardAnimation (reelManager.resultContent.awardResult,reelManager);
        }

		if (reelManager.HasBonusGame &&reelManager.BonusGame!=null &&reelManager.BonusGame.AwardInfo!=null ) {
			double bonusAward = 0;
			if (reelManager.BonusGame.IsNewResultMode)
			{
				bonusAward = reelManager.BonusGame.AwardInfo.awardValue;
			}
			else
			{
				bonusAward= reelManager.BonusGame.AwardInfo.awardValue * currentBettingTemp;
			}
			totalAward += bonusAward;
			if (bonusAward > 0)
				SpinClickedMsgMgr.Instance.AddWinDictData(SpinClickedMsgMgr.BonusWin_Key, bonusAward * currentBetting_X);

#if UNITY_EDITOR
            Debug.Log("bonusAward:" + bonusAward + "currentBetting_X" + currentBetting_X);
#endif
            reelManager.BonusGame.AddAwardAnimation (reelManager.resultContent.awardResult,reelManager);
        }
        onceMore = reelManager.EnableOnceMore();
        reelManager.NextTimeOnceMore = onceMore;
		SpinClickedMsgMgr.Instance.SetIsRespin(onceMore);

        spinTotalAward = 0;
        isEpicWin = false;
		isMegaWin = false;
		isBigWin = false;
		spinTotalAward = totalAward;

		if (reelManager.IsShowBigWin())
		{
			if(spinTotalAward >= reelManager.gameConfigs.EpicWinTag * currentBettingTemp) isEpicWin = true;
			else if(spinTotalAward >= reelManager.gameConfigs.MegaWinTag * currentBettingTemp) isMegaWin = true;
			else if(spinTotalAward >= reelManager.gameConfigs.BigWinTag * currentBettingTemp) isBigWin = true;
		}
		
        //当次Spin计算完奖励之后的广播事件
		Messenger.Broadcast<ReelManager,long>(GameConstants.SpinAwardEndMsg,reelManager,Utils.Utilities.CastValueLong(Math.Round(spinTotalAward)));
		

#if DEBUG || UNITY_EDITOR
		// Debug环境机器触发Feature时，上报ES，方便测试
		CheckMachineTaskFeatureES();
#endif
		

//        hasAnticipation = reelManager.CheckAnticipation();
		if (addLastWinCoinsToDisplay || isFreeRun||(lastOnceMore &&(!onceMore))) {
			winCoinsForDisplay += Utils.Utilities.CastValueLong( Math.Round(totalAward));
		} else {
			winCoinsForDisplay = Utils.Utilities.CastValueLong( Math.Round(totalAward));
		}

		if (isBigWin || isMegaWin || isEpicWin) {
			SequencePaidResultsManager.RequestPatterEvent (SequencePaidResultsManager.BIG_WIN);
		}

		//收集奖励
		if (spinTotalAward > 0)
			SpinClickedMsgMgr.Instance.AddWinDictData(SpinClickedMsgMgr.TotalWin_Key,spinTotalAward * currentBetting_X);
		if (isReturnFromFreespin) {
			List<BaseElementPanel> freespinSymbols = reelManager.GetFreespinSymbols ();
            if(freespinSymbols.Count > 0)
			    reelManager.resultContent.awardResult.AddAwardPayLine (freespinGame.AwardInfo.awardValue / GetBetWeight (), "TotalAwardInfreespin", freespinSymbols, reelManager.gameConfigs, -2);

			if (freespinGame.AwardInfo.awardValue > 0)
				SpinClickedMsgMgr.Instance.AddWinDictData(SpinClickedMsgMgr.FreespinWin_Key,freespinGame.AwardInfo.awardValue * currentBetting_X);
		}

		if(spinTotalAward>0){

			#region Restore FreeSpin
			if (reelManager.FreespinGame!=null) {
				reelManager.FreespinGame.currentSpinFinished = true;
			}
			#endregion
			UserManager.GetInstance ().IncreaseBalance (Utils.Utilities.CastValueLong (Math.Round (spinTotalAward)));
			#region Statistics RTP
			if (BaseGameConsole.ActiveGameConsole ().IsInSlotMachine () && spinTotalAward > 0) {
				BaseSlotMachineController slot = BaseGameConsole.ActiveGameConsole ().SlotMachineController;
				slot.statisticsManager.WinCoinsNum += spinTotalAward;
			}
			#endregion
		}

		this.SendSpinEvent(); //发送SpinClick事件
		// 尝试进行Enroll
		Messenger.Broadcast (SlotControllerConstants.TOURNAMENT_SPIN_ENROLL_MSG);
		Messenger.Broadcast (SlotControllerConstants.EndCheckResult);//必须放在此方法的最后 
    }

	public void SendSpinEvent(int sourceType=0)
	{
		// Dictionary<string,object> spinEvent = new Dictionary<string, object> ();
		// string spinEventName = SendLogEvent (ref spinEvent,sourceType);
		// if (string.IsNullOrEmpty(spinEventName))
		// {
		// 	return;
		// }
		// BaseGameConsole.singletonInstance.AddESParameters (spinEvent);
		//
		// List<Dictionary<string,object>> spinEventlist = new List<Dictionary<string,object>> ();
		// spinEventlist.Add (spinEvent);
		//
		// if(Application.internetReachability == NetworkReachability.NotReachable)
		// {
		// }
		// else
		// {
		// 	Dictionary<string,object> dict = new Dictionary<string, object>();
		// 	dict[KEY_EVENTS] = spinEventlist;
		// 	dict[BET_KEY] = currentBettingTemp;
		// 	dict[Machine_KEY] = SwapSceneManager.Instance.GetLogicSceneName();
		// 	dict[Balance_KEY] = UserManager.GetInstance().UserProfile().Balance();
		// 	dict[KEY_TIME_STAMP] = TimeUtils.ConvertDateMillTimeLong(System.DateTime.Now);
		// 	dict[MAX_BET_KEY] = GetMaxBet();
		// 	
  //           AddExtraToSpinData(dict);
  //           
  //           // 重置通用JP数据
  //           if(IsJackpotWin)
		// 		ResetJackpotData();
		// }

		long currentBalance = UserManager.GetInstance ().UserProfile ().Balance ();
		long maxBalance = UserManager.GetInstance ().MaxBalance;
		UserManager.GetInstance ().MaxBalance =currentBalance > maxBalance ? currentBalance : maxBalance;
		
		Messenger.Broadcast(SlotControllerConstants.SendSpinEvent);
	}

	public void AddExtraToSpinData(Dictionary<string, object> dict)
	{
		if(dict == null)
			return;
	}

	public bool mIsUseRemoteR = false;
	/// <summary>
	/// 移动此方法调用位置要谨慎，防止参数不起作用
	/// </summary>
	string SendLogEvent(ref Dictionary<string,object> spinEvent,int sourceType = 0)
	{
		string result = "";

		if (!string.IsNullOrEmpty(ADWinconsReson))
		{
			//			Debug.Log ("---------- : "+ADWinconsReson);
			spinEvent["adwinCoinsReason"] = ADWinconsReson;
			ADWinconsReson=string.Empty;
		}
		hasAnticipation = reelManager.CheckAnticipation();
		spinEvent.Add ("Anticipation",hasAnticipation?1:0);
		spinEvent.Add ("IsEpicWin",isEpicWin?1:0);
		spinEvent.Add ("IsMegaWin",isMegaWin?1:0);
		spinEvent.Add ("IsBigWin",isBigWin?1:0);
		spinEvent.Add ("IsUsePattern",reelManager.GetUsePattern()?1:0);
		spinEvent.Add ("BetDiffence",currentBetting-preBettingForEventLog);
		spinEvent.Add ("FeatureInUse",SpinClickedMsgMgr.Instance.FeatureInUse);
		spinEvent.Add ("MPlist",slotMachineConfig.CurSlotPlistFileName);
		spinEvent.Add ("reelspeed",reelManager.ClientV);
		// if(AutopilotManager.GetInstance().IsCheckNoWinBySlotName(slotMachineConfig.Name()))
		// {
		// 	spinEvent.Add ("NoWinRequestPossibility",slotMachineConfig.NoWinRequestPossibility);
		// }
			
		preBettingForEventLog = currentBetting;
		Dictionary<string, object> featureDict = new Dictionary<string, object>();
		
		if (IsJackpotWin)//通用jackpot
		{
			featureDict["jackpotName"] = JackpotName;
		}
		

		if (onceMore && lastOnceMore)
		{
			featureDict["RespinType"] = "InRespin";
		}

		if (!lastOnceMore && (onceMore))
		{
			featureDict["RespinType"] = "TriggerRespin";
		}
		
		if (mIsUseRemoteR)
		{
			spinEvent.Add ("UseRemoteR",mIsUseRemoteR);
			mIsUseRemoteR = false;
		}

		if (UserManager.GetInstance().UserProfile().Balance() < currentBetting && (!isFreeRun))
		{
			spinEvent.Add("IsOutofCoin", 1);
		}
		else
		{
			spinEvent.Add("IsOutofCoin", 0);
		}
		spinEvent.Add("Coin_divise_Bet", (long)(UserManager.GetInstance().UserProfile().Balance() / currentBetting));
		
		//方便op pm统计数据,非fs中才有
		if(!isReturnFromFreespin)
		{
			if (reelManager.HasBonusGame && reelManager.GetBonusAward() > 0)
			{
				featureDict["BonusGameAwardValue"] = reelManager.GetBonusAward();
			}

			if (!onceMore && lastOnceMore && reelManager.LinkAwardForEs() > 0)
			{
				featureDict["RespinGameAwardValue"] = reelManager.LinkAwardForEs();
			}
		}
		
		var maxBet = GetMaxBet();
		if (maxBet > 0)
		{
			double betPercent = Math.Round((double)currentBetting / maxBet, 4);
			spinEvent.Add("CurrentBetPercent", betPercent);
		}

		long wincoins = 0;
		if (isReturnFromFreespin&&!onceMore)
		{
			if (RewardSpinManager.Instance.RewardIsValid() || RewardSpinManager.Instance.IsLastRewardSpin())
			{
				long curentWinCoins = winCoinsForDisplay - RewardSpinWinCoinsByLast;
				RewardSpinWinCoinsByLast = winCoinsForDisplay;
				spinEvent.Add ("increaseCoins", curentWinCoins);
				spinEvent.Add ("WinCoins", curentWinCoins);
				wincoins = curentWinCoins;
                AddOutComeEncodetoDict(spinEvent);
//				BaseGameConsole.ActiveGameConsole ().LogBaseEvent (Analytics.FreeSpinReward, spinEvent);
				result = Analytics.FreeSpinReward;
			}
			else
			{
				spinEvent.Add ("increaseCoins", winCoinsForDisplay);
				spinEvent.Add ("WinCoins", winCoinsForDisplay);
				wincoins = winCoinsForDisplay;
				featureDict["freespinNumber"] = freespinGame.TotalTime;
				featureDict["freespinAwardValue"] = freespinGame.AwardInfo.awardValue ;
                AddOutComeEncodetoDict(spinEvent);
				CoinsIn -= winCoinsForDisplay;
//				BaseGameConsole.ActiveGameConsole ().LogBaseEvent (Analytics.SpinClicked, spinEvent);
				result = Analytics.SpinClicked;
			}
		}
		else if (RewardSpinManager.Instance.RewardSpinIsValid() || RewardSpinManager.Instance.IsLastRewardSpin()) 
		{
			long curentWinCoins = winCoinsForDisplay - RewardSpinWinCoinsByLast;
			RewardSpinWinCoinsByLast = winCoinsForDisplay;
			spinEvent.Add ("increaseCoins", curentWinCoins);
			spinEvent.Add ("WinCoins", curentWinCoins);
			wincoins = curentWinCoins;
			AddOutComeEncodetoDict(spinEvent);
			result= Analytics.FreeSpinReward;
		}
		else if (!isFreeRun || (lastOnceMore && (!onceMore)))
		{
			if (!reelManager.isFreespinBonus)
			{
				spinEvent.Add ("increaseCoins", winCoinsForDisplay);
				spinEvent.Add ("WinCoins", winCoinsForDisplay);
				wincoins = winCoinsForDisplay;
				AddOutComeEncodetoDict(spinEvent);
				CoinsIn -= winCoinsForDisplay;
//					BaseGameConsole.ActiveGameConsole ().LogBaseEvent (Analytics.SpinClicked, spinEvent);
				result = Analytics.SpinClicked;
			}
		}
		else 
		{
			spinEvent.Add ("increaseCoins", totalAward);
			spinEvent.Add ("WinCoins", totalAward);
			wincoins = (long)totalAward;
//			BaseGameConsole.ActiveGameConsole ().LogBaseEvent (Analytics.RespinClicked, spinEvent);
			result= Analytics.RespinClicked;
		}
		spinEvent["Feature"] = featureDict;

		spinEvent["spin_rtp"] = (wincoins * 1.0f - reelManager.JackPotIncreaseValue) / currentBetting;
		reelManager.JackPotIncreaseValue = 0;
		
		spinEvent["coins_hold"] = wincoins  -(long) currentBetting;
		
		//rule相关
		if (!string.IsNullOrEmpty(RulePatternManager.GetInstance().RuleId))
		{
			spinEvent["RuleId"] = RulePatternManager.GetInstance().RuleId;
			spinEvent["Round"] = RulePatternManager.GetInstance().Round;
			
			if (this.RuleConfig != null)
			{
				if(this.RuleConfig.IsInUseServerConfig())
				{
					spinEvent["RCStatus"] = 1;
				}
				else
				{
					spinEvent["RCStatus"] = 0;
				}
			}
		}
		//pattern
		
		if(reelManager.PatternEsType != -1)
		{
			spinEvent["putype"] = reelManager.PatternEsType;
			Log.LogWhiteColor(spinEvent["putype"]);
				
		}

		if (!string.IsNullOrEmpty(reelManager.PatternEsId))
		{
			spinEvent["puid"] = reelManager.PatternEsId;
			Log.LogWhiteColor(spinEvent["puid"]);
		}
		spinEvent["IsCollect"] = sourceType;
		spinEvent["SpinStatus"] = GetSpinStatus();
		
		return result;
	}

	// 获取Spin状态，状态之间用半角逗号隔开
	public string GetSpinStatus()
	{
		string status = "";

		// 去掉末尾的逗号
		if (!string.IsNullOrEmpty(status))
			status = status.Substring(0, status.Length - 1);
		return status;
	}

    private void AddOutComeEncodetoDict(Dictionary<string,object> spinEvent)
    {
        if (spinEvent == null) return;
        spinEvent.Add ("outComeEncode", SpinClickedMsgMgr.Instance.GetEnCodeString());
    }

    #region Animation

    protected ActionSequence animationAction = new ActionSequence();
    BaseActionNormal extraAnimationAction = new BaseActionNormal();
    BaseActionNormal normalAnimationStartAction = new BaseActionNormal();
	BaseActionNormal jackpotAnimationAction = new BaseActionNormal();
	BaseActionNormal spinWinAnimationAction = new BaseActionNormal();
    BaseActionNormal blackOutAnimationAction = new BaseActionNormal();
	BaseActionNormal missionProgressAnimationAction = new BaseActionNormal();
    BaseActionNormal autoQuestTermDoneAnimationAction = new BaseActionNormal();
    BaseActionNormal allTitleAnimationAction = new BaseActionNormal();
    BaseActionNormal awardSymbolsAnimationAction = new BaseActionNormal();
    BaseActionNormal normalAnimationEndAction = new BaseActionNormal();
	BaseActionNormal payLineWholeAction = new BaseActionNormal(); 
	BaseActionNormal extraRewardCashAnimationAction = new BaseActionNormal();

	protected virtual void InitAnimationActions()
	{
        extraAnimationAction.AutoPlayNextAction = false;
        extraAnimationAction.PlayCallBack.AddStartMethod (PlayExtraAwardAnimation);

        normalAnimationStartAction.AutoPlayNextAction = false;
        normalAnimationStartAction.PlayCallBack.AddStartMethod (PlayNormalAwardAnimation);

        extraRewardCashAnimationAction.AutoPlayNextAction = false;
        extraRewardCashAnimationAction.PlayCallBack.AddStartMethod (PlayExtraRewardCashAnimation);
        
		jackpotAnimationAction.AutoPlayNextAction = false;
		jackpotAnimationAction.PlayCallBack.AddStartMethod (PlayJackpotAnimation);

		spinWinAnimationAction.AutoPlayNextAction = false; 
        spinWinAnimationAction.PlayCallBack.AddStartMethod (PlaySpinWinAnimation);

        blackOutAnimationAction.AutoPlayNextAction = false;
        blackOutAnimationAction.PlayCallBack.AddStartMethod (ShowWinBlackOutDialog);

		missionProgressAnimationAction.AutoPlayNextAction = false;
		missionProgressAnimationAction.PlayCallBack.AddStartMethod (OpenMissionProgressDialog);

        autoQuestTermDoneAnimationAction.AutoPlayNextAction = false;
        autoQuestTermDoneAnimationAction.PlayCallBack.AddStartMethod(PlayAutoQuestTermDoneAnimation);

        allTitleAnimationAction.AutoPlayNextAction = false;
        allTitleAnimationAction.PlayCallBack.AddStartMethod (PlayAllTitleAnimaton);

        awardSymbolsAnimationAction.AutoPlayNextAction = false;
        awardSymbolsAnimationAction.PlayCallBack.AddStartMethod (PlayAwardSymbolAnimation);

		payLineWholeAction.AutoPlayNextAction = false;
		payLineWholeAction.PlayCallBack.AddStartMethod (PlayWholePayLineAnimation);

		normalAnimationEndAction.AutoPlayNextAction = false;
        normalAnimationEndAction.PlayCallBack.AddStartMethod (NormalAnimationEndHandler);
    }

	protected void OpenMissionProgressDialog(){
		if (needShowMissionProgress) {
			Messenger.Broadcast<int> (GameDialogManager.OpenMissionProgressDialog, missionProgressShowNum);
			needShowMissionProgress = false;
		}
		animationAction.PlayNextChildAction ();

	}

    protected virtual void InitAnimationSequence(){
        animationAction.RemoveAll ();
        if (reelManager.extraAward != null)
		{
            animationAction.AppendAction (extraAnimationAction);
        }

		animationAction.AppendAction (normalAnimationStartAction);
		animationAction.AppendAction (extraRewardCashAnimationAction);

        if (reelManager.resultContent.awardResult.isAwarded || totalAward > 0) {
			if (reelManager.resultContent.awardResult.blackAward.isAwarded) {
				animationAction.AppendAction (blackOutAnimationAction);
			}

			if (reelManager.gameConfigs.ShowAllAnimation && reelManager.resultContent.awardResult.awardValue > reelManager.gameConfigs.ShowAllAnimationTag * currentBettingTemp) {
				animationAction.AppendAction (allTitleAnimationAction);
			}

			if ((isEpicWin || isMegaWin || isBigWin) && !temporaryHideBigWin && totalAward >0) {
				animationAction.AppendAction (spinWinAnimationAction);
			}
			
			if (needShowMissionProgress) {
				animationAction.AppendAction (missionProgressAnimationAction);
			}

            if (BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestJustFinishedTerm != null &&
                BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestTermJustFinished != null &&
                !this.isFreeRun) {
                animationAction.AppendAction(autoQuestTermDoneAnimationAction);
            }

			if (reelManager.HasAwardSymbol()) {
				if (reelManager.gameConfigs.ShowSpaghetti && reelManager.awardLines.Count > 1) {
					animationAction.AppendAction (payLineWholeAction);
				}
				animationAction.AppendAction (awardSymbolsAnimationAction);
			}

		} else {
			if (needShowMissionProgress) {
				animationAction.AppendAction (missionProgressAnimationAction);
			}
			if (reelManager.AutoRun) {
                //reelManager.ChangeMiddleMessage ("AUTO SPIN");
			} else {
                if (BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestJustFinishedTerm != null &&
                    BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestTermJustFinished != null &&
                    !this.isFreeRun) {
                    animationAction.AppendAction(autoQuestTermDoneAnimationAction);
                }
			}
		}

        animationAction.AppendAction (normalAnimationEndAction);
    }

    public void PlayAnimation()
	{
        reelManager.AddAwardElements();
        InitAnimationSequence ();
        animationAction.Play ();
    }

    private void PlayExtraAwardAnimation()
	{
        if (reelManager.extraAward != null) 
		{
            reelManager.extraAward.PlayAnimation ();
        } else 
		{
            animationAction.PlayNextChildAction();
        }
    }

    protected float winAudio =0f;
	public bool changWinText = true;

	//播放正常奖励的动画
    private void PlayNormalAwardAnimation()
    {
		if (reelManager.HasAwardSymbol() && (totalAward > 0  || (isReturnFromFreespin && winCoinsForDisplay > 0))) {
            reelManager.DisableAllUnAwardSymbol ();
        }
        if ((totalAward >= BigAnimationTag * currentBettingTemp) || (isReturnFromFreespin))
        {
            reelManager.PlayBigAnimation();
        }
        else if (totalAward >= MiddleAnimationTag * currentBettingTemp)
        {
            reelManager.PlayMiddleWinAnimation();
        }
        else if (totalAward >= SmallAnimationTag * currentBettingTemp)
        {
            reelManager.PlaySmallWinAnimation();
        }
        animationAction.PlayNextChildAction();

        PlayWinCoinsAnimationWithDelay(isBigWin == false && reelManager.HasAwardSymbol() && winAudio > 0 ? winAudio : 0);
	}

	private void ShowWinBlackOutDialog() 
	{
		reelManager.OpenBlackoutDialog(animationAction.PlayNextChildAction);
    }

	public virtual void PlayExtraRewardCashAnimation()
	{
		//当前有任何奖励弹版的时候，都不进入action
		if (hasPopReward||isBigWin||isMegaWin||isEpicWin||reelManager.HitFs||reelManager.isFreespinBonus||reelManager.HasBonusGame)
		{
			OnLineEarningMgr.Instance.ResetRewardCount();
			animationAction.PlayNextChildAction();
			return;
		}

		if (!OnLineEarningMgr.Instance.CheckShowRewardCash())
		{
			animationAction.PlayNextChildAction();
			return;
		}
		OnLineEarningMgr.Instance.ShowExtraRewardCashDialog(animationAction.PlayNextChildAction);
	}
	
    protected virtual void PlayJackpotAnimation()
    {
        long totalWin = Utils.Utilities.CastValueLong(Math.Round(totalAward));
    }

    protected virtual void PlaySpinWinAnimation()
    {
        SpinWinType type = SpinWinType.NONE;

        Messenger.Broadcast(GameConstants.OPEN_SPIN_WIN_DIALOG);

        if (isEpicWin) type = SpinWinType.EPIC; Messenger.Broadcast(GameConstants.OPEN_EPIC_WIN_DIALOG);
        if (isMegaWin) type = SpinWinType.MEGA; Messenger.Broadcast(GameConstants.OPEN_MEGA_WIN_DIALOG);
        if (isBigWin) type = SpinWinType.BIG; Messenger.Broadcast(GameConstants.OPEN_BIG_WIN_DIALOG);

		new Libs.DelayAction(0.5f, null, ()=>{OpenSpinWinDialog(type);}).Play();
    }

	private void OpenSpinWinDialog(SpinWinType type)
	{
		if (!reelManager.IsShowBigWin())
		{
			// reelManager.PauseBackGroundAudio(true);
			if (BaseSlotMachineController.Instance == null) return;
            animationAction.PlayNextChildAction();
			return;
		}
		
		long totalWin = Utils.Utilities.CastValueLong(Math.Round(totalAward));
		
		OpenSpinWinDialog(totalWin,type,() => 
		{
			reelManager.SetBackGroundAudio(1f);
			if (BaseSlotMachineController.Instance == null) return;
			animationAction.PlayNextChildAction();
		});
	}

    public void OpenWinEffectDialog(double v, Action callBack)
    {
        SpinWinType type = SpinWinType.NONE;
        if (v >= reelManager.gameConfigs.EpicWinTag * currentBetting)
        {
            type = SpinWinType.EPIC;
            Messenger.Broadcast(GameConstants.OPEN_EPIC_WIN_DIALOG);
        }
        else if (v >= reelManager.gameConfigs.MegaWinTag * currentBetting)
        {
            type = SpinWinType.MEGA;
            Messenger.Broadcast(GameConstants.OPEN_MEGA_WIN_DIALOG);
        }
        else if (v >= reelManager.gameConfigs.BigWinTag * currentBetting)
        {
            type = SpinWinType.BIG;
            Messenger.Broadcast(GameConstants.OPEN_BIG_WIN_DIALOG);
        }
        if(type == SpinWinType.NONE)
        {
            if (callBack != null) callBack();
            return;
        }
        long totalWin = Utils.Utilities.CastValueLong(Math.Round(v));
        OpenSpinWinDialog(totalWin,type,() =>
        {
	        if (BaseSlotMachineController.Instance == null) return;
	        //if (UserManager.GetInstance().EnableRateUsAlert() && !this.isFreeRun) Messenger.Broadcast(GameDialogManager.OpenRateUsAlert);
	        if(callBack!=null)
	        {
		        callBack();
	        }
        });
    }

    private void OpenCommonJackPotEnterDialog<T>(Action callback) where T : UIDialog
    {
	    UIManager.Instance.OpenSystemDialog(new OpenConfigParam<T>(openType: OpenType.Normal, dialogCloseCallBack: () =>
	    {
		    if (BaseSlotMachineController.Instance == null) return;
		    if (callback != null) callback();
		    animationAction.PlayNextChildAction();
	    }, animationIn: UIAnimation.NOAnimation, animationOut: UIAnimation.NOAnimation, maskAlpha: 0f));
    }
    private void OpenAnimation<T>(long awardNumber, System.Action closeCallback = null, string dialogPath = null) where T : UIDialog
    {
	    Libs.AudioEntity.Instance.StopAllAudio();//打开之前关闭所有音效
	    UIManager.Instance.OpenSystemDialog(new OpenConfigParam<T>(openType: OpenType.Normal, dialogInitCallBack:
		    (dialog) => { dialog.SetData(awardNumber); }, dialogCloseCallBack: () =>
		    {
			    if (BaseSlotMachineController.Instance == null) return;
			    if (closeCallback != null) closeCallback();
			    animationAction.PlayNextChildAction();
		    }, animationIn: UIAnimation.NOAnimation, animationOut: UIAnimation.NOAnimation, maskAlpha: 0.1f,
		    defaultResourcePath: dialogPath));
    }

    public void OpenSpinWinDialog(long totalWin,SpinWinType type,Action closeCallBack,Action initCallBack = null)
    {
	    bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
	                      SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
	    OpenConfigParam<SpinWinDialog> param = new OpenConfigParam<SpinWinDialog>(isPortrait,0,OpenType.Normal,"",new MachineUIPopupStrategy(),
		    (dialog) =>
		    {
			    reelManager.SetBackGroundAudio(1f,0.3f);
			    dialog.OnStart(totalWin, type);
			    initCallBack?.Invoke();
		    }, closeCallBack, null, UIAnimation.Scale, UIAnimation.Scale, 0.8f);
	    UIManager.Instance.OpenSystemDialog(param);
    }

    protected virtual void PlayAutoQuestTermDoneAnimation() {
        if (BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestJustFinishedTerm != null &&
            BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestTermJustFinished != null &&
            !this.isFreeRun) {
            Dictionary<string, object> openAutoQuestDetailDialogParameters = new Dictionary<string, object>();
            openAutoQuestDetailDialogParameters["OpenAsModel"] = true;
            openAutoQuestDetailDialogParameters["Quest"] = BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestJustFinishedTerm;
            openAutoQuestDetailDialogParameters["QuestTerm"] = BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestTermJustFinished;

            System.Action ShowCurrentProcessingQuestDetailDialog = () => {
                BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestJustFinishedTerm = null;
                BaseGameConsole.ActiveGameConsole().autoQuestManager.QuestTermJustFinished = null;

                (new Libs.DelayAction(0.25f, null, () => {
                    Messenger.Broadcast(GameConstants.AutoQuestDetailDialogWillOpenForNewQuest);
                    Dictionary<string, object> openCurrentProcessingDialogParameters = new Dictionary<string, object>();
                    openCurrentProcessingDialogParameters["OpenAsModel"] = true;
                    Messenger.Broadcast<Dictionary<string, object>>(GameDialogManager.OpenAutoQuestDetailDialog, openCurrentProcessingDialogParameters);
                })).Play();
            };
            openAutoQuestDetailDialogParameters["ActionOnClose"] = ShowCurrentProcessingQuestDetailDialog;

            Messenger.Broadcast<Dictionary<string, object>>(GameDialogManager.OpenAutoQuestDetailDialog, openAutoQuestDetailDialogParameters);
        }

        animationAction.PlayNextChildAction();
    }

    void PlayWinCoinsAnimation ()
    { 
		Messenger.Broadcast (SlotControllerConstants.OnBlanceChangeForDisPlay);
		Messenger.Broadcast (SlotControllerConstants.OnCashChangeForDisPlay);
    }

    void PlayAllTitleAnimaton(){
        Libs.DelayAction delayAction = new Libs.DelayAction (reelManager.gameConfigs.TileAnimationDuration, PlayAllSymbolHighAwardAnimation, animationAction.PlayNextChildAction);
        delayAction.Play ();
    }

    void PlayAllSymbolHighAwardAnimation(){
        reelManager.PlayAllSymbolHighAwardAnimation ();
    }

    void PlayAwardSymbolAnimation ()
    {
		reelManager.PlayAwardSymbolAnimation();
		//减低背景音乐音量
		// reelManager.SetBackGroundAudio(2f,0.3f);
		reelManager.PlayAwardSymbolAudio(this.LineAudioName()); //20200706将音效归入到粒子框的动画里, 07
        animationAction.PlayNextChildAction();
    }

    public void StopAwardSymbolAnimation()
    {
	    AudioEntity.Instance.StopEffectAudio(this.LineAudioName());
    }

	public string LineAudioName()
	{
		if (totalAward > 0 && totalAward <= this.currentBettingTemp) return "bet_1";
		if (totalAward > this.currentBettingTemp && totalAward < 5 * this.currentBettingTemp) return "bet_1_5";
		if (totalAward >= 5 * this.currentBettingTemp) return "bet_5";
		return "";
	}

    public void PlaySymbolAnimation ()
    {
		StartCoroutine (reelManager.PlaySymbolAnimationByLine (reelManager.gameConfigs.TileAnimationDuration));
    }

	public void PlayWholePayLineAnimation()
	{
		StartCoroutine (PayLineShowAllAnimation ());
	}
	private WaitForSeconds paylineWait = new WaitForSeconds (1.5f);
	private IEnumerator PayLineShowAllAnimation()
	{ 
		reelManager.PlayWholePaylineAnimation ();
        switch (reelManager.gameConfigs.payLineConfig.lineType)
        {
			case GameConfigs.LineType.None:
		        yield return paylineWait;
				break;
			case GameConfigs.LineType.Fade:
		        yield return new WaitForSeconds(0.1f);
				break;
            default:
                break;
        }
		animationAction.PlayNextChildAction ();
	}

	public void PlayExtraWinAnimation(){
		reelManager.PlayShineAnimation ();
		reelManager.PlayBaseWinAnimation ();
	}
	public void StopExtraWinAnimation(){
		reelManager.StopShineAnimation ();
		reelManager.StopBaseWinAnimation ();
	}
    public void StopAllAnimation ()
    {
        StopAllCoroutines ();
		reelManager.resultContent.awardResult.Reset ();
        reelManager.StopAllAnimation ();
        reelManager.ClearAnimationInfos ();
		StopExtraWinAnimation ();
		animationAction.Stop ();
		animationAction.RemoveAll ();
    }

    #endregion

    IEnumerator Delay (float timer, Action a)
    {
        yield return new WaitForSeconds (timer);
        a ();
    }
	public static string SUPSPEND_FREESPIN = "FreeSpin_suspend";
	public static string RESUME_FREESPIN = "FreeSpin_resume";
	public static string CALCULATE_CASH = "Calculate_cash";

	bool isFirstSpin = true;
	
	private void NormalAnimationEndHandler()
	{
		reelManager.PlayAnimationFinishHandler(OnSpinEnd);
	}
	
    public async void OnSpinEnd ()
    {
	    //旧的流程不动，新版本升级的流程放到addNormal之后
	    if (!reelManager.IsNewProcess)
	    {
		    ///LQ 此处不可以使用消息机制
		    await RewardSpinManager.Instance.CheckRewardSpinEnd();
		    
		    if (!isFreeRun)
		    {
			    if (hasPopReward)
			    {
				    ResultStateManager.Instante.AddPopReward();
			    }

			    hasPopReward = false;
		    }

		    ResultStateManager.Instante.AddResultOut();
	    }


	    if (isFirstSpin)
	    {
		    isFirstSpin = false;
		    SequencePaidResultsManager.RequestPatterEvent(SequencePaidResultsManager.ENTER);
	    }

	    ResultStateManager.Instante.PlayNext();

    }

    
    //20191230 尝试新增加最终控制的流程接口
    public void NormaEndToNextSpin()
    {
	    if(!isFreeRun)
	    {
		    CommandTriggerManager.Instance.CheckMomentConditions(GameConstants.SPIN_END);
		    Messenger.Broadcast(GameConstants.SPIN_END);
	    }
	    
	    bool auto_spin = false;
	    if (reelManager.AutoRun || isFreeRun)
	    {
		    auto_spin = true;
		    DelayAction delayAction = new DelayAction (1f, null, ForAutoSpin);
		    delayAction.Play ();
	    }
	    if (!(reelManager.AutoRun|| isFreeRun)) {
		    //此判断为解决EpicWin弹窗后Bet按钮必定变为有效的问题而设置
		    if (reelManager.enableBetChangeAfterEpicWin) {
			    Messenger.Broadcast<bool> (SlotControllerConstants.ActiveButtons,false);
		    }
	    }	
	    spinning = false;
	    
	    addLastWinCoinsToDisplay = false;
			
	    if (!(isFreeRun || GetIsSpining() || (reelManager != null && (!reelManager.IsStartSpin()))))
	    {
		    Messenger.Broadcast(GameConstants.DIALOG_CLOSE_AWARD_HANDLER);//弹下一个中奖框
	    }
    }

	private IEnumerator WaitforShowPiggyBankTips()
	{
		while (!UIManager.Instance.IsEmpty ()) {
			yield return GameConstants.OneSecondWait;
		}

		Messenger.Broadcast<bool> (GameConstants.SHOW_PIGGY_TIPS_PANEL,false);
	}

    //刷新底部金币和现金的显示
    public void HandlePlayWinCoinsAnimation()
    {
        if (totalAward > 0) 
        {
	        PlayWinCoinsAnimation ();
		}
		
	    if (extraAwardIsAccumulatedAlone()) 
	    {
		    double totalAwardExceptExtraAward = totalAward - reelManager.extraAward.AwardInfo.awardValue;
		    Messenger.Broadcast<float,long,long> (SlotControllerConstants.OnChangeWinText, 0.75f, winCoinsForDisplay - Utils.Utilities.CastValueLong(Math.Round(totalAwardExceptExtraAward)), Utils.Utilities.CastValueLong(Math.Round(totalAwardExceptExtraAward)));
	    } 
	    else if(changWinText)
	    {
		    //通知改变底部栏的金币和现金显示
		    long from = winCoinsForDisplay - Utils.Utilities.CastValueLong(Math.Round(totalAward));
		    Messenger.Broadcast<float,long,long> (SlotControllerConstants.OnChangeWinText, 0.75f,from ,Utils.Utilities.CastValueLong(Math.Round(totalAward)));
		    int cashFrom = winCashForDisplay - totalCash;
		    // Debug.Log("GetResultAward  winCashForDisplay =======" + winCashForDisplay);
		    Messenger.Broadcast<bool>(SlotControllerConstants.RefreshCashState,totalCash>0);
		    Messenger.Broadcast<float,int,int> (SlotControllerConstants.OnChangeCashText, 0.75f,cashFrom ,totalCash);
	    }
	    
        Messenger.Broadcast<double>(SlotControllerConstants.OnWinCoins, totalAward);// 调整右下角win框金币特效刷新时机
          //if (winCoinsForDisplay > 0 && (!isFreeRun)) {
          //    Messenger.Broadcast<int> (SlotControllerConstants.OnWinCoinsForDisplay, winCoinsForDisplay);
          //}
    }

    public void PlayWinCoinsAnimationWithDelay(float delay) {
        Action winCoinsAction = () => {
            HandlePlayWinCoinsAnimation();
        };
        if (Mathf.Approximately(delay, 0)) {
            winCoinsAction();
        } else {
            //Libs.SoundEntity.Instance.PlayWheelStopAndWinSoundEffect ();
           // Libs.AudioEntity.Instance.PlayWheelStopAndWinSoundEffect();
            (new Libs.DelayAction(delay,null, () => {
                winCoinsAction();
            })).Play();
        }
    }

	public void HandleCoinsShowWithoutAnimation()
	{
		Messenger.Broadcast<float,long,long,bool> (SlotControllerConstants.OnChangeWinTextEx, 1f, winCoinsForDisplay - Utils.Utilities.CastValueLong(Math.Round(totalAward)),Utils.Utilities.CastValueLong(Math.Round(totalAward)),false);

		//if (winCoinsForDisplay > 0 && (!isFreeRun)) {
		//	Messenger.Broadcast<int> (SlotControllerConstants.OnWinCoinsForDisplay, winCoinsForDisplay);
		//}
	}

	public void SetBackGroundAudio(float time,float value)
	{
		reelManager.SetBackGroundAudio(time,value);
	}
	
	public void CreateCoinBezierFrom (Transform dialogTransformVector)
	{
		Transform targetTransformVector;
		targetTransformVector = CashTransform;
        if (ToCamera == null || targetTransformVector == null) {
            return;
        }

        Vector3 from = Libs.CoinsBezier.Instance.LocalPositionFromTransForm (Libs.UIManager.Instance.UICamera, dialogTransformVector);
        Vector2 to = Libs.CoinsBezier.Instance.LocalPositionFromTransForm (ToCamera, targetTransformVector);
        Libs.CoinsBezier.Instance.Create (from, to);
	}

    public void CreateCoinBezierFrom(Transform dialogTransformVector, Libs.CoinsBezier.BezierType bezierType, System.Action callback = null)
	{
		Transform targetTransformVector;
		targetTransformVector = CashTransform;
		
		if (ToCamera == null || targetTransformVector == null) {
			return;
		}

		Vector3 from = Libs.CoinsBezier.Instance.LocalPositionFromTransForm (Libs.UIManager.Instance.UICamera, dialogTransformVector);
		Vector2 to = Libs.CoinsBezier.Instance.LocalPositionFromTransForm (ToCamera, targetTransformVector);
        Libs.CoinsBezier.Instance.Create (from, to, bezierType, callback);
	}
    
    public void CreateCoinBezierFromTo(Transform dialogTransformVector, Transform target, Libs.CoinsBezier.BezierType bezierType, System.Action callback = null)
    {
	    Transform targetTransformVector = target;

	    if (targetTransformVector == null) {
		    return;
	    }
	    Messenger.Broadcast(GameConstants.SET_BALANCE_TARGET_POSITION,targetTransformVector);

	    Vector3 from = Libs.CoinsBezier.Instance.LocalPositionFromTransForm (Libs.UIManager.Instance.UICamera, dialogTransformVector);
	    Vector2 to = Libs.CoinsBezier.Instance.LocalPositionFromTransForm (Libs.UIManager.Instance.UICamera, targetTransformVector);
	    
	    Libs.CoinsBezier.Instance.Create (from, to, bezierType, callback);
    }

    public void CreateCoinBezierFromInputMouse ( )
    {
	    Transform targetTransformVector = this.CoinsTransform;

	    if (ToCamera == null || targetTransformVector == null) {
		    return;
	    }

	    Vector3 from = Libs.CoinsBezier.Instance.LocalPositionFromInputMouse ();
	    Vector2 to = Libs.CoinsBezier.Instance.LocalPositionFromTransForm (ToCamera, targetTransformVector);
	    Libs.CoinsBezier.Instance.Create (from, to, Libs.CoinsBezier.BezierType.DailyBonus);
    }

	
	
	#region rateMachine
	private long StayStartTime = 0;

	private void BeginStay()
	{
		StayStartTime = TimeUtils.ConvertDateTimeLong (DateTime.Now);
	}
	#endregion

	#region stop autospin when pop dialog
    //是否autospin正在停止当中
    private bool _autospinSuspending = false;
    private bool _preSuspendIsAutoRun = false;
    public bool IsAutoSpinSuspending()
    {
        return _autospinSuspending;
    }
    private void AutoSpinSuspendHandle()
    {
        //		Debug.LogAssertion ("autoRun : "+BaseSlotMachineController.Instance.reelManager.AutoRun+" ---- _autospinSuspending "+_autospinSuspending);
        if (_autospinSuspending)
        {
            return;
        }
        if (BaseSlotMachineController.Instance.reelManager.AutoRun)
        {
            _autospinSuspending = true;
            BaseSlotMachineController.Instance.reelManager.AutoRun = false;
        }
    }

    private void AutoSpinResumeHandle()
    {
        if (!_autospinSuspending)
        {
            return;
        }
        _autospinSuspending = false;
        if(BaseSlotMachineController.Instance != null && BaseSlotMachineController.Instance.reelManager != null)
        {
            BaseSlotMachineController.Instance.reelManager.AutoRun = true;
            BaseSlotMachineController.Instance.DoSpin();
        }
    }

    /// <summary>
    /// 防止使用Message通知停止AutoSpin后恢复AutoSpin时状态异常
    /// </summary>
    /// <param name="autospinSuspending"></param>
    public void Set_autospinSuspendingValue(bool autospinSuspending)
    {
	    _autospinSuspending = autospinSuspending;
    }

    public void StopHandle()
    {
        // BaseSlotMachineController.Instance.reelManager.AutoRun = false;
        // _preSuspendIsAutoRun = false;
    }

    public void SuspendHandle()
    {
        //		Debug.LogAssertion ("autoRun : "+BaseSlotMachineController.Instance.reelManager.AutoRun+" ---- _preSuspendIsAutoRun "+_preSuspendIsAutoRun);
        if (BaseSlotMachineController.Instance.reelManager.AutoRun)
        {
            BaseSlotMachineController.Instance.reelManager.AutoRun = false;
            _preSuspendIsAutoRun = true;
        }
        _autospinSuspending = true;
    }

    public void RestoreHandle()
    {
        if (_preSuspendIsAutoRun)
        {
            BaseSlotMachineController.Instance.reelManager.AutoRun = true;
            BaseSlotMachineController.Instance.DoSpin();
            _preSuspendIsAutoRun = false;
        }
        _autospinSuspending = false;
    }
    #endregion



	public virtual int ReelMoveBeforeStopCount(){
		return 10;
	}

#region 新版的流程结算处理

	public double JackpotTotalAward
	{
		get;
		set;
	}
	

	public void SetNextLinkState()
	{
		onceMore = reelManager.EnableOnceMore();
		reelManager.NextTimeOnceMore = onceMore;
		SpinClickedMsgMgr.Instance.SetIsRespin(onceMore);
	}

	public void ResetTotalAward()
	{
		totalAward = 0;
	}
	
	/// <summary>
	/// 在bonus结束之后的end时候结算
	/// </summary>
	public void IncreaseBonusGameAward()
	{
		this.reelManager.IncreaseBonusAward();
	}
    public virtual void GetResultAward (ResultProcessData _resultData)
	{
		double spinTotalAward = 0;
		currentBetting_X = (double)1 / currentBetting;
		reelManager.PostCheckAwardEvent ();
		totalAward = _resultData.AccountValue;
		totalCash = _resultData.CashValue;
		//计算结算奖励的显示
		if(reelManager.isFreespinBonus || reelManager.FreespinCount > 0)
		{
			reelManager.FreespinGame.AwardInfo.awardValue += totalAward;
			reelManager.FreespinGame.AwardInfo.awardCashValue += totalCash;
		}

		spinTotalAward = totalAward;
		
		SetBigWinType(spinTotalAward);

        //当次Spin计算完奖励之后的广播事件
		Messenger.Broadcast<ReelManager,long>(GameConstants.SpinAwardEndMsg,reelManager,Utils.Utilities.CastValueLong(Math.Round(spinTotalAward)));
		
#if DEBUG || UNITY_EDITOR
		// Debug环境机器触发Feature时，上报ES，方便测试
		CheckMachineTaskFeatureES();
#endif

//        hasAnticipation = reelManager.CheckAnticipation();
		if (addLastWinCoinsToDisplay || isFreeRun||(lastOnceMore &&(!onceMore)) || _resultData.IsBonusType) {
			winCoinsForDisplay += Utils.Utilities.CastValueLong( Math.Round(totalAward));
			winCashForDisplay += _resultData.CashValue;
		} else {
			winCoinsForDisplay = Utils.Utilities.CastValueLong( Math.Round(totalAward));
			winCashForDisplay = _resultData.CashValue;
		}

		//收集奖励
		if (spinTotalAward > 0)
			SpinClickedMsgMgr.Instance.AddWinDictData(SpinClickedMsgMgr.TotalWin_Key,spinTotalAward * currentBetting_X);
		if (isReturnFromFreespin) {
			if (freespinGame.AwardInfo.awardValue > 0)
				SpinClickedMsgMgr.Instance.AddWinDictData(SpinClickedMsgMgr.FreespinWin_Key,freespinGame.AwardInfo.awardValue * currentBetting_X);
		}

		if(spinTotalAward>0){

			#region Restore FreeSpin
			if (reelManager.FreespinGame!=null) {
				reelManager.FreespinGame.currentSpinFinished = true;
			}
			#endregion
			
			//此处直接加spin给的金币
			UserManager.GetInstance ().IncreaseBalance (Utils.Utilities.CastValueLong (Math.Round (spinTotalAward)));
			
			
			// #region Statistics RTP
			// if (BaseGameConsole.ActiveGameConsole ().IsInSlotMachine () && spinTotalAward > 0) {
			// 	BaseSlotMachineController slot = BaseGameConsole.ActiveGameConsole ().SlotMachineController;
			// 	slot.statisticsManager.WinCoinsNum += spinTotalAward;
			// }
			// #endregion
		}
		if (totalCash>0)
		{
			//此处直接加spin给的钱
			OnLineEarningMgr.Instance.IncreaseCash(totalCash);
		}
		
		// 任务收集相关逻辑
		if (_resultData.AccountType == ResultAccountTypeEnum.LINKEND)
		{
			Messenger.Broadcast<long>(GameConstants.WinCoinsInLinkGame_Key, (long)_resultData.AccountValue);
		}
    }

    private void CheckMachineTaskFeatureES()
    {
#if DEBUG || UNITY_EDITOR
	    if (reelManager.HitFs)
	    {
		    Dictionary<string, object> dic = new Dictionary<string, object>();
		    dic.Add("FeatureName", "FreeGame");
		    BaseGameConsole.singletonInstance.LogBaseEvent("MachineHitFeature",dic);
	    }

	    if (reelManager.HitActBonus || reelManager.HasBonusGame || reelManager.HitLinkGame)
	    {
		    Dictionary<string, object> dic = new Dictionary<string, object>();
		    dic.Add("FeatureName", "Bonus");
		    BaseGameConsole.singletonInstance.LogBaseEvent("MachineHitFeature",dic);
	    }
#endif
    }
    
    public void SetBigWinType(double v)
    {
	    isEpicWin = false;
	    isMegaWin = false;
	    isBigWin = false;
	    if(v >= reelManager.gameConfigs.EpicWinTag * currentBettingTemp) isEpicWin = true;
	    else if(v >= reelManager.gameConfigs.MegaWinTag * currentBettingTemp) isMegaWin = true;
	    else if(v >= reelManager.gameConfigs.BigWinTag * currentBettingTemp) isBigWin = true;
	    if (isBigWin || isMegaWin || isEpicWin) {
		    SequencePaidResultsManager.RequestPatterEvent (SequencePaidResultsManager.BIG_WIN);
	    }
    }
#endregion

}
