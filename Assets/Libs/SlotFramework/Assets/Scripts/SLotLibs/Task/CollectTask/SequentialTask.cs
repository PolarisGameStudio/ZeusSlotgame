using System;
using System.Collections.Generic;
using System.Linq;

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
            SetChildTaskIndex();
        }

        private void SetChildTaskIndex()
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
        
        protected override void HandleChildTaskCompleted(BaseTask childTask)
        {
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
            if (ChildTasks[ChildIndex].State == (int)TaskState.AHEAD)
            {
                ChildTasks[ChildIndex].State = (int)TaskState.ONGOING;
                ChildTasks[ChildIndex].HandleChildTaskActivated();
            }
        }
        protected virtual void CompleteOneProgress()
        {
            AddNumber = 1;
            DoCollectAction();
            UpdateTaskStatus();
            OnProgressUpdated?.Invoke(this, (int)HasCollectNum);
            if (IsTaskConditionOK)
            {
                //完成任务
                CompleteTask();
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
    }
}