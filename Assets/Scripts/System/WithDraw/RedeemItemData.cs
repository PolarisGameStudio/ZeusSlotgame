using System.Collections.Generic;
using CardSystem;
using Libs;
using UnityEngine;
using Utils;

namespace System
{
    public enum RedeemItemState
    {
        InProgress = 0,
        Complete,
        Wait,
        Failed,
        Done
    }
    public class RedeemItemData
    {
        public RedeemItemState state;
        public RedeemItem itemUI;
        private Dictionary<string, object> data = new Dictionary<string, object>();
        public BaseTask task;
        public int index;
        private GameObject prefab;
        //平台序号
        public int platSpIndex = 0;
        public RedeemItemData(Dictionary<string,object> config)
        {
            data = config;
            index = Utilities.GetInt(config, "index", -1);
            Dictionary<string,object> taskInfos = Utilities.GetValue<Dictionary<string,object>>(config, "taskInfo", null);
            if (taskInfos ==null || taskInfos.Count == 0)
            {
                Debug.LogError("[RedeemItemData] taskInfos is null");
                return;
            }
            int taskId = Utilities.GetInt(taskInfos, TaskConstants.TaskId_Key, -1);
            task = TaskManager.Instance.RegisterTask(taskId,taskInfos);
            //根据任务刷新状态
            UpdateState();
            //初始化时根据失败状态执行
            Messenger.AddListener(task.UpdateTaskDataMsg,UpdateTaskData);
        }

        ~RedeemItemData()
        {
            Messenger.RemoveListener(task.UpdateTaskDataMsg,UpdateTaskData);
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
            if (task.GetTaskState()==TaskState.ONGOING)
            {
                //未点击领取按钮回退过
                if (task.CanRewardTime == 0)
                {
                    if (!task.IsTaskConditionOK)
                    {
                        state = RedeemItemState.InProgress;
                    }
                    else
                    {
                        state = RedeemItemState.Complete;
                    }
                }
                else if (task.CanRewardTime > 0)
                {
                    if (WithDrawManager.Instance.NeedLoginDays)
                    {
                        //是否达到要求天数
                        var loginDays = WithDrawManager.Instance.UpDateLoginDays(task.TaskId);
                        var requireDays = WithDrawManager.Instance.GetCoolTime();
                        if (loginDays >= requireDays)
                        {
                            state = RedeemItemState.Failed;
                        }
                        else
                        {
                            state = RedeemItemState.Wait;
                        }
                    }
                    else
                    {
                        //已经点击了领取按钮，任务进度已回退,此时 istaskconditionOK不影响
                        if (task.CanRewardTime>TimeUtils.ConvertDateTimeLong(DateTime.Now))
                        {
                            state = RedeemItemState.Wait;
                        }
                        else
                        {
                            state = RedeemItemState.Failed;
                        }
                    }
                }
            }else if (task.GetTaskState() == TaskState.CLOSE)
            {
                state = RedeemItemState.Done;
            }
        }

        //任务状态已刷新，根据当前的状态进行操作
        private void UpdateTaskData()
        {
            // if(!WithDrawManager.WithDrawUIShow) return;
            Debug.Log($"[RedeemItemData][UpdateTaskData] [task.TaskId ==]:{task.TaskId}   [SelectId==]:{WithDrawManager.Instance.GetSelectId()}");
            bool isSelf = WithDrawManager.Instance.GetSelectId() == task.TaskId;
            if (isSelf && WithDrawManager.Instance.IsInWithDrawProgress)
            {
                //可领奖时长 = 现在时间+额外领奖时长
                task.CanRewardTime = TimeUtils.ConvertDateTimeLong(DateTime.Now) + WithDrawManager.Instance.GetCoolTime();
  
                WithDrawManager.Instance.IsInWithDrawProgress = false;
            }
            UpdateState();
            //此处是否刷新UI取决于界面是否关闭
            if (itemUI!=null)
            {
                Debug.Log("绑定过刷新Ui taskId:{task.TaskId}");
                Messenger.Broadcast(WithDrawConstants.UpdateRedeemItemState);
            }
            //Messenger.Broadcast(WithDrawConstants.UpdateRedeemItemState);
        }

        public void WithDrawFailed()
        {
            if (state == RedeemItemState.InProgress ||state == RedeemItemState.Wait)
            {
                state = RedeemItemState.Failed;
            }
            //提现状态失败，转换为集卡任务
            RecordItemData recordItemData = ToRecordItemData();
            WithDrawManager.Instance.RemoveRedeemItem(this);
            WithDrawManager.Instance.AddRecordItemData(recordItemData);
        }

        public bool IsFished()
        {
            return state == RedeemItemState.Done || state == RedeemItemState.Failed;
        }

        public RecordItemData ToRecordItemData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["index"] =  index;
            data["platSpIndex"] = platSpIndex;
            data["cash"] = (int)task.TargetNum;
            data["taskId"] = task.TaskId;
            RecordItemData itemData = new RecordItemData(data);
            return itemData;
        }
    }
}