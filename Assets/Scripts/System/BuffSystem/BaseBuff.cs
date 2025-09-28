using System.Collections.Generic;
using Libs;

namespace System.BuffSystem
{
    [Serializable]
    public class BaseBuff
    {
        //持续时间
        public int duration;
        //已生效时间
        public float activiteTime;
        //开始时间
        public long startTime;
        //结束时间 -1为永久存在
        public long endTime;
        //是否激活
        public bool isActive;
        //buff类型
        public int buffType;
        //目标数量
        public int targetNum;
        //当前数量
        public int currentNum;
        //唯一ID
        public int buffId;
        //是否可用
        public bool isAvailable = true;
        public string buffName;

        public string UpdateBuffMsg = "";
        //初始化标志，在app启动完成后设置为true
        public bool isInitialized = false;

        public BaseBuff(Dictionary<string,object> dict)
        {
            buffType = Utils.Utilities.GetInt(dict,BuffConstant.BuffTypeKey);
            buffId = Utils.Utilities.GetInt(dict,BuffConstant.BuffIdKey);
            duration = Utils.Utilities.GetInt(dict,BuffConstant.DurationKey,0);
            startTime = Utils.Utilities.GetLong(dict,BuffConstant.StartTimeKey,0);
            endTime = Utils.Utilities.GetLong(dict,BuffConstant.EndTimeKey,0);
            targetNum = Utils.Utilities.GetInt(dict,BuffConstant.TargetNumKey);
            currentNum = Utils.Utilities.GetInt(dict,BuffConstant.CurrentNumKey);
            buffName = Utils.Utilities.GetString(dict,BuffConstant.BuffNameKey,"");
            isActive = false;
            UpdateBuffMsg = "UpdateBuffMsg" + buffId;
        }

        //是否激活
        public virtual bool CheckActive()
        {
            return isActive;
        }
        //是否生效
        public virtual bool IsAvailable()
        {
            return isAvailable;
        }
        /// <summary>
        /// Buff激活时调用
        /// </summary>
        public virtual void OnActive()
        {
            //重置进度
            currentNum = 0;
            activiteTime = 0;
            isActive = true;
            isInitialized = true;
            Messenger.Broadcast<bool>(UpdateBuffMsg,true);
        }
        /// <summary>
        /// Buff失效时调用
        /// </summary>
        public virtual void OnDeactivate()
        {
            Messenger.Broadcast<bool>(UpdateBuffMsg,false);
        }

        /// <summary>
        /// Buff起作用时调用
        /// </summary>
        public virtual void OnTrigger()
        {
            
        }
        
        /// <summary>
        /// 检查是否可以激活Buff
        /// </summary>
        protected virtual bool CanActivate()
        {
            // 可被重写以添加激活条件检查
            return true;
        }

        public void IsInitialized()
        {
            isInitialized = true;
        }
        
        /// <summary>
        /// Buff更新（仅在激活状态调用）
        /// </summary>
        public virtual void OnUpdate(float deltaTime)
        {
        }

        public virtual int GetExtraCount()
        {
            return 0;
        }
        
        public virtual void CloneBuff(BaseBuff other)
        {
            if (other == null) return;
            this.duration = other.duration;
            this.activiteTime = other.activiteTime;
            this.startTime = other.startTime;
            this.endTime = other.endTime;
            this.buffType = other.buffType;
            this.targetNum = other.targetNum;
            this.currentNum = other.currentNum;
            this.buffId = other.buffId;
            this.isAvailable = other.isAvailable;
            this.buffName = other.buffName;
            this.isActive = other.isActive;
        }
    }
}