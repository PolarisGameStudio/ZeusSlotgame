using System;
using System.Collections;
using System.Collections.Generic;
using Ads;
using Classic;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Activity
{
    public class FloatingRewardIcon : BaseIcon
    {
        private FloatingRewardActivity activity;
        private RectTransform rectTransform;
        private Canvas canvas;
        private Button button;
        private TextMeshProUGUI rewardText;
        private int currentReward = 0;
        
        // 移动状态
        private enum MoveState
        {
            Hidden,      // 隐藏状态
            MovingRightUp,  // 向右上方移动
            MovingLeftUp    // 向左上方移动
        }
        
        private MoveState currentMoveState = MoveState.Hidden;
        private float hideTimer = 0f;
        private Vector3 currentVelocity;
        private Coroutine hideCoroutine = null;
        
        // 屏幕边界（本地坐标）
        private float screenLeft, screenRight, screenTop, screenBottom;
        
        // 设计尺寸（竖屏：宽x高）
        private const float DESIGN_WIDTH = 1080f;
        private const float DESIGN_HEIGHT = 1920f;
        
        // 速度缩放比例
        private float speedScaleX = 1f;
        private float speedScaleY = 1f;
		// 缓存后的速度（适配屏幕尺寸）
		private float adjustedSpeedX = 0f;
		private float adjustedSpeedY = 0f;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnIconClick);
            }
            
            // 查找奖励文本组件（可能在子对象中，常见名称如 "txt_reward", "rewardText", "text" 等）
            rewardText = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "txt_reward");
            if (rewardText == null)
            {
                rewardText = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "rewardText");
            }
            
            // 获取Canvas
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas c in canvases)
                {
                    if (c.isRootCanvas)
                    {
                        canvas = c;
                        break;
                    }
                }
            }
        }

        public override void OnInit(int Id, Dictionary<string, object> data)
        {
            base.OnInit(Id, data);
            UpdateScreenBounds();
            // 初始状态为隐藏
            gameObject.SetActive(false);
        }

        public void SetActivity(FloatingRewardActivity act)
        {
			// 取消之前订阅
			if (activity != null)
			{
				activity.OnActivated -= OnActivityActivated;
			}
			activity = act;
			// 订阅新的激活事件
			if (activity != null)
			{
				activity.OnActivated += OnActivityActivated;
			}
            // 活动未激活时不展示，等待OnActivityActivated回调
            if (activity != null && activity.IsActivated)
            {
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }
                // 开始显示流程
                StartCoroutine(ShowIconCoroutine());
            }
        }

        /// <summary>
        /// 活动激活时回调，由FloatingRewardActivity触发
        /// </summary>
        public void OnActivityActivated()
        {
            if (activity == null)
            {
                return;
            }
            // 确保对象处于激活以便协程运行
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            // 重置移动状态，开始显示流程
            currentMoveState = MoveState.Hidden;
            currentVelocity = Vector3.zero;
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            StopAllCoroutines();
            StartCoroutine(ShowIconCoroutine());
        }

        private void UpdateScreenBounds()
        {
            if (canvas == null)
            {
                return;
            }
            
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                // 使用本地坐标的边界（RectTransform的rect属性）
                float halfWidth = canvasRect.rect.width * 0.5f;
                float halfHeight = canvasRect.rect.height * 0.5f;
                
                screenLeft = -halfWidth;
                screenRight = halfWidth;
                screenBottom = -halfHeight;
                screenTop = halfHeight;
                
                // 计算速度缩放比例（基于实际屏幕尺寸和设计尺寸的比例）
                float actualWidth = canvasRect.rect.width;
                float actualHeight = canvasRect.rect.height;
                
				// 计算相对于设计尺寸的缩放比例
                speedScaleX = actualWidth / DESIGN_WIDTH;
                speedScaleY = actualHeight / DESIGN_HEIGHT;
				// 根据当前活动参数缓存适配后的速度
				if (activity != null)
				{
					adjustedSpeedX = activity.SpeedX * speedScaleX;
					adjustedSpeedY = activity.SpeedY * speedScaleY;
				}

                // Debug.Log($"[FloatingRewardIcon] Screen bounds updated - Actual: {actualWidth}x{actualHeight}, Design: {DESIGN_WIDTH}x{DESIGN_HEIGHT}, SpeedScale: {speedScaleX}x{speedScaleY}");
            }
        }

        private void Update()
        {
            if (activity == null)
            {
                return;
            }

            // 更新屏幕边界（防止屏幕旋转等情况）
            if (Time.frameCount % 60 == 0)
            {
                UpdateScreenBounds();
            }

            switch (currentMoveState)
            {
                case MoveState.Hidden:
                    // 隐藏状态下，计时由协程处理，这里不需要做任何事情
                    break;

                case MoveState.MovingRightUp:
                case MoveState.MovingLeftUp:
                    // 移动状态
                    MoveIcon();
                    CheckBounds();
                    break;
            }
        }

        private void ShowIcon()
        {
            if (rectTransform == null || activity == null || canvas == null)
            {
                return;
            }

            UpdateScreenBounds();
            
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null)
            {
                return;
            }

            // 生成随机奖励值并显示
            currentReward = activity.GetRandomReward();
            UpdateRewardText();

            // 设置起始位置为左下方（使用本地坐标）
            // 左下角是 (0, 0) 在RectTransform中，但需要考虑pivot
            float iconWidth = rectTransform.rect.width;
            float iconHeight = rectTransform.rect.height;
            
            // 计算左下角位置（考虑pivot）
            float startX = -canvasRect.rect.width * 0.5f + iconWidth * 0.5f;
            float startY = -canvasRect.rect.height * 0.5f + iconHeight * 0.5f;
            
            rectTransform.localPosition = new Vector3(startX, startY, 0f);

			// 设置移动方向和速度（使用缓存后的速度）
			currentVelocity = new Vector3(adjustedSpeedX, adjustedSpeedY, 0f);
            currentMoveState = MoveState.MovingRightUp;
            gameObject.SetActive(true);
            
            // 启用按钮
            if (button != null)
            {
                button.interactable = true;
            }
            
			Debug.Log($"[FloatingRewardIcon] ShowIcon - Original Speed: {activity.SpeedX}x{activity.SpeedY}, Adjusted Speed: {adjustedSpeedX}x{adjustedSpeedY}");
            
            // 计算并输出预估时间
            // LogEstimatedTime();
        }

        private void UpdateRewardText()
        {
            if (rewardText != null)
            {
                rewardText.text = OnLineEarningMgr.Instance.GetMoneyStr(currentReward,needIcon:false, needBigNum:true);
            }
        }

        private void MoveIcon()
        {
            if (rectTransform == null)
            {
                return;
            }

            // 移动icon
            Vector3 moveDelta = currentVelocity * Time.deltaTime;
            rectTransform.localPosition += moveDelta;
        }

        private void CheckBounds()
        {
            if (rectTransform == null)
            {
                return;
            }

            // 使用本地坐标
            Vector3 localPos = rectTransform.localPosition;
            float iconWidth = rectTransform.rect.width;
            float iconHeight = rectTransform.rect.height;
            
            // 计算icon的边界（考虑pivot在中心）
            float iconLeft = localPos.x - iconWidth * 0.5f;
            float iconRight = localPos.x + iconWidth * 0.5f;
            float iconTop = localPos.y + iconHeight * 0.5f;
            float iconBottom = localPos.y - iconHeight * 0.5f;

            // 检查是否碰触屏幕边缘
            if (currentMoveState == MoveState.MovingRightUp)
            {
                // 向右上方移动时，检查是否碰触右边缘或上边缘
                if (iconRight >= screenRight || iconTop >= screenTop)
                {
                    // 改变方向为左上方
                    currentMoveState = MoveState.MovingLeftUp;
					currentVelocity = new Vector3(-adjustedSpeedX, adjustedSpeedY, 0f);
                }
            }
            else if (currentMoveState == MoveState.MovingLeftUp)
            {
                // 向左上方移动时，检查是否碰触左边缘
                if (iconLeft <= screenLeft)
                {
                    // 改变方向为右上方（但通常不会发生，因为已经在上方了）
                    currentMoveState = MoveState.MovingRightUp;
					currentVelocity = new Vector3(adjustedSpeedX, adjustedSpeedY, 0f);
                }
            }

            // 检查是否移出屏幕上方
            if (iconBottom > screenTop)
            {
                HideIcon();
            }
        }

        private void HideIcon()
        {
            currentMoveState = MoveState.Hidden;
            
            // 停止之前的隐藏协程（如果存在）
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            
            // 将icon移动到屏幕外很远的地方（隐藏但不SetActive(false)，以便协程可以继续运行）
            if (rectTransform != null && canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    // 移动到屏幕下方很远的地方
                    rectTransform.localPosition = new Vector3(0, -canvasRect.rect.height * 2, 0);
                }
            }
            
            // 隐藏游戏对象（但保持激活状态以便协程运行）
            // 注意：如果SetActive(false)，协程会停止，所以我们需要保持对象激活
            // 但为了视觉上隐藏，我们可以将对象移动到屏幕外
            // 实际上，将对象移动到屏幕外已经足够隐藏了，不需要SetActive(false)
            
            // 启动隐藏协程，等待ShowInterval秒后重新显示
            hideCoroutine = StartCoroutine(HideIconCoroutine());
        }

        private IEnumerator HideIconCoroutine()
        {
            // 等待ShowInterval秒
            yield return new WaitForSeconds(activity.ShowInterval);
            
            // 重新显示icon
            if (gameObject != null && activity != null && currentMoveState == MoveState.Hidden)
            {
                ShowIcon();
            }
        }

        private void OnIconClick()
        {
            // 立即停止移动
            StopMoving();
            
            // 点击时播放广告
            PlayFloatingRewardAd();
            // 不立即隐藏icon，等待广告播放结果
        }
        
        /// <summary>
        /// 停止icon移动
        /// </summary>
        private void StopMoving()
        {
            // 停止移动：设置状态为Hidden，速度归零
            currentMoveState = MoveState.Hidden;
            currentVelocity = Vector3.zero;
            
            // 禁用按钮，防止重复点击
            if (button != null)
            {
                button.interactable = false;
            }
            
            Debug.Log("[FloatingRewardIcon] Icon movement stopped, waiting for ad callback");
        }

        private void PlayFloatingRewardAd()
        {
            if (activity != null)
            {
                PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint, "Ad_FloatingReward");
                Messenger.Broadcast<string>(ADConstants.PlayAdByEntrance, ADEntrances.REWARD_VIDEO_FLOATING_REWARD);
            }
        }

        private void OnAdSuccess(int type)
        {
            // 广告播放成功，给玩家加钱
            if (type == 0 && currentReward > 0) // type == 0 表示激励视频
            {
                Action closeCallBack = () =>
                {
                    // 广告回调完成，隐藏icon
                    HideIcon();
                    if (!PlatformManager.Instance.IsWhiteBao())
                    {
                        Messenger.Broadcast(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL);
                    }
                };
                Messenger.Broadcast<int,Action>(GameDialogManager.OpenExtraAwardCashDialogMsg,currentReward,closeCallBack);
            }
        }

        private void OnAdFailed(int type)
        {
            // 广告播放失败，也给予奖励
            Debug.Log($"[FloatingRewardIcon] Ad failed, no reward");
            
            // 广告回调完成，隐藏icon
            Action closeCallBack = () =>
            {
                // 广告回调完成，隐藏icon
                HideIcon();
                if (!PlatformManager.Instance.IsWhiteBao())
                {
                    Messenger.Broadcast(GameConstants.SHOW_WITH_DRAW_TIPS_PANEL);
                }
            };
            Messenger.Broadcast<int,Action>(GameDialogManager.OpenExtraAwardCashDialogMsg,currentReward,closeCallBack);
        }

        private IEnumerator ShowIconCoroutine()
        {
            // 首次显示时，hideTimer应该为0，这样会立即显示
            hideTimer = 0f;
            
            // 延迟一小段时间后首次显示（确保所有初始化完成）
            yield return null;
            
            ShowIcon();
        }

        protected override void AddListener()
        {
            base.AddListener();
            Messenger.AddListener<int>(ADConstants.PlayFloatingRewardAD, OnAdSuccess);
            Messenger.AddListener<int>(ADConstants.PlayFloatingRewardADFailed, OnAdFailed);
        }

        protected override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener<int>(ADConstants.PlayFloatingRewardAD, OnAdSuccess);
            Messenger.RemoveListener<int>(ADConstants.PlayFloatingRewardADFailed, OnAdFailed);
        }

        /// <summary>
        /// 预估从起始位置移动到屏幕顶部所需的时间（秒）
        /// </summary>
        public float EstimateTimeToTop()
        {
            if (activity == null || canvas == null)
            {
                return 0f;
            }

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null || rectTransform == null)
            {
                return 0f;
            }

            // 获取实际尺寸
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;
            float iconWidth = rectTransform.rect.width;
            float iconHeight = rectTransform.rect.height;

            // 计算缩放后的速度
            float scaleX = canvasWidth / DESIGN_WIDTH;
            float scaleY = canvasHeight / DESIGN_HEIGHT;
            float adjustedSpeedX = activity.SpeedX * scaleX;
            float adjustedSpeedY = activity.SpeedY * scaleY;

            // 起始位置（左下角，icon中心的位置）
            float startX = -canvasWidth * 0.5f + iconWidth * 0.5f;
            float startY = -canvasHeight * 0.5f + iconHeight * 0.5f;

            // 屏幕边界
            float screenRight = canvasWidth * 0.5f;
            float screenTop = canvasHeight * 0.5f;

            float totalTime = 0f;
            float currentX = startX;
            float currentY = startY;

            // 第一阶段：向右上方移动，直到碰到右边缘或上边缘
            float iconRight = currentX + iconWidth * 0.5f;
            float iconTop = currentY + iconHeight * 0.5f;
            
            // 计算到右边缘和上边缘的距离
            float distToRight = screenRight - iconRight;
            float distToTop = screenTop - iconTop;
            
            // 计算到达边界的时间
            float timeToRight = distToRight / adjustedSpeedX;
            float timeToTop = distToTop / adjustedSpeedY;

            if (timeToRight <= timeToTop && timeToRight > 0)
            {
                // 先碰到右边缘
                totalTime += timeToRight;
                currentX = screenRight - iconWidth * 0.5f;
                currentY += adjustedSpeedY * timeToRight;
                
                // 第二阶段：向左上方移动，直到超出屏幕顶部
                float iconBottom = currentY - iconHeight * 0.5f;
                float remainingDistToTop = screenTop - iconBottom;
                
                if (remainingDistToTop > 0)
                {
                    float timeToExit = remainingDistToTop / adjustedSpeedY;
                    totalTime += timeToExit;
                }
            }
            else if (timeToTop > 0)
            {
                // 先碰到上边缘（或同时碰到）
                totalTime += timeToTop;
                currentX += adjustedSpeedX * timeToTop;
                
                // 如果此时还没超出顶部，继续向左上方移动
                float iconBottom = currentY + adjustedSpeedY * timeToTop - iconHeight * 0.5f;
                float remainingDistToTop = screenTop - iconBottom;
                
                if (remainingDistToTop > 0)
                {
                    // 向左上方移动直到超出
                    float timeToExit = remainingDistToTop / adjustedSpeedY;
                    totalTime += timeToExit;
                }
            }

            return totalTime;
        }

        /// <summary>
        /// 在ShowIcon时计算并输出预估时间
        /// </summary>
        private void LogEstimatedTime()
        {
            float estimatedTime = EstimateTimeToTop();
            Debug.Log($"[FloatingRewardIcon] Estimated time from bottom to top: {estimatedTime:F2} seconds");
        }

        public override void OnDestroy()
        {
			if (activity != null)
			{
				activity.OnActivated -= OnActivityActivated;
			}
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            StopAllCoroutines();
            base.OnDestroy();
        }
    }
}



