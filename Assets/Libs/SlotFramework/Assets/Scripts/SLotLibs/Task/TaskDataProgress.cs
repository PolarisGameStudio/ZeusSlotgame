using System.Collections.Generic;

namespace Libs
{
    public class TaskDataProgress:ProgressDataBase<TaskDataProgress>
    {
        public string fileName = "TaskDataProgress";
        public Dictionary<int, BaseTask> taskDict = new Dictionary<int, BaseTask>();
        public override void LoadData(TaskDataProgress progressData)
        {
            taskDict = progressData.taskDict;
        }

        public override void SaveData()
        {
            taskDict = TaskManager.Instance.taskDict;
            StoreManager.Instance.SaveDataJson(fileName,this);
        }

        public override void ClearData()
        {
            StoreManager.Instance.DeleteProgress(fileName);
        }
    }
}