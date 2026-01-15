using Libs;

namespace System.SliderMultiplier
{
    /// <summary>
    /// 滑块倍率系统数据持久化类
    /// 保存配置索引、提现状态和重置时间
    /// </summary>
    [Serializable]
    public class SliderMultiplierProgressData : ProgressDataBase<SliderMultiplierProgressData>
    {
        public string fileName = "SliderMultiplierProgressData";

        /// <summary>
        /// 提现前配置索引
        /// </summary>
        public int PreWithdrawConfigIndex = 0;

        /// <summary>
        /// 提现后配置索引
        /// </summary>
        public int PostWithdrawConfigIndex = 0;

        /// <summary>
        /// 是否已提现（从WithDrawSystemProgressData读取）
        /// </summary>
        public bool HasWithdrawn = false;

        /// <summary>
        /// 上次重置日期（yyyy-MM-dd格式）
        /// </summary>
        public string LastResetDate = "";

        public override void LoadData(SliderMultiplierProgressData progressData)
        {
            if (progressData == null)
            {
                UnityEngine.Debug.LogWarning("[SliderMultiplierProgressData] LoadData: progressData is null");
                return;
            }

            PreWithdrawConfigIndex = progressData.PreWithdrawConfigIndex;
            PostWithdrawConfigIndex = progressData.PostWithdrawConfigIndex;
            HasWithdrawn = progressData.HasWithdrawn;
            LastResetDate = progressData.LastResetDate;

            UnityEngine.Debug.Log($"[SliderMultiplierProgressData] LoadData: PreIndex={PreWithdrawConfigIndex}, PostIndex={PostWithdrawConfigIndex}, HasWithdrawn={HasWithdrawn}, LastResetDate={LastResetDate}");
        }

        public override void SaveData()
        {
            StoreManager.Instance.SaveDataJson(fileName, this);
            UnityEngine.Debug.Log($"[SliderMultiplierProgressData] SaveData: PreIndex={PreWithdrawConfigIndex}, PostIndex={PostWithdrawConfigIndex}, HasWithdrawn={HasWithdrawn}, LastResetDate={LastResetDate}");
        }

        public override void ClearData()
        {
            PreWithdrawConfigIndex = 0;
            PostWithdrawConfigIndex = 0;
            HasWithdrawn = false;
            LastResetDate = "";
            StoreManager.Instance.DeleteProgress(fileName);
            UnityEngine.Debug.Log("[SliderMultiplierProgressData] ClearData: All data cleared");
        }
    }
}
