using System.Collections.Generic;
using OnLineEarning;
using UnityEngine;
using Utils;

namespace Core
{
    public class BaseOnlineEarningModel
    {
        #region PlayerPrefs Keys

        public const string CurLuckyADNumberKey = "CurLuckyADNumberKey";

        #endregion
       

        public const string BigPopNumKey = "BigPopNum";
        public const string SmallPopNumKey = "SmallPopNum";
        public const string SpinIntervalTimeKey = "SpinInternalTime";
        public const string GetRewardCountKey = "GetRewardCountKey";
        public const string GetH5RewardCountCountKey = "GetH5RewardCountCountKey";

        public const string PopLevelKey = "PopLevelKey";
        
        public int withDrawMoney = 0;
        //LuckyCash广告弹出次数限制,前15次spin不弹出luckycash弹窗,之后开始计数，按照当前 level spinlimit弹出
        public int LuckyADLimit = 15;
        
        //插屏广告翻倍奖励的倍数，默认为 1
        public int ADMultiple = 1;
        
        private int Multiple = 1;

        public int CurLuckyADNumber
        {
            set
            {      
                SharedPlayerPrefs.SetPlayerPrefsIntValue(CurLuckyADNumberKey, value);
            }
            get
            {
                int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(CurLuckyADNumberKey, 0);
                return value;
            }
        }
   
     
        //大弹窗总的弹出次数，一直累计
        public int BigDialogPopNum
        {
            set
            {      
                SharedPlayerPrefs.SetPlayerPrefsIntValue(BigPopNumKey, value);
            }
            get
            {
                int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(BigPopNumKey, 0);
                return value;
            }
        }
        
        //大弹窗弹出后,重置为0
        public int SmallDialogPopNum
        {
            set
            {      
                SharedPlayerPrefs.SetPlayerPrefsIntValue(SmallPopNumKey, value);
            }
            get
            {
                int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(SmallPopNumKey, 0);
                return value;
            }
        }
        //当前 spin 次数
        public int CurSpinTime
        {
            set
            {      
                SharedPlayerPrefs.SetPlayerPrefsIntValue(SpinIntervalTimeKey, value);
            }
            get
            {
                int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(SpinIntervalTimeKey, 0);
                return value;
            }
        }
        //玩家领奖次数
        public int GetRewardCount
        {
            set
            {
                SharedPlayerPrefs.SetPlayerPrefsIntValue(GetRewardCountKey, value);
            }
            get
            {
                int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(GetRewardCountKey, 0);
                return value;
            }
        }
        //弹窗弹出次数
        public int GetH5RewardCount
        {
            set
            {
                SharedPlayerPrefs.SetPlayerPrefsIntValue(GetH5RewardCountCountKey, value);
            }
            get
            {
                int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(GetH5RewardCountCountKey, 0);
                return value;
            }
        }
        
        public int PopLevel
        {
            set
            {      
                SharedPlayerPrefs.SetPlayerPrefsIntValue(PopLevelKey, value);
            }
            get
            {
                int value = SharedPlayerPrefs.GetPlayerPrefsIntValue(PopLevelKey, 1);
                return value;
            }
        }
        
        
        Dictionary<string,BaseOnLineEarningTimer> rewardTimers = new Dictionary<string, BaseOnLineEarningTimer>();

        public virtual void ParseConfig(Dictionary<string,object> config)
        {
            LuckyADLimit = Utilities.GetInt(config,OnLineEarningConstants.LuckyVideoLimit,15);
            Multiple = Utilities.GetInt(config,OnLineEarningConstants.Multiple,100);
            Dictionary<string,object> rewards = Utils.Utilities.GetValue<Dictionary<string,object>>(config,OnLineEarningConstants.RewardTimerKey,null);
            if (rewards == null)
            {
                Debug.LogWarning("BaseOnlineEarningModel ParseConfig rewards is null");
                return;
            }

            foreach (var item in rewards)
            {
                string name = item.Key;
                Dictionary<string,object> rewardData = item.Value as Dictionary<string, object>;
                if (rewardData==null)
                {
                    continue;
                }
                BaseOnLineEarningTimer timer= CreateRewardTimer(name, rewardData);
                rewardTimers[name] = timer;
            }
        }

        BaseOnLineEarningTimer CreateRewardTimer(string name,Dictionary<string,object> rewardData)
        {
            BaseOnLineEarningTimer timer = null;
            if (name == OnLineEarningConstants.REWARD_NewUser)
            {
                timer = new NewUserTimer(name, rewardData);
            }
            else if (name == OnLineEarningConstants.REWARD_SPIN)
            {
                timer = new NormalSpinTimer(name, rewardData);

            }else if (name == OnLineEarningConstants.REWARD_FREESPIN)
            {
                timer = new FreeSpinTimer(name, rewardData);
            }
            else if (name == OnLineEarningConstants.REWARD_LUCKYCASH)
            {
                timer = new LuckyCashTimer(name, rewardData);
            }
            else if (name == OnLineEarningConstants.REWARD_JACKPOT)
            {
                timer = new JackPotGameEndTimer(name, rewardData);
            }
            else if (name == OnLineEarningConstants.REWARD_SPINWIN)
            {
                timer = new SpinWinTimer(name, rewardData);
            }
            else if (name == OnLineEarningConstants.REWARD_H5Reward)
            {
                timer = new OpenH5RewardTimer(name, rewardData);
            }
            else if (name == OnLineEarningConstants.REWARD_FREEGAMEEND)
            {
                timer = new FreeGameEndTimer(name, rewardData);
            }
            else if (name == OnLineEarningConstants.REWARD_ExtraAward)
            {
                timer = new ExtraRewardTimer(name, rewardData);
            }
            return timer;
        }

        public virtual int GetRewardByName(string name, int level = 0)
        {
            if (rewardTimers.ContainsKey(name))
            {
                return rewardTimers[name].GetReward(level);
            }
            Debug.LogError($"BaseOnlineEarningModel GetRewardByName name:{name} not found");
            return 0;
        }
        
        public virtual bool CanShowH5()
        {
            return false;
        }

        public virtual int CashMultiple()
        {
            return Multiple;
        }
        
        public virtual bool CanShowBig()
        {
            return false;
        }
        public virtual bool CheckCanPopReward()
        {
            return false;
        }

        public virtual void PopSmallDialogEnd()
        {
        }
        
        public virtual void PopBigDialogEnd()
        {
        }
        
        public void AddSpinTime()
        {
            if (CurLuckyADNumber<LuckyADLimit)
            {
                //前20次不弹luckycash弹板
                AddLuckyADNum();
            }
            else
            {
                // Debug.Log($"AddSpinTime CurSpinTime:{CurSpinTime}");
                CurSpinTime++;
            }
        }
        
        public void ResetSpinTime()
        {
            CurSpinTime=0;
        }
        
        public void AddSmallPop()
        {
            SmallDialogPopNum++;
        }
        public void AddBigPop()
        {
            BigDialogPopNum++;
        }
        
        public void AddLuckyADNum()
        {
            CurLuckyADNumber++;
        } 
        
        public virtual int GetReward()
        {
            return 0;
        }
        
        public virtual int GetBigReward()
        {
           return 0;
        }
        public virtual int GetBigRewardMultiple()
        {
            return 0;
        } 
      
        public virtual int GetH5Reward()
        {
            return 0;
        }
        public virtual int GetMaxValue()
        {
            return 1;
        }
        public virtual void AddGetRewardCount()
        {
            GetRewardCount++;
        }
        public virtual void AddGetH5RewardCount()
        {
            GetH5RewardCount++;
        }
        
        //弹窗弹出奖励次数+h5点击获取奖励次数
        private int GetTotalH5RewardCount()
        {
            return GetRewardCount + GetH5RewardCount;
        }
        public virtual void HandleH5Event(int amount)
        {
            
        }

        public virtual int Level()
        {
            return PopLevel;
        }

        public virtual int GetRewardsByName(string key,int level = 0)
        {
            return 0;
        }

        public BaseOnLineEarningTimer GetTimerByName(string key)
        {
            if (rewardTimers.ContainsKey(key))
            {
                return rewardTimers[key];
            }

            return null;
        }
    }
}