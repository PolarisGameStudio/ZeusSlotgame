//看广告条件
using System;
using System.Collections.Generic;

namespace Ads
{
    [Serializable]
    public class ADCondition
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();
        public ADCondition(Dictionary<string, object> data)
        {
        
        }

        //条件满足
        public virtual bool isMeetCondition()
        {
            return true;
        }

        //条件重置
        public virtual void ResetCondition()
        {
        }

        //进度增加
        public virtual void UpdateCondition()
        {
        }
        //克隆,用于本地保存数据读取
        public virtual void Clone(ADCondition adCondition)
        {
        }
    }
}
