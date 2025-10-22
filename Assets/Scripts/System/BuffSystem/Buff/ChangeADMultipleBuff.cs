using System.Collections.Generic;

namespace System.BuffSystem
{
    public class ChangeADMultipleBuff:BaseCountBuff
    {
        private const string adMultipleKey = "adMultiple";
        private int adMultiple;
        public ChangeADMultipleBuff(Dictionary<string, object> dict) : base(dict)
        {
            adMultiple = Utils.Utilities.GetInt(dict, adMultipleKey, 0);
        }
        //累加buff
        public int GetAdMultiple()
        {
            return adMultiple;
        }
    }
}