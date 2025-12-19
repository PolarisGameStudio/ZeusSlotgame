using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Tutorial引导覆盖层组件
/// 挂载到每个Tutorial_XXX Prefab的根节点
/// 负责：生命周期管理、动画播放、点击事件处理
/// </summary>
public class TutorialOverlay : MonoBehaviour
{
    [Header("必须引用")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("高亮元素（至少配置一个）")]
    [SerializeField] private Image highlightBorder;  // UI_Highlight_Border 矩形边框
    [SerializeField] private Image highlightCircle;  // UI_Highlight_Circle 圆形边框

    [Header("序列帧动画（可选）")]
    [SerializeField] private Image sequenceImage;    // 序列帧动画Image
    [SerializeField] private Sprite[] sequenceFrames; // 序列帧Sprite数组
    [SerializeField] private float sequenceFrameRate = 24f; // 序列帧帧率

    [Header("指示器")]
    [SerializeField] private Image handPointer;      // UI_Hand_Pointer 手指图标

    [Header("点击区域（可选）")]
    [SerializeField] private Button clickArea;       // 全屏点击区域

    [Header("动画设置")]
    [SerializeField] private float fadeDuration = 0.3f;

    private TutorialManager.TutorialStep currentStep;
    private bool canClickToClose = true;
    private System.Action<TutorialManager.TutorialStep> onCompleteCallback;

    // 动画引用
    private Tweener handPointerTweener;
    private Tweener highlightBorderTweener;
    private Tweener highlightCircleTweener;
    private SequenceAnimator sequenceAnimator;

    /// <summary>
    /// 初始化（由TutorialManager调用）
    /// </summary>
    /// <param name="step">引导步骤</param>
    /// <param name="allowClickToClose">是否允许点击关闭</param>
    /// <param name="onComplete">完成回调（可选）</param>
    public void Initialize(TutorialManager.TutorialStep step, bool allowClickToClose, System.Action<TutorialManager.TutorialStep> onComplete = null)
    {
        currentStep = step;
        canClickToClose = allowClickToClose;
        onCompleteCallback = onComplete;

        // 设置点击事件
        if (clickArea != null)
        {
            clickArea.onClick.AddListener(OnClickArea);
        }

        Debug.Log($"[TutorialOverlay] 初始化完成 - Step: {step}, CanClickToClose: {allowClickToClose}, HasCallback: {onComplete != null}");
    }

    /// <summary>
    /// 显示引导（淡入动画 + 启动元素动画）
    /// </summary>
    public void Show()
    {
        if (canvasGroup == null)
        {
            Debug.LogError("[TutorialOverlay] CanvasGroup未设置，无法显示");
            return;
        }

        // 淡入动画
        TutorialAnimations.FadeIn(canvasGroup, fadeDuration, OnShowComplete);
    }

    /// <summary>
    /// 淡入完成回调，启动元素动画
    /// </summary>
    private void OnShowComplete()
    {
        // 启动手指浮动动画
        if (handPointer != null && handPointer.gameObject.activeSelf)
        {
            handPointerTweener = TutorialAnimations.StartHandPointerAnimation(handPointer);
        }

        // 启动高亮边框呼吸动画
        if (highlightBorder != null && highlightBorder.gameObject.activeSelf)
        {
            highlightBorderTweener = TutorialAnimations.StartHighlightBorderAnimation(highlightBorder);
        }

        // 启动圆形高亮呼吸动画
        if (highlightCircle != null && highlightCircle.gameObject.activeSelf)
        {
            highlightCircleTweener = TutorialAnimations.StartHighlightCircleAnimation(highlightCircle);
        }

        // 启动序列帧动画
        if (sequenceImage != null && sequenceImage.gameObject.activeSelf &&
            sequenceFrames != null && sequenceFrames.Length > 0)
        {
            sequenceAnimator = TutorialAnimations.StartSequenceAnimation(
                sequenceImage,
                sequenceFrames,
                sequenceFrameRate
            );
        }

        Debug.Log($"[TutorialOverlay] 显示完成，动画已启动: {currentStep}");
    }

    /// <summary>
    /// 隐藏引导（淡出动画 + 销毁）
    /// </summary>
    public void Hide(System.Action onComplete = null)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("[TutorialOverlay] CanvasGroup未设置，直接销毁");
            onComplete?.Invoke();
            Destroy(gameObject);
            return;
        }

        // 停止所有动画
        StopAllAnimations();

        // 淡出动画
        TutorialAnimations.FadeOut(canvasGroup, fadeDuration, () =>
        {
            Debug.Log($"[TutorialOverlay] 淡出完成，销毁实例: {currentStep}");
            onComplete?.Invoke();
            Destroy(gameObject);
        });
    }

    /// <summary>
    /// 点击区域事件
    /// </summary>
    private void OnClickArea()
    {
        if (canClickToClose)
        {
            Debug.Log($"[TutorialOverlay] 点击关闭引导: {currentStep}");

            // 执行完成回调（在通知TutorialManager之前）
            if (onCompleteCallback != null)
            {
                Debug.Log($"[TutorialOverlay] 执行完成回调: {currentStep}");
                try
                {
                    onCompleteCallback.Invoke(currentStep);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[TutorialOverlay] 回调执行异常: {e.Message}\n{e.StackTrace}");
                }
            }
            else
            {
                Debug.Log($"[TutorialOverlay] 没有设置完成回调");
            }

            // 通知TutorialManager完成引导（传入自己的引用以验证）
            Debug.Log($"[TutorialOverlay] 准备通知 TutorialManager 完成引导: {currentStep}");

            if (TutorialManager.Instance == null)
            {
                Debug.LogError("[TutorialOverlay] TutorialManager.Instance 是 null！");
                return;
            }

            // 传入自己的引用，让 TutorialManager 验证是否是当前正在显示的引导
            Debug.Log($"[TutorialOverlay] 调用 CompleteSpecificTutorial，step: {currentStep}");
            TutorialManager.Instance.CompleteSpecificTutorial(currentStep, this);
        }
        else
        {
            Debug.Log($"[TutorialOverlay] 点击被忽略，不允许关闭: {currentStep}");
        }
    }

    /// <summary>
    /// 停止所有动画
    /// </summary>
    private void StopAllAnimations()
    {
        TutorialAnimations.StopAnimation(handPointerTweener);
        TutorialAnimations.StopAnimation(highlightBorderTweener);
        TutorialAnimations.StopAnimation(highlightCircleTweener);
        TutorialAnimations.StopSequenceAnimation(sequenceAnimator);

        handPointerTweener = null;
        highlightBorderTweener = null;
        highlightCircleTweener = null;
        sequenceAnimator = null;
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    private void OnDestroy()
    {
        StopAllAnimations();

        if (clickArea != null)
        {
            clickArea.onClick.RemoveListener(OnClickArea);
        }

        Debug.Log($"[TutorialOverlay] 资源已清理: {currentStep}");
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器调试：验证引用完整性
    /// </summary>
    [UnityEngine.ContextMenu("验证组件引用")]
    private void ValidateReferences()
    {
        Debug.Log("=== TutorialOverlay 组件引用检查 ===");

        if (canvasGroup == null)
            Debug.LogError("❌ CanvasGroup 未设置");
        else
            Debug.Log("✅ CanvasGroup 已设置");

        if (highlightBorder == null && highlightCircle == null && sequenceImage == null)
            Debug.LogWarning("⚠️ 没有配置任何高亮元素（highlightBorder / highlightCircle / sequenceImage）");
        else
            Debug.Log($"✅ 高亮元素已配置: Border={highlightBorder != null}, Circle={highlightCircle != null}, Sequence={sequenceImage != null}");

        if (sequenceImage != null && (sequenceFrames == null || sequenceFrames.Length == 0))
            Debug.LogWarning("⚠️ sequenceImage已设置但sequenceFrames为空");

        if (handPointer == null)
            Debug.LogWarning("⚠️ handPointer 未设置");
        else
            Debug.Log("✅ handPointer 已设置");

        if (clickArea == null)
            Debug.LogWarning("⚠️ clickArea 未设置（如果不需要点击关闭可忽略）");
        else
            Debug.Log("✅ clickArea 已设置");
    }
#endif
}
