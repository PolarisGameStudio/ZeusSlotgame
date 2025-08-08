using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Activity;
using Classic;
using SevenZip.Compression.LZMA;
using UnityEngine;
using Utils;
namespace Libs
{
   [Serializable]
    public enum TaskState
    {
        AHEAD=0,
        ONGOING=1,
        CLOSE=2,
        NONE=3
    }
    
    [System.Serializable]
    public class BaseTask
    {
        public BaseTask ParentTask;
        public string RewardList;
        public int TaskType;
        public long TargetNum;
        public bool IsTaskConditionOK = false;
        public string Description;
        public int TaskId;
        public int AddNumber;
        public long HasCollectNum;
        public long StartTime;
        public long EndTime;
        //玩家点击任务完成后，可以领取奖励的时间
        public long CanRewardTime;
        public int State;
        public string DestroyTaskUIMsg;
        public string UpdateTaskDataMsg;
        public float multipleAddNum = 1;
        public int SpinTotalNum{ get; set;}
        protected int SpinCollectNum { get; set;}
        //标记位，用于任务的特殊功能
        public int Mark;
        public BaseTask(Dictionary<string,object> taskInfoDict,BaseTask parentTask)
        {
            if (null == taskInfoDict) return;
            ParentTask = parentTask;
            RewardList = Utilities.GetString(taskInfoDict, TaskConstants.RewardList_Key, "");
            TaskId = Utilities.GetInt(taskInfoDict, TaskConstants.TaskId_Key, 0);
            TaskType = Utilities.GetInt(taskInfoDict, TaskConstants.Type_Key, 0);
            
            Description = Utilities.GetString(taskInfoDict, TaskConstants.TaskLocalizeDesc_Key, "");
            
            StartTime= Utils.Utilities.GetLong(taskInfoDict, TaskConstants.StartTime_Key, 0);
            EndTime= Utils.Utilities.GetLong(taskInfoDict, TaskConstants.EndTime_Key, 0);
            CanRewardTime= Utils.Utilities.GetLong(taskInfoDict, TaskConstants.CanRewardTime_Key, 0);
            
            HasCollectNum = Utils.Utilities.GetLong(taskInfoDict, TaskConstants.CollectNumber_Key, 0);
            TargetNum = Utils.Utilities.GetLong(taskInfoDict, TaskConstants.TargetNum_Key, 0);
            SpinTotalNum = Utils.Utilities.GetInt(taskInfoDict, TaskConstants.SpinTotalNum_Key, 0);
            
            State = Utils.Utilities.GetInt(taskInfoDict, TaskConstants.TaskState_Key, 0);
            Mark = Utils.Utilities.GetInt(taskInfoDict, TaskConstants.TaskMark_Key, 0);
            DestroyTaskUIMsg = GameConstants.DestroyTaskUIMsg + TaskId.ToString();
            UpdateTaskDataMsg = GameConstants.UpdateTaskDataMsg + TaskId.ToString();
            
            IsTaskConditionOK = HasCollectNum >= TargetNum;
            //初始化判断一次状态
            SwitchTaskState();
        }


        //
        protected virtual void SwitchTaskState()
        {
            //初始化创建时任务未开始，判断一次状态
            if (this.State == (int)TaskState.AHEAD) 
            {
                //-1表明当前任务永久存在，直接切换为进行时态
                if (EndTime == -1)
                {
                    State = (int)TaskState.ONGOING;
                    return;
                }
                long timeMill = TimeUtils.ConvertDateTimeLong(DateTime.Now);
                if (timeMill>StartTime && timeMill<EndTime)
                {
                    State = (int)TaskState.ONGOING;
                }
            }
        }
        public virtual void MultipleAddNum()
        {
            AddNumber = Utilities.CastValueInt( AddNumber * multipleAddNum);
        }
        protected virtual void DoCollectAction()
        {
            HasCollectNum += AddNumber;
            HasCollectNum = Utils.Utilities.ClampLong(HasCollectNum, 0, TargetNum);
        }
        public virtual bool IsConditionOK() {
            return IsTaskConditionOK;
        }
        protected virtual void UpdateTaskStatus()
        {
            IsTaskConditionOK = HasCollectNum >= TargetNum;
            Messenger.Broadcast(UpdateTaskDataMsg);
        }
        protected virtual bool IsCollectConditionOk(ReelManager reelManager,long totalWin){return true;}

        public virtual void OnSpinAwardEnd(ReelManager reelManager, long totalWin)
        {
            if (IsTaskConditionOK) return;
            if (reelManager == null) return;
            SpinTotalNum++;
            if (IsCollectConditionOk(reelManager, totalWin))
            {
                //为了做machinequest 计费点临时加的 可以加快任务收集进度
                MultipleAddNum();
                DoCollectAction();
            }
            UpdateTaskStatus();
        }

        //判断任务状态，是否发送过奖励
        protected virtual void GrantAward()
        {
            if (GetTaskState()!= Libs.TaskState.ONGOING)
            {
                return;
            }
            if (!IsTaskConditionOK)
            {
                return;
            }
            if (!CheckGrantAwardCondition())
            {
                return;
            }
            if (!string.IsNullOrEmpty(RewardList))
            {
                //执行发奖放发
                // string[] rewards = RewardList.Split(';');
                Debug.Log($"[BaseTask][GrantAward] RewardList:{RewardList}");
                // RewardManager.Instance.GrantAwardByStr(RewardList);
            }
        }

        //任务完成后，领取奖励附加条件
        //todo 后续扩展为Condition判断
        public virtual bool CheckGrantAwardCondition()
        {
            //时间条件
            long timeMill = TimeUtils.ConvertDateTimeLong(DateTime.Now);
            if (timeMill>CanRewardTime)
            {
                return true;
            }
            return false;
        }
        
        public TaskState GetTaskState()
        {
            return (TaskState)State;
        }

        /// <summary>
        /// 返回多语言文本的key,不直接返回文本内容，方便外部修改
        /// </summary>
        /// <returns></returns>
        public virtual string GetDesc()
        {
            return Description;
        }
        
        public virtual string GetProgressDesc()
        {
            return $"{HasCollectNum}/{TargetNum}";
        }

        public virtual void Clone(BaseTask task)
        {
            HasCollectNum = task.HasCollectNum;
            CanRewardTime = task.CanRewardTime;
            State = task.State;
            IsTaskConditionOK = task.IsTaskConditionOK;
            ParentTask = task.ParentTask;
            Mark = task.Mark;
        }
    }
}