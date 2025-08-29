using System;
using System.Collections.Generic;
using System.Numerics;
using Classic;
using Libs;
using OnLineEarning;
using Utils;

namespace Core
{
    public class ThreeHundredModel : BaseOnlineEarningModel
    {
        static readonly string SmallInterval_KEY = "Small";
        static readonly string SpinInterval_Key = "Spin";
        static readonly string VedioLimitKey = "VedioLimit";
        static readonly string VedioMultipleKey = "VedioMultiple";
        static readonly string CurADNumberKey = "CurADNumberKey";
        static readonly string LimitCountKey = "LimitCount";
        static readonly string LimitRateKey = "LimitRate";
        static readonly string LimitRewardKey = "LimitReward";
        static readonly string RewardArrayKey = "RewardArray";

        static readonly string GetFirstRewardTimeKey = "GetFirstRewardTime";
        static readonly string HideH5TagKey = "HideH5TagKey";
        
        static readonly string TodayGetRewardCountKey = "TodayGetRewardCountKey";
        static readonly string IsSecondKey = "IsSecondKey";
        static readonly string MaxValueKey = "MaxValue";
        static readonly string CalculateCountKey = "CalculateCountList";

        //此字段用来标志第二个数组的取值下标，满足走第二个数组条件时会一直累加
        private int GetRewardCountInToday
        {
            set
            {
                SharedPlayerPrefs.SetPlayerPrefsIntValue(TodayGetRewardCountKey, value);
            }
            get
            {
                int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(TodayGetRewardCountKey, 0);
                return value;
            }
        }
        
      
     
        public int CurSmallLimit = 3;
        public int CurSpinLimit = 1;
        public int RewardCountLimit = 20;

        //正常概率为0.95
        private float limitRate = 0.95f;
        //为了数值精确，扩大了CashMultiple倍
        private int LimitRate
        {
            get
            {
                return 9500;
            }
        }
        private int minReward = 100;
        private int maxReward = 200;
        private int[][] arrayOne;
        private int[][] arrayTwo;
        private int maxValue = 300;

        private List<object> CalculateCountList = new List<object>();
        private long getFirstRewardTime
        {
            set
            {
                SharedPlayerPrefs.SavePlayerPrefsLong(GetFirstRewardTimeKey, value);
            }
            get
            {
                return SharedPlayerPrefs.LoadPlayerPrefsLong(GetFirstRewardTimeKey, 0);
            }
        }
       
        public override void ParseConfig(Dictionary<string, object> config)
        {
            CurSmallLimit = Utilities.GetInt(config, SmallInterval_KEY, 3);
            CurSpinLimit = Utilities.GetInt(config, SpinInterval_Key, 1);
            RewardCountLimit = Utilities.GetInt(config, LimitCountKey, 1);
            maxValue = Utilities.GetInt(config, MaxValueKey, 1);
            CalculateCountList = Utilities.GetValue<List<object>>(config, CalculateCountKey, null);
            List<object> array = Utilities.GetValue<List<object>>(config, LimitRewardKey, null);
            if (array.Count == 2)
            {
                minReward = (int)array[0];
                maxReward = (int)array[1];
            }
            base.ParseConfig(config);
            InitData();
            InitRewardArray(config);
        }

        private void InitData()
        {
            if (SharedPlayerPrefs.GetPlayerPrefsIntValue(IsSecondKey,0)==0)
            {
                DateTime saveTime = TimeUtils.ToDateTimeFromTimeStamp(getFirstRewardTime);
                bool isSameDay = TimeUtils.IsSameDay(DateTime.Now, saveTime);
                if (getFirstRewardTime!=0 && !isSameDay)
                {
                    //重置今天的领奖次数
                    GetRewardCountInToday = 0;
                    //设置标志防止重复重置
                    SharedPlayerPrefs.SetPlayerPrefsIntValue(IsSecondKey,1);
                }
            }
        }
        
        private void InitRewardArray(Dictionary<string, object> config)
        {
            List<object> RewardArray = Utilities.GetValue<List<object>>(config, RewardArrayKey, null);
            if (RewardArray == null || RewardArray.Count < 2)
            {
                throw new Exception("RewardArray is not configured correctly.");
                return;
            }
            List<object> arrayOneList = RewardArray[0] as List<object>;
            for (int i = 0; i < arrayOneList.Count; i++)
            {
                List<object> arrayOneItem = arrayOneList[i] as List<object>;
                if (arrayOneItem != null && arrayOneItem.Count == 2)
                {
                    if (arrayOne==null)
                    {
                        arrayOne = new int[arrayOneList.Count][];
                    }
                    arrayOne[i] = new int[]{(int)arrayOneItem[0], (int)arrayOneItem[1]};
                }
                else
                {
                    throw new Exception($"Invalid data format in RewardArray at index {i}.");
                }
            }
            
            List<object> arrayTwoList = RewardArray[1] as List<object>;
            for (int i = 0; i < arrayTwoList.Count; i++)
            {
                List<object> arrayTwoItem = arrayTwoList[i] as List<object>;
                if (arrayTwoItem != null && arrayTwoItem.Count == 2)
                {
                    if (arrayTwo==null)
                    {
                        arrayTwo = new int[arrayTwoList.Count][];
                    }
                    arrayTwo[i] = new int[]{(int)arrayTwoItem[0], (int)arrayTwoItem[1]};
                }
            }
        }
        
        public int GetMaxValue()
        {
            return maxValue;
        }
        
        
        //玩家当前所获金钱超过这个值需要做特殊判断
        //已扩大CashMultiple
        public int GetLimitCount()
        {
            int money= GetMaxValue();;
            return money * LimitRate;
        }
        
        public int GetReward(bool nextValue)
        {
            //钱数是扩大CashMultiple倍的数值
            int totalMoney = OnLineEarningMgr.Instance.Cash();
            //金钱临界值 
            int limitMoney = GetLimitCount();
            //最大钱数
            int maxValue = GetMaxValue() * CashMultiple();
            DateTime saveTime = TimeUtils.ToDateTimeFromTimeStamp(getFirstRewardTime);
            bool isSameDay = TimeUtils.IsSameDay(DateTime.Now, saveTime);
            if (totalMoney>=limitMoney)
            {
                return HandleMaxReward(totalMoney,maxValue);
            }

            if (getFirstRewardTime == 0 || isSameDay)
            {
                return this.HandleFirstReward(nextValue);
            }
            return HandleRegularReward(nextValue);
        }

        private int HandleMaxReward(int totalMoney,int maxValue)
        {
            Random random = new Random();
            int rateNum = random.Next(minReward, maxReward);
            int rewardNum = (maxValue - totalMoney) / rateNum;
            int newNum = totalMoney + rewardNum;
            if (newNum>=maxValue)
            {
                rewardNum = 0;
                //关闭H5按钮
                SharedPlayerPrefs.SetPlayerPrefsBoolValue(HideH5TagKey,false);
                Messenger.Broadcast<bool>(GameConstants.OnH5InitSuccess,false);
                //通知原生部分
                PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UserAmount,newNum);
            }
            return rewardNum;
        }

        private int HandleFirstReward(bool nextValue)
        {
            Random random = new Random();
            int rewardNum = 0;
            if (GetRewardCount>=this.arrayOne.Length)
            {
                rewardNum = random.Next(minReward, maxReward);
            }
            else
            {
                int[] array = nextValue ? this.arrayOne[GetRewardCount + 1] : this.arrayOne[GetRewardCount];
                // rewardNum = GetRewardCount == 1 ? array[1] : random.Next(array[0], array[1]);
                //第一个奖励随机，不再指定
                rewardNum = random.Next(array[0], array[1]);
                if (GetRewardCount ==1)
                {
                    //存储第一次获奖时间
                    getFirstRewardTime = TimeUtils.GetTimestampInSeconds(DateTime.Now);
                }
            }
            return rewardNum;
        }
        /**非第一天奖励*/
        private int HandleRegularReward(bool nextValue)
        {
            Random random = new Random();
            int rewardNum = 0;
            //获奖次数小于 20次使用列表 1
            if (GetRewardCount<this.RewardCountLimit)
            {
                int[] array = nextValue ? this.arrayOne[GetRewardCount + 1] : this.arrayOne[GetRewardCount];
                rewardNum = random.Next(array[0], array[1]);
            }
            else
            {
                if (GetRewardCountInToday>arrayTwo.Length)
                {
                    rewardNum = random.Next(minReward, maxReward);
                }
                else
                {
                    int[] array = nextValue ? this.arrayTwo[GetRewardCountInToday + 1] : this.arrayTwo[GetRewardCountInToday];
                    rewardNum = random.Next(array[0], array[1]);
                }
            }
            return rewardNum;
        }
        public override int CashMultiple()
        {
            return 10000;
        }
        public override void PopSmallDialogEnd()
        {
            CurSpinTime =0;
            AddSmallPop();
        }
        public override void PopBigDialogEnd()
        {
            SmallDialogPopNum = 0;
            CurSpinTime =0;
            AddBigPop();
        }
        public override int GetBigRewardMultiple()
        {
            return 2;
        }   
        public override void AddGetRewardCount()
        {
            GetRewardCount++;
            if (CanShowH5())
            {
                Messenger.Broadcast<bool>(GameConstants.OnH5InitSuccess,true);
            }
            DateTime saveTime = TimeUtils.ToDateTimeFromTimeStamp(getFirstRewardTime);
            bool isSameDay = TimeUtils.IsSameDay(DateTime.Now, saveTime);
            if (!isSameDay && GetRewardCount>=RewardCountLimit)
            {
                GetRewardCountInToday++;
            }

            int newlevel = (int)Math.Ceiling(GetRewardCount/10.0f);
            if (newlevel >PopLevel)
            {
                PopLevel = newlevel;
                //广播升级
                Messenger.Broadcast(SlotControllerConstants.OnPopLevelChange);
            }
        }
        
        public override bool CheckCanPopReward()
        {
            return CurSpinTime>=CurSpinLimit;
        }

        public override bool CanShowBig()
        {
            return SmallDialogPopNum >= CurSmallLimit;
        }
        public override bool CanShowH5()
        {
            return GetRewardCount >=1 && SharedPlayerPrefs.GetPlayerBoolValue(HideH5TagKey,true)&& PlatformManager.Instance.CheckCanShowH5();
        }

        public override int GetH5Reward()
        {
            return GetReward(true);
        }
        
        public override void HandleH5Event(int amount)
        {
            if (OnLineEarningMgr.Instance.Cash() + amount< GetMaxValue() * CashMultiple())
            {
                OnLineEarningMgr.Instance.IncreaseCash(amount);
                Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            }
            else if(OnLineEarningMgr.Instance.Cash()+amount>GetMaxValue()*9990)
            {
                Messenger.Broadcast<bool>(GameConstants.OnH5InitSuccess,false);
                SharedPlayerPrefs.SetPlayerPrefsBoolValue(HideH5TagKey,false);
            }
        }

        int SetCalculateCount()
        {
            if (CalculateCountList ==null || CalculateCountList.Count==0)
            {
                return 1;
            }

            foreach (var item in CalculateCountList)
            {
                Dictionary<string, object> itemdict = item as Dictionary<string, object>;
                int limit = Utilities.GetInt(itemdict, "limit", 0);
                int count =Utilities.GetInt(itemdict, "count", 0);
                if (GetRewardCount<limit)
                {
                    return count;
                }
            }
            return 1;
        }
        #region Rewards
        public override int GetRewardsByName(string key,int level = 0)
        {
            int reward = 0;
            if (key == OnLineEarningConstants.REWARD_SPIN||key == OnLineEarningConstants.REWARD_FREESPIN)
            {
                //普通spin不给钱
                return 0;
            }
            
            if (key == OnLineEarningConstants.REWARD_NewUser)
            {
                //增加领奖次数
                AddGetRewardCount();
                reward = GetReward(false);

                return reward;
            }
            
            int count = SetCalculateCount();
            //每次计算CalculateCount次,等价爬几个数组
            for (int i = 0; i < count; i++)
            {
                //增加领奖次数
                AddGetRewardCount();
                BaseOnLineEarningTimer timer = GetTimerByName(key);
                if (timer!=null)
                {
                    //不为空说明有特殊规则
                    reward += timer.GetReward();
                }
                else
                {
                    //为空说明没有特殊规则，直接走列表
                    if (key == OnLineEarningConstants.REWARD_H5Reward)
                    {
                        reward +=  GetReward(true);
                    }
                    else 
                    {
                        reward +=  GetReward(false);
                    } 
                }
            }
            if (key == OnLineEarningConstants.REWARD_ExtraAward)
            {
                reward =(int)(reward * OnLineEarningMgr.Instance.popRewardRate);
            }
            return reward;
        }
        #endregion
    }
}