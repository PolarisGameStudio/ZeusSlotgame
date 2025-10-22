using System.Collections.Generic;

namespace System.BuffSystem
{
    public class MoreCashBuff:BaseTimeBuff
    {
        private const string cashMultipleKey = "cashMultiple";
        public float cashMultiple = 2; // Default cash multiple is 2
        public MoreCashBuff(Dictionary<string, object> dict) : base(dict)
        {
            cashMultiple = Utils.Utilities.GetFloat(dict, cashMultipleKey, 2);
        }
        
        public float GetCashMultiple()
        {
            return cashMultiple;
        }
    }
}