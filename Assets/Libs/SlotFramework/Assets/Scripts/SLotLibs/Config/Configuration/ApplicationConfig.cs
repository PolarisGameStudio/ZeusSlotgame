using System.Collections.Generic;
using Utils;

namespace Core
{
    public class ApplicationConfig
    {
        public readonly static string DefaultNicknameFormat = "guest{0}";
        public readonly static int DefaultNicknameSeedStart = 2000000;
        public readonly static int DefaultNicknameSeedEnd =   9999999;
        public const int DefaultStartingBonus = 1000;

        //public string RatingUsEmailAddress {get; set;}
		//public bool EnableRateAlert { get; set;}
	
		private float totalBetCoins = 0.01f;

		//spin模式配置：0不显示auto按钮 1spin达到多少次显示auto按钮 2一直显示auto按钮
		public int SpinPattern { get; set; }
		//spin显示auto按钮的限制次数
		public int SpinShowAutoLimit { get; set; }
		public int MaxBetCoins{ get; set;}
		public int ShowCoinsMultiplier{ set; get; }

		public float FastTimeScale { get; private set;}

		public double GetBaseBetCoins()
		{
			return totalBetCoins;
		}
        public float StartingBonus()
        {
            return startingBonus;
        }
        public void SetStartingBonus(float newStartingBonus)
        {
            startingBonus = newStartingBonus;
        }
		public float GetTotalBetCoins()
		{
			float xp = GetTotalBetCoinsFactor();
			return totalBetCoins * xp;
		}
		public float GetTotalBetCoinsFactor()
		{
			return 0;
		}
		public Dictionary<string,string> LocalizedMessages {
			get {
				if (_localizedMessages == null) {
					_localizedMessages = new Dictionary<string, string> ();
				}
				return _localizedMessages;
			}
			set {
				_localizedMessages = value;
			}
		}
		private Dictionary<string,string> _localizedMessages;
		
		/***
        *是否打开指定次数的Spin菜单项，在Spin按钮上面
        * */
		public bool IsOpenAutoSpinNumOfMenu
		{
			set;
			get;
		}
        public bool EnableEncryptPlist{
            get;
            set;
        }
        public bool EnableReadEncryptPlistInPackage
        {
            get;
            set;
        }
		public string[] BotNames
		{
			get;
			set;
		}
		
		/***
         * Spin按钮长按后显示的菜单数字信息 对应配表里的AutoSpinNewLongPress
         ***/
		public string AutoSpinNumOfMenu
		{
			get;
			set;
		}
	    private float startingBonus = DefaultStartingBonus;

		public int EnaMaxBetChangeDialogLevel
		{
			get;
			set;
		}

		public static  ApplicationConfig GetInstance()
        {
            if (instance == null) {
                lock (syncRoot) 
                {
                    if (instance == null) {
                        instance = new ApplicationConfig();
                    }
                }
            }
            return instance;
        }


        //TODO: should be private
        public ApplicationConfig()
        {
	       
        }


        public void CopyValuesFrom(ApplicationConfig appConfig)
        {
            this.AutoSpinNumOfMenu = appConfig.AutoSpinNumOfMenu;
            this.IsOpenAutoSpinNumOfMenu = appConfig.IsOpenAutoSpinNumOfMenu;
            this.SetStartingBonus(appConfig.startingBonus);

			this.LocalizedMessages = appConfig.LocalizedMessages;
			this.EnableEncryptPlist = appConfig.EnableEncryptPlist;
            this.EnableReadEncryptPlistInPackage = appConfig.EnableReadEncryptPlistInPackage;
            this.totalBetCoins = appConfig.totalBetCoins;
		
			this.MaxBetCoins = appConfig.MaxBetCoins;
			
			this.BotNames = appConfig.BotNames;
			this.ShowCoinsMultiplier = appConfig.ShowCoinsMultiplier;

			this.EnaMaxBetChangeDialogLevel = appConfig.EnaMaxBetChangeDialogLevel;
			this.FastTimeScale = appConfig.FastTimeScale;
			this.SpinPattern = appConfig.SpinPattern;
			this.SpinShowAutoLimit = appConfig.SpinShowAutoLimit;
        }

        bool IsNullOrEmpty(string str)
        {
            return str == null || str.Length == 0;
        }
        private int[] maximumBetEachLevel ;   // 每条payline的最大压的筹码数
        private int[] rollDistanceEachReel ; // 界面reel转动动画中转动的距离, n表示向下转动n个symbol

        private static ApplicationConfig instance;
        private static object syncRoot = new object();

		public readonly static string APPLICATION_CONFIG_AUTOSPIN_NUMBER_OF_MENU_KEY = "AutoSpinNewLongPress";
		public readonly static string APPLICATION_CONFIG_LOCALIZED_STRINGS_KEY = "LocalizedStrings";
		public readonly static string APPLICATION_CONFIG_ENABLE_SLOTS_FAST_BTN = "EnableSlotFastBtn";
        public readonly static string APPLICATION_CONFIG_ENABLE_ENCRYPT_PLIST = "EnableEncryptPlist";
        public readonly static string APPLICATION_CONFIG_ENABLE_FORCE_READ_ENCRYPT_PLIST = "EnableForceReadEncryptPlistInPackage";
		public readonly static string APPLICATION_CONFIG_TOTALBET_COINS= "BetCoins";
		public readonly static string APPLICATION_CONFIG_MAX_BET= "MaxBetCoins";
		public readonly static string APPLICATION_CONFIG_COINS_MULTIPLE = "ShowCoinsMultiplier";
		public readonly static string APPLICATION_CONFIG_IS_OPEN_AUTOSPIN_NUM_MENU = "IsOpenAutoSpin";
		public readonly static string APPLICATION_CONFIG_Menu_Feature_Button_STATUS_KEY = "MenuFeatureButtonStatus";
		public readonly static string APPLICATION_CONFIG_Fast_Time_Scale_KEY = "MachineSpeedRatio";
		public readonly static string APPLICATION_CONFIG_STARTING_BONUS_KEY = "StartingBonus";
		public readonly static string APPLICATION_CONFIG_SPIN_PATTERN_KEY = "SpinPattern";
		public readonly static string APPLICATION_CONFIG_SPIN_SHOW_AUTO_LIMIT_KEY = "SpinShowAutoLimit";

		public static ApplicationConfig ParseAppConfig (Dictionary<string, object> appConfigDict)
		{
			ApplicationConfig appConfig = new ApplicationConfig ();

			appConfig.AutoSpinNumOfMenu = Utilities.GetValue<string>(appConfigDict, APPLICATION_CONFIG_AUTOSPIN_NUMBER_OF_MENU_KEY,"");
			
			Dictionary<string,object> localizedMessages =  Utilities.GetValue<Dictionary<string,object>> (appConfigDict, APPLICATION_CONFIG_LOCALIZED_STRINGS_KEY, null);
			appConfig.LocalizedMessages = JSONUtil.DictionaryWithStringKeyAndValues (localizedMessages);

            appConfig.EnableEncryptPlist = Utilities.GetBool(appConfigDict, APPLICATION_CONFIG_ENABLE_ENCRYPT_PLIST, true);
            appConfig.EnableReadEncryptPlistInPackage = Utilities.GetBool(appConfigDict, APPLICATION_CONFIG_ENABLE_FORCE_READ_ENCRYPT_PLIST, true); 
            int intValue = Utilities.CastValueInt(appConfigDict [APPLICATION_CONFIG_STARTING_BONUS_KEY]);
			appConfig.SetStartingBonus ((float)intValue);

			if (appConfigDict.ContainsKey (APPLICATION_CONFIG_IS_OPEN_AUTOSPIN_NUM_MENU)) {
				appConfig.IsOpenAutoSpinNumOfMenu = Utilities.GetBool(appConfigDict, APPLICATION_CONFIG_IS_OPEN_AUTOSPIN_NUM_MENU, false);
			}

			if (appConfigDict.ContainsKey (APPLICATION_CONFIG_TOTALBET_COINS)) {
				appConfig.totalBetCoins = Utilities.CastValueFloat (appConfigDict[APPLICATION_CONFIG_TOTALBET_COINS],0.01f);
			}
			appConfig.MaxBetCoins = Utilities.GetInt (appConfigDict, APPLICATION_CONFIG_MAX_BET, 500);

			int coinsMultiplier = Classic.UserManager.GetInstance().UserProfile().LoadCoinsMultiplier();
			if (coinsMultiplier == 0)
			{
				appConfig.ShowCoinsMultiplier = Utilities.GetInt(appConfigDict, APPLICATION_CONFIG_COINS_MULTIPLE, 1);
				Classic.UserManager.GetInstance().UserProfile().SaveCoinsMultiplier(appConfig.ShowCoinsMultiplier);
			}
			else
				appConfig.ShowCoinsMultiplier = coinsMultiplier;
			
			appConfig.FastTimeScale = Utilities.GetFloat(appConfigDict, APPLICATION_CONFIG_Fast_Time_Scale_KEY, -1f);
			
			appConfig.SpinPattern = Utilities.GetInt(appConfigDict, APPLICATION_CONFIG_SPIN_PATTERN_KEY, 0);
			appConfig.SpinShowAutoLimit = Utilities.GetInt(appConfigDict, APPLICATION_CONFIG_SPIN_SHOW_AUTO_LIMIT_KEY, 0);
			
			return appConfig;
		}
    }
}
