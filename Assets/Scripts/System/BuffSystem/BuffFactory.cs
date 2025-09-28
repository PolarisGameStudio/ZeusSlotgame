using System.Collections.Generic;

namespace System.BuffSystem
{
    public class BuffFactory
    {
        public static BaseBuff CreateBuff(Dictionary<string,object> dict)
        {
            BaseBuff buff = null;
            int buffType = Utils.Utilities.GetInt(dict,BuffConstant.BuffTypeKey);
            switch (buffType)
            {
                case BuffConstant.MoreCashBuff:
                    buff = new MoreCashBuff(dict);
                    break;
                case BuffConstant.MultipleWildSymbolBuff:
                    buff = new MultipleWildSymbolBuff(dict);
                    break;
                case BuffConstant.ChangeADMultipleBuff:
                    buff = new ChangeADMultipleBuff(dict);
                    break;
            }
            return buff;
        }
    }
}