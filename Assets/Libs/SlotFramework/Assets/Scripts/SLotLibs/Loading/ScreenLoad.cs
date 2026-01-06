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

namespace Classic
{
    public class ScreenLoad : MonoBehaviour
    {
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
            // 初始等待和屏幕适配 (权重: 0.02)
            yield return InitializeScreen();
            UpdateRealProgress(0.02f);
            
            // 1. 初始化Addressables系统 (权重: 0.05)
            yield return AddressableManager.Instance.InitializeAsync(progress => {
                UpdateRealProgress(0.02f + progress * 0.05f);
            });
            // 2. 加载游戏配置 (权重: 0.08)
            yield return LoadConfig();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"LoadConfig");
            // 3. 预加载核心资源 (权重: 0.45)
            yield return PreloadCoreAssets();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"PreloadCoreAssets");
            // 4. 预加载机器资源 (权重: 0.20)
            yield return PreloadMachineAssets();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"PreloadMachineAssets");
            // 5. 预加载机器配置所需资源 (权重: 0.15)
            yield return PreLoadMachineConfig();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"PreLoadMachineConfig");
            // 6. 加载主场景 (权重: 0.05)
            yield return LoadMainScene();
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint,"LoadMainSceneEnd");

            // 清理资源（异步，避免卡顿）
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();

            // 确保进度显示100%
            UpdateRealProgress(1f);
            yield return new WaitUntil(() => Mathf.Approximately(currentProgress, 1f));
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

        #region 配置加载
        private IEnumerator LoadConfig()
        {
            float maxWaitTime = LOAD_TIME;
            float elapsedTime = 0f;
            float baseProgress = 0.07f; // 0.02 + 0.05
            
            bool[] checkpoints = new bool[3];
            float[] checkpointWeights = { 0.3f, 0.3f, 0.4f };

            while (elapsedTime < maxWaitTime)
            {
                bool allCompleted = true;
                
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
                
                UpdateRealProgress(baseProgress + Mathf.Clamp01(checkpointProgress) * 0.08f);
                
                if (checkpoints[0] && checkpoints[1] && checkpoints[2]) break;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            UpdateRealProgress(0.15f); // 0.07 + 0.08
        }
        #endregion

        #region 资源预加载
        private IEnumerator PreloadCoreAssets()
        {
            // 使用AddressableManager加载PreLoad组资源（图集作为依赖会自动加载）
            yield return AddressableManager.Instance.LoadAssetsByLabelCoroutine<GameObject>(
                "PreLoad",
                objects => Debug.Log($"加载了{objects.Count}个核心预制体"),
                error => Debug.LogError(error),
                progress => UpdateRealProgress(0.15f + progress * 0.45f)); // 0.15-0.60 (权重: 0.45)
        }

        private IEnumerator PreloadMachineAssets()
        {
            // 使用AddressableManager加载机器资源
            yield return AddressableManager.Instance.LoadAssetsByLabelCoroutine<GameObject>(
                "Machines",
                machines => Debug.Log($"加载了{machines.Count}个机器资源"),
                error => Debug.LogError(error),
                progress => UpdateRealProgress(0.60f + progress * 0.20f)); // 0.60-0.80 (权重: 0.20)
        }
        #endregion

        #region 机器配置加载
        private IEnumerator PreLoadMachineConfig()
        {
            float baseProgress = 0.80f;
            // 获取基础配置
            config = BaseGameConsole.ActiveGameConsole().SlotMachineConfig(SlotName);
            UpdateRealProgress(baseProgress + 0.05f); // 0.85
            yield return null;

            if (config != null)
            {
                // 解析配置字典
                try
                {
                    config.ParseDict();
                    UpdateRealProgress(baseProgress + 0.10f); // 0.90
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
                        progress => UpdateRealProgress(baseProgress + 0.10f + progress * 0.05f) // 0.90-0.95
                    ));
                }
            }

            UpdateRealProgress(0.95f); // 0.80 + 0.15
        }
        #endregion

        #region 场景加载
        private IEnumerator LoadMainScene()
        {
            // 使用AddressableManager加载场景
            yield return AddressableManager.Instance.LoadSceneCor("WesternTreasure.unity", LoadSceneMode.Single,
                onLoaded:(result) => Debug.Log("主场景加载完成"),
                onFailed:error => Debug.LogError(error),
                onProgress:progress => UpdateRealProgress(0.95f + progress * 0.05f)); // 0.95-1.00
        }
        #endregion

        #region 进度控制核心方法
        
        // 更新实际加载进度
        private void UpdateRealProgress(float progress)
        {
            lastRealProgress = Mathf.Clamp01(progress);
            targetProgress = lastRealProgress;
            stuckTimer = 0f; // 重置卡顿计时器
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
