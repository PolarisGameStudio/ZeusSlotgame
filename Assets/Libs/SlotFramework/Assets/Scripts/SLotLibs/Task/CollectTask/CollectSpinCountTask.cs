using System.Collections.Generic;
using Classic;

namespace Libs
{
    public class CollectSpinCountTask:BaseTask
    {
        public CollectSpinCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener<ReelManager, long> (GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
        }

        ~CollectSpinCountTask()
        {
            Messenger.RemoveListener<ReelManager, long>(GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
        }

        protected override bool IsCollectConditionOk(ReelManager reelManager,long totalWin){
            if (!reelManager.IsSpinCostCoins) return false;
            //只收集付钱的 Spin 次数
            AddNumber = 1;
            return true;
        }

        public override string GetDesc()
        {
            return "SpinNumTimes";
        }
    }
}