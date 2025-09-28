using System.Collections.Generic;

namespace System.BuffSystem
{
    public class MultipleWildSymbolBuff:BaseTimeBuff
    {
        private int extraCount;
        public MultipleWildSymbolBuff(Dictionary<string, object> dict) : base(dict)
        {
            extraCount = Utils.Utilities.GetInt(dict, BuffConstant.ExtraNumKey, 0);
        }
        
        public override int GetExtraCount()
        {
            return extraCount;
        }
    }
}