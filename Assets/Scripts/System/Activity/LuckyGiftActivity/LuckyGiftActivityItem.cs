using System.Collections;
using Ads;
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
        // 槽位索引（当前在 Icon 上的位置：0, 1, 2）
        private int slotIndex;
        // 玩家历史上获得的第几个 item（从 1 开始）
        private int totalItemCount;
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
        private Image bgAD;
        private Image biankuanguang;
        // 背景切换相关
        private GameObject bgInfinite;
        private GameObject bg300;
        private GameObject bgInterval;
        // Animator组件（状态机）
        private Animator animator;
        private const string BIANKUANG_TRIGGER = "biankuang"; // 切换到biankuang动画的trigger名称
        
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
            bgAD = Utilities.RealFindObj<Image>(transform, "imagead");
            biankuanguang = Utilities.RealFindObj<Image>(transform, "biankuangguang");

            // 查找背景对象
            Transform bgInfiniteTransform = Utilities.RealFindObj<Transform>(transform, "bg_infinite");
            bgInfinite = bgInfiniteTransform != null ? bgInfiniteTransform.gameObject : null;

            Transform bg300Transform = Utilities.RealFindObj<Transform>(transform, "bg_300");
            bg300 = bg300Transform != null ? bg300Transform.gameObject : null;

            Transform bgIntervalTransform = Utilities.RealFindObj<Transform>(transform, "bg_interval");
            bgInterval = bgIntervalTransform != null ? bgIntervalTransform.gameObject : null;

            // 查找rewardtext子物体
            rewardText = Utilities.RealFindObj<TextMeshProUGUI>(transform, "text_reward");
            if (rewardText == null)
            {
                rewardText = Utilities.RealFindObj<TextMeshProUGUI>(transform, "text_reward");
            }

            // 初始化时先隐藏rewardText，等Initialize调用UpdateRewardDisplay时再根据模式显示
            if (rewardText != null)
            {
                rewardText.gameObject.SetActive(false);
            }

            // 获取Animator组件
            animator = GetComponent<Animator>();
            if (animator != null)
            {
                // Entry会自动播放Rotate动画，初始禁用Animator
                animator.enabled = false;
            }
        }

        /// <summary>
        /// 初始化item
        /// </summary>
        /// <param name="icon">所属 Icon</param>
        /// <param name="reward">奖励数值</param>
        /// <param name="slotIndex">槽位索引（0, 1, 2）</param>
        /// <param name="totalItemCount">玩家历史上获得的第几个 item（从 1 开始）</param>
        /// <param name="scaleDuration">缩放动画时长</param>
        /// <param name="stayDuration">停留时长</param>
        /// <param name="freeCount">前几个历史 item 免费</param>
        public void Initialize(LuckyGiftActivityIcon icon, int reward, int slotIndex, int totalItemCount, float scaleDuration, float stayDuration, int freeCount)
        {
            this.icon = icon;
            this.reward = reward;
            this.slotIndex = slotIndex;
            this.totalItemCount = totalItemCount;
            this.scaleDuration = scaleDuration;
            this.stayDuration = stayDuration;
            bgGuang.gameObject.SetActive(true);

            // 启用Animator，Entry会自动播放Rotate动画
            if (animator != null)
            {
                animator.enabled = true;
            }

            // 设置奖励文本
            UpdateRewardDisplay();

            // 切换背景
            SwitchBackgroundByMode();

            // 前 freeCount 个历史 item（免费）不显示广告图片
            if (bgAD != null)
            {
                bgAD.gameObject.SetActive(totalItemCount > freeCount);
            }

            Debug.Log($"[LuckyGiftActivityItem] Initialize - slotIndex: {slotIndex}, totalItemCount: {totalItemCount}, freeCount: {freeCount}, showAd: {totalItemCount > freeCount}");

            StartCoroutine(PlayCreateAnimation());
        }
        
        /// <summary>
        /// 初始化item（无需动画，用于数据恢复）
        /// </summary>
        public void InitializeWithoutAnimation(LuckyGiftActivityIcon icon, int reward, int slotIndex, int totalItemCount, int freeCount)
        {
            this.icon = icon;
            this.reward = reward;
            this.slotIndex = slotIndex;
            this.totalItemCount = totalItemCount;
            bgGuang.gameObject.SetActive(false);
            UpdateRewardDisplay();
            SwitchBackgroundByMode();
            isAnimating = false;

            // 前 freeCount 个历史 item（免费）不显示广告图片
            if (bgAD != null)
            {
                bgAD.gameObject.SetActive(totalItemCount > freeCount);
            }

            // 无动画初始化时，直接启用Animator并切换到biankuang
            if (animator != null)
            {
                animator.enabled = true;
                SwitchToBiankuangAnimation();
            }

            bool isInfinite = OnLineEarningMgr.Instance.isInfiniteOpen();
            bool rewardTextActive = rewardText != null && rewardText.gameObject.activeSelf;
            Debug.Log($"[LuckyGiftActivityItem] InitializeWithoutAnimation - slotIndex: {slotIndex}, totalItemCount: {totalItemCount}, freeCount: {freeCount}, showAd: {totalItemCount > freeCount}, isInfinite: {isInfinite}, rewardTextActive: {rewardTextActive}");
        }

        /// <summary>
        /// 播放创建动画：由小变大，停留后飞行到目标位置
        /// </summary>
        private IEnumerator PlayCreateAnimation()
        {
            isAnimating = true;

            // Animator Entry会自动播放Rotate动画，无需手动触发

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

            // 检查是否需要显示 LuckyGift 引导（玩家历史上第一个 item）
            if (totalItemCount == 1)
            {
                // 第一个item不切换动画，禁用Animator停止所有动画
                StopAnimation();
                CheckAndShowLuckyGiftTutorial();
            }
            else
            {
                // 其他item从Rotate切换到biankuang
                SwitchToBiankuangAnimation();
            }
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

        /// <summary>
        /// 获取玩家历史上获得的第几个 item
        /// </summary>
        public int GetTotalItemCount() => totalItemCount;

        private void OnDestroy()
        {
            // 停止所有动画
            if (transform != null)
            {
                transform.DOKill();
            }

            // 禁用Animator
            StopAnimation();
        }

        private void UpdateRewardDisplay()
        {
            if (rewardText != null)
            {
                rewardText.text = OnLineEarningMgr.Instance.GetMoneyStr(reward, needIcon: false, needBigNum: true);

                // 无限模式显示，其他模式隐藏
                bool shouldShowRewardText = OnLineEarningMgr.Instance.isInfiniteOpen();
                rewardText.gameObject.SetActive(shouldShowRewardText);
            }

            if (particleEffect != null)
            {
                particleEffect.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 根据OnLineEarningMgr的模式切换背景
        /// </summary>
        private void SwitchBackgroundByMode()
        {
            bool isInfinite = OnLineEarningMgr.Instance.isInfiniteOpen();
            bool is300 = OnLineEarningMgr.Instance.isThreeHundredOpen();
            bool isInterval = OnLineEarningMgr.Instance.isIntervalDataPatternOpen();

            if (bgInfinite != null)
            {
                bgInfinite.SetActive(isInfinite);
            }

            if (bg300 != null)
            {
                bg300.SetActive(is300);
            }

            if (bgInterval != null)
            {
                bgInterval.SetActive(isInterval);
            }

            Debug.Log($"[LuckyGiftActivityItem] SwitchBackground - Infinite: {isInfinite}, 300: {is300}, Interval: {isInterval}");
        }

        /// <summary>
        /// 检查并显示 LuckyGift 引导
        /// </summary>
        private void CheckAndShowLuckyGiftTutorial()
        {
            // 检查是否需要显示 LuckyGift 引导
            if (!global::TutorialManager.ShouldShow(global::TutorialManager.TutorialStep.LuckyGift))
            {
                Debug.Log("[LuckyGiftActivityItem] LuckyGift 引导已完成或未启用，播放边框光动画");
                // 无法显示引导时，播放边框光动画
                SwitchToBiankuangAnimation();
                return;
            }

            Debug.Log("[LuckyGiftActivityItem] 开始显示 LuckyGift 引导");

            // 暂停 AutoSpin（通过消息机制）
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_SUSPEND);
            Debug.Log("[LuckyGiftActivityItem] 已广播 AUTO_SPIN_SUSPEND 消息");

            // 获取父节点
            Transform parentNode = GetTutorialParentNode();

            // 显示引导
            global::TutorialManager.Start(
                global::TutorialManager.TutorialStep.LuckyGift,
                parentNode,
                (step) => {
                    Debug.Log("[LuckyGiftActivityItem] LuckyGift 引导已完成");

                    // 恢复 AutoSpin（通过消息机制）
                    Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
                    Debug.Log("[LuckyGiftActivityItem] 已广播 AUTO_SPIN_RESUME 消息");

                    // 引导完成后添加呼吸动画提示用户点击
                    OnItemClick();
                }
            );
        }

        /// <summary>
        /// 获取 Tutorial 的父节点
        /// </summary>
        private Transform GetTutorialParentNode()
        {
            // 方案2：查找 BannerCanvas
            GameObject bannerCanvas = GameObject.Find("BannerCanvas");
            if (bannerCanvas != null)
            {
                Debug.Log("[LuckyGiftActivityItem] 使用 BannerCanvas");
                return bannerCanvas.transform;
            }

            // 方案3：使用 null（自动使用 DialogCanvas）
            Debug.Log("[LuckyGiftActivityItem] 使用默认节点");
            return null;
        }

        /// <summary>
        /// 切换到biankuang动画（飞行到指定位置后）
        /// </summary>
        private void SwitchToBiankuangAnimation()
        {
            if (animator == null)
            {
                Debug.LogWarning("[LuckyGiftActivityItem] Animator组件未找到");
                return;
            }

            // 显示biankuanguang Image
            if (biankuanguang != null)
            {
                biankuanguang.gameObject.SetActive(true);
            }

            // 触发biankuang trigger，从Any State切换到biankuang动画
            animator.SetTrigger(BIANKUANG_TRIGGER);
            Debug.Log($"[LuckyGiftActivityItem] 触发Trigger切换到biankuang动画: {BIANKUANG_TRIGGER}");
        }

        /// <summary>
        /// 停止所有动画
        /// </summary>
        private void StopAnimation()
        {
            if (animator != null)
            {
                animator.enabled = false;
                Debug.Log("[LuckyGiftActivityItem] 已禁用Animator，停止所有动画");
            }

            // 隐藏biankuanguang Image
            if (biankuanguang != null)
            {
                biankuanguang.gameObject.SetActive(false);
            }
        }
    }
}

