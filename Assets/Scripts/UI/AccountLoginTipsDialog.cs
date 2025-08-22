using System;
using Libs;
using UnityEngine;
using UnityEngine.UI;

namespace Classic
{
    public class AccountLoginTipsDialog:UIDialog
    {
      
        public Button ensureBtn;
        public Button closeBtn;
        private int money = 0;
        protected override void Awake()
        {
            base.Awake();
            ensureBtn.onClick.AddListener(EnsureBtnClick);
            closeBtn.onClick.AddListener(Close);
        }
  

        public void SetUIData(int cash)
        {
            money = cash;
        }
        private void EnsureBtnClick()
        {
            Debug.Log("[AccountEnsureDialog][EnsureBtnClick]");
            WithDrawManager.Instance.ReduceCash(money);
            this.Close();
        }
    }
}