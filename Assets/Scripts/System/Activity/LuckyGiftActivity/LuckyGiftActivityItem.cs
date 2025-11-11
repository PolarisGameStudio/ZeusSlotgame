using System;
using System.Collections;
using Ads;
using Classic;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;

namespace Activity
{
    public class LuckyGiftActivityItem : MonoBehaviour
    {
        // 所属icon
        private LuckyGiftActivityIcon icon;
        // 奖励数值
        private int reward;
        // 槽位索引
        private int slotIndex;
        // 创建动画时长
        private float scaleDuration;
        private float stayDuration;

        private RectTransform rectTransform;
        private ParticleSystem particleEffect;

        private Button button;
        private TextMeshProUGUI rewardText;
        
        // 动画及点击状态
        private bool isAnimating = false;
        private bool isClicked = false;
        private RectTransform bgGuang;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnItemClick);
            }
            particleEffect = Utilities.RealFindObj<ParticleSystem>(transform, "particleEffect");
            bgGuang = Utilities.RealFindObj<RectTransform>(transform, "bg_guang");
            // 查找rewardtext子物体
            rewardText = Utilities.RealFindObj<TextMeshProUGUI>(transform, "text_reward");
            if (rewardText == null)
            {
                rewardText = Utilities.RealFindObj<TextMeshProUGUI>(transform, "text_reward");
            }
        }

        /// <summary>
        /// 初始化item
        /// </summary>
        public void Initialize(LuckyGiftActivityIcon icon, int reward, int slotIndex, float scaleDuration, float stayDuration)
        {
            this.icon = icon;
            this.reward = reward;
            this.slotIndex = slotIndex;
            this.scaleDuration = scaleDuration;
            this.stayDuration = stayDuration;
            bgGuang.gameObject.SetActive(true);
            // 设置奖励文本
            UpdateRewardDisplay();

            StartCoroutine(PlayCreateAnimation());
        }
        
        /// <summary>
        /// 初始化item（无需动画，用于数据恢复）
        /// </summary>
        public void InitializeWithoutAnimation(LuckyGiftActivityIcon icon, int reward, int slotIndex)
        {
            this.icon = icon;
            this.reward = reward;
            this.slotIndex = slotIndex;
            bgGuang.gameObject.SetActive(false);
            UpdateRewardDisplay();
            isAnimating = false;
        }

        /// <summary>
        /// 播放创建动画：由小变大，停留后飞行到目标位置
        /// </summary>
        private IEnumerator PlayCreateAnimation()
        {
            isAnimating = true;
            transform.localScale = Vector3.zero;
            yield return transform.DOScale(2*Vector3.one, scaleDuration).SetUpdate(true).WaitForCompletion();
            yield return new WaitForSeconds(stayDuration);
			//使用dotween一边飞行到Vector3.zero，一边缩小至原比例Vector3.one（并行动画）
			Sequence flyAndShrink = DOTween.Sequence();
			flyAndShrink.Join(transform.DOScale(Vector3.one, 0.5f));
			flyAndShrink.Join(rectTransform.DOLocalMove(Vector3.zero, 0.5f));
			yield return flyAndShrink.SetUpdate(true).WaitForCompletion();

            if (particleEffect != null)
            {
                particleEffect.gameObject.SetActive(false);
            }
            icon.mask.gameObject.SetActive(false);
            bgGuang.gameObject.SetActive(false);
            isAnimating = false;
        }

        /// <summary>
        /// Item点击处理
        /// </summary>
        private void OnItemClick()
        {
            if (isAnimating || isClicked)
            {
                return;
            }
            
            if (icon == null || !icon.CanClickItem())
            {
                return;
            }
            
            isClicked = true;
            isAnimating = true; // 点击后也视为正在处理中
            
            // 通知icon处理点击
            icon.OnItemClicked(this);
        }

        /// <summary>
        /// 播放插屏广告
        /// </summary>
        public void PlayAd()
        {
            // 禁用按钮
            if (button != null)
            {
                button.interactable = false;
            }
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint, "Ad_LuckyGift");
            Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance, ADEntrances.Interstitial_Entrance_LUCKYGIFT_ACTIVITY);
        }

        /// <summary>
        /// 获取奖励值
        /// </summary>
        public int GetReward() => reward;

        /// <summary>
        /// 获取slot索引
        /// </summary>
        public int GetSlotIndex() => slotIndex;

        public void SetSlotIndex(int newIndex) => slotIndex = newIndex;

        private void OnDestroy()
        {
            // 停止所有动画
            if (transform != null)
            {
                transform.DOKill();
            }
        }

        private void UpdateRewardDisplay()
        {
            if (rewardText != null)
            {
                rewardText.text = OnLineEarningMgr.Instance.GetMoneyStr(reward, needIcon: false, needBigNum: true);
            }

            if (particleEffect != null)
            {
                particleEffect.gameObject.SetActive(false);
            }
        }
    }
}

