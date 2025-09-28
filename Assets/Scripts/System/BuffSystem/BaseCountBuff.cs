using System.Collections.Generic;

namespace System.BuffSystem
{
    /// <summary>
    /// buff 在数次内生效数
    /// </summary>
    public class BaseCountBuff:BaseBuff
    {
        public BaseCountBuff(Dictionary<string, object> dict) : base(dict)
        {
        }

        public override void OnTrigger()
        {
            base.OnTrigger();
            currentNum++;
            Messenger.Broadcast<bool>(UpdateBuffMsg,currentNum<targetNum);
            if (currentNum >= targetNum)
            {
                isActive = false;
                OnDeactivate();
            }
        }
    }
}