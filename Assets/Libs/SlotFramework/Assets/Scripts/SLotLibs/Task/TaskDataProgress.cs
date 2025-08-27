using System.Collections.Generic;

namespace Libs
{
    public class TaskDataProgress:ProgressDataBase<TaskDataProgress>
    {
        public string fileName = "TaskDataProgress";
        public List<Dictionary<string,object>> taskDataList = new List<Dictionary<string, object>>();
        public override void LoadData(TaskDataProgress progressData)
        {
            taskDataList = progressData.taskDataList;
        }

        public override void SaveData()
        {
            taskDataList = TaskManager.Instance.ConvertTaskDataToJson();
            StoreManager.Instance.SaveDataJson(fileName,this);
        }
        
        public override void ClearData()
        {
            StoreManager.Instance.DeleteProgress(fileName);
        }
    }
}