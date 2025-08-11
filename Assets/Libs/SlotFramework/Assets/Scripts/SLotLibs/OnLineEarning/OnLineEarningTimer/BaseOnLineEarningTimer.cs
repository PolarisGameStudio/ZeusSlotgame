
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace OnLineEarning
{
    public enum OnLineRewardType
    {
        Random = 0,
        Level = 1,
        ThreeHundred = 2,
    }
    /// <summary>
    /// 奖励节点的功能接口
    /// 1.奖励节点是否满足 2.cash的数量 3.奖励次数
    /// </summary>
    public class BaseOnLineEarningTimer
    {
        readonly string Random_Key = "Random";
        readonly string Min_Key = "min";
        readonly string Max_Key = "max";
        readonly string Level_Key = "Level";
        readonly string Range_Key = "Range";
       

        public string name = "";
        private int rewardModel = 0;
        public string DataSaveKey = "";
        public int min;
        public int max;
        public OnLineRewardType type;
        public List<int> rewards;
        private float range;//用于 level 的浮点范围
        private float rate;//用于300模式奖励的缩放比例

        private Dictionary<string, object> data = new Dictionary<string, object>();
        public BaseOnLineEarningTimer(string name,Dictionary<string,object> config = null)
        {
            if (config == null)
            {
                return;
            }
            this.name = name;
            DataSaveKey = OnLineEarningConstants.Prefix;
            ParseConfig(config);
        }
        
        void ParseConfig(Dictionary<string,object> data)
        {
            if (OnLineEarningMgr.Instance.isThreeHundredOpen())
            {
                type = OnLineRewardType.ThreeHundred;
            }else if (data.ContainsKey(Random_Key))
            {
                //random类型在 min和 max区间取值
                type = OnLineRewardType.Random;
                Dictionary<string, object> config =
                    Utilities.GetValue<Dictionary<string, object>>(data, Random_Key, null);
                min = Utilities.GetValue<int>(config, Min_Key, 0);
                max = Utilities.GetValue<int>(config, Max_Key, 0);
            }else if (data.ContainsKey(Level_Key))
            {
                range = Utilities.GetFloat(data, Range_Key, 0);
                type = OnLineRewardType.Level;
                List<object> rewardList = Utilities.GetValue<List<object>>(data, Level_Key, null);
                if (rewardList==null || rewardList.Count==0)
                {
                    Debug.LogError("ParsePopRewards have Error node name ===="+name);
                    return;
                }
                if (rewards==null)
                {
                    rewards = new List<int>();
                }
                //Todo此处拆箱操作后续想办法优化
                for (int i = 0; i < rewardList.Count; i++)
                {
                    rewards.Add((int)rewardList[i]);
                }
            }
        }
        
        /// <summary>
        /// 获取奖励值
        /// </summary>
        /// <param name="level">针对Level类型获取奖励时传入的值</param>
        /// <returns></returns>
        public virtual int GetReward(int level = 0)
        {
            int num = 0;
            if (type == OnLineRewardType.Random)
            {
                num = Random.Range(min, max);
            }else if (type == OnLineRewardType.Level)
            {
                int baseNum = rewards[level];
                float minNum = baseNum * (1 - range);
                float maxNum = baseNum * (1 + range);
                //向下取整
                num = (int)Math.Floor(Random.Range(minNum,maxNum));
            }else if (type ==OnLineRewardType.ThreeHundred)
            {
                num = GetReward300();
            }
            return num;
        }

        public virtual int GetReward300()
        {
            if (OnLineEarningMgr.Instance.isThreeHundredOpen())
            {
                return OnLineEarningMgr.Instance.GetThreeHundredConfig().GetReward(false);
            }
            return 0;
        }
        
        public virtual bool IsConditionMeet()
        {
            return true;
        }

        public virtual void DoAction()
        {
            
        }
    }
}

