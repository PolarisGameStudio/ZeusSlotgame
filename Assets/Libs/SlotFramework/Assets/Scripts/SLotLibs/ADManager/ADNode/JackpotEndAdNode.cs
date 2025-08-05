using System.Collections.Generic;

namespace Ads
{
    public class JackpotEndAdNode:BaseAdNode
    {
        //关闭jackpot领奖弹窗时播放的广告
        public JackpotEndAdNode(string name, Dictionary<string, object> data, ADCondition adCondition) : base(name, data, adCondition)
        {
            AddListener();
        }

        public override void AddListener()
        {
            base.AddListener();
            Messenger.AddListener(ADConstants.JackpotGameEndMsg, UpdateCondition);
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener(ADConstants.JackpotGameEndMsg, UpdateCondition);
        }
        

        ~JackpotEndAdNode()
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