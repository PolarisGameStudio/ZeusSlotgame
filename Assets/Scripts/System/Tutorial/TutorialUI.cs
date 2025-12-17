using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ZeusSlotgame.Tutorial
{
    /// <summary>
    /// 引导UI控制器
    /// 负责引导界面的显示、隐藏和交互处理
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [Header("UI组件引用")]
        [SerializeField] private CanvasGroup overlayCanvasGroup;
        [SerializeField] private GameObject highlightEffect;
        [SerializeField] private TextMeshProUGUI tutorialText;
        [SerializeField] private RectTransform arrowTransform;
        [SerializeField] private Button backgroundButton;
        [SerializeField] private Image highlightImage;
        
        [Header("动画设置")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float highlightPulseDuration = 1f;
        [SerializeField] private float highlightMinAlpha = 0.5f;
        [SerializeField] private float highlightMaxAlpha = 1f;
        
        [Header("引导文本配置")]
        [SerializeField] private string firstSpinText = "点击SPIN按钮开始游戏";
        [SerializeField] private string withDrawButtonText = "点击这里可以提现";
        [SerializeField] private string withDrawItemText = "选择提现档位获取奖励";
        [SerializeField] private string luckyGiftText = "点击幸运礼物获取额外奖励";
        [SerializeField] private string autoSpinText = "开启自动旋转功能";
        
        private TutorialManager.TutorialStep currentStep;
        private Tweener highlightTweener;
        
        private void Awake()
        {
            InitializeComponents();
            Hide();
        }
        
        private void InitializeComponents()
        {
            if (backgroundButton != null)
            {
                backgroundButton.onClick.AddListener(OnBackgroundClick);
            }
            
            if (highlightImage != null)
            {
                highlightImage.raycastTarget = false;
            }
        }
        
        /// <summary>
        /// 设置引导步骤
        /// </summary>
        public void SetupTutorial(TutorialManager.TutorialStep step)
        {
            currentStep = step;
            Show();
            
            switch (step)
            {
                case TutorialManager.TutorialStep.FirstSpin:
                    SetupFirstSpinTutorial();
                    break;
                case TutorialManager.TutorialStep.WithDrawButton:
                    SetupWithDrawButtonTutorial();
                    break;
                case TutorialManager.TutorialStep.WithDrawItem:
                    SetupWithDrawItemTutorial();
                    break;
                case TutorialManager.TutorialStep.LuckyGift:
                    SetupLuckyGiftTutorial();
                    break;
                case TutorialManager.TutorialStep.AutoSpin:
                    SetupAutoSpinTutorial();
                    break;
            }
            
            StartHighlightAnimation();
        }
        
        private void SetupFirstSpinTutorial()
        {
            tutorialText.text = firstSpinText;
            PositionHighlightAtUIElement("SpinButton");
        }
        
        private void SetupWithDrawButtonTutorial()
        {
            tutorialText.text = withDrawButtonText;
            PositionHighlightAtUIElement("WithDrawButton");
        }
        
        private void SetupWithDrawItemTutorial()
        {
            tutorialText.text = withDrawItemText;
            PositionHighlightAtUIElement("RedeemItem_0");
        }
        
        private void SetupLuckyGiftTutorial()
        {
            tutorialText.text = luckyGiftText;
            PositionHighlightAtUIElement("LuckyGiftIcon");
        }
        
        private void SetupAutoSpinTutorial()
        {
            tutorialText.text = autoSpinText;
            PositionHighlightAtUIElement("AutoButton");
        }
        
        private void PositionHighlightAtUIElement(string elementName)
        {
            GameObject targetElement = GameObject.Find(elementName);
            if (targetElement != null)
            {
                RectTransform targetRect = targetElement.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    // 将高亮效果定位到目标元素位置
                    highlightEffect.transform.position = targetRect.position;
                    
                    // 调整高亮大小匹配目标元素
                    highlightEffect.GetComponent<RectTransform>().sizeDelta = targetRect.sizeDelta;
                    
                    // 定位箭头
                    arrowTransform.position = targetRect.position + new Vector3(0, targetRect.sizeDelta.y + 50, 0);
                }
            }
            else
            {
                Debug.LogWarning($"[TutorialUI] 未找到UI元素: {elementName}");
            }
        }
        
        private void StartHighlightAnimation()
        {
            if (highlightImage != null && highlightTweener == null)
            {
                highlightTweener = highlightImage.DOFade(highlightMaxAlpha, highlightPulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        
        private void StopHighlightAnimation()
        {
            if (highlightTweener != null)
            {
                highlightTweener.Kill();
                highlightTweener = null;
            }
            
            if (highlightImage != null)
            {
                highlightImage.color = new Color(highlightImage.color.r, highlightImage.color.g, highlightImage.color.b, highlightMaxAlpha);
            }
        }
        
        private void OnBackgroundClick()
        {
            // 只有特定步骤允许点击背景关闭
            if (currentStep == TutorialManager.TutorialStep.WithDrawItem || 
                currentStep == TutorialManager.TutorialStep.LuckyGift)
            {
                TutorialManager.Complete();
            }
        }
        
        /// <summary>
        /// 显示引导界面
        /// </summary>
        public void Show()
        {
            overlayCanvasGroup.alpha = 0;
            overlayCanvasGroup.DOFade(0.8f, fadeDuration);
            overlayCanvasGroup.blocksRaycasts = true;
            
            highlightEffect.SetActive(true);
            tutorialText.gameObject.SetActive(true);
            
            // 禁用事件穿透
            SetRaycastTargets(true);
        }
        
        /// <summary>
        /// 隐藏引导界面
        /// </summary>
        public void Hide()
        {
            overlayCanvasGroup.DOFade(0f, fadeDuration)
                .OnComplete(() => {
                    overlayCanvasGroup.blocksRaycasts = false;
                    highlightEffect.SetActive(false);
                    tutorialText.gameObject.SetActive(false);
                    StopHighlightAnimation();
                });
            
            // 启用事件穿透
            SetRaycastTargets(false);
        }
        
        private void SetRaycastTargets(bool enabled)
        {
            // 设置所有UI元素的raycastTarget
            Graphic[] graphics = GetComponentsInChildren<Graphic>();
            foreach (var graphic in graphics)
            {
                graphic.raycastTarget = enabled;
            }
            
            // 背景按钮始终可点击
            if (backgroundButton != null)
            {
                backgroundButton.interactable = enabled;
            }
        }
        
        /// <summary>
        /// 设置引导文本（用于本地化）
        /// </summary>
        public void SetTutorialTexts(string firstSpin, string withDrawButton, string withDrawItem, string luckyGift, string autoSpin)
        {
            firstSpinText = firstSpin;
            withDrawButtonText = withDrawButton;
            withDrawItemText = withDrawItem;
            luckyGiftText = luckyGift;
            autoSpinText = autoSpin;
        }
    }
}