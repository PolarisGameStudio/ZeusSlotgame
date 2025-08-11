using System;
using System.Collections.Generic;
using Classic;
using UnityEngine;

namespace Libs
{
    public class CollectFreeSpinSymbolCountTask:BaseTask
    {
        public CollectFreeSpinSymbolCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener<ReelManager, long> (GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);

        }
        ~CollectFreeSpinSymbolCountTask()
        {
            // 这里可以添加清理逻辑，如果有需要的话
            Messenger.RemoveListener<ReelManager, long>(GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
        }
        protected override bool IsCollectConditionOk(ReelManager reelManager, long totalWin)
        {
            int scatterNum = reelManager.GetSpecialCount(SymbolMap.IS_FREESPIN);
            AddNumber = scatterNum;
            return true;
        }
        
        public override string GetDesc()
        {
            return "CollectToWin";
        }
    }
}