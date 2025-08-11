using System.Collections.Generic;
using Libs;

namespace System
{
    public class WithDrawSystemProgressData: ProgressDataBase<WithDrawSystemProgressData>
    {
        public string fileName = "WithDrawSystemProgressData";
        private List<BaseTask> _tasks = new List<BaseTask>();
        public int FreeSymbolNum = 0;
        public override void LoadData(WithDrawSystemProgressData progressData)
        {
            FreeSymbolNum = progressData.FreeSymbolNum;
        }

        public override void SaveData()
        {
            FreeSymbolNum = WithDrawManager.Instance.FreeSymbolNum;
            StoreManager.Instance.SaveDataJson(fileName,this);
        }

        public override void ClearData()
        {
            FreeSymbolNum = 0;
            StoreManager.Instance.DeleteProgress(fileName);
        }
    }
}