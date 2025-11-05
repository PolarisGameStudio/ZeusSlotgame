using System.BuffSystem;
using System.Collections.Generic;
using Ads;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Activity
{
    public class RotationBuffDialog:UIDialog
    {
        private BaseBuff _buff;
        private GameObject panelMoreBet;
        private GameObject panelMoreMoney;
        private GameObject panelMoreBonus;
        private Button BtnAd;
        private Button BtnFree;
        private Button CloseBtn;
        private bool isFreeActive = false;
        private bool isAutoPop = false;
        private bool OpenCloseAd = false;
        private int activityId;
        protected void Init()
        {
            panelMoreBet = transform.Find("Anchor/Animation/panelMoreBet").gameObject;
            panelMoreMoney = transform.Find("Anchor/Animation/panelMoreMoney").gameObject;
            panelMoreBonus = transform.Find("Anchor/Animation/panelMoreBonus").gameObject;
            BtnAd = transform.Find("Anchor/Animation/BtnAd").GetComponent<Button>();
            BtnFree = transform.Find("Anchor/Animation/BtnFree").GetComponent<Button>();
            CloseBtn= transform.Find("Anchor/Animation/BtnClose").GetComponent<Button>();
            panelMoreBet.SetActive(false);
            panelMoreMoney.SetActive(false);
            panelMoreBonus.SetActive(false);
            if (BtnAd!=null)
            {
                UGUIEventListener.Get(BtnAd.gameObject).onClick += OnButtonClickHandler;
            }
            if (BtnFree!=null)
            {
                UGUIEventListener.Get(BtnFree.gameObject).onClick += OnButtonClickHandler;
            }
            if (CloseBtn!=null)
            {
                UGUIEventListener.Get(CloseBtn.gameObject).onClick += OnButtonClickHandler;
            }
        }
        protected override void OnEnable()
        {
            Messenger.AddListener(RotationBuffConstant.CloseRotationBuffDialog, CloseDialog);
            Messenger.AddListener<int>(ADConstants.PlayRotationBuffAD,OnPlayAdSuccess);
            Messenger.AddListener<int>(ADConstants.PlayRotationBuffADFailed,OnPlayAdFailed);
            Messenger.AddListener<string>(ADConstants.NotMeetConditionMsg,NotMeetConditionMsg);
        }

        protected override void OnDisable()
        {
            Messenger.RemoveListener(RotationBuffConstant.CloseRotationBuffDialog, CloseDialog);
            Messenger.RemoveListener<int>(ADConstants.PlayRotationBuffAD,OnPlayAdSuccess);
            Messenger.RemoveListener<int>(ADConstants.PlayRotationBuffADFailed,OnPlayAdFailed);
            Messenger.RemoveListener<string>(ADConstants.NotMeetConditionMsg,NotMeetConditionMsg);
        }

        void OnPlayAdSuccess(int adType)
        {
            if (adType==0)
            {
                ActiveBuffActive();
            }
        }
        void OnPlayAdFailed(int adType)
        {
            if (adType == 0)
            {
                ActiveBuffActive();
            }
        }

        void NotMeetConditionMsg(string name)
        {
            if (name == ADEntrances.REWARD_VIDEO_ROTATION_BUFF)
            {
                ActiveBuffActive();
            }
        }
        
        private void CloseDialog()
        {
            //广播弹窗关闭消息
            Messenger.Broadcast<bool>(RotationBuffConstant.RotationBuffDialogClose,false);
            this.Close();
        }
        
        public void SetUIData(Dictionary<string,object> data,BaseBuff buff)
        {
            Init();
            if (data == null)
            {
                return;
            }
            isFreeActive = Utils.Utilities.GetBool(data,"isFree",false);
            _buff = buff;
            isAutoPop = Utils.Utilities.GetBool(data,"isAutoPop",false);
            OpenCloseAd = Utils.Utilities.GetBool(data,"OpenCloseAd",false);
        }

        public override void Refresh()
        {
            base.Refresh();
            if (_buff == null)
            {
                Debug.LogError("RotationBuffDialog: Buff data is null!");
                return;
            }
            
            //根据_buff更新UI显示
            switch (_buff.buffType)
            {
                case BuffConstant.MoreCashBuff:
                    //显示MoreCashBuff相关UI
                    Debug.Log("Displaying More Cash Buff UI");
                    ShowMoreCashUI();
                    break;
                case BuffConstant.ChangeADMultipleBuff:
                    //显示ChangeADMultipleBuff相关UI
                    Debug.Log("Displaying Change AD Multiple Buff UI");
                    ShowMoreBetUI();
                    break;
                case BuffConstant.MultipleWildSymbolBuff:
                    //显示MultipleWildSymbolBuff相关UI
                    Debug.Log("Displaying Multiple Wild Symbol Buff UI");
                    ShowMoreBonusUI();
                    break;
                default:
                    Debug.LogWarning("Unknown buff type");
                    break;
            }

            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (_buff.isActive)
            {
                BtnAd.gameObject.SetActive(false);
                BtnFree.gameObject.SetActive(false);
            }
            else
            {
                BtnAd.gameObject.SetActive(!isFreeActive);
                BtnFree.gameObject.SetActive(isFreeActive);
            }
        }
        
        private void ShowMoreBetUI()
        {
            panelMoreBet.gameObject.SetActive(true);
            // Implement UI logic to show More Bet Buff
            //显示翻倍数
            int count = (_buff as ChangeADMultipleBuff).GetAdMultiple();
            Text txt1 = panelMoreBet.transform.Find("txt1").GetComponent<Text>();
            Text txt2 = panelMoreBet.transform.Find("txt2").GetComponent<Text>();
            txt1.text = "2";
            txt2.text = ""+count;
            
            //显示进度
            string key1 = "MoreBetInfo1";
            UIText tmp_info1 = panelMoreBet.transform.Find("tmp_info1").GetComponent<UIText>();
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,key1);
            int targetnum = _buff.targetNum;
            localizedString.Arguments = new object[] {targetnum,count};
            tmp_info1.SetText(localizedString.GetLocalizedString());
            
            UIText tmp_progress = panelMoreBet.transform.Find("tmp_progress").GetComponent<UIText>();
            tmp_progress.gameObject.SetActive(_buff.isActive);
            // //buff在激活状态
            // if (_buff.isActive)
            // {
            // }
            // else
            // {
            //    
            // }
        }
        private void ShowMoreCashUI()
        {
            panelMoreMoney.gameObject.SetActive(true);
            // Implement UI logic to show More Bonus Buff
            //显示翻倍数
            UIText tmp_multiple = panelMoreMoney.transform.Find("tmp_multiple").GetComponent<UIText>();
            float count = (_buff as MoreCashBuff).GetCashMultiple()-1;
            tmp_multiple.SetText("+"+count*100+"%");
            //显示提示信息
            string key1 = "MoreBonusInfo";
            UIText tmp_info1 = panelMoreMoney.transform.Find("tmp_info1").GetComponent<UIText>();
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,key1);
            string info = count*100 + "%";
            localizedString.Arguments = new object[]{info};
            tmp_info1.SetText(localizedString.GetLocalizedString());
            
            UIText tmp_progress = panelMoreMoney.transform.Find("tmp_progress").GetComponent<UIText>();
            tmp_progress.gameObject.SetActive(_buff.isActive);
        }
        private void ShowMoreBonusUI()
        {
            panelMoreBonus.gameObject.SetActive(true);
            // Implement UI logic to show More Wild Buff
            UIText tmp_progress = panelMoreBonus.transform.Find("tmp_progress").GetComponent<UIText>();
            tmp_progress.gameObject.SetActive(_buff.isActive);
            string key1 = "GetMoreWild";
            UIText tmp_info1 = panelMoreBonus.transform.Find("tmp_info1").GetComponent<UIText>();
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,key1);
            tmp_info1.SetText(localizedString.GetLocalizedString());
        }
        
        private void OnButtonClickHandler(GameObject go)
        {
            if (go == BtnAd.gameObject)
            {
                Debug.Log("Ad button clicked");
                //播放广告，激活buff
                Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance,ADEntrances.REWARD_VIDEO_ROTATION_BUFF);
            }
            else if (go == BtnFree.gameObject)
            {
                Debug.Log("Free button clicked");
                //免费激活buff
                ActiveBuffActive();
            }else if (go == CloseBtn.gameObject)
            {
                Debug.Log("Free button clicked");
                this.Close();
                if (isAutoPop && OpenCloseAd)
                {
                    Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance,ADEntrances.Interstitial_Entrance_CLOSEROTATIONBUFF);
                }
            }
        }
        
        void ActiveBuffActive()
        {
            if (_buff != null)
            {
                PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,_buff.buffName);
                Messenger.Broadcast<int>(BuffConstant.OnBuffActive,_buff.buffId);
                Close();
            }
        }
    }
}