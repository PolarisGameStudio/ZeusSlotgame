using System.Collections.Generic;
using Libs;

namespace System.BuffSystem
{
    public class BuffProgressData:ProgressDataBase<BuffProgressData>
    {
        public string fileName = "BuffSystem";
        public Dictionary<int,BaseBuff> BaseBuffs = new Dictionary<int,BaseBuff>();
        public override void LoadData(BuffProgressData progressData)
        {
            BaseBuffs = progressData.BaseBuffs;
        }

        public override void SaveData()
        {
            BaseBuffs = BuffManager.Instance._allBuffDic;
            StoreManager.Instance.SaveDataJson(fileName,this);
        }

        public override void ClearData()
        {
            
        }
    }
}