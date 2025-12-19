using UnityEngine;

/// <summary>
/// Tutorial引导Prefab配置表
/// ScriptableObject，在Inspector中配置每个步骤对应的Prefab路径
/// </summary>
[CreateAssetMenu(fileName = "TutorialPrefabConfig", menuName = "Tutorial/Prefab Config")]
public class TutorialPrefabConfig : ScriptableObject
{
    [System.Serializable]
    public class StepPrefab
    {
        [Header("基础配置")]
        public TutorialManager.TutorialStep step;
        public string prefabPath;  // AssetResources相对路径，例如: "Tutorial/Prefab/Tutorial_FirstSpin"
        public string displayName; // 显示名称（用于调试和Inspector显示）

        [Header("交互配置")]
        [Tooltip("是否允许点击关闭引导")]
        public bool canClickToClose = true;

        [Tooltip("自动关闭延迟（秒），0表示不自动关闭")]
        public float autoCloseDelay = 0f;
    }

    [Header("引导步骤配置列表")]
    public StepPrefab[] stepPrefabs = new StepPrefab[5];

    /// <summary>
    /// 根据引导步骤获取对应的配置
    /// </summary>
    public StepPrefab GetStepConfig(TutorialManager.TutorialStep step)
    {
        foreach (var config in stepPrefabs)
        {
            if (config.step == step)
            {
                return config;
            }
        }

        Debug.LogWarning($"[TutorialPrefabConfig] 未找到步骤配置: {step}");
        return null;
    }

    /// <summary>
    /// 验证配置完整性（编辑器调试用）
    /// </summary>
    public bool ValidateConfig()
    {
        bool isValid = true;

        foreach (var config in stepPrefabs)
        {
            if (string.IsNullOrEmpty(config.prefabPath))
            {
                Debug.LogError($"[TutorialPrefabConfig] 步骤 {config.step} 的Prefab路径为空");
                isValid = false;
            }
        }

        return isValid;
    }

#if UNITY_EDITOR
    [UnityEngine.ContextMenu("验证配置")]
    private void ValidateInEditor()
    {
        if (ValidateConfig())
        {
            Debug.Log("[TutorialPrefabConfig] ✅ 配置验证通过");
        }
        else
        {
            Debug.LogError("[TutorialPrefabConfig] ❌ 配置验证失败，请检查");
        }
    }
#endif
}
