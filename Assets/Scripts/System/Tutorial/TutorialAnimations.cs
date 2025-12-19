using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Tutorial公用动画工具类
/// 提供手指指示器、高亮边框、序列帧的动画效果
/// </summary>
public static class TutorialAnimations
{
    /// <summary>
    /// 手指指示器浮动动画配置
    /// </summary>
    public static class HandPointerSettings
    {
        public static float floatDistance = 15f;  // 浮动距离
        public static float floatDuration = 0.8f; // 浮动周期
        public static Ease floatEase = Ease.InOutSine;
    }

    /// <summary>
    /// 高亮边框呼吸动画配置
    /// </summary>
    public static class HighlightSettings
    {
        public static float pulseDuration = 1f;   // 呼吸周期
        public static float minAlpha = 0.5f;      // 最小透明度
        public static float maxAlpha = 1f;        // 最大透明度
        public static Ease pulseEase = Ease.InOutSine;
    }

    /// <summary>
    /// 序列帧动画配置
    /// </summary>
    public static class SequenceSettings
    {
        public static float defaultFrameRate = 24f;  // 默认帧率
    }

    /// <summary>
    /// 启动手指指示器浮动动画
    /// </summary>
    /// <param name="handPointer">手指Image组件</param>
    /// <returns>Tweener动画对象，用于后续Kill</returns>
    public static Tweener StartHandPointerAnimation(Image handPointer)
    {
        if (handPointer == null)
        {
            Debug.LogWarning("[TutorialAnimations] handPointer为null");
            return null;
        }

        RectTransform rectTransform = handPointer.rectTransform;
        Vector2 originalPos = rectTransform.anchoredPosition;

        // 上下浮动动画
        Tweener tweener = rectTransform.DOAnchorPosY(
            originalPos.y - HandPointerSettings.floatDistance,
            HandPointerSettings.floatDuration
        )
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(HandPointerSettings.floatEase);

        Debug.Log($"[TutorialAnimations] 手指浮动动画已启动: {handPointer.name}");
        return tweener;
    }

    /// <summary>
    /// 启动高亮边框呼吸动画
    /// </summary>
    /// <param name="highlightImage">高亮边框Image组件</param>
    /// <returns>Tweener动画对象，用于后续Kill</returns>
    public static Tweener StartHighlightBorderAnimation(Image highlightImage)
    {
        if (highlightImage == null)
        {
            Debug.LogWarning("[TutorialAnimations] highlightImage为null");
            return null;
        }

        // 透明度呼吸动画
        Tweener tweener = highlightImage.DOFade(
            HighlightSettings.minAlpha,
            HighlightSettings.pulseDuration
        )
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(HighlightSettings.pulseEase);

        Debug.Log($"[TutorialAnimations] 高亮呼吸动画已启动: {highlightImage.name}");
        return tweener;
    }

    /// <summary>
    /// 启动圆形高亮边框呼吸动画
    /// </summary>
    /// <param name="highlightCircle">圆形高亮Image组件</param>
    /// <returns>Tweener动画对象，用于后续Kill</returns>
    public static Tweener StartHighlightCircleAnimation(Image highlightCircle)
    {
        if (highlightCircle == null)
        {
            Debug.LogWarning("[TutorialAnimations] highlightCircle为null");
            return null;
        }

        // 透明度呼吸动画
        Tweener tweener = highlightCircle.DOFade(
            HighlightSettings.minAlpha,
            HighlightSettings.pulseDuration
        )
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(HighlightSettings.pulseEase);

        Debug.Log($"[TutorialAnimations] 圆形高亮呼吸动画已启动: {highlightCircle.name}");
        return tweener;
    }

    /// <summary>
    /// 启动序列帧动画
    /// </summary>
    /// <param name="targetImage">目标Image组件</param>
    /// <param name="frames">序列帧Sprite数组</param>
    /// <param name="frameRate">帧率（默认24fps）</param>
    /// <returns>SequenceAnimator组件，用于后续停止</returns>
    public static SequenceAnimator StartSequenceAnimation(Image targetImage, Sprite[] frames, float frameRate = -1)
    {
        if (targetImage == null)
        {
            Debug.LogWarning("[TutorialAnimations] targetImage为null");
            return null;
        }

        if (frames == null || frames.Length == 0)
        {
            Debug.LogWarning("[TutorialAnimations] frames为空或长度为0");
            return null;
        }

        // 使用默认帧率
        if (frameRate <= 0)
        {
            frameRate = SequenceSettings.defaultFrameRate;
        }

        // 创建SequenceAnimator GameObject
        GameObject animatorObject = new GameObject($"SequenceAnimator_{targetImage.name}");
        animatorObject.transform.SetParent(targetImage.transform, false);

        // 添加SequenceAnimator组件
        SequenceAnimator animator = animatorObject.AddComponent<SequenceAnimator>();
        animator.Initialize(targetImage, frames, frameRate);
        animator.Play();

        Debug.Log($"[TutorialAnimations] 序列帧动画已启动: {targetImage.name}, 帧数: {frames.Length}, 帧率: {frameRate}fps");
        return animator;
    }

    /// <summary>
    /// 停止序列帧动画
    /// </summary>
    /// <param name="animator">SequenceAnimator组件</param>
    public static void StopSequenceAnimation(SequenceAnimator animator)
    {
        if (animator != null)
        {
            animator.Stop();
            Object.Destroy(animator.gameObject);
        }
    }

    /// <summary>
    /// 停止并重置Tweener动画
    /// </summary>
    /// <param name="tweener">动画对象</param>
    public static void StopAnimation(Tweener tweener)
    {
        if (tweener != null)
        {
            tweener.Kill();
        }
    }

    /// <summary>
    /// 淡入动画
    /// </summary>
    /// <param name="canvasGroup">CanvasGroup组件</param>
    /// <param name="duration">淡入时长</param>
    /// <param name="onComplete">完成回调</param>
    public static void FadeIn(CanvasGroup canvasGroup, float duration = 0.3f, System.Action onComplete = null)
    {
        if (canvasGroup == null) return;

        canvasGroup.DOKill();
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        canvasGroup.DOFade(1f, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => onComplete?.Invoke());

        Debug.Log($"[TutorialAnimations] 淡入动画已启动");
    }

    /// <summary>
    /// 淡出动画
    /// </summary>
    /// <param name="canvasGroup">CanvasGroup组件</param>
    /// <param name="duration">淡出时长</param>
    /// <param name="onComplete">完成回调</param>
    public static void FadeOut(CanvasGroup canvasGroup, float duration = 0.3f, System.Action onComplete = null)
    {
        if (canvasGroup == null) return;

        canvasGroup.DOKill();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        canvasGroup.DOFade(0f, duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => onComplete?.Invoke());

        Debug.Log($"[TutorialAnimations] 淡出动画已启动");
    }
}

/// <summary>
/// 序列帧动画播放器
/// MonoBehaviour组件，用于在Update中播放序列帧
/// </summary>
public class SequenceAnimator : MonoBehaviour
{
    private Image targetImage;
    private Sprite[] frames;
    private float frameRate;
    private float frameDuration;
    private int currentFrame;
    private float frameTimer;
    private bool isPlaying;

    /// <summary>
    /// 初始化序列帧动画
    /// </summary>
    public void Initialize(Image target, Sprite[] spriteFrames, float fps)
    {
        targetImage = target;
        frames = spriteFrames;
        frameRate = fps;
        frameDuration = 1f / frameRate;
        currentFrame = 0;
        frameTimer = 0f;
        isPlaying = false;

        // 设置初始帧
        if (targetImage != null && frames != null && frames.Length > 0)
        {
            targetImage.sprite = frames[0];
        }
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    public void Play()
    {
        isPlaying = true;
        currentFrame = 0;
        frameTimer = 0f;
        Debug.Log($"[SequenceAnimator] 开始播放: {frames.Length}帧, {frameRate}fps");
    }

    /// <summary>
    /// 停止动画
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        Debug.Log($"[SequenceAnimator] 停止播放");
    }

    /// <summary>
    /// 暂停动画
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }

    /// <summary>
    /// 恢复动画
    /// </summary>
    public void Resume()
    {
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying || targetImage == null || frames == null || frames.Length == 0)
        {
            return;
        }

        frameTimer += Time.deltaTime;

        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;

            // 循环播放
            currentFrame = (currentFrame + 1) % frames.Length;

            // 更新Sprite
            targetImage.sprite = frames[currentFrame];
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"[SequenceAnimator] 已销毁");
    }
}
