using Libs;
using TMPro;
using UnityEngine;
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
            infotext.text = TaskManager.Instance.GetTaskInfos(task);
        }
    }
}