using System.Collections.Generic;
using Activity;

namespace Libs
{
    public class TaskFactory
    {
        public static BaseTask CreateTask(Dictionary<string,object> taskInfoDict,BaseTask parentTask = null,int type = GameConstants.LeafTask_Key)
        {
            if (taskInfoDict == null) return null;
            if (taskInfoDict.ContainsKey(TaskConstants.sub_tasks_Key)) type = GameConstants.InternalTask_Key;
            type = Utils.Utilities.GetInt(taskInfoDict, TaskConstants.Type_Key,type);
            BaseTask task = null;
            switch (type)
            {
                case TaskConstants.LeafTask_Key:
                    task = new BaseTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.SpinAwardCashTask_Key:
                    task = new SpinAwardCashTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.AccumulateCashTask_Key:
                    task = new AccumulateTotalCashTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectSpinCountTask_Key:
                    task = new CollectSpinCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.WatchADTimeTask_Key:
                    task = new CollectADCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectCardTask_Key:
                    task = new CollectCardTypeCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectCashFromZeroTask_Key:
                    task = new CollectCashFromZeroTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectNewCardCountTask_Key:
                    task = new CollectNewCardCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectNewCardTypeCountTask_Key:
                    task = new CollectNewCardTypeCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectFreeGameTriggerCountTask_Key:
                    task = new CollectFreeGameTriggerCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectTriggerSpinWinCountTask_Key:
                    task = new CollectTriggerBigWinCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectJackpotGameCountTask_Key:
                    task = new CollectBonusGameCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectWildSymbolCountTask_Key:
                    task = new CollectWildSymbolCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectSymbolCountTask_Key:
                    task = new CollectSymbolCountTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.SequentialTask_Key:
                    task = new SequentialTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectLoginDaysTask_Key:
                    task = new CollectLoginDaysTask(taskInfoDict, parentTask);
                    break;
                case TaskConstants.CollectLuckyGiftAdTask_Key:
                    task = new CollectLuckyGiftAdTask(taskInfoDict, parentTask);
                    break;
            }
            return task;
        }
    }
}