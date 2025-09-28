using System.Collections.Generic;

namespace Ads
{
    public class FreeGameEndAdNode:BaseAdNode
    {
        public FreeGameEndAdNode(string name, Dictionary<string, object> data, ADCondition adCondition) : base(name, data, adCondition)
        {
            
        }

        public override void DoAction()
        {
            base.DoAction();
            // 重置OnLineEarning模块LuckyCash弹出的条件计数
            Messenger.Broadcast(OnLineEarningConstants.ResetLuckyCashMsg);
        }
        public override int GetMultiple()
        {
            int multiple = ADManager.Instance.GetMultipleADBuff();
            return multiple>Multiple?multiple:Multiple;
        }
    }
}