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
        [SerializeField] private GameObject highlightContainer;
        [SerializeField] private TextMeshProUGUI tutorialText;
        [SerializeField] private RectTransform pointerContainer;
        [SerializeField] private Button backgroundButton;

        [Header("高亮边框")]
        [SerializeField] private Image highlightBorder;      // UI_Highlight_Border 矩形边框
        [SerializeField] private Image highlightCircle;      // UI_Highlight_Circle 圆形边框

        [Header("LuckyGift序列帧动画")]
        [SerializeField] private Image luckyGiftParticle;    // 序列帧动画Image
        [SerializeField] private Sprite[] luckyGiftFrames;   // 12帧sprite数组
        [SerializeField] private float frameRate = 24f;       // 帧率
        private int currentFrame = 0;
        private float frameTimer = 0f;

        [Header("指示器")]
        [SerializeField] private Image handPointer;          // UI_Hand_Pointer 手指图标
        [SerializeField] private Image arrowPointer;         // UI_Arrow_Down 箭头图标
        
        [Header("动画设置")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float highlightPulseDuration = 1f;
        [SerializeField] private float highlightMinAlpha = 0.5f;
        [SerializeField] private float highlightMaxAlpha = 1f;
        [SerializeField] private float pointerFloatDistance = 15f;  // 手指浮动距离
        [SerializeField] private float pointerFloatDuration = 0.8f; // 手指浮动周期
        
        [Header("引导文本配置")]
        [SerializeField] private string firstSpinText = "点击SPIN按钮开始游戏";
        [SerializeField] private string withDrawButtonText = "点击这里可以提现";
        [SerializeField] private string withDrawItemText = "选择提现档位获取奖励";
        [SerializeField] private string luckyGiftText = "点击幸运礼物获取额外奖励";
        [SerializeField] private string autoSpinText = "开启自动旋转功能";
        
        private TutorialManager.TutorialStep currentStep;
        private Tweener highlightTweener;
        private Tweener pointerTweener;
        private bool isPlayingSequence = false;
        
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

            // 设置所有高亮边框的raycastTarget为false，允许点击穿透
            if (highlightBorder != null)
            {
                highlightBorder.raycastTarget = false;
            }

            if (highlightCircle != null)
            {
                highlightCircle.raycastTarget = false;
            }

            if (luckyGiftParticle != null)
            {
                luckyGiftParticle.raycastTarget = false;
            }
        }

        private void Update()
        {
            // 播放LuckyGift序列帧动画
            if (isPlayingSequence && luckyGiftFrames != null && luckyGiftFrames.Length > 0)
            {
                frameTimer += Time.deltaTime;
                float frameDuration = 1f / frameRate;

                if (frameTimer >= frameDuration)
                {
                    frameTimer -= frameDuration;
                    currentFrame = (currentFrame + 1) % luckyGiftFrames.Length;

                    if (luckyGiftParticle != null)
                    {
                        luckyGiftParticle.sprite = luckyGiftFrames[currentFrame];
                    }
                }
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
        
        /// <summary>
        /// 高亮类型枚举
        /// </summary>
        private enum HighlightType
        {
            Border,      // 矩形边框（默认）
            Circle,      // 圆形边框
            Sequence     // 序列帧动画（LuckyGift专用）
        }

        private void SetupFirstSpinTutorial()
        {
            tutorialText.text = firstSpinText;
            PositionHighlightAtUIElement("SpinButton", HighlightType.Border);
        }

        private void SetupWithDrawButtonTutorial()
        {
            tutorialText.text = withDrawButtonText;
            PositionHighlightAtUIElement("WithDrawButton", HighlightType.Border);
        }

        private void SetupWithDrawItemTutorial()
        {
            tutorialText.text = withDrawItemText;
            PositionHighlightAtUIElement("RedeemItem_0", HighlightType.Border);
        }
        
        private void SetupLuckyGiftTutorial()
        {
            tutorialText.text = luckyGiftText;

            // LuckyGift使用序列帧动画高亮
            PositionHighlightAtUIElement("LuckyGiftIcon", HighlightType.Sequence);
        }
        
        private void SetupAutoSpinTutorial()
        {
            tutorialText.text = autoSpinText;
            PositionHighlightAtUIElement("AutoButton", HighlightType.Circle);  // Auto按钮通常是圆形
        }

        /// <summary>
        /// 定位高亮效果到目标UI元素
        /// </summary>
        private void PositionHighlightAtUIElement(string elementName, HighlightType type = HighlightType.Border)
        {
            GameObject targetElement = GameObject.Find(elementName);
            if (targetElement == null)
            {
                Debug.LogWarning($"[TutorialUI] 未找到UI元素: {elementName}");
                return;
            }

            RectTransform targetRect = targetElement.GetComponent<RectTransform>();
            if (targetRect == null)
            {
                Debug.LogWarning($"[TutorialUI] UI元素缺少RectTransform: {elementName}");
                return;
            }

            // 定位高亮容器到目标位置
            if (highlightContainer != null)
            {
                highlightContainer.transform.position = targetRect.position;
                highlightContainer.GetComponent<RectTransform>().sizeDelta = targetRect.sizeDelta;
                highlightContainer.SetActive(true);
            }

            // 根据类型显示对应的高亮效果
            switch (type)
            {
                case HighlightType.Border:
                    ShowBorderHighlight(targetRect);
                    break;

                case HighlightType.Circle:
                    ShowCircleHighlight(targetRect);
                    break;

                case HighlightType.Sequence:
                    ShowSequenceHighlight(targetRect);
                    break;
            }

            // 定位指示器
            PositionPointer(targetRect);

            Debug.Log($"[TutorialUI] 高亮定位成功: {elementName}, 类型: {type}");
        }

        /// <summary>
        /// 显示矩形边框高亮
        /// </summary>
        private void ShowBorderHighlight(RectTransform targetRect)
        {
            if (highlightBorder != null)
            {
                highlightBorder.gameObject.SetActive(true);
                highlightBorder.rectTransform.sizeDelta = targetRect.sizeDelta * 1.1f; // 稍微放大
            }

            if (highlightCircle != null)
            {
                highlightCircle.gameObject.SetActive(false);
            }

            if (luckyGiftParticle != null)
            {
                luckyGiftParticle.gameObject.SetActive(false);
            }

            isPlayingSequence = false;
        }

        /// <summary>
        /// 显示圆形边框高亮
        /// </summary>
        private void ShowCircleHighlight(RectTransform targetRect)
        {
            if (highlightCircle != null)
            {
                highlightCircle.gameObject.SetActive(true);
                float maxSize = Mathf.Max(targetRect.sizeDelta.x, targetRect.sizeDelta.y) * 1.2f;
                highlightCircle.rectTransform.sizeDelta = new Vector2(maxSize, maxSize);
            }

            if (highlightBorder != null)
            {
                highlightBorder.gameObject.SetActive(false);
            }

            if (luckyGiftParticle != null)
            {
                luckyGiftParticle.gameObject.SetActive(false);
            }

            isPlayingSequence = false;
        }

        /// <summary>
        /// 显示序列帧动画高亮
        /// </summary>
        private void ShowSequenceHighlight(RectTransform targetRect)
        {
            if (luckyGiftParticle != null && luckyGiftFrames != null && luckyGiftFrames.Length > 0)
            {
                luckyGiftParticle.gameObject.SetActive(true);
                luckyGiftParticle.rectTransform.sizeDelta = targetRect.sizeDelta * 1.3f; // 序列帧稍大
                luckyGiftParticle.sprite = luckyGiftFrames[0];

                // 开始播放序列帧动画
                currentFrame = 0;
                frameTimer = 0f;
                isPlayingSequence = true;

                Debug.Log($"[TutorialUI] 序列帧动画已启动，共{luckyGiftFrames.Length}帧");
            }

            if (highlightBorder != null)
            {
                highlightBorder.gameObject.SetActive(false);
            }

            if (highlightCircle != null)
            {
                highlightCircle.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 定位指示器（手指或箭头）
        /// </summary>
        private void PositionPointer(RectTransform targetRect)
        {
            if (pointerContainer == null) return;

            // 默认定位在目标下方
            Vector3 pointerOffset = new Vector3(0, -targetRect.sizeDelta.y * 0.6f - 60, 0);
            pointerContainer.position = targetRect.position + pointerOffset;

            // 显示手指指示器，隐藏箭头
            if (handPointer != null)
            {
                handPointer.gameObject.SetActive(true);
                StartPointerAnimation();
            }

            if (arrowPointer != null)
            {
                arrowPointer.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 开始高亮呼吸动画
        /// </summary>
        private void StartHighlightAnimation()
        {
            // 高亮边框呼吸动画
            if (highlightBorder != null && highlightBorder.gameObject.activeSelf)
            {
                highlightTweener?.Kill();
                highlightTweener = highlightBorder.DOFade(highlightMinAlpha, highlightPulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            // 圆形边框呼吸动画
            if (highlightCircle != null && highlightCircle.gameObject.activeSelf)
            {
                highlightTweener?.Kill();
                highlightTweener = highlightCircle.DOFade(highlightMinAlpha, highlightPulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }

        /// <summary>
        /// 停止高亮动画
        /// </summary>
        private void StopHighlightAnimation()
        {
            if (highlightTweener != null)
            {
                highlightTweener.Kill();
                highlightTweener = null;
            }

            // 重置透明度
            if (highlightBorder != null)
            {
                highlightBorder.color = new Color(highlightBorder.color.r, highlightBorder.color.g, highlightBorder.color.b, highlightMaxAlpha);
            }

            if (highlightCircle != null)
            {
                highlightCircle.color = new Color(highlightCircle.color.r, highlightCircle.color.g, highlightCircle.color.b, highlightMaxAlpha);
            }

            // 停止序列帧播放
            isPlayingSequence = false;
        }

        /// <summary>
        /// 开始指示器浮动动画
        /// </summary>
        private void StartPointerAnimation()
        {
            if (handPointer != null && handPointer.gameObject.activeSelf)
            {
                pointerTweener?.Kill();

                // 手指上下浮动动画
                Vector3 originalPos = handPointer.rectTransform.anchoredPosition;
                pointerTweener = handPointer.rectTransform.DOAnchorPosY(
                    originalPos.y - pointerFloatDistance,
                    pointerFloatDuration
                )
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            }
        }

        /// <summary>
        /// 停止指示器动画
        /// </summary>
        private void StopPointerAnimation()
        {
            if (pointerTweener != null)
            {
                pointerTweener.Kill();
                pointerTweener = null;
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

            if (highlightContainer != null)
            {
                highlightContainer.SetActive(true);
            }

            if (tutorialText != null)
            {
                tutorialText.gameObject.SetActive(true);
            }

            if (pointerContainer != null)
            {
                pointerContainer.gameObject.SetActive(true);
            }

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

                    if (highlightContainer != null)
                    {
                        highlightContainer.SetActive(false);
                    }

                    if (tutorialText != null)
                    {
                        tutorialText.gameObject.SetActive(false);
                    }

                    if (pointerContainer != null)
                    {
                        pointerContainer.gameObject.SetActive(false);
                    }

                    StopHighlightAnimation();
                    StopPointerAnimation();

                    // 停止序列帧播放
                    isPlayingSequence = false;
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