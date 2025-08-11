using System.Collections.Generic;

namespace Ads
{
    public class JackpotStartAdNode: BaseAdNode
    {
        public JackpotStartAdNode(string name, Dictionary<string, object> data, ADCondition adCondition) : base(name, data, adCondition)
        {
            AddListener();
        }

        public override void AddListener()
        {
            base.AddListener();
            Messenger.AddListener(ADConstants.JackpotGameStartMsg, UpdateCondition);
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener(ADConstants.JackpotGameStartMsg, UpdateCondition);
        }
        

        ~JackpotStartAdNode()
        {
            RemoveListener();
        }

        public override void DoAction()
        {
            base.DoAction();
            //重置计数条件
            ResetCondition();
            //重置OnLineEarning模块LuckyCash弹出的条件计数
            Messenger.Broadcast(OnLineEarningConstants.ResetLuckyCashMsg);
        }
    }
}