using System;
using System.Collections.Generic;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Classic
{
    public class AccountEnsureDialog:UIDialog
    {
        public TextMeshProUGUI cashTmp;
        public Image image;
        private int platformIndex;
        public TextMeshProUGUI emailTMP; 
        public TextMeshProUGUI dataTmp;
        public Button ensureBtn;
        private int money = 0;
        private RedeemItemData _itemData;
        protected override void Awake()
        {
            base.Awake();
            ensureBtn.onClick.AddListener(EnsureBtnClick);
            // UGUIEventListener.Get(closeBtn.gameObject).onClick = CloseBtnClick;
            // UGUIEventListener.Get(ensureBtn.gameObject).onClick = EnsureBtnClick;
        }
        
        public void SetUIData(RedeemItemData data,string account)
        {
            _itemData = data;
            money = (int)_itemData.RewardCash*OnLineEarningMgr.Instance.GetCashMultiple();
            cashTmp.text = OnLineEarningMgr.Instance.GetMoneyStr(money, needIcon: false);
            
            AddressableManager.Instance.LoadAsset<SpriteAtlas>("Platform.spriteatlas", (result) =>
            {
                if (result != null)
                {
                    Sprite sp = result.GetSprite(_itemData.platSpIndex.ToString());
                    if (sp != null)
                    {
                        image.sprite = sp;
                    }
                }
            });

            emailTMP.text = account;
            DateTime now = DateTime.Now;
            dataTmp.text = now.ToString("MM/dd/yyyy");
        }

        private void EnsureBtnClick()
        {
            Debug.Log("[AccountEnsureDialog][EnsureBtnClick]");
            this.Close();
            Messenger.Broadcast<int>(WithDrawConstants.UpdateRedeemItemState,_itemData.CurTask.TaskId);
            WithDrawManager.Instance.ReduceCash(money);
            //Messenger.Broadcast<int>(GameDialogManager.OpenAccountLoginTipsMsg,money);
            WithDrawManager.Instance.CheckShowWithDrawTipDialog(money);
            WithDrawManager.Instance.SendMsg(money);
            Messenger.Broadcast<int>(WithDrawConstants.DoneWithDrawAction,money);
        }
    }
}