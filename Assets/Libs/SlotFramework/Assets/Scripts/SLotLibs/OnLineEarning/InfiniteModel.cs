using System;
using System.Collections.Generic;
using OnLineEarning;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Core
{
    public struct PopPlanItem
    {
        //等级
        public int level;
        //弹出大弹窗的总数，超过大弹窗即为当前等级
        public int bigLimit;
        //在当前等级累计弹出多少个小弹窗才弹一个大弹窗
        public int smallInterval;
        //几次spin后弹出小弹窗
        public int spinLimit;

        public PopPlanItem(int level,int bigLimit,int smallInterval,int spinLimit)
        {
            this.level = level;
            this.bigLimit = bigLimit;
            this.smallInterval = smallInterval;
            this.spinLimit = spinLimit;
        }
    }
    public class InfiniteModel : BaseOnlineEarningModel
    {
        static readonly string PopPlan_Key = "PopPlan";
        static readonly string PopReward_Key = "Rewards";
     
       
        static readonly string WithDraw_Key = "WithDraw";

        static readonly string Money_KEY = "money"; 
        static readonly string Level_KEY = "level"; 
        static readonly string BigLimit_KEY = "bigLimit";        
        static readonly string SmallInterval_KEY = "small";
        static readonly string SpinInterval_Key = "spin";
        

        public const string LuckyVedioLimit = "LuckyVedioLimit";

        public int CurLevelSmallLimit = 0;
        public int CurLevelBigLimit = 0;
        public int CurSpinLimit = 0;

        private List<PopPlanItem> PopPlanList = new List<PopPlanItem>();

        public override void ParseConfig(Dictionary<string,object> config)
        {
            if (config == null)
            {
                return;
            }            
            LuckyADLimit = Utilities.GetInt(config, LuckyVedioLimit, 15);
            List<object> popPlan = Utils.Utilities.GetValue<List<object>>(config, PopPlan_Key, null);
            for (int i = 0; i < popPlan.Count; i++)
            {
                Dictionary<string, object> info = popPlan[i] as Dictionary<string, object>;
                if (info == null)
                {
                    return;
                }

                int level = Utils.Utilities.GetValue<int>(info, Level_KEY, 0);
                int bigLimit = Utils.Utilities.GetValue<int>(info, BigLimit_KEY, 0);
                int smallInterval = Utils.Utilities.GetValue<int>(info, SmallInterval_KEY, 0);
                int spinInterval = Utils.Utilities.GetValue<int>(info, SpinInterval_Key, 0);
                PopPlanItem item = new PopPlanItem(level, bigLimit, smallInterval,spinInterval);
                PopPlanList.Add(item);
            }
             PopPlanList.Sort((PopPlanItem item1,PopPlanItem item2)=>
            {
                if (item1.level < item2.level)
                {
                    return -1;
                }
                return 1;
            });
            PopLevel = GetCurrentLevel();
            CurLevelSmallLimit = GetSmallIntervalByLevel(PopLevel);
            CurLevelBigLimit = GetBigLimitByLevel(PopLevel);
            CurSpinLimit = GetSpinLimitByLevel(PopLevel);
            Debug.Log("InfiniteModel CurLevel ="+PopLevel+"   SmallDialogPopNum="+SmallDialogPopNum+"    BigDialogPopNum = "+BigDialogPopNum);
            base.ParseConfig(config);
        }
        
        public int GetMaxLevel()
        {
            return PopPlanList.Count;
        }

        public int WithDrawMoney()
        {
            return withDrawMoney;
        }
        
        public override int GetBigRewardMultiple()
        {
            return 2;
        }   
        public int GetBigLimitByLevel(int level)
        {
            return PopPlanList[level - 1].bigLimit;
        }
        public int GetSmallIntervalByLevel(int level)
        {
            return PopPlanList[level - 1].smallInterval;
        }
        public int GetSpinLimitByLevel(int level)
        {
            return PopPlanList[level - 1].spinLimit;
        }

        public int GetCurrentLevel()
        {
            for (int i = 0; i < PopPlanList.Count; i++)
            {
                if (PopPlanList[i].bigLimit==-1)
                {
                    return PopPlanList[i].level;
                }
                if (PopPlanList[i].bigLimit>BigDialogPopNum)
                {
                    return PopPlanList[i].level;
                }
            }
            return 1;
        }
        public int Level()
        {
            return PopLevel;
        }
        
        //检测能否弹出Luckycash弹窗
        public override bool CheckCanPopReward()
        {
            return CanShowBig();
        }
        
        //弹出小弹窗，小弹窗弹出后，CurSpinLimit 需要重置为 0
        public override void PopSmallDialogEnd()
        {
            CurSpinTime =0;
            AddSmallPop();
        }
        
        //前20次不弹弹窗
        public override bool CanShowBig()
        {
            return CurLuckyADNumber >= LuckyADLimit && CurSpinTime>=CurSpinLimit;
        }

        //大弹窗弹出后，检测是否可以升级
        public bool CheckCanLevelUp()
        {
            //最后一个等级为无限
            if (CurLevelBigLimit == -1)
            {
                return false;
            }
            return BigDialogPopNum>=CurLevelBigLimit;
        }
  
        //等级加一,重置数据
        public void AddLevel()
        {
            PopLevel += 1;
            Reset();
            Debug.Log("InfiniteModel AddLevel CurLevel ="+PopLevel);
            Messenger.Broadcast(SlotControllerConstants.OnPopLevelChange);
        }

        public void Reset()
        {
            CurLevelSmallLimit = GetSmallIntervalByLevel(PopLevel);
            CurLevelBigLimit = GetBigLimitByLevel(PopLevel);
            CurSpinLimit = GetSpinLimitByLevel(PopLevel);
            Debug.Log("InfiniteModel Reset CurSpinLimit ="+CurSpinLimit+"\nCurLevelSmallLimit="+CurLevelSmallLimit+"\nCurLevelBigLimit = "+CurLevelSmallLimit);
        }
        
        //弹出大弹窗
        public override void PopBigDialogEnd()
        {
            SmallDialogPopNum = 0;
            CurSpinTime =0;
            AddBigPop();
            if (CheckCanLevelUp())
            {
                AddLevel();
            }
        }
        
        public int GetADRewardCash()
        {
            int reward = GetBigReward() * ADMultiple;
            return reward;
        }

        public override void HandleH5Event(int amount)
        {
            base.HandleH5Event(amount);
            //此处应根据不同的模式来做处理
            OnLineEarningMgr.Instance.IncreaseCash(amount);
            Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
        }

        #region Rewards
        public override int GetRewardsByName(string key,int level = 0)
        {
            int reward = 0;
            BaseOnLineEarningTimer timer = GetTimerByName(key);
            return timer.GetReward(level);
        }
        #endregion
    }
}