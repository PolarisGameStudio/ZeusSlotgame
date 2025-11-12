using System;
using Libs;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Classic
{
    public class WithDrawTipDialog:UIDialog
    {
        public TextMeshProUGUI tipTmp;
        public TextMeshProUGUI titleTmp;
        public TextMeshProUGUI tipTmp2;

        private int cash=0;
        // protected override void Start()
        // {
        //     base.Start();
        //     // 在Start中设置，此时本地化系统通常已经初始化完成
        //     if (LocalizationSettings.InitializationOperation.IsDone)
        //     {
        //         SetPageInfo();
        //     }
        //     else
        //     {
        //         LocalizationSettings.InitializationOperation.Completed += (op) => SetPageInfo();
        //     }
        // }
        
        public void SetData(int cashAmount)
        {
            cash = cashAmount;
        }

        // public void SetTipData()
        // {
        //     LocalizedString title = new LocalizedString(LocalizationManager.Instance.tableName, "withdrawtips1");
        //     string info = string.Format("<color=#00E4FF>{0}</color>",OnLineEarningMgr.Instance.GetMoneyStr(cash, needIcon:false));
        //     title.Arguments = new object[] {info};
        //     titleTmp.text = title.GetLocalizedString();
        // }
        //
        // void SetPageInfo()
        // {
        //     SetTipData();
        //     string page2_info = $"{LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationManager.Instance.tableName, "withdrawtips3")}\n\n" +
        //                         $"{LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationManager.Instance.tableName, "withdrawtips4")}\n\n" +
        //                         $"{LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationManager.Instance.tableName, "withdrawtips5")}";
        //                         tipTmp2.text = page2_info;
        // }
    }
}