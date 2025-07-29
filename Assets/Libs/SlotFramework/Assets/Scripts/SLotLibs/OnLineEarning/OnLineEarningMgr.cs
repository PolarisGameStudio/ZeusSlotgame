using System;
using System.Collections.Generic;
using Classic;
using UnityEngine;
using Core;
using Libs;
using Utils;


public enum CountryType
{
    EN,
    BR,
    KR,
    PK,
    ID,
    IN,
    RU,
    JP,
    DE,
    VN,
    TR,
    TH,
    PH,
    MY,
    AR,
    CO,
    MX,
    PE
}
//网赚模块
public class OnLineEarningMgr
{
    private const string OPEN_KEY = "OPEN";
    private const string POP_PLAN_CONFIG_KEY = "PopPlanConfig";
    private const string NEW_USER_KEY = "NewUser";
    private const string Spin_KEY = "spin";
    private const string RewardMin_KEY = "rewardmin";
    private const string RewardMax_KEY = "rewardmax";
    private const string IsWhitePackage_KEY = "IsWhitePackage";

    private const string THREE_HUNDRED_CONFIG_KEY = "ThreeHundredConfig";
    private const string ON_LINE_EARNING_MODEL_KEY = "OnLineEarningModel"; //网赚模式 0--》无限模式  1--》300模式
    #region 单例
    protected static OnLineEarningMgr _Instance;
    private static object syncRoot = new object ();
    public static OnLineEarningMgr Instance {
        get {
            if (_Instance == null) {
                lock (syncRoot) {
                    if (_Instance == null) {
                        _Instance = new OnLineEarningMgr ();
                    }
                }
            }
            return _Instance;
        }
    }
    #endregion
   
    private int OnLineEarningModel = 0;
    private PopPlanConfig popPlanConfig;
    private ThreeHundredConfig threeHundredConfig;

    private BaseRewardConfig baseRewardConfig;
    //系统开启按钮
    private bool isOpen = true;

    private string cashSymbol="$";
    private CountryType countryType = CountryType.EN;
    public bool ShowCashInFree = false;
    
    private int newUserSpinCount = 0; //新用户在spin几次后弹出一个奖励弹板
    private int newUserCashMin = 0; //新用户首次弹板奖励金钱
    private int newUserCashMax = 0; //新用户首次弹板奖励金钱

    private bool isWhitePackage = false; //白包用户
    public void ParseConfig(Dictionary<string,object> config)
    {
        isOpen = Utils.Utilities.GetBool(config,OPEN_KEY,true);
        if (!isOpen)
        {
            return;
        }
        isWhitePackage = Utils.Utilities.GetBool(config,IsWhitePackage_KEY,false);
        OnLineEarningModel = Utils.Utilities.GetInt(config,ON_LINE_EARNING_MODEL_KEY,0);
        if (OnLineEarningModel == 0)
        {
            Dictionary<string,object> PopPlanConfigDict = Utils.Utilities.GetValue<Dictionary<string,object>>(config,POP_PLAN_CONFIG_KEY,null);
            if(PopPlanConfigDict == null) 
                throw new ArgumentNullException ("PopPlan dict not in big plist");
            //开启无限模式，解析无限模式配置
            if (popPlanConfig == null)
            {
                popPlanConfig = new PopPlanConfig();
            }
            popPlanConfig.ParseConfig(PopPlanConfigDict);
            baseRewardConfig = popPlanConfig;
        }else if (OnLineEarningModel == 1)
        {
            Dictionary<string,object> ThreeHundredConfigDict = Utils.Utilities.GetValue<Dictionary<string,object>>(config,THREE_HUNDRED_CONFIG_KEY,null);
            if(ThreeHundredConfigDict == null) 
                throw new ArgumentNullException ("PopPlan dict not in big plist");
            //开启300模式，解析300模式配置
            if (threeHundredConfig == null)
            {
                threeHundredConfig = new ThreeHundredConfig();
            }
            threeHundredConfig.ParseConfig(ThreeHundredConfigDict);
            baseRewardConfig = threeHundredConfig;
        }
        
        //加载本地化数据
        LoadFromPlayerPrefs();
        
        Dictionary<string,object> newUserConfig = Utils.Utilities.GetValue<Dictionary<string,object>>(config,NEW_USER_KEY,null);
        if (newUserConfig!=null && newUserConfig.Count>0)
        {
            newUserSpinCount = Utils.Utilities.GetInt(newUserConfig,Spin_KEY,0);
            newUserCashMin = Utils.Utilities.GetInt(newUserConfig,RewardMin_KEY,0);
            newUserCashMax = Utils.Utilities.GetInt(newUserConfig,RewardMax_KEY,0);
            if (UserManager.GetInstance().UserProfile().GetTotalSpinCounter()<newUserSpinCount)
            {
                Messenger.AddListener(SlotControllerConstants.SendSpinEvent,OnSpinEnd);
            }
        }
    }

    OnLineEarningMgr()
    {
        Messenger.AddListener<bool>(GameConstants.RefreshCashInFree,RefreshCashInFree);
    }
    ~OnLineEarningMgr()
    {
        Messenger.RemoveListener<bool>(GameConstants.RefreshCashInFree,RefreshCashInFree);
    }

    void OnSpinEnd()
    {
        //奖励弹板只弹出一次
        if (UserManager.GetInstance().UserProfile().GetTotalSpinCounter()>=newUserSpinCount && !PlatformManager.Instance.IsWhiteBao())
        {
            new DelayAction(1.3f,null, () =>
            {
                Messenger.RemoveListener(SlotControllerConstants.SendSpinEvent,OnSpinEnd);
                int newUserCash = UnityEngine.Random.Range(newUserCashMin,newUserCashMax);
                //加钱动画播放完毕
                IncreaseCash(newUserCash);
                Messenger.Broadcast<int>(GameDialogManager.OpenRewardCashDialogMsg, newUserCash);
            }).Play();
        }
    }
    public void SaveToPlayerPrefs()
    {
        
        PlayerPrefs.SetInt(CASH, cash);
    }
    
    private void LoadFromPlayerPrefs()
    {
        cash = SharedPlayerPrefs.GetPlayerPrefsIntValue(CASH,0);
    }
    
    private void RefreshCashInFree(bool show)
    {
        ShowCashInFree = show;
    }
   
    public bool ShowCashInFreeSpin()
    {
        return ShowCashInFree;
    }
    public bool isInfiniteOpen()
    {
        return isOpen && popPlanConfig != null;
    }
    
    public bool isThreeHundredOpen()
    {
        return isOpen && threeHundredConfig != null;
    }
    
    public PopPlanConfig GetPopPlanConfig()
    {
        return popPlanConfig;
    }
    public ThreeHundredConfig GetThreeHundredConfig()
    {
        return threeHundredConfig;
    }
    private string language = "";
    private string country = "";
    private int numberGK = 0;
    public void SetCommonPara(string lang,string ctr,int gk)
    {
        language = lang;
        country = ctr;
        SetCountry(ctr.ToUpper());
        numberGK = gk;
    }
    
    [SerializeField] private int cash = 0;
   
    private static readonly string CASH = "cash";
    private static readonly string CASHSTATEINFREE = "cashStateInFree";

    private static readonly string GETGASHTIME = "getCashTime";
    public int Cash()
    {
        return cash;
    }
    public int GetCashTime()
    {
        return baseRewardConfig.GetRewardCount;
    }
    
    public void SetCash(int newCash)
    {
        if (newCash < 0)
        {
            throw new System.ArgumentOutOfRangeException("newCash");
        }
        cash = newCash;
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"Cash",cash.ToString());
        SharedPlayerPrefs.SetPlayerPrefsIntValue(CASH,cash); 
    }
    
    public virtual void IncreaseCash(int newCash)
    {
        if (newCash>0)
        {
            Messenger.Broadcast<int>(OnLineEarningConstants.IncreaseCashMsg, newCash);
        }
        SetCash(cash + newCash);
    }
    //金钱汇率 以美元为基数转换
    
    public double ExchangeRate(int v){
        double money= 0;
        if(country == "br"){//巴西
            money = v * 5.0;
        }else if(country == "mx"){//西班牙
            money = v * 19.7;
        }else if(country == "pt"){//葡萄牙
            money = v * 5.6;
        }else if(country == "in"){//印度
            money = v * 86.2;
        }else if(country == "ru"){//俄罗斯
            money = v * 84.7;
        }else if(country == "de"){//德国
            money = v * 1.08;
        }else if(country == "fr"){//法国
            money = v * 1.08;
        }else if(country == "it"){//意大利
            money = v * 1.08;
        }else if(country == "nl"){//荷兰
            money = v * 1.08;
        }else if(country == "jp"){//日本
            money = v * 149.5;
        }else if(country == "tr"){//土耳其语
            money = v * 38.7;
        }else if(country == "kr"){ //韩国
            money = v * 1472.7;
        }else if(country == "vn"){ //越南
            money = v * 25576;
        }else if(country == "th"){ //泰国
            money = v * 34.8;
        }else if(country == "pl"){ //波兰
            money = v * 3.8;
        }else if(country == "id"){ //印尼
            money = v * 16372;
        }else if(country == "ph"){ //菲律宾
            money = v * 57;
        }else if(country == "my"){ //马来西亚
            money = v * 4.4;
        }else if(country == "ro"){ //罗马尼亚
            money = v * 4.5;
        }else if(country == "cz"){ //捷克
            money = v * 23;
        }else{ //美元
            money = v;
        }
        return money;
    }
    public string GetCashSymbol()
    {
        return cashSymbol;
    }
    public void SetCashSymbol(string symbol)
    {
        cashSymbol = symbol;
    }
    public string GetLanguage()
    {
        return language;
    }
    public CountryType GetCountry()
    {
        return countryType;
    }
    public void SetCountry(string country)
    {
        if(country == "BR"){//巴西
            countryType = CountryType.BR;
        }else if (country == "PK") { //巴基斯坦
            countryType = CountryType.PK; 
        }else if(country == "IN"){//印度
            countryType = CountryType.IN;
        }else if(country == "ID"){//印尼
            countryType = CountryType.ID;
        }else if(country == "RU"){ //俄罗斯
            countryType = CountryType.RU;
        }else if(country == "AR"){ //阿根廷
            countryType = CountryType.AR;
        }else if(country == "CO"){ //哥伦比亚
            countryType = CountryType.CO;
        }else if(country == "MX"){//墨西哥
            countryType = CountryType.MX;
        }else if(country == "PE"){//秘鲁
            countryType = CountryType.PE;
        }else if(country == "KR"){ //韩国
            countryType = CountryType.KR;
        }else if(country == "JP"){ //日本
            countryType = CountryType.JP;
        }else if(country == "DE"){ //德国
            countryType = CountryType.DE;
        }else if(country == "VN"){ //越南
            countryType = CountryType.VN;
        }else if(country == "TR"){ //土耳其
            countryType = CountryType.TR;
        }else if(country == "TR"){ //泰国
            countryType = CountryType.TH;
        }else if(country == "PH"){ //菲律宾
            countryType = CountryType.PH;
        }else if(country == "MY"){ //马来西亚
            countryType = CountryType.MY;
        }else{ //美元
            countryType = CountryType.EN;
        }
    }
    public bool CanShowH5()
    {
        if (isThreeHundredOpen() )
        {
            return PlatformManager.Instance.CheckCanShowH5() && threeHundredConfig.CanShowH5();
        }

        return PlatformManager.Instance.CheckCanShowH5();
    }
    public void AddSpinTime()
    {
        if (baseRewardConfig!=null)
        {
            baseRewardConfig.AddSpinTime();
        }
    }
    
    public void ResetSpinTime()
    {
        if (baseRewardConfig!=null)
        {
            baseRewardConfig.ResetSpinTime();
        }
    }
    public bool CheckCanShowBig()
    {
        if (baseRewardConfig==null)
        {
            return false;
        }
        return baseRewardConfig.CanShowBig();
    }
    
    public bool CheckCanPopReward()
    {
        if (baseRewardConfig==null)
        {
            return false;
        }
        return baseRewardConfig.CheckCanPopReward();
    }
 
    public void AddGetRewardCount()
    {
        baseRewardConfig?.AddGetRewardCount();
    } 
    public void AddGetH5RewardCount()
    {
        baseRewardConfig?.AddGetRewardCount();
    } 
    public void PopSmallDialogEnd()
    {
        baseRewardConfig?.PopSmallDialogEnd();
    }
    public void PopBigDialogEnd()
    {
        baseRewardConfig?.PopBigDialogEnd();
    }
    
    
    /// <summary>
    /// 0表明是 bigwin 1-->freegame 2-->bonus
    /// </summary>
    /// <param name="type"></param>
    public void AddADNum(int type = 0)
    {
        switch (type)
        {
            case 0:
                baseRewardConfig?.AddADNum();
                break;
            case 1:
                baseRewardConfig?.AddFreeInterstitialADNum();
                break;
            case 2:
                baseRewardConfig?.AddBonusInterstitialADNum();
                break;
        }
    }
    public int GetAdNum(int type = 0)
    {
        int num = 0;
        switch (type)
        {
            case 0:
                num = baseRewardConfig.GetADNum();
                break;
            case 1:
                num = baseRewardConfig.GetFreeInterstitialADNum();
                break;
            case 2:
                num = baseRewardConfig.GetBonusInterstitialADNum();
                break;
        }
        return num;
    }
  
    public int AddPopSpinWinCount()
    {
        if (baseRewardConfig==null)
        {
            return 0;
        }
        return baseRewardConfig.AddPopSpinWinCount();
    }
    
    public void ResetADNum(int type = 0)
    {
        switch (type)
        {
            case 0:
                baseRewardConfig?.ResetADNum();
                break;
            case 1:
                baseRewardConfig?.ResetFreeInterstitialADNum();
                break;
            case 2:
                baseRewardConfig?.ResetBonusInterstitialADNum();
                break;
        }
       
    }
    
    public bool CheckCanPopAD(int type=0)
    {
        bool canshow = false;
        switch (type)
        {
            case 0:
                canshow = baseRewardConfig.CheckCanPopAD();
                break;
            case 1:
                canshow = baseRewardConfig.CheckCanPopFreeInterstitialAD();
                break;
            case 2:
                canshow = baseRewardConfig.CheckCanPopBonusInterstitialAD();
                break;
        }

        return canshow;
    }
    public int GetBigRewardMultiple()
    {
        if (baseRewardConfig==null)
        {
            return 0;
        }
        return baseRewardConfig.GetBigRewardMultiple();
    }

    public int GetH5Reward()
    {
        if (baseRewardConfig==null)
        {
            return 0;
        }
        return baseRewardConfig.GetH5Reward();
    }

    public void HandleH5Event(float addCash)
    {
        //广播H5加钱
        int amount = Utilities.CastValueInt(addCash * GetCashMultiple());
        baseRewardConfig.HandleH5Event(amount);
    }
    
    public int GetCashMultiple()
    {
        return baseRewardConfig.CashMultiple();
    }
    public int GetLevel()
    {
        return baseRewardConfig.Level();
    }
    //由于cash根据不同的模式扩大的倍数不同，需要金钱文案显示时，通过此方法转换
    public double ConvertMoneyToDouble(int amount,int decimalPlaces=2,bool needExchange = true)
    {
        int multiple = baseRewardConfig.CashMultiple();
        double number = 0;
        if (needExchange)
        {
            //转换汇率，用于文本显示
            number = Math.Round(ExchangeRate(amount) / (multiple * 1.0f),decimalPlaces);
        }
        else
        {
            //需要保持原样数据，用于数据传输
            number = Math.Round(amount / (multiple * 1.0f),decimalPlaces);
        }
        return number;
    }

    //钱的图标 + 钱的符号 + 保留两位小数的钱的数量
    public string GetMoneyStr(int amount,int decimalPlace = 2,bool needIcon = true,bool needBigNum =false)
    {
        string str = "";
        double money = ConvertMoneyToDouble(amount, decimalPlace);
        string decimalFormat = "F" + decimalPlace+"}";
        //需要针对金钱进行大数处理
        if (needBigNum)
        {
            string cashInfo = Utilities.GetBigNumberShow((int)money,false);
            if (needIcon)
            {
                //确保有两位小数
                str = string.Format("<sprite name=\"{0}\"> {1}{2:"+decimalFormat,GetCashSpriteByNation(),cashSymbol,cashInfo); 
            }
            else
            {
                str = string.Format("{0}{1:"+decimalFormat,cashSymbol,cashInfo); 
            }
        }
        else
        {
            if (needIcon)
            {
                //确保有两位小数
                str = string.Format("<sprite name=\"{0}\"> {1}{2:"+decimalFormat,GetCashSpriteByNation(),cashSymbol,money); 
            }
            else
            {
                str = string.Format("{0}{1:"+decimalFormat,cashSymbol,money); 
            }
        }
       
        return str;
    }

    /// <summary>
    /// 根据不同国家显示不同的钱币图片
    /// </summary>
    /// <returns></returns>
    private string GetCashSpriteByNation()
    {
        string symbol = "";
        switch (countryType)
        {
            case CountryType.EN:
                symbol = "qian";
                break;
            default:
                symbol = "qian";
                break;
        }

        return symbol;
    }
    
    public List<PopPlanDiscountItem> GetDiscountItem()
    {
        return baseRewardConfig.GetDiscountItem();
    }

    public int GetCurLevelDiscount()
    {
        return baseRewardConfig.GetCurLevelDiscount();
    }
    public double GetWithDrawMoney()
    {
        return ConvertMoneyToDouble(baseRewardConfig.withDrawMoney);
    }

    private int GetRewardsByName(string key,int level = 0)
    {
        return baseRewardConfig.GetRewardsByName(key,level);
    }

    public int GetLuckyCashReward()
    {
        return GetRewardsByName(OnLineEarningConstants.REWARD_LUCKYCASH);
    }
    //freespin时从此处获取金钱
    public int GetFreeSpinReward(bool needMultiple)
    {
        // int multiple = needMultiple?2:1;
        int num = GetRewardsByName(OnLineEarningConstants.REWARD_FREESPIN);
        // return multiple * num;
        return num;
    }
    public int GetSpinWinReward(int type)
    {
        return GetRewardsByName(OnLineEarningConstants.REWARD_SPINWIN,type);
    }
    //jackpot 类型的枚举整数跟配置列表下标需转换
    public int GetJackpotGameWinReward(int jackpotType)
    {
        int type = Math.Abs(jackpotType-5);
        return GetRewardsByName(OnLineEarningConstants.REWARD_JACKPOT,type);
    }

    public int GetNormalSpinReward()
    {
        return GetRewardsByName(OnLineEarningConstants.REWARD_SPIN);
    }

    public bool IsWhiteBao()
    {
        return isWhitePackage;
    }
    
    /// <summary>
    /// 作用于触发 freespin和 jackpot的次数统计
    /// 第一次触发 freespin和 jackpot时，不播放广告
    /// </summary>
    public const string FreeSpinCountKey = "OnLineFreeSpinCountKey";
    public const string JackpotCountKey = "OnLineJackpotCountKey";
    //触发 freespin的次数
    public int FreeSpinCount
    {
        set
        {      
            SharedPlayerPrefs.SetPlayerPrefsIntValue(FreeSpinCountKey, value);
        }
        get
        {
            int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(FreeSpinCountKey, 0);
            return value;
        }
    }
    //触发 jackpot的次数
    public int JackpotCount {
        set
        {      
            SharedPlayerPrefs.SetPlayerPrefsIntValue(JackpotCountKey, value);
        }
        get
        {
            int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(JackpotCountKey, 0);
            return value;
        }
    }

    public int FreeSpinCountLimit = 1;
    public int JackpotCountLimit = 1;
    public bool CheckCanShowFreeStartAD()
    {
       return FreeSpinCount>FreeSpinCountLimit;
    }
    public bool CheckCanShowJackpotStartAD()
    {
        return JackpotCount>JackpotCountLimit;
    }
}


