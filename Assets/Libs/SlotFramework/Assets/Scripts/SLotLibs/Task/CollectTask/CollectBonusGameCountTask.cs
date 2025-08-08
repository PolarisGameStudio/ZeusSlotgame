using System.Collections.Generic;
using Classic;

namespace Libs
{
    public class CollectBonusGameCountTask:BaseTask
    {
        public CollectBonusGameCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener<ReelManager, long> (GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
        }
        
        ~CollectBonusGameCountTask()
        {
            // 这里可以添加清理逻辑，如果有需要的话
            Messenger.RemoveListener<ReelManager, long>(GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
        }

        protected override bool IsCollectConditionOk(ReelManager reelManager, long totalWin)
        {
            if (!reelManager.HasBonusGame) return false;
            AddNumber = 1;
            return true;
        }
    }
}