using System;
using System.Collections.Generic;
using Libs;
using OnLineEarning;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Core
{
    public struct IntervalReward
    {
        public int intervalMax;
        public int intervalMin;
        public int min;
        public int max;
        public IntervalReward(int intervalMax, int intervalMin, int min, int max)
        {
            this.intervalMax = intervalMax;
            this.intervalMin = intervalMin;
            this.min = min;
            this.max = max;
        }
    }
    
    public class IntervalDataPattern : BaseOnlineEarningModel
    {
        public List<IntervalReward> IntervalRewards = new List<IntervalReward>();
        public int CurSpinLimit = 1;
        private int maxValue = 300;
        private int multiple = 1;
        public override void ParseConfig(Dictionary<string, object> config)
        {
            base.ParseConfig(config);
            CurSpinLimit = Utilities.GetInt(config, OnLineEarningConstants.Spin, 1);
            maxValue = Utilities.GetInt(config, OnLineEarningConstants.MaxValue, 1);
            multiple = Utilities.GetInt(config, OnLineEarningConstants.Multiple, 1);
            List<object> intervalRewards = Utils.Utilities.GetValue<List<object>>(config, OnLineEarningConstants.IntervalArray, null);
            if (intervalRewards == null || intervalRewards.Count == 0)
            {
                return;
            }
            IntervalRewards.Clear();
            for (int i = 0; i < intervalRewards.Count; i++)
            {
                Dictionary<string,object> rewardData = intervalRewards[i] as Dictionary<string, object>;
                if (rewardData == null)
                {
                    continue;
                }
                int intervalMax = Utils.Utilities.GetInt(rewardData, OnLineEarningConstants.intervalMax, 0);
                int intervalMin = Utils.Utilities.GetInt(rewardData, OnLineEarningConstants.intervalMin, 0);
                int min = Utils.Utilities.GetInt(rewardData, OnLineEarningConstants.min, 0);
                int max = Utils.Utilities.GetInt(rewardData, OnLineEarningConstants.max, 0);
                IntervalReward intervalReward = new IntervalReward(intervalMax*CashMultiple(), intervalMin*CashMultiple(), min, max);
                IntervalRewards.Add(intervalReward);
            }
            if (IntervalRewards.Count == 0)
            {
                Debug.LogWarning("IntervalDataPattern ParseConfig IntervalRewards is empty");
            }
        }
        
        public override bool CanShowBig()
        {
            return CurLuckyADNumber >= LuckyADLimit && CurSpinTime>=CurSpinLimit;
        }
        
        public override bool CheckCanPopReward()
        {
            return CanShowBig();
        }
        
        public override void PopSmallDialogEnd()
        {
            CurSpinTime =0;
        }
        
        public override void PopBigDialogEnd()
        {
            CurSpinTime =0;
            AddBigPop();
        }
        
        public override void HandleH5Event(int amount)
        {
            if (OnLineEarningMgr.Instance.Cash() + amount< GetMaxValue() * CashMultiple())
            {
                OnLineEarningMgr.Instance.IncreaseCash(amount);
                Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            }
            else if(OnLineEarningMgr.Instance.Cash()+amount>GetMaxValue()*99.9)
            {
                Messenger.Broadcast<bool>(GameConstants.OnH5InitSuccess,false);
                SharedPlayerPrefs.SetPlayerPrefsBoolValue(OnLineEarningConstants.HideH5TagKey,false);
            }
        }

        public override int GetReward()
        {
            int rewardNum = 0;
            int cash = OnLineEarningMgr.Instance.Cash();
            for (int i = 0; i < IntervalRewards.Count; i++)
            {
                IntervalReward intervalReward = IntervalRewards[i];
                if (cash >= intervalReward.intervalMin && cash < intervalReward.intervalMax)
                {
                    rewardNum = Random.Range(intervalReward.min, intervalReward.max + 1);
                    break;
                }
            }
            int newNum = cash + rewardNum;
            if (newNum>=maxValue*CashMultiple())
            {
                rewardNum = 0;
                //关闭H5按钮
                SharedPlayerPrefs.SetPlayerPrefsBoolValue(OnLineEarningConstants.HideH5TagKey,false);
                Messenger.Broadcast<bool>(GameConstants.OnH5InitSuccess,false);
                //通知原生部分
                PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UserAmount,newNum);
            }
            return rewardNum;
        }

        public override bool CanShowH5()
        {
            return SharedPlayerPrefs.GetPlayerBoolValue(OnLineEarningConstants.HideH5TagKey,true)&& PlatformManager.Instance.CheckCanShowH5();
        }

        public override int GetMaxValue()
        {
            return maxValue;
        }

        public override int GetRewardsByName(string key, int level = 0)
        {
            int reward = 0;
            if (key == OnLineEarningConstants.REWARD_SPIN||key == OnLineEarningConstants.REWARD_FREESPIN)
            {
                //普通spin不给钱
                return 0;
            }

            if (key == OnLineEarningConstants.REWARD_NewUser)
            {
                BaseOnLineEarningTimer timer = GetTimerByName(key);
                return timer.GetReward(level);
            }

            reward = GetReward();
            if (key == OnLineEarningConstants.REWARD_ExtraAward)
            {
                reward =(int)Math.Floor(reward * OnLineEarningMgr.Instance.popRewardRate);
            }
            return reward;
        }
    }
}