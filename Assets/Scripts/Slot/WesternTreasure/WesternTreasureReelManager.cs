using Libs;
using Classic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;
using UnityEngine.Serialization;
//using NUnit.Framework;
using Utils;

public class WesternTreasureReelManager : GoldsReelManager
{
    public List<JackpotItemRender> jackpotItemRenders = new List<JackpotItemRender>();
    //需要保存恢复
    public List<Vector3> BGBonusAppearPosition = new List<Vector3>();
    private bool isFreeRecover = false;
    [HideInInspector]
    public WesternTreasureSpinResult spinResult;
    [HideInInspector]
    public WesternTreasureJackpotData jackPotData;
    [FormerlySerializedAs("WesternTreasureJackpotGame")] public WesternTreasureJackpotGame westernTreasureJackpotGame;
    [FormerlySerializedAs("WesternTreasureFly")] public WesternTreasureFly westernTreasureFly;
    public GameObject jackpotGameBack;
    private System.Action reelStopcallback;

    protected override void Awake()
    {
        base.Awake();
        Messenger.AddListener<long>(SlotControllerConstants.OnBetChange, OnChangBet);
    }
    protected override void OnDestroy()
    {
        Messenger.RemoveListener<long>(SlotControllerConstants.OnBetChange, OnChangBet);
        base.OnDestroy();
    }

    public void OnChangBet(long bet)
    {
        jackPotData.UpdataUI(true);
    }
    public override void InitReels(SlotMachineConfig slotConfig, Classic.GameCallback onStop, Classic.GameCallback onStart)
    {
        spinResult = new WesternTreasureSpinResult(this, slotConfig.extroInfos.infos);
        base.InitReels(slotConfig, onStop, onStart);
        jackPotData = new WesternTreasureJackpotData(jackpotItemRenders, slotConfig);
        if (jackPotData.JackPotPrizePoolInfos.Count > 0) jackPotData.UpdataUI();
        if (westernTreasureJackpotGame!=null) {
            westernTreasureJackpotGame.InitJackpotGame(this);
        }
        if (westernTreasureFly!=null) {
            westernTreasureFly.Init(this);
        }
    }

    public override string GetTestLog()
    {
        return base.GetTestLog();
    }

    public override void InitAtStart() 
    { 
        this.IsNewProcess = true; 
    } 
    public override double FsEnterScatterAward()
    {
        return this.spinResult.GetScatterPayAward() * SlotsBet.GetCurrentBet();
    }
    public override double FsRetriggerScatterAward()
    {
        return this.spinResult.GetScatterPayAward() * SlotsBet.GetCurrentBet();
    }

    public override double GetBonusAward()
    {
        return spinResult.winMoney;
    }
    public override void CheckHitFS()
    {
        this.spinResult.SetFreespin();
    }
   
    public override void CheckHitBonus()
    {
        this.HasBonusGame = spinResult.CheckHitBonus();
    }

    public override void PreStartState()
    {
        NeedReCreatResult = true;
        base.PreStartState();
    }
    public override void ChangeSmartAnimationPosition(int reelId, float offsetY)
    {
        
    }
    // ReSharper disable Unity.PerformanceAnalysis
    protected override void ReelsStartRun()
    {
        PlayStartRunAudio();
        if (this.IsNewSpeedPattern)
        {
            //NeedAntiReels = this.spinEffectController.GenerateSpinEffectResult (GetCurrentResults(),symbolMap);
            //TODO: result处理
            if (smartEffectController != null)
            {
                this.NeedAntiReels = smartEffectController.CheckSmartPositionAndAnticipation(this.resultContent.ReelResults, this.symbolMap, this);
            }

            this.NeedPlaySmartSound = CheckPlaySmartSound();

            this.boardController.SetTestSpeedPattern(this.slotConfig.IsUseTestSpeed);

            List<List<int> > resultData = new List<List<int>>();
            List<List<int> > showData = new List<List<int>>();
            for (int i = 0; i < resultContent.ReelResults.Count; i++)
            {
                resultData.Add(resultContent.ReelResults[i].SymbolResults);
                showData.Add(resultContent.ReelResults[i].ShowIndexs);
            }
            this.boardController.DoReelsSpin(resultData,showData, this.NeedAntiReels, GetReelCurveIndexs(), NeedPlaySmartSound);

        }

        if (isFreespinBonus)
        {
            IsTriggeredFreespinAndNotReelStartRun = false;
        }
        else
        {
            if (FreespinCount > 0)
            {
                IsTriggeredFreespinAndNotReelStartRun = true;
            }
        }
        SaveBoardProgressResult();
        jackPotData.OnSpinJackPot();
    }

    

    //回弹时的回调
    public override void ReelBounceBackHandler(int reelIndex)
    {
        base.ReelBounceBackHandler(reelIndex);
        RecordBonusPositions(reelIndex);
    }
    

    public override void PlayStartRunAudio()
    {
        AudioEntity.Instance.StopAwardSymbolAudio();
        if (this.isFreespinBonus)
        {
            Libs.AudioEntity.Instance.UnPauseBackGroundAudio(Libs.AudioEntity.audioSpecial);
        }
        else
        {
            AudioEntity.Instance.PlayClickEffect();
            // Libs.AudioEntity.Instance.UnPauseBackGroundAudio(Libs.AudioEntity.audioSpin);
        }
    }
    
    protected override void ReCheckAward(Dictionary<string, object> infos)
    {
        this.CalculateProductPayTableAwardInUnidirectional(infos);
        //this.Test();

    }
    public override void CreateRawResult()
    {
        base.CreateRawResult();
        spinResult.ChangeResult();

    }

    public override void ParseSpinResult()
    {
        base.ParseSpinResult();
    }

    protected override void HandleRollEndEvent(Action callback)
    {
        reelStopcallback = callback;
        base.HandleRollEndEvent(callback);
    }
   
    public override void OpenRewinDialog()
    {
        if (isFreeRecover)
        {
            isFreeRecover = false;
            return;
        }
    }
    //如果是单条线判断special则需重写此方法
    public override List<BaseElementPanel> GetFreespinSymbols()
    {
        List<BaseElementPanel> list = new List<BaseElementPanel>();
        foreach (var position in spinResult.freeTriggerSymbolDic)
        {
            list.Add(GetSymbolRender(position.Key, position.Value));
        }
        return list;
    }
    
    private void RecordBonusPositions(int reelIndex)
    {
        BGBonusAppearPosition.Clear();
        for (int i = 0; i < this.resultContent.ReelResults.Count; i++)
        {
            if(i!=reelIndex)continue;
            
            for (int j = 0; j < this.resultContent.ReelResults[i].SymbolResults.Count; j++)
            {
                SymbolMap.SymbolElementInfo info =
                    symbolMap.getSymbolInfo(this.resultContent.ReelResults[i].SymbolResults[j]);
                if (info.getBoolValue(SymbolMap.IS_WILD))
                {
                    Vector3 v = this.GetSymbolRender(i, j).transform.position;
                    BGBonusAppearPosition.Add(v);
                    //spinResult.wildNum++;
                }
            }
        }

        if (westernTreasureFly!=null)
        {
            westernTreasureFly.BaseGameCollectBonus(reelIndex,BGBonusAppearPosition);
        }
    }

    
    #region OpenDialog

    public void OpenJackpotGame()
    {
        spinResult.isTriggerJackpotGame = false;
       
//        Messenger.Broadcast<bool>(SlotControllerConstants.DisactiveButtons, true);
//        Messenger.Broadcast<bool>(SpinButtonStyle.DISABLEAUTOSPIN, true);
//        Messenger.Broadcast<bool>(SlotControllerConstants.DisableSpinButton, true); 
       
        if (westernTreasureJackpotGame!=null) {
            westernTreasureJackpotGame.gameObject.SetActive(true);
            westernTreasureJackpotGame.EnterGame();
        }

        if (jackpotGameBack!=null)
        {
            jackpotGameBack.SetActive(true);
        }
    }
    
    
    private void CloseJackpotGame()
    {
        Messenger.Broadcast(WesternTreasureBonusGame.TreeBonusGameEnd);
        this.HasBonusGame = false;
        spinResult.isOpenEndDialog = false;
        
        this.spinResult.ClearJackpotData();
        westernTreasureFly.ChangeSkin(1);
        westernTreasureFly.ClearAnimation();
        
        westernTreasureFly.RefreshSlider();
       
        
        if (westernTreasureJackpotGame != null)
        {
            westernTreasureJackpotGame.gameObject.SetActive(false);
        }
        if (jackpotGameBack!=null)
        {
            jackpotGameBack.SetActive(false);
        }
    }

    public void OpenJackpotEndDialog(int jackpotType,double money)
    {
        if (jackpotType==2)
        {
            
            UIManager.Instance.OpenMachineDialog<WesternTreasureGrandDialog>((dialog) =>
            {
                AudioManager.Instance.AsyncPlayEffectAudio("Bonus_grand");
                dialog.OnStart(money,jackpotType);
            }, () =>
            {
               
                CloseJackpotGame();
                //westernTreasureJackpotGame.ClearGame();
            });
        }
        else if (jackpotType==3)
        {
            
            UIManager.Instance.OpenMachineDialog<WesternTreasureMajorDialog>( (dialog) =>
            {
                AudioManager.Instance.AsyncPlayEffectAudio("Bonus_jackpot");
                dialog.OnStart(money,jackpotType);
            }, () => {   
               
                CloseJackpotGame();
                //westernTreasureJackpotGame.ClearGame();
            });
        }
        else if (jackpotType==4)
        {
            UIManager.Instance.OpenMachineDialog<WesternTreasureManorDialog>((dialog) =>
            {
                AudioManager.Instance.AsyncPlayEffectAudio("Bonus_jackpot");
                dialog.OnStart(money,jackpotType);
            }, () => {  
               
                CloseJackpotGame();
                //westernTreasureJackpotGame.ClearGame();
            });
        }else if (jackpotType==5)
        {
            UIManager.Instance.OpenMachineDialog<WesternTreasureMiniDialog>((dialog) =>
            {
                AudioManager.Instance.AsyncPlayEffectAudio("Bonus_jackpot");
                dialog.OnStart(money,jackpotType);
            }, () => { 
               
                CloseJackpotGame();
                //westernTreasureJackpotGame.ClearGame();
            });
        }
    }

    
    

    #endregion


    #region 保存恢复

    protected override void CreateProgreeData()
    {
        if (FreespinCount>0 && IsAlreadyAddFreeSpinCount)
        {
            FreespinCount = 0;
        }
        this.spinResult.SaveGameData(); 
    }
    
    protected override void SaveProgressDataToLocal()
    {
        SceneProgressManager.SaveSceneJson<WesternTreasureSpinResult>(slotConfig.Name(), this.spinResult);
    }

    protected override void LoadLocalProgress()
    {
        WesternTreasureSpinResult TreeFr = SceneProgressManager.LoadSceneJson<WesternTreasureSpinResult>(slotConfig.Name());
        this.spinResult.LoadGameData(TreeFr);
    }

    private int freespinCount;
    protected override void RestoreBaseProgressData()
    {
        if (westernTreasureFly!=null)
        {
            int skinId = westernTreasureFly.ChargeTreeLevel(spinResult.wildNum);
            westernTreasureFly.ChangeSkin(skinId);
            //刷新进度条的进度
            westernTreasureFly.RefreshSlider(true);
            //初始化动画状态
            westernTreasureFly.ClearAnimation();
        }
        //BaseSlotMachineController.Instance.RestoreSlotsMachinesGameStateData(this.spinResult.currentBetting);
        if (spinResult.IsInFreespin)
        {
            
            AudioEntity.Instance.StopMusicAudio("Initiation");
            this.isFreespinBonus = true;
            
            FreespinGame.NeedRecoveryData = true;
            
            BaseSlotMachineController.Instance.winCoinsForDisplay = spinResult.currentWinCoins;
            BaseSlotMachineController.Instance.winCashForDisplay = spinResult.currentWinCash;
            Messenger.Broadcast<long>(SlotControllerConstants.OnChangeWinTextSilence, BaseSlotMachineController.Instance.winCoinsForDisplay);
            Messenger.Broadcast<int>(SlotControllerConstants.OnChangeCashTextSilence, BaseSlotMachineController.Instance.winCashForDisplay);

            BaseSlotMachineController.Instance.RestoreSlotsMachinesGameStateData(this.spinResult.currentBetting);
            ResultStateManager.Instante.slotController.spinning = true;
            
            if (this.spinResult.freespinCount > 0)
            {
                this.FreespinCount = this.spinResult.freespinCount;
            }
       
            FreespinGame.multiplier = this.spinResult.awardMultipler;
            FreespinGame.LeftTime = this.spinResult.leftFreeSpinCount;
            FreespinGame.TotalTime = this.spinResult.totalFreeSpinCount;
            FreespinGame.NeedShowCash = this.spinResult.needShowCash;
            resultContent.ChangeResult(this.freeSpinResult);
            ChangeSymbols(this.freeSpinResult);
            ResultStateManager.Instante.RestoreFreespin(spinResult.leftFreeSpinCount, spinResult.totalFreeSpinCount, spinResult.currentWinCoins, true, spinResult.awardMultipler, !spinResult.isInJackpotGame, false, true,spinResult.currentWinCash,spinResult.needShowCash);
            
            
           this.RecoverLinkGameData();
           this.spinResult.IsInFreespin = false;
        }  else if (spinResult.freespinCount > 0 && this.spinResult.isInJackpotGame)
        {
            this.RecoverLinkGameData();
        }
        else if (spinResult.freespinNum > 0 || spinResult.freespinCount>0)
        {
            AudioEntity.Instance.StopMusicAudio("Spin");
            BaseSlotMachineController.Instance.winCoinsForDisplay = spinResult.currentWinCoins;
            Messenger.Broadcast<long>(SlotControllerConstants.OnChangeWinTextSilence, BaseSlotMachineController.Instance.winCoinsForDisplay);
            BaseSlotMachineController.Instance.winCashForDisplay = spinResult.currentWinCash;
            Messenger.Broadcast<int>(SlotControllerConstants.OnChangeCashTextSilence, BaseSlotMachineController.Instance.winCashForDisplay);
            BaseSlotMachineController.Instance.RestoreSlotsMachinesGameStateData(this.spinResult.currentBetting);
            if (spinResult.freespinNum > 0) freespinCount = spinResult.freespinNum;
            if (spinResult.freespinCount > 0) freespinCount = spinResult.freespinCount;
            this.FreespinCount = freespinCount;
            resultContent.ChangeResult(this.baseGameResult);
            ChangeSymbols(this.baseGameResult);
            ResultStateManager.Instante.slotController.spinning = true;
            ResultStateManager.Instante.RestoreFreespinOnBaseGame(freespinCount);
        }
        else
        {
            this.RecoverLinkGameData();
        }

    }
   
    private void RecoverLinkGameData()
    {
        if (this.spinResult.isInJackpotGame)
        {
            BaseSlotMachineController.Instance.spinning = false;
            HasBonusGame = true;
            if (BonusGame != null)
                BonusGame.IsBonusGame = true;
            BaseSlotMachineController.Instance.RestoreSlotsMachinesGameStateData(this.spinResult.currentBetting);
            BaseSlotMachineController.Instance.winCoinsForDisplay = this.spinResult.currentWinCoins;
            Messenger.Broadcast<float, long, long>(SlotControllerConstants.OnChangeWinText, 0, 0,BaseSlotMachineController.Instance.winCoinsForDisplay);
            BaseSlotMachineController.Instance.winCashForDisplay = this.spinResult.currentWinCash;
            Messenger.Broadcast<float, int, int>(SlotControllerConstants.OnChangeCashText, 0, 0,BaseSlotMachineController.Instance.winCashForDisplay);
            //spin按钮
            Messenger.Broadcast<bool>(SlotControllerConstants.DisableSpinButton, true); 
            if (spinResult.freespinCount > 0 && !spinResult.IsInFreespin)
            {
                if (spinResult.freespinNum > 0) freespinCount = spinResult.freespinNum;
                if (spinResult.freespinCount > 0) freespinCount = spinResult.freespinCount;
                this.FreespinCount = freespinCount;
                resultContent.ChangeResult(this.baseGameResult);
                ChangeSymbols(this.baseGameResult);
                ResultStateManager.Instante.AddState (new FreespinResultState(spinResult.freespinCount,1).action,true);
            }
            ResultStateManager.Instante.AddBonusGame(true);
            
            ResultStateManager.Instante.AddNormal();
            ResultStateManager.Instante.ExcuteRestoreState();
        }
    }

    #endregion

    
}
