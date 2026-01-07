using System;
using UnityEngine;
using System.Collections;
using Libs;
using RealYou.Unity.UIAdapter;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.Profiling;

namespace Classic
{
    public class ScreenLoad : MonoBehaviour
    {
        #region 性能监控数据结构

        /// <summary>
        /// 加载阶段性能数据
        /// </summary>
        private class LoadingPhaseMetrics
        {
            public string phaseName;
            public float startTime;
            public float endTime;
            public long startMemory;
            public long endMemory;

            public float Duration => endTime - startTime;
            public long MemoryDelta => endMemory - startMemory;

            public void LogMetrics()
            {
                Debug.Log($"<color=cyan>【性能监控】{phaseName}</color>\n" +
                         $"  ⏱ 耗时: <color=yellow>{Duration:F3}s</color>\n" +
                         $"  📊 内存变化: <color=yellow>{FormatBytes(MemoryDelta)}</color> " +
                         $"({FormatBytes(startMemory)} → {FormatBytes(endMemory)})");
            }

            private string FormatBytes(long bytes)
            {
                if (bytes < 1024) return $"{bytes}B";
                if (bytes < 1024 * 1024) return $"{bytes / 1024f:F2}KB";
                return $"{bytes / (1024f * 1024f):F2}MB";
            }
        }

        // 存储所有阶段的性能数据
        private static System.Collections.Generic.List<LoadingPhaseMetrics> performanceMetrics =
            new System.Collections.Generic.List<LoadingPhaseMetrics>();

        #endregion

        [Header("UI References")] 
        [SerializeField] private Slider progressBar;
        [SerializeField] private UIText progressText;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Image staticImage;
        
        // 进度控制变量
        private float targetProgress = 0f;
        private float currentProgress = 0f;
        private float lastRealProgress = 0f;
        private float progressVelocity = 0f;
        private bool hasSetScreenRatio = false;
        
        // 进度平滑参数
        [Header("Progress Smoothing")]
        [SerializeField] private float minSmoothTime = 0.3f;
        [SerializeField] private float maxSmoothTime = 1.5f;
        [SerializeField] private float minProgressSpeed = 0.5f; // 最小进度速度(0-1/s)
        [SerializeField] private float maxProgressSpeed = 2f;   // 最大进度速度(0-1/s)
        [SerializeField] private float stuckThreshold = 0.02f; // 卡顿判定阈值
        [SerializeField] private float stuckTimeThreshold = 0.5f; // 卡顿时间阈值(秒)
        private float stuckTimer = 0f;
        
        public static string SlotName = GameConstants.Slot_NAME;
        private const float LOAD_TIME = 3f;
        private SlotMachineConfig config;

        void Awake()
        {
            Messenger.AddListener(GameConstants.HIDE_DEFAULT_LOAING_IMAGE, HideDefaultLoadingImage);
        }

        private void OnDestroy()
        {
            Messenger.RemoveListener(GameConstants.HIDE_DEFAULT_LOAING_IMAGE, HideDefaultLoadingImage);
        }

        void Start()
        {
            StartCoroutine(LoadGameSequence());
        }

        private IEnumerator LoadGameSequence()
        {
            // 清空之前的性能数据
            performanceMetrics.Clear();

            // 初始等待和屏幕适配 (权重: 0.01)
            yield return InitializeScreen();
            UpdateRealProgress(0.01f);

            // 1. 初始化Addressables系统 (权重: 0.02)
            yield return AddressableManager.Instance.InitializeAsync(progress => {
                UpdateRealProgress(0.01f + progress * 0.02f); // 0.01-0.03
            });

            // 2. 加载游戏配置 (权重: 0.01) - 配置加载极快，减少权重
            yield return LoadConfig();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"LoadConfig");

            // 3. 预加载核心资源 (权重: 0.85) - 实际最耗时，大幅增加权重
            yield return PreloadCoreAssets();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"PreloadCoreAssets");

            // 4. 预加载机器资源 (权重: 0.08) - 根据实际耗时调整
            yield return PreloadMachineAssets();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"PreloadMachineAssets");

            // 5. 预加载机器配置所需资源 (权重: 0.02) - 减少权重匹配实际
            yield return PreLoadMachineConfig();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"PreLoadMachineConfig");

            // 6. 加载主场景 (权重: 0.01) - 场景加载较快
            yield return LoadMainScene();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"LoadMainSceneEnd");

            // 清理资源（异步，避免卡顿）
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();

            // 确保进度显示100%
            UpdateRealProgress(1f);
            yield return new WaitUntil(() => Mathf.Approximately(currentProgress, 1f));

            // 输出总体性能报告
            LogTotalPerformanceReport();
        }

        #region 初始化阶段
        private IEnumerator InitializeScreen()
        {
#if UNITY_ANDROID || UNITY_IOS
            // 等待两帧确保分辨率正确初始化
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.2f);
#endif
            
            SetAdapterScreenRatio();
            SkySreenUtils.SetScreenResolutions();
            CameraAdapter.AdapterAllCameras();
            
#if UNITY_ANDROID
            yield return new WaitForSeconds(0.2f);
            if (staticImage != null) staticImage.gameObject.SetActive(false);
#endif
        }

        private void SetAdapterScreenRatio()
        {
            if (hasSetScreenRatio) return;
            hasSetScreenRatio = true;
            
            ScreenRatio currentScreenRatio = ScreenRatio.None;
            if (IphoneXAdapter.IsIphoneX())
            {
                currentScreenRatio = ScreenRatio.IphoneX;
            }
            else if (SkySreenUtils.GetScreenSizeType() == SkySreenUtils.ScreenSizeType.Size_4_3)
            {
                currentScreenRatio = ScreenRatio.FourThree;
            }
            else
            {
                currentScreenRatio = ScreenRatio.SixteenNine;
            }

            AdapterBase.SetScreenRatio(currentScreenRatio);
        }
        #endregion

        #region 性能监控辅助方法

        /// <summary>
        /// 开始监控一个加载阶段
        /// </summary>
        private LoadingPhaseMetrics StartPhaseMonitoring(string phaseName)
        {
            var metrics = new LoadingPhaseMetrics
            {
                phaseName = phaseName,
                startTime = Time.realtimeSinceStartup,
                startMemory = Profiler.GetTotalAllocatedMemoryLong()
            };
            Debug.Log($"<color=green>→ 开始阶段: {phaseName}</color>");
            return metrics;
        }

        /// <summary>
        /// 结束监控一个加载阶段
        /// </summary>
        private void EndPhaseMonitoring(LoadingPhaseMetrics metrics)
        {
            metrics.endTime = Time.realtimeSinceStartup;
            metrics.endMemory = Profiler.GetTotalAllocatedMemoryLong();
            metrics.LogMetrics();
            performanceMetrics.Add(metrics);
        }

        /// <summary>
        /// 输出总体性能报告
        /// </summary>
        private void LogTotalPerformanceReport()
        {
            if (performanceMetrics.Count == 0) return;

            float totalDuration = 0f;
            long totalMemoryDelta = 0L;

            Debug.Log("<color=magenta>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            Debug.Log("<color=magenta>【加载性能总览】</color>");

            foreach (var metric in performanceMetrics)
            {
                totalDuration += metric.Duration;
                totalMemoryDelta += metric.MemoryDelta;
                Debug.Log($"  • {metric.phaseName}: {metric.Duration:F3}s (+{FormatBytes(metric.MemoryDelta)})");
            }

            Debug.Log($"<color=yellow>  ⏱ 总耗时: {totalDuration:F3}s</color>");
            Debug.Log($"<color=yellow>  📊 总内存增长: {FormatBytes(totalMemoryDelta)}</color>");
            Debug.Log("<color=magenta>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 0) return $"-{FormatBytes(-bytes)}";
            if (bytes < 1024) return $"{bytes}B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024f:F2}KB";
            return $"{bytes / (1024f * 1024f):F2}MB";
        }

        #endregion

        #region 进度模拟辅助方法

        private bool shouldStopSimulation = false; // 控制模拟进度的停止

        /// <summary>
        /// 在资源加载准备阶段模拟进度推进（避免长时间卡住）
        /// </summary>
        /// <param name="startProgress">起始进度</param>
        /// <param name="endProgress">结束进度</param>
        /// <param name="duration">持续时间（秒）</param>
        private IEnumerator SimulateProgressDuringPreparation(float startProgress, float endProgress, float duration)
        {
            Debug.Log($"<color=orange>【模拟进度启动】{startProgress:P0} → {endProgress:P0}，持续 {duration}秒</color>");
            float elapsed = 0f;
            int frameCount = 0;
            shouldStopSimulation = false; // 重置标志

            while (elapsed < duration && !shouldStopSimulation) // 检查停止标志
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Lerp(startProgress, endProgress, elapsed / duration);
                UpdateSimulatedProgress(progress); // 使用模拟进度更新方法

                frameCount++;
                if (frameCount % 30 == 0) // 每30帧打印一次
                {
                    Debug.Log($"<color=orange>【模拟进度】当前: {progress:P1}, 已耗时: {elapsed:F2}s</color>");
                }

                yield return null;
            }

            if (shouldStopSimulation)
            {
                Debug.Log($"<color=green>【模拟进度中断】真实加载已开始，当前进度: {currentProgress:P1}</color>");
            }
            else
            {
                UpdateSimulatedProgress(endProgress);
                Debug.Log($"<color=orange>【模拟进度完成】到达 {endProgress:P0}</color>");
            }
        }

        #endregion

        #region 配置加载
        private IEnumerator LoadConfig()
        {
            var metrics = StartPhaseMonitoring("配置加载 (LoadConfig)");

            float maxWaitTime = LOAD_TIME;
            float elapsedTime = 0f;
            float baseProgress = 0.03f; // 0.01 + 0.02
            float totalWeight = 0.01f;  // 配置加载极快，权重降低

            bool[] checkpoints = new bool[3];
            float[] checkpointWeights = { 0.3f, 0.3f, 0.4f };

            while (elapsedTime < maxWaitTime)
            {
                // 检查各个模块状态
                if (!checkpoints[0] && BaseGameConsole.HasActiveGameConsole())
                {
                    checkpoints[0] = true;
                    Debug.Log("GameConsole activated");
                }

                if (!checkpoints[1] && BaseGameConsole.IsConfigInit)
                {
                    checkpoints[1] = true;
                    Debug.Log("Config initialized");
                }

                if (!checkpoints[2] && PlatformManager.Instance.IsInitSuccess())
                {
                    checkpoints[2] = true;
                    Debug.Log("PlatformManager initialized");
                }

                // 计算当前进度
                float checkpointProgress = 0f;
                for (int i = 0; i < checkpoints.Length; i++)
                {
                    checkpointProgress += checkpoints[i]
                        ? checkpointWeights[i]
                        : checkpointWeights[i] * (elapsedTime / maxWaitTime);
                }

                UpdateRealProgress(baseProgress + Mathf.Clamp01(checkpointProgress) * totalWeight); // 0.03-0.04

                if (checkpoints[0] && checkpoints[1] && checkpoints[2]) break;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            UpdateRealProgress(0.04f); // 0.03 + 0.01
            EndPhaseMonitoring(metrics);
        }
        #endregion

        #region 资源预加载
        private IEnumerator PreloadCoreAssets()
        {
            var metrics = StartPhaseMonitoring("核心资源预加载 (PreloadCoreAssets)");

            float baseProgress = 0.04f;  // 前面阶段结束位置
            float totalWeight = 0.85f;   // 核心资源预加载占85%，是最耗时的阶段

            // === 1. 加载启动必需资源 (权重: 35%) ===

            // 模拟准备阶段进度：4% → 30% (预计10秒，覆盖实际准备时间)
            StartCoroutine(SimulateProgressDuringPreparation(
                baseProgress,
                baseProgress + 0.26f,  // 推进26%，给足缓冲空间
                10.0f  // 预计准备时间10秒，覆盖实际最长准备时间
            ));

            // 开始真正的资源加载
            yield return AddressableManager.Instance.LoadAssetsByLabelCoroutine<UnityEngine.Object>(
                "PreLoad_Essential",
                objects => {
                    shouldStopSimulation = true; // 设置停止标志
                    Debug.Log($"✓ 加载了 {objects.Count} 个核心必需资源 (PreLoad_Essential)");
                },
                error => {
                    shouldStopSimulation = true;
                    Debug.LogError($"✗ PreLoad_Essential 加载失败: {error}");
                },
                progress => {
                    // 只有当真实进度超过5%时才停止模拟（避免初始微小进度值立即中断）
                    if (progress > 0.05f)
                    {
                        shouldStopSimulation = true;
                        Debug.Log($"<color=cyan>【真实进度接管】progress={progress:P1}</color>");
                    }
                    UpdateRealProgress(baseProgress + progress * totalWeight * 0.35f); // 0.04-0.34
                });

            // === 2. 加载核心图集 (权重: 30%) ===

            float atlas_baseProgress = baseProgress + totalWeight * 0.35f; // 0.34

            // 模拟图集准备阶段：34% → 42% (预计6秒)
            StartCoroutine(SimulateProgressDuringPreparation(
                atlas_baseProgress,
                atlas_baseProgress + 0.08f,  // 推进8%（原6%按比例扩大）
                6.0f
            ));

            yield return AddressableManager.Instance.LoadAssetsByLabelCoroutine<SpriteAtlas>(
                "Atlas_Core",
                atlases => {
                    shouldStopSimulation = true;
                    Debug.Log($"✓ 加载了 {atlases.Count} 个核心图集 (Atlas_Core)");
                },
                error => {
                    shouldStopSimulation = true;
                    Debug.LogError($"✗ Atlas_Core 加载失败: {error}");
                },
                progress => {
                    if (progress > 0.05f)
                    {
                        shouldStopSimulation = true;
                        Debug.Log($"<color=cyan>【真实进度接管】progress={progress:P1}</color>");
                    }
                    UpdateRealProgress(atlas_baseProgress + progress * totalWeight * 0.30f); // 0.34-0.60
                });

            // === 3. 加载核心UI预制体 (权重: 35%) ===

            float ui_baseProgress = baseProgress + totalWeight * 0.65f; // 0.60

            // 模拟UI准备阶段：60% → 68% (预计6秒)
            StartCoroutine(SimulateProgressDuringPreparation(
                ui_baseProgress,
                ui_baseProgress + 0.08f,  // 推进8%（原6%按比例扩大）
                6.0f
            ));

            yield return AddressableManager.Instance.LoadAssetsByLabelCoroutine<GameObject>(
                "PreLoad_UI",
                prefabs => {
                    shouldStopSimulation = true;
                    Debug.Log($"✓ 加载了 {prefabs.Count} 个核心UI预制体 (PreLoad_UI)");
                },
                error => {
                    shouldStopSimulation = true;
                    Debug.LogError($"✗ PreLoad_UI 加载失败: {error}");
                },
                progress => {
                    if (progress > 0.05f)
                    {
                        shouldStopSimulation = true;
                        Debug.Log($"<color=cyan>【真实进度接管】progress={progress:P1}</color>");
                    }
                    UpdateRealProgress(ui_baseProgress + progress * totalWeight * 0.35f); // 0.60-0.89
                });

            EndPhaseMonitoring(metrics);
        }

        private IEnumerator PreloadMachineAssets()
        {
            var metrics = StartPhaseMonitoring("机器资源预加载 (PreloadMachineAssets)");

            float baseProgress = 0.89f;  // 核心资源加载结束位置
            float totalWeight = 0.08f;   // 机器资源占8%

            // 模拟机器资源准备阶段：89% → 92% (预计3秒)
            StartCoroutine(SimulateProgressDuringPreparation(
                baseProgress,
                baseProgress + 0.03f,  // 推进3%（原5%按比例缩小）
                3.0f
            ));

            // 使用AddressableManager加载机器资源
            yield return AddressableManager.Instance.LoadAssetsByLabelCoroutine<GameObject>(
                "Machines",
                machines => {
                    shouldStopSimulation = true;
                    Debug.Log($"✓ 加载了 {machines.Count} 个机器资源 (Machines)");
                },
                error => {
                    shouldStopSimulation = true;
                    Debug.LogError($"✗ Machines 加载失败: {error}");
                },
                progress => {
                    if (progress > 0.05f)
                    {
                        shouldStopSimulation = true;
                        Debug.Log($"<color=cyan>【真实进度接管】progress={progress:P1}</color>");
                    }
                    UpdateRealProgress(baseProgress + progress * totalWeight); // 0.89-0.97
                });

            EndPhaseMonitoring(metrics);
        }
        #endregion

        #region 机器配置加载
        private IEnumerator PreLoadMachineConfig()
        {
            var metrics = StartPhaseMonitoring("机器配置加载 (PreLoadMachineConfig)");

            float baseProgress = 0.97f;  // 机器资源加载结束位置
            float totalWeight = 0.02f;   // 机器配置占2%

            // 立即推进一点进度，表示阶段已启动
            UpdateRealProgress(baseProgress + 0.002f); // 0.97 -> 0.972
            yield return null;

            // 获取基础配置
            config = BaseGameConsole.ActiveGameConsole().SlotMachineConfig(SlotName);
            UpdateRealProgress(baseProgress + totalWeight * 0.25f); // 0.975
            yield return null;

            if (config != null)
            {
                // 解析配置字典
                try
                {
                    config.ParseDict();
                    UpdateRealProgress(baseProgress + totalWeight * 0.50f); // 0.98
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Config parsing failed: {ex.Message}");
                }

                // 初始化Spine资源
                if (config.UseSpine)
                {
                    config.ClearSpineData();
                    yield return StartCoroutine(config.InitSpineAsset(
                        progress => UpdateRealProgress(baseProgress + totalWeight * 0.50f + progress * totalWeight * 0.50f) // 0.98-0.99
                    ));
                }
            }

            UpdateRealProgress(0.99f); // 0.97 + 0.02
            EndPhaseMonitoring(metrics);
        }
        #endregion

        #region 场景加载
        private IEnumerator LoadMainScene()
        {
            var metrics = StartPhaseMonitoring("主场景加载 (LoadMainScene)");

            float baseProgress = 0.99f;  // 机器配置加载结束位置
            float totalWeight = 0.01f;   // 场景加载占1%

            // 立即推进一点进度，表示阶段已启动
            UpdateRealProgress(baseProgress + 0.002f); // 0.99 -> 0.992
            yield return null;

            // 使用AddressableManager加载场景
            yield return AddressableManager.Instance.LoadSceneCor("WesternTreasure.unity", LoadSceneMode.Single,
                onLoaded:(result) => Debug.Log("主场景加载完成"),
                onFailed:error => Debug.LogError(error),
                onProgress:progress => UpdateRealProgress(baseProgress + progress * totalWeight)); // 0.99-1.00

            EndPhaseMonitoring(metrics);
        }
        #endregion

        #region 进度控制核心方法

        // 更新实际加载进度（真实资源加载进度，优先级高）
        private void UpdateRealProgress(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            lastRealProgress = clampedProgress;
            // 只有当真实进度大于当前目标进度时才更新（避免被模拟进度覆盖）
            if (clampedProgress > targetProgress)
            {
                targetProgress = clampedProgress;
                Debug.Log($"<color=cyan>【真实进度更新】{targetProgress:P1}</color>");
            }
            stuckTimer = 0f; // 重置卡顿计时器
        }

        // 更新模拟进度（仅在没有真实进度时使用）
        private void UpdateSimulatedProgress(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            // 只有当模拟进度大于当前目标进度时才更新（避免回退）
            if (clampedProgress > targetProgress)
            {
                targetProgress = clampedProgress;
            }
        }
        
        // 平滑更新显示进度
        private void Update()
        {
            // 检测卡顿情况
            if (Mathf.Abs(targetProgress - currentProgress) > stuckThreshold)
            {
                stuckTimer += Time.deltaTime;
                
                // 如果卡顿时间超过阈值，加速进度条
                if (stuckTimer > stuckTimeThreshold)
                {
                    targetProgress = Mathf.MoveTowards(
                        currentProgress, 
                        lastRealProgress, 
                        maxProgressSpeed * Time.deltaTime
                    );
                }
            }
            else
            {
                stuckTimer = 0f;
            }
            
            // 动态计算平滑时间(基于剩余进度差)
            float remaining = Mathf.Abs(targetProgress - currentProgress);
            float smoothTime = Mathf.Lerp(
                minSmoothTime, 
                maxSmoothTime, 
                Mathf.InverseLerp(0.1f, 0.5f, remaining)
            );
            
            // 使用平滑阻尼函数
            currentProgress = Mathf.SmoothDamp(
                currentProgress, 
                targetProgress, 
                ref progressVelocity, 
                smoothTime,
                maxProgressSpeed
            );
            
            // 确保最小进度速度
            if (progressVelocity < minProgressSpeed * Time.deltaTime)
            {
                currentProgress = Mathf.MoveTowards(
                    currentProgress, 
                    targetProgress, 
                    minProgressSpeed * Time.deltaTime
                );
            }
            
            // 更新UI
            progressBar.value = currentProgress;
            progressText.SetText($"{Mathf.RoundToInt(currentProgress * 100)}%");
        }
        
        #endregion

        public void HideDefaultLoadingImage()
        {
            mainCamera.depth = -1;
        }
    }
}
