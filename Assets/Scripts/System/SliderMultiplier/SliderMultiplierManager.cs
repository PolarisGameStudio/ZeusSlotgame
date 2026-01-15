using System.Collections.Generic;
using Libs;
using UnityEngine;
using Utils;

namespace System.SliderMultiplier
{
    /// <summary>
    /// 滑块倍率系统管理器（单例）
    /// 负责管理配置、索引推进、提现状态检测和数据持久化
    /// </summary>
    public class SliderMultiplierManager
    {
        private const string ConfigKey = "SliderMultiplierConfig";
        private const string PreWithdrawConfigKey = "PreWithdrawConfig";
        private const string PostWithdrawConfigKey = "PostWithdrawConfig";
        private const string SliderSpeedKey = "SliderSpeed";

        // 单例实例
        private static SliderMultiplierManager _instance;
        public static SliderMultiplierManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SliderMultiplierManager();
                }
                return _instance;
            }
        }

        // 当前使用的配置类型（提现前/提现后）
        private bool isPostWithdraw;

        // 当前配置索引（配置1的索引）
        private int preWithdrawConfigIndex;

        // 当前配置索引（配置2的索引）
        private int postWithdrawConfigIndex;

        // 配置数据
        private List<int[]> preWithdrawConfigs;
        private List<int[]> postWithdrawConfigs;

        // 滑块移动速度
        private float sliderSpeed = 450f;

        // 上次重置时间（用于每日24:00重置判断）
        private DateTime lastResetDate;

        // 持久化数据
        private SliderMultiplierProgressData progressData;

        // 是否已初始化
        private bool isInitialized = false;

        private SliderMultiplierManager()
        {
            progressData = new SliderMultiplierProgressData();
        }

        /// <summary>
        /// 初始化管理器
        /// </summary>
        public void OnInit()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[SliderMultiplierManager] Already initialized");
                return;
            }

            Debug.Log("[SliderMultiplierManager] OnInit start");

            // 1. 加载配置
            ParseConfig();

            // 2. 加载持久化数据
            LoadProgressData();

            // 3. 检查提现状态
            CheckWithdrawStatus();

            // 4. 检查每日重置
            CheckDailyReset();

            // 5. 添加监听器
            AddListeners();

            isInitialized = true;
            Debug.Log("[SliderMultiplierManager] OnInit complete");
        }

        /// <summary>
        /// 清理（游戏退出时调用）
        /// </summary>
        public void OnDestroy()
        {
            RemoveListeners();
            SaveProgressData();
        }

        #region 配置解析

        /// <summary>
        /// 解析配置文件
        /// </summary>
        private void ParseConfig()
        {
            Dictionary<string, object> config = Plugins.Configuration.GetInstance().GetValue<Dictionary<string, object>>(ConfigKey, null);
            if (config == null || config.Count == 0)
            {
                Debug.LogError("[SliderMultiplierManager] ParseConfig: config is null or empty");
                return;
            }

            // 解析提现前配置
            List<object> preConfigList = Utilities.GetValue<List<object>>(config, PreWithdrawConfigKey, null);
            if (preConfigList == null || preConfigList.Count == 0)
            {
                Debug.LogError("[SliderMultiplierManager] ParseConfig: PreWithdrawConfig is null or empty");
                return;
            }
            preWithdrawConfigs = ParseMultiplierConfigList(preConfigList);
            Debug.Log($"[SliderMultiplierManager] ParseConfig: Loaded {preWithdrawConfigs.Count} PreWithdrawConfigs");

            // 解析提现后配置
            List<object> postConfigList = Utilities.GetValue<List<object>>(config, PostWithdrawConfigKey, null);
            if (postConfigList == null || postConfigList.Count == 0)
            {
                Debug.LogError("[SliderMultiplierManager] ParseConfig: PostWithdrawConfig is null or empty");
                return;
            }
            postWithdrawConfigs = ParseMultiplierConfigList(postConfigList);
            Debug.Log($"[SliderMultiplierManager] ParseConfig: Loaded {postWithdrawConfigs.Count} PostWithdrawConfigs");

            // 解析滑块速度
            sliderSpeed = Utilities.GetFloat(config, SliderSpeedKey, 450f);
            Debug.Log($"[SliderMultiplierManager] ParseConfig: SliderSpeed={sliderSpeed}");
        }

        /// <summary>
        /// 解析倍率配置列表
        /// 支持字符串格式："1,2,3,2,1" 或数组格式：[1,2,3,2,1]
        /// </summary>
        private List<int[]> ParseMultiplierConfigList(List<object> configList)
        {
            List<int[]> result = new List<int[]>();

            foreach (object configObj in configList)
            {
                int[] multipliers = null;

                // 尝试解析为字符串格式："1,2,3,2,1"
                if (configObj is string configStr)
                {
                    multipliers = ParseMultiplierString(configStr);
                }
                // 尝试解析为数组格式：[1,2,3,2,1]
                else if (configObj is List<object> multiplierList)
                {
                    if (multiplierList.Count != 5)
                    {
                        Debug.LogError($"[SliderMultiplierManager] ParseMultiplierConfigList: Invalid config, expected 5 elements, got {multiplierList.Count}");
                        continue;
                    }

                    multipliers = new int[5];
                    for (int i = 0; i < 5; i++)
                    {
                        multipliers[i] = Convert.ToInt32(multiplierList[i]);
                    }
                }
                else
                {
                    Debug.LogError($"[SliderMultiplierManager] ParseMultiplierConfigList: Unknown config type {configObj.GetType()}");
                    continue;
                }

                if (multipliers != null && multipliers.Length == 5)
                {
                    result.Add(multipliers);
                    Debug.Log($"[SliderMultiplierManager] ParseMultiplierConfigList: [{multipliers[0]}, {multipliers[1]}, {multipliers[2]}, {multipliers[3]}, {multipliers[4]}]");
                }
            }

            return result;
        }

        /// <summary>
        /// 解析字符串格式的倍率配置："1,2,3,2,1"
        /// </summary>
        private int[] ParseMultiplierString(string configStr)
        {
            if (string.IsNullOrEmpty(configStr))
            {
                Debug.LogError("[SliderMultiplierManager] ParseMultiplierString: Config string is null or empty");
                return null;
            }

            string[] parts = configStr.Split(',');
            if (parts.Length != 5)
            {
                Debug.LogError($"[SliderMultiplierManager] ParseMultiplierString: Invalid config '{configStr}', expected 5 elements, got {parts.Length}");
                return null;
            }

            int[] multipliers = new int[5];
            for (int i = 0; i < 5; i++)
            {
                string part = parts[i].Trim();
                if (int.TryParse(part, out int value))
                {
                    multipliers[i] = value;
                }
                else
                {
                    Debug.LogError($"[SliderMultiplierManager] ParseMultiplierString: Failed to parse '{part}' to int in config '{configStr}'");
                    return null;
                }
            }

            return multipliers;
        }

        #endregion

        #region 数据持久化

        /// <summary>
        /// 加载持久化数据
        /// </summary>
        private void LoadProgressData()
        {
            try
            {
                SliderMultiplierProgressData data = StoreManager.Instance.LoadDataJson<SliderMultiplierProgressData>(progressData.fileName);
                if (data != null)
                {
                    progressData.LoadData(data);
                    preWithdrawConfigIndex = progressData.PreWithdrawConfigIndex;
                    postWithdrawConfigIndex = progressData.PostWithdrawConfigIndex;
                    isPostWithdraw = progressData.HasWithdrawn;

                    // 解析上次重置日期
                    if (!string.IsNullOrEmpty(progressData.LastResetDate))
                    {
                        DateTime.TryParse(progressData.LastResetDate, out lastResetDate);
                    }
                    else
                    {
                        lastResetDate = DateTime.Now.Date;
                    }

                    Debug.Log($"[SliderMultiplierManager] LoadProgressData: PreIndex={preWithdrawConfigIndex}, PostIndex={postWithdrawConfigIndex}, IsPostWithdraw={isPostWithdraw}");
                }
                else
                {
                    Debug.Log("[SliderMultiplierManager] LoadProgressData: No saved data, using defaults");
                    lastResetDate = DateTime.Now.Date;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SliderMultiplierManager] LoadProgressData error: {e.Message}");
            }
        }

        /// <summary>
        /// 保存持久化数据
        /// </summary>
        public void SaveProgressData()
        {
            progressData.PreWithdrawConfigIndex = preWithdrawConfigIndex;
            progressData.PostWithdrawConfigIndex = postWithdrawConfigIndex;
            progressData.HasWithdrawn = isPostWithdraw;
            progressData.LastResetDate = lastResetDate.ToString("yyyy-MM-dd");
            progressData.SaveData();
        }

        #endregion

        #region 配置获取

        /// <summary>
        /// 刷新提现状态并获取最新配置（每次使用前调用）
        /// </summary>
        public void RefreshConfig()
        {
            CheckWithdrawStatus();
            Debug.Log($"[SliderMultiplierManager] RefreshConfig: IsPostWithdraw={isPostWithdraw}");
        }

        /// <summary>
        /// 获取当前倍率配置（5个int数组）
        /// </summary>
        public int[] GetCurrentMultiplierConfig()
        {
            if (preWithdrawConfigs == null || preWithdrawConfigs.Count == 0)
            {
                Debug.LogError("[SliderMultiplierManager] GetCurrentMultiplierConfig: preWithdrawConfigs is null or empty");
                return new int[] { 2, 3, 5, 3, 2 }; // 默认配置
            }

            if (postWithdrawConfigs == null || postWithdrawConfigs.Count == 0)
            {
                Debug.LogError("[SliderMultiplierManager] GetCurrentMultiplierConfig: postWithdrawConfigs is null or empty");
                return new int[] { 2, 3, 5, 3, 2 }; // 默认配置
            }

            if (isPostWithdraw)
            {
                // 使用提现后配置
                int index = Mathf.Clamp(postWithdrawConfigIndex, 0, postWithdrawConfigs.Count - 1);
                Debug.Log($"[SliderMultiplierManager] GetCurrentMultiplierConfig: Using PostWithdraw config at index {index}");
                return postWithdrawConfigs[index];
            }
            else
            {
                // 使用提现前配置
                int index = Mathf.Clamp(preWithdrawConfigIndex, 0, preWithdrawConfigs.Count - 1);
                Debug.Log($"[SliderMultiplierManager] GetCurrentMultiplierConfig: Using PreWithdraw config at index {index}");
                return preWithdrawConfigs[index];
            }
        }

        /// <summary>
        /// 获取滑块移动速度
        /// </summary>
        public float GetSliderSpeed()
        {
            return sliderSpeed;
        }

        #endregion

        #region 配置索引管理

        /// <summary>
        /// 领奖时调用，推进到下一个配置
        /// </summary>
        public void AdvanceToNextConfig()
        {
            if (isPostWithdraw)
            {
                // 配置2
                int maxIndex = postWithdrawConfigs.Count - 1;
                if (postWithdrawConfigIndex < maxIndex)
                {
                    postWithdrawConfigIndex++;
                    Debug.Log($"[SliderMultiplierManager] AdvanceToNextConfig: PostWithdraw index advanced to {postWithdrawConfigIndex}");
                }
                else
                {
                    Debug.Log($"[SliderMultiplierManager] AdvanceToNextConfig: PostWithdraw index already at max ({maxIndex})");
                }
            }
            else
            {
                // 配置1
                int maxIndex = preWithdrawConfigs.Count - 1;
                if (preWithdrawConfigIndex < maxIndex)
                {
                    preWithdrawConfigIndex++;
                    Debug.Log($"[SliderMultiplierManager] AdvanceToNextConfig: PreWithdraw index advanced to {preWithdrawConfigIndex}");
                }
                else
                {
                    Debug.Log($"[SliderMultiplierManager] AdvanceToNextConfig: PreWithdraw index already at max ({maxIndex})");
                }
            }

            SaveProgressData();
        }

        #endregion

        #region 提现状态管理

        /// <summary>
        /// 检查并处理提现状态变化
        /// </summary>
        public void CheckWithdrawStatus()
        {
            // 直接从 WithDrawManager 获取实时状态，而不是从磁盘加载
            bool hasCompletedWithdraw = WithDrawManager.Instance.HasCompletedAnyCashTask();

            if (hasCompletedWithdraw)
            {
                // 用户已完成提现
                if (!isPostWithdraw)
                {
                    // 状态变化：从提现前切换到提现后
                    Debug.Log("[SliderMultiplierManager] CheckWithdrawStatus: User completed withdraw, switching to PostWithdraw config");
                    isPostWithdraw = true;
                    ResetPostWithdrawConfig();
                }
            }
            else
            {
                // 用户未完成提现
                if (isPostWithdraw)
                {
                    Debug.LogWarning("[SliderMultiplierManager] CheckWithdrawStatus: Withdraw status inconsistent, keeping PostWithdraw config");
                }
            }
        }

        /// <summary>
        /// 提现后重置配置2的索引
        /// </summary>
        public void ResetPostWithdrawConfig()
        {
            postWithdrawConfigIndex = 0;
            Debug.Log("[SliderMultiplierManager] ResetPostWithdrawConfig: PostWithdraw index reset to 0");
            SaveProgressData();
        }

        #endregion

        #region 每日重置

        /// <summary>
        /// 每日重置检查（在游戏启动或每日登录时调用）
        /// </summary>
        public void CheckDailyReset()
        {
            DateTime today = DateTime.Now.Date;
            string todayString = today.ToString("yyyy-MM-dd");
            string lastResetString = lastResetDate.ToString("yyyy-MM-dd");

            if (todayString != lastResetString)
            {
                // 新的一天，执行重置
                Debug.Log($"[SliderMultiplierManager] CheckDailyReset: New day detected (last: {lastResetString}, today: {todayString}), resetting configs");

                preWithdrawConfigIndex = 0;
                postWithdrawConfigIndex = 0;
                lastResetDate = today;

                SaveProgressData();
            }
            else
            {
                Debug.Log($"[SliderMultiplierManager] CheckDailyReset: Same day ({todayString}), no reset needed");
            }
        }

        #endregion

        #region 监听器

        /// <summary>
        /// 添加监听器
        /// </summary>
        private void AddListeners()
        {
            // 监听提现完成事件
            Messenger.AddListener(WithDrawConstants.OnCashTaskCompleted, OnCashTaskCompleted);
            Debug.Log("[SliderMultiplierManager] AddListeners: Listeners added");
        }

        /// <summary>
        /// 移除监听器
        /// </summary>
        private void RemoveListeners()
        {
            Messenger.RemoveListener(WithDrawConstants.OnCashTaskCompleted, OnCashTaskCompleted);
            Debug.Log("[SliderMultiplierManager] RemoveListeners: Listeners removed");
        }

        /// <summary>
        /// 提现完成回调
        /// </summary>
        private void OnCashTaskCompleted()
        {
            Debug.Log("[SliderMultiplierManager] OnCashTaskCompleted: Cash task completed, checking withdraw status");
            CheckWithdrawStatus();
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取指定索引的倍率值
        /// </summary>
        public int GetMultiplierAtIndex(int index)
        {
            int[] config = GetCurrentMultiplierConfig();
            if (index < 0 || index >= config.Length)
            {
                Debug.LogError($"[SliderMultiplierManager] GetMultiplierAtIndex: Invalid index {index}");
                return 1;
            }
            return config[index];
        }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized()
        {
            return isInitialized;
        }

        #endregion
    }
}
