using System.Collections.Generic;

namespace System.BuffSystem
{
    public class MoreCashBuff:BaseTimeBuff
    {
        public int cashMultiple = 2; // Default cash multiple is 2
        public MoreCashBuff(Dictionary<string, object> dict) : base(dict)
        {
            cashMultiple = Utils.Utilities.GetInt(dict, BuffConstant.ExtraNumKey, 2);
        }
        
        public override int GetExtraCount()
        {
            return cashMultiple;
        }
    }
}