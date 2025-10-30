using Libs;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Classic
{
    public class WithDrawTaskCompletePanel:UIDialog
    {
        private BaseTask task;
        public TextMeshProUGUI infotext;

        protected override void Start()
        {
            base.Start();
            // 在Start中设置，此时本地化系统通常已经初始化完成
            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                SetTaskInfo();
            }
            else
            {
                LocalizationSettings.InitializationOperation.Completed += (op) => SetTaskInfo();
            }
        }

        public void SetData(BaseTask taskData)
        {
            task = taskData;
        }

        public void SetTaskInfo()
        {
            string info = "";
            LocalizedString title = null;
            switch (task.TaskType)
            {
                case TaskConstants.CollectSpinCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawspin");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectNewCardTypeCountTask_Key:
                case TaskConstants.CollectNewCardCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawcard");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.AccumulateCashTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawcash");
                    title.Arguments = new object[] {OnLineEarningMgr.Instance.GetMoneyStr((int)task.TargetNum, needIcon: false)};
                    break;
                case TaskConstants.WatchADTimeTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawad");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectFreeGameTriggerCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawfreegame");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectWildSymbolCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdraww01");
                    title.Arguments = new object[] {task.TargetNum,"<sprite=0>"};
                    break;
                case TaskConstants.CollectSymbolCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdraws01");
                    title.Arguments = new object[] {task.TargetNum,"<sprite=1>"};
                    break;
                case TaskConstants.CollectTriggerSpinWinCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawwin");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectJackpotGameCountTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawjackpot");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
                case TaskConstants.CollectLoginDaysTask_Key:
                    title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawlogin");
                    title.Arguments = new object[] {task.TargetNum};
                    break;
            }
            info = title.GetLocalizedString();
            infotext.text = info;
        }
    }
}