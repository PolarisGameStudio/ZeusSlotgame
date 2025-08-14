using System.Collections.Generic;
using Classic;

namespace Libs
{
    public class CollectSpinCountTask:BaseTask
    {
        public CollectSpinCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener(SlotControllerConstants.OnSpinEnd, OnSpinEnd);
        }

        ~CollectSpinCountTask()
        {
            Messenger.AddListener(SlotControllerConstants.OnSpinEnd, OnSpinEnd);
        }

        void OnSpinEnd()
        {
            if (IsConditionOK())
            {
                return;
            }

            AddNumber = 1;
            MultipleAddNum();
            DoCollectAction();
            UpdateTaskStatus();
        }
        
        // protected override bool IsCollectConditionOk(ReelManager reelManager,long totalWin){
        //     //只收集付钱的 Spin 次数
        //     AddNumber = 1;
        //     return true;
        // }

        public override string GetDesc()
        {
            return "SpinNumTimes";
        }
    }
}