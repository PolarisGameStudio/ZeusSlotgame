using System.Collections.Generic;

namespace System.BuffSystem
{
    public class ChangeADMultipleBuff:BaseCountBuff
    {
        private int adMultiple;
        public ChangeADMultipleBuff(Dictionary<string, object> dict) : base(dict)
        {
            adMultiple = Utils.Utilities.GetInt(dict, BuffConstant.ExtraNumKey, 0);
        }
        //累加buff
        public override int GetExtraCount()
        {
            return adMultiple;
        }
    }
}