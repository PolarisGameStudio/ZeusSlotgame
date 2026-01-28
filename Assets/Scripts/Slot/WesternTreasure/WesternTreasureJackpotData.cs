using System.Collections.Generic;
using UnityEngine;
using Classic;

public class WesternTreasureJackpotData
{
    public List<JackPotPrizePool> JackPotPrizePoolInfos = new List<JackPotPrizePool>();
    private List<JackpotItemRender> jackpotItemRenders ;

    public WesternTreasureJackpotData(List<JackpotItemRender> _jackpotItemRenders, SlotMachineConfig _slotConfig)
    {
        this.jackpotItemRenders = _jackpotItemRenders;
        this.InitJackPot(_slotConfig);
    }

    private void InitJackPot(SlotMachineConfig slotConfig)
    {
        JackPotPrizePoolInfos = slotConfig.JackPotPrizeInfos;

        if (JackPotPrizePoolInfos.Count == 0)
        {
            Utils.Utilities.LogPlistError("WesternTreasure JackPotData not config in classicconfig.plist.xml! ");
        }

        // 初始化奖池数据和对应的渲染器
        for (int i = 0; i < JackPotPrizePoolInfos.Count; i++)
        {
            // 在无限模式下初始化 cash 值（优先从缓存读取）
            if (OnLineEarningMgr.Instance != null && OnLineEarningMgr.Instance.isInfiniteOpen())
            {
                int jackpotType = GetJackpotTypeByAwardName(JackPotPrizePoolInfos[i].AwardName);
                if (jackpotType > 0)
                {
                    int defaultCash = OnLineEarningMgr.Instance.GetJackpotGameWinReward(jackpotType);
                    JackPotPrizePoolInfos[i].InitCash(defaultCash);
                }
            }

            switch (JackPotPrizePoolInfos[i].AwardName)
            {
                case "GRAND":
                    {
                        JackPotIncreaseMachineConfig increaseConfig = slotConfig.GetJackPotIncreaseConfig(JackPotPrizePoolInfos[i].AwardName);
                        jackpotItemRenders[0].SetPrizePoolData(slotConfig, JackPotPrizePoolInfos[i], increaseConfig);
                    }
                    break;
                case "MAJOR":
                    {
                        JackPotIncreaseMachineConfig increaseConfig = slotConfig.GetJackPotIncreaseConfig(JackPotPrizePoolInfos[i].AwardName);
                        jackpotItemRenders[1].SetPrizePoolData(slotConfig, JackPotPrizePoolInfos[i], increaseConfig);
                    }
                    break;
                case "MINOR":
                    {
                        JackPotIncreaseMachineConfig increaseConfig = slotConfig.GetJackPotIncreaseConfig(JackPotPrizePoolInfos[i].AwardName);
                        jackpotItemRenders[2].SetPrizePoolData(slotConfig, JackPotPrizePoolInfos[i], increaseConfig);
                    }
                    break;
                case "MINI":
                    {
                        JackPotIncreaseMachineConfig increaseConfig = slotConfig.GetJackPotIncreaseConfig(JackPotPrizePoolInfos[i].AwardName);
                        jackpotItemRenders[3].SetPrizePoolData(slotConfig, JackPotPrizePoolInfos[i], increaseConfig);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 根据 AwardName（GRAND/MAJOR/MINOR/MINI）映射到 jackpotType（2~5）
    /// 与 WesternTreasureSpinResult.winType 及 GetJackPotName 保持一致
    /// </summary>
    private int GetJackpotTypeByAwardName(string awardName)
    {
        switch (awardName)
        {
            case "GRAND":
                return 2;
            case "MAJOR":
                return 3;
            case "MINOR":
                return 4;
            case "MINI":
                return 5;
            default:
                return -1;
        }
    }


    public void OnSpinJackPot()
    {
        if (BaseSlotMachineController.Instance.isFreeRun)
        {
            return;
        }
        for (int i = 0; i < JackPotPrizePoolInfos.Count; i++)
        {
            JackPotPrizePoolInfos[i].AddPoolAwardWithBet(BaseSlotMachineController.Instance.currentBetting);
        }

        for (int i = 0; i < jackpotItemRenders.Count; i++)
        {
            jackpotItemRenders[i].TxtAwardNum.IncreaseTo(JackPotPrizePoolInfos[i].GetTotalAward());
        }
        UpdataUI(true);
    }
    public void UpdataUI(bool isNeedPlaySound = false)
    {
        if (jackpotItemRenders == null || jackpotItemRenders.Count == 0)
        {
            return;
        }
        for (int i = 0; i < jackpotItemRenders.Count; i++)
        {
            jackpotItemRenders[i].Refresh(isNeedPlaySound);
        }
    }
    public double GetJackPotIncreaseValue(int index)
    {
        string name = GetJackPotName(index);
        JackPotPrizePool jackPotPrizePool = GetJackPotInof(name);
        if (jackPotPrizePool == null) return 0;
        return jackPotPrizePool.GetExtraAwardValue();
    }

    public double GetJackPotAward(string name)
    {
        //Debug.LogError("获取jackpot           "+name);
        JackPotPrizePool jackPotPrizePool = GetJackPotInof(name);
        double jackpotAward = 0;
        if (jackPotPrizePool != null)
        {
            jackpotAward = jackPotPrizePool.GetTotalAward();
            jackPotPrizePool.ExtraAward = 0;
            jackPotPrizePool.SaveExtraAward(jackPotPrizePool.ExtraAward);
        }
        return jackpotAward;
    }
    public double GetJackPotAward(int index)
    {
        string name = GetJackPotName(index);

        //Debug.LogError("获取jackpot           "+name);
        JackPotPrizePool jackPotPrizePool = GetJackPotInof(name);
        double jackpotAward = 0;
        if (jackPotPrizePool != null)
        {
            jackpotAward = jackPotPrizePool.GetTotalAward();
            jackPotPrizePool.ExtraAward = 0;
            jackPotPrizePool.SaveExtraAward(jackPotPrizePool.ExtraAward);
        }

        return jackpotAward;
    }

    /// <summary>
    /// 获取 Jackpot 对应的 Cash 值（与 GetJackPotAward 逻辑一致）
    /// </summary>
    public int GetJackPotCash(string name)
    {
        JackPotPrizePool jackPotPrizePool = GetJackPotInof(name);
        if (jackPotPrizePool != null)
        {
            return jackPotPrizePool.Cash;
        }
        return 0;
    }

    /// <summary>
    /// 根据索引获取 Jackpot 对应的 Cash 值
    /// </summary>
    public int GetJackPotCash(int index)
    {
        string name = GetJackPotName(index);
        JackPotPrizePool jackPotPrizePool = GetJackPotInof(name);
        if (jackPotPrizePool != null)
        {
            return jackPotPrizePool.Cash;
        }
        return 0;
    }

    private JackPotPrizePool GetJackPotInof(string jackPotName)
    {
        for (int i = 0; i < JackPotPrizePoolInfos.Count; i++)
        {
            if (jackPotName == JackPotPrizePoolInfos[i].AwardName)
            {
                return JackPotPrizePoolInfos[i];
            }
        }
        return null;
    }
    public string GetJackPotName(int index)
    {
        switch (index)
        {
            case 2:
                return "GRAND";
            case 3:
                return "MAJOR";
            case 4:
                return "MINOR";
            case 5:
                return "MINI";
            default:
                return "";
        }
    }
    public int JackPotIndex(string name)
    {
        switch (name)
        {
            case "GRAND":
                return 1;
            case "MAJOR":
                return 2;
            case "MINOR":
                return 3;
            case "MINI":
                return 4;
            default:
                return -1;
        }
    }
}
