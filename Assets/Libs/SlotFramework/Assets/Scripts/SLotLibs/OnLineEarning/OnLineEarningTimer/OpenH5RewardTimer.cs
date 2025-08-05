using System.Collections.Generic;

namespace OnLineEarning
{
    public class OpenH5RewardTimer:BaseOnLineEarningTimer
    {
        public OpenH5RewardTimer(string name, Dictionary<string, object> config = null) : base(name, config)
        {
            
        }

        public override int GetReward300()
        {
            if (OnLineEarningMgr.Instance.isThreeHundredOpen())
            {
                OnLineEarningMgr.Instance.GetThreeHundredConfig().GetReward(true);
            }
            return 0;
        }
    }
}