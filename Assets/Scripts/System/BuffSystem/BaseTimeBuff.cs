using System.Collections.Generic;
using Libs;

namespace System.BuffSystem
{
    public class BaseTimeBuff:BaseBuff
    {
        public BaseTimeBuff(Dictionary<string, object> dict) : base(dict)
        {
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            if (isActive && isInitialized)
            {
                activiteTime += deltaTime;
                if (activiteTime>=duration && duration>0)
                {
                    isActive = false;
                    OnDeactivate();
                }
            }
        }
    }
}