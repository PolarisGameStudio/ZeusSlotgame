using Libs;

namespace Core
{
    public class OnLineEarningProgressData:ProgressDataBase<OnLineEarningProgressData>
    {
        public string fileName = "OnLineEarning";
        public int curRewardCount = 0;
        public int cash = 0;
        public override void LoadData(OnLineEarningProgressData progressData)
        {
            curRewardCount = progressData.curRewardCount;
            cash = progressData.cash;
        }

        public override void SaveData()
        {
            curRewardCount = OnLineEarningMgr.Instance.curRewardCount;
            if (!OnLineEarningMgr.UnLimitMoney)
            {
                cash = OnLineEarningMgr.Instance.Cash();
            }
            StoreManager.Instance.SaveDataJson(fileName,this);
        }

        public override void ClearData()
        {
            StoreManager.Instance.DeleteProgress(fileName);
        }
    }
}