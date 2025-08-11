using UnityEngine;
using System.Collections.Generic;
using Core;
using System;
using Utils;
using App.SubSystems;
using System.Linq;
using Libs;
using Plugins;
using Beebyte.Obfuscator;
using RealYou.Core.UI;
using RealYou.Utility.Json;

namespace Classic
{
	[Skip]
	public class UserManager
	{
		public static void CreateInstance ()
		{
			instance = new UserManager ();
		}

		public static UserManager GetInstance ()
		{
			if (instance == null) {
				lock (syncRoot) {
					if (instance == null) {
						CreateInstance ();
					}
				}
			}
			return instance;
		}
		
		public bool UserDataIsValid
		{
			get
			{
				if (_userDataIsValid == -1)
				{
					_userDataIsValid = PlayerPrefs.GetInt("_userDataIsValid", 1);
				}
				
				return _userDataIsValid == 1;
			}
			set
			{
				_userDataIsValid = value ? 1 : 0;
				PlayerPrefs.SetInt("_userDataIsValid", _userDataIsValid);
				PlayerPrefs.Save();
			}
		}

		public bool NotFirstTimeLogin
		{
			get
			{
				if (_notfirstTimeLogin == -1)
				{
					_notfirstTimeLogin = PlayerPrefs.GetInt("_notfirstTimeLogin", 0);
				}
				
				return _notfirstTimeLogin==1;
			}
			set
			{
				_notfirstTimeLogin = value ? 1 : 0;
				PlayerPrefs.SetInt("_notfirstTimeLogin", _notfirstTimeLogin);
				PlayerPrefs.Save();
			}
		}
		
		private int _notfirstTimeLogin = -1;
		public string deviceId; 
		public string appVersion;
		
		private int _userDataIsValid = -1;

		public UserManager ()
		{
			Messenger.AddListener (GameConstants.OnGameConsoleStarted, OnGameConsoleStarted);
			deviceId = SystemInfo.deviceUniqueIdentifier;
			appVersion = Application.version;
		}

		~UserManager ()
		{
			Messenger.RemoveListener (GameConstants.OnGameConsoleStarted, OnGameConsoleStarted);
		}
		
		public bool IsLastPlayMachineFeature { get; set;}
		public int LastPlayMachineSpinNum { get; set;}
		public string LastPlayMachine { get; set;}

		public long LevelUpAddAward{ set; get; }
		public float LevelUpAddVipCount{ set; get;}

		private bool m_IsLastPlayFeature = false;
		
		private void OnGameConsoleStarted ()
		{
			UserProfile ().IncreasePlayGameTotalTimes ();
		}

		public virtual UserProfile UserProfile ()
		{
			if (userProfile == null) {
				userProfile = new UserProfile ();
				userProfile.Init ();
			}
			return userProfile;
		}

		public void IncreaseBalanceAndSendMessage (long prize)
		{
			UserProfile ().IncreaseBalance (prize);
			Messenger.Broadcast (SlotControllerConstants.OnBlanceChangeForDisPlay);
		}

		public void IncreaseBalance (long prize)
		{
			UserProfile ().IncreaseBalance (prize);
		}
		
		#region AccountSDK Function

		public bool IsBack2FrontForApp{ get; set;}

		public void SetUserData(Dictionary<string,object> userDict)
		{
			if (userDict == null)
				return;

			userProfile.SaveCoinsMultiplier(Core.ApplicationConfig.GetInstance ().ShowCoinsMultiplier);

			if (userDict.ContainsKey("credits"))
			{
				long credits = Utils.Utilities.CastValueLong(userDict["credits"]);
				userProfile.SetBalance(credits);
				Messenger.Broadcast (SlotControllerConstants.OnBlanceChangeForDisPlay);
				userProfile.SetHasGrantInitialBonus(true);
			}
		}
		
		#endregion

		private UserProfile userProfile;
		public static Core.ApplicationConfig ApplicationConfig ()
		{
			return Plugins.Configuration.GetInstance ().ConfigurationParseResult ().ApplicationConfig ();
		}
		public Core.InfiniteModel PopPlanConfig ()
		{
			return Plugins.Configuration.GetInstance ().ConfigurationParseResult ().PopPlanConfig();
		}

		/// <summary>
		/// Find the maximum possible bet that satisifies the level requirement.
		/// </summary>
		/// <returns>The maximum betting.</returns>
		/// <param name="level">Level.</param>
		public virtual long MaximumBetting (int level)
		{
			// double dividedBet = GetInstance().UserProfile ().Balance() / 20;
			// long result = Utilities.CastValueLong(dividedBet);
			// result = Utilities.GetNearestTgtValue(result,DataManager.GetInstance().MachineBetDatas, NearestType.GreaterTgtValue);
			// return result;
			return DataManager.GetInstance().GetLevelMaxBet(level);
		}

		/// <summary>
		/// Finds the betting lower than current betting, and plays the sound.
		/// </summary>
		/// <returns>The betting.</returns>
		/// <param name="currentBetting">Current betting.</param>
		/// <param name="profile">useless</param>
		/// <param name="_slotConfig">useless</param>
		public virtual long PrevBetting (long currentBetting, UserProfile profile = null, SlotMachineConfig _slotConfig = null,bool needPlayEffect=true)
		{
			//1.2.4
			List<long> BetDatas = DataManager.GetInstance().MachineBetDatas;

			int index = -1;
			long bet = BetDatas[0];//lowest
			for (int i = 0; i < BetDatas.Count; i++) {
				if (BetDatas [i] < currentBetting) {
					index = i;
					bet = BetDatas [i];
					continue;
				} 
				break;
			}
			return bet;
		}

		/// <summary>
		/// The sound effect to play.
		/// -1 = max bet
		/// 0  = "malfunction sound"
		/// levels above 11 is treated as 11
		/// </summary>
		/// <param name="audioLevel">Audio level.</param>
		public void PlayBetEffect(int audioLevel){
			//1.2.4
//			if (-1 == audioLevel) {
//				Messenger.Broadcast (SlotControllerConstants.MAX_BET_TIP);
//				Libs.AudioEntity.Instance.PlayChangeBetEffect (12);
//				return;
//			}
//
//			int maxNormalLevel = 11;//the audio entity holds 13 sound files, 11 of them are for normal uses.
//
//			Libs.AudioEntity.Instance.PlayChangeBetEffect (Mathf.Clamp(audioLevel, 0, maxNormalLevel));
		}


		/// <summary>
		/// Finds the next betting.
		/// </summary>
		/// <returns>The betting.</returns>
		/// <param name="currentBetting">Current betting.</param>
		/// <param name="profile">Profile.</param>
		/// <param name="_slotConfig">Slot config.</param>
		public virtual long NextBetting (long currentBetting, UserProfile profile, SlotMachineConfig _slotConfig)
		{
			//1.2.4 MachineBetDatas每次修改都会对其进行升序排序处理
			List<long> BetDatas = DataManager.GetInstance().MachineBetDatas;

			long maximumBet = MaximumBetting (profile.Level());
			if ( maximumBet <= currentBetting) {
				PlayBetEffect (0);
				return maximumBet;
			}
			//find the minimal bet that is larger than current
			int index = -1;
			long bet = maximumBet;
			for (int i = 0; i < BetDatas.Count; i++) {
				if (BetDatas [i] > currentBetting) {
					index = i;
					bet = BetDatas [i];
					break;
				}
			}
			//bet为最大值时说明当次以达到最大，需要播放最大提示音效，不能直接比对索引值
			if (bet == maximumBet) {
				PlayBetEffect (-1);
			} else {
				PlayBetEffect (index);
			}
			return bet;
		}

		#region Bet
		///<returns>
		/// 返回值必须大于等于以最小机器设定的最小值,即通用规则要受特定规则约束
		///</returns>
		public long getBetByBalance (SlotMachineConfig _slotConfig)
		{
			long result = DataManager.GetInstance().GetCurrentBet(_slotConfig);
			return result;
		}

		public void SetCurrentBet(long bet)
		{
			UserProfile().SetCurrentBet(bet);
		}
		
		
		
		public void HandleBetTips()
		{
			//升级不会改变用户Bet，故不再提示BetChange GameDialogManager.OpenBetTips
			//1.通知用户当前Bet已经更新，改变用户当前Bet
			if(DataManager.GetInstance().needChangeCurrentBet)
			{
				if (MachineUtility.Instance.CanChangeBet())
				{
					Messenger.Broadcast(GameDialogManager.OpenBetTips);
				}
				DataManager.GetInstance().needChangeCurrentBet = false;

			}
			
			if (DataManager.GetInstance().isMaxBetChange)
			{
				UIManager.Instance.OpenTips(new OpenConfigParam<MaxBetIncreasedDialog>(0,OpenType.Normal,uiPopupStrategy:new MachineUIPopupStrategy(),animationIn:UIAnimation.NOAnimation,animationOut:UIAnimation.NOAnimation,queueId:Constants.UI_TIPS_EVENT_KEY));
				DataManager.GetInstance().isMaxBetChange = false;
			}
		}
		
		
		
		#endregion

		#region save machine spin times

		//是否返回过大厅

		public void SaveMachineSpinTimes (string machineName, int addCounts)
		{
			if (addCounts <= 0) {
				return;
			}
			SlotMachineConfig config = BaseGameConsole.ActiveGameConsole (true).SlotMachineConfig (machineName);
			//			config.MachineSpinCount += addCounts;
			if (config != null) {
				config.AddTodaySpinCount (addCounts);
			}
		}

		public void SaveFirstPlayedSlotMachineSpinCount (int spinCount)
		{
			if (SharedPlayerPrefs.GetPlayerPrefsIntValue (UserProfile ().FIRST_PLAY_SLOT_MACHINE_SPIN_COUNT, 0) == 0 && spinCount > 0) {
				SharedPlayerPrefs.SetPlayerPrefsIntValue (UserProfile ().FIRST_PLAY_SLOT_MACHINE_SPIN_COUNT, spinCount);
				SharedPlayerPrefs.SavePlayerPreference ();
			}
		}

		public void SetFirstPlayedSlotMachineSpinFlag ()
		{
			if (SharedPlayerPrefs.GetPlayerPrefsIntValue (UserProfile ().FIRST_PLAY_SLOT_MACHINE_SPIN_COUNT, 0) == 0) {
				SharedPlayerPrefs.SetPlayerPrefsBoolValue (UserProfile ().FIRST_PLAY_SLOT_MACHINE_SPIN_FLAG, true);
			} else {
				SharedPlayerPrefs.SetPlayerPrefsBoolValue (UserProfile ().FIRST_PLAY_SLOT_MACHINE_SPIN_FLAG, false);
			}
			SharedPlayerPrefs.SavePlayerPreference ();
		}
		#endregion

		
		public long totalLoginDays
		{
			get{ return SharedPlayerPrefs.LoadPlayerPrefsLong(Analytics.TotalLoginDays, 0);}
			set{ SharedPlayerPrefs.SavePlayerPrefsLong(Analytics.TotalLoginDays, value);}
		}
		public long LastLoginTime
		{
			get{ return SharedPlayerPrefs.LoadPlayerPrefsLong(Analytics.LastLoginTime, CSharpUtil.ConvertDateTimeLongInSecond(TimeUtils.GetDefaultTime()));}
			set{ SharedPlayerPrefs.SavePlayerPrefsLong(Analytics.LastLoginTime, value);}
		}
		private long LastCollectUpTime{
			get{ return SharedPlayerPrefs.LoadPlayerPrefsLong (Analytics.LastCollectUpTime, 0); }
			set{ SharedPlayerPrefs.SavePlayerPrefsLong(Analytics.LastCollectUpTime,value);}
		}
		private DateTime lastLoginDate{
			get{ return CSharpUtil.ConvertLongToDateInSecond (LastLoginTime); }
			set{ LastLoginTime = CSharpUtil.ConvertDateTimeLongInSecond (value); }
		}
		protected static UserManager instance;
		private static object syncRoot = new object ();
		
		#region EventCondtion
		public long MaxBalance = 0;
		#endregion
	}
}
