using System.Collections.Generic;

namespace System.BuffSystem
{
    public class MultipleWildSymbolBuff:BaseTimeBuff
    {
        private const string extraCountKey = "extraCount";

        private int extraCount;
        public MultipleWildSymbolBuff(Dictionary<string, object> dict) : base(dict)
        {
            extraCount = Utils.Utilities.GetInt(dict,extraCountKey, 0);
        }
        
        public int GetExtraWildSymbolCount()
        {
            return extraCount;
        }
    }
}