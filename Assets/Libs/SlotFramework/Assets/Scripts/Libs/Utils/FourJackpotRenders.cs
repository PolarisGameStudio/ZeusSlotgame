using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourJackpotRenders : MonoBehaviour
{
    [SerializeField]
    private UIIncreaseNumber m_GrandTxt;
    [SerializeField]
    private UIIncreaseNumber m_MajorTxt;
    [SerializeField]
    private UIIncreaseNumber m_MinorTxt;
    [SerializeField]
    private UIIncreaseNumber m_MiniTxt;

    private JackPotPrizePool m_GrandJackpot;
    private JackPotPrizePool m_MajorJackpot;
    private JackPotPrizePool m_MinorJackpot;
    private JackPotPrizePool m_MiniJackpot;

    private SlotMachineConfig m_slotConfig;

    private void Awake()
    {
        Messenger.AddListener<SlotControllerConstants.JACKPOT_TYPE>(SlotControllerConstants.REFRESH_JACKPOT_UI_TYPE,RefreshJpUi);
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener<SlotControllerConstants.JACKPOT_TYPE>(SlotControllerConstants.REFRESH_JACKPOT_UI_TYPE,RefreshJpUi);
    }

    public void InitJackpotData(SlotMachineConfig slotConfig)
    {
        m_slotConfig = slotConfig;

        m_GrandJackpot = this.m_slotConfig.GetJackPotPool(SlotControllerConstants.JACKPOT_TYPE.GRAND.ToString());
        m_MajorJackpot = this.m_slotConfig.GetJackPotPool(SlotControllerConstants.JACKPOT_TYPE.MAJOR.ToString());
        m_MinorJackpot = this.m_slotConfig.GetJackPotPool(SlotControllerConstants.JACKPOT_TYPE.MINOR.ToString());
        m_MiniJackpot = this.m_slotConfig.GetJackPotPool(SlotControllerConstants.JACKPOT_TYPE.MINI.ToString());

        // 在无限模式下初始化每个 Jackpot 的 Cash 值（优先从缓存读取）
        if (OnLineEarningMgr.Instance != null && OnLineEarningMgr.Instance.isInfiniteOpen())
        {
            if (m_GrandJackpot != null)
            {
                int defaultCash = OnLineEarningMgr.Instance.GetJackpotGameWinReward(2); // GRAND = 2
                m_GrandJackpot.InitCash(defaultCash);
            }
            if (m_MajorJackpot != null)
            {
                int defaultCash = OnLineEarningMgr.Instance.GetJackpotGameWinReward(3); // MAJOR = 3
                m_MajorJackpot.InitCash(defaultCash);
            }
            if (m_MinorJackpot != null)
            {
                int defaultCash = OnLineEarningMgr.Instance.GetJackpotGameWinReward(4); // MINOR = 4
                m_MinorJackpot.InitCash(defaultCash);
            }
            if (m_MiniJackpot != null)
            {
                int defaultCash = OnLineEarningMgr.Instance.GetJackpotGameWinReward(5); // MINI = 5
                m_MiniJackpot.InitCash(defaultCash);
            }
        }
    }
    
    public double GetJackPotIncreaseValue(string jackpotName)
    {
        JackPotPrizePool jackPotPrizePool = m_slotConfig.GetJackPotPool(jackpotName);
        return jackPotPrizePool.GetExtraAwardValue();
    }

    public double GetJackpotAward(string jackpotName)
    {
        double ret = 0;
        if (m_slotConfig !=null)
        {
            JackPotPrizePool jackpot = this.m_slotConfig.GetJackPotPool(jackpotName);
            if (jackpot != null)
            {
                ret = jackpot.GetTotalAward();
                jackpot.ExtraAward = 0f;
                jackpot.SaveExtraAward(0);
            }
            //awardValue = (long)tmp;
        }
        return ret;
    }

    /// <summary>
    /// 获取 Jackpot 的 Cash 值（网赚无限模式下使用）
    /// </summary>
    public int GetJackpotCash(string jackpotName)
    {
        if (m_slotConfig != null)
        {
            JackPotPrizePool jackpot = this.m_slotConfig.GetJackPotPool(jackpotName);
            if (jackpot != null)
            {
                return jackpot.Cash;
            }
        }
        return 0;
    }

    public void RefreshJpUi(SlotControllerConstants.JACKPOT_TYPE jpType)
    {
        bool isInfiniteMode = OnLineEarningMgr.Instance != null && OnLineEarningMgr.Instance.isInfiniteOpen();

        if (jpType == SlotControllerConstants.JACKPOT_TYPE.GRAND)
        {
            if (isInfiniteMode)
            {
                // 无限模式：显示 cash（固定值）
                m_GrandTxt.SetNumber(m_GrandJackpot.Cash, 0f);
            }
            else
            {
                // 其他模式：显示金币
                m_GrandTxt.SetNumber(this.m_GrandJackpot.GetTotalAward(), 0f);
            }
        }
        else if(jpType == SlotControllerConstants.JACKPOT_TYPE.MAJOR)
        {
            if (isInfiniteMode)
            {
                m_MajorTxt.SetNumber(m_MajorJackpot.Cash, 0f);
            }
            else
            {
                m_MajorTxt.SetNumber(this.m_MajorJackpot.GetTotalAward(), 0f);
            }
        }
        else if(jpType == SlotControllerConstants.JACKPOT_TYPE.MINOR)
        {
            if (isInfiniteMode)
            {
                m_MinorTxt.SetNumber(m_MinorJackpot.Cash, 0f);
            }
            else
            {
                m_MinorTxt.SetNumber(this.m_MinorJackpot.GetTotalAward(), 0f);
            }
        }
        else if(jpType == SlotControllerConstants.JACKPOT_TYPE.MINI)
        {
            if (isInfiniteMode)
            {
                m_MiniTxt.SetNumber(m_MiniJackpot.Cash, 0f);
            }
            else
            {
                m_MiniTxt.SetNumber(this.m_MiniJackpot.GetTotalAward(), 0f);
            }
        }
    }

    public void JackpotNumRefreshDirect()
    {
        bool isInfiniteMode = OnLineEarningMgr.Instance != null && OnLineEarningMgr.Instance.isInfiniteOpen();

        if (isInfiniteMode)
        {
            // 无限模式：显示 cash（固定值）
            m_GrandTxt.SetNumber(m_GrandJackpot.Cash, 0f);
            m_MajorTxt.SetNumber(m_MajorJackpot.Cash, 0f);
            if (m_MinorTxt != null && m_MinorJackpot != null)
            {
                m_MinorTxt.SetNumber(m_MinorJackpot.Cash, 0f);
            }
            if (m_MiniTxt != null && m_MiniJackpot != null)
            {
                m_MiniTxt.SetNumber(m_MiniJackpot.Cash, 0f);
            }
        }
        else
        {
            // 其他模式：显示金币
            m_GrandTxt.SetNumber(this.m_GrandJackpot.GetTotalAward(), 0f);
            m_MajorTxt.SetNumber(this.m_MajorJackpot.GetTotalAward(), 0f);
            if (m_MinorTxt != null)
            {
                m_MinorTxt.SetNumber(this.m_MinorJackpot.GetTotalAward(), 0f);
            }
            if(m_MiniTxt !=null)
            {
                m_MiniTxt.SetNumber(this.m_MiniJackpot.GetTotalAward(), 0f);
            }
        }
    }

    public void JackpotNumRefreshTween()
    {
        bool isInfiniteMode = OnLineEarningMgr.Instance != null && OnLineEarningMgr.Instance.isInfiniteOpen();

        if (isInfiniteMode)
        {
            // 无限模式：显示 cash（固定值，不需要动画）
            m_GrandTxt.SetNumber(m_GrandJackpot.Cash, 0f);
            m_MajorTxt.SetNumber(m_MajorJackpot.Cash, 0f);
            if (m_MinorTxt != null && m_MinorJackpot != null)
            {
                m_MinorTxt.SetNumber(m_MinorJackpot.Cash, 0f);
            }
            if (m_MiniTxt != null && m_MiniJackpot != null)
            {
                m_MiniTxt.SetNumber(m_MiniJackpot.Cash, 0f);
            }
        }
        else
        {
            // 其他模式：显示金币增长动画
            m_GrandTxt.IncreaseTo(this.m_GrandJackpot.GetTotalAward());
            m_MajorTxt.IncreaseTo(this.m_MajorJackpot.GetTotalAward());
            if(m_MinorTxt!=null)
            {
                m_MinorTxt.IncreaseTo(this.m_MinorJackpot.GetTotalAward());
            }
            if (m_MiniTxt != null)
            {
                m_MiniTxt.IncreaseTo(this.m_MiniJackpot.GetTotalAward());
            }
        }
    }

    public void AddGrandWithBet()
    {
        long bet = CurrentBetting;
        this.m_GrandJackpot.AddPoolAwardWithBet(bet);
    }

    public void AddMajorWithBet()
    {
        long bet = CurrentBetting;
        this.m_MajorJackpot.AddPoolAwardWithBet(bet);
    }

    public void AddMinorWithBet()
    {
        long bet = CurrentBetting;
        if(this.m_MinorJackpot !=null)
        {
            this.m_MinorJackpot.AddPoolAwardWithBet(bet);
        }
    }

    public void AddMiniWithBet()
    {
        long bet = CurrentBetting;
        if(m_MiniJackpot !=null)
        {
            this.m_MiniJackpot.AddPoolAwardWithBet(bet);
        }
    }

    long CurrentBetting
    {
        get
        {
            if (BaseSlotMachineController.Instance != null)
            {
                return (long)BaseSlotMachineController.Instance.currentBetting;
            }
            else
            {
                return (long)Classic.TestController.Instance.currentBetting;
            }
        }
    }
}
