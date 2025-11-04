using System;
using System.Collections.Generic;

namespace Libs
{
    public class CollectLoginDaysTask:BaseTask
    {
        public long activeTime = 0;
        public CollectLoginDaysTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask = null) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener(WithDrawConstants.WithDrawDialogOpened,UpdateTask);
            Messenger.AddListener(GameConstants.OnSceneInit,UpdateTask);
        }

        ~CollectLoginDaysTask()
        {
            Messenger.RemoveListener(WithDrawConstants.WithDrawDialogOpened,UpdateTask);
            Messenger.RemoveListener(GameConstants.OnSceneInit,UpdateTask);
        }
        
        void UpdateTask()
        {
            if (IsTaskConditionOK||State!=(int)TaskState.ONGOING)
            {
                return;
            }
            int days = TimeUtils.CheckSameDayAndGetInterval(activeTime);
            if (days<=0)
            {
                return;
            }
            HasCollectNum = days;
            UpdateTaskStatus();
        }

        public override void ResetTask()
        {
            base.ResetTask();
            activeTime = 0;
        }

        public override Dictionary<string, object> GetSaveDataDict()
        {
            Dictionary<string,object> data = new Dictionary<string, object>();
            data[TaskConstants.TaskId_Key] = TaskId;
            data[TaskConstants.CollectNumber_Key] = HasCollectNum;
            data[TaskConstants.CanRewardTime_Key] = CanRewardTime;
            data[TaskConstants.SpinTotalNum_Key] = SpinTotalNum;
            data[TaskConstants.TaskState_Key] = State;
            data[TaskConstants.ActiveTime_Key] = activeTime;
            return data;
        }

        public override void LoadSaveDataDict(Dictionary<string, object> taskDict)
        {
            base.LoadSaveDataDict(taskDict);
            activeTime = Utils.Utilities.GetLong(taskDict, TaskConstants.ActiveTime_Key, 0);
        }

        public override void HandleChildTaskActivated()
        {
            activeTime = TimeUtils.ConvertDateTimeLong(DateTime.Now);
        }

        public override string GetDesc()
        {
            return "tasklogin";
        }
    }
}