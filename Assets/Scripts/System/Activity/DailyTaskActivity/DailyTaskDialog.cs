using Activity;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
namespace System.Activity.DailyTaskActivity
{
    public class DailyTaskDialog:UIDialog
    {
        public DailyTaskItem mainTaskItem;
        public GameObject taskItemPrefab;

        public TextMeshProUGUI timer;
        
        private DailyTaskActivity _activity;

        private DailyTaskData _mainTaskData;
        public void SetUIData(int activityId)
        {
            _activity = ActivityManager.Instance.GetActivityByID(DailyTaskActivity.ActivityId) as DailyTaskActivity;
            CreateTaskItem();
            ChangeMainTask();
            ChangeTime();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Messenger.AddListener(DailyTaskActivity.ChangeMainTask,ChangeMainTask);
            Messenger.AddListener(DailyTaskActivity.ChangeTime,ChangeTime);
        }
        private void ChangeTime()
        {
            float leftTime = _activity.LeftTime;
            // 把_leftTime转换成00:00:00的形式,要显示倒计时
            int hours = Mathf.FloorToInt(leftTime / 3600);
            int minutes = Mathf.FloorToInt((leftTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(leftTime % 60);
            string timeString = $"{hours:00}:{minutes:00}:{seconds:00}";
            timer.text = timeString;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Messenger.RemoveListener(DailyTaskActivity.ChangeMainTask,ChangeMainTask);
            Messenger.RemoveListener(DailyTaskActivity.ChangeTime,ChangeTime);
        }
        private void ChangeMainTask()
        {
            if (ActivityManager.Instance.GetActivityByID(WithDrawTaskActivity.ActivityId) is WithDrawTaskActivity activity)
            {
                //每次需要重新切换任务
                _mainTaskData.Task = activity.Task;
                mainTaskItem.SetData(_mainTaskData);
            }
        }

        private void CreateTaskItem()
        {

            var list = _activity.GetSortList();
            
            foreach (DailyTaskData baseTask in list)
            {
                if (baseTask.IsMainTask)
                {
                    //主线已经单独实例化,不需要再创建
                    _mainTaskData = baseTask;
                    continue;
                }
                var obj = Instantiate(taskItemPrefab,taskItemPrefab.transform.parent);
                obj.SetActive(true);
                var script = obj.GetComponent<DailyTaskItem>();
                script.SetData(baseTask);
            }
        }
        
        
    }
}