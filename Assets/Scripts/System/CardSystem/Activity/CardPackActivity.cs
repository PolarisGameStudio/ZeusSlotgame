using System.Collections.Generic;
using Activity;
using Libs;
using UnityEngine;

namespace CardSystem.Activity
{
    public class CardPackActivity:BaseActivity
    {
        public CardPackActivity(Dictionary<string, object> data) : base(data)
        {
            // ParseTaskData();
        }
        
        protected override void ParseTaskData()
        {
            Dictionary<string, object> taskData = Utils.Utilities.GetValue<Dictionary<string,object>>(Data, ActivityConstants.TASKS, null);
            if (taskData==null || taskData.Count==0)
            {
                return;
            }
            int taskId =  Utils.Utilities.GetInt(taskData, TaskConstants.TaskId_Key, 0);
            Task = TaskManager.Instance.RegisterTask(taskId,taskData);
        }
        //广播刷新icon
        public override BaseIcon RegisterIcon(GameObject go)
        {
            icon = go.AddComponent<CardPackIcon>();
            icon.OnInit(id, iconData);
            // CheckShowIcon();
            return icon;
        }
        
        public override void AddListener()
        {
            base.AddListener();
            // Messenger.AddListener(Task.UpdateTaskDataMsg,UpdateProgress);
        }
        
        public override void RemoveListener()
        {
            base.RemoveListener();
            // Messenger.RemoveListener(Task.UpdateTaskDataMsg,UpdateProgress);
        }
        
        private void UpdateProgress()
        {
            if (Task.IsTaskConditionOK)
            {
                if (icon!= null)
                {
                    //显示图标
                    CheckShowIcon();
                }
            }
        }
       
        public void CheckShowIcon()
        {
            if (!CardSystemManager.Instance.isFirstShow)
            {
                icon.gameObject.SetActive(true);
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }
        
        public override void OnClickIcon()
        {
            base.OnClickIcon();
            // 这里可以添加特定于CardPackActivity的点击逻辑
            CardSystemManager.Instance.ShowCollectionDialog();
        }
    }
}