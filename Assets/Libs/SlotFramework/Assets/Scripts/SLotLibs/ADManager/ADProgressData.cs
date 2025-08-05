
using System.Collections.Generic;
using Libs;

namespace Ads
{
    /// <summary>
    /// 广告本地保存数据
    /// </summary>
    public class ADProgressData : ProgressDataBase<ADProgressData>
    {
        public int RewardCount = 0; //奖励广告次数
        public int InterstitialCount = 0; //插屏广告次数
        public string fileName = "ADProgressData";
        public Dictionary<string, ADCondition> ADConditions = new Dictionary<string, ADCondition>(); //广告节点

        public override void LoadData(ADProgressData progressData)
        {
            RewardCount = progressData.RewardCount;
            InterstitialCount = progressData.InterstitialCount;
            ADConditions = progressData.ADConditions;
        }

        public override void SaveData()
        {
            RewardCount = ADManager.Instance.RewardCount;
            InterstitialCount = ADManager.Instance.InterstitialCount;
            ADConditions = ADManager.Instance.ADConditions;
            StoreManager.Instance.SaveDataJson(fileName, this);
        }

        public override void ClearData()
        {
            RewardCount = 0;
            InterstitialCount = 0;
            ADConditions.Clear();
        }
    }
}