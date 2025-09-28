using System.Collections.Generic;

namespace Ads
{
    public class RotationBuffAdNode: BaseAdNode
    {
        public RotationBuffAdNode(string name, Dictionary<string, object> data, ADCondition adCondition) : base(name, data, adCondition)
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