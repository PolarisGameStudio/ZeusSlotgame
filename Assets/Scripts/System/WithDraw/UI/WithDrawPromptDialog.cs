using System;
using Libs;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
namespace Classic
{
    public class WithDrawPromptDialog:UIDialog
    {
        public TextMeshProUGUI cashTmp;
        public TextMeshProUGUI tipTmp;

        public Button button;
        private int cash=0;
        protected override void Start()
        {
            base.Start();
            // 在Start中设置，此时本地化系统通常已经初始化完成
            if (LocalizationSettings.InitializationOperation.IsDone)
            {
                SetTipData();
            }
            else
            {
                LocalizationSettings.InitializationOperation.Completed += (op) => SetTipData();
            }
            if (button!=null)
            {
                button.onClick.AddListener(OnGoToWithDrawButtonClick);
            }
        }
        
        public void SetData(int cashAmount)
        {
            cash = cashAmount;
            cashTmp.text = OnLineEarningMgr.Instance.GetMoneyStr(cash, needIcon:false);
        }

        public void SetTipData()
        {
            LocalizedString title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawtips6");
            string info = string.Format("<color=#00E4FF>{0}</color>",OnLineEarningMgr.Instance.GetMoneyStr(cash, needIcon:false));
            title.Arguments = new object[] {info};
            tipTmp.text = title.GetLocalizedString();
        }

        /// <summary>
        /// 按钮点击事件：关闭当前弹窗并跳转至WithDrawDialog
        /// </summary>
        public void OnGoToWithDrawButtonClick()
        {
            // 关闭当前弹窗
            Close();

            // 跳转至WithDrawDialog
            System.WithDrawManager.Instance.ShowWithDrawDialog();
        }
    }
}