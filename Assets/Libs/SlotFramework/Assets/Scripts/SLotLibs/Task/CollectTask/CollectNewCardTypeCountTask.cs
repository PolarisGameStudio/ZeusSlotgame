using System;
using System.Collections.Generic;
using CardSystem;
namespace Libs
{
    /// <summary>
    /// 收集新卡牌类型数量的任务，任务创建时HasCollectNum为0，完成条件是收集到指定数量的新类型的卡牌
    /// </summary>
    public class CollectNewCardTypeCountTask: BaseTask
    {
        public CollectNewCardTypeCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Messenger.AddListener(CardSystemConstants.GetCardNewTypeCountMsg,UpdateCard);
        }

        ~CollectNewCardTypeCountTask()
        {
            Messenger.RemoveListener(CardSystemConstants.GetCardNewTypeCountMsg,UpdateCard);
        }

        void UpdateCard()
        {
            HasCollectNum++;
            UpdateTaskStatus();
        }

        public override string GetDesc()
        {
            return "CollectNumCards";
        }
    }
}