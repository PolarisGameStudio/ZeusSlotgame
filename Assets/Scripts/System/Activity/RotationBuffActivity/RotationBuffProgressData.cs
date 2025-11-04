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
        //当前累计的spin次数
        public int curSpin = 0;
        public override void LoadData(RotationBuffProgressData progressData)
        {
            currentIndex = progressData.currentIndex;
            activeCount= progressData.activeCount;
            curSpin = progressData.curSpin;
        }

        public override void SaveData()
        {
            
        }

        public void SaveData(int index, BaseBuff buff,int activityC, int curSpinCount)
        {
            currentIndex = index;
            activeCount = activityC;
            curSpin = curSpinCount;
            StoreManager.Instance.SaveDataJson(fileName, this);
        }
        
        public override void ClearData()
        {
            currentIndex = 0;
            activeCount = 0;
            curSpin = 0;
        }
    }
}