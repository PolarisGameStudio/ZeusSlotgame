using System.Collections.Generic;
using UnityEngine;
using System;
using Libs;

/// <summary>
/// 新手引导管理器
/// 负责控制引导流程和状态管理
/// </summary>
public class TutorialManager : MonoSingleton<TutorialManager>
{
    /// <summary>
    /// 引导步骤枚举
    /// </summary>
    public enum TutorialStep
    {
        None = 0,
        FirstSpin = 1,
        WithDrawButton = 2,
        WithDrawItem = 3,
        LuckyGift = 4,
        AutoSpin = 5
    }

    [Header("Prefab配置")]
    [SerializeField] private TutorialPrefabConfig prefabConfig;

    [Header("调试设置")]
    [SerializeField] private bool enableTutorials = true;
    [SerializeField] private bool debugMode = false;

    private TutorialStep currentStep = TutorialStep.None;
    private Dictionary<TutorialStep, bool> completedSteps = new Dictionary<TutorialStep, bool>();
    private TutorialOverlay currentOverlay;

    /// <summary>
    /// MonoSingleton初始化方法
    /// </summary>
    protected override void Init()
    {
        base.Init();

        // 初始化引导数据
        foreach (TutorialStep step in Enum.GetValues(typeof(TutorialStep)))
        {
            if (step != TutorialStep.None)
            {
                completedSteps[step] = PlayerPrefs.GetInt($"Tutorial_{step}", 0) == 1;
            }
        }

        if (debugMode)
        {
            Debug.Log("[TutorialManager] 引导数据初始化完成");
        }
    }

    /// <summary>
    /// 开始指定引导步骤
    /// </summary>
    /// <param name="step">引导步骤</param>
    /// <param name="parentNode">父节点，留空则使用DialogCanvas</param>
    /// <param name="onComplete">完成回调（可选），点击关闭时触发</param>
    public void StartTutorial(TutorialStep step, Transform parentNode = null, System.Action<TutorialStep> onComplete = null)
    {
        Debug.Log($"[TutorialManager] StartTutorial被调用 - Step: {step}, Parent: {parentNode?.name ?? "default"}, HasCallback: {onComplete != null}");

        if (!enableTutorials)
        {
            Debug.LogWarning("[TutorialManager] 引导功能未启用");
            return;
        }

        if (currentStep != TutorialStep.None)
        {
            Debug.LogWarning($"[TutorialManager] 已有引导正在显示: {currentStep}");
            return;
        }

        // 获取Prefab配置
        if (prefabConfig == null)
        {
            Debug.LogError("[TutorialManager] TutorialPrefabConfig未设置");
            return;
        }

        var stepConfig = prefabConfig.GetStepConfig(step);
        if (stepConfig == null)
        {
            Debug.LogError($"[TutorialManager] 未找到步骤配置: {step}");
            return;
        }

        // 异步加载Prefab
        StartCoroutine(LoadTutorialPrefabAsync(step, stepConfig, parentNode, onComplete));
    }

    /// <summary>
    /// 异步加载Tutorial Prefab
    /// </summary>
    private System.Collections.IEnumerator LoadTutorialPrefabAsync(TutorialStep step, TutorialPrefabConfig.StepPrefab stepConfig, Transform parentNode, System.Action<TutorialStep> onComplete)
    {
        Debug.Log($"[TutorialManager] 开始异步加载Prefab: {stepConfig.prefabPath}");

        bool loadCompleted = false;
        GameObject loadedPrefab = null;

        // 使用ResourceLoadManager异步加载
        yield return ResourceLoadManager.Instance.AsyncLoadResource<GameObject>(
            stepConfig.prefabPath,
            // 加载成功回调
            (name, prefab) =>
            {
                loadedPrefab = prefab;
                loadCompleted = true;
                Debug.Log($"[TutorialManager] Prefab加载成功: {name}");
            },
            // 加载进度回调（可选）
            (progress) =>
            {
                if (debugMode)
                {
                    Debug.Log($"[TutorialManager] Prefab加载进度: {progress * 100:F1}%");
                }
            },
            // Bundle名称（可选）
            null,
            // 加载失败回调
            (error) =>
            {
                Debug.LogError($"[TutorialManager] Prefab加载失败: {stepConfig.prefabPath}, 错误: {error}");
                loadCompleted = true;
            }
        );

        // 等待加载完成
        yield return new UnityEngine.WaitUntil(() => loadCompleted);

        // 检查加载结果
        if (loadedPrefab == null)
        {
            Debug.LogError($"[TutorialManager] 无法加载Prefab: {stepConfig.prefabPath}");
            yield break;
        }

        // 确定挂载节点
        Transform parent = parentNode;
        if (parent == null)
        {
            // 默认使用DialogCanvas
            parent = Libs.UIManager.Instance.Root.transform.parent;
            if (parent == null)
            {
                Debug.LogError("[TutorialManager] 无法确定挂载节点，DialogCanvas未找到");
                yield break;
            }
            Debug.Log($"[TutorialManager] 使用默认挂载节点: {parent.name}");
        }
        else
        {
            Debug.Log($"[TutorialManager] 使用传入挂载节点: {parent.name}");
        }

        // 实例化Prefab
        GameObject overlayInstance = Instantiate(loadedPrefab, parent);
        overlayInstance.name = $"Tutorial_{step}";
        overlayInstance.transform.SetAsLastSibling();  // 确保在最上层

        // 获取TutorialOverlay组件
        currentOverlay = overlayInstance.GetComponent<TutorialOverlay>();
        if (currentOverlay == null)
        {
            Debug.LogError($"[TutorialManager] Prefab缺少TutorialOverlay组件: {stepConfig.prefabPath}");
            Destroy(overlayInstance);
            yield break;
        }

        // 初始化并显示（传入回调）
        currentOverlay.Initialize(step, stepConfig.canClickToClose, onComplete);
        currentOverlay.Show();

        currentStep = step;
        Debug.Log($"[TutorialManager] currentStep 已设置为: {currentStep}");

        // 自动关闭
        if (stepConfig.autoCloseDelay > 0)
        {
            Invoke(nameof(CompleteCurrentTutorial), stepConfig.autoCloseDelay);
        }

        Debug.Log($"[TutorialManager] 引导已显示 - Step: {step}, Prefab: {stepConfig.prefabPath}");
    }

    /// <summary>
    /// 完成指定的引导步骤（带验证）
    /// </summary>
    /// <param name="step">要完成的引导步骤</param>
    /// <param name="overlay">调用者的 TutorialOverlay 实例</param>
    public void CompleteSpecificTutorial(TutorialStep step, TutorialOverlay overlay)
    {
        Debug.Log($"[TutorialManager] CompleteSpecificTutorial 被调用 - 请求步骤: {step}, 当前步骤: {currentStep}");

        // 验证：只有当请求的步骤与当前步骤匹配，且 overlay 实例也匹配时，才执行销毁
        if (currentStep != step)
        {
            Debug.LogWarning($"[TutorialManager] 步骤不匹配！请求: {step}, 当前: {currentStep}，忽略销毁请求");

            // 虽然步骤不匹配，但仍然保存这个步骤的完成状态
            completedSteps[step] = true;
            PlayerPrefs.SetInt($"Tutorial_{step}", 1);
            PlayerPrefs.Save();
            Debug.Log($"[TutorialManager] 已保存步骤完成状态: Tutorial_{step} = 1");

            // FIX: 如果当前步骤仍然是这个旧步骤，需要重置以解除阻塞
            // 这种情况可能发生在异步回调时序问题导致状态不一致
            if (currentStep == step)
            {
                Debug.Log($"[TutorialManager] 检测到 currentStep 仍为 {step}，重置为 None 以解除阻塞");
                currentStep = TutorialStep.None;
                if (currentOverlay == overlay)
                {
                    currentOverlay = null;
                }
            }

            // 让 overlay 自己销毁自己（不通过 TutorialManager）
            Debug.Log($"[TutorialManager] 让 {step} 的 Overlay 自己销毁");
            overlay.Hide(() => {
                Debug.Log($"[TutorialManager] {step} 的 Overlay 已自行销毁");
            });
            return;
        }

        if (currentOverlay != overlay)
        {
            Debug.LogWarning($"[TutorialManager] Overlay 实例不匹配！请求的是步骤 {step} 的实例，但 currentOverlay 已经是其他实例了");

            // 保存完成状态
            completedSteps[step] = true;
            PlayerPrefs.SetInt($"Tutorial_{step}", 1);
            PlayerPrefs.Save();
            Debug.Log($"[TutorialManager] 已保存步骤完成状态: Tutorial_{step} = 1");

            // FIX: 如果当前步骤仍然是这个步骤，需要重置以解除阻塞
            // 步骤匹配但实例不匹配，说明该步骤已完成，应该重置状态
            if (currentStep == step)
            {
                Debug.Log($"[TutorialManager] 检测到 currentStep 仍为 {step}，重置为 None 以解除阻塞");
                currentStep = TutorialStep.None;
                currentOverlay = null;
            }

            // 让 overlay 自己销毁自己
            Debug.Log($"[TutorialManager] 让 {step} 的 Overlay 自己销毁");
            overlay.Hide(() => {
                Debug.Log($"[TutorialManager] {step} 的 Overlay 已自行销毁");
            });
            return;
        }

        Debug.Log($"[TutorialManager] 验证通过，开始完成引导: {step}");

        // 保存完成状态
        completedSteps[currentStep] = true;
        PlayerPrefs.SetInt($"Tutorial_{currentStep}", 1);
        PlayerPrefs.Save();
        Debug.Log($"[TutorialManager] 已保存完成状态到 PlayerPrefs: Tutorial_{currentStep} = 1");

        // 取消自动关闭的延迟调用
        CancelInvoke(nameof(CompleteCurrentTutorial));

        // 隐藏并销毁
        if (currentOverlay != null)
        {
            Debug.Log($"[TutorialManager] 开始隐藏 Overlay: {currentStep}");
            currentOverlay.Hide(() => {
                Debug.Log($"[TutorialManager] Overlay 隐藏完成: {currentStep}");
                currentOverlay = null;
            });
        }
        else
        {
            Debug.LogWarning($"[TutorialManager] currentOverlay 是 null，无法隐藏");
        }

        currentStep = TutorialStep.None;
        Debug.Log($"[TutorialManager] currentStep 已重置为 None");
    }

    /// <summary>
    /// 完成当前引导步骤（旧方法，保留用于自动关闭）
    /// </summary>
    public void CompleteCurrentTutorial()
    {
        Debug.Log($"[TutorialManager] CompleteCurrentTutorial 被调用，当前步骤: {currentStep}");

        if (currentStep == TutorialStep.None)
        {
            Debug.LogWarning("[TutorialManager] 当前没有正在显示的引导（currentStep == None）");
            return;
        }

        Debug.Log($"[TutorialManager] 完成引导: {currentStep}");

        // 保存完成状态
        completedSteps[currentStep] = true;
        PlayerPrefs.SetInt($"Tutorial_{currentStep}", 1);
        PlayerPrefs.Save();
        Debug.Log($"[TutorialManager] 已保存完成状态到 PlayerPrefs: Tutorial_{currentStep} = 1");

        // 取消自动关闭的延迟调用
        CancelInvoke(nameof(CompleteCurrentTutorial));

        // 隐藏并销毁
        if (currentOverlay != null)
        {
            Debug.Log($"[TutorialManager] 开始隐藏 Overlay: {currentStep}");
            currentOverlay.Hide(() => {
                Debug.Log($"[TutorialManager] Overlay 隐藏完成: {currentStep}");
                currentOverlay = null;
            });
        }
        else
        {
            Debug.LogWarning($"[TutorialManager] currentOverlay 是 null，无法隐藏");
        }

        currentStep = TutorialStep.None;
        Debug.Log($"[TutorialManager] currentStep 已重置为 None");
    }

    /// <summary>
    /// 检查是否需要显示指定引导步骤
    /// </summary>
    public bool ShouldShowStep(TutorialStep step)
    {
        // 检查引导功能是否启用
        if (!enableTutorials)
        {
            return false;
        }

        // 检查配置中是否包含对应step
        if (prefabConfig == null || prefabConfig.GetStepConfig(step) == null)
        {
            return false;
        }

        return !completedSteps.ContainsKey(step) || !completedSteps[step];
    }

    /// <summary>
    /// 获取当前引导步骤
    /// </summary>
    public TutorialStep GetCurrentStep() => currentStep;

    /// <summary>
    /// 是否正在显示引导
    /// </summary>
    public bool IsShowingTutorial() => currentStep != TutorialStep.None;

    /// <summary>
    /// 重置所有引导状态
    /// </summary>
    public void ResetAllTutorials()
    {
        foreach (TutorialStep step in Enum.GetValues(typeof(TutorialStep)))
        {
            if (step != TutorialStep.None)
            {
                PlayerPrefs.DeleteKey($"Tutorial_{step}");
                completedSteps[step] = false;
            }
        }
        PlayerPrefs.Save();

        if (debugMode)
        {
            Debug.Log("[TutorialManager] 所有引导状态已重置");
        }
    }

    /// <summary>
    /// MonoSingleton清理方法
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();

        // 清理当前引导
        if (currentOverlay != null)
        {
            currentOverlay.Hide();
            currentOverlay = null;
        }

        if (debugMode)
        {
            Debug.Log("[TutorialManager] 资源已清理");
        }
    }

    // 静态快捷方法
    public static bool ShouldShow(TutorialStep step) => Instance?.ShouldShowStep(step) ?? false;
    public static void Start(TutorialStep step, Transform parentNode = null, System.Action<TutorialStep> onComplete = null) => Instance?.StartTutorial(step, parentNode, onComplete);
    public static void Complete() => Instance?.CompleteCurrentTutorial();
    public static bool IsShowing() => Instance?.IsShowingTutorial() ?? false;
}
