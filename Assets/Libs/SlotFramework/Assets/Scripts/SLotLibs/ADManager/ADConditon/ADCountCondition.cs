
using System;
using System.Collections.Generic;

namespace Ads
{
    [Serializable]
    public class ADCountCondition:ADCondition
    {
        public int TargetTime = 0;
        public int HasCollectTime = 0;
        public int FreeCount = 0;
        public int CurrentFreeCount = 0;
        public ADCountCondition(Dictionary<string, object> data) : base(data)
        {
            TargetTime = Utils.Utilities.GetInt(data, "target", 0);
            FreeCount = Utils.Utilities.GetInt(data, "freeCount", 0);
        }
    
        public override void ResetCondition()
        {
            CurrentFreeCount = FreeCount;
            HasCollectTime = 0;
        }
    
        public override void UpdateCondition()
        {
            CurrentFreeCount++;
            if (CurrentFreeCount>FreeCount)
            {
                HasCollectTime++;
            }
        }
    
        public override bool isMeetCondition()
        {
            return HasCollectTime >= TargetTime;
        }

        public override void Clone(ADCondition adCondition)
        {
            base.Clone(adCondition);
            if (adCondition is ADCountCondition adCountCondition)
            {
                HasCollectTime = adCountCondition.HasCollectTime;
                CurrentFreeCount = adCountCondition.CurrentFreeCount;
            }
        }
    }
}

