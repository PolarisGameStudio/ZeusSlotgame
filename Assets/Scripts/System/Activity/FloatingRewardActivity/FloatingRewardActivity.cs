using System.Collections.Generic;
using System;
using Classic;
using Libs;
using UnityEngine;
using Utils;

namespace Activity
{
    public class FloatingRewardActivity : BaseActivity
    {
        //显示间隔时间（秒）
        public float ShowInterval { get; private set; } = 10f;
        
        //水平移动速率
        public float SpeedX { get; private set; } = 100f;
        
        //垂直移动速率
        public float SpeedY { get; private set; } = 100f;
        
        //奖励范围
        public int MinReward { get; private set; } = 100;
        public int MaxReward { get; private set; } = 500;
        
        //Spin次数限制
        public int SpinLimit { get; private set; } = 0;
        
        //当前累计的spin次数
        private int curSpin = 0;
        
        //活动是否已解锁激活
        public bool IsActivated { get; private set; } = false;

		// 活动激活事件，供外部订阅
		public event Action OnActivated;

        public FloatingRewardActivity(Dictionary<string, object> data) : base(data)
        {
            ParseConfig(data);
            curSpin= (int)UserManager.GetInstance().UserProfile().GetTotalSpinCounter();
        }

        private void ParseConfig(Dictionary<string, object> data)
        {
            ShowInterval = Utilities.GetFloat(data, "ShowInterval", 10f);
            SpeedX = Utilities.GetFloat(data, "SpeedX", 100f);
            SpeedY = Utilities.GetFloat(data, "SpeedY", 100f);
            SpinLimit = Utilities.GetInt(data, "SpinLimit", 0);
            MinReward = Utilities.GetInt(data, "Min", 100)*OnLineEarningMgr.Instance.GetCashMultiple();
            MaxReward = Utilities.GetInt(data, "Max", 500)*OnLineEarningMgr.Instance.GetCashMultiple();
            
            Debug.Log($"[FloatingRewardActivity] Config loaded - ShowInterval: {ShowInterval}, SpeedX: {SpeedX}, SpeedY: {SpeedY}, SpinLimit: {SpinLimit}, MinReward: {MinReward}, MaxReward: {MaxReward}");
        }

        /// <summary>
        /// 获取随机奖励值
        /// </summary>
        public int GetRandomReward()
        {
            // 如果启用了IntervalDataPattern模式，使用对应的倍率计算奖励
            if (OnLineEarningMgr.Instance.isIntervalDataPatternOpen())
            {
                var intervalPattern = OnLineEarningMgr.Instance.GetIntervalDataPattern();
                if (intervalPattern != null)
                {
                    return intervalPattern.GetRewardsByName(OnLineEarningConstants.REWARD_FloatingReward);
                }
            }

            // 否则使用配置的随机范围
			return UnityEngine.Random.Range(MinReward, MaxReward + 1);
        }

        public override BaseIcon RegisterIcon(GameObject go)
        {
            icon = go.AddComponent<FloatingRewardIcon>();
            icon.OnInit(id, iconData);
            if (icon is FloatingRewardIcon floatingIcon)
            {
                floatingIcon.SetActivity(this);
            }
            return icon;
        }

        public override void OnInit()
        {
            base.OnInit();
            // 检查是否已经达到SpinLimit
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
            if (IsActivated)
            {
                // 已经激活，不需要再计数
                return;
            }
            
            curSpin++;
            Debug.Log($"[FloatingRewardActivity] Spin count updated: {curSpin}/{SpinLimit}");
            
            // 检查是否达到SpinLimit
            if (curSpin >= SpinLimit)
            {
				Activate();
            }
        }
        
        /// <summary>
        /// 检查活动是否应该激活
        /// </summary>
		private void CheckActivation()
        {
            if (SpinLimit <= 0)
            {
                // SpinLimit为0或负数，表示不需要限制，直接激活
				Activate();
            }
            else if (curSpin >= SpinLimit)
            {
                // 已经达到SpinLimit，直接激活
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
			Debug.Log($"[FloatingRewardActivity] Activity activated (curSpin: {curSpin}, SpinLimit: {SpinLimit})");
			try
			{
				OnActivated?.Invoke();
			}
			catch (Exception e)
			{
				Debug.LogError($"[FloatingRewardActivity] OnActivated invoke error: {e}");
			}
		}
    }
}

