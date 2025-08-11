using System.Collections.Generic;
using Ads;
namespace Libs
{
    /// <summary>
    /// 收集看广告次数的任务
    /// </summary>
    public class CollectADCountTask:BaseTask
    {
        public CollectADCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener(ADConstants.OnPlayVideoEnd,UpdateAdCount);
        }
        
        ~CollectADCountTask()
        {
            // 这里可以添加清理逻辑，如果有需要的话
            Messenger.RemoveListener(ADConstants.OnPlayVideoEnd,UpdateAdCount);
        }

        void UpdateAdCount()
        {
            if (IsTaskConditionOK)
            {
                return;
            }
            HasCollectNum++;
            UpdateTaskStatus();
            Messenger.Broadcast(UpdateTaskDataMsg);
        }
        public override string GetDesc()
        {
            return "WatchNumVideo";
        }
    }
}