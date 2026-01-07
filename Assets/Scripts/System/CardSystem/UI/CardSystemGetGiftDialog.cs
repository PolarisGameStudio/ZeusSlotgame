using Libs;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.UI;
namespace CardSystem
{
    public class CardSystemGetGiftDialog:UIDialog   
    {
        private Button _btnCollect;
        //private Image _imgIcon;
        private Text _tips;
        //private TextMeshProUGUI _name;
        //private Transform _cardImage;
        protected override void Awake()
        {
            base.Awake();
            _tips = Util.FindObject<Text>(transform, "Anchor/Tips");
            //_name = Util.FindObject<TextMeshProUGUI>(transform, "Anchor/Name");
            _btnCollect = Util.FindObject<Button>(transform, "Anchor/btn_collect");
            //_imgIcon = Util.FindObject<Image>(transform, "Anchor/content/Icon");
            //_cardImage = Util.FindObject<Transform>(transform, "Anchor/content/Card");
            _btnCollect.onClick.AddListener(BtnCloseClick);
        }
        
        public void SetUIData()
        {
            //_cardImage.gameObject.SetActive(false);

            
          
           
            
            var randomReward = CardSystemManager.Instance.GetRandomMoneyReward();
            
            OnLineEarningMgr.Instance.IncreaseCash(randomReward);
            
            var money = OnLineEarningMgr.Instance.GetMoneyStr(randomReward,needIcon:false);
            
            _tips.text = $"x{money}";
            
            // PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.UpdateLevel,OnLineEarningMgr.Instance.GetCashTime());
            
            Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
        }
        
        private void BtnCloseClick()
        {
            Close();
        }
    }
}