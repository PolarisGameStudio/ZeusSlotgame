using UnityEngine;
using UnityEngine.UI;
using System.SliderMultiplier;

namespace System.SliderMultiplier
{
    /// <summary>
    /// 示例：如何在弹窗中集成滑块倍率系统
    /// 将此脚本作为参考，集成到您的实际弹窗中
    /// </summary>
    public class SliderMultiplierDialogExample : MonoBehaviour
    {
        [Header("Slider Multiplier")]
        [SerializeField] private SliderMultiplierComponent sliderMultiplierComponent;

        [Header("Buttons")]
        [SerializeField] private Button claimButton;  // 领奖按钮
        [SerializeField] private Button closeButton;  // 关闭按钮

        [Header("Reward Settings")]
        [SerializeField] private int baseReward = 1000;  // 基础奖励

        private void Start()
        {
            // 绑定按钮事件
            if (claimButton != null)
            {
                claimButton.onClick.AddListener(OnClaimButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }
        }

        private void OnEnable()
        {
            // 弹窗打开时初始化滑块组件
            if (sliderMultiplierComponent != null)
            {
                sliderMultiplierComponent.OnInit();
            }
        }

        /// <summary>
        /// 点击领奖按钮
        /// </summary>
        private void OnClaimButtonClicked()
        {
            Debug.Log("[SliderMultiplierDialogExample] Claim button clicked");

            if (sliderMultiplierComponent == null)
            {
                Debug.LogError("[SliderMultiplierDialogExample] sliderMultiplierComponent is null!");
                return;
            }

            // 1. 停止滑块并计算最终倍率
            int finalMultiplier = sliderMultiplierComponent.CalculateFinalMultiplier();

            // 2. 计算最终奖励
            int finalReward = baseReward * finalMultiplier;

            Debug.Log($"[SliderMultiplierDialogExample] Base Reward: {baseReward}, Multiplier: {finalMultiplier}, Final Reward: {finalReward}");

            // 3. 发放奖励（这里需要调用您的奖励系统）
            GiveReward(finalReward);

            // 4. 推进配置索引
            SliderMultiplierManager.Instance.AdvanceToNextConfig();

            // 5. 关闭弹窗
            CloseDialog();
        }

        /// <summary>
        /// 点击关闭按钮
        /// </summary>
        private void OnCloseButtonClicked()
        {
            Debug.Log("[SliderMultiplierDialogExample] Close button clicked");
            CloseDialog();
        }

        /// <summary>
        /// 发放奖励（示例）
        /// </summary>
        private void GiveReward(int reward)
        {
            // TODO: 在这里调用您的奖励系统
            // 例如：OnLineEarningMgr.Instance.AddCash(reward);
            Debug.Log($"[SliderMultiplierDialogExample] Give reward: {reward} coins");
        }

        /// <summary>
        /// 关闭弹窗
        /// </summary>
        private void CloseDialog()
        {
            // TODO: 在这里实现关闭弹窗的逻辑
            gameObject.SetActive(false);
            Debug.Log("[SliderMultiplierDialogExample] Dialog closed");
        }

        private void OnDestroy()
        {
            // 清理按钮事件
            if (claimButton != null)
            {
                claimButton.onClick.RemoveListener(OnClaimButtonClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }
    }
}
