using System.Collections.Generic;
using Classic;

namespace Libs
{
    public class CollectBonusGameCountTask:BaseTask
    {
        public CollectBonusGameCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener(GameConstants.TriggerBonusGame, TriggerBonusGame);
        }
        
        ~CollectBonusGameCountTask()
        {
            // 这里可以添加清理逻辑，如果有需要的话
            Messenger.RemoveListener(GameConstants.TriggerBonusGame, TriggerBonusGame);
        }

        private void TriggerBonusGame()
        {
            if (IsTaskConditionOK)
            {
                return;
            }

            AddNumber = 1;
            //为了做machinequest 计费点临时加的 可以加快任务收集进度
            MultipleAddNum();
            DoCollectAction();
            UpdateTaskStatus();
        }

        
        
        public override string GetDesc()
        {
            return "CollectJackpot";
        }
    }
}