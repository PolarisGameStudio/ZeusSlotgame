using System.Collections.Generic;

namespace Libs
{
    public class TaskTipProgress:ProgressDataBase<TaskTipProgress>
    {
        public int spinCount = 0; // Spin count for task tips
        public Dictionary<int,bool> taskTipStatus = new Dictionary<int, bool>(); // Task tip status, key is task type, value is whether to show the tip
        public string fileName = "TaskTipProgressData";
        public int FreeSymbolNum = 0;
        public int S01SymbolNum = 0;
        public override void LoadData(TaskTipProgress progressData)
        {
            taskTipStatus = progressData.taskTipStatus;
            spinCount = progressData.spinCount;
            FreeSymbolNum = progressData.FreeSymbolNum;
            S01SymbolNum = progressData.S01SymbolNum;
        }

        public override void SaveData()
        {
            taskTipStatus = TaskTipManager.Instance.TaskTipStatus;
            spinCount = TaskTipManager.Instance.CurrentSpinCount;
            FreeSymbolNum = TaskTipManager.Instance.FreeSymbolNum;
            S01SymbolNum = TaskTipManager.Instance.S01SymbolNum;
            StoreManager.Instance.SaveDataJson(fileName, this);
        }

        public override void ClearData()
        {
            spinCount = 0;
            taskTipStatus.Clear();
            FreeSymbolNum = 0;
            S01SymbolNum = 0;
            StoreManager.Instance.DeleteProgress(fileName);
        }
    }
}