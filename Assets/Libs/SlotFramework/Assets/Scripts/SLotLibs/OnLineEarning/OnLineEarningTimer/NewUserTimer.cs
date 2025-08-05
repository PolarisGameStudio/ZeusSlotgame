using System.Collections.Generic;
using Classic;
using Libs;

namespace OnLineEarning
{
    /// NewUserTimer is a specific implementation of BaseOnLineEarningTimer for new users. <summary>
    /// 新用户第一次登录时的奖励
    /// </summary>
    public class NewUserTimer:BaseOnLineEarningTimer
    {
        public NewUserTimer(string name, Dictionary<string, object> config = null) : base(name, config)
        {
        }
    }
}