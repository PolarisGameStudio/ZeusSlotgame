using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Libs
{
    /// <summary>
    /// 累积总现金的任务，任务创建时会自动获取当前已有的现金数量
    /// </summary>
    [Serializable]
    public class AccumulateTotalCashTask:BaseTask
    {
        public AccumulateTotalCashTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            if (TaskId>0)
            {
                Messenger.AddListener(SlotControllerConstants.OnCashChangeForDisPlay,UpdateTaskNum);
            }
            TargetNum *= OnLineEarningMgr.Instance.GetCashMultiple();
            //hasCollectNum 因为存储的本来就是扩大的倍数所以无需特殊处理
            HasCollectNum = OnLineEarningMgr.Instance.Cash();
            IsTaskConditionOK = HasCollectNum >= TargetNum;
        }

        ~AccumulateTotalCashTask()
        {
            Messenger.RemoveListener(SlotControllerConstants.OnCashChangeForDisPlay,UpdateTaskNum);
        }
        private void UpdateTaskNum()
        {
            // Debug.Log($"[AccumulateTotalCashTask][UpdateTaskStatus]  taskId:{TaskId}");
            HasCollectNum = OnLineEarningMgr.Instance.Cash();
            base.UpdateTaskStatus();
        }

        public override void LoadSaveDataDict(Dictionary<string, object> taskDict)
        {
            base.LoadSaveDataDict(taskDict);
            HasCollectNum = Math.Max(OnLineEarningMgr.Instance.Cash(), this.HasCollectNum);
        }

        public override string GetProgressDesc()
        {
            return string.Format("{0}/{1}",OnLineEarningMgr.Instance.GetMoneyStr((int)HasCollectNum,0,false,true),
                OnLineEarningMgr.Instance.GetMoneyStr((int)TargetNum,0,false,true));
        }
        public override string GetDesc()
        {
            return "CollectNumCash";
        }
    }
}