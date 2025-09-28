using Ads;
using DG.Tweening;
using Libs;
using UnityEngine;
using UnityEngine.UI;
namespace Activity
{
    public class BubbleItem:MonoBehaviour
    {
        public float amplitude = 50f; // 浮动振幅 (上下各50像素)
        public float frequency = 1f;  // 浮动频率 (每秒多少个来回)
        
        // 新增一个最大延迟时间
        [Tooltip("动画开始前最大的随机延迟时间")]
        public float maxStartDelay = 1.0f; 
        

        private Tweener _floatTweener;
        private RectTransform _rectTransform;
        private float _startPosY;

        private Image _icon;
        private WheelLuckActivity _activity;

        private Button _button;
        void Start()
        {
            _activity = ActivityManager.Instance.GetActivityByID(WheelLuckActivity.ActiveId) as WheelLuckActivity;
            _icon =  Util.FindObject<Image>(transform, "Icon");
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);
            StartFloating();
            RefreshIcon();
        }
        private void OnButtonClick()
        {
            WheelLuckActivity.AdType = WheelLuckAdType.Bubble;
            WheelLuckActivity.OnClickBubble = gameObject;
            _button.interactable = false; // 禁用按钮，防止重复点击
            Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance,ADEntrances.REWARD_VIDEO_WHEELLUCK_SPIN);
        }
        
        protected void OnEnable()
        {
            Messenger.AddListener<int>(ADConstants.PlayWheelLuckAD, ShowVideoCallBack);
            Messenger.AddListener<int>(ADConstants.PlayWheelLuckADFailed, ShowVideoCallBack);
            Messenger.AddListener<int>(WheelLuckActivity.RefreshShopItemCurrentCount,RefreshTextCount);
        }
        private void ShowVideoCallBack(int arg0)
        {
            if (WheelLuckActivity.OnClickBubble == null) return; 
            if(WheelLuckActivity.AdType != WheelLuckAdType.Bubble) return;
            if(WheelLuckActivity.OnClickBubble != gameObject) return;
            _button.interactable = true;
            //展示获奖界面
            Messenger.Broadcast(GameDialogManager.OpenWheelLuckReceiveCardDialogMsg,_curShopItemId,false);
            //这个必须放在广播后面，因为RefreshShopItemCurrentCount广播会刷新_curShopItemId，导致获奖界面显示错误
            RefreshIcon();
            
            _activity.BuryPoint(WheelLuckActivity.WheelShowAD3Count);
        }

        protected void OnDisable()
        {
            Messenger.RemoveListener<int>(ADConstants.PlayWheelLuckAD, ShowVideoCallBack);
            Messenger.RemoveListener<int>(ADConstants.PlayWheelLuckADFailed, ShowVideoCallBack);
            Messenger.RemoveListener<int>(WheelLuckActivity.RefreshShopItemCurrentCount,RefreshTextCount);
        }
        private void RefreshTextCount(int shopItemId)
        {
            if(_curShopItemId != shopItemId) return;
            
            var data = _activity.ShopItems[_curShopItemId];

            if (data.TargetNum - data.CurrentSpinCount == 1)  //只差一个
            {
                _curShopItemId = WheelLuckActivity.MoneyShopItemId;
                
                _activity.LoadIcon(_curShopItemId,_icon);
            }
        }

        /// <summary>
        /// 开启浮动效果
        /// </summary>
        private void StartFloating()
        {
            _rectTransform = GetComponent<RectTransform>();
            _startPosY = _rectTransform.anchoredPosition.y;

            // 计算一个随机的延迟时间
            float randomDelay = Random.Range(0f, maxStartDelay);
            
            _floatTweener = _rectTransform.DOAnchorPosY(_startPosY + amplitude, 1 / frequency)
                .SetEase(Ease.InOutSine) // 使用平滑的缓动，Linear会很生硬
                .SetLoops(-1, LoopType.Yoyo) // 无限循环，Yoyo模式
                .SetDelay(randomDelay); // 在这里设置随机延迟
        }


        private int _curShopItemId;
        private void RefreshIcon()
        {
            if(_curShopItemId == WheelLuckActivity.MoneyShopItemId) return;
            
            var oldShopItemId = _curShopItemId;
            
            _curShopItemId = _activity.GetRandomShopItem();

            if (_curShopItemId != WheelLuckActivity.MoneyShopItemId)
            {
                //和之前和图片不相同
                int loopCount = 3;
                while (oldShopItemId == _curShopItemId)
                {
                    loopCount--;
                    _curShopItemId = _activity.GetRandomShopItem();
                    if (loopCount <= 0)
                    {
                        break;
                    }
                }
                
                var data = _activity.ShopItems[_curShopItemId];

                if (data.TargetNum - data.CurrentSpinCount == 1)  //只差一个
                {
                    _curShopItemId = WheelLuckActivity.MoneyShopItemId;
                }
            }
            
            _activity.LoadIcon(_curShopItemId,_icon);
        }
        
        void OnDestroy()
        {
            _floatTweener?.Kill();
            
            if (WheelLuckActivity.OnClickBubble == gameObject)
            {
                WheelLuckActivity.OnClickBubble = null;
            }
        }
    }
}