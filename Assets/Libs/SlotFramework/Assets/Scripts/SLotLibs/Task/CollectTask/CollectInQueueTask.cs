using System.Collections.Generic;
using Ads;
using Plugins;
using UnityEngine;

namespace Libs
{
    /// <summary>
    /// 收集队列任务：根据广告类型增加累计进度
    /// </summary>
    public class CollectInQueueTask : BaseTask
    {
        // 存储不同广告类型的min和max配置
        private Dictionary<int, (int min, int max)> adTypeRangeConfig = new Dictionary<int, (int min, int max)>();

        //每隔多少秒自增
        private int IntervalSeconds = 1;
        private int TimeMin=0;
        private int TimeMax=0;
        public CollectInQueueTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener<string>(ADConstants.OnPlayVideoEnd, UpdateAdCount);

            Dictionary<string,object> extraInfos = Utils.Utilities.GetValue<Dictionary<string,object>>(taskInfoDict, TaskConstants.TaskExtras_Key, null);
            // 从配置中加载min和max值
            LoadConfig(extraInfos);
        }

        ~CollectInQueueTask()
        {
            Messenger.RemoveListener<string>(ADConstants.OnPlayVideoEnd, UpdateAdCount);
        }

        /// <summary>
        /// 从配置中加载CollectInQueneTask的min和max配置
        /// </summary>
        private void LoadConfig(Dictionary<string,object> extraInfos)
        {
            // 读取激励视频广告的min和max
            int rewardADMin = Utils.Utilities.GetInt(extraInfos, "RewardADMin", 1);
            int rewardADMax = Utils.Utilities.GetInt(extraInfos, "RewardADMax", 3);
            adTypeRangeConfig[(int)ADType.RewardAD] = (rewardADMin, rewardADMax);

            // 读取插屏广告的min和max
            int interstitialADMin = Utils.Utilities.GetInt(extraInfos, "InterstitialADMin", 2);
            int interstitialADMax = Utils.Utilities.GetInt(extraInfos, "InterstitialADMax", 5);
            adTypeRangeConfig[(int)ADType.InterstitialAD] = (interstitialADMin, interstitialADMax);

            // 读取AdMob广告的min和max
            int adMobMin = Utils.Utilities.GetInt(extraInfos, "AdMobMin", 1);
            int adMobMax = Utils.Utilities.GetInt(extraInfos, "AdMobMax", 2);
            adTypeRangeConfig[(int)ADType.AdMob] = (adMobMin, adMobMax);
            IntervalSeconds = Utils.Utilities.GetInt(extraInfos, "IntervalSeconds", 1);
            TimeMin = Utils.Utilities.GetInt(extraInfos, "TimeMin", 0);
            TimeMax = Utils.Utilities.GetInt(extraInfos, "TimeMax", 0);
        }

        void UpdateAdCount(string entranceName)
        {
            if (IsConditionOK() || State != (int)TaskState.ONGOING)
            {
                return;
            }

            // 使用ADManager的方法获取广告类型
            int adType = ADManager.Instance.GetAdTypeFromEntrance(entranceName);
            
            // 从配置中获取对应广告类型的min和max，并生成随机数
            if (adTypeRangeConfig.ContainsKey(adType))
            {
                var (min, max) = adTypeRangeConfig[adType];
                AddNumber = Random.Range(min, max + 1); // Random.Range的max是exclusive，所以+1
                MultipleAddNum();
                DoCollectAction();
                UpdateTaskStatus();
            }
        }

        void UpdateCount()
        {
            if (IsConditionOK() || State != (int)TaskState.ONGOING)
            {
                return;
            }

            AddNumber = Random.Range(TimeMin, TimeMax + 1); // Random.Range的max是exclusive，所以+1
            MultipleAddNum();
            DoCollectAction();
            UpdateTaskStatus();
        }
        float totaltime = 0;
        public void OnUpdate()
        {
            //累积时间,当超过IntervalSeconds时，增加一次进度，然后累计进度重置
            totaltime += Time.deltaTime;
            if (IntervalSeconds>0&&totaltime>IntervalSeconds)
            {
                UpdateCount();
                totaltime = 0;
            }
        }
        
        public override string GetDesc()
        {
            return "CollectInQueue";
        }
    }
}

