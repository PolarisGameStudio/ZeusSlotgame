using UnityEngine;
public static class GameConstants
{
	public static string[] AssetBundleExcludeScenes = {"EmptyScene", "Loading", "ClassicLobby"};
	public const string GetBuffConfigEndKey = "GetBuffConfigEnd";
	public const string ExitPopRewardState = "ExitPopRewardState";
	public const string RefreshCashInFree = "RefreshCashInFree";
	
    #region GoodSKU相关

    public const string OnGoodCoinPaymentSucceeded = "OnGoodCoinPaymentSucceeded";
    public const string COIN_199 = "coin_199";
    #endregion

    #region Uimanage.event

    public const string SHOW_MACHINE_SYSTEM_UI_KEY = "ShowMachineSystemUI";
    public const string CHECK_UP_UI_EXCEPTION_KEY = "CheckUpUIException";
    public const string ON_UIEVENT_SEQUENCE_EMPTY_KEY = "OnUIEventSequenceEmpty";
    public const string GET_UI_EVENT_NUM_KEY = "GetUIEventNum";
    public const string LEFT_UI_CREATE_SUCCEED = "LeftUICreateSucceed";
    public const string CORNER_UI_CREATE_SUCCEED = "RightUICreateSucceed";

    #endregion
    #region Loading Scene
    public const string LOADING_BASE_PATH = "Anchor_Landscape/Panel/";
    #endregion
   
    #region FlyCoinsPanel

    public const string GetTopPanelScaleAdaption = "GetTopPanelScaleAdaption";

    #endregion
	
	public const string LOBBY_NAME = "ClassicLobby";
	public const string Slot_NAME = "WesternTreasure";

	public const string BonusGameIsOver = "BonusGameIsOver";
	public const string OnGameConsoleStarted = "OnGameConsoleStarted";
	public const string OnGameConsoleReStart = "OnGameConsoleReStart";
	

	public const string CloseInboxDialog = "CloseInboxDialog";

	public const string CollectBonusWithType = "CollectBonusWithType";
	public const string CollectBonusWithTypeAndTarget = "CollectBonusWithTypeAndTarget";
	public const string CollectBonusFromInputMouse = "CollectBonusFromInputMouse";
	public const string CollectCashWithTypeAndTarget = "CollectCashWithTypeAndTarget";

	public const string OnApplicationResume ="OnApplicationResume";
	public const string OnSceneInit = "OnSceneInit";
	public const string OnH5InitSuccess = "OnH5InitSuccess";
	public const string OnExitH5 = "OnExitH5";
	public const string OnSlotMachineSceneInit = "OnSlotMachineSceneInit";
	public const string OnSlotMachineSceneQuit = "OnSlotMachineSceneDestroy";
	
	public const string OnLevelChange = "OnLevelChange";
	
	public const string SPINNOMONEY="SpinNoMoney";
	public const string DO_SPIN = "DoSpin";
	public const string NEW_USER_GUIDE = "NewUserGuide";
	public const string ShowButtonMask = "ShowButtonMask";
	public const string ChangeMaskOrder = "ChangeMaskOrder";
    
    public const string SpinAwardEndMsg = "SpinAwardEndMsg";
	
	public const string ChangeGameConfigsMsg = "ChangeGameConfigsMsg";

	public const string DataMiningRecordUnlockedMachinesOnLevelUp = "DataMiningRecordUnlockedMachinesOnLevelUp";

	public const string ENABLE_FAST_STOP = "EnableFastStop";

	
    public const string NOW_SPIN_CLICK = "NOW_SPIN_CLICK";//freespin 中是forbidden spin，而normal状态是正常spin。后全改为每次spin触发一次
    public const string PressedFastButton = "PressedFastButton";
    public const string ON_BET_CHANGE_SUCCEED = "ON_BET_CHANGE_SUCCEED";
    public const string DeletePrefsCommand_key = "DeletePrefsCommand";
    public const string NetworkErrorAlertInLogin_Show = "NetworkErrorAlertInLogin_Show";

    public const string SWAP_SCENE_NEED_DO = "SWAP_SCENE_NEED_DO";
    public const string LOBBY_CONTROLLER_START_OVER = "LOBBY_CONTROLLER_START_OVER";
    public const string SLOT_CONTROLLER_START_OVER = "SLOT_CONTROLLER_START_OVER";

	public const int DEFAULT_SPIN_NUM = 20;
	public const string Time_N_D_B_Key = "{0} d";

	#region AD
	public const string ADPlaySuccess = "ADPlaySuccess";
	#endregion
	
	public const string MachineOutofCoins = "MachineOutofCoins";
	
	public const string InboxOffer = "InboxOffer";
	
	public const string EVENT_ID = "EventID";
	
	
	public const string IntervalBackToApp = "IntervalBackToApp";
	
    #region  Event message

    public const string NeverClickedSpin = "Never_Clicked_spin";

    public const string RemoteConfigChange = "Remote_Config_Change";

    public const string OnSessionStart = "OnSessionStart";

    public const string OnSessionEnd = "OnSessionEnd";

    public const string NetworkSpinResponse = "NetworkSpinResponse";

    public const string NetwokDisconnect = "NetwokDisconnect";

    #endregion
	public const string MISSION_PROGRESS="MISSION_PROGRESS";

	public const string AutoQuestSavedDataLoadFinish = "AutoQuestSavedDataLoadFinish";
	public const string AutoQuestDoCurrentProcessingQuestTermItem = "AutoQuestDoCurrentProcessingQuestTermItem";
	public const string AutoQuestCurrentProcessingQuestTermItemsDone = "AutoQuestCurrentProcessingQuestTermItemsDone";
	public const string AutoQuestCurrentProcessingQuestTermDone = "AutoQuestCurrentProcessingQuestTermDone";
	public const string AutoQuestCurrentProcessingQuestChanged = "AutoQuestCurrentProcessingQuestChanged";
	public const string AutoQuestDetailDialogWillOpenForQuestDone = "AutoQuestDetailDialogWillOpenForQuestDone";
	public const string AutoQuestDetailDialogWillOpenForNewQuest = "AutoQuestDetailDialogWillOpenForNewQuest";
	public const string AutoQuestCloseAllDialogs = "AutoQuestCloseAllDialogs";
	public const string AutoQuestCatalogDialogShowContentPanel = "AutoQuestCatalogDialogShowContentPanel";

	public const string NETWORK_CHANGE = "NETWORK_CHANGE";
	public const string Localload_ = "Localload_";

	public const string FIRST_GO_LOBBY_PROGRESS="FirstGoLobbyProgress";
	public const string HIDE_DEFAULT_LOAING_IMAGE="HideDefaultLoadingImage";
	public static WaitForSeconds OneSecondWait =	new WaitForSeconds (1f);
    public static WaitForSeconds FiveSecondWait =	new WaitForSeconds (5f);
	public static WaitForSeconds FiveIn10SecondWait =	new WaitForSeconds (0.5f);
	public static WaitForSeconds FiveIn100SecondWait =	new WaitForSeconds (0.05f);
    public static WaitForSeconds TwoIn10SecondWait =	new WaitForSeconds (0.2f);
	public static WaitForEndOfFrame FrameTime =	new WaitForEndOfFrame();
	public const string ESMsgDict_Key = "ESMsgDict";
	public const string ICommandEvent_Key = "ICommandEvent";
	public const string Event_Key = "event";
	public const string Event_Object_Key = "this";
	public const string ParaDict_Key = "paraDict";
	public const string EventDict_Key = "eventDict";

	public const string HTTP_Key = "http://";
	public const string HTTPS_Key = "https://";

	public const string SHOW_PIGGY_TIPS_PANEL = "SHOW_PIGGY_TIPS_PANEL";

    #region CARD PACKAGE TRIGGER TYPE
    
    public const string CARD_PACKAGE_TRIGGER_TYPE_POWER_MACHINE = "POWER-MACHINE";
    public const string CARD_PACKAGE_TRIGGER_TYPE_CARD_ROOKIE = "CARD-ROOKIE";
    
    public const string IGNORE_TRIGGER_COUNT ="ignoreTriggerCount";

    public const string CARD_PACKAGE_TYPE_WILD = "wild";
    public const string CARD_PACKAGE_TYPE_POWER = "power";
    public const string CARD_PACKAGE_TYPE_COMMON = "common";
    public const string CARD_PACKAGE_TYPE_GOLD = "gold_puzzle";

    public const string DIALOG_CLOSE_AWARD_HANDLER = "NextRewardHandler";
    public const string REMOVE_REWARD_COMMAND = "RemoveRewardCommand";

    public const string AFFECTED_SET_ID = "set_id";
    public const string AFFECTED_ALBUM_ID = "album_id";
    public const string AFFECTED_COLLECTION_ID = "collection_id";
    public const string REWARD_CARD_ID = "card_id";
    public const string REWARD_COUNT = "count";
    public const string EXTRA_DICT_KEY = "extraDict";
    public const string TRIGGER_SHOW_KEY = "triggerShow";
    public const string REWARD_COINS_TYPE = "reward_coins_type";
    public const string CREATE_TIME_KEY = "create_ts";
    public const string INCREASE_STARS = "increase_stars";
    #endregion
    #region PersonalizedConfig
    
    public const string Query_Key = "query";
    public const string Query_Collection_Key = "collection";//启动或者切换账号时，带回数据
    public const string Query_Power_Machine_Game_Key = "power_machine_game";
    public const string Query_Power_Machine_Game_Config_Key = "power_machine_game_config";
    public const string Album_Configs_Key = "album_configs";
    public const string Album_States_Key = "album_states";
    public const string Album_Benefits_Key = "album_benefits";
    public const string Collection_Id_Key = "id";
    public const string Collection_Albums_Key = "albums";
    public const string Collection_Card_Status_Key = "card_status";
    public const string Collection_AlbumCycle_Count_Key = "cycle_num";
    public const string Collection_Card_Status_Collected_Key = "collected";
    public const string Collection_Card_Status_Total_Count_Key = "total";
    public const string Collection_Close_Time_Key = "close_ts";
    public const string Collection_Status_Key = "state";
    
    public const string Crafting_Game_Key = "crafting_game";

    public const string Crafting_Collection_Id_Key = "collection_id";

    public const string Crafting_WHeel_Result_Game_Id_Key = "game_id";
  
    public const string Crafting_WHeel_Result_Wheel_Id_Key = "wheel_id";
    public const string Crafting_WHeel_Result_Wedge_Id_Key = "wedge_id";
    public const string Crafting_WHeel_Result_Rewards_Key = "rewards";
    

    public const string Power_Machine_Games_Key = "games";
    public const string Power_Machine_Game_ID_Key = "game_id";
    public const string Power_Machine_Expired_Time_Key = "expired_at";
    public const string Power_Machine_Game_Card_Id_Key = "card_id";
    public const string Power_Machine_Game_Type_Key = "type";
    public const string Power_Machine_Game_Rewards_Key = "rewards";
    
    public const string REFRESH_CRAFTING_PREVIEW_INFO = "RefreshCraftingPreviewInfo";
    public const string REFRESH_CRAFTING_LUCKY_WHEEL_WEDGES_INFO = "RefreshCraftingLuckyWheelWedgesInfo";
    #region RequestParam
    public const string Query_Dict_Key = "query_dict";
    public const string Query_Dict_Hash_Key = "hash";
    public const string Album_ID_Key = "id";

    #endregion

    #region ResponseParam
    
    #region AlbumConfig
    public const string Album_Name_Key = "name";
    public const string Album_Start_Time_Key = "start_ts";
    public const string Album_End_Time_Key = "expiration_ts";
    public const string Album_Config_Hash_Key = "hash";
    public const string Album_Sets_Key = "sets";
    public const string Album_Image_Url_Key = "image_url";
    public const string Album_Title_Logo_Url_Key = "title_logo_url";
    public const string Album_Reward_Key = "reward";
    public const string ALBUM_STATE_UPDATE_KEY = "update_ts";
    #endregion

    #region SetsConfig
    public const string Set_ID_Key = "id";
    public const string Set_Name_Key = "name";
    public const string Set_Description_Key = "description";
    public const string Set_Background_Url_Key = "background_url";
    public const string Set_Card_Frame_Color_Key = "card_frame_color";
    public const string Set_Default_Card_Background_Key = "default_card_background_url";
    public const string Set_Cards_Key = "cards";
    public const string Set_Image_Url_Key = "image_url";
    public const string Set_Title_Logo_Url_Key = "title_logo_url";
    public const string Set_Reward_Key = "reward";
    public const string Set_Album_ID_Key = "album_id";

    #endregion

    #region CardConfig
    public const string Card_ID_Key = "id";
    public const string Card_Name_Key = "name";
    public const string Card_Type_Key = "type";
    public const string Card_Rareness_Key = "rareness";
    public const string Card_IsTradable_Key = "is_tradable";
    public const string Card_IsNew_Key = "is_new";
    public const string Card_Count_Key = "count";
    public const string Card_Description_Key = "did_you_know";
    public const string Card_Image_Url_Key = "image_url";
    public const string Card_HowToGet_Key = "how_to_get";
    public const string Card_Set_ID_Key = "set_id";
    public const string Card_First_Card_History_Key = "first_card_history";
    public const string Card_First_Collected_Time_Key = "ts";
    public const string Card_First_Collected_Position_Key = "source_name";
    #endregion
    #endregion

    #endregion

    public const string UPDATE_CLIENT_TIME = "UpdateClientTime";
    #region SingletonManager
    public const string INIT_ALBUM_CONTROLLER = "InitAlbumController";

    public const string INIT_POWER_MACHINE_CONTROLLER = "InitPowerMachineController";
    public const string KILL_POWER_MACHINE_CONTROLLER = "KillPowerMachineController";

    public const string KILL_SERVER_MODULE_CONFIG_LOADER = "KillServerModuleConfigLoader";

    public const string CLOSE_ALL_COLLECTION_DIALOG = "CloseAllCollectionDialog";
    public const string Update_Album_Config = "UpdateAlbumConfig";
    public const string Update_Album_State = "UpdateAlbumState";
    public const string Update_Album_Benefits = "UpdateAlbumBenefits";
    public const string UPDATE_COLLECTION_STATE = "UpdateCollectionState";

    public const string QUERY_POWER_MACHINE_INFO = "QueryPowerMachineInfo";
    public const string QUERY_POWER_MACHINE_ASSET_STATE = "QueryPowerMachineAssetState";
    public const string REFRESH_POWER_MACHINE_INFO = "RefreshPowerMachineInfo";
    public const string GET_ALBUM_CARDS_DICT = "GetAlbumCardsDict";
    public const string REFRESH_LOBBY_ALBUM_ICON = "RefreshLobbyAlbumIcon";
   
    public const string REFRESH_POWER_MACHINE_GAME_ICON = "RefreshPowerMachineGameIcon";

    public const string CARD_TYPE_SIMPLE = "Simple";
    public const string CARD_TYPE_GOLD = "Gold";
    public const string CARD_TYPE_POWER = "Power";
    public const string CARD_TYPE_WILD = "Wild";
   

    public const string PAGE_UP_SET = "PageUpCardSet";
    public const string PAGE_DOWN_SET = "PageDownCardSet";

    public const string QUERY_COLLECTION_INFO = "Query_Collection_Info";
    
    public const string GET_CARD_PACKAGE_DATA = "GetCardPackageData";
    public const string GET_ALBUMS_DATA = "GetAlbumsData";
    public const string REWARD_CARD_ID_KEY = "card_id";
    public const string UPDATE_ALBUM_CARD_STATE = "UpdateAlbumCardState";
    public const string UPDATE_ALBUM_CARD_NEW_STATE = "UpdateAlbumCardNewState";
    public const string GET_LASTEST_ALBUM_DATA = "GetLastestAlbumData";
    public const string REWARD_ACK_RESPONSE_SUCCESS = "RewardAckResponseSuccess";
    public const string REWARD_ACK_RESPONSE_FAIL = "RewardAckResponseFail";
    public const string GET_SPECIAL_ALBUM_DATA = "GetSpecialAlbumData";
    
    public const string POWER_MACHINE_REWARD_TYPE_JACKPOT_COINS = "jackpot_coins";
    public const string POWER_MACHINE_REWARD_TYPE_GOLD_PUZZLE = "gold_puzzle";
    public const string POWER_MACHINE_REWARD_TYPE_CARDPACKAGE = "cards";
    public const string POWER_MACHINE_REWARD_TYPE_COINS_AND_RESPIN = "respin/coins";
    public const string POWER_MACHINE_REWARD_TYPE_COINS = "coins";
    #region ClubSystem
    public const string POWER_MACHINE_REWARD_TYPE_CLUB_POINTS = "club_point";
    public const string from_level_key = "from_level";
    public const string CLUB_CASHBACK_TASK_CREATED = "ClubCashBackTaskCreated";
    #endregion

    #region JackpotSystem

    public const string OpenSuperJackpotEntranceDialog = "OpenSuperJackpotEntranceDialog";

    #endregion
    
    
    #endregion
    

    #region trigger event
    public const string TRIGGER_EVENT_ENTER_MACHINE_KEY = "enter_machine_event";
    public const string TRIGGER_EVENT_EXIT_MACHINE_KEY = "exit_machine_event";
    public const string TRIGGER_EVENT_PURCHASE_EVENT = "purchase_event";
    public const string TRIGGER_EVENT_LEVELUP_EVENT = "levelup_event";
	public const string TRIGGER_EVENT_APPOPEN_EVENT = "app_open";
	public const string TRIGGER_EVENT_VIPLEVELUP_EVENT = "viplevelup";
	public const string TRIGGER_EVENT_DailyBonus_EVENT = "dailybonus";
	public const string TRIGGER_EVENT_HourlyBonus_EVENT = "hourlybonus";
	public const string TRIGGER_EVENT_OUTOFCOINS_EVENT = "outofcoins";
	public const string TRIGGER_EVENT_AFTERREWARDVIDEO_EVENT = "afterrewardvideo";
	public const string TRIGGER_EVENT_TreasureChestCollect_EVENT = "TreasureChestCollect";
    public const string TRIGGER_EVENT_IDLE_REWARD_AD = "AdsReady";
    public const string TRIGGER_EVENT_USER_STATUS_KEY = "user_status";

    #endregion

	public const string AckingCommand_Key = "RemoveRewardInfo";
	public const string AckingCommand_Token = "token";
	public const string AckingCommand_Statue = "statue";


	public const string Token_Key = "token";
	#region MyRegion
	public const string ValidateSuccessMsg = "ValidateSuccessMsg";
	public const string BindSuccessMsg = "BindSuccessMsg";

	#endregion

	public const string CheckCreateInboxCollectEmailMsg = "CheckCreateInboxCollectEmailMsg";

	#region Command Relation
	public const string SPIN_END="SpinEnd";
	public const string ENTER_LOBBY ="EnterLobby";
	public const string BACK_TO_APP ="BackToApp";
	public const string LEVEL_UP_END ="LevelUpEnd";
	public const string ENTER_MACHINE = "EnterMachine";
	public const string BACK_TO_LOBBY="BackToLobby";
	public const string EPIC_WIN = "EpicWin";
	public const string BIG_WIN = "BigWin";
	public const string AFTER_BIG_WIN = "AfterBigWin";
	public const string AFTER_MEGA_WIN = "AfterMegaWin";
	public const string AFTER_EPIC_WIN = "AfterEpicWin";
	public const string CLOSE_SHOP_DIALOG ="CloseShop";
	public const string OUTOF_COINS ="OutOfCoins";
	public const string OPEN_SPIN_WIN_DIALOG = "OpenSpinWin";
	public const string CLOSE_SPIN_WIN_DIALOG = "CloseSpinWin";
	public const string OPEN_BIG_WIN_DIALOG = "OpenBigWin";
	public const string CLOSE_BIG_WIN_DIALOG ="CloseBigWin";
	public const string OPEN_MEGA_WIN_DIALOG = "OpenMegaWin";
	public const string CLOSE_MEGA_WIN_DIALOG ="CloseMegaWin";
	public const string OPEN_EPIC_WIN_DIALOG = "OpenEpicWin";
	public const string CLOSE_EPIC_WIN_DIALOG = "CloseEpicWin";
	public const string CLOSE_OUTOF_COINS_DIALOG ="CloseOutOfCoins";
	public const string OnCloseBigRewardVideo = "OnCloseBigRewardVideo";
	public const string Trigger_Key = "trigger";
	#endregion

    public const string BundleDownloadSuccess_ESName = "BundleDownloadSuccess";
    public const string BundleDownloadFailed_ESName = "BundleDownloadFailed";
    public const string BundleStartDownload_ESName = "BundleStartDownload";
	public const string ResourceName_Key = "resourceName";

	public const string Asset_Key = "Asset";
	public const string AssetName_Key = "name";
	public const string AssetVersion_Key = "version";
	public const string AssetBlundleInfo_Key = "AssetBlundleInfo";
	public const string Priority_Key = "Priority";
	public const string Conditions_Key = "Conditions";
	public const string AssetMultipler_Key = "Multiplier";
	public const string ShopIsNew_Key = "IsNewShop";
	public const string ShopItems_KEY = "ShopItems";
	public const string ShopItems_RewardConfig_key = "/ShopConfig/ShopRewardConfig";
	public const string ShopItemsEvent_RewardConfig_Key = "ShopItemsEvent/ShopConfig/ShopRewardConfig";
	public const string ShopItemsShow_Key = "ShopItemsShow";
	
	public const string OptimizeShopItems_KEY = "OptimizeShopItems";
	public const string CloseOptimizeShopDialog_KEY = "CloseOptimizeShop";

	public const string IsShowWelcomeOnce_Key = "IsShowWelcomeOnce";
	public const string IsCanShowWelcomeBack_Key = "IsCanShowWelcomeBack";
	public const string IsCheckFeature_Key = "IsCheckFeature";
	public const string RequireSpinNum_Key = "RequireSpinNum";

	#region MyRegion
	public const string ShopFeature_Key = "ShopFeature";
	public const string PiggyScaleFeature_Key = "PiggySale";
	public const string ShopDialog_Key = "ShopDialog";
	public const string NewShopDialog_Key = "NewShopDialog";
	public const string RateAlertDialog_Key = "RateAlertDialog";
    public const string RateAlertThankDialog_Key = "RateAlertThankDialog";
	public const string ShopPromotionDiscount_Key = "ShopPromotionDiscount";
    public const string ShopRewardSprite_Key = "ShopRewardSprite";


	public const string RemoteR_Key = "RemoteR";
	#endregion

    public const string ClosePurchaseSuccessfulMsg = "ClosePurchaseSuccessfulMsg";

	public const string TimeDown_Key = "{0:D2}:{1:D2}:{2:D2}";


    public const string LocalFeature_Key = "localFeature";
    public const string ShowTxt_Key = "showtxt";

	public const string PushData_Key = "pushData";

	public const string InboxDeal_BG_Key = "inbox_SpecialOffer_01";

	//AdapterName
	public const string AdapterName_Coins_Key = "coins";
	public const string AdapterName_SpinNum_Key = "spinNum";

    public const string ADWinCoins_key = "ADWincoins";
    public const string NewDesc_key = "NewDesc";
    public const string OpenInstoreEvent_Key = "OpenInstoreEvent";
    public const string OpenURLEvent_Key = "OpenURLEvent";
    public const string SimulateClickShopEvent_Key = "SimulateClickShopEvent";
    public const string OpenEventCmdShopEvent_Key = "OpenEventCmdShopEvent";
    public const string OpenRewardAssetBundleDialogEvent_Key = "OpenRewardAssetBundleDialogEvent";
	public const string OpenRateMachineEvent_KEY = "OpenRateMachineEvent";
	public const string OpenWelcomeEvent_KEY = "OpenWelcomeEvent";
	public const string OpenWelcomeBackEvent_KEY = "OpenWelcomeBackEvent";
    public const string ChangeLevelPlanEvent_KEY = "ChangeLevelPlanEvent";
    public const string CollectQuestPointEvent_KEY = "CollectQuestPointEvent";
    public const string AddGiftItemEvent_KEY = "AddGiftItemEvent";
	public const string CloseInboxAndPushEvent_Key = "CloseInboxAndPushEvent";
	public const string RequestRelativetimeRewardEvent_Key = "RequestRelativetimeRewardEvent";

    public const string CollectSymbolDecorationTask = "CollectSymbolDecorationTask";

    public const string AcceptCommandMsg = "AcceptCommandMsg";
    public const string AckCommandMsg = "AckCommandMsg";

	public const string NotificateMsgEvent_Key = "NotificateMsgEvent";
	public const string NOURLEvent_Key = "No URL ";
	public const string URL_Key = "url";


	public const string LocalFeatur_Key = "LocalFeature_";
	public const string DownloadSlot = "DownloadSlot";
	public const string DownloadSlot_Accept = "DownloadSlot_Accept";

    public const string SpecialOfferDialogConfig_Key = "Module/SpecialOfferDialogConfig";
    public const string ResourceLibrary_Key = "ResourceLibrary";
	public const string ResourceElements_Key = "Elements";
	public const string ResourceConfigs_Key = "Configs";
	public const string ResourcePreDownload_Key = "preDownload";
	public const string ResourceBlockLoad_Key = "blockLoad";
	public const string ResourcePriority_Key = "priority";

	public const string USE_NEW_ASSET_HANDLE_KEY = "useNewAssetHandle";
	public const string USE_NEW_ASSET_HANDLE_FOR_2_KEY = "useNewAssetHandleFor2";
	
	public const string InboxItemExpire_Key = "<color=#FFC517FF>Expires in </color>";

	public const string SubType_Key = "subType";
	public const string BtnTxt_Key = "BtnTxt";
	public const string btnTxt_Key = "btnTxt";
    public const string coinTxt_Key = "coinTxt";
    public const string EventToken_Key = "eventToken";
    public const string rewards_Key = "rewards";
    public const string desc_Key = "desc";
    public const string SystemSourceType_Key = "systemSource";

    public const string AppOpen_Key = "AppOpen";
	public const string MachineBacktoLobby_Key = "MachineBacktoLobby";
	public const string LobbyClickIcon_Key = "LobbyClickIcon";


	public const string UnlockSlotInfoByEvent = "UnlockSlotInfoByEvent";
	public const string UnlockSlotInfoForever = "ForeverUnlockSlotInfo";
	public const string QueryServerConfig_Key = "QueryServerConfig";

	public static string UpdateAccountDataMsg ="UpdateAccountDataMsg";
	public const string SignUpSucceedMsg = "SignUpSucceedMsg";
	public const string CheckInboxCouponLabelMsg = "CheckInboxCouponLabelMsg";
    public const string RemoveInboxItemByToken = "RemoveInboxItemByToken";
    public const string RequestGiftBoxFailedMsg = "RequestGiftBoxFailedMsg";
    public const string RequestUseItemFailedMsg = "RequestUseItemFailedMsg";
    public const string RequestDeleteItemFailedMsg = "RequestDeleteItemFailedMsg";
    public const string AddQuestPointMsg = "AddQuestPointMsg";

	public const string _TriggerCount = "_PopupCount";
	public const string _LastTriggerTime = "_LastPopupTime";

	public const string LevelChangeMsg = "LevelChangeMsg";
	
	#region Command Type
	public const string CommandType_Key = "type";
	public const string NormalCommand_Key = "normalCmd";
	public const string EventCommand_Key = "eventCmd";
	#endregion

	#region CommandItem
    public const string adWinCoins_Key = "adWinCoins";
    public const string PushNormalCommand_Key = "pushNormal";
    public const string ReclaimCoinsCommandItem_Key = "reclaimCoins";
    public const string inboxPrefab_Key = "inboxPrefab";
    public const string big_Key = "big";
    public const string normal_Key = "normal";
    public const string giftBox_Key = "giftBox";
    public const string email_Key = "email";
    public const string luaPrefab_Key = "luaPrefab";
    public const string luaPrefabPath_Key = "luaPrefabPath";
    public const string luaPrefabName_Key = "luaPrefabName";
	#endregion

	public const string InboxItem_Key = "InboxItem";
    public const string TaskUltimateAward_LocalFeature = "TaskUltimateAward";

	public const string Title_Key = "title";
	public const string CanClose_Key = "canClose";
    public const string hideExpiration_Key = "hideExpiration";
	public const string BackGroundPic_Key = "bg_pic";
	public const string BackGroundPicUrl_Key = "bg_url";
	public const string ShowItemIcon_Key = "canShowItemIcon";
	public const string InboxType_Key = "inbox_type";
	public const string InboxType_NotShowDefaultImage_Value = "notShowDefaultImage";
	public const string InboxType_Coins_Value = "coins";
	public const string InboxType_FreeSpin_Value = "freespin";
	
	public const string InboxType_MiniGame = "minigame";
	public const string InboxType_Common = "common";

	public const string InboxBg_Coupon = "inbox_coupon";
	public const string InboxBg_Coins = "inbox_coins";
	public const string InboxBg_Freespin = "inbox_free_spin";
	public const string InboxBg_GiftBox = "inbox_gift_box";
	public const string InboxBg_Common = "inbox_common";
	public const string InboxBg_PowerCardCoins = "inbox_power_card_coins";
	public const string InboxBg_ManualCardPackage = "inbox_manual_card_package";
	public const string InboxBg_AdsCardPackage = "inbox_ads_card_package";
	public const string AcceptBtnText_Key = "btnText";
	public const string Reward_Default_Icon_Key = "rewardicon_coin1";
	public const string Reward_Default_Slot_Key = "rewardslot_default";
	public const string AdapterName_Key = "adapter";

    public const string taskInfoImg_Key = "taskInfoImg";
	public const string RewardToken_Key = "reward_token";
	public const string RewardName_Key = "reward_name";
	public const string Icon_Key = "icon";
	public const string Subtitle_Key = "subTitle";
	public const string Description_Key = "desc";
	public const string AwardTriggerType_Key = "award_trigger_type";
	public const string CampaignIds_Key = "campaignIds";
    public const string ShowDialogName_Key = "showDialogName";
    
    public const string TRIGGER_TYPE_KEY = "trigger_type";
    public const string CARD_PACKAGE_TITLE_DESC_KEY = "desc";
    
    public const string CornerDict_Key = "CornerDict";
    public const string InboxItemDict_Key = "InboxItemDict";
    public const string AdDict_Key = "AdDict";
    public const string AdScrollPanelDict_Key = "AdScrollPanelDict";
	public const string ChannelName_Key = "ChannelName";
	public const string MinVersion_Key = "version_min";
	public const string MaxVersion_Key = "version_max";
	public const string name_Key = "name";
	public const string Name_Key = "Name";
	public const string OldPrice_Key = "oldPrice";
	public const string CreateTime_Key = "create_time";
	public const string Expiration_Key = "expiration";
    public const string StartShowTime_Key = "startShowTime";
	public const string IsCreateByApp_Key = "createByApp";
	public const string IsBtnShow_Key = "IsBtnShow";
	public const string IsTop_Key = "IsTop";
	public const string IsStoreInbox_Key = "storeInbox";
    public const string IsSendACKByAccept_Key = "acceptAck";
    public const string Multiple_Key = "multiple";
    public const string Specialmultiple_Key = "specialmultiple";
    public const string SpecialLevelValue_Key = "specialLevelValue";
    public const string OfferData_Key = "offer_data";
    public const string OfferType_Key = "offer_type";
    public const string OfferIndex_Key = "offerIndex";
    public const string OfferFree_Key = "Free";
    public const string OfferTreasureBox_Key = "IsTreasureBox";

    public const string featureActivated_Key = "featureActivated";
	public const string noActivated_Value = "noActivated";
	public const string DailyADName_Key = "dailyAD";
    public const string coupon_Value = "coupon";
    public const string Coupon_Value = "Coupon";
    public const string LocalCouponDict_Key = "LocalCouponDict";
    //public const string mul_Prefix = "mul";

    public const string shopCoupon_Value = "shopCoupon";
    public const string addDiscount_Value = "addDiscount";
    public const string mulDiscount_Value = "mulDiscount";

    public const string mulPromoType_Value = "mulPromoType";
    public const string addPromoType_Value = "addPromoType";

    public const string curShopCoupon_Key = "curShopCoupon";
    public const string UpdateCurrentCouponMsg = "UpdateCurrentCouponMsg_";

	public const string BtnAccept_Key = "BtnAccept";
	public const string BtnDiscard_Key = "BtnDiscard";

	public const string Interval_Key = "Interval";

    public const string PushNormal = "PushNormal_";
    public const string rewardGame_Key = "rewardGame";


	public const string MaxCount_Key = "MaxCount";
	public const string MinCount_Key = "MinCount";

	public const string ActiviesNames_Key = "ActiviesNames";
	public const string ActiviesTokens_Key = "ActiviesTokens";



	public const string RookieOfferOldCoinsText_Key = "was {0} coins";
	public const string UnlockSlotListBefore1_5_0_Key = "UnlockSlotListBefore1_5_0";
	public const string IsCheckUnlockSlotsForUserBefore_1_5_0_Key = "IsCheckUnlockSlotsForUserBefore_1_5_0";




	#region RenderLevel
	public const string RenderLevelMgr_Key = "RenderLevelMgr";
	public const string LoadTimeLevel_Key = "LoadTimeLevel";
	public const int AnticipationAnimationLevel_Key = 6;
	public const int SymbolBlinkAnimationLevel_Key = 6;
	public const int SymbolBonusAnimationLevel_Key = 6;
	public const int SpinAnimationLevel_Key = 6;
	public const int ReelBlinkAnimationLevel_Key = 6;
	public const int LobbySlotLightAnimationLevel_Key = 6;
	public const int SpinWinCoinsAnimationLevel_Key = 6;
	public const int ShineAnimationLevel_Key = 6;
	public const int MaxBetAnimationLevel_Key = 6;
	public const int BottomButtonAnimationLevel_Key = 6;
	public const int IdleAnimationLevel_Key = 6;
	#endregion

	public const string IsNeedUpdateApp_Key = "ApplicationConfig/IsNeedUpdateApp";
	public const string VersionExpirationTime_Key = "ApplicationConfig/VersionExpirationTime";
	public const string OpenAppUpdatePrompt_Key = "OpenAppUpdatePrompt";
	public const string ClickSlotUpdatePrompt_Key = "ClickSlotUpdatePrompt";


	public const string UpdateBundleDownloaded_Key = "Update_Bundle_Downloaded";
	public const string UpdatePromptShowed_Key = "Update_Prompt_Showed";
	public const string UpdatePromptClicked_Key = "Update_Prompt_Clicked";
	public const string UpdateMachinePromptShowed_Key = "Update_Machine_Prompt_Showed";
	public const string UpdateMachinePromptClicked_Key = "Update_Machine_Prompt_Clicked";


	public const string BGReloadInterval_Key = "ApplicationConfig/BGReloadInterval";
	public const string IsBlockBundle_Key = "ApplicationConfig/IsBlockBundle";
    public const string IsBlockNewPlayer_Key = "ApplicationConfig/IsBlockNewPlayer";
    public const string IsShowOpenAppDailyQuestDialog_Key = "ApplicationConfig/IsShowOpenAppDailyQuestDialog";
	public const string IsNoPreLoad_Key = "ApplicationConfig/NoPreLoad";		//过审资源下载开关
    public const string IsUnLoad_Key = "ApplicationConfig/IsUnLoad";		//过审资源下载开关

    public const string HourlyDropBox_Key = "ApplicationConfig/HourlyDropBox";
    
	public const int DefaultBGReloadInterval_Key = 3600;
	public const int Int_Zero_Key = 0;
	public const long Long_Zero_Key = 0;
	public const long Int_3600_Key = 0;
	public const int OneMillion_Key = 1000000;
	public const int MillionSecondsPerDay = 86400000;

	public const string IsShowPromoCode_Key = "ApplicationConfig/IsShowPromoCode";

	public const float ReelSlowSpeed = 6.5f;
	public const float ReelFastSpeed = 10.0f;
	public const string Unlock_Key = "Unlock";
	public const string ReelLock_Key = "ReelLock";
	public const string SymbolLock_Key = "SymbolLock";
	public const string MultiplierLock_Key = "MultiplierLock";

	public const string IsShowTournamentAd_Key = "ApplicationConfig/IsShowTournamentAd";
    public const string IsShowTask_Key = "ApplicationConfig/IsShowTask";
    public const string IsShowMapTask_Key = "ApplicationConfig/IsShowMapTask";

	#region Autopilot

	public const string CurrentSlotDict_Key = "CurrentSlotDict";
	public const string UIBundleDict_Key = "UIBundleDict";
	public const string SlotPlistDict_Key = "SlotPlistDict";
	public const string SlotMinBetDict_Key = "SlotMinBetDict";
	public const string SlotNoWinDict_Key = "SlotNoWinDict";

    public const string SlotOrder_Key = "SlotOrder";
	public const string LobbyMusic_Key = "LobbyMusic";
    public const string VideoAdCoins_Key = "VideoAdCoins";
    public const string OutOfCoinsTriggerMoment_Key = "OutOfCoinsTriggerMoment";
    public const string LevelPlan_Key = "LevelPlan";

	public const string Test_Key = "test";
	public const string Spin_Key = "spin";
	public const string Buy_Key = "buy";
	public const string Pay_Key = "pay";
	public const string Click_Key = "click";
	public const string Show_Key = "show";
	public const string Time_Key = "time";
	public const string User_Key = "user";
	public const string OutOfCoins_Key = "out_of_coins";
	public const string NearlyOutOfCoins_Key = "nearly_out_of_coins";
    public const string Coins_hold_Key = "coins_hold";

    public const string OldNearlyOutOfCoinsDialog_Key = "OldNearlyOutOfCoinsDialog";
    public const string RokieOfferDialog_Key = "RokieOfferDialog";
    public const string SpecialOfferDialog_Key = "SpecialOfferDialog";

    public const string Product_Id_Key = "product_id";

	public const string Bundle_Key = "bundle";
	public const string Math_Model_Key = "math_model";
	public const string Bet_Key = "bet";
	public const string NoWinRequestPossibility_Key = "nowinrequestpossibility";

	public const string Prize_Key = "prize";
	public const string LobbyMusicName_Key = "music_name";
    public const string slotNameOrder_Key = "slot_order";
    public const string OutOfCoinsSku_Key = "out_of_coins_sku";
    public const string Levelplan_Key = "level_plan";

	public const string AutopilotDataResponse_Key = "AutopilotDataResponse";

	public const string REMOVE_USELESS_REWARD_COMMAND = "RemoveUselessRewardCommand";
	
	public const string PayLineNumFlag_Key = "PayLineNumFlag";

	#endregion


	public const string SignupDelayTime_Key = "SignupDelayTime";

	public const string DeviceToken_Key = "deviceToken";
	public const string DeviceId_Key = "deviceId";
	public const string Region_Key = "region";
	public static readonly string VersionCode = "versionCode";
	public static readonly string SDKVersion = "AndroidSDKVersion";
	
	public const string OnRefreshFunctionUIItem = "RefreshFunctionUIItem";
	public const string OnRefreshLeftToolBox = "RefreshFunctionUIItemOnlyLeftToolBox";
	public const string SwitchFunctionUIState = "SwitchFunctionUIState";
	public const string SwitchActivityPanelState = "SwitchActivityPanelState";
	public const string OnRemoveFunctionUIItem = "RemoveFunctionUIItem";
	public const string OnRemoveAllFunctionUIItem = "RemoveAllFunctionUIItem";
	public const string OnFunctionUIItemDragingLeft = "FunctionUIItemDragingLeft";
	public const string OnFunctionUIItemDragEnd = "FunctionUIItemDragEnd";
	public const string OnFunctionUIItemMoveEnd = "FunctionUIItemMoveEnd";
	public const string OnFunctionUIStart = "FunctionUIStart";
	

	public const string TournamentWin_Key = "TournamentWin";
	public const string BetCoins_Key = "BetCoins";
    public const string LevelBoost_Key = "levelboost";
    public const string LevelBoostIsOver_Key = "levelboostover";
    public const string IsLevelBoost_Key = "IsLevelBoost";
    public const string LevelBoostGiftBoxLocalFeature_Key = "LevelBoostGiftBox";
    public const string LevelBoostTipDialogLocalFeature_Key = "levelboosttipdialog";
    public const string HourlyBonusSpeedUP_Key = "HourlyBonusSpeedUP";
    public const string DiceMapAddDice_Key = "dice_map_add_dice";
    public const string AddPickerCardEvent_Key="AddPickerCardEvent";

    public const string GiftBoxWheelGame_Key = "GiftBoxWheelGame";

    public const string AddDisplayDialogTokenMsg = "AddDisplayDialogTokenMsg";
	public const string DelDisplayDialogTokenMsg = "DelDisplayDialogTokenMsg";
	#region GiftBox
	public const string GiftBoxId_Key = "giftBoxID";
	public const string RequestGiftBoxEvent_Key = "RequestGiftBoxEvent";
	public const string Query_GiftBox_Key = "giftBox";
	public const string Campaign_Id = "campaign_id";
	#endregion

    #region Task
    public const string taskType_Key = "taskType";
    public const string TaskId_Key = "taskId";
    public const string TaskInfoId_Key = "task_info_id";
    public const string EndTime_Key = "expiration";
    public const string CreateTask_EndTime_Key = "end_time";
    public const string CreateTask_Durtion_Key = "duration";
    public const string AddNumber_Key = "addNumber";

    public const string FlushTime_Key = "flush_time";
    public const string AwardAmount_Key = "awardAmount";
    public const string TaskStatus_Key = "task_status";
    public const string TaskToken_Key = "taskUserId";
    public const string NextTaskToken_Key = "nextTaskUserId";
    public const string PrizeRate_Key = "prizeRate";
    public const string SumPrize_Key = "sumPrize";
    public const string bonus_Key = "bonus";
    public const string AddPrize_Key = "addPrize";
    public const string TotalAward_Key = "totalAward";
    public const string TaskProgress_Key = "TaskProgress";
    public const string TaskRTP_Key = "TaskRTP";
    public const string CurrentCollectProgress_Key = "CurrentCollectProgress";
    public const string Source_Key = "Source";
    public const string rewardList_Key = "rewardList";
    public const string sub_tasks_Key = "sub_tasks";
    public const string delayTiem_Key = "delayTime";
    public const string betLimit_Key = "betLimit";
    public const string limitData_Key = "limit_data";
    public const string spinLimit_Key = "spinLimit";
    public const string rewardListShow_Key = "reward_list_show";
    public const string UseNewTaskAwardMsg_Key = "UseNewTaskAwardMsg";
    public const string TaskFilters_Key = "filters";
    public const string TaskFilterOK_Key = "filter_ok";

    public const string winLimit_Key = "winLimit";
    public const string limitTime_Key = "limitTime";


    public const string round_Key = "round";

    public const string total_points_Key = "total_points";

    public const string MapNode_TaskType = "MapNode";
    public const string MapRoute_TaskType = "MapRoute";
    public const string MapNode2_TaskType = "MapNode2";
    public const string MapRoute2_TaskType = "MapRoute2";
    public const string Leaderboard_TaskType = "LeaderboardQuest";
    public const string PickerGameRoute_TaskType = "PickerGameRouteTask";
    public const string PickerGameSingle_TaskType = "PickerGameNodeTask";

    public const string MapNode3_TaskType = "MapNode3";
    public const string MapRoute3_TaskType = "MapRoute3";

    public const string DailyQuest_Key = "DailyQuest";
    public const string PointsQuest_Key = "PointsQuest";
    public const string DailyQuestOfDay = "DailyQuestOfDay";

    public const string processStatus_Key = "status";
    public const string AHEAD_STATUS = "AHEAD";
    public const string CLOSED_STATUS = "CLOSED";
    public const string ONGOING_STATUS = "ONGOING";

    public const string mapType_Key = "mapType";
    public const string map_TASKMAPTYPE = "map";
    public const string quest_TASKMAPTYPE = "quest";
    public const string active_TASKMAPTYPE = "active";
    
    public const string TaskCoins_Key = "TaskCoins";
    public const string TaskResource_Key = "TaskResource";
    public const string PlistTaskCoins_Key = "PlistTaskCoins";
    public const string GetRewardResponseMsg_Key = "GetRewardResponseMsg";
    public const string GetLBRewardResponseMsg_Key = "GetLBRewardResponseMsg";
    public const string TaskAwardSuccessMsg = "TaskAwardSuccessMsg";
    public const string TournamentAwardSuccessMsg = "TournamentAwardSuccessMsg";
    public const string DailyQuestSubTaskSuccessMsg_Key = "DailyQuestSubTaskSuccessMsg";

    public const string mapTaskGiftBox_LocalFeature = "mapTaskGiftBox";

    public const string CollectType_Key = "collectType";

    public const string UpdateTaskDataMsg = "UpdateTaskDataMsg";
    public const string UpdateCashBackTaskDataMsg = "UpdateCashBackTaskDataMsg";
    public const string UpdateWinCoinTaskDataMsg = "UpdateWinCoinTaskDataMsg";
    public const string TaskCompleteMsg = "TaskCompleteMsg";

    public const string TaskShowAdSucMsg = "TaskShowAdSucMsg";
    public const string TaskShowAdFailMsg = "TaskShowAdFailMsg";
    public const string TaskNeedShowAdMsg = "TaskNeedShowAdMsg";

    public const string SpinCollectNum_Key = "spinCollectNum";

    public const string ResetLimitedTaskMsg = "ResetLimitedTask";
    public const string ResetTaskMsg = "ResetTaskMsg";
    public const string TaskTimeOverMsg = "TaskTimeOverMsg";
    public const string TaskEndMsg = "TaskEndMsg";
    public const string TaskInvalidMsg = "TaskInvalidMsg";
    public const string DestroyTaskUIMsg = "DestroyTaskUIMsg";

    public const string SpinTotalNum_Key = "spinTotalNum";
    public const string SpinTotalCost_Key = "spinTotalCost";
    public const string SpinTotalWin_Key = "spinTotalWin";
    public const string SpinIsJackporWin_Key = "jackpot";
    public const string CollectDatas_Key = "collectDatas";

    public const string TimeType_Key = "timeType";
    public const string ItemId_Key = "item_id";

    public const string Response_TaskIdNULL = "taskId_null";
    public const string Response_TaskTokenNULL = "taskUserId_null";
    public const string Response_TaskInvalid = "task_invalid";
    public const string Response_TaskCollectSumInvalid = "collect_sum_invalid";
    public const string Response_TaskCollectAddInvalid = "collect_add_invalid";
    public const string Response_TaskPrizeSumInvalid = "prize_sum_invalid";
    public const string Response_TaskPrizeAddInvalid = "prize_add_invalid";
    public const string Response_TaskEventId_Invalid = "eventid_invalid";
    public const string Response_TaskEndMsg = "task_ended";

    public const string Response_RepeatAward = "repeat_award";

    public const string Response_ErrorType = "error_type";


    public const string TaskName_Key = "taskName";
	public const string TaskIntroduceDialog_NAME = "TaskLobbyDisplayDialog";
	public const string TaskStartNoticeDialog_Key = "TaskStartNoticeDialog";
	public const string TaskRestartNoticeDialog_Key = "TaskRestartNoticeDialog";
	public const string TaskMapPanel_Key = "TaskMapPanel";
    public const string TaskMoreInfoDialog_Key = "TaskMoreInfoDialog";
    public const string TaskSettlementDialog_Key = "TaskSettlementDialog";

    public const string TaskUltimateAwardDialog_Key = "TaskUltimateAwardDialog";

    public const string MapTaskName_Key = "MapTaskName";
    public const string SwitchTaskMsg = "SwitchTaskMsg";
    public const string TaskAwardErrorTypeMsg = "TaskAwardErrorTypeMsg";
    public const string RewardTaskCreatedMsg = "RewardTaskCreatedMsg";

    public const  string RequestRewardTaskEvent_Key = "RequestRewardTaskEvent";


   public const int LeafTask_Key = 10;//
    public const int CollectPayLineSymbol_Key = 1;//收集 PayLine Symbol
    public const int CollectReelSymbol_Key = 2;//收集 盘面 Symbol

    public const int CollectSpinCountTask_Key = 3;// 收集Spin次数
    public const int CollectWinCountTask_Key = 4;//收集Spin中奖次数
    public const int CollectWinCoinsTask_Key = 5;//收集中奖金额
    public const int CollectBigWinCountTask_Key = 6;//收集Spin中BigWin次数
    public const int CollectContinuousWinCountTask_Key = 7;//收集连续Spin中奖次数
    public const int CollectQuestPointTask_Key = 9;//收集QuestPoint
    public const int CollectEpicWinCountTask_Key = 11;//收集Spin中EpicWin次数
    public const int CollectSymbolDecorationOnReelsTask_Key = 12;//收集盘面 Symbol的动态装饰
    public const int CollectSymbolDecorationOnLineTask_Key = 13;//收集PayLine Symbol的动态装饰

    public const int GetGiftBoxTask_Key = 14;//直接领取礼包
    public const int CollectBigWinCoinsTask_Key = 15;//收集Spin中BigWin中奖金额
    public const int CollectEpicWinCoinsTask_Key = 16;//收集Spin中EpicWin中奖金额
    public const int CollectLevelUpCountTask_Key = 17;//收集升级次数
    public const int UpgradeToTargetLevelTask_Key = 18;//升到目标Level
    public const int CollectTaskAwardSuccessCountTask_Key = 19;//收集完成任务次数
    public const int CollectSpinSpendCoinsTask_Key = 21;//收集Spin花掉指定金额
    public const int CollectJackpotCoinsTask_Key = 22;//收集Jackpot WinCoins金额
    public const int CollectJackpotCountTask_Key = 23;//收集Jackpot 中奖次数

    public const int LeaderBoardTask_Key = 24;//排行榜相关的任务
    public const int CollectOnlyBigWinCountWithBundleTask_Key = 25;//收集BigWin次数并替换BigWinBundle
    public const int CollectSymbolDecorationOnPanelTask_Key = 26;//收集Symbol装饰  出现在panel中间
    public const int CollectFiveOfKindCountTask_Key = 27;//收集5ofkind次数的任务
    public const int CollectFiveOfKindCoinsTask_Key = 28;//收集5ofkind金币的任务
    public const int CollectFreeGameCountTask_Key = 29;//收集freegame次数的任务
    public const int CollectFreeGameWinCoinsTask_Key = 30;//收集freegame金币的任务
    public const int CollectWinTargetCoinsInTargetFreeSpinTask_Key = 31;//限定在指定次数的freespin中赢指定金币的任务
	public const int CollectMegawinCountTask_Key = 32; //收集megawin次数的任务
    public const int CollectMegawinCoinsTask_key = 33; //收集megawin金币的任务
    public const int CollectSymbolDecorationOnPanelTaskDiceMap_Key = 34;//收集Symbol装饰  出现在panel中间 dicemap
    
    public const int CollectScatterSymbolCountTask_Key = 37;      // 收集 N 个Scatter图标
    public const int CollectBonusRespinTriggerCountTask_Key = 38; // 触发 Bonus或Respin N 次
    public const int CollectSpinCountInFreeSpinTask_Key = 39;     // 在FreeSpin中Spin N 次

    public const int CollectCoinsInLinkGame_Key = 52; 	// 在LinkGame中收集 XX Coins
    public const int CollectMachineFeatureTask_Key = 53;//收集指定机器Feature
    public const int CollectPaymentCountTask_Key = 54; 	// 收集付费次数 
    public const int CollectPaymentAmountTask_Key = 55; 	// 收集付费金额
    public const int CollectDailyTaskSuccessCountTask_Key = 56; 	// 完成N个 日常任务 
    public const int CollectFreeSpinEndMultiBoardNumTask_Key = 59;	//收集n次freespin结束时，收集到的棋盘数量
    public const int CollectFreeSpinEndSymbolMapTask_Key = 60;	//收集n次指定机器freespin时指定的symbol（目前只支持一台机器的一种symbol）
    public const int CollectWinCoinsInSingleSpinForTargetTimesTask_Key = 61;	//赢取n次大于m数量的奖励
    public const int CollectConsecutiveWinForTargetTimesTask_Key = 62;	//连续赢取奖励m次为收集一次，收集n次
    public const int CollectScattersInSingleSpinForTargetTimesTask_Key = 63;	//在一次spin中至少收集m个scatter，收集n次
    public const int CollectFreeSpinCountInSingleSpinForTargetTimesTask_Key = 64;   //再一次spin中至少触发freespincount>m的数值,收集n次	
                                                                                    // 收集Stamp 个数

    public const int CollectHourlyBonusCountTask_Key = 67;     // 收集 HourlyBonus 次数

    public const int CollectSymbolsInFreeGameTask_Key = 60;    // 收集在FreeSpin中 特定图标 X个
    
    public const int CollectFreeSpinWinCountTask_Key = 70;    //收集FreeSpin中中奖 X 次 
    public const int CollectSymbolsInSingleFreeGameTask_Key = 71;    // 单次Free Spin收集特定图标XX个
    public const int CollectSpinCountInReSpinTask_Key = 72;    //收集Respin中Spin的次数
    public const int CollectWinCoinsInRespinTask_Key = 73;            // 收集在 Respin中赢钱数量
    public const int CollectSymbolsInRespinTask_Key = 74;            // 收集在 Respin中牌面
    public const int CollectTriggerBonusCountTask_Key = 75;    //触发BonusGame多少次
    public const int CollectWinCoinsInBonusTask_Key = 76;    //收集Bonus赢取金币数量
    public const int CollectCommJackpotLevelTask_Key = 77;            // 触发某个等级的通用Jackpot最低等级为0
    public const int CollectMachineJackpotLevelTask_Key = 78;            // 触发某个等级的机器Jackpot最低等级为0
    public const int CollectWinCoinsInMachineJackpotTask_Key = 79;    //	机器JackPot 赢钱
    public const int CollectFullScreenSymbolWinTask_Key = 80;    //某个symbol全屏中奖计数1次，wild代替该symbol全屏中奖也算
    public const int CollectReelSymbolTotalCountSingleSpinCount_Key = 81;     // 收集单次Reel Spin中，同时出现某些图标超过N个
    public const int CollectReelSymbolUnityCountSingleSpinCount_Key = 82;     // 收集单次Reel Spin中，同时出现X图标N个和Y图标M个
    public const int CollectTargetCoinsInTargetSpinTask_Key = 83;    //收集指定金币在指定的Spin次数内

    public const int InternalTask_Key = 100;//任务树中的中间任务节点
    public const int LeaderBoardInternalTask_Key = 200;//任务树中的中间任务节点
    public const int CanActiveCloseParentTask_Key = 300;//前端可以随时主动完成和切换的父任务节点
    #endregion

	public const string Time_Day_HMS_Key = "{0}d {1:D2}:{2:D2}:{3:D2}";

	public const string Time_HMS_Key = "{0:D2}:{1:D2}:{2:D2}";
	public const string Time_MS_Key = "{0:D2}:{1:D2}";

	public const string Time_OneDay_Key = "1 day";
	public const string Time_N_Day_Key = "{0} days";

	public const string Time_OneDay_B_Key = "1 DAY";
	public const string Time_N_Day_B_Key = "{0} DAYS";
	
	public const string Time_HM_Key = "{0:D2}h:{1:D2}m";
	public const string Time_MS_Key2 = "{0:D2}m:{1:D2}s";

	public const string StringFormat_0_1_Key = "{0}/{1}";
	public const string StringFormat_01_Key = "{0}{1}";
    public const string Sprite_coinSingle_Key = "<sprite=\"coinSingle\" index=0> ";

    public const char Comma_Key = ',';
	public const char Semicolon_Key = ';';

    public const string name__Key = "name_";

    public const string X_CHAR = "X";

	public const string EnterSlotSource_Key = "EnterSlotSource";
	public const string EnterSlotType_Unknow = "Unknow";

	public const string USEFUL_DATA_PATH="Assets/DifferentSources/UsefulData.txt";

	public const string USELESS_DATA_PATH="Assets/DifferentSources/UselessData.txt";

	public const string CLASSIC_CONFIG_PATH = "Assets/DifferentSources/Resources/Config_classicIOS.plist.xml";
	public const string COMMON_CONFIG_PATH = "Assets/Resources/GameConfig.plist.xml";

	public const string NewbieTimeInHour = "ApplicationConfig/NewbieTimeInHour";

	public const string CLICK_WATCH_ADS_MOVIE = "ClickWatchAdsMovie";
	public const string NEXT_WATCH_ADS_TIME = "NextWatchAdsTime";
	public const string SET_COINSPANEL_TWEENER_PARAM = "SET_COINSPANEL_TWEENER_PARAM";
	public const string RemoveCorner_ = "RemoveCorner_";


	#region 触发器监听字符串

	public const string Cdt_FinishDailyQuest1_Key = "Cdt_FinishDailyQuest1";
	public const string Cdt_FinishDailyQuest2_Key = "Cdt_FinishDailyQuest2";
	public const string Cdt_FinishDailyQuest3_Key = "Cdt_FinishDailyQuest3";
	public const string Cdt_LevelUp_Key = "Cdt_LevelUp";
	public const string Cdt_Purchase_Key = "Cdt_Purchase";
	public const string Cdt_GetProgressQuestItem_Key = "Cdt_GetProgressQuestItem";
    public const string Cdt_OpenAppPQReady_Key = "Cdt_OpenAppPQReady";
    public const string Cdt_OpenAppCouponReady_Key = "Cdt_OpenAppCouponReady";
    public const string Cdt_BackToLobby_Key = "Cdt_BackToLobby";

    #endregion

    #region Slot_Ani

    public const string win2_Ani = "win2";
	public const string idle_Ani = "idle";
	public const string win_Ani = "win";
	public const string in_Ani = "in";
	public const string out_Ani = "out";

	#endregion

    #region Gift Box
    public const string CloseLoadingDialog = "CloseLoadingDialog";
    public const string ShowGiftBoxDialogMsg = "ShowGiftBoxDialogMsg";
    public const string CloseGiftBoxDialogMsg = "CloseGiftBoxDialogMsg";
    #endregion

    public const string CloseTaskUltimateAwardDialogMsg = "CloseTaskUltimateAwardDialogMsg";
    public const string CloseMapTaskNewRoundDialog = "CloseMapTaskNewRoundDialog";

    public const string String_0_VALUE = "0";
    public const string StringFormat_0_Per = "0%";
    public const string StringFormat_00_Per = "00%";
    public const string StringFormat_100_Per = "100%";
    public const string StringFormat_F2 = "F2";


    public const string Wait_Reseting_TEXT = "Wait Reseting...";
    public const string Reseting_TEXT = "Reset";

    public const string NeedUpgradeBetMsg = "NeedUpgradeBetMsg";
    public const string SetSpinLimitTxt = "SetSpinLimitTxt";

    #region Animation
    public const string in_key = "in";
    public const string out_key = "out";
    public const string hint_key = "hint";
    public const string idle_key = "idle";
    public const string click_key = "click";
    public const string collect_key = "collect";
    public const string shouji_key = "shouji";
    public const string out_idle_key = "out_idle";
    public const string shoujijinbi_Key = "shoujijinbi";
    #endregion

    #region HourlyBonus

    public const string UPDATE_HOURLYBONUS = "UpdateHourlyBonusUI";

    #endregion
    
    #region RewardGame
    public const string Dollar_Key = "$";
    public const string wheelBonus_RGType = "wheelBonus";

    public const string Vipaddition_Key = "Vipaddition";
    public const string Leveladdition_Key = "Leveladdition";

    public const string targetIndex_Key = "targetIndex";
    public const string extraMultiple_Key = "extraMultiple";
    public const string currentBonusList_Key = "currentBonusList";

    public const string STORE_PLAN = "store_plan";

    
    public const string completed_Ani = "completed";
    public const string doing_Ani = "doing";
    public const string unlock_Ani = "unlock";
    public const string locked_Ani = "locked";
    public const string groupFeature_LocalF = "groupFeature";
   
    public const string index_Key = "index";
   
    public const string ItemList_Key = "ItemList";
    public const string AddVipPoint_Format = "+{0} VIP";
    public const string AddVipPointMsg_Key = "AddVipPointMsg";
    public const string WildSpinRewardGame_Key = "WildSpinRewardGame";
    public const string RewardGameMgr_Key = "Module/RewardGameMgr";
   
   
    public const string skuName_Key = "skuName";
    public const string multipler_Key = "multipler";
    public const string baseCoins_Key = "baseCoins";
    public const string totalCoins_Key = "totalCoins";
    #endregion

    public const string PIC_PATH = "Prefab/Atlas/InboxItem/";
    public const string DEFAULT_INBOX_PIC_PATH = "Prefab/Atlas/DefaultInbox/";
    public const string OpenTaskBigWinBundleDialogMsg = "OpenTaskBigWinBundleDialogMsg";
    public const string CloseTaskBigWinBundleDialogMsg = "CloseTaskBigWinBundleDialogMsg";

    public const string CurrentBigWinBundleTaskConditionOKMsg = "CurrentBigWinBundleTaskConditionOKMsg";

    #region ReclaimCoins
    public const string ReclaimLevelList_Key = "ReclaimLevelList";
    public const string Module_ReclaimCoins_Key = "Module/ReclaimCoins";

    public const string LimitUp_Key = "LimitUp";
    public const string LimitDown_Key = "LimitDown";
    public const string PricePoint_Key = "PricePoint";

    public const string reclaimSlots_Key = "reclaimSlots";
    public const string SpinNumLimit_Key = "SpinNumLimit";
    public const string BalanceLimit_Key = "BalanceLimit";
    public const string BetRatioLimit_Key = "BetRatioLimit";

    public const string AckRewardTokenSave_Key = "AckRewardtokenSave";
    
    #endregion

    public const string ShowFiveOfKind_Key = "ShowFiveOfKind";
    public const string WinFiveOfKindCoins_Key = "WinFiveOfKindCoins";
    public const string WinCoinsInLinkGame_Key = "WinCoinsInLinkGame";

    public const string OnSpinCloseSetting = "OnSpinCloseSetting";

    public const string Level_UP = "LevelUp";
    public const string OnLevelUpDialogCloseMsg = "OnLevelUpDialogCloseMsg";

    public const string OnCollectProgressQuestItem_Key = "OnCollectProgressQuestItem";
    public const string TaskCollectProgressQuestItem_Key = "TaskCollectProgressQuestItem";

    public const string PQItemNum_Key = "PQItem_num";
    
    public const string OnShowPQLevelUI_Key = "OnShowPQLevelUI";
    public const string OnHidePQLevelUI_Key = "OnHidePQLevelUI";
    public const string OnPQEnterFatalError_Key = "OnPQEnterFatalError";

    public const string OnPQEntranceClick_Key = "OnPQEntranceClick";
    public const string OnPQOfferAdClick_Key = "OnPQOfferAdClick";
    public const string RefreshToolBoxUIKey = "RefreshToolBoxUI";

    #region Coupon
    public const string CouponItemNum_Key = "CouponItem_num";
    public const string OnCouponEnterFatalError_Key = "OnCouponEnterFatalError";
    public const string OnCouponEntranceClick_Key = "OnCouponEntranceClick";
    #endregion

    #region AdTask
    public const string OnRewardVideoWatched_Key = "OnRewardVideoWatched";
    public const string OnActiveAdBtn_Key = "OnActiveAdBtn";
    #endregion

    #region CommonJackpot

    public const string SHOW_COMMON_JACKPOT_WIN_INFO = "ShowCommonJackpotWinInfo";

    #endregion

    public const string SET_BALANCE_TARGET_POSITION = "SetBalanceTargetPosition";
    
    
    public const string OnPurchaseSucessfulKey = "OnPurchaseSucessful";
    public const string OnRefreshTopIconPanel = "RefreshTopIconPanel";
    public const string REFRESH_TOP_LEVEL_PROGRESS_BUFF = "RefreshTopLevelProgressBuff";

    public const string REPLACE_PREFAB_UI_ELEMENTS = "ReplacePrefabUIElements";
    public const string LOAD_REPLACE_PREFAB_UI_RES = "LoadReplacePrefabUIRes";

    #region ServerSpinInitProxy

    /// <summary>
    /// 后端化机器进入机器时分发Config中的bonusConif结构
    /// </summary>
    public const string OnEnterMachinePB_BonusConfig = "OnEnterMachinePB_BonusConfig";

    #endregion
    #region ClubSystem
    public const string CMD_MSG_TYPE_CLUB_SYSTEM = "club_system";

    public const string REPLACE_LOADING_BG_KEY = "ReplaceLoadingBG";

    public const string LOADING_SCENE_ASSETS_CONFIG_KEY = "LoadingSceneAssetsConfig";
    public const string LOADING_SCENE_ASSET_PREFAB_PATH_KEY = "LoadingSceneAssetPrefabPath";
    public const string LOADING_SCENE_ASSET_BUNDLE_PATH_KEY = "LoadingSceneAssetBundlePath";

    public const string SCENE_BUNDLE_PREFIX_KEY = "SceneBundle_";

    public const string INIT_APP_SYSTEM_KEY = "InitAppSystem";
    public const string SCENE_LOADING = "Loading";
    #endregion
    
    
    #region GameFrameworkDebugger

	public const string SEGMENT_INIT_DONE = "SEGMENT_INIT_DONE";


    #endregion
    #region WebmMechineNames

    public static string[] WEBM_MECHINE_NAMES = {"TaleOfRed","SakuraSecret","LeprechaunsFortune","EmeraldOfOZ","CatWorld","BuffaloTerritory","PirateBonanza"};
    
    #endregion
    
    #region ClubSystem

    public const string REFRESH_TASK_MSG = "RefreshTaskMsg";
    public const string END_AUTO_RUN = "EndAutoRun";
    #endregion
    
    #region MachineQuest
    public const int QuestMachineCollectTask_Key = 10000; // QuestMachine 新的任务类型

    public const string QuestMachineDataMsg = "QuestMachineDataMsg";
    public const string QuestMachineOnResponseMsg = "QuestMachineOnResponseMsg";
    public const string QuestMachineUsedDataTokenListKey = "QuestMachineUsedDataTokenListKey";
    public const string SystemDataSendMsg = "SystemDataSendMsg";
    public const string CommonDataSendMsg = "CommonDataSendMsg";
    public const string OnSpinEndSendResultEvent = "OnSpinEndSendResultEvent";
    #endregion 
	public const string TreasureRushOpenDialog = "TreasureRushOpenDialog";
	public const string TreasureRushCollectTipsOpenDialog = "TreasureRushCollectTipsOpenDialog";

	public const string RefreshSysProgressInfo = "RefreshSysProgressInfo";
	
	#region MissionPass
	public const string SHOW_ON_MISSIONPASS = "IsShowOnMissionPass";
	#endregion
	
	#region 后端化Account

	public const string CoinAsset = "CoinAsset"; 
	public const string LevelAsset = "LevelAsset"; 
	public const string ExpAsset = "ExpAsset"; 
	public const string PiggyAsset = "PiggyAsset"; 
	public const string VipPointAsset = "VipPointAsset"; 
	public const string VipLevelAsset = "VipLevelAsset"; 

	#endregion
	
	public static string DetailEffectTips = "DetailEffectTips";

	#region Medal

	public const string OpenMedalGalleryEventKey = "OpenMedalGalleryEvent";
	#endregion

	#region HourlyBonus

	public const string BACKGROUND = "HourlyBonus-Background";
	public const string HOULYBONUSBACKGROUND = "hourlybonus_wheel_background";
	public const string HOURLYBONUS_PAY_BACKGROUND = "hourlybonus_pay_background";
	public const string BTN_SPIN = "HourlyBonus-SpinButton";
	public const string HOURLYBONUS_SPIN = "hourlybonus_spin";
	public const string EXTRA_BONUS = "HourlyBonus-Extra-Bonus";
	public const string INITIATION = "HourlyBonus-Initiation";
	public const string WHEEL_WIN = "HourlyBonus-Wheel-Win";
	public const string HOURLYBONUS_ENDING = "hourlybonus_ending";
	public const string HOURLYBONUS_APPEAR_NUMBER = "hourlybonus_appear_number";
	public const string HOURLYBONUS_CHANGE_WHEEL = "hourlybonus_change_wheel";
	public const string ROLLING1 = "HourlyBonus-Rolling1";
	public const string ROLLING2 = "HourlyBonus-Rolling2";
	public const string ROLLING_END = "HourlyBonus-RollingEnd";
	public const string ROLLING_UP = "HourlyBonus-RollingUp";
	public const string WHEEL_WIN3 = "HourlyBonus-Wheel-Win3";

	public const string ANIMATION_RUN = "Rotation";
	public const string ANIMATION_WIN = "win";
	public const string ANIMATION_OUT = "Out";
	public const string ANIMATION_OUT_up = "Out_up";
	public const string ANIMATION_zero = "zero";
	public const string ANIMATION_In = "in";

	public const string PAY = "pay";
	public const string WEIGHT = "weight";
	public const string IS_MAX_VALUE = "IsMaxValue";
	public const string CLOSE_LOADING_DIALOG = "closeloading";
	
	public const string HourlyBonus_AdBonusFreespinSuccessful = "AdBonusSuccessful";
	public const string HourlyBonus_AdBonusFreespinDefeated = "AdBonusFreespinDefeated";
	public const string HourlyBonus_HourlyBonusReset = "HourlyBonusReset";

	public const string HourlyBonus_SuperWheelRewardGameShowMsg = "SuperWheelRewardGameShowMsg";
	public const string HourlyBonus_CloseHourlyBonusDialogMsg = "CloseHourlyBonusDialogMsg";
	public const string HOURLYBONUS_Change_Color = "HOURLYBONUS_Change_Color";

	#endregion
	
	public const string data_Key = "data";
	public const string TIME_STAMP = "timestamp";
	public const string KEY_MAIN_ID = "mid";
	public const string KEY_TOUR_AVATAR = "avatar_url";
	public const string KEY_TOUR_NAME = "nickname";
	public const string KEY_JACKPOT= "jackpots";
	public const string WinCoins_key = "Wincoins";
	public const string FreeSpinCount_key = "FreeSpinCount";
	public const string WinCash_key = "Wincashs";
	public const string StartWithDraw = "StartWithDraw";
	public const string SHOW_WITH_DRAW_TIPS_PANEL = "SHOW_WITH_DRAW_TIPS_PANEL";
	
	public const string TriggerBonusGame = "TriggerBonusGame";
}
