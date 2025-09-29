using Ads;
using Libs;
using UnityEngine;
using UnityEngine.UI;
namespace Activity
{
    public class WheelLuckGetRewardDialog:UIDialog   
    {
        private int _shopItemId;
        private Button _btnCollect;
        private Button _btnClaim;
        private Image _imageCard;
        private WheelLuckActivity _activity;
        protected override void Awake()
        {
            base.Awake();
            _btnCollect = Util.FindObject<Button>(transform, "Anchor/btn_collect");
            _btnClaim = Util.FindObject<Button>(transform, "Anchor/btn_claim");
            _imageCard = Util.FindObject<Image>(transform, "Anchor/content/Icon");
            _btnCollect.onClick.AddListener(OnClickMutCountBtn);
            _btnClaim.onClick.AddListener(OnClickClaimBtn);
            _activity = ActivityManager.Instance.GetActivityByID(WheelLuckActivity.ActiveId) as WheelLuckActivity;
        }
        private void OnClickClaimBtn()
        {
            WheelLuckActivity.AdType = WheelLuckAdType.Spin;
            Messenger.Broadcast(ADConstants.PlayAdByEntrance,ADEntrances.Interstitial_Entrance_WHEELLUCKBubble);
        }
        private void OnClickMutCountBtn()
        {
            WheelLuckActivity.AdType = WheelLuckAdType.MultipleReward;
            Messenger.Broadcast(ADConstants.PlayAdByEntrance,ADEntrances.REWARD_VIDEO_WHEELLUCK_SPIN);
        }
        
        protected override void OnDisable()
        {
            Messenger.RemoveListener<int>(ADConstants.PlayWheelLuckAD, ShowVideoCallBack);
            Messenger.RemoveListener<int>(ADConstants.PlayWheelLuckADFailed, ShowVideoCallBack);
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Messenger.AddListener<int>(ADConstants.PlayWheelLuckAD, ShowVideoCallBack);
            Messenger.AddListener<int>(ADConstants.PlayWheelLuckADFailed, ShowVideoCallBack);
        }
        
        private void ShowVideoCallBack(int arg0)
        {
            if (WheelLuckActivity.AdType == WheelLuckAdType.MultipleReward)
            {
                Messenger.Broadcast(GameDialogManager.OpenWheelLuckReceiveCardDialogMsg,_shopItemId,true);
                _activity.BuryPoint(WheelLuckActivity.WheelShowAD2Count);
            }else if (WheelLuckActivity.AdType == WheelLuckAdType.Spin)
            {
                Messenger.Broadcast(GameDialogManager.OpenWheelLuckReceiveCardDialogMsg,_shopItemId,false);
                _activity.BuryPoint(WheelLuckActivity.WheelShowAD1Count);
            }
            Close();
        }
        
        public void SetUIData(int shopItemId)
        {
            _shopItemId = shopItemId;
            ShowCardItem();
        }

        private void ShowCardItem()
        {
            _activity.LoadIcon(_shopItemId,_imageCard);
        }
    }
}