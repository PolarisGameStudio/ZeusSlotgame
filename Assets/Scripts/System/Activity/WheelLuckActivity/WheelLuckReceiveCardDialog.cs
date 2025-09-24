using Ads;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Activity
{
    public class WheelLuckReceiveCardDialog:UIDialog   
    {
        private int _shopItemId;
        private Button _btnCollect;
        private Image _imgIcon;
        private WheelLuckActivity _activity;
        private TextMeshProUGUI _tips;
        private Transform _cardImage;
        protected override void Awake()
        {
            base.Awake();
            _tips = Util.FindObject<TextMeshProUGUI>(transform, "Anchor/Tips");
            _btnCollect = Util.FindObject<Button>(transform, "Anchor/btn_collect");
            _imgIcon = Util.FindObject<Image>(transform, "Anchor/Icon");
            _cardImage = Util.FindObject<Transform>(transform, "Anchor/Card");
            _btnCollect.onClick.AddListener(BtnCloseClick);
            _activity = ActivityManager.Instance.GetActivityByID(WheelLuckActivity.ActiveId) as WheelLuckActivity;
        }
        
        public void SetUIData(int shopItemId,bool isMultiple)
        {
            _shopItemId = shopItemId;
            int multiple = 1;
            if(isMultiple)  multiple = _activity.GetMultipleCount(_shopItemId);
            if (_shopItemId == WheelLuckActivity.MoneyShopItemId)
            {
                _cardImage.gameObject.SetActive(false);
                var randomReward = _activity.GetRandomReward();
                randomReward *= multiple;
                var money = OnLineEarningMgr.Instance.GetMoneyStr(randomReward,needIcon:false);
                _tips.text = $"You are recieved {money} ";
                _activity.AddShopItemCount(_shopItemId,randomReward);
            }
            else
            {
                _cardImage.gameObject.SetActive(true);
                _tips.text = $"You are recieved {multiple} Cards";
                _activity.AddShopItemCount(_shopItemId,multiple);
            }
            
            _activity?.LoadIcon(_shopItemId, _imgIcon);
        }
        
        private void BtnCloseClick()
        {
            Close();
        }
    }
}