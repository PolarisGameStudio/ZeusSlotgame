using System;
using System.Collections.Generic;
using System.Linq;
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
        REWARDED=3
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
        public bool IsSequential { get; protected set; }
        public List<BaseTask> ChildTasks { get; protected set; }
        public int ProgressIndex { get; protected set; } // 对于子任务，表示在父任务中的顺序
        public int ParentTaskId { get; protected set; }
        //对于父任务，相当于正在进行的子任务的序号
        public int ChildIndex { get; protected set; }
        public bool CanAutoComplete { get; protected set; }

        //标记位，用于任务的特殊功能
        public int Mark;
        
        public Action<BaseTask> OnTaskCompleted;
        public Action<BaseTask> OnChildTaskCompleted;
        public BaseTask(Dictionary<string,object> taskInfoDict,BaseTask parentTask = null)
        {
            if (null == taskInfoDict) return;
            ParentTask = parentTask;
            if (ParentTask!=null)
            {
                ParentTaskId = parentTask.TaskId;
            }
            RewardList = Utilities.GetString(taskInfoDict, TaskConstants.RewardList_Key, "");
            TaskId = Utilities.GetInt(taskInfoDict, TaskConstants.TaskId_Key, 0);
            TaskType = Utilities.GetInt(taskInfoDict, TaskConstants.Type_Key, 0);
            CanAutoComplete =Utilities.GetBool(taskInfoDict, TaskConstants.CanAutoComplete_Key, true);
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
            ParentTask = parentTask;
            IsSequential = Utilities.GetBool(taskInfoDict, TaskConstants.IsSequential_Key, false);
            ProgressIndex = Utilities.GetInt(taskInfoDict, TaskConstants.TaskIndex_Key, 0);
            
            // 初始化子任务列表，创建子任务
            ChildTasks = new List<BaseTask>();
            CreateChildTasks(taskInfoDict);
            
            //初始化判断一次状态
            SwitchTaskState();
        }
        
        #region ChildTask
        protected virtual void CreateChildTasks(Dictionary<string,object> taskInfoDict)
        {
            List<object> subTasksObj = Utilities.GetValue<List<object>>(taskInfoDict, TaskConstants.ChildTasks_Key, null);
            if (subTasksObj==null||subTasksObj.Count==0)
            {
                return;
            }
            
            //创建子任务
            for (int i = 0; i < subTasksObj.Count; i++)
            {
                Dictionary<string,object> subTaskDict = subTasksObj[i] as Dictionary<string, object>;
                if (subTaskDict==null) continue;
                int taskId = Utilities.GetInt(subTaskDict, TaskConstants.TaskId_Key, 0);
                //通过TaskManager注册任务，避免重复创建，获取缓存的任务进度
                BaseTask baseTask = TaskManager.Instance.RegisterTask(taskId,subTaskDict,this);
                if (baseTask!=null)
                {
                    AddChildTask(baseTask);
                }
            }
        }
        
        // 新增方法
        public virtual void AddChildTask(BaseTask childTask)
        {
            ChildTasks.Add(childTask);
            childTask.OnTaskCompleted += HandleChildTaskCompleted;
        }

        public virtual bool CanStartChildTask(BaseTask childTask)
        {
            if (!IsSequential) return true;
        
            int index = ChildTasks.IndexOf(childTask);
            if (index <= 0) return true;
            // 检查前一个任务是否完成
            return ChildTasks[index-1].State == (int)TaskState.CLOSE;
        }

        protected virtual void HandleChildTaskCompleted(BaseTask childTask)
        {
            if (State!= (int)TaskState.ONGOING)
            {
                return;
            }
            OnChildTaskCompleted?.Invoke(childTask);
            // 检查所有子任务是否完成
            if (CheckAllChildTasksCompleted())
            {
                CompleteTask();
            }
        }

        public virtual void HandleChildTaskActivated()
        {
            
        }
        
        protected virtual bool CheckAllChildTasksCompleted()
        {
            return ChildTasks.All(child => child.State == (int)TaskState.CLOSE);
        }

        public virtual void CompleteTask()
        {
            State = (int)TaskState.CLOSE;
            OnTaskCompleted?.Invoke(this);
        }
        
        public virtual void ResetTask()
        {
            HasCollectNum = 0;
            IsTaskConditionOK = false;
            State = (int)TaskState.AHEAD;
        }
        #endregion
        
        protected virtual void SwitchTaskState()
        {
            //当前没有父任务时，才进行状态切换判断
            //存在父任务时，需要父任务来控制状态
            if (ParentTask == null)
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
        }
        public virtual void MultipleAddNum()
        {
            AddNumber = Utilities.CastValueInt( AddNumber * multipleAddNum);
        }
        protected virtual void DoCollectAction()
        {
            if (State != (int)TaskState.ONGOING)
            {
                return;
            }
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
            CheckCanCompletedAsChildTask();
        }
        
        private void CheckCanCompletedAsChildTask()
        {
            if (ParentTask == null) return;
            if (IsTaskConditionOK && CanAutoComplete)
            {
                CompleteTask();
            }
        }
        protected virtual bool IsCollectConditionOk(ReelManager reelManager,long totalWin){return true;}

        public virtual void OnSpinAwardEnd(ReelManager reelManager, long totalWin)
        {
            if (IsTaskConditionOK) return;
            if (State != (int)TaskState.ONGOING) return;
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
        
        //加载进度相关动态数据，在执行构造方法之后
        public virtual void LoadSaveDataDict(Dictionary<string,object> taskDict)
        {
            if (taskDict == null) return;
            HasCollectNum = Utilities.GetLong(taskDict, TaskConstants.CollectNumber_Key, 0);
            CanRewardTime = Utilities.GetLong(taskDict, TaskConstants.CanRewardTime_Key, 0);
            SpinTotalNum = Utilities.GetInt(taskDict, TaskConstants.SpinTotalNum_Key, 0);
            State = Utilities.GetInt(taskDict, TaskConstants.TaskState_Key, 0);
            IsTaskConditionOK = HasCollectNum >= TargetNum;
        }

        //存储进度相关动态数据，方便本地存储
        public virtual Dictionary<string, object> GetSaveDataDict()
        {
            Dictionary<string,object> data = new Dictionary<string, object>();
            data[TaskConstants.TaskId_Key] = TaskId;
            data[TaskConstants.CollectNumber_Key] = HasCollectNum;
            data[TaskConstants.CanRewardTime_Key] = CanRewardTime;
            data[TaskConstants.SpinTotalNum_Key] = SpinTotalNum;
            data[TaskConstants.TaskState_Key] = State;
            return data;
        }

        public bool IsSpinRelated()
        {
            if (TaskType== TaskConstants.CollectSpinCountTask_Key ||
                TaskType== TaskConstants.CollectSymbolCountTask_Key ||
                TaskType== TaskConstants.CollectJackpotGameCountTask_Key ||
                TaskType== TaskConstants.CollectTriggerSpinWinCountTask_Key||
                TaskType== TaskConstants.CollectFreeGameTriggerCountTask_Key)
            {
                return true;
            }
            return false;
        }
    }
}