using System.Collections.Generic;
using CardSystem;
using Libs;
using UnityEngine;
using Utils;

namespace System
{
    public enum RedeemItemState
    {
        InTaskProgress1 = 0,
        InTaskProgress2,
        Complete,
        Failed
    }
    
    public class RedeemItemData
    {
        public RedeemItemState state;
        public RedeemItem itemUI;
        private Dictionary<string, object> data = new Dictionary<string, object>();
        public AccumulateTotalCashTask CashTask;
        public SequentialTask SequentialTask;
        public SequentialTask SequentialChildTask;

        //当前任务
        public BaseTask CurTask;
        public BaseTask ChildTask1;
        public int index;
        private GameObject prefab;
        //平台序号
        public int platSpIndex = 0;
        public int RewardCash =0;
        public RedeemItemData(Dictionary<string,object> config)
        {
            data = config;
            index = Utilities.GetInt(config, "index", -1);
            RewardCash = Utilities.GetInt(config, "rewardCash", 0);
            List<object> taskConfigList = Utilities.GetValue<List<object>>(config, "taskConfig", null);
            if (taskConfigList ==null || taskConfigList.Count == 0)
            {
                Debug.LogError("[RedeemItemData] taskInfos is null");
                return;
            }

            for (int i = 0; i < taskConfigList.Count; i++)
            {
                Dictionary<string,object> taskInfos = taskConfigList[i] as Dictionary<string,object>;
                int taskId = Utilities.GetInt(taskInfos, TaskConstants.TaskId_Key, -1);
                BaseTask task = TaskManager.Instance.RegisterTask(taskId,taskInfos);
                if (task.TaskType == TaskConstants.AccumulateCashTask_Key)
                {
                    CashTask = task as AccumulateTotalCashTask;
                }else if (task.TaskType == TaskConstants.SequentialTask_Key)
                {
                    SequentialTask = task as SequentialTask;
                }
            }
            // SequentialTask.OnChildTaskCompleted += OnSequentialTaskChildTaskCompleted;
            SequentialTask.OnProgressUpdated += OnSequentialTaskProgressUpdated;
            SequentialTask.OnTaskCompleted += OnSequentialTaskCompleted;
            SequentialTask.OnSwitchChildTask+= OnSwitchChildTask;
            UpdateState();
        }
        
        public void OnInit(RedeemItem item)
        {
            BindUI(item);
            UpdateState();
        }
        
        public void BindUI(RedeemItem item)
        {
            itemUI = item;
        }
        
        public void UnBindUI()
        {
            itemUI = null;
        }

        public void SetPlatSprite(int index)
        {
            platSpIndex = index;
        }

        public void UpdateState()
        {
            if (CashTask.State == (int)TaskState.ONGOING)
            {
                CurTask = CashTask;
                state = RedeemItemState.InTaskProgress1;
            }else if (SequentialTask.State == (int)TaskState.ONGOING)
            {
                CurTask = SequentialTask;
                state = RedeemItemState.InTaskProgress2;
                BindSequentialTaskEvents();
            }
            else
            {
                state = RedeemItemState.Complete;
            }
        }

        //从收集现金切换至下一个任务
        public void SwitchToNextTask()
        {
            if (state == RedeemItemState.InTaskProgress1)
            {
                //切换到第二个任务
                CashTask.CompleteTask();
                SequentialTask.ActiveChildTask();
                CurTask = SequentialTask;
                state = RedeemItemState.InTaskProgress2;
                BindSequentialTaskEvents();
            }
        }

        private void BindSequentialTaskEvents()
        {
            SequentialChildTask = SequentialTask.GetOnGoingChildTask() as SequentialTask;
            UnBindSequentialTaskEvents();
            SequentialChildTask.OnSwitchChildTask += OnSwitchChildTask;
            SequentialChildTask.OnChildTaskCompleted += OnSequentialTaskChildTaskCompleted;
        }
        private void UnBindSequentialTaskEvents()
        {
            if (SequentialChildTask!=null)
            {
                SequentialChildTask.OnSwitchChildTask -= OnSwitchChildTask;
                SequentialChildTask.OnChildTaskCompleted -= OnSequentialTaskChildTaskCompleted;
            }
        }
        
        private void OnSequentialTaskProgressUpdated(BaseTask task, int progress)
        {
            if (itemUI!=null)
            {
                itemUI.SetSequentialChildTaskUI();
            }
        }

        private void OnSequentialTaskChildTaskCompleted(BaseTask childTask)
        {
            Messenger.Broadcast(GameDialogManager.OpenWithDrawTaskCompletePanelMsg,childTask);
        }
        
        private void OnSwitchChildTask(BaseTask task, int childIndex)
        {
            //当天任务已完成，已切换至下一天任务
            if (task.TaskId == SequentialTask.TaskId)
            {
                UnBindSequentialTaskEvents();
                SequentialChildTask = SequentialTask.GetOnGoingChildTask() as SequentialTask;
                BindSequentialTaskEvents();
                if (itemUI!=null)
                {
                    itemUI.RefreshUI();
                }
                
            }else if (task.TaskId == SequentialChildTask.TaskId)
            {
                //已切换至当天的下一个子任务
                if (itemUI!=null)
                {
                    itemUI.SetSequentialChildTaskUI();
                }
            }
        }
        
        private void OnSequentialTaskCompleted(BaseTask task)
        {
            WithDrawFailed();
        }
        
        public void WithDrawFailed()
        {
            //提现状态失败，转换为集卡任务
            RecordItemData recordItemData = ToRecordItemData();
            WithDrawManager.Instance.RemoveRedeemItem(this);
            WithDrawManager.Instance.AddRecordItemData(recordItemData);
            Messenger.Broadcast(WithDrawConstants.UpdateRedeemItemMsg);
        }

        public bool IsFished()
        {
            return state == RedeemItemState.Complete || state == RedeemItemState.Failed;
        }

        public RecordItemData ToRecordItemData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["index"] =  index;
            data["platSpIndex"] = platSpIndex;
            data["cash"] = RewardCash;
            RecordItemData itemData = new RecordItemData(data);
            return itemData;
        }
    }
}