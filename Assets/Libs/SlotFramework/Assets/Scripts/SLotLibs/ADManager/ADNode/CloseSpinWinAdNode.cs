using System.Collections.Generic;

namespace Ads
{
    public class CloseSpinWinAdNode:BaseAdNode
    {
        public CloseSpinWinAdNode(string name, Dictionary<string, object> data, ADCondition adCondition) : base(name, data, adCondition)
        {
            AddListener();
        }

        public override void AddListener()
        {
            Messenger.AddListener(ADConstants.CloseSpinWinMsg, UpdateCondition);
        }

        public override  void RemoveListener()
        {
            Messenger.RemoveListener(ADConstants.CloseSpinWinMsg, UpdateCondition);
        }

        ~CloseSpinWinAdNode()
        {
            RemoveListener();
        }
        
        public override void DoAction()
        {
            base.DoAction();
            //重置CloseSpinWin广告的条件
            ResetCondition();
            //重置OnLineEarning模块LuckyCash弹出的条件计数
            Messenger.Broadcast(OnLineEarningConstants.ResetLuckyCashMsg);
        }
    }
}