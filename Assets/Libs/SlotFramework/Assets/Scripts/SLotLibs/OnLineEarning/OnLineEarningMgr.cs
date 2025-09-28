using System;
using System.BuffSystem;
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
    private const string POP_PLAN_CONFIG_KEY = "InfiniteModelConfig";
    private const string IsWhitePackage_KEY = "IsWhitePackage";

    private const string THREE_HUNDRED_CONFIG_KEY = "300ModelConfig";
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
    private InfiniteModel _infiniteModel;
    private ThreeHundredModel _threeHundredModel;
    private IntervalDataPattern _intervalDataPattern;

    private BaseOnlineEarningModel _baseOnlineEarningModel;
    //系统开启按钮
    private bool isOpen = true;

    private string cashSymbol="$";
    private CountryType countryType = CountryType.EN;
    public bool ShowCashInFree = false;
    
    private int newUserSpinCount = 0; //新用户在spin几次后弹出一个奖励弹板

    private bool isWhitePackage = false; //白包用户

    //300和区间模式下，奖励弹板的弹出限制
    private int PopRewardLimit = 3;
    //300和区间模式下，奖励弹板的弹出条件数数 curRewardCount >= PopRewardLimit
    public int curRewardCount = 0;
    public float popRewardRate = 1f;
    
    public const string PopSpinWinCountKey = "PopSpinWinCountKey";
    public int PopSpinWinCount
    {
        set
        {      
            SharedPlayerPrefs.SetPlayerPrefsIntValue(PopSpinWinCountKey, value);
        }
        get
        {
            int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(PopSpinWinCountKey, 0);
            return value;
        }
    }
    
    OnLineEarningProgressData progressData = new OnLineEarningProgressData();
    
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
            if (_infiniteModel == null)
            {
                _infiniteModel = new InfiniteModel();
            }
            _infiniteModel.ParseConfig(PopPlanConfigDict);
            _baseOnlineEarningModel = _infiniteModel;
        }else if (OnLineEarningModel == 1)
        {
            Dictionary<string,object> ThreeHundredConfigDict = Utils.Utilities.GetValue<Dictionary<string,object>>(config,THREE_HUNDRED_CONFIG_KEY,null);
            if(ThreeHundredConfigDict == null) 
                throw new ArgumentNullException ("PopPlan dict not in big plist");
            //开启300模式，解析300模式配置
            if (_threeHundredModel == null)
            {
                _threeHundredModel = new ThreeHundredModel();
            }
            _threeHundredModel.ParseConfig(ThreeHundredConfigDict);
            _baseOnlineEarningModel = _threeHundredModel;
        }
        else if (OnLineEarningModel == 2)
        {
            Dictionary<string,object> IntervalModelConfig = Utils.Utilities.GetValue<Dictionary<string,object>>(config,OnLineEarningConstants.IntervalModelConfig,null);
            if(IntervalModelConfig == null) 
                throw new ArgumentNullException ("PopPlan dict not in big plist");
            //开启300模式，解析300模式配置
            if (_intervalDataPattern == null)
            {
                _intervalDataPattern = new IntervalDataPattern();
            }
            _intervalDataPattern.ParseConfig(IntervalModelConfig);
            _baseOnlineEarningModel = _intervalDataPattern;
        }
        newUserSpinCount = Utilities.GetInt(config,OnLineEarningConstants.NewUserLimitKey,0);
        PopRewardLimit = Utilities.GetInt(config,OnLineEarningConstants.PopRewardLimitKey,0);
        popRewardRate = Utilities.GetFloat(config,OnLineEarningConstants.PopRewardRateKey,1f);

        //加载本地化数据
        LoadProgressData();
    }

    OnLineEarningMgr()
    {
        Messenger.AddListener(SlotControllerConstants.OnSpinEnd, OnSpinEnd);
        Messenger.AddListener<bool>(GameConstants.RefreshCashInFree, RefreshCashInFree);
        Messenger.AddListener(OnLineEarningConstants.ResetLuckyCashMsg,ResetSpinTime);
        Messenger.AddListener(GameConstants.OnSlotMachineSceneInit, OnSlotMachineSceneInit);
        Messenger.AddListener<ReelManager,long>(GameConstants.SpinAwardEndMsg,HandleSpinAwardEnd);

        // Messenger.AddListener(GameConstants.IntervalBackToApp, IntervalBackToApp);
    }
    ~OnLineEarningMgr()
    {
        Messenger.RemoveListener(SlotControllerConstants.OnSpinEnd, OnSpinEnd);
        Messenger.RemoveListener<bool>(GameConstants.RefreshCashInFree,RefreshCashInFree);
        Messenger.RemoveListener(OnLineEarningConstants.ResetLuckyCashMsg,ResetSpinTime);
        Messenger.RemoveListener(GameConstants.OnSlotMachineSceneInit, OnSlotMachineSceneInit);
        Messenger.RemoveListener<ReelManager,long>(GameConstants.SpinAwardEndMsg,HandleSpinAwardEnd);
        // Messenger.RemoveListener(GameConstants.IntervalBackToApp, IntervalBackToApp);
    }
    
    void OnSpinEnd()
    {
        AddSpinTime();
        //奖励弹板只弹出一次
        if (UserManager.GetInstance().UserProfile().GetTotalSpinCounter()==newUserSpinCount && !PlatformManager.Instance.IsWhiteBao())
        {
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_SUSPEND);
            new DelayAction(1.3f,null, () =>
            {
                int newUserCash = GetRewardsByName(OnLineEarningConstants.REWARD_NewUser);
                
                ShowRewardCashDialog(newUserCash);
            }).Play();
        }
    }

    //处理300和区间模式下，奖励弹板的弹出
    //当前spin未有任何奖励弹窗弹出时，bigwin,freespin,bonus,rewardcash,withdrawactivity等都不弹出时，计一次数，
    //当计数达到一定值时，弹出奖励弹窗
    //有任何奖励弹窗弹出时次数重置
    void HandleSpinAwardEnd(ReelManager reelManager, long coins)
    {
        if (isWhitePackage)
        {
            return;
        }
        //当前spin结果有bigwin
        if (BaseSlotMachineController.Instance.hasPopReward || BaseSlotMachineController.Instance.isBigWin || BaseSlotMachineController.Instance.isMegaWin || BaseSlotMachineController.Instance.isEpicWin)
        {
            curRewardCount = 0;
            SaveProgressData();
            return;
        }
        //当前spin结果有freespin
        if (reelManager.HitFs||reelManager.isFreespinBonus)
        {
            curRewardCount = 0;
            SaveProgressData();
            return;
        }
        //当前spin结果有bonus
        if (reelManager.HasBonusGame)
        {
            curRewardCount = 0;
            SaveProgressData();
            return;
        }
        //当前spin结果有luckycash奖励弹版
        if (BaseSlotMachineController.Instance.hasPopReward)
        {
            curRewardCount = 0;
            SaveProgressData();
            return;
        }
        curRewardCount++;
        if (curRewardCount>=PopRewardLimit)
        {
            curRewardCount = 0;
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_SUSPEND);
            //满足弹出条件弹出小弹窗
            Messenger.Broadcast<int>(GameDialogManager.OpenExtraAwardCashDialogMsg,GetRewardsByName(OnLineEarningConstants.REWARD_ExtraAward));
        }
        SaveProgressData();
    }
    
    void ShowRewardCashDialog(int cash)
    {
        //弹出奖励现金的弹板
        Messenger.Broadcast<int>(GameDialogManager.OpenRewardCashDialogMsg, cash);
    }
    void LoadProgressData()
    {
        OnLineEarningProgressData  data = StoreManager.Instance.LoadDataJson<OnLineEarningProgressData>(progressData.fileName);
        if (data!=null)
        {
            curRewardCount = data.curRewardCount;
            cash = data.cash;
            progressData.LoadData(data);
        }
    }
        
    public void SaveProgressData()
    {
        progressData.SaveData();
    }
    
    private void RefreshCashInFree(bool show)
    {
        ShowCashInFree = show;
    }
    
    public bool isInfiniteOpen()
    {
        return isOpen && _infiniteModel != null;
    }
    
    public bool isThreeHundredOpen()
    {
        return isOpen && _threeHundredModel != null;
    }
    
    public bool isIntervalDataPatternOpen()
    {
        return isOpen && _intervalDataPattern != null;
    }
    
    public InfiniteModel GetPopPlanConfig()
    {
        return _infiniteModel;
    }
    public ThreeHundredModel GetThreeHundredConfig()
    {
        return _threeHundredModel;
    }
    
    public IntervalDataPattern GetIntervalDataPattern()
    {
        return _intervalDataPattern;
    }
    
    #region Cash

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

    public static bool UnLimitMoney = false;
    public int Cash()
    {
        if (UnLimitMoney)
        {
            return 999999999;
        }
        return cash;
    }
    public int GetCashTime()
    {
        return _baseOnlineEarningModel.GetRewardCount;
    }
    
    public void SetCash(int newCash)
    {
        if (newCash < 0)
        {
            throw new System.ArgumentOutOfRangeException("newCash");
        }
        cash = newCash;
        PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"Cash",cash.ToString());
        SaveProgressData();
    }
    
    public virtual void IncreaseCash(int newCash,bool needMultiple = false)
    {
        if (needMultiple)
        {
            newCash *= _baseOnlineEarningModel.CashMultiple();
        }
        newCash *= GetCashBuffMultiple();
        int totalCash = cash + newCash;
        if (!isInfiniteOpen()&&totalCash>=GetMaxValue()*GetCashMultiple())
        {
            totalCash= (int)Math.Floor(GetMaxValue()*GetCashMultiple()*0.98f);
            newCash = totalCash-cash;
        }
        if (newCash>0)
        {
            Messenger.Broadcast<int>(OnLineEarningConstants.IncreaseCashMsg, newCash);
        }
        SetCash(totalCash);
    }

    //获取金钱的加倍buff数值
    public int GetCashBuffMultiple()
    {
        List<BaseBuff> buffs = BuffManager.Instance.GetBuffByType(BuffConstant.MoreCashBuff);
        int multiple = 0;
        for (int i = 0; i < buffs.Count; i++)
        {
            if (buffs[i].isActive)
            {
                multiple += buffs[i].GetExtraCount();
            }
        }
        return multiple>0?multiple:1;
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

    #endregion
    public bool CanShowH5()
    {
        if (isThreeHundredOpen() )
        {
            return PlatformManager.Instance.CheckCanShowH5() && _threeHundredModel.CanShowH5();
        }

        return PlatformManager.Instance.CheckCanShowH5();
    }
    
    
    public void AddSpinTime()
    {
        if (_baseOnlineEarningModel!=null)
        {
            _baseOnlineEarningModel.AddSpinTime();
        }
    }
    
    //重置奖励弹窗的弹出逻辑
    public void ResetSpinTime()
    {
        if (_baseOnlineEarningModel!=null)
        {
            _baseOnlineEarningModel.ResetSpinTime();
        }
    }

    void OnSlotMachineSceneInit()
    {
        Debug.Log("OnlineEarning OnSlotMachineSceneInit");
        HandleNotifyReward();
    }

    void IntervalBackToApp()
    {
        Debug.Log("OnlineEarning IntervalBackToApp");
        HandleNotifyReward();
    }

    public float notifyReward = 30.88f;
    void HandleNotifyReward()
    {
        if (PlatformManager.Instance.HaveNotifyReward())
        {
            PlatformManager.Instance.PushNotifyReward();
            int reward = (int)Math.Round(notifyReward * GetCashMultiple());
            IncreaseCash(reward);
            ShowRewardCashDialog(reward);
        }
    }
    public bool CheckCanShowBig()
    {
        if (_baseOnlineEarningModel==null)
        {
            return false;
        }
        return _baseOnlineEarningModel.CanShowBig();
    }
    
    public bool CheckCanPopReward()
    {
        if (_baseOnlineEarningModel==null)
        {
            return false;
        }
        return _baseOnlineEarningModel.CheckCanPopReward();
    }
 
    public void AddGetRewardCount()
    {
        _baseOnlineEarningModel?.AddGetRewardCount();
    } 
    public void AddGetH5RewardCount()
    {
        _baseOnlineEarningModel?.AddGetRewardCount();
    } 
    public void PopSmallDialogEnd()
    {
        _baseOnlineEarningModel?.PopSmallDialogEnd();
    }
    public void PopBigDialogEnd()
    {
        _baseOnlineEarningModel?.PopBigDialogEnd();
    }
    
    public int AddPopSpinWinCount()
    {
        PopSpinWinCount++;
        return PopSpinWinCount;
    }
    public void HandleH5Event(float addCash)
    {
        //广播H5加钱
        int amount = Utilities.CastValueInt(addCash * GetCashMultiple());
        _baseOnlineEarningModel.HandleH5Event(amount);
    }
    
    public int GetCashMultiple()
    {
        return _baseOnlineEarningModel.CashMultiple();
    }
    public int GetLevel()
    {
        return _baseOnlineEarningModel.Level();
    }
    public string GetLevelDesc()
    {
        string prekey = "Level.";
        if (isThreeHundredOpen())
        {
            prekey = "Lv.";
        }
        return prekey+ _baseOnlineEarningModel.Level();
    }
    //由于cash根据不同的模式扩大的倍数不同，需要金钱文案显示时，通过此方法转换
    public double ConvertMoneyToDouble(int amount,int decimalPlaces=2,bool needExchange = true)
    {
        int multiple = _baseOnlineEarningModel.CashMultiple();
        
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
    
    
    //获取现金的字符串.针对plist配置 activity 和 reward 配置的相关奖励，不需要图标显示内容
    public string GetCashStr(int amount,int decimalPlace = 2,bool needIcon = false, bool needBigNum = false)
    {
        //amount走的是 plist的配置数量，需要乘以不同网赚模式倍数变为存储值
        amount *= _baseOnlineEarningModel.CashMultiple();
        //获取金钱的字符串
        return GetMoneyStr(amount,decimalPlace,needIcon,needBigNum);
    }
    
    //用于现金任务的显示,无需放大倍数的
    //钱的图标 + 钱的符号 + 保留两位小数的钱的数量
    public string GetMoneyStr(int amount,int decimalPlace = 2,bool needIcon = true,bool needBigNum =false)
    {
        string str = "";
        double money = ConvertMoneyToDouble(amount, decimalPlace);
        if (money<0.01&& money>=0&&!isInfiniteOpen())
        {
            money = 0.01;
        }
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

    private int GetReward()
    {
        return _baseOnlineEarningModel.GetReward();
    }
    public int GetRewardsByName(string key,int level = 0)
    {
        return _baseOnlineEarningModel.GetRewardsByName(key,level);
    }

    public int GetLuckyCashReward()
    {
        return GetRewardsByName(OnLineEarningConstants.REWARD_LUCKYCASH);
    }
    public int GetH5Reward()
    {
        return GetRewardsByName(OnLineEarningConstants.REWARD_H5Reward);
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

    public int GetMaxValue()
    {
        return _baseOnlineEarningModel.GetMaxValue();
    }
    
    public bool IsWhiteBao()
    {
        return isWhitePackage;
    }
}


