using System.Collections.Generic;
using Activity;
using Libs;
using UnityEngine;

namespace CardSystem.Activity
{
    public class CardLotteryActivity: BaseActivity
    {
        public CardLotteryActivity(Dictionary<string, object> data) : base(data)
        {
            ParseTaskData();
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
            icon = go.AddComponent<CardLotteryIcon>();
            icon.OnInit(id, iconData);
            // CheckShowIcon();
            return icon;
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
        
        // private void UpdateProgress()
        // {
        //     // //如果是第一次展示卡牌系统，则不累积任务进度
        //     // if (CardSystemManager.Instance.isFirstShow)
        //     // {
        //     //     Task.HasCollectNum = 0;
        //     //     return;
        //     // }
        //     // if (Task.IsTaskConditionOK)
        //     // {
        //     //     Task.HasCollectNum = 0;
        //     //     Task.IsTaskConditionOK = false;
        //     //     //任务进度完成了
        //     //     CardSystemManager.Instance.ShowLotteryDialog();
        //     // }
        //     // if (icon!= null && icon is CardLotteryIcon cardLotteryIcon)
        //     // {
        //     //     //显示图标
        //     //     cardLotteryIcon.RefreshProgress(GetProgress());
        //     // }
        // }

        public float GetProgress()
        {
            if (Task!=null)
            {
                float f = Task.HasCollectNum* 1.0f / Task.TargetNum ;
                return f;
            }

            return 0;
        }
        
        // public void ShowHand()
        // {
        //     // if (icon is CardLotteryIcon)
        //     // {
        //     //     CardLotteryIcon lotteryIcon = icon as CardLotteryIcon;
        //     //     // 这里可以添加特定的逻辑，比如显示小手
        //     //     lotteryIcon.ShowHand();
        //     // }
        //     //展示lottery面板
        //     CardSystemManager.Instance.ShowLotteryDialog();
        // }
        
        
        public override void OnClickIcon()
        {
            base.OnClickIcon();
            // 这里可以添加特定于CardLotteryActivity的点击逻辑
            CardSystemManager.Instance.ShowLotteryDialog();
        }
    }
}