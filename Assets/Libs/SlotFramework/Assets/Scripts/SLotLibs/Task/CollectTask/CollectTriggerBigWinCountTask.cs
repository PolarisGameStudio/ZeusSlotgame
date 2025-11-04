using System.Collections.Generic;
using Classic;

namespace Libs
{
    public class CollectTriggerBigWinCountTask:BaseTask
    {
        public CollectTriggerBigWinCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener<ReelManager, long> (GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
        }
        
        ~CollectTriggerBigWinCountTask()
        {
            // 这里可以添加清理逻辑，如果有需要的话
            Messenger.RemoveListener<ReelManager, long>(GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
        }
        
        protected override bool IsCollectConditionOk(ReelManager reelManager, long totalWin)
        {
            if (IsConditionOK()||State!=(int)TaskState.ONGOING)
            {
                return false;
            }
            if (BaseSlotMachineController.Instance.hasPopReward || BaseSlotMachineController.Instance.isBigWin || BaseSlotMachineController.Instance.isMegaWin || BaseSlotMachineController.Instance.isEpicWin || isWin(totalWin))
            {
                AddNumber = 1;
                return true;
            }
            return false;
        }
        
        private bool isWin(long totalWin)
        {
            if (totalWin >= BaseSlotMachineController.Instance.reelManager.gameConfigs.EpicWinTag * BaseSlotMachineController.Instance.currentBetting) return true;
            else if (totalWin >= BaseSlotMachineController.Instance.reelManager.gameConfigs.MegaWinTag * BaseSlotMachineController.Instance.currentBetting) return true;
            else if (totalWin >= BaseSlotMachineController.Instance.reelManager.gameConfigs.BigWinTag * BaseSlotMachineController.Instance.currentBetting) return true;
            return false;
        }

        public override string GetDesc()
        {
            return "CollectBigWin";
        }
    }
}