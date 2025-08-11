using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classic;

public class GoldsReelManager : ReelManager
{
	protected string PAYLINE_ALL_SOUND = "line_all";
	protected GoldGameConfig m_GoldConfig;
	protected WaitForSeconds paylineWait = new WaitForSeconds (2.0f);
	public bool EnableOldSaveAndRestore= false;
    //
    protected bool m_FirstRoundHideAni = true;
	public override void InitReels (SlotMachineConfig slotConfig, GameCallback onStop, GameCallback onStart)
	{
		base.InitReels (slotConfig, onStop, onStart);
		//获得config
		m_GoldConfig = GetComponent<GoldGameConfig> ();
		if (m_GoldConfig == null) {
			m_GoldConfig = this.gameObject.AddComponent<GoldGameConfig> ();
		}
	}


	#region play symbol animation

	//重写父类的播放动画方法
	public override void PlayAwardSymbolAnimation ()
	{
		if (m_GoldConfig.IsAllPaylinesRound) {
			StartCoroutine (PlayPaylinesRound (gameConfigs.TileAnimationDuration));
		} else if (m_GoldConfig.IsParticlePayline) {
			StartCoroutine (PlayBoxParticle (gameConfigs.TileAnimationDuration));
		} else {
			base.PlayAwardSymbolAnimation ();
		}

		//去掉 kindofsymboldialog
		// this.ShowKindOfSymbol();
	}

	//symbol的动画都要播放，方框粒子循环播放
	public virtual IEnumerator PlayBoxParticle(float delayTime)
	{
//		PlayAwardSymbolAudio(BaseSlotMachineController.Instance.LineAudioName());
		WaitForSeconds m_secondsFrame = new WaitForSeconds (delayTime);

		Messenger.Broadcast<List<BaseElementPanel>, GameObject, ReelManager> (SlotControllerConstants.PAYLINE_ANIMATION_SHOW_BOXPARTICLE,awardElements,gameConfigs.payLineConfig.boxParticle, this);

		for (int i = 0; i < awardElements.Count; i++) {
			awardElements [i].PlayAnimation (1);
		}

		PlayAllBoxParticleHandler();
		if (awardLines.Count == 1) {
			PlayEveryLineSound (awardLines[0]);
		}
		yield return m_secondsFrame;

		

		int paylineIndex = 0;
		if (awardLines.Count <= 0)
			yield break;
		
		List<BaseElementPanel> forbiddenSymbols = new List<BaseElementPanel>();
		List<BaseElementPanel> playSymbols = new List<BaseElementPanel>();
		foreach (var element in awardElements)
		{

			if (!awardLines[0].awardElements.Contains(element))
			{
				forbiddenSymbols.Add(element);
			}
		}
		Messenger.Broadcast<List<BaseElementPanel>,ReelManager>(SlotControllerConstants.PAYLINE_SYMBOL_ANIMATION_HIDE,forbiddenSymbols,this);
		int curPaylineIndex = 0;
		if (AutoRun) 
			yield break; 
		while (true) {
			
			awardLines [paylineIndex].NeedReBuildPayLine = false;
			PlayEveryLineSound (awardLines[paylineIndex]);
			if (!BaseSlotMachineController.Instance.slotMachineConfig.UseSpine)
			{
				awardLines [paylineIndex].PlayAwardAnimation (false, this);
			}
			
			
			if (curPaylineIndex != paylineIndex)
			{
				playSymbols.Clear();
				foreach (var element in awardLines[paylineIndex].awardElements)
				{
					if (!awardLines[curPaylineIndex].awardElements.Contains(element))
					{
						playSymbols.Add(element);
					}
				}
				Messenger.Broadcast<List<BaseElementPanel>, GameObject,ReelManager>(SlotControllerConstants.PAYLINE_ANIMATION_SHOW_BOXPARTICLE, playSymbols, gameConfigs.payLineConfig.boxParticle,this);
			}

			PlaySingleLineBoxParticleHandler();
			yield return m_secondsFrame;
			
			curPaylineIndex = paylineIndex;
			paylineIndex = (paylineIndex + 1) % awardLines.Count;

			if (curPaylineIndex != paylineIndex)
			{
				forbiddenSymbols.Clear();
				foreach (var element in awardLines[curPaylineIndex].awardElements)
				{
					if (!awardLines[paylineIndex].awardElements.Contains(element))
					{
						forbiddenSymbols.Add(element);
					}
				}
				Messenger.Broadcast<List<BaseElementPanel>,ReelManager>(SlotControllerConstants.PAYLINE_SYMBOL_ANIMATION_HIDE,forbiddenSymbols,this);
			}

		}
	}


	protected virtual void PlayAllBoxParticleHandler()
	{
		
	}

	protected virtual void PlaySingleLineBoxParticleHandler()
	{
		
	}
	
	//按照all paylines轮播的形式
	protected void StopAllSymbolAnimation ()
	{
		for (int i=0; i<awardElements.Count; i++) {
			awardElements [i].StopAnimation(); 
		}
	}
	//按照all paylines轮播的形式
	//delayTime:动画时长
	protected virtual IEnumerator PlayPaylinesRound (float delayTime)
	{
		if (awardLines.Count <= 0||this==null) {
			yield break;
		}

		WaitForSeconds m_secondsFrame = new WaitForSeconds (delayTime);
		//每条线的处理

		int i = 0;
		int Next = 1 % awardLines.Count;
		this.PauseAllSymbolAnimation ();
		Messenger.Broadcast<ReelManager>(SlotControllerConstants.PAYLINE_ANIMATION_HIDE, this);
//		float startTime = Time.realtimeSinceStartup;
		
		while (true) {
			if (i == 0 && awardLines.Count >= 1)
			{
				StopAllSymbolAnimation();
				//处理所有线的paylines
				for (int k = awardLines.Count - 1; k >= 0; k--) {
					if(awardLines [k]==null) continue;
					awardLines [k].PlayAwardAnimation (not_singleLine, this, m_FirstRoundHideAni);
//					Debug.Log (awardLines [k].ShowSpaghetti);
//					Messenger.Broadcast<AwardLineElement> (SlotControllerConstants.PAYLINE_ANIMATION_SHOW, awardLines[k]);
				}
				Libs.AudioEntity.Instance.PlayEffect (PAYLINE_ALL_SOUND);
				yield return paylineWait;
			}

			Messenger.Broadcast<ReelManager>(SlotControllerConstants.PAYLINE_ANIMATION_HIDE, this);

			//在freespin中不需要播放单条线动画，直接退出循环逻辑
			if (this.isFreespinBonus || this.FreespinCount >0) {
				yield break;
			}

            //2019.5.28在auto中不进行下面的动画，除非手动暂停
            if(this.AutoRun)
            {
                yield return GameConstants.FrameTime;
            }
			if (AutoRun) 
				yield break; 
            List<BaseElementPanel> pauseElements = GetShouldCloseAwardElements(this.awardElements, awardLines[0].awardElements);

            pauseElements.ForEach(delegate (BaseElementPanel obj) {
                obj.StopAnimation();
            });
            if (i>=awardLines.Count||awardLines[i] == null) continue;
            awardLines [i].NeedReBuildPayLine = (gameConfigs.SingleLineReBuild || awardLines.Count > 1);
			this.PlayEveryLineSound (awardLines [i]);
			awardLines [i].PlayAwardAnimation (not_singleLine, this);
			//动画时长
			yield return m_secondsFrame;

			//找出需要隐藏的动画symbol，然后再隐藏动画
//			if (awardLines.Count > 1) {
				pauseElements = GetShouldCloseAwardElements (awardLines [i].awardElements, awardLines [Next].awardElements);

				pauseElements.ForEach (delegate(BaseElementPanel obj) {
					obj.StopAnimation(); 
				});

//			}
			Messenger.Broadcast<ReelManager>(SlotControllerConstants.PAYLINE_ANIMATION_HIDE, this);

			i = Next;
			Next = (Next + 1) % awardLines.Count;

			if (i == 0 && this.m_FirstRoundHideAni)
			{
				pauseElements = this.awardElements;
				pauseElements.ForEach (delegate(BaseElementPanel obj) {
					obj.PauseAnimation (true);
				});
			}
		}
	}



	//重写父类，目的是为了播放下条线的动画时，重复的动画symbol不再重复隐藏消失
	public override IEnumerator PlaySymbolAnimationByLine (float delayTime)
	{
		if (hasPlayAllSymbolHighAwardAnimation) {
			PauseAllSymbolAnimation ();
		}
		float startTime = Time.realtimeSinceStartup;
		if (awardLines.Count <= 0) {
			yield break;
		} else {
			int i = 0;
			this.PauseAllSymbolAnimation ();
			Messenger.Broadcast<ReelManager>(SlotControllerConstants.PAYLINE_ANIMATION_HIDE, this);
			this.PlayEveryLineSound (awardLines [i]);
			awardLines [i].PlayAwardAnimation (not_singleLine, this);
			awardLines [i].NeedReBuildPayLine = (gameConfigs.SingleLineReBuild || awardLines.Count > 1);
			int Next = (i + 1) % awardLines.Count;
			while (true) {
				yield return m_AnimationWaitForScecond;
				if (startTime + delayTime <= Time.realtimeSinceStartup) {
//					awardLines[i].PauseAwardAnimation (not_singleLine);

					//找出需要隐藏的动画symbol，然后再隐藏动画
					List<BaseElementPanel> pauseElements = GetShouldCloseAwardElements (awardLines [i].awardElements, awardLines [Next].awardElements);

					for (int j = 0; j < pauseElements.Count; j++) {
						pauseElements [j].PauseAnimation (true);
					}
					if (BaseSlotMachineController.Instance.reelManager.gameConfigs.EnableAnimationPayLine && awardLines [i].NeedReBuildPayLine) {
						Messenger.Broadcast<ReelManager>(SlotControllerConstants.PAYLINE_ANIMATION_HIDE, this);
					}
					this.PlayEveryLineSound (awardLines [Next]);
					awardLines [Next].PlayAwardAnimation (not_singleLine, this);
					i = Next;
					Next = (Next + 1) % awardLines.Count;
					startTime = Time.realtimeSinceStartup;
				} 
			}
		}
	}

	public virtual void PlayEveryLineSound (AwardLineElement line)
	{

	}

	protected List<BaseElementPanel> GetShouldCloseAwardElements (List<BaseElementPanel> preAwards, List<BaseElementPanel> afterAwards)
	{
		List<BaseElementPanel> result = new List<BaseElementPanel> ();
		for (int i = 0; i < preAwards.Count; i++) {
			if (!afterAwards.Contains (preAwards [i])) {
				result.Add (preAwards [i]);
			}
		}
		return result;
	}



	//所有不中奖的symbol不变灰,在gold 中都这么处理
	public override void DisableAllUnAwardSymbol ()
	{

	}
	#endregion

	#region 数据保存和恢复
	protected SceneProgressDataBase LocalProgressData = null;
	//保存的旧方法
	public override void SavePlayerCurrentSlotMachineStateData()
	{
		if (EnableOldSaveAndRestore) {
			base.SavePlayerCurrentSlotMachineStateData ();
		} 
		else {
			CreateProgreeData();
			SaveBaseProgressData ();
			SaveProgressDataToLocal ();
		}

	}

	protected virtual void CreateProgreeData()
	{
		LocalProgressData = new SceneProgressDataBase ();
	}

	//子类继承之，然后子类里的数据可以用Data的CopyValue方法
	protected virtual void SaveBaseProgressData()
	{
		if (LocalProgressData == null) {
//			Debug.LogError ("数据本地数据为空，请先初始化");
			return;
		}

		LocalProgressData.IsInFreespin = LocalProgressData.IsInBonusGame = false;
		LocalProgressData.leftFreeSpinCount = LocalProgressData.totalFreeSpinCount =  LocalProgressData.freespinCount = 0;
		LocalProgressData.currentWinCoins = 0;
		
//		string sceneName = slotConfig.Name ();
		
		if(FreespinGame !=null && !freespinFinished){
			//在freespin中的数据，
			if (isFreespinBonus) {
				if (FreespinCount>0 && IsAlreadyAddFreeSpinCount)
				{
					FreespinCount = 0;
				}
				LocalProgressData.IsInFreespin = true;
//				LocalProgressData.currentFreespinFinished = FreespinGame.currentSpinFinished;
				LocalProgressData.needShowCash = FreespinGame.NeedShowCash;
				LocalProgressData.leftFreeSpinCount = FreespinGame.LeftTime;
				LocalProgressData.totalFreeSpinCount = FreespinGame.TotalTime;
				LocalProgressData.awardMultipler = FreespinGame.multiplier;
				//存钱
				LocalProgressData.currentWinCash = BaseSlotMachineController.Instance.winCashForDisplay;
				LocalProgressData.currentWinCoins = BaseSlotMachineController.Instance.winCoinsForDisplay;//不要使用reelManager.FreespinGame.AwardInfo.awardValue,不准确
				//freespin 带子
				LocalProgressData.FreespinResultString = Utils.Utilities.ConvertListToString<int>(this.freeSpinResult);
				LocalProgressData.freespinCount = FreespinCount;
			}
			else
			{
				if (FreespinCount > 0) {
					
					LocalProgressData.freespinCount = FreespinCount;
					LocalProgressData.awardMultipler = FreespinGame.multiplier;
					LocalProgressData.currentWinCoins = 0;
				}
			}
		}
		LocalProgressData.currentWinCash = BaseSlotMachineController.Instance.winCashForDisplay;
		LocalProgressData.currentWinCoins = BaseSlotMachineController.Instance.winCoinsForDisplay;
		LocalProgressData.BaseGameResultString = Utils.Utilities.ConvertListToString<int> (baseGameResult);
		LocalProgressData.RName = reelStrips.GetCurrentUseRName ();
		LocalProgressData.AName = reelStrips.GetCurrentUseAName ();
		LocalProgressData.currentBetting = BaseSlotMachineController.Instance.currentBetting;


	}
	//子类继承此方法保存本地
	protected virtual void SaveProgressDataToLocal()
	{
		if (LocalProgressData != null) {
			SceneProgressManager.SaveSceneJson<SceneProgressDataBase> (slotConfig.Name (), LocalProgressData);
		}
	}
		
	//恢复的旧方法
	public override void RecoveryPlayerPreviousSlotMachineStateData()
	{
		if (EnableOldSaveAndRestore) {
			base.RecoveryPlayerPreviousSlotMachineStateData ();
		} 
		else 
		{
			try
			{
				LoadLocalProgress ();
				SendSlotSaveDataEsEvent();
				RestoreBaseProgressData ();
				RestoreIndependentProgressData ();
			}
			catch (Exception e)
			{
				SceneProgressManager.HasRestoreException = true;
				Debug.LogError("RecoveryException: " + e);
			}
			finally
			{
				SceneProgressManager.DeleteProgress(slotConfig.Name (), this);
			}
		}

	}

	//子类继承此方法，然后将子类的progress赋值给基类的progress
	protected virtual void LoadLocalProgress()
	{
		string sceneName = slotConfig.Name ();
		LocalProgressData = SceneProgressManager.LoadSceneJson<SceneProgressDataBase> (sceneName);
	}

	protected virtual void SendSlotSaveDataEsEvent()
	{
		string sceneName = slotConfig.Name ();
		string data = SceneProgressManager.GetSlotSaveData(sceneName);
		Dictionary<string, object> para = new Dictionary<string, object>();
		if (String.IsNullOrEmpty(data))
		{
			para.Add("ProgressData","Have No Save Data");
			BaseGameConsole.ActiveGameConsole().LogBaseEvent("SceneProgressData", para);
		}
		else
		{
			para.Add("ProgressData",data);
			BaseGameConsole.ActiveGameConsole().LogBaseEvent("SceneProgressData", para);
		}
	}

	//子类需要完全覆盖此类，然后需要判断是否为null，
	protected virtual void RestoreIndependentProgressData()
	{

	}

	protected virtual void RestoreBaseProgressData()
	{
		
	}

	#endregion

	//播放一整条线中相同symbol的动画
	protected virtual bool ShowKindOfSymbol()
	{
        bool isShowDialog = false;
        double fiveOfKindwinCoins = 0;
        bool isHaveFiveOfKind = false;
        foreach (var lines in this.awardLines)
        {
			if(!lines.awardPayLine.isAwarded || lines.awardPayLine.LineSymbolIndexs.Count == 0) continue;
			
			bool isEqual = true;

			int symbolIndex = lines.awardPayLine.LineSymbolIndexs[0];

			foreach (var itmeId in lines.awardPayLine.LineSymbolIndexs)
			{
				if(!this.symbolMap.getSymbolInfo(itmeId).isWild) {
					symbolIndex = itmeId;
					break;
				};
			}

			foreach (var index in lines.awardPayLine.LineSymbolIndexs)
			{
				var info = this.symbolMap.getSymbolInfo(index);
				if (info==null)
				{
					isEqual = false;
					break;
				}
				if(info.isWild) continue;
				if(index != symbolIndex)
				{
					isEqual = false;
					break;
				}
			}
			
			if(!isEqual) continue;

            int symbolCount = lines.awardPayLine.LineSymbolIndexs.Count;
            if (symbolCount >= 5)
            {
                isHaveFiveOfKind = true;
                fiveOfKindwinCoins += lines.awardPayLine.awardValue * AwardResult.CurrentResultBet;
            }
            if (!isShowDialog)
            {
	            if(symbolIndex>=0 && symbolIndex < this.gameConfigs.elementResources.Length)
	            {
		            Sprite symbolSprite = this.gameConfigs.elementResources[symbolIndex].staticSprite;
		            ShowKindOfSymbolMessengerCount(symbolSprite,symbolCount);
	            }
                isShowDialog = true;
            }
		}
        if(isHaveFiveOfKind)
        {
            Messenger.Broadcast<int>(GameConstants.WinFiveOfKindCoins_Key, Utils.Utilities.CastValueInt(fiveOfKindwinCoins));
            Messenger.Broadcast(GameConstants.ShowFiveOfKind_Key);
        }
        if (isShowDialog)
            return true;
		return false;
	}

	//获取轮子配置数据
	public GoldReelConfig GetGoldReelConfig(int index)
	{
		return null;// this.m_GoldConfig.GoldReelConfigs[index];
	}
	
	/// <summary>
	/// 防止过度重写 By:0906王航 
	/// </summary>
	/// <param name="symbolCount"></param>
	protected virtual void ShowKindOfSymbolMessengerCount(Sprite symbolSprite,int symbolCount)
	{
		Messenger.Broadcast<Sprite, int, System.Action>(GameDialogManager.OpenKindOfSymbolDialog,
			symbolSprite, symbolCount, null);
	}

}
