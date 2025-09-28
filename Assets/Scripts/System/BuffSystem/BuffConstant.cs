namespace System.BuffSystem
{
    public class BuffConstant
    {
        //类型
        public const int MoreCashBuff = 10;
        public const int MultipleWildSymbolBuff = 11;
        public const int ChangeADMultipleBuff = 12;

        #region key
        public const string BuffTypeKey = "buffType";
        public const string BuffIdKey = "buffId";
        public const string DurationKey = "duration";
        public const string StartTimeKey = "startTime";
        public const string EndTimeKey = "endTime";
        public const string TargetNumKey = "targetNum";
        public const string CurrentNumKey = "currentNum";
        public const string ExtraNumKey = "extraNum";
        public const string BuffNameKey = "buffName";
        #endregion
        
        //监听
        public static string OnBuffChange = "OnBuffChange";
        public static string OnBuffActive = "OnBuffActive";
        public static string OnBuffDeActive = "OnBuffDeActive";
        public static string OnBuffTrigger = "OnBuffTrigger";
    }
}