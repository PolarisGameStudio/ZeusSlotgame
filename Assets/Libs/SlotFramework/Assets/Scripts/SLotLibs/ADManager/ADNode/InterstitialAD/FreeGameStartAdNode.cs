using System.Collections.Generic;

namespace Ads
{
    public class FreeGameStartAdNode: BaseAdNode
    {
        public FreeGameStartAdNode(string name, Dictionary<string, object> data, ADCondition adCondition) : base(name, data, adCondition)
        {
            AddListener();
        }

        public override void AddListener()
        {
            base.AddListener();
            Messenger.AddListener(ADConstants.CloseFreeGameStartMsg, UpdateCondition);
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener(ADConstants.CloseFreeGameStartMsg, UpdateCondition);
        }
        public override bool IsMeetCondition()
        {
            return base.IsMeetCondition()&&ADManager.Instance.CheckSpinInterval();
        }
        ~FreeGameStartAdNode()
        {
            RemoveListener();
        }

        public override void DoAction()
        {
            base.DoAction();
            //重置计数条件
            ResetCondition();
            //重置OnLineEarning模块LuckyCash弹出的条件计数
            // Messenger.Broadcast(OnLineEarningConstants.ResetLuckyCashMsg);
        }
    }
}