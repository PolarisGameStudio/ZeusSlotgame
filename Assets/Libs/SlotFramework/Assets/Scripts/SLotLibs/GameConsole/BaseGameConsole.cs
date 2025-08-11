#define ASYNC_LOAD_BUG_EXISTS
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using App.SubSystems;
using Core;
using Plugins;
using Classic;
using Libs;
using SlotFramework.AutoQuest;
using UnityEngine.SceneManagement;
using System.Globalization;
using Activity;
using CardSystem;
using Ads;

public class BaseGameConsole :MonoBehaviour
{
	public BaseSlotMachineController SlotMachineController;

	public static string defaultGameObjectName = "GameConsole";
	public static BaseGameConsole singletonInstance;
	public static bool IsConfigInit = false;
	private bool isUpdateFailed = false;
	private bool hasLoadPlist = false;
	protected static object syncRoot = new object();
	public AutoQuestManager autoQuestManager = new AutoQuestManager();
	public  DateTime GameStartDateTime;
	[HideInInspector]
	public bool isForTestScene = false;
	protected bool m_IsClassic_iOS = false;  //用来区分是否是ios的classic版本，true为新版本

	#region Static Methods

	public static BaseGameConsole ActiveGameConsole(bool createIfNotExist = true)
	{
		return ActiveGameConsole<BaseGameConsole>(createIfNotExist);
	}

	public static AudioEntity ActiveAudioEntity()
	{
		return AudioEntity.ActiveAudioEntity();
	}

	public static T ActiveGameConsole<T>(bool createIfNotExist = true) where T: BaseGameConsole
	{
		if (applicationIsQuitting)
		{
			return null;
		}

		if (singletonInstance == null)
		{
			var consoleGO = GameObject.FindGameObjectWithTag(defaultGameObjectName);
			if (consoleGO != null)
			{
				consoleGO.name = defaultGameObjectName;
				singletonInstance = consoleGO.GetComponent<T>();
			}
		}
		else
		{
			T tGo = singletonInstance.GetComponent<T>();
			if (tGo != null)
			{				
				return tGo;
			}
			else
			{
				if (!typeof(T).IsInstanceOfType(singletonInstance))
				{
					logger.LogErrorF("Found an exsiting game console is created,destroy it.Type:{0}", singletonInstance.GetType());
					Destroy(singletonInstance.gameObject);
					singletonInstance = null;
				}
			}
		}

		if (singletonInstance == null && createIfNotExist)
		{
			lock (syncRoot)
			{
				GameObject consoleGO = new GameObject(defaultGameObjectName);
				consoleGO.tag = defaultGameObjectName;
				singletonInstance = consoleGO.AddComponent<T>();
			}
		} 
		return singletonInstance as T;
	}

	public static bool HasActiveGameConsole()
	{
		if (applicationIsQuitting)
		{
			return false;
		}

		GameObject consoleGO = GameObject.FindWithTag(defaultGameObjectName);
		return consoleGO != null && consoleGO.GetComponent<BaseGameConsole>() != null;
	}

	#endregion

	#region Slot-Ralated

	public virtual bool IsInSlotMachine()
	{
		logger.Log("IsInSlotMachine");
		return SlotMachineController != null;
	}
  
    public virtual bool IsInLobby()
	{
		return false;
	}
    
	public virtual List<SlotMachineConfig> SlotMachines()
	{
		return new List<SlotMachineConfig>(_slotMachines);
	}

	public virtual SlotMachineConfig SlotMachineConfig(string slotsName)
	{
		SlotMachineConfig config = null;
		List<SlotMachineConfig> configs = Plugins.Configuration.GetInstance().ConfigurationParseResult().AllSlotMachineConfigs();
		if (configs != null)
		{
			config =  configs.Find((c) => c.Name() == slotsName);
		}
		
		return config;
	}

	#endregion

	public void EnterSlotMachine(BaseSlotMachineController controller)
	{
		SlotMachineController = controller;
	}
	
	#region life circle

    void Awake()
	{
		Debug.Log("BaseGameConsole Awake");
		//禁止多点触控
		Input.multiTouchEnabled = false;
		
#if UNITY_EDITOR
        SkySreenUtils.SetScreenResolutions();
        if (this.gameObject.GetComponent<CanvasScalerLandscapeAdapter>()==null )
	        this.gameObject.AddComponent<CanvasScalerLandscapeAdapter>();
        if (this.gameObject.GetComponent<CanvasScalerPortraitAdapter>()==null ) 
	        this.gameObject.AddComponent<CanvasScalerPortraitAdapter>();
#endif
        if (singletonInstance == null) {
			singletonInstance = this;
			this.name = "GameConsole";
			
	        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
			DontDestroyOnLoad(this);
			OnGameConsoleAwake();
			GameStartDateTime = DateTime.Now;
        } else if (singletonInstance != this) {
			Destroy(this.gameObject);
		}
	}

    void SceneManager_activeSceneChanged(Scene sc1,Scene sc2 )
    {
        CameraAdapter.AdapterAllCameras();
    }
	protected virtual void OnGameConsoleAwake()
	{
		CreateSingleInstance();
		Application.targetFrameRate = 120;
		singletonInstance = this;
		IsConfigInit = false;
	}
	
	protected virtual void CreateSingleInstance()
	{
		UserManager.CreateInstance();
		ActiveAudioEntity();
		Plugins.Configuration.DefaultConfigFileName = "GameConfig.plist";
	}

	void Start()
	{
		SetDecimalSeparator();
		UserManager.GetInstance().UserProfile().SaveFirstLaunchTime();
		OnGameConsoleStart();
		SendGlobalException();
	}
	
	//一些国家用逗号作为小数点，强制把逗号变成小数点
	private void SetDecimalSeparator()
	{
		string CultureName = Thread.CurrentThread.CurrentCulture.Name;
		CultureInfo ci = new CultureInfo(CultureName);
		if (ci.NumberFormat.NumberDecimalSeparator!=".")
		{
			ci.NumberFormat.NumberDecimalSeparator = ".";
			Thread.CurrentThread.CurrentCulture = ci;
		}

	}

	public void Restart()
	{
		DelayAction da = new DelayAction(0.3f, null, () =>{
			Messenger.Broadcast(GameConstants.OnGameConsoleReStart);
			Libs.UIManager.Instance.CloseAll(true);
			SwapSceneManager.Instance.ResetState();
			SwapSceneManager.Instance.LoadSceneAsync(GameConstants.SCENE_LOADING);
			
			UserManager.GetInstance().UserProfile().SaveFirstLaunchTime();
			OnGameConsoleStart();
			Debug.Log("AppRestart");
		});
		da.Play();
	}

	
	protected virtual void OnGameConsoleStart()
	{
		hasLoadPlist = false;
		SwapSceneManager.Instance.Init();
		UIEventMgr.Instance.Init();
		WaitLoadPlist();
	}
	 
	bool IsPlistLoaded = false;
	private void WaitLoadPlist()
	{
		InitSystem();
	}
	
	protected void InitSystem()
	{
		//读取 plist采用协程分帧加载
		StartCoroutine(InitLocalPlistData());
		//后台的加载速度优化，不过太高会阻塞主线程ui
		Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.Normal;
	}

	private IEnumerator InitLocalPlistData()
	{
		if (hasLoadPlist)
			yield break;
		hasLoadPlist = true;
		//生成Configuration
		LoadConfiguration();
		Plugins.Configuration.GetInstance().Init();
		_slotMachines = new List<SlotMachineConfig> (Plugins.Configuration.GetInstance ().ConfigurationParseResult ().SlotMachineConfigs ());
		yield return GameConstants.FrameTime;;
		//初始化LevelPlan，与 bet相关
		ADManager.Instance.Init();
		SetupPlatformFPS ();
		Settings.GetInstance ().SetupPlatform ();
		UIManager.Instance.Init();
		DataManager.GetInstance().InitLvPlanBet();
		UserManager.GetInstance ().UserProfile ().SaveLastLaunchTime ();
		yield return GameConstants.FrameTime;;
		Dictionary<string, object> dict = new Dictionary<string, object>();
		//第一次登录送给玩家初始金币
		if (!UserManager.GetInstance().NotFirstTimeLogin)
		{
			dict.Add("credits",Configuration.GetInstance().ConfigurationParseResult().ApplicationConfig().StartingBonus()); 
		}
		else
		{
			dict.Add("credits",SharedPlayerPrefs.LoadPlayerPrefsLong("balance"));
		}
		UserManager.GetInstance().NotFirstTimeLogin = true;
		UserManager.GetInstance ().SetUserData (dict);
		yield return GameConstants.FrameTime;;
		//先任务系统初始化
		TaskManager.Instance.OnInit();
		//卡牌系统初始化
		CardSystemManager.Instance.OnInit();
		//提现系统初始化
		WithDrawManager.Instance.OnInit();
		yield return GameConstants.FrameTime;;
		//活动系统初始化
		ActivityManager.Instance.OnInit();
		IsConfigInit = true;
	}
	void OnApplicationPause(bool paused)
	{
		OnGameConsoleApplicationPause(paused);
	}

	private List<string> FirstTriggerFeatureInSessionList = new List<string>();
	public bool IsFirstTriggerFeatureInThisSession(string featureName)
	{
		return !FirstTriggerFeatureInSessionList.Contains(featureName);
	}

	public void AddFirstTriggerFeatureInSessionList(string featureName)
	{
		if (!FirstTriggerFeatureInSessionList.Contains(featureName))
		{
			FirstTriggerFeatureInSessionList.Add(featureName);
		}
	}

	protected void OnGameConsoleApplicationPause(bool paused)
	{
		if (!IsConfigInit)
		{
			return;
		}
		if (paused)
		{
			CommandManager.Instance.SaveAckingCommand();
			UserManager.GetInstance().UserProfile().LastExitGameTime = DateTime.Now;
			TaskManager.Instance.SaveTaskDictPlayerPrefers();
			Analytics.GetInstance().LogEvent(Analytics.AppOut);
			if (UserManager.GetInstance().UserProfile().HasGrantInitialBonus())
			{
				UserManager.GetInstance().UserProfile().GameTimeWhenLastClose += TimeUtils.SecondsBetween(GameStartDateTime, DateTime.Now);
				if (UserManager.GetInstance().UserProfile().IsFirstGameSession)
				{
					Analytics.GetInstance().LogEvent(Analytics.FirstSessionSeconds, Analytics.FirstSessionSeconds, UserManager.GetInstance().UserProfile().GameTimeWhenLastClose);
					UserManager.GetInstance().UserProfile().IsFirstGameSession = false;
				}
				if (BaseGameConsole.ActiveGameConsole().IsInSlotMachine())
				{
					BaseGameConsole.ActiveGameConsole().SlotMachineController.SendFlurry(BaseSlotMachineController.EXIT);
				}
			}

			autoQuestManager.SaveAutoQuest();

            if(!UserManager.GetInstance ().IsBack2FrontForApp){
                if (IsInSlotMachine())
                {
                    UserManager.GetInstance().UserProfile().SaveSpinCountForLevelUpInterval(SlotMachineController.currentLevelSpinCount);
                    SlotMachineController.statisticsManager.Send_Machine_Real_RTP(SlotMachineController.startTimeInSlot, "Pause");
                    SlotMachineController.reelManager.SavePlayerCurrentSlotMachineStateData();
                    
                    RewardSpinManager.Instance.SendRewardSpinLogEvent();
					SlotMachineController.SendSlotAutopilotCoinsIn();
                }
            }
            RulePatternManager.SaveLocalRuleData();
			//
			// if (AutopilotManager.IsActived())
			// {
			// 	AutopilotManager.GetInstance().OnQuit();
			// }
			AsyncLogger.Instance.SaveData();
			UserManager.GetInstance ().UserProfile ().SaveToPlayerPrefs ();
			TimeUtils.SetPauseDataTime();
		}
		else
		{
			SkySreenUtils.SetScreenResolutions(true);

			
			int bgReloadInterval = Plugins.Configuration.GetInstance().GetValueWithPath<int>(GameConstants.BGReloadInterval_Key,GameConstants.DefaultBGReloadInterval_Key);
			if (bgReloadInterval > 0)
			{
				long exitTime = TimeUtils.ConvertDateTimeLong(UserManager.GetInstance().UserProfile().LastExitGameTime);
				if (TimeUtils.ConvertDateTimeLong(DateTime.Now) - exitTime > bgReloadInterval)
				{
					ActiveGameConsole().Restart();
				}
				else
				{
					Messenger.Broadcast(GameConstants.IntervalBackToApp);
				}
			}
			if (!UserManager.GetInstance().IsBack2FrontForApp)
			{
				FirstTriggerFeatureInSessionList.Clear();
			}
			GameStartDateTime = DateTime.Now;
			if (BaseGameConsole.ActiveGameConsole().IsInSlotMachine())
			{
				BaseGameConsole.ActiveGameConsole().SlotMachineController.SendFlurry(BaseSlotMachineController.ENTER);
				SlotMachineController.startTimeInSlot = DateTime.Now;
			}
			Settings.GetInstance().SetupSound();
			Messenger.Broadcast(GameConstants.OnApplicationResume);
			UserManager.GetInstance().IsBack2FrontForApp = false;
			TimeUtils.StatisticsPauseDurTime();
		}
	}

	void OnApplicationQuit()
	{
		// #if UNITY_EDITOR
		// AutopilotManager.GetInstance().OnQuit();
		// #endif
		OnGameConsoleApplicationQuit();
	}

	protected void OnGameConsoleApplicationQuit()
	{
		if (!IsConfigInit)
		{
			return;
		}

		UserManager.GetInstance().UserProfile().LastExitGameTime = DateTime.Now; 
		

		if (IsInSlotMachine())
		{
			SlotMachineController.statisticsManager.Send_Machine_Real_RTP(SlotMachineController.startTimeInSlot, "AppQuit");
			SlotMachineController.reelManager.SavePlayerCurrentSlotMachineStateData();
			
			RewardSpinManager.Instance.SendRewardSpinLogEvent();
		}
		RulePatternManager.SaveLocalRuleData();
		AsyncLogger.Instance.SaveData();
		TaskManager.Instance.SaveTaskDictPlayerPrefers();
		UserManager.GetInstance().UserProfile().IsFirstGameSession = false;
		UserManager.GetInstance ().UserProfile ().SaveToPlayerPrefs ();
		CardSystemManager.Instance.SaveProgressData();
		ADManager.Instance.SaveADProgressData();
	}
	
	void Update()
	{
		OnGameConsoleUpdate();
	}


	public static bool isGMAHPlistReady = false;
	// 待执行的主线程代码
	private List<Action> actionOnMainThread = new List<Action>();
	private readonly object balanceLock = new object();
	protected virtual void OnGameConsoleUpdate()
	{
		ExecuteMainThreadAction();
	}

	public void RunOnMainThread(Action act) {
		lock (balanceLock)
		{
			actionOnMainThread.Add(act);
		}		
	}

	private int testCrash = 0;
	// 执行需要在主线程执行的代码
	private List<Action> _currentActions = new List<Action>();
	private void ExecuteMainThreadAction() {
		if(actionOnMainThread.Count >0)
		{
			lock (balanceLock)
			{
				_currentActions.Clear();
				_currentActions.AddRange(actionOnMainThread);
				actionOnMainThread.Clear();
			}

			for (var i = 0; i < _currentActions.Count; i++)
			{
//				Utilities.NativePlatformLog("OldAdTest", "Executint Main Thread Action No." + i);
				_currentActions[i].Invoke();
			}
		}
		
		#if DEBUG
		if(!BaseGameConsole.singletonInstance.isForTestScene)
		{
			testCrash++;
			if (testCrash % 3000 == 0)
			{
				Debug.Log(UIManager.Instance.FindAllNataiveName());
			}
		}
		#endif
	}

	void OnDestroy()
	{
		applicationIsQuitting = true;
		OnGameConsoleDestroy();
	}
	
	public bool IsAutoPilotSlotOrder { get; private set;}
	protected void OnGameConsoleDestroy()
	{
	}
	void OnGUI()
	{
		OnGameConsoleGUI();
	}

	protected virtual void OnGameConsoleGUI()
	{
	}

	#endregion


	private void SetupPlatformFPS()
	{
		//UnityUtil.DumpPlatformInfo ();

			#if UNITY_ANDROID
			int plistFps = Utils.Utilities.CastValueInt( Plugins.Configuration.GetInstance ().GetValueWithPath<int> ("Module/RefreshRate",60));

			Application.targetFrameRate = plistFps;
			#endif
	}

	protected void LoadConfiguration()
	{
		Plugins.Configuration.GetInstance();
	}

	protected virtual void OnCoinfigDidLoad(ConfigurationParseResult result)
	{	

		DataManager.GetInstance().DataMining.LoadConfigAndReadSavedDataFromFile();

		if (result.AutoQuestConfigs != null)
		{
			autoQuestManager.LoadAutoQuestConfig(result.AutoQuestConfigs);
			autoQuestManager.LoadSavedAutoQuestsFromFile();
		}
        
		SequencePaidResultsManager.Init (Utils.Utilities.GetValue<Dictionary<string,object>>(result.RawConfiguration(),SequencePaidResultsManager.PATTER_SWITCH,null));
	}

	#region RTOT

	public Dictionary<string, object> rtotDict { get; set; }

	public string rtotTitle { get; set; }

	List<string> showedRtotTitlesList = new List<string>();
	const string ShowedRtotTitles = "ShowedRtotTitles";

	private static bool rtotFetched = false;


	public void ShowRtotIfNeed()
	{
		//		Debug.Log ("ShowRtotIfNeed  ..... ");
		if (rtotFetched == false)
		{
			if (Plugins.NativeAPIs._IsRtotContentReady() == false)
			{
				//				Debug.Log ("ShowRtotIfNeed  ..... Plugins.NativeAPIs._IsRtotContentReady  false");
				return;	
			}

			string rtotContent = Plugins.NativeAPIs._GetRtotContent();
			//				Debug.Log ("ShowRtotIfNeed  ..... Plugins.NativeAPIs._IsRtotContentReady true");

			if (string.IsNullOrEmpty(rtotContent))
			{
				//				Debug.Log ("ShowRtotIfNeed  ..... Plugins.NativeAPIs._IsRtotContentReady rtotContent null");
				return;
			}
			rtotFetched = true;
		}

		if (rtotDict == null || rtotDict.Count == 0 || string.IsNullOrEmpty(rtotTitle))
		{
			return;
		}

		if (showedRtotTitlesList != null && showedRtotTitlesList.Contains(rtotTitle) == false)
		{
			bool imagesAreReady = true;
			if (rtotDict.ContainsKey("Buttons"))
			{
				List<object> buttons = rtotDict["Buttons"] as List<object>;
				for (int i = 0; i < buttons.Count; i++)
				{
					Dictionary<string, object> buttonDetails = buttons[i] as Dictionary<string, object>;
					if (buttonDetails.ContainsKey("ImageSprite") == false)
					{
						imagesAreReady = false;
						break;
					}
				}
			}

			if (imagesAreReady == true)
			{
				showedRtotTitlesList.Add(rtotTitle);
				string rtotTitlesString = MiniJSON.Json.Serialize(showedRtotTitlesList);
				PlayerPrefs.SetString(ShowedRtotTitles, rtotTitlesString);
				PlayerPrefs.Save();
			}
		}
	}

	#endregion

	#region  load cache dialog

	//		public bool NeedLoadCacheDialog = false;
	public bool HasCacheDialogLoaded = true;

	public virtual IEnumerator LoadMachineCacheDialog(Action<float> progressHandler = null)
	{
		yield break;
	}

	#endregion

	#region net change

	public void OnNetworkConnectionChanged(string msg)
	{
		bool netChange = Boolean.Parse(msg);
		Messenger.Broadcast<bool>(GameConstants.NETWORK_CHANGE, netChange);
	}

	#endregion

	/// <summary>
    ///发送抓取的异常
    /// </summary>
    /// <param name="e"></param>
    public void SendCatchExceptionToServer(Exception e)
    {
        Debug.LogAssertion("SendCatchExceptionToServer:"+e.ToString());
        Dictionary<string, object> eventParams = new Dictionary<string, object>();
        eventParams.Add("log", e.Message + ":" + e.StackTrace);
        LogBaseEvent(Analytics.CATCH_EXCEPTION, eventParams);
    }

	#region errorException

	private int MainThreadId;
	public  void SendGlobalException ()
	{
		MainThreadId = Thread.CurrentThread.ManagedThreadId;

		Application.logMessageReceivedThreaded += HandleLog;
	}

	private  void HandleLog(string message, string stackTraceString, LogType type)
	{
		if (type == LogType.Error)
		{
			Dictionary<string, object> events = new Dictionary<string, object>();
			
			Action generateData = () =>
			{
				events.Add("message",message);
				events.Add("stackTrace",stackTraceString);
				events.Add("systemInfo",UnityUtil.DumpPlatformInfo());
				
			};
			
			if (Thread.CurrentThread.ManagedThreadId == MainThreadId)
			{
				events.Add("ThreadInfo", Thread.CurrentThread.ManagedThreadId);
				events.Add("ThreadType", "UnityMain");
				generateData();
				
				LogBaseEvent("UnityErrorMsg", events);
			}
			else
			{
				events.Add("ThreadInfo", Thread.CurrentThread.ManagedThreadId);
				events.Add("ThreadType", string.IsNullOrEmpty(Thread.CurrentThread.Name) ? "Thread" : Thread.CurrentThread.Name);
				
				//异步线程异常收集
				RunOnMainThread(() =>
				{
					generateData();
					LogThreadBaseEvent("UnityErrorMsg", events);
				});
			}
		}
	}

	#endregion
	
	#region es
	public void LogBaseEvent(string eventName, Dictionary<string,object> infos = null)
	{
		Dictionary<string,object> parameters = new Dictionary<string, object>();
		if (infos != null)
		{
			foreach (var pair in infos)
			{
				if (!parameters.ContainsKey(pair.Key))
				{
					parameters.Add(pair.Key, pair.Value);
				}
			}
		}
		Debug.Log("eventName:"+eventName+"\n"+ MiniJSON.Json.Serialize(parameters));
	}

	public void LogThreadBaseEvent(string eventName, Dictionary<string, object> infos = null)
	{
		infos = infos ?? new Dictionary<string, object>();

		AddESParameters (infos);
		SendES(eventName, infos, true);
	}
	
	public void LogError(string msg, string tag)
	{
		LogError(msg, tag, null);
	}

	public void LogError(string msg, string tag, Dictionary<string,object> extra)
	{
		if(string.IsNullOrEmpty(msg))
			return;

		if (string.IsNullOrEmpty(tag))
		{
			tag = "none";
		}
		Dictionary<string, object> eventParams = new Dictionary<string, object>();
		eventParams.Add("error",msg);
		eventParams.Add("errorTag",tag);

		if (extra != null)
		{
			foreach (var temp in extra)
			{
				if(temp.Value == null)
					continue;
                
				eventParams[temp.Key] = temp.Value;
			}
		}
        
		LogBaseEvent(Analytics.CATCH_ERROR, eventParams);
	}
	public void LogBaseEvent(string eventName, string key1, object v1, string key2 = null, object v2 = null)
	{
		Dictionary<string,object> parameters = new Dictionary<string, object>();
		parameters.Add(key1, v1);
		if (key2 != null && v2 != null)
		{
			parameters.Add(key2, v2);
		}
		AddESParameters (parameters);
	}
	
	public void LogESAtOnce(string eventName, Dictionary<string,object> infos = null)
	{
		Dictionary<string,object> parameters = new Dictionary<string, object>();
		if (infos != null)
		{
			foreach (var pair in infos)
			{
				if (!parameters.ContainsKey(pair.Key))
				{
					parameters.Add(pair.Key, pair.Value);
				}
			}
		}
		AddESParameters (parameters);
	}

	public void AddESParameters( Dictionary<string,object> parameters = null)
	{
		if (parameters == null) {
			return;
		}

		UserProfile profile = UserManager.GetInstance ().UserProfile ();
		long balance = profile.Balance ();
		parameters["UserID"] = UserManager.GetInstance().deviceId;
		parameters ["CurrentBalance"] = balance;
		parameters ["CurrentLevel"] = profile.Level();
		parameters ["CurrentBetNum"] = SlotsBet.GetCurrentBet();
		if (IsInSlotMachine ()) {
			if (SlotMachineController.reelManager!=null&&SlotMachineController.reelManager.reelStrips!=null) {
				parameters["IsAuto"] =SlotMachineController.reelManager.AutoRun;
				parameters["isQuickStop"] =SlotMachineController.reelManager.boardController.isQuickStop;
				if (ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu)
				{
					parameters.Add("NewAuto", SlotMachineController.reelManager.AutoSpinSegment);
				}
				parameters["RUsed"] =SlotMachineController.reelManager.reelStrips.GetCurrentUseRName();
				parameters["AUsed"] =SlotMachineController.reelManager.reelStrips.GetCurrentUseAName();	
		    }
		}
		parameters["CurrentMachine"] = UserManager.GetInstance().UserProfile().LastChooseSlotName;
		parameters["LoginDays"] = 0;
		parameters[Analytics.TotalLoginDays] = 0;
	}
	private void SendES(string eventName, Dictionary<string,object> parameters = null, bool isThread = false)
	{
	}

	protected static bool applicationIsQuitting = false;

	protected List<SlotMachineConfig> _slotMachines;

	protected static float NativeCallBackDelay = 0.01f;

	private static global::Utils.Logger logger = global::Utils.Logger.GetUnityDebugLogger(typeof(BaseGameConsole), false);
	#endregion
    
}
