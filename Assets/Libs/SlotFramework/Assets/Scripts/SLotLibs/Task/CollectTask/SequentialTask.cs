using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Libs
{
    /// <summary>
    /// 顺序完成多轮子任务的任务 
    /// </summary>
    public class SequentialTask:BaseTask
    {
        public Action<BaseTask, int> OnProgressUpdated;
        public Action<BaseTask, int> OnSwitchChildTask;
        public SequentialTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask = null) : base(taskInfoDict, parentTask)
        {
            CheckTaskState();
            SetChildTaskIndex();
        }

        private void SetChildTaskIndex()
        {
            //检测是否所有子任务都完成
            if (CheckAllChildTasksCompleted())
            {
                ChildIndex = ChildTasks.Count - 1;
            }
            else
            {
                for (int i = 0; i < ChildTasks.Count; i++)
                {
                    if (ChildTasks[i].State == (int)TaskState.AHEAD||ChildTasks[i].State == (int)TaskState.ONGOING)
                    {
                        ChildIndex = i;
                        break;
                    }
                }
            }
        }

        public void CheckTaskState()
        {
            if (State != (int)TaskState.ONGOING)
            {
                return;
            }
            //任务全部完成，并且满足时间期限
            if (IsConditionOK())
            {
                State = (int)TaskState.CLOSE;
            }
        }
        
        public void OnActivate()
        {
            StartTime = TimeUtils.ConvertDateTimeLong(DateTime.Now);
            EndTime = StartTime + DurationTime;
        }


        public bool IsAllChildComplete()
        {
            return CheckAllChildTasksCompleted();
        }
        public override bool IsConditionOK()
        {
            return base.IsConditionOK() && StartTime+DurationTime <= TimeUtils.ConvertDateTimeLong(DateTime.Now);
        }

        protected override void HandleChildTaskCompleted(BaseTask childTask)
        {
            if (State != (int)TaskState.ONGOING)
            {
                return;
            }
            OnChildTaskCompleted?.Invoke(childTask);
            // 检查所有子任务是否完成
            if (CheckAllChildTasksCompleted())
            {
                CompleteOneProgress();
            }
            else
            {
                //未全部完成时，更新当前进行中的子任务索引
                ChildIndex++;
                ActiveChildTask();
                OnSwitchChildTask?.Invoke(this, ChildIndex);
            }
        }
        
        public virtual void ActiveChildTask()
        {
            if (State != (int)TaskState.ONGOING)
            {
                return;
            }
            Debug.Log($"[SequentialTask] ActiveChildTask ChildIndex:{ChildIndex}:self:{this.TaskId}");
            if (ChildTasks[ChildIndex].State == (int)TaskState.AHEAD)
            {
                ChildTasks[ChildIndex].State = (int)TaskState.ONGOING;
                ChildTasks[ChildIndex].HandleChildTaskActivated();
            }
        }

        public override void HandleChildTaskActivated()
        {
            //重启第一个子任务
            ChildIndex = 0;
            ActiveChildTask();
        }
        
        protected virtual void CompleteOneProgress()
        {
            AddNumber = 1;
            DoCollectAction();
            IsTaskConditionOK = HasCollectNum >= TargetNum;
            Messenger.Broadcast(UpdateTaskDataMsg);
            OnProgressUpdated?.Invoke(this, (int)HasCollectNum);
            if (IsTaskConditionOK)
            {
                //等待时间满足要求
                if (StartTime+DurationTime<=TimeUtils.ConvertDateTimeLong(DateTime.Now))
                {
                    //完成任务
                    CompleteTask();
                }
            }
            else
            {
                ResetChildTasksForNextProgress();
                OnSwitchChildTask?.Invoke(this, ChildIndex);
            }
        }

        protected virtual void ResetChildTasksForNextProgress()
        {
            foreach (var child in ChildTasks)
            {
                child.ResetTask();
            }
            //重启第一个子任务
            ChildIndex = 0;
            ActiveChildTask();
        }

        public BaseTask GetOnGoingChildTask()
        {
            if (ChildIndex < 0 || ChildIndex >= ChildTasks.Count)
            {
                ChildIndex = 0;
            }
            return ChildTasks[ChildIndex];
        }

        public string GetChildInfo()
        {
            return string.Format("{0}/{1}",ChildIndex,ChildTasks.Count);
        }
    }
}