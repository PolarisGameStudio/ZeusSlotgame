using System;
using System.Collections.Generic;
using UnityEngine;

namespace Libs
{
    /// <summary>
    /// 收集现金的任务，任务创建时，HasCollectNum从0开始计算
    /// </summary>
    [Serializable]
    public class CollectCashFromZeroTask: BaseTask
    {
        public CollectCashFromZeroTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            if (TaskId>0)
            {
                Messenger.AddListener<int>(OnLineEarningConstants.IncreaseCashMsg,UpdateTaskNum);
            }
            TargetNum *= OnLineEarningMgr.Instance.GetCashMultiple();
            UpdateTaskStatus();
        }

        ~CollectCashFromZeroTask()
        {
            Messenger.RemoveListener<int>(OnLineEarningConstants.IncreaseCashMsg,UpdateTaskNum);
        }
        private void UpdateTaskNum(int addCash)
        {
            if (IsConditionOK()||State!=(int)TaskState.ONGOING)
            {
                return;
            }
            HasCollectNum += addCash;
            // Debug.Log($"[AccumulateTotalCashTask][UpdateTaskStatus]  taskId:{TaskId}");
            base.UpdateTaskStatus();
        }
        public override string GetProgressDesc()
        {
            return string.Format("{0}/{1}",OnLineEarningMgr.Instance.GetMoneyStr((int)HasCollectNum,0,false,true),
                OnLineEarningMgr.Instance.GetMoneyStr((int)TargetNum,0,false,true));
        }
        
        public override string GetDesc()
        {
            return "WinMoreCash";
        }
    }
}