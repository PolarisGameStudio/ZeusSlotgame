using System.Collections.Generic;
using System;
using Classic;
using Libs;
using UnityEngine;
using Utils;

namespace Activity
{
    public class LuckyGiftActivity : BaseActivity
    {
        public int UnlockSpinLimit { get; private set; } = 0;
        public int TriggerSpinLimit { get; private set; } = 5;
        public int MinReward { get; private set; } = 100;
        public int MaxReward { get; private set; } = 500;
        public float ItemScaleDuration { get; private set; } = 0.5f;
        public float ItemStayDuration { get; private set; } = 1f;

        // 当前累计的spin次数
        private int curSpin;
        // 与触发条件相关的spin累计
        private int lastTriggerSpin;

        // 活动是否已经激活
        public bool IsActivated { get; private set; }

        public event Action OnActivated;

        public LuckyGiftActivity(Dictionary<string, object> data) : base(data)
        {
            ParseConfig(data);
            curSpin = (int)UserManager.GetInstance().UserProfile().GetTotalSpinCounter();
        }

        private void ParseConfig(Dictionary<string, object> data)
        {
            UnlockSpinLimit = Utilities.GetInt(data, "UnlockSpinLimit", 0);
            TriggerSpinLimit = Utilities.GetInt(data, "TriggerSpinLimit", 5);
            MinReward = Utilities.GetInt(data, "Min", 100) * OnLineEarningMgr.Instance.GetCashMultiple();
            MaxReward = Utilities.GetInt(data, "Max", 500) * OnLineEarningMgr.Instance.GetCashMultiple();
            
            Debug.Log($"[LuckyGiftActivityAdNode] Config loaded - UnlockSpinLimit: {UnlockSpinLimit}, TriggerSpinLimit: {TriggerSpinLimit}, MinReward: {MinReward}, MaxReward: {MaxReward}");
        }

        /// <summary>
        /// 获取随机奖励值
        /// </summary>
        public int GetRandomReward()
        {
            return UnityEngine.Random.Range(MinReward, MaxReward + 1);
        }

        public override BaseIcon RegisterIcon(GameObject go)
        {
            icon = go.AddComponent<LuckyGiftActivityIcon>();
            icon.OnInit(id, iconData);
            if (icon is LuckyGiftActivityIcon luckyCashIcon)
            {
                luckyCashIcon.SetActivity(this);
            }
            return icon;
        }

        public override void OnInit()
        {
            base.OnInit();
            CheckActivation();
        }

        public override void OnUpdate()
        {
            // 更新逻辑由icon自己管理
        }
        
        public override void AddListener()
        {
            base.AddListener();
            // 监听spin结束事件
            Messenger.AddListener(SlotControllerConstants.OnSpinEnd, UpdateSpinCount);
        }
        
        public override void RemoveListener()
        {
            base.RemoveListener();
            // 移除spin结束事件监听
            Messenger.RemoveListener(SlotControllerConstants.OnSpinEnd, UpdateSpinCount);
        }
        
        /// <summary>
        /// 更新spin计数
        /// </summary>
        private void UpdateSpinCount()
        {
            curSpin++;
            Debug.Log($"[LuckyGiftActivityAdNode] Spin count updated: {curSpin}/{UnlockSpinLimit}");

            if (!IsActivated && curSpin >= UnlockSpinLimit)
            {
                Activate();
            }
        }
        
        /// <summary>
        /// 检查活动是否应该激活
        /// </summary>
        private void CheckActivation()
        {
            if (IsActivated)
            {
                return;
            }

            if (UnlockSpinLimit <= 0 || curSpin >= UnlockSpinLimit)
            {
                Activate();
            }
        }

        private void Activate()
        {
            if (IsActivated)
            {
                return;
            }
            IsActivated = true;
            Debug.Log($"[LuckyGiftActivityAdNode] Activity activated (curSpin: {curSpin}, UnlockSpinLimit: {UnlockSpinLimit})");
            try
            {
                OnActivated?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[LuckyGiftActivityAdNode] OnActivated invoke error: {e}");
            }
        }
        
        /// <summary>
        /// 检查是否满足触发条件（spin次数间隔）
        /// </summary>
        public bool CheckTriggerSpinLimit()
        {
            return ++lastTriggerSpin >= TriggerSpinLimit;
        }
        
        public void ResetTriggerSpinLimit()
        {
            lastTriggerSpin = 0;
        }
        
        /// <summary>
        /// 获取当前spin次数
        /// </summary>
        public int GetCurrentSpin()
        {
            return curSpin;
        }
    }
}

