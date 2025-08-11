using System;
using UnityEngine;
using System.Collections.Generic;
using Beebyte.Obfuscator;

[Skip]
public class GameDialogManager : MonoBehaviour
{
    #region
    /// relate to GoodSKU by石浩卿
    public const string PurchaseCoinSuccessMsg = "PurchaseCoinSuccessMsg";
    #endregion

	public const string OpenSettingDialog = "OpenSettingDialog";
	
	public const string OpenTerms_EUDialogMsg = "OpenTerms_EUDialogMsg";
	/// <summary>
	/// 1.2.6 or later
	/// The open scene exchange.不要直接调用广播事件，要通过SwapSceneManager的SwapScene来切换场景
	/// </summary>
	public const string OpenSceneExchange = "OpenSceneExchange";
	public const string OpenBlackOutPanel = "OpenBlackOutPanel";
	public const string OpenAutoSpinWinDialog = "OpenAutoSpinWinDialog";
    public const string OpenReSpinStartDialog = "OpenReSpinStartDialog";
    public const string OpenAutoSpinAdditionalDialog = "OpenAutoSpinAdditionalDialog";
    public const string OpenHotHotChiliFreeSpinlDialog = "OpenHotHotChiliFreeSpinlDialog";
    public const string OpenHotHotChiliFreeSpinWinDialog = "OpenHotHotChiliFreeSpinWinDialog";

    public const string OPEN_WIN_PUSH = "OpenWinPush";
    
	public const string OpenFreeSpinEndDialog = "OpenFreeSpinEndDialog";
    public const string OpenReSpinEndDialog = "OpenReSpinEndDialog";
	public const string OpenKindOfSymbolDialog = "OpenKindOfSymbolDialog";
	public const string OpenPowerMachineTipsDialog = "OpenPowerMachineTipsDialog";
	public const string OpenRewardSpinEndDialog = "OpenRewardSpinEndDialog";
	public const string OpenRewin = "OpenRewin";
	public const string OpenPaytablePanel = "OpenPaytablePanel";

	public const string CloseLoadingWaitDialog = "CloseLoadingWaitDialog";
	public const string OpenLoadingWaitDialog = "OpenLoadingWaitDialog";

	public const string OpenLoadingWaitDialogCallback = "OpenLoadingWaitDialogCallback ";
	public const string OpenLoadingWaitDialogCallbackAtOnce = "OpenLoadingWaitDialogCallbackAtOnce";
	public const string OpenLoadingWaitDialogCallbackAtOnceWithTime = "OpenLoadingWaitDialogCallbackAtOnceWithTime";
	
	public const string OpenRateMachine = "OpenRateMachine";

    public const string OpenJackPotWinDialog_Jackpot777 = "openJackPotWinDialog_Jackpot777";

	public const string OpenJackPotMineWinDialog = "OpenJackPotMineWinDialog";
	public const string OpenBetTips = "OpenBetTips";
	public const string OpenMissionProgressDialog = "OpenMissionProgressDialog";
	public const string OpenAutoQuestCatalogDialog = "OpenAutoQuestCatalogDialog";
	public const string OpenAutoQuestDetailDialog = "OpenAutoQuestDetailDialog";
	
	public const string OPEN_EPIC_WIN_DIALOG = "OpenEpicWinDialog";
	public const string OPEN_FIRST_GO_LOBBY_DIALOG = "OpenFirstGoLobbyDialog";
	public const string OPEN_HIGH_ROLLER_DIALOG ="OpenHighRollerDialog";
	public const string OPEN_JACKPOT_BET_CHANGE_DIALOG = "OpenJackpotChangeDialog";
	public const string HIDE_JACKPOT_BET_CHANGE_DIALOG = "HideJackpotChangeDialog";
	public const string OPEN_JACKPOT_WIN_PUSH = "OpenJackpotWinPush";

	public const string TaskAwardMessageTimeOutMsg ="TaskAwardMessageOutOfTimeMsg";
	public const string OpenInvalidCodeDialog ="OpenInvalidCodeDialog";
	public const string OpenExpirePromoCodeDialog ="OpenExpirePromoCodeDialog";
	public const string OpenCodeAlreadyClaimedDialogMsg ="OpenCodeAlreadyClaimedDialog";

	
    public const string OpenSpinLimitDialog = "OpenSpinLimitDialog";
	public const string OPEN_TIME_OUT_DIALOG = "OPEN_TIME_OUT_DIALOG";
	public const string openLoadingDialogMsg ="openLoadingDialog";

	public const string OpenPopRewardSmallDialog = "OpenPopRewardSmallDialog";
	public const string OpenPopRewardBigDialog = "OpenPopRewardBigDialog";
	public const string OpenWithDrawDialog = "OpenWithDrawDialog";
	public const string CloseWithDrawDialog = "CloseWithDrawDialog";

	public const string OpenGetMoreCashDialog = "OpenGetMoreCashDialog";
	public const string OpenSpinWithDrawStartDialogMsg = "OpenSpinWithDrawStartDialog";
	public const string OpenSpinWithDrawEndDialogMsg = "OpenSpinWithDrawEndDialog";
	public const string OpenRewardCashDialogMsg = "OpenRewardCashDialogMsg";
	public const string OpenContinueSpinDialogMsg = "OpenContinueSpinDialogMsg";

	public const string OpenNewUserGuidDialogMsg = "OpenNewUserGuidDialog";
	public const string OpenAccountDialogMsg = "OpenAccountDialog";
	public const string OpenAccountLoginTipsMsg = "OpenAccountLoginTipsMsg";
	public const string OpenAccountEnsureMsg = "OpenAccountEnsureMsg";
	//cardSystem
	public const string OpenCardSystemCollectionDialogMsg = "OpenCardSystemCollectionDialog";
	public const string OpenCardSystemGetCardDialogMsg = "OpenCardSystemGetCardDialogMsg";
	public const string OpenCardSystemLuckyDrawDialogMsg = "OpenCardSystemLuckyDrawDialogMsg";
	
	//Tips
	public const string OpenTipsDialogMsg = "OpenTipsDialogMsg";
	public const string OpenTaskTipsDialogMsg = "OpenTaskTipsDialogMsg";

	public static bool QuestDialogShowed { get; set; }

  

	// Cause GameDialogManager script has a "DontDestroy" sibling which destroy all "new" GameDialogManagers. 
	// So use static here is OK for preventing multi-times add listener before "new" GameDialogManager got destroyed. 
	public static int ListenerAddedCount { get; set; }
	
	void Awake ()
	{
		AddListeners ();
	}

	public virtual void AddListeners ()
	{
		ListenerAddedCount++;

		if (ListenerAddedCount == 1) {
			Messenger.AddListener (OpenSettingDialog, openSettingDialog);
			
			Messenger.AddListener<System.Action,object> (OpenSceneExchange, openSceneExchange);
			Messenger.AddListener<System.Action> (OpenBlackOutPanel, openBlackOutPanel);
			Messenger.AddListener<int,System.Action,bool,bool> (OpenAutoSpinWinDialog, openAutoSpinWinDialog);
            Messenger.AddListener<System.Action>(OpenReSpinStartDialog, openReSpinStartDialog);
            Messenger.AddListener<int,System.Action,bool> (OpenAutoSpinAdditionalDialog, openAutoSpinAdditionalDialog);
			Messenger.AddListener<bool> (OpenTerms_EUDialogMsg, OnOpenTerms_EUDialogMsg);

			Messenger.AddListener<Dictionary<string,object>,System.Action,bool> (OpenFreeSpinEndDialog, openFreeSpinEndDialog);
            Messenger.AddListener<long, System.Action>(OpenReSpinEndDialog, openReSpinEndDialog);

			Messenger.AddListener<Sprite, int, System.Action>(OpenKindOfSymbolDialog, openKindOfSymbolDialog);
			Messenger.AddListener(OpenPowerMachineTipsDialog, openPowerMachineTipsDialog);

            Messenger.AddListener<int, System.Action, bool>(OpenHotHotChiliFreeSpinlDialog, openHotHotChiliFreeSpinlDialog);
			Messenger.AddListener<long, System.Action>(OpenHotHotChiliFreeSpinWinDialog, openHotHotChiliFreeSpinWinDialog);

			//注册adWincon
			Messenger.AddListener<RewardSpinEvent,System.Action> (OpenRewardSpinEndDialog, openRewardSpinEndDialog);

		
			
		
			Messenger.AddListener<System.Action> (OpenRewin, openRewin);
			Messenger.AddListener (OpenPaytablePanel, openPaytablePanel);
			
			Messenger.AddListener (OpenLoadingWaitDialog, openLoadingWaitingDialog);
			Messenger.AddListener <Action>(OpenLoadingWaitDialogCallback, openLoadingWaitCallback);
			Messenger.AddListener <Action>(OpenLoadingWaitDialogCallbackAtOnce, openLoadingWaitCallbackAtOnce);
			Messenger.AddListener <Action,float>(OpenLoadingWaitDialogCallbackAtOnceWithTime, openLoadingWaitCallbackAtOnceWithTime);

			Messenger.AddListener (CloseLoadingWaitDialog, closeLoadingWaitDialog);
			Messenger.AddListener<SlotMachineConfig> (OpenRateMachine, this.openRateMachineDialog);
			
			Messenger.AddListener(openLoadingDialogMsg, OpenLoadingDialog);

			Messenger.AddListener<int,long,System.Action> (OpenJackPotMineWinDialog, openJackPotMineWinDialog);
			Messenger.AddListener<SlotMachineConfig, long,bool> (OPEN_JACKPOT_WIN_PUSH, openJackPotMineWinPush);
            Messenger.AddListener<int, long, System.Action>(OpenJackPotWinDialog_Jackpot777, openJackPotWinDialog_Jackpot777);

			Messenger.AddListener (OpenBetTips, openBetTips);
			Messenger.AddListener(OpenAutoQuestCatalogDialog, this.openAutoQuestCatalogDialog);
			Messenger.AddListener<Dictionary<string, object>>(OpenAutoQuestDetailDialog, this.openAutoQuestDetailDialog);
			
			
			Messenger.AddListener (OPEN_EPIC_WIN_DIALOG, this.epicWinDialog);
			Messenger.AddListener (OPEN_FIRST_GO_LOBBY_DIALOG, openFirstGoLobbyDialog);
			Messenger.AddListener<SlotMachineConfig> (OPEN_HIGH_ROLLER_DIALOG, openHighRollerDialog);
			Messenger.AddListener (OPEN_JACKPOT_BET_CHANGE_DIALOG, openJackpotBetChangeDialog);

            Messenger.AddListener(OpenInvalidCodeDialog, OnOpenInvalidCodeDialog);
            
			Messenger.AddListener(OpenExpirePromoCodeDialog, OnOpenExpirePromoCodeDialog);
			Messenger.AddListener(OpenCodeAlreadyClaimedDialogMsg, OnOpenCodeAlreadyClaimedDialogMsg);

			Messenger.AddListener (OPEN_TIME_OUT_DIALOG, openTimeOutDialog);
            // Messenger.AddListener (OpenBetHigherDialog, openBetHigherDialog);
            Messenger.AddListener<System.Action>(OpenSpinLimitDialog, OpenSpinLimimiDialogMsg);
            Messenger.AddListener<int>(OpenRewardCashDialogMsg, OpenRewardCashDialog);


            #region PopReward
            Messenger.AddListener<int,System.Action>(OpenPopRewardSmallDialog, OnOpenPopRewardSmallDialog);
            Messenger.AddListener<int,Action>(OpenPopRewardBigDialog, OnOpenPopRewardBigDialog);
            Messenger.AddListener<int>(OpenGetMoreCashDialog, OnOpenGetMoreCashDialog);
            Messenger.AddListener(OpenWithDrawDialog, OnOpenWithDrawDialog);
            #endregion
            Messenger.AddListener(OpenNewUserGuidDialogMsg, OpenNewUserGuidDialog);

            #region activity

            Messenger.AddListener<int,int,System.Action>(OpenSpinWithDrawStartDialogMsg, OpenSpinWithDrawStartDialog);
            Messenger.AddListener<int,System.Action>(OpenSpinWithDrawEndDialogMsg, OpenSpinWithDrawEndDialog);

            Messenger.AddListener<int,int>(OpenAccountDialogMsg, OpenAccountDialog);
            Messenger.AddListener<int,string,int>(OpenAccountEnsureMsg, OpenAccountEnsureDialog);
            Messenger.AddListener<int>(OpenAccountLoginTipsMsg, OpenAccountLoginTipsDialog);
            Messenger.AddListener<int>(OpenContinueSpinDialogMsg, OpenContinueSpinDialog);
            #endregion

            #region cardSystem
            Messenger.AddListener(OpenCardSystemCollectionDialogMsg, OpenCardSystemCollectionDialog);
            Messenger.AddListener<int,GameObject>(OpenCardSystemGetCardDialogMsg, OpenCardSystemGetCardDialog);
            Messenger.AddListener(OpenCardSystemLuckyDrawDialogMsg, OpenCardSystemLuckyDrawDialog);
            #endregion
			
			#region tips
			Messenger.AddListener<string>(OpenTipsDialogMsg, OpenTipsDialog);
			Messenger.AddListener(OpenTaskTipsDialogMsg, OpenTaskTipsDialog);

            #endregion
            
        }
	}

	void OnDestroy ()
	{
		RemoveListeners ();
	}

	public virtual void RemoveListeners ()
	{
		ListenerAddedCount--;

		if (ListenerAddedCount == 0) {
			Messenger.RemoveListener (OpenSettingDialog, openSettingDialog);
			Messenger.RemoveListener<System.Action,object> (OpenSceneExchange, openSceneExchange);
			Messenger.RemoveListener<System.Action> (OpenBlackOutPanel, openBlackOutPanel);
			Messenger.RemoveListener<int,System.Action,bool,bool> (OpenAutoSpinWinDialog, openAutoSpinWinDialog);
            Messenger.RemoveListener<System.Action>(OpenReSpinStartDialog, openReSpinStartDialog);
            Messenger.RemoveListener<bool>(OpenTerms_EUDialogMsg, OnOpenTerms_EUDialogMsg);

			Messenger.RemoveListener<int,System.Action,bool> (OpenAutoSpinAdditionalDialog, openAutoSpinAdditionalDialog);
			Messenger.RemoveListener<Dictionary<string,object>,System.Action,bool> (OpenFreeSpinEndDialog, openFreeSpinEndDialog);
            Messenger.RemoveListener<long, System.Action>(OpenReSpinEndDialog, openReSpinEndDialog);

			Messenger.RemoveListener<Sprite, int, System.Action>(OpenKindOfSymbolDialog, openKindOfSymbolDialog);
			Messenger.RemoveListener(OpenPowerMachineTipsDialog, openPowerMachineTipsDialog);
            Messenger.RemoveListener<int, System.Action, bool>(OpenHotHotChiliFreeSpinlDialog, openHotHotChiliFreeSpinlDialog);
			Messenger.RemoveListener<long, System.Action>(OpenHotHotChiliFreeSpinWinDialog, openHotHotChiliFreeSpinWinDialog);


			Messenger.RemoveListener<RewardSpinEvent,System.Action> (OpenRewardSpinEndDialog, openRewardSpinEndDialog);
			Messenger.RemoveListener<System.Action> (OpenRewin, openRewin);
			Messenger.RemoveListener (OpenPaytablePanel, openPaytablePanel);
			Messenger.RemoveListener (OpenLoadingWaitDialog, openLoadingWaitingDialog);
			Messenger.RemoveListener <Action>(OpenLoadingWaitDialogCallback, openLoadingWaitCallback);
			Messenger.RemoveListener<Action>(OpenLoadingWaitDialogCallbackAtOnce, openLoadingWaitCallbackAtOnce);
			Messenger.RemoveListener<Action,float>(OpenLoadingWaitDialogCallbackAtOnceWithTime, openLoadingWaitCallbackAtOnceWithTime);

			Messenger.RemoveListener (CloseLoadingWaitDialog, closeLoadingWaitDialog);
			Messenger.RemoveListener<SlotMachineConfig> (OpenRateMachine, this.openRateMachineDialog);
			
			Messenger.RemoveListener(openLoadingDialogMsg, OpenLoadingDialog);

			Messenger.RemoveListener<int, long, System.Action>(OpenJackPotWinDialog_Jackpot777, openJackPotWinDialog_Jackpot777);

			Messenger.RemoveListener<int,long,System.Action > (OpenJackPotMineWinDialog, openJackPotMineWinDialog);
			Messenger.RemoveListener (OpenBetTips, openBetTips);
			Messenger.RemoveListener(OpenAutoQuestCatalogDialog, this.openAutoQuestCatalogDialog);
			Messenger.RemoveListener<Dictionary<string, object>>(OpenAutoQuestDetailDialog, this.openAutoQuestDetailDialog);
			
			
			Messenger.RemoveListener (OPEN_FIRST_GO_LOBBY_DIALOG, openFirstGoLobbyDialog);
			Messenger.RemoveListener<SlotMachineConfig>  (OPEN_HIGH_ROLLER_DIALOG, openHighRollerDialog);
			Messenger.RemoveListener (OPEN_JACKPOT_BET_CHANGE_DIALOG, openJackpotBetChangeDialog);

			Messenger.RemoveListener<SlotMachineConfig, long,bool> (OPEN_JACKPOT_WIN_PUSH, openJackPotMineWinPush);

            
			Messenger.RemoveListener(OpenInvalidCodeDialog, OnOpenInvalidCodeDialog);
			Messenger.RemoveListener(OpenExpirePromoCodeDialog, OnOpenExpirePromoCodeDialog);
			Messenger.RemoveListener(OpenCodeAlreadyClaimedDialogMsg, OnOpenCodeAlreadyClaimedDialogMsg);

			Messenger.RemoveListener (OPEN_TIME_OUT_DIALOG, openTimeOutDialog);
			// Messenger.RemoveListener (OpenBetHigherDialog, openBetHigherDialog);

			Messenger.RemoveListener<System.Action>(OpenSpinLimitDialog, OpenSpinLimimiDialogMsg);
			Messenger.RemoveListener<int,System.Action>(OpenPopRewardSmallDialog, OnOpenPopRewardSmallDialog);
			Messenger.RemoveListener<int,Action>(OpenPopRewardBigDialog, OnOpenPopRewardBigDialog);
			Messenger.RemoveListener(OpenWithDrawDialog, OnOpenWithDrawDialog);
			Messenger.RemoveListener<int>(OpenGetMoreCashDialog, OnOpenGetMoreCashDialog);
			Messenger.RemoveListener(OpenNewUserGuidDialogMsg, OpenNewUserGuidDialog);
			Messenger.RemoveListener<int>(OpenRewardCashDialogMsg, OpenRewardCashDialog);

			
			Messenger.RemoveListener<int,int,System.Action>(OpenSpinWithDrawStartDialogMsg, OpenSpinWithDrawStartDialog);
			Messenger.AddListener<int,System.Action>(OpenSpinWithDrawEndDialogMsg, OpenSpinWithDrawEndDialog);
			Messenger.RemoveListener<int,int>(OpenAccountDialogMsg, OpenAccountDialog);
			Messenger.RemoveListener<int,string,int>(OpenAccountEnsureMsg, OpenAccountEnsureDialog);
			Messenger.RemoveListener<int>(OpenAccountLoginTipsMsg, OpenAccountLoginTipsDialog);
			Messenger.RemoveListener<int>(OpenContinueSpinDialogMsg, OpenContinueSpinDialog);
			
			#region cardSystem
			Messenger.RemoveListener(OpenCardSystemCollectionDialogMsg, OpenCardSystemCollectionDialog);
			Messenger.RemoveListener<int,GameObject>(OpenCardSystemGetCardDialogMsg, OpenCardSystemGetCardDialog);
			Messenger.RemoveListener(OpenCardSystemLuckyDrawDialogMsg, OpenCardSystemLuckyDrawDialog);
			#endregion
			
			#region tips
			Messenger.RemoveListener<string>(OpenTipsDialogMsg, OpenTipsDialog);
			Messenger.RemoveListener(OpenTaskTipsDialogMsg, OpenTaskTipsDialog);
			#endregion
		}
	}

	protected virtual void openSettingDialog ()
	{
	}
	protected virtual void OpenSpinLimimiDialogMsg(System.Action DefeatedAction)
    {
    }

	protected virtual void OpenLoadingDialog() { }
	
	protected virtual void openLobbySlotLoad (List<SlotMachineConfig> slotMachineConfigs, System.Action<string,GameObject> onDownLoadHandler, System.Action onCompleteHandler,string lobbyAssetBundleName)
	{
		if (onCompleteHandler != null) {
			onCompleteHandler ();
		}
	}

	protected virtual void OnOpenTerms_EUDialogMsg (bool IsModel){}

	protected virtual void openSceneExchange (System.Action onDialogClose, object data)
	{
		if (onDialogClose != null) {
			onDialogClose ();
		}
	}

	protected virtual void openBlackOutPanel (System.Action onDialogClose)
	{
		if (onDialogClose != null) {
			onDialogClose ();
		}
	}

	protected virtual void openAutoSpinWinDialog (int times, System.Action onDialogClose, bool isShow,bool newRuleDialog)
	{
		if (onDialogClose != null) {
			onDialogClose ();
		}
	}

    protected virtual void openReSpinStartDialog(System.Action onDialogClose)
    {
        if (onDialogClose != null)
        {
            onDialogClose();
        }
    }

	protected virtual void openAutoSpinAdditionalDialog (int times, System.Action onDialogClose,bool newRuleDialog)
	{
		if (onDialogClose != null) {
			onDialogClose ();
		}
    }

    protected virtual void openHotHotChiliFreeSpinlDialog(int times, System.Action onDialogClose, bool isShow)
    {
        if (onDialogClose != null)
        {
            onDialogClose();
        }
    }

	protected virtual void openHotHotChiliFreeSpinWinDialog(long winCoins, System.Action onDialogClose)
    {
        if (onDialogClose != null)
        {
            onDialogClose();
        }
    }

	protected virtual void openFreeSpinEndDialog (Dictionary<string,object> infos, System.Action onDialogClose,bool newRuleDialog)
	{
		if (onDialogClose != null) {
			onDialogClose ();
		}
	}

    protected virtual void openReSpinEndDialog(long winCoins,  System.Action onDialogClose)
    {
        if (onDialogClose != null)
        {
            onDialogClose();
        }
    }


    protected virtual void openKindOfSymbolDialog(Sprite sprite, int count,  System.Action onDialogClose)
    {
        if (onDialogClose != null)
        {
            onDialogClose();
        }
    }

    protected virtual void openPowerMachineTipsDialog()
    {
    }
    
    protected virtual void openRewardSpinEndDialog(RewardSpinEvent rse,System.Action onDialogClose){
		if (onDialogClose != null) {
			onDialogClose ();
		}
	}
    #region ClubSystem
	protected virtual void openLevelUpDialog (int level, System.Action onDialogClose,int clubAddPoints)
	{
		if (onDialogClose != null) {
			onDialogClose ();
		}
	}
	#endregion
	protected virtual void OnOpenVipLevelUpDialog (int preLevel,int curlevel){}
	protected virtual void GiftBoxRewardDialog (NormalCommandItem commandItem){}
	protected virtual void LevelUpWithBoostBoxDialog (NormalCommandItem commandItem){}

	protected virtual void openRewin (System.Action onDialogClose)
	{
		if (onDialogClose != null) {
			onDialogClose ();
		}
	}

	protected virtual void openPaytablePanel ()
	{
	}
	
	protected virtual void closeLoadingWaitDialog(){

	}
	
	protected virtual void openLoadingWaitCallback(Action callback){

	}
	
	protected virtual void openLoadingWaitCallbackAtOnce(Action callback){
		
	}

	protected virtual void openLoadingWaitCallbackAtOnceWithTime(Action callback, float time)
	{
		
	}
	
	protected virtual void openLoadingWaitingDialog(){

	}
	
	protected virtual void openRateMachineDialog(SlotMachineConfig slotMachineConfig)
	{

	}

    protected virtual void openJackPotWinDialog_Jackpot777(int winNumber, long winValue, System.Action callback)
    {
       
    }

	protected virtual void openJackPotMineWinDialog(int winNumber, long winValue,System.Action callBack)
	{

	}

	protected virtual void openBetTips()
	{

	}

	protected virtual void openPushPromptTextDialog (NormalCommandItem sci)
	{

	}

	protected virtual void openAutoQuestCatalogDialog() {

	}

	protected virtual void openAutoQuestDetailDialog(Dictionary<string, object> autoQuestParameters) {

	}

	protected virtual void openFbLoginUI(Dictionary<string,object> info = null)
	{

	}

	protected virtual void openFbInvite()
	{

	}

	protected virtual void openRtotDialog(Dictionary<string, object> rtotDict) {

	}

	protected virtual void vipUnlockDialog(int vipLevel)
	{

	}

	protected virtual void epicWinDialog()
	{

	}

	protected virtual void openFirstGoLobbyDialog()
	{

	}
	

	protected virtual void openHighRollerDialog(SlotMachineConfig _config)
	{

	}

	protected virtual void openJackpotBetChangeDialog()
	{

	}
	

	protected virtual void openJackPotMineWinPush (SlotMachineConfig slotConfig, long winValue,bool isMine)
	{

	}

	protected virtual void OnRefrashFB()
	{
	}
	protected virtual void OnOpenPopup1Dialog(NormalCommandItem rewardItem)
	{

	}

	protected virtual void OnOpenPopupDealDialogMsg(NormalCommandItem rewardItem)
	{

    }

	protected virtual void OnOpenPopupTXT1Dialog(NormalCommandItem rewardItem)
	{

    }

	protected virtual void OnOpenRechargeAwardDialog(NormalCommandItem rewardItem)
    {

    }

	protected virtual void OnOpenInvalidCodeDialog()
	{

	}

	protected virtual void OnOpenExpirePromoCodeDialog()
	{

	}

	protected virtual void OnOpenCodeAlreadyClaimedDialogMsg()
	{

	}
	#region ClubSystem
	protected virtual void openSmallLevelUpDialog(int level,System.Action onDialogClose,int clubAddPoints)
	{

	}
	#endregion

	protected virtual void openTimeOutDialog()
	{

	}

	protected virtual void OnOpenPopRewardSmallDialog(int cash,Action callback)
	{
		
	}
	protected virtual void OnOpenPopRewardBigDialog(int cash,Action closeCallBack)
	{
		
	}
	protected virtual void OnOpenGetMoreCashDialog(int cash)
	{
		
	}
	protected virtual void OnOpenWithDrawDialog()
	{
		
	}
	protected virtual void OpenNewUserGuidDialog()
	{
		
	}
	
	protected virtual void OpenSpinWithDrawStartDialog(int spinCount,int cash,Action callback)
	{
		
	}
	protected virtual void OpenSpinWithDrawEndDialog(int cash,Action callback)
	{
		
	}

	protected virtual void OpenAccountDialog(int index,int cash)
	{
		
	}
	protected virtual void OpenAccountEnsureDialog(int index,string account,int cash)
	{
		
	}
	protected virtual void OpenCardSystemCollectionDialog()
	{
		
	}
	protected virtual void OpenCardSystemGetCardDialog(int cardId,GameObject cardBtn)
	{
		
	}
	protected virtual void OpenCardSystemLuckyDrawDialog()
	{
		
	}
	
	protected virtual void OpenRewardCashDialog(int cash)
	{
		
	}
	
	protected virtual void OpenContinueSpinDialog(int activityId)
	{
		
	}
	
	protected virtual void OpenTipsDialog(string text)
	{
		
	}
	
	protected virtual void OpenTaskTipsDialog()
	{
		
	}
}
