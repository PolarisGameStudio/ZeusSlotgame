using System.Collections.Generic;

namespace Ads
{
    public class CloseLuckyCashAdNode: BaseAdNode
    {
        public CloseLuckyCashAdNode(string name, Dictionary<string, object> data, ADCondition adCondition) : base(name, data, adCondition)
        {
            
        }
        public override bool IsMeetCondition()
        {
            return base.IsMeetCondition()&&ADManager.Instance.CheckSpinInterval();
        }
    }
}