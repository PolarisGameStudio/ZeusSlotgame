using UnityEngine;
using UnityEngine.UI;
using System.SliderMultiplier;
using TMPro;
using Activity;
namespace System.SliderMultiplier
{
    /// <summary>
    /// 滑块倍率UI组件
    /// 挂载在弹窗的Prefab上，控制滑块的移动和倍率计算
    /// </summary>
    public class SliderMultiplierComponent : MonoBehaviour
    {
        #region UI引用

        [Header("UI References")]
        [SerializeField] private Image backgroundImage;      // 背景图
        [SerializeField] private Image sliderImage;          // 滑块图
        [SerializeField] private RectTransform sliderTransform;  // 滑块 RectTransform

        [Header("Optional: Multiplier Text Display")]
        [SerializeField] private TextMeshProUGUI[] multiplierTexts;     // 可选：5个倍率文本显示

        #endregion

        #region 私有变量

        // 移动范围（根据背景图尺寸计算）
        private float leftBoundary;
        private float rightBoundary;
        private float centerPosition;  // 中心位置（起始位置）

        // 滑块移动速度（从 Manager 获取）
        private float sliderSpeed;

        // 当前倍率配置
        private int[] currentConfig;

        // 滑块是否正在移动
        private bool isMoving;

        // 移动方向（1：向右，-1：向左）
        private int direction = 1;

        // 是否已初始化
        private bool isInitialized = false;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 确保引用正确
            if (sliderTransform == null && sliderImage != null)
            {
                sliderTransform = sliderImage.rectTransform;
            }
        }

        private void Update()
        {
            if (isInitialized && isMoving)
            {
                UpdateSliderPosition();
            }
        }

        private void OnDestroy()
        {
            isMoving = false;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化组件
        /// </summary>
        public void OnInit()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[SliderMultiplierComponent] Already initialized");
                return;
            }

            Debug.Log("[SliderMultiplierComponent] OnInit start");

            // 检查 Manager 是否已初始化
            if (!SliderMultiplierManager.Instance.IsInitialized())
            {
                Debug.LogError("[SliderMultiplierComponent] SliderMultiplierManager not initialized!");
                return;
            }

            // 1. 验证UI引用
            if (!ValidateReferences())
            {
                Debug.LogError("[SliderMultiplierComponent] UI references validation failed!");
                return;
            }

            // 2. 刷新配置（检查提现状态）并从 Manager 获取配置
            SliderMultiplierManager.Instance.RefreshConfig();
            currentConfig = SliderMultiplierManager.Instance.GetCurrentMultiplierConfig();
            sliderSpeed = SliderMultiplierManager.Instance.GetSliderSpeed();

            Debug.Log($"[SliderMultiplierComponent] Config: [{currentConfig[0]}, {currentConfig[1]}, {currentConfig[2]}, {currentConfig[3]}, {currentConfig[4]}]");
            Debug.Log($"[SliderMultiplierComponent] Speed: {sliderSpeed}");

            // 3. 计算移动边界（基于背景图尺寸）
            CalculateBoundaries();

            // 4. 更新倍率文本显示（如果有）
            UpdateMultiplierTexts();

            // 5. 重置滑块到中心位置
            ResetSliderToCenter();

            // 6. 设置初始化标志
            isInitialized = true;

            // 7. 开始移动
            StartSliderMovement();

            Debug.Log("[SliderMultiplierComponent] OnInit complete");
        }

        /// <summary>
        /// 验证UI引用
        /// </summary>
        private bool ValidateReferences()
        {
            if (backgroundImage == null)
            {
                Debug.LogError("[SliderMultiplierComponent] backgroundImage is null!");
                return false;
            }

            if (sliderImage == null)
            {
                Debug.LogError("[SliderMultiplierComponent] sliderImage is null!");
                return false;
            }

            if (sliderTransform == null)
            {
                Debug.LogError("[SliderMultiplierComponent] sliderTransform is null!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 计算移动边界
        /// </summary>
        private void CalculateBoundaries()
        {
            // 获取背景图的宽度
            RectTransform bgRect = backgroundImage.rectTransform;
            float bgWidth = bgRect.rect.width;

            // 假设背景图居中对齐，计算左右边界
            // 给滑块留一点边距，避免完全贴边
            float margin = sliderImage.rectTransform.rect.width / 2;
            leftBoundary = -bgWidth / 2 + margin;
            rightBoundary = bgWidth / 2 - margin;

            // 中心位置（起始位置）
            centerPosition = 0f;

            Debug.Log($"[SliderMultiplierComponent] Boundaries: Left={leftBoundary}, Right={rightBoundary}, Center={centerPosition}");
        }

        /// <summary>
        /// 更新倍率文本显示
        /// </summary>
        private void UpdateMultiplierTexts()
        {
            if (multiplierTexts == null || multiplierTexts.Length != 5)
            {
                Debug.LogWarning("[SliderMultiplierComponent] multiplierTexts not configured or invalid length");
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                if (multiplierTexts[i] != null)
                {
                    multiplierTexts[i].text = $"x{currentConfig[i]}";
                }
            }

            Debug.Log("[SliderMultiplierComponent] Multiplier texts updated");
        }

        #endregion

        #region 滑块移动

        /// <summary>
        /// 重置滑块到中心位置（仅限定X轴，Y轴保持不变）
        /// </summary>
        private void ResetSliderToCenter()
        {
            Vector2 pos = sliderTransform.anchoredPosition;
            pos.x = centerPosition;  // 只设置X轴到中心位置
            // Y轴保持原有位置，不做修改
            sliderTransform.anchoredPosition = pos;

            // 初始方向：向右
            direction = 1;

            Debug.Log($"[SliderMultiplierComponent] Slider reset to center position: X={pos.x}, Y={pos.y}");
        }

        /// <summary>
        /// 开始滑块移动（从中心位置开始）
        /// </summary>
        public void StartSliderMovement()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[SliderMultiplierComponent] Not initialized, cannot start movement");
                return;
            }

            isMoving = true;
            Debug.Log("[SliderMultiplierComponent] Slider movement started");
        }

        /// <summary>
        /// 更新滑块位置（在Update中调用）
        /// </summary>
        private void UpdateSliderPosition()
        {
            if (!isMoving) return;

            // 根据方向移动
            float movement = sliderSpeed * direction * Time.deltaTime;
            Vector2 pos = sliderTransform.anchoredPosition;
            pos.x += movement;

            // 边界检测并反转方向
            if (pos.x >= rightBoundary)
            {
                pos.x = rightBoundary;
                direction = -1;  // 向左
            }
            else if (pos.x <= leftBoundary)
            {
                pos.x = leftBoundary;
                direction = 1;   // 向右
            }

            sliderTransform.anchoredPosition = pos;
        }

        #endregion

        #region 倍率计算

        /// <summary>
        /// 停止滑块并获取当前倍率
        /// </summary>
        public int StopAndGetMultiplier()
        {
            if (!isInitialized)
            {
                Debug.LogError("[SliderMultiplierComponent] Not initialized, cannot stop slider");
                return 1;
            }

            isMoving = false;
            int multiplier = CalculateMultiplierByPosition();

            Debug.Log($"[SliderMultiplierComponent] Slider stopped at position {sliderTransform.anchoredPosition.x}, multiplier: {multiplier}");

            return multiplier;
        }

        /// <summary>
        /// 根据滑块位置计算对应的倍率
        /// </summary>
        private int CalculateMultiplierByPosition()
        {
            // 获取滑块当前 X 坐标
            float currentX = sliderTransform.anchoredPosition.x;

            // 计算总宽度
            float totalWidth = rightBoundary - leftBoundary;

            // 计算每个区域的宽度（5个区域）
            float sectionWidth = totalWidth / 5;

            // 计算当前在第几个区域（0-4）
            int sectionIndex = Mathf.Clamp(
                Mathf.FloorToInt((currentX - leftBoundary) / sectionWidth),
                0,
                4
            );

            // 返回对应区域的倍率
            int multiplier = currentConfig[sectionIndex];

            Debug.Log($"[SliderMultiplierComponent] Position={currentX:F2}, Section={sectionIndex}, Multiplier={multiplier}");

            return multiplier;
        }

        /// <summary>
        /// 计算最终倍率（累加 ChangeADMultipleBuff 的倍率）
        /// </summary>
        public int CalculateFinalMultiplier()
        {
            // 1. 获取滑块倍率
            int sliderMultiplier = StopAndGetMultiplier();
            int totalMultiplier = sliderMultiplier;

            // 2. 尝试获取 RotationBuffActivity 中激活的 ChangeADMultipleBuff 倍率
            try
            {
                // 获取 RotationBuffActivity 实例
                RotationBuffActivity rotationActivity = global::Activity.ActivityManager.Instance
                    .GetActivityByType(global::Activity.ActivityType.RotationBuff) as RotationBuffActivity;

                if (rotationActivity != null && rotationActivity.isInitialized)
                {
                    Debug.Log("[SliderMultiplierComponent] RotationBuffActivity found and initialized");

                    // 遍历 buffList，查找激活的 ChangeADMultipleBuff
                    foreach (int buffId in rotationActivity.buffList)
                    {
                        BuffSystem.BaseBuff buff = BuffSystem.BuffManager.Instance.GetBuffById(buffId);

                        // 检查是否是 ChangeADMultipleBuff 且已激活
                        if (buff != null &&
                            buff.buffType == BuffSystem.BuffConstant.ChangeADMultipleBuff &&
                            buff.isActive)
                        {
                            BuffSystem.ChangeADMultipleBuff adMultipleBuff = buff as BuffSystem.ChangeADMultipleBuff;
                            int adMultiple = adMultipleBuff.GetAdMultiple();

                            // 累加倍率
                            totalMultiplier += adMultiple-2;

                            Debug.Log($"[SliderMultiplierComponent] Added ChangeADMultipleBuff: {adMultiple}, Total: {totalMultiplier}");
                            break;  // 只累加一个激活的 Buff
                        }
                    }
                }
                else
                {
                    Debug.Log("[SliderMultiplierComponent] RotationBuffActivity not found or not initialized");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SliderMultiplierComponent] Error getting ChangeADMultipleBuff: {e.Message}");
            }

            Debug.Log($"[SliderMultiplierComponent] Final multiplier: Slider={sliderMultiplier}, Total={totalMultiplier}");

            return totalMultiplier;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 重新开始（用于再次打开弹窗时）
        /// </summary>
        public void Restart()
        {
            Debug.Log("[SliderMultiplierComponent] Restart");

            // 重新获取配置（可能已经推进到下一个）
            currentConfig = SliderMultiplierManager.Instance.GetCurrentMultiplierConfig();

            // 更新倍率文本
            UpdateMultiplierTexts();

            // 重置到中心位置
            ResetSliderToCenter();

            // 开始移动
            StartSliderMovement();
        }

        /// <summary>
        /// 暂停移动
        /// </summary>
        public void PauseMovement()
        {
            isMoving = false;
            Debug.Log("[SliderMultiplierComponent] Movement paused");
        }

        /// <summary>
        /// 恢复移动
        /// </summary>
        public void ResumeMovement()
        {
            if (isInitialized)
            {
                isMoving = true;
                Debug.Log("[SliderMultiplierComponent] Movement resumed");
            }
        }

        /// <summary>
        /// 获取当前是否正在移动
        /// </summary>
        public bool IsMoving()
        {
            return isMoving;
        }

        #endregion
    }
}
