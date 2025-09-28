using System.BuffSystem;
using Libs;

namespace Activity
{
    public class RotationBuffProgressData:ProgressDataBase<RotationBuffProgressData>
    {
        public string fileName = "RotationBuff";
        public int currentIndex = 0;
        //buff激活的次数
        public int activeCount = 0;
        public override void LoadData(RotationBuffProgressData progressData)
        {
            currentIndex = progressData.currentIndex;
            activeCount= progressData.activeCount;
        }

        public override void SaveData()
        {
            
        }

        public void SaveData(int index, BaseBuff buff,int activityC)
        {
            currentIndex = index;
            activeCount = activityC;
            StoreManager.Instance.SaveDataJson(fileName, this);
        }
        
        public override void ClearData()
        {
            currentIndex = 0;
            activeCount = 0;
        }
    }
}