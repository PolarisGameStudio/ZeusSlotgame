using System.Collections.Generic;
using Libs;

namespace System
{
    public class WithDrawSystemProgressData: ProgressDataBase<WithDrawSystemProgressData>
    {
        public string fileName = "WithDrawSystemProgressData";
        private List<BaseTask> _tasks = new List<BaseTask>();
        public int FreeSymbolNum = 0;
        public int S01SymbolNum = 0;
        public bool isFirstWithDraw = false;
        public bool hasShownWithDrawPrompt = false;
        public bool showRecordRedPoint = false; // 是否显示record红点

        public override void LoadData(WithDrawSystemProgressData progressData)
        {
            FreeSymbolNum = progressData.FreeSymbolNum;
            S01SymbolNum = progressData.S01SymbolNum;
            isFirstWithDraw= progressData.isFirstWithDraw;
            hasShownWithDrawPrompt = progressData.hasShownWithDrawPrompt;
            showRecordRedPoint = progressData.showRecordRedPoint;
        }

        public override void SaveData()
        {
            isFirstWithDraw= WithDrawManager.Instance.isFirstWithDraw;
            hasShownWithDrawPrompt = WithDrawManager.Instance.hasShownWithDrawPrompt;
            showRecordRedPoint = WithDrawManager.Instance.showRecordRedPoint;
            StoreManager.Instance.SaveDataJson(fileName,this);
        }

        public override void ClearData()
        {
            FreeSymbolNum = 0;
            S01SymbolNum = 0;
            StoreManager.Instance.DeleteProgress(fileName);
        }
    }
}