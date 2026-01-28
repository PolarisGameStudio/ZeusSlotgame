using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Classic;
using Utils;
public class JackPotPrizePool
{
	private	float BaseValueMultiple;
	private	float AddMultiple;
	private long LobbyShowBet;
	private float m_increaseAwardSecond;
	private string themeName;
	private  double BaseAward
	{
		get{
			if (BaseGameConsole.singletonInstance.isForTestScene) {
				return  TestController.Instance.currentBetting * this.BaseValueMultiple;
			} else {
				return BaseSlotMachineController.Instance.currentBetting * this.BaseValueMultiple;
			}
		}
	}

	public  string AwardName;

	public long MinBet
	{
		get
		{
			if (serverFeatureOpenBet>0.001f)
			{
				return serverFeatureOpenBet;
			}

			return LocalMinBet;
		}
	}
	//禁止外部直接使用
	private long LocalMinBet;
	//禁止外部直接使用
	private long serverFeatureOpenBet;
	
	public int JackPotIndex;

	public	double ExtraAward;

	/// <summary>
	/// 对应的现金值（网赚无限模式下使用，固定值）
	/// </summary>
	public int Cash { get; set; }

	public void SetServerFeatureOpenBet(long bet)
	{
		this.serverFeatureOpenBet = bet;
	}
	public void AddPoolAwardWithBet (long bet)
	{
		ExtraAward += AddMultiple * bet ;
		// double extraAwardTmp = Utils.Utilities.CastValueLong(ExtraAward);
		SaveExtraAward (ExtraAward);
	}

	public float GetJackPotBaseAward()
	{
		return BaseValueMultiple;
	}

	public void AddPoolAwardEachTime (float awardSecond,int seconds)
	{
		m_increaseAwardSecond = awardSecond;

		ExtraAward += awardSecond *seconds ;

		SaveExtraAward (ExtraAward);
	}

	public double GetExtraAwardValue()
	{
		return System.Math.Round(ExtraAward + m_increaseAwardSecond * JackPotManager.Instance.GetLastTime(),2) ;
	}
	public double GetTotalAward(float multiple =1f){
		double baseValue = BaseAward* System.Convert.ToDouble (multiple.ToString ());
		//添加变化中没计算的数值  先转化为字符串，在转化为double ，最后计算相乘 来消除精度损失问题；在crazy7s关卡测试出了精度问题，调整到2billion后，精度会有所损失
		return System.Math.Round(baseValue + ExtraAward + m_increaseAwardSecond * JackPotManager.Instance.GetLastTime(),2) ;
//		return Mathf.Floor( BaseAward * multiple + ExtraAward);
	}
	/// <summary>
	/// 未解锁时显示的奖励数
	/// Gets the unlock show award.
	/// </summary>
	/// <returns>The unlock show award.</returns>
	public double GetUnlockShowAward()
	{
		return System.Math.Round(this.MinBet * BaseValueMultiple + ExtraAward);
	}

	public double GetLobbyShowAward()
	{
		return System.Math.Round(this.LobbyShowBet * BaseValueMultiple + ExtraAward);
	}
	/// <summary>
	/// 未解锁时得到的奖励数
	/// Gets the unlock award.
	/// freeSpin的时候乘以倍率
	/// </summary>
	/// <returns>The unlock award.</returns>
	public float GetUnlockAward(float multiple = 1f)
	{
		if (BaseGameConsole.singletonInstance.isForTestScene) {
			return TestController.Instance.currentBetting * BaseValueMultiple * multiple;
		} else {
			return  BaseSlotMachineController.Instance.currentBetting * BaseValueMultiple * multiple;
		}
	}

	public JackPotPrizePool(Dictionary<string,object> temp,string themeName)
	{ 
		this.BaseValueMultiple = Utilities.GetFloat (temp,BASEVALUE, 0);
		this.AddMultiple = Utilities.GetFloat (temp,CONTRIBUTION, 0);
		this.LocalMinBet = Utilities.GetLong(temp ,BET_NAME, 0);
		this.LobbyShowBet = Utilities.GetLong (temp, LOBBY_BET_NAME, 0);
		if (temp.ContainsKey (RapidNum)) {
			this.JackPotIndex = Utilities.GetInt (temp,RapidNum, 0);
		}
		this.AwardName = Utilities.GetValue<string>(temp, AWARD_NAME, string.Empty);
		//temp [AWARD_NAME] as string;
		this.themeName = themeName;
		GetExtraAward ();
	}

	public void SaveExtraAward(double awardNum)
	{
		ExtraAward = awardNum;
		Utils.Utilities.SetValueDoubleFromFloat (themeName + SAVE_NAME + JackPotIndex,awardNum);
	}

	private void GetExtraAward()
	{
		string extraAwardKey = themeName + SAVE_NAME + this.JackPotIndex;
		this.ExtraAward = Utils.Utilities.GetValueDoubleFromFloat (extraAwardKey);
	}

	/// <summary>
	/// 保存 Cash 值到本地缓存（网赚无限模式使用）
	/// </summary>
	public void SaveCash(int cash)
	{
		Cash = cash;
		if (!string.IsNullOrEmpty(themeName) && !string.IsNullOrEmpty(AwardName))
		{
			string cashKey = themeName + "_cash_" + AwardName;
			SharedPlayerPrefs.SetPlayerPrefsIntValue(cashKey, cash);
		}
	}

	/// <summary>
	/// 从本地缓存获取 Cash 值
	/// </summary>
	private int GetCashFromCache()
	{
		if (!string.IsNullOrEmpty(themeName) && !string.IsNullOrEmpty(AwardName))
		{
			string cashKey = themeName + "_cash_" + AwardName;
			return SharedPlayerPrefs.GetPlayerPrefsIntValue(cashKey, -1);
		}
		return -1;
	}

	/// <summary>
	/// 初始化 Cash 值（优先从缓存读取）
	/// </summary>
	public void InitCash(int defaultCash)
	{
		int cachedCash = GetCashFromCache();
		if (cachedCash > 0)
		{
			Cash = cachedCash;
		}
		else
		{
			Cash = defaultCash;
			SaveCash(defaultCash);
		}
	}
	public static readonly string SAVE_NAME ="_progress_";
	public static readonly string CONTRIBUTION = "Contribution";   //添加的奖池倍率
	public static readonly string BASEVALUE = "BaseValue";			//基础倍率
	public static readonly string AWARD_NAME = "AwardName";
	public static readonly string BET_NAME="Bet";					//最小bet
	public static readonly string LOBBY_BET_NAME="ShowBet";			//大厅展示时的bet
	public static readonly string RapidNum = "RapidNum";			//出现的num
}
