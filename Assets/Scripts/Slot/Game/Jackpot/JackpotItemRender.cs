using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using Libs;
namespace Classic{
    public class JackpotItemRender : MonoBehaviour
    {
        [FormerlySerializedAs("NeedUnlock")]
        public bool UseAnimationControl = true;
        protected readonly string ANI_LOCK = "Lock";
        protected readonly string ANI_UnLOCK = "Unlock";
        protected readonly string ANI_AWARD = "HitAward";
        protected readonly string ANI_Idle = "Idle";
        protected readonly string ANI_ACTIVE = "Active";
        protected readonly string ANI_Awarded = "Awarded";
        protected readonly string AUDIO_JACKPOT_ON = "Jackpot-on";
        protected readonly string AUDIO_JACKPOT_OFF = "Jackpot-off";
        //  public GameObject BackGround;
        public UIIncreaseNumber TxtAwardNum;
        public CommonNumberUpText jackPotCoinsText;
        public NumbertSlideIncrease numbertSlideIncreaseText;
        // 新增：单独用于显示 cash 的文本（网赚无限模式下）
        public TextMeshProUGUI cashText;
        public Animator m_Animator;

        private JackPotPrizePool PrizePool = null;
        private JackPotIncreaseMachineConfig IncreaseConfig = null;
        private SlotMachineConfig slotConfig;
        private bool m_IsUnlock = false;    //是否解锁状态
        private bool m_IsInHitAward = false; // 在循环的中奖状态


        public void SetPrizePoolData(SlotMachineConfig _slotConfig, JackPotPrizePool prizeInfo, JackPotIncreaseMachineConfig increaseConfig, bool needRefresh = false)
        {
            this.slotConfig = _slotConfig;
            this.PrizePool = prizeInfo;
            this.IncreaseConfig = increaseConfig;
            if (needRefresh)
            {
                this.Refresh();
            }
        }

        public void Refresh(bool isBetChange = false)
        {
            if (this.PrizePool == null||BaseSlotMachineController.Instance==null)
            {
                return;
            }

            bool isInfiniteMode = OnLineEarningMgr.Instance != null &&
                                  OnLineEarningMgr.Instance.isInfiniteOpen();

            // 金币文本：在无限模式下整体隐藏，只由 cashText 显示现金
            if (jackPotCoinsText != null)
            {
                jackPotCoinsText.gameObject.SetActive(!isInfiniteMode);
            }

            // 数值增长组件：在无限模式下隐藏文本展示，仅保留逻辑需要时可继续驱动内部数值
            if (TxtAwardNum != null)
            {
                TxtAwardNum.gameObject.SetActive(!isInfiniteMode);
            }

            // 更新 cash 文本显示（仅无限模式下）
            if (cashText != null)
            {
                if (isInfiniteMode && PrizePool != null && PrizePool.Cash > 0)
                {
                    cashText.gameObject.SetActive(true);
                    // 使用 OnLineEarningMgr 的格式化函数显示现金（带图标 & 大数处理）
                    string cashStr = OnLineEarningMgr.Instance.GetMoneyStr(PrizePool.Cash, 2, false, true);
                    cashText.text = cashStr;
                }
                else
                {
                    cashText.gameObject.SetActive(false);
                }
            }

            long increaseNum = IncreaseConfig != null ? IncreaseConfig.IncreaseEverySeconds : 0;
            if (this.PrizePool.MinBet <= BaseSlotMachineController.Instance.currentBetting)
            {
                // 金币数值组件：仅在非无限模式下更新
                if (this.TxtAwardNum!=null && !isInfiniteMode)
                {
                    this.TxtAwardNum.SetNumber(PrizePool.GetTotalAward(), increaseNum);
                }
                if (this.jackPotCoinsText!=null)
                {
                    if(this.jackPotCoinsText.gameObject.activeSelf)
                    {
                        jackPotCoinsText.SetAutoNumberUpText(PrizePool.GetUnlockShowAward(),(int)increaseNum);
                    }
                }

                if(numbertSlideIncreaseText != null)
                {
                    if(numbertSlideIncreaseText.gameObject.activeSelf)
                    {
                        numbertSlideIncreaseText.SetText(PrizePool.GetTotalAward());
                    }
                }

                if (!m_IsUnlock)
                {
                    if (UseAnimationControl) this.m_Animator.SetTrigger(ANI_UnLOCK);
                    m_IsUnlock = true;
                    if (isBetChange)
                    {
                        Libs.AudioEntity.Instance.PlayEffect(AUDIO_JACKPOT_ON);
                    }
                }
            }
            else
            {
                // 未解锁时：金币数值组件仅在非无限模式下更新
                if (this.TxtAwardNum!=null && !isInfiniteMode)
                {
                    // 未解锁时同样按模式切换显示内容
                    TxtAwardNum.SetNumber(PrizePool.GetUnlockShowAward(), increaseNum);
                }
                if (this.jackPotCoinsText!=null)
                {
                    if(jackPotCoinsText.gameObject.activeSelf)
                    {
                        jackPotCoinsText.SetAutoNumberUpText(PrizePool.GetUnlockShowAward(),(int)increaseNum);
                    }
                }

                if(numbertSlideIncreaseText != null)
                {
                    if(numbertSlideIncreaseText.gameObject.activeSelf)
                    {
                        numbertSlideIncreaseText.SetText(PrizePool.GetUnlockShowAward());
                    }
                }

                if (m_IsUnlock)
                {
                    if (UseAnimationControl) this.m_Animator.SetTrigger(ANI_LOCK);
                    Messenger.Broadcast<int>(JackPotConstants.HEAD_TIPS_SHOW, this.PrizePool.JackPotIndex);
                    m_IsUnlock = false;
                    if (isBetChange)
                    {
                        Libs.AudioEntity.Instance.PlayEffect(AUDIO_JACKPOT_OFF);
                    }
                }
            }

        }

		private void callBack(SlotMachineConfig _slotConfig, int jackPotIndex, double awardValue)
        {
            if (_slotConfig == null || this.slotConfig == null || this.PrizePool == null)
            {
                return;
            }

            if (_slotConfig.Name().Equals(this.slotConfig.Name()))
            {
                if (jackPotIndex == this.PrizePool.JackPotIndex)
                {
                    Refresh();
                }
            }
        }

        private void hitAward(int jackpotIndex)
        {
            if (PrizePool != null)
            {
                if (this.PrizePool.MinBet <= BaseSlotMachineController.Instance.currentBetting)
                {
                    if (jackpotIndex == this.PrizePool.JackPotIndex)
                    {
                        this.m_IsInHitAward = true;
                        if(UseAnimationControl) this.m_Animator.SetTrigger(ANI_AWARD);
                    }
                }
            }
        }

        private void resetNormal()
        {
            if (m_IsInHitAward)
            {
                if (UseAnimationControl) this.m_Animator.SetTrigger(ANI_Idle);
            }
        }

        void Awake()
        {
			Messenger.AddListener<SlotMachineConfig, int, double>(JackPotManager.JACKPOT_DATA_MACHINE_CHANGE, callBack);
            Messenger.AddListener<int>(JackPotConstants.SCATTER_HIT_AWARD, hitAward);
            Messenger.AddListener(JackPotConstants.RESET_NORMAL, resetNormal);
        }

        void OnDestroy()
        {
			Messenger.RemoveListener<SlotMachineConfig, int, double>(JackPotManager.JACKPOT_DATA_MACHINE_CHANGE, callBack);
            Messenger.RemoveListener<int>(JackPotConstants.SCATTER_HIT_AWARD, hitAward);
            Messenger.RemoveListener(JackPotConstants.RESET_NORMAL, resetNormal);
        }
    }
}

