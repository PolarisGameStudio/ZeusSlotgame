using System;
using System.Collections.Generic;
using CardSystem;

namespace Libs
{
    /// <summary>
    /// 收集卡牌类型的任务，任务创建时自动获取当前已有的类型的卡牌，完成条件是收集到指定数量的不同类型的卡牌
    /// </summary>
    [Serializable]
    public class CollectCardTypeCountTask:BaseTask
    {
        public CollectCardTypeCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            HasCollectNum = CardSystemManager.Instance.GetHaveCardTypeCount();
            Messenger.AddListener(CardSystemConstants.GetCardNewTypeCountMsg,UpdateCard);
        }

        ~CollectCardTypeCountTask()
        {
            Messenger.RemoveListener(CardSystemConstants.GetCardNewTypeCountMsg,UpdateCard);
        }

        void UpdateCard()
        {
            HasCollectNum = CardSystemManager.Instance.GetHaveCardTypeCount();
            UpdateTaskStatus();
        }
        public override string GetDesc()
        {
            return "CollectNumCards";
        }
    }
}