using System.Collections.Generic;
using Classic;

namespace Libs
{
    public class CollectSymbolCountTask: BaseTask
    {
        public string symbolName = "";
        public CollectSymbolCountTask(Dictionary<string, object> taskInfoDict, BaseTask parentTask) : base(taskInfoDict, parentTask)
        {
            Dictionary<string,object> extraInfos = Utils.Utilities.GetValue<Dictionary<string,object>>(taskInfoDict, TaskConstants.TaskExtras_Key, null);
            symbolName = Utils.Utilities.GetString(extraInfos, "symbolName", "");
            Messenger.AddListener<ReelManager, long> (GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
        }
        
        ~CollectSymbolCountTask()
        {
            // 这里可以添加清理逻辑，如果有需要的话
            Messenger.RemoveListener<ReelManager, long>(GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
        }
        protected override bool IsCollectConditionOk(ReelManager reelManager, long totalWin)
        {
            List<BaseElementPanel> elementList = reelManager.GetElementsWithSymbolName(symbolName);
            AddNumber = (elementList == null || elementList.Count == 0) ? 0 : elementList.Count;
            return true;
        }
        
        public override string GetDesc()
        {
            return "CollectToWin";
        }
    }
}