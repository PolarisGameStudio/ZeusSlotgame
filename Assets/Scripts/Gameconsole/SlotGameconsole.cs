using UnityEngine;
using System.Collections;
using Libs;
using Classic;
using System;
using System.Collections.Generic;
public class SlotGameconsole : BaseGameConsole {

	protected override void OnGameConsoleAwake ()
	{
		base.OnGameConsoleAwake ();
		singletonInstance = this;
	}

	protected override void OnGameConsoleUpdate() {
		base.OnGameConsoleUpdate();
		#if UNITY_EDITOR
		UpdateTick();
		#else
		if (Debug.isDebugBuild) {
		UpdateTick();
		}
		#endif
	}

	protected override void OnGameConsoleGUI() {
		base.OnGameConsoleGUI();
		#if UNITY_EDITOR
		// DrawFPS();
		#else
		if (Debug.isDebugBuild) {
		DrawFPS();
		}
		#endif
	}

	#region FPS

	long framesCount = 0;
	int currentFPS = 0;
	long currentFPSUpdateCount = 0;
	float timeLeft = 0;
	float accumulated = 0;
	const float updateInterval = 0.5f;

	private GUIStyle labelStyle;
	private string segment="";
	protected GUIStyle LabelStyle
	{
		get
		{
			if (this.labelStyle == null)
			{
				this.labelStyle = new GUIStyle(GUI.skin.label);
				this.labelStyle.fontSize = (int)(Screen.dpi / 5f);
			}

			return this.labelStyle;
		}
	}

	private void DrawFPS() {
		Color oldColor = GUI.color;
		if (currentFPS > 50) {
			GUI.color = new Color(0, 1, 0);
		} else if (currentFPS > 40) {
			GUI.color = new Color(1, 1, 0);
		} else {
			GUI.color = new Color(1, 0, 0);
		}
		GUI.Label(new Rect(128, 35, 1024, 128), "FPS: " + currentFPS + " SEG: " + segment, this.LabelStyle);
		GUI.color = oldColor;

		//if(GUI.Button(new Rect(0, 300, 100, 50), "Crash"))
		//{
		//	UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.Abort);
		//}
	}

	private void UpdateTick() {
		framesCount++;
		timeLeft -= Time.deltaTime;
		accumulated += Time.timeScale / Time.deltaTime;

		if (timeLeft <= 0) {
			currentFPS = (int)(accumulated / framesCount);
			currentFPSUpdateCount++;

			timeLeft = updateInterval;
			accumulated = 0;
			framesCount = 0;
		}
	}

	#endregion

	static SlotGameconsole()
	{
//		AsyncLogger.Instance.StartTraceLog ("GlodSlotGameconsole");
		if (singletonInstance == null) {
			GameObject go = new GameObject (defaultGameObjectName);
			go.tag = defaultGameObjectName;
			DontDestroyOnLoad (go);
			singletonInstance = go.AddComponent<SlotGameconsole> ();
		}
//		AsyncLogger.Instance.EndTraceLog ("GlodSlotGameconsole");
	}



	public new static SlotGameconsole ActiveGameConsole (bool createIfNotExist = true)
	{
		return singletonInstance as SlotGameconsole;
		//		return ActiveGameConsole<GlodSlotGameconsole> (createIfNotExist);
	}

	protected new static AudioEntity ActiveAudioEntity()
	{
		return AudioEntity.ActiveAudioEntity();
	}

	protected override void CreateSingleInstance ()
	{
		//		NeedLoadCacheDialog = true;
		HasCacheDialogLoaded = false;
		UserManager.CreateInstance ();
		//		CoroutineUtil.Instance.StartCoroutine (LoadCacheDialog ());
		ActiveAudioEntity ();
		Plugins.Configuration.DefaultConfigFileName = "GameConfig.plist";
	}
	
	public override IEnumerator LoadMachineCacheDialog (Action<float> progressHandler)
	{
		LoadMachineNeedCacheDialog (progressHandler);
		yield break;
	}
	public void LoadMachineNeedCacheDialog(Action<float> progressHandler)
	{ 
		string[] ClassicLoadDialogs;
		#if UNITY_IOS 
		ClassicLoadDialogs = ClassicLoadIOSDialogs;
		#else
		ClassicLoadDialogs = ClassicLoadAndroidDialogs;
		#endif
		List<string> classicDialogs = new List<string>(ClassicLoadDialogs);
		UIManager.Instance.LoadInitDialog (classicDialogs);

		HasCacheDialogLoaded = true; 
	}
	private string[] ClassicLoadAndroidDialogs = { };// {"JackPotCommonOtherWinDialog","EpicWinDialog","MegaWinDialog","LevelUpDialog","MissionDialog","MissionProgressDialog","PiggyDialog","RateAlertDialog","SpecialOfferDialog","SpecialOfferDialog2","SpecialOfferNoMoneyDialog" };
	private string[] ClassicLoadIOSDialogs = { };// {"JackPotCommonOtherWinDialog","EpicWinDialog","MegaWinDialog","LevelUpDialog","MissionDialog","MissionProgressDialog","PiggyDialog","RateAlertDialog","SpecialOfferDialog","SpecialOfferDialog2","SpecialOfferNoMoneyDialog" };
}


