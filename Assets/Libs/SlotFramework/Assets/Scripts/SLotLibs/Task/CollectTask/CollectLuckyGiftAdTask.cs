using System.Collections.Generic;
using Ads;

namespace Libs
{
    public class CollectLuckyGiftAdTask:BaseTask
    {
        public CollectLuckyGiftAdTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask = null) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener<string>(ADConstants.OnPlayVideoEnd,UpdateAdCount);
        }
        ~CollectLuckyGiftAdTask()
        {
            // 这里可以添加清理逻辑，如果有需要的话
            Messenger.RemoveListener<string>(ADConstants.OnPlayVideoEnd,UpdateAdCount);
        }

        void UpdateAdCount(string name)
        {
            if (IsConditionOK()||State!=(int)TaskState.ONGOING)
            {
                return;
            }

            if (name != ADEntrances.Interstitial_Entrance_LUCKYGIFT_ACTIVITY)
            {
                return;
            }
            AddNumber = 1;
            MultipleAddNum();
            DoCollectAction();
            UpdateTaskStatus();
        }
    }
}