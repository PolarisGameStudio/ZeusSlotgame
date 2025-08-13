using System.Collections.Generic;

namespace Ads
{
    public class JackPotNode: BaseAdNode
    {
        public JackPotNode(string name, Dictionary<string, object> data, ADCondition adCondition) : base(name, data, adCondition)
        {
        }

        public override void DoAction()
        {
            base.DoAction();
            //重置OnLineEarning模块LuckyCash弹出的条件计数
            Messenger.Broadcast(OnLineEarningConstants.ResetLuckyCashMsg);
        }
    }
}