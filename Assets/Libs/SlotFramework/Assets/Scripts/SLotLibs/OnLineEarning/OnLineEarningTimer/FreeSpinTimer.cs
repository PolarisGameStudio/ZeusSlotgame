using System.Collections.Generic;
using Utils;

namespace OnLineEarning
{
    public class FreeSpinTimer: BaseOnLineEarningTimer
    {
        readonly string Rate_Key = "Rate";
        float rate = 1f;
        public FreeSpinTimer(string name, Dictionary<string, object> config = null) : base(name, config)
        {
            rate = Utilities.GetFloat(config, Rate_Key, 1f);
        }

        public override int GetReward300()
        {
            int num = base.GetReward300();
            return (int) (num * rate);
        }
    }
}