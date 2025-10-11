using Libs;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.UI;
namespace CardSystem
{
    public class CardSystemGetGiftDialog:UIDialog   
    {
        private Button _btnCollect;
        private Text _tips;

        private const string GetCardGiftCount = "GetCardGiftCount";
        protected override void Awake()
        {
            base.Awake();
            _tips = Util.FindObject<Text>(transform, "Anchor/Tips");
            _btnCollect = Util.FindObject<Button>(transform, "Anchor/btn_collect");
            _btnCollect.onClick.AddListener(BtnCloseClick);
        }
        
        public void SetUIData()
        {
            var allCount = SharedPlayerPrefs.GetPlayerPrefsIntValue(GetCardGiftCount);
            allCount++;
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint, GetCardGiftCount, allCount);
            SharedPlayerPrefs.SetPlayerPrefsIntValue(GetCardGiftCount,allCount);

            var amount = CardSystemManager.Instance.GetRandomMoneyReward();
          
            var money = OnLineEarningMgr.Instance.GetMoneyStr(amount,needIcon:false);
            
            _tips.text = $"x{money}";
        }
        
        private void BtnCloseClick()
        {
            Close();
        }
    }
}