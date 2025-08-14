using System.Collections.Generic;

namespace Libs
{
    public class TaskTipProgress:ProgressDataBase<TaskTipProgress>
    {
        public int spinCount = 0; // Spin count for task tips
        public Dictionary<int,bool> taskTipStatus = new Dictionary<int, bool>(); // Task tip status, key is task type, value is whether to show the tip
        public string fileName = "TaskTipProgressData";        
        public override void LoadData(TaskTipProgress progressData)
        {
            taskTipStatus = progressData.taskTipStatus;
            spinCount = progressData.spinCount;
        }

        public override void SaveData()
        {
            taskTipStatus = TaskTipManager.Instance.TaskTipStatus;
            spinCount = TaskTipManager.Instance.CurrentSpinCount;
            StoreManager.Instance.SaveDataJson(fileName, this);
        }

        public override void ClearData()
        {
            spinCount = 0;
            taskTipStatus.Clear();
            StoreManager.Instance.DeleteProgress(fileName);
        }
    }
}