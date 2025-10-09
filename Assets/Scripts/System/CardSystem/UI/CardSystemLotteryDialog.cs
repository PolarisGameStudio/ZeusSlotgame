using System;
using System.Collections;
using System.Collections.Generic;
using Classic;
using DG.Tweening;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using Ads;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace CardSystem
{
    public enum SpinState
    {
        UnNormal,
        Normal,
        Spinning,
        Finished
    }

    public enum WheelItemState
    {
        UnSelected,
        Turn,
        Select
    }
    
    public class CardSystemLotteryDialog : UIDialog
    {
        public Text txtPercent;
        public Button btn_spin;
        public Button btn_cardPack;
        public TextMeshProUGUI cardPackCountText;
        private SpinState spinState = SpinState.Normal;
        private List<GameObject> wheelItems = new List<GameObject>();
        private List<int> config = new List<int>();
        public GameObject showHand;
        private BaseWeightCondition weightCondition;
        private int curPercent = 0;
        private int targetPercent = 0; // 假设总百分比为100
        //百分比文本缓动持续时间
        private float tweenDuration = 0.5f; 
        private Tween percentTween;
        private int targetPositon = 0;

        private GameObject tipInfo;
        public TextMeshProUGUI tmp_tip;
        public LocalizedString localizeString;
        public TextMeshProUGUI tmp_bottom;
        public LocalizedString bottomString;
        
        public Image giftProgress;
        public TextMeshProUGUI giftProgressTxt;
        public Button giftBtn;
        
        // 这里可以添加特定于CardSystemLuckyDrawDialog的逻辑
        protected override void Awake()
        {
            base.Awake();
            Transform content = Util.FindObject<Transform>(transform, "Anchor/content");
            for (int i = 0; i < 8; i++)
            {
                GameObject go = Util.FindObject<GameObject>(content, "" + i);
                wheelItems.Add(go);
            }

            if (btn_spin != null)
            {
                UGUIEventListener.Get(btn_spin.gameObject).onClick += OnClickSpinButton;
            }
            if (btn_cardPack != null)
            {
                UGUIEventListener.Get(btn_cardPack.gameObject).onClick += OnClickCardPackButton;
            }
            spinCurve = new AnimationCurve(
                new Keyframe(0, 0, 0, 2),          // 开始快速
                new Keyframe(slowDownRatio, 0.9f, 0, 0), // 减速点
                new Keyframe(1, 1, 0, 0)           // 缓慢停止
            );
            UpdateSelection(0);
            
            giftBtn.onClick.AddListener(GiftBtnOnClick);
            RefreshCardScoreProgress();
        }
        private void GiftBtnOnClick()
        {
            //throw new System.NotImplementedException();
            Messenger.Broadcast(GameDialogManager.OpenCardSystemGetGiftDialogMsg);
            CardSystemManager.Instance.ResetProgress();
            RefreshCardScoreProgress();
        }

        protected override void Start()
        {
            base.Start();
            // 在这里刷新抽奖对话框的内容
            config = CardSystemManager.Instance.GetLotteryUI();
            for (int i = 0; i < config.Count; i++)
            {
                GameObject go = wheelItems[i];
                ShowItem(config[i], go);
            }
            string cashInfo = OnLineEarningMgr.Instance.GetCashStr(CardSystemManager.Instance.GetCurCollectionCoins(),needIcon:false);
            bottomString.Arguments = new object[] { string.Format("<color=#FFFF00>{0}</color>",cashInfo) };
            tmp_bottom.text = bottomString.GetLocalizedString();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Messenger.AddListener(CardSystemConstants.RefreshLotteryMsg, Refresh);
            Messenger.AddListener<int>(ADConstants.PlayCardLotteryAD, PlayCardLotteryAD);
            Messenger.AddListener<int>(ADConstants.PlayCardLotteryADFailed, PlayCardLotteryAD);
            Messenger.AddListener(CardSystemConstants.RefreshCardScoreProgress, RefreshCardScoreProgress);
        }
        private void RefreshCardScoreProgress()
        {
            var childCount = CardSystemManager.Instance.currentScore;
            var parentCount = CardSystemManager.Instance.progressMaxScore;
            if (childCount >= parentCount)
            {
                giftProgress.fillAmount = 1;
                giftBtn.interactable = true;
            }
            else
            {
                giftProgress.fillAmount = (float)childCount / parentCount;
                giftBtn.interactable = false;
            }

            giftProgressTxt.text = $"{childCount}/{parentCount}";
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Messenger.RemoveListener(CardSystemConstants.RefreshLotteryMsg, Refresh);
            Messenger.RemoveListener<int>(ADConstants.PlayCardLotteryAD, PlayCardLotteryAD);
            Messenger.RemoveListener<int>(ADConstants.PlayCardLotteryADFailed, PlayCardLotteryAD);
            Messenger.RemoveListener(CardSystemConstants.RefreshCardScoreProgress, RefreshCardScoreProgress);
        }

        // 例如，处理抽奖逻辑、更新UI等
        public override void Refresh()
        {
            base.Refresh();
            try
            {
                UpdateCardPackCount();
                //动画状态重置
                for (int i = 0; i < wheelItems.Count; i++)
                {
                    SetItemAni(wheelItems[i]);
                }
                ShowWheelItemState(targetPositon,WheelItemState.Turn);
                weightCondition = CardSystemManager.Instance.GetCurrentWeightCondition();
                if (weightCondition == null)
                {
                    Debug.LogError("Current weight condition is null");
                    this.Close();
                    return;
                }
                targetPercent = weightCondition.GetWeightPercent();
                //非第一次转动
                if (spinState == SpinState.Finished)
                {
                    percentTween = Utils.Utilities.AnimationTo(this.curPercent, targetPercent, tweenDuration, UpdatePercentText, null, () =>
                    {
                        spinState = SpinState.Normal;
                        AudioEntity.Instance.StopRollingUpEffect();
                        UpdatePercentText(targetPercent);
                        curPercent = targetPercent;
                        percentTween = null;
                    });
                    percentTween.Play();
                }
                else
                {
                    //第一次转动
                    UpdatePercentText(targetPercent);
                }
                // 更新按钮状态
                ShowBtn();
            }
            catch (Exception e)
            {
                Debug.LogError("Error in CardSystemLotteryDialog.Refresh: " + e.Message);
                this.Close();
                throw;
            }
        }

        private void UpdateCardPackCount()
        {
            int HasCollectNum = CardSystemManager.Instance.GetHaveCardTypeCount();
            int TargetNum = CardSystemManager.Instance.GetTotalCardTypeCount();
            if (cardPackCountText != null)
            {
                cardPackCountText.text = $"{HasCollectNum}/{TargetNum}";
            }        
        }

        private void PlayCardLotteryAD(int type)
        {
            btn_spin.interactable = false; // 禁用按钮，防止重复点击
            OnDoSpin();
        }
        
        private void ShowItem(int i, GameObject go)
        {
            Transform img_cardpack = Util.FindObject<Transform>(go.transform, "img_cardpack");
            GameObject img_card = Util.FindObject<GameObject>(img_cardpack.transform, "" + i);
            img_card.SetActive(true);
        }

        private void SetItemAni(GameObject go,string trigger = "idle")
        {
            // 设置转轮项的动画
            Animator ani = go.GetComponent<Animator>();
            if (ani != null)
            {
                ani.SetTrigger(trigger);
            }
        }
        

        private void ShowBtn()
        {
            Transform img_normal = Util.FindObject<Transform>(btn_spin.transform, "img_normal");
            Transform img_unnormal = Util.FindObject<Transform>(btn_spin.transform, "img_unnormal");
            Transform img_freespin = Util.FindObject<Transform>(btn_spin.transform, "img_freespin");
            Transform state_ad = Util.FindObject<Transform>(btn_spin.transform, "state_ad");
            Transform state_costCoin = Util.FindObject<Transform>(btn_spin.transform, "state_costCoin");
            TextMeshProUGUI tmp_coinNum = Util.FindObject<TextMeshProUGUI>(state_costCoin, "tmp_coinNum");
            GameObject tip = Util.FindObject<GameObject>(transform, "Anchor/tip");
            // bool canShowHand = true;
            tip.gameObject.SetActive(false);
            switch (weightCondition.type)
            {
                case ConditionType.Free:
                    state_ad.gameObject.SetActive(false);
                    state_costCoin.gameObject.SetActive(false);
                    img_freespin.gameObject.SetActive(true);
                    break;
                case ConditionType.CostCoin:
                    CostCoinsCondition costCoinsCondition = weightCondition as CostCoinsCondition;
                    int cost = costCoinsCondition.Cost;
                    int limit = costCoinsCondition.Limit;
                    tmp_coinNum.text = Utils.Utilities.GetBigNumberShow(cost);
                    state_ad.gameObject.SetActive(false);
                    state_costCoin.gameObject.SetActive(true);
                    img_freespin.gameObject.SetActive(false);
                    // canShowHand = UserManager.GetInstance().UserProfile().Balance() > limit;
                    img_normal.gameObject.SetActive(UserManager.GetInstance().UserProfile().Balance() > limit);
                    img_unnormal.gameObject.SetActive(UserManager.GetInstance().UserProfile().Balance() < limit);
                    ShowNoCoinTip(limit);
                    tip.gameObject.SetActive(true);
                    break;
                case ConditionType.AD:
                    state_ad.gameObject.SetActive(true);
                    state_costCoin.gameObject.SetActive(false);
                    img_freespin.gameObject.SetActive(false);
                    break;
            }

            // if (canShowHand && CardSystemManager.Instance.isFirstShow)
            // {
            //     CardSystemManager.Instance.isFirstShow = false;
            //     showHand.gameObject.SetActive(true);
            // }
        }
        
        private void UpdatePercentText(int count)
        {
            txtPercent.text = count + "%";
        }
        
        private int cardIndex = -1; // 当前选中的卡牌索引
        private void OnClickSpinButton(GameObject go)
        {
            if (spinState != SpinState.Normal)
            {
                Debug.LogWarning("Already spinning, please wait.");
                return;
            }
            btn_spin.interactable = false; // 禁用按钮，防止重复点击
            // if (showHand.gameObject.activeSelf)
            // {
            //     showHand.gameObject.SetActive(false);
            // }
            switch (weightCondition.type)
            {
                case ConditionType.Free:
                    OnFreeSpin();
                    break;
                case ConditionType.CostCoin:
                    OnCostCoinSpin();
                    break;
                case ConditionType.AD:
                    OnADSpin();
                    break;
            }
        }

        private void OnClickCardPackButton(GameObject go)
        {
            this.Close();
            // 打开卡包界面
            CardSystemManager.Instance.ShowCollectionDialog();
        }
        
        void OnFreeSpin()
        {
            // 处理免费转动的逻辑
            Debug.Log("Free spin logic executed.");
            // 这里可以添加具体的免费转动逻辑
            OnDoSpin();
        }
        
        void OnCostCoinSpin()
        {
            // 处理消耗金币转动的逻辑
            Debug.Log("Cost coin spin logic executed.");
            // 这里可以添加具体的消耗金币转动逻辑
            try
            {
                CostCoinsCondition costCoinsCondition = weightCondition as CostCoinsCondition;
                if (UserManager.GetInstance().UserProfile().Balance()>costCoinsCondition.Limit)
                {
                    costCoinsCondition.Execute();
                    OnDoSpin();
                }
                else
                {
                    Debug.LogWarning("Not enough balance to spin.");
                    btn_spin.interactable = true; // 重新启用按钮
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                btn_spin.interactable = true;
                throw;
            }
        }
        
        void OnADSpin()
        {
            // 处理广告转动的逻辑
            Debug.Log("AD spin logic executed.");
            // 这里可以添加具体的广告转动逻辑
            // 例如，展示广告并在观看完成后调用 OnDoSpin()
            try
            {
                ADCondition adCondition = weightCondition as ADCondition;
                adCondition.Execute();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                btn_spin.interactable = true;
                throw;
            }
        }
        
        void OnDoSpin()
        {
            
            spinState = SpinState.Spinning;
            cardIndex = weightCondition.GetResultByWeight();
            int level = CardSystemManager.Instance.GetCardLevel(cardIndex);
            Debug.Log($"Spinning... Selected Card Index: {cardIndex}, Level: {level}");
            int targetPosition= GetResultByLevel(level);
            Debug.Log($"Target Position: {targetPosition}");
            if (targetPosition < 0 || targetPosition >= wheelItems.Count)
            {
                Debug.LogError("无效的目标位置: " + targetPosition);
                return;
            }
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint, "CardLottery");
            spinCoroutine = StartCoroutine(SpinToPosition(targetPosition));
        }
        
        //根据结果卡牌的星级确定ui配置的卡包序号
        private int GetResultByLevel(int level)
        {
            List<int> curLevelIndex = new List<int>();
            for (int i = 0; i < config.Count; i++)
            {
                if (config[i] == level)
                {
                    curLevelIndex.Add(i);
                }
            }

            int count = curLevelIndex.Count;
            Debug.Log($"GetResultByLevel: level:{level} count:{count}");
            int resultIndex = Random.Range(0, count);
            Debug.Log($"GetResultByLevel: resultIndex:{resultIndex}");
            return curLevelIndex[resultIndex];
        }

        #region 转轮

        public float spinDuration = 3f; // 旋转持续时间
        
        [Range(0.1f, 0.9f)] [Header("可调节减速阶段占总动画时长的比例（默认70%时间完成90%旋转，剩下30%时间完成最后10%")]
        public float slowDownRatio = 0.7f;
        [Header("动画曲线")]  
        public AnimationCurve spinCurve;
        private int currentSelection = 0;
        private Coroutine spinCoroutine;
        public int turnCycles = 5; // 转动圈数
        private bool isSpinning = false;

        // 修改后的位置角度定义（0在左上135度，顺时针排列）
        private readonly float[] positionAngles = new float[8]
        {
            135f,   // 0 - 左上 (起始位置)
            180f,   // 1 - 左
            225f,   // 2 - 左下
            270f,   // 3 - 下
            315f,   // 4 - 右下
            0f,     // 5 - 右
            45f,    // 6 - 右上
            90f     // 7 - 上
        };

        private IEnumerator SpinToPosition(int targetPosition)
        {
            isSpinning = true;
            float elapsedTime = 0f;
            float startAngle = positionAngles[currentSelection];

            // 计算需要旋转的角度（考虑最短路径）
            float targetAngle = positionAngles[targetPosition];
            float angleDifference = Mathf.DeltaAngle(startAngle, targetAngle);

            // 添加额外旋转圈数（3圈）
            float totalRotation = angleDifference + 360f * turnCycles;

            while (elapsedTime < spinDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / spinDuration;

                // 应用缓动曲线
                float easedProgress = spinCurve.Evaluate(progress);

                // 计算当前角度
                float currentAngle = startAngle + totalRotation * easedProgress;

                // 更新当前选择
                UpdateCurrentSelection(currentAngle);

                yield return null;
            }

            yield return GameConstants.TwoIn10SecondWait;
            ShowWheelItemState(targetPosition, WheelItemState.Select);
           
            yield return GameConstants.TwoIn10SecondWait;
            //转动完成的回调
            OnCompleteCor();
            isSpinning = false;
        }

        private void UpdateCurrentSelection(float currentAngle)
        {
            currentAngle = (currentAngle % 360 + 360) % 360;

            float minDiff = float.MaxValue;
            int closestPosition = 0;

            for (int i = 0; i < positionAngles.Length; i++)
            {
                float diff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, positionAngles[i]));
                if (diff < minDiff)
                {
                    minDiff = diff;
                    closestPosition = i;
                }
            }

            if (closestPosition != currentSelection)
            {
                UpdateSelection(closestPosition);
            }
        }

        private void UpdateSelection(int newSelection)
        {
            // 取消之前的选择状态
            if (currentSelection >= 0 && currentSelection < wheelItems.Count)
            {
                ShowWheelItemState(currentSelection,WheelItemState.UnSelected);
            }

            // 设置新的选择状态
            currentSelection = newSelection;
            if (currentSelection >= 0 && currentSelection < wheelItems.Count)
            {
                ShowWheelItemState(currentSelection,WheelItemState.Turn);
            }
        }

        private void ShowWheelItemState(int index,WheelItemState state)
        {
            if (index < 0 || index >= wheelItems.Count)
            {
                Debug.LogError("Index out of range: " + index);
                return;
            }
            GameObject go = wheelItems[index];
            Transform img_unSelect = Util.FindObject<Transform>(go.transform, "img_bg");
            Transform img_turn = Util.FindObject<Transform>(go.transform, "img_turn");
            Transform img_select = Util.FindObject<Transform>(go.transform, "img_select");
            img_unSelect.gameObject.SetActive(state == WheelItemState.UnSelected);
            img_turn.gameObject.SetActive(state == WheelItemState.Turn);
            img_select.gameObject.SetActive(state == WheelItemState.Select);
            if (state == WheelItemState.Select)
            {
                targetPositon = index; // 更新目标位置
                SetItemAni(go, "select");
            }
        }

        void OnCompleteCor()
        {
            // 停止转动
            if (spinCoroutine != null)
            {
                StopCoroutine(spinCoroutine);
                spinCoroutine = null;
            }
            // 更新状态为Finished,权重组自增
            CardSystemManager.Instance.AddWeightIndex();
            //展示获奖界面
            CardSystemManager.Instance.ShowGetRewardDialog(cardIndex,btn_cardPack.gameObject);
            spinState = SpinState.Finished;
            //打开获奖按钮
            btn_spin.interactable = true; // 重新启用按钮
        }
        #endregion


        protected override void BtnCloseClick(GameObject go)
        {
            if (spinState == SpinState.Spinning)
            {
                Debug.LogWarning("Cannot close dialog while spinning.");
                return;
            }
            base.BtnCloseClick(go);
        }

        public void ShowNoCoinTip(long limit)
        {
            localizeString.Arguments = new object[] { Utils.Utilities.GetBigNumberShow(limit)+"+" };
            tmp_tip.text = localizeString.GetLocalizedString();
        }
    }
}