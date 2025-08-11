using UnityEngine;
using Core;
using System.Collections.Generic;
using Libs;
using System;
using Activity;
using CardSystem;
using Constants = Libs.Constants;

namespace Classic
{
    public class SlotGameDialogManager : GameDialogManager
    {
        public static bool needOpenPortrait = true;


        protected override void openSettingDialog()
        {
            //Libs.UIManager.Instance.Open<SettingDialog> ();
        }

        protected override void OnOpenTerms_EUDialogMsg(bool IsModel)
        {
        }

        protected override void openSceneExchange(System.Action closeCallBack, object data)
        {
            AsyncLogger.Instance.StartTraceLog("SceneExchange:openSceneExchange");

            if (data == null) return;

            Libs.UIManager.Instance.CloseAll();

            string sceneName;
            if (data is SlotMachineConfig)
            {
                sceneName = (data as SlotMachineConfig).Name();
            }
            else
            {
                sceneName = data.ToString();
            }

            Sprite s = LoadingSceneUtil.GetSceneLoading(data);

            System.Action ac = new System.Action(() =>
            {
                if (closeCallBack != null)
                {
                    closeCallBack();
                }

                if (LoadingSceneUtil.loadingBundle != null)
                {
                    LoadingSceneUtil.loadingBundle.Unload(false);
                    LoadingSceneUtil.loadingBundle = null;
                }

                AsyncLogger.Instance.EndTraceLog("SceneExchange:openSceneExchange");
            });
            
            ReplaceLoadingDialog<SceneExchange>(null,ac,data,1f);
        }
        
		private void ReplaceLoadingDialog<T>(Action<T> initCB =null,Action endCB = null,object data =null,float maskAlpha =0.7f) where T:UIDialog
		{
				//扩展接口 将其他模块的替换配置数据持久化到本地，每次打开时，如果存在数据则使用本地持久化的，否则使用默认的
			string resPath = "Prefab/UI/SceneExchange";
			string bundleName = string.Empty;

			Action<bool> OpenLoadingDialog = (releaseBundle) =>
			{
                // bool isPortrait = SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
                OpenConfigParam<T> param = new OpenConfigParam<T>(0, OpenType.AtOnce, Constants.CUT_SCENE,dialogInitCallBack:dialog =>
				{
					if (releaseBundle)
					{
						dialog.CompleteQuitCallBack += () =>
						{
							//当不存在弹框时，恢复自动旋转
							if (BaseGameConsole.ActiveGameConsole().IsInSlotMachine())
							{
								if (!UIManager.Instance.CheckExistDialog())
									Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
							}
						};
					}

					if (initCB != null) initCB(dialog);
				},dialogCloseCallBack:endCB,data:data,animationIn:UIAnimation.NOAnimation,animationOut:UIAnimation.NOAnimation,defaultResourcePath:resPath,bundleName:bundleName,maskAlpha:maskAlpha);
				
				param.forceSkipCloseAll = true;
				
				Libs.UIManager.Instance.OpenHighLevelDialog(param);
			};
            OpenLoadingDialog(false);
		}

        protected override void openBlackOutPanel(System.Action onDialogClose)
        {
            if (BaseGameConsole.singletonInstance.IsInLobby()) return;

            UIManager.Instance.OpenSystemDialog(new OpenConfigParam<BlackOutPanel>(openType: OpenType.Normal,
                uiPopupStrategy: new SystemUIPopupStrategy(), dialogInitCallBack:
                (Dialog) => { Dialog.AutoQuit = true; }, runEnd: (over) => onDialogClose?.Invoke()));
        }

        protected override void openAutoSpinWinDialog(int times, System.Action onDialogClose, bool isShow,
            bool newRuleDialog)
        {
            if (BaseGameConsole.singletonInstance.IsInLobby()) return;
            Libs.UIManager.Instance.OpenMachineDialog<FreeGameStartDialog>((dialog) => { dialog.OnStart(times); },
                onDialogClose, inAnimation: Libs.UIAnimation.Scale, outAnimation: Libs.UIAnimation.Scale,
                maskAlpha: 0.8f);
        }

        protected override void openReSpinStartDialog(System.Action onDialogClose)
        {
            if (BaseGameConsole.singletonInstance.IsInLobby()) return;

            Libs.UIManager.Instance.OpenMachineDialog<ReSpinStartDialog>((dialog) => { dialog.isModel = false; },
                onDialogClose, inAnimation: Libs.UIAnimation.Scale, outAnimation: Libs.UIAnimation.Scale,
                maskAlpha: 0.8f);
        }

        protected override void openAutoSpinAdditionalDialog(int times, System.Action onDialogClose, bool newRuleDialog)
        {
            if (BaseGameConsole.singletonInstance.IsInLobby()) return;
            Libs.UIManager.Instance.OpenMachineDialog<FreeGameRetriggerDialog>((dialog) =>
            {
                dialog.OnStart(times);
                dialog.AutoQuit = true;
                dialog.DisplayTime = 3;
            }, onDialogClose);
        }

        protected override void openFreeSpinEndDialog(Dictionary<string,object> infos, System.Action onDialogClose,
            bool newRuleDialog)
        {
            if (BaseGameConsole.singletonInstance.IsInLobby())
                return;
            long winCoins = Utils.Utilities.GetLong(infos,GameConstants.WinCoins_key,0);
            int freeSpinCount = Utils.Utilities.GetInt(infos,GameConstants.FreeSpinCount_key,0);
            int cash = Utils.Utilities.GetInt(infos,GameConstants.WinCash_key,0);;
            Libs.UIManager.Instance.OpenMachineDialog<FreeGameEndDialog>(
                (dialog) => { dialog.OnStart(winCoins, freeSpinCount,cash); }, onDialogClose,
                inAnimation: Libs.UIAnimation.Scale, outAnimation: Libs.UIAnimation.Scale, maskAlpha: 0.8f);
        }

        protected override void openReSpinEndDialog(long winCoins, System.Action onDialogClose)
        {
            if (BaseGameConsole.singletonInstance.IsInLobby())
                return;
            Libs.UIManager.Instance.OpenMachineDialog<ReSpinEndDialog>((dialog) => { dialog.SetNum(winCoins); },
                onDialogClose, inAnimation: Libs.UIAnimation.Scale, outAnimation: Libs.UIAnimation.Scale,
                maskAlpha: 0.8f);
        }

       

       

       

       

        protected override void openKindOfSymbolDialog(Sprite sprite, int count, System.Action onDialogClose)
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenTips(new OpenConfigParam<KindOfSymbolDialog>(isPortrait, 0, OpenType.Normal,
                uiPopupStrategy: new MachineUIPopupStrategy(),
                dialogInitCallBack: (dialog) => { dialog.KindInit(sprite, count); }, dialogCloseCallBack: onDialogClose,
                animationIn: UIAnimation.NOAnimation, animationOut: UIAnimation.NOAnimation,
                queueId: Constants.UI_TIPS_EVENT_KEY));
        }

        protected override void openPowerMachineTipsDialog()
        {
            string dialogPrefab = "Prefab/Shared/PowerTipsInMachine";
            GameObject topRoot = null;
            topRoot = GameObject.Find("Main Camera/BannerCanvas/BannerPrefab/SlotsDialogPanel/Panel");
            if (topRoot == null)
                topRoot = GameObject.Find("Main Camera/BannerCanvas/BannerPrefab_Portrait/SlotsDialogPanel/Panel");
            if (topRoot != null)
            {
                GameObject powerTipsPrefab = Resources.Load<GameObject>(dialogPrefab);
                if (powerTipsPrefab != null)
                {
                    Transform powerTips = Instantiate(powerTipsPrefab).transform;
                    powerTips.SetParent(topRoot.transform, false);
                    powerTips.localScale = Vector3.one;
                }
            }
        }

        protected override void openRewin(System.Action onDialogClose)
        {
            Libs.UIManager.Instance.OpenSystemDialog<ReWin>(new OpenConfigParam<ReWin>(
                uiPopupStrategy: new MachineUIPopupStrategy(), dialogInitCallBack: (dialog) =>
                {
                    dialog.AutoQuit = true;
                    dialog.DisplayTime = 1.5f;
                }, dialogCloseCallBack: onDialogClose, animationIn: UIAnimation.Scale,
                animationOut: Libs.UIAnimation.Top));
        }

        protected override void openPaytablePanel()
        {
            if (BaseGameConsole.singletonInstance.IsInLobby())
                return;
            Libs.UIManager.Instance.OpenMachineDialog<PaytablePanel>((dialog) => { dialog.SetOpenMask(); }, null);
        }

        protected override void OpenLoadingDialog()
        {
            UIManager.ShowLoadingUI(10f);
        }

        protected override void openLoadingWaitingDialog()
        {
            UIManager.ShowLoadingUI();
        }

        protected override void closeLoadingWaitDialog()
        {
            UIManager.HideLoadingUI();
        }


        protected override void openLoadingWaitCallback(Action callback)
        {
            UIManager.ShowLoadingUI(10f, endCB: callback);
        }

        protected override void openLoadingWaitCallbackAtOnce(Action callback)
        {
            if (callback == null) return;
            UIManager.ShowLoadingUI(10f);
            callback.Invoke();
            //关闭loading在具体callback中做，别忘记关闭loading
        }

        protected override void openLoadingWaitCallbackAtOnceWithTime(Action callback, float time)
        {
            UIManager.ShowLoadingUI(time);
            callback?.Invoke();
        }

        protected override void openRateMachineDialog(SlotMachineConfig config)
        {
            UIManager.Instance.OpenSystemDialog(new OpenConfigParam<RateMachineDialog>(openType: OpenType.Normal,
                uiPopupStrategy: new SystemUIPopupStrategy(), data: config.Name()));
        }

        protected override void openJackPotMineWinDialog(int winNumber, long winValue, System.Action callBack)
        {
            UIManager.Instance.OpenMachineDialog<JackPotWinDialog>(
                delegate(JackPotWinDialog obj) { obj.Init(winNumber, winValue, callBack); }, null);
        }

        protected override void openJackPotWinDialog_Jackpot777(int winNumber, long winValue, System.Action callback)
        {
            OpenConfigParam<JackPotWinDialog_Jackpot777> param = new OpenConfigParam<JackPotWinDialog_Jackpot777>(
                openType: OpenType.Normal, uiPopupStrategy: new SystemUIPopupStrategy(),
                dialogInitCallBack: (obj) => { obj.Init(winNumber, winValue, callback); });
            UIManager.Instance.OpenSystemDialog(param);
        }

        protected override void openBetTips()
        {
            UIManager.Instance.OpenTips(
                new OpenConfigParam<BetChangeDialog>(uiPopupStrategy: new SystemUIPopupStrategy(),
                    animationIn: UIAnimation.NOAnimation, animationOut: UIAnimation.NOAnimation,
                    queueId: Constants.UI_TIPS_EVENT_KEY));
        }

        protected override void epicWinDialog()
        {
        }

        protected override void openFirstGoLobbyDialog()
        {
            string resPath = "Prefab/UI/SceneExchange";
            string bundleName = string.Empty;
            OpenConfigParam<FirstToLobbyDialog> param = new OpenConfigParam<FirstToLobbyDialog>(0, OpenType.AtOnce,
                Constants.CUT_SCENE, dialogInitCallBack: dialog =>
                {
                    dialog.CompleteQuitCallBack += () =>
                    {
                        //当不存在弹框时，恢复自动旋转
                        if (BaseGameConsole.ActiveGameConsole().IsInSlotMachine())
                        {
                            if (!UIManager.Instance.CheckExistDialog())
                                Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
                        }
                    };
                }, animationIn: UIAnimation.NOAnimation, animationOut: UIAnimation.NOAnimation,
                defaultResourcePath: resPath, bundleName: bundleName, maskAlpha: 0.7f);

            param.forceSkipCloseAll = true;

            Libs.UIManager.Instance.OpenHighLevelDialog(param);
        }

        protected override void openHighRollerDialog(SlotMachineConfig _config)
        {
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<HighRollerDialog>(uiPopupStrategy: new SystemUIPopupStrategy(), dialogInitCallBack:
                    (dialog) => { dialog.InitData(_config.HighRollerDisplayTime); }, data: _config));
        }


        protected override void openJackpotBetChangeDialog()
        {
//			UIManager.Instance.Open<JackpotBetChangeDialog> (false);
        }

        protected override void OnOpenInvalidCodeDialog()
        {
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<InvalidCodeDialog>(uiPopupStrategy: new SystemUIPopupStrategy()));
        }

        protected override void OnOpenExpirePromoCodeDialog()
        {
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<ExpirePromoCodeDialog>(uiPopupStrategy: new SystemUIPopupStrategy()));
        }

        protected override void OnOpenCodeAlreadyClaimedDialogMsg()
        {
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<CodeAlreadyClaimedDialog>(uiPopupStrategy: new SystemUIPopupStrategy()));
        }

        protected override void OnOpenPopRewardSmallDialog(int cash, Action callback)
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<SmallRewardPopDialog>(isPortrait,uiPopupStrategy: new MachineUIPopupStrategy(),
                    dialogInitCallBack: (dialog) => { dialog.SetUIData(cash, callback); }));
        }

        protected override void OnOpenPopRewardBigDialog(int cash,Action closeCallBack)
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<LuckyCashDialog>(isPortrait,uiPopupStrategy: new MachineUIPopupStrategy(),
                    dialogInitCallBack: (dialog) =>
                    {
                        dialog.SetUIData(cash);
                    },dialogCloseCallBack:() =>
                    {
                        closeCallBack();
                    }));
        }
        protected override void OnOpenGetMoreCashDialog(int cash)
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<GetMoreCashDialog>(isPortrait,openType:OpenType.AtOnce,uiPopupStrategy: new MachineUIPopupStrategy(),
                    dialogInitCallBack: (dialog) =>
                    {
                        dialog.SetUIData(cash);
                    }));
        }
        protected override void OnOpenWithDrawDialog()
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<WithDrawDialog>(isPortrait,uiPopupStrategy: new MachineUIPopupStrategy()));
        }
        protected override void OpenNewUserGuidDialog()
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<NewUserGuideDialog>(isPortrait,uiPopupStrategy: new MachineUIPopupStrategy()));
        }
        protected override void OpenSpinWithDrawStartDialog(int spinCount,int cash,Action callback)
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<SpinWithDrawStartDialog>(isPortrait,uiPopupStrategy: new MachineUIPopupStrategy(),dialogInitCallBack: (dialog) =>
                {
                    dialog.SetUIData(spinCount,cash);
                },dialogCloseCallBack:callback,defaultResourcePath:"SpinWithDraw/Prefab/SpinWithDrawStartDialog"));
        }
        protected override void OpenSpinWithDrawEndDialog(int cash,Action callback)
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<SpinWithDrawEndDialog>(isPortrait,uiPopupStrategy: new SystemUIPopupStrategy(),dialogInitCallBack: (dialog) =>
                {
                    dialog.SetUIData(cash);
                },dialogCloseCallBack:callback,defaultResourcePath:"SpinWithDraw/Prefab/SpinWithDrawEndDialog"));
        }
        protected override void OpenAccountDialog(int index,int cash)
        {
            UIDialog dialog = UIManager.Instance.GetActiveDialog();
            OpenType type = dialog == null ? OpenType.Normal : OpenType.FrontOfHead;
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<AccountDialog>(isPortrait,dialog.eId,openType:type,uiPopupStrategy: new SystemUIPopupStrategy(),dialogInitCallBack: (dialog) =>
                {
                    dialog.SetUIData(index,cash);
                }));
        }
        protected override void OpenAccountEnsureDialog(int index,string account,int cash)
        {
            UIDialog dialog = UIManager.Instance.GetActiveDialog();
            OpenType type = dialog == null ? OpenType.Normal : OpenType.FrontOfHead;
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<AccountEnsureDialog>(isPortrait,dialog.eId,openType:type,uiPopupStrategy: new SystemUIPopupStrategy(),dialogInitCallBack: (dialog) =>
                {
                    dialog.SetUIData(index,account,cash);
                }));
        }
        
        protected override void OpenAccountLoginTipsDialog(int cash)
        {
            UIDialog dialog = UIManager.Instance.GetActiveDialog();
            OpenType type = dialog == null ? OpenType.Normal : OpenType.FrontOfHead;
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<AccountLoginTipsDialog>(isPortrait,dialog.eId,openType:type,uiPopupStrategy: new SystemUIPopupStrategy(),dialogInitCallBack: (dialog) =>
                {
                    dialog.SetUIData(cash);
                }));
        }
        
        protected override void OpenCardSystemCollectionDialog()
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<CardSystemCollectionDialog>(isPortrait,uiPopupStrategy: new SystemUIPopupStrategy(),defaultResourcePath:"CardSystem/Prefab/CardSystemCollectionDialog"));
        }
        protected override void OpenCardSystemGetCardDialog(int cardId,GameObject parent)
        {
            UIDialog dialog = UIManager.Instance.GetActiveDialog();
            OpenType type = dialog == null ? OpenType.Normal : OpenType.FrontOfHead;
            int eid = dialog == null ? 0 : dialog.eId;
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<CardSystemGetCardDialog>(isPortrait,eid,openType:type,uiPopupStrategy: new SystemUIPopupStrategy(),dialogInitCallBack: (dialog) =>
                {
                    dialog.SetCardData(cardId,parent);
                },defaultResourcePath:"CardSystem/Prefab/CardSystemGetCardDialog"));
        }
        protected override void OpenCardSystemLuckyDrawDialog()
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<CardSystemLotteryDialog>(isPortrait,uiPopupStrategy: new SystemUIPopupStrategy(),defaultResourcePath:"CardSystem/Prefab/CardSystemLotteryDialog"));
        }     
        protected override void OpenRewardCashDialog(int cash)
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<RewardCashDialog>(isPortrait,uiPopupStrategy: new SystemUIPopupStrategy(),dialogInitCallBack: (dialog) =>
                {
                    dialog.SetUIData(cash);
                }));
        }
        
        protected override void OpenContinueSpinDialog(int activityId)
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<ContinueSpinDialog>(isPortrait,uiPopupStrategy: new SystemUIPopupStrategy(),dialogInitCallBack: (dialog) =>
                {
                    dialog.SetUIData(activityId);
                },defaultResourcePath:"ContinueSpin/Prefab/ContinueSpinDialog"));
        }
        
        protected override void OpenTipsDialog(string text)
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIManager.Instance.OpenSystemDialog(
                new OpenConfigParam<TipsDialog>(isPortrait,uiPopupStrategy: new SystemUIPopupStrategy(),dialogInitCallBack: (dialog) =>
                {
                    dialog.SetUIData(text);
                }));
        }
        protected override void OpenTaskTipsDialog()
        {
            bool isPortrait = BaseGameConsole.ActiveGameConsole().IsInSlotMachine() &&
                              SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait;
            UIDialog dialog = UIManager.Instance.GetActiveTipDialog<TaskTipDialog>();
            if (dialog!=null)
            {
                return;
            }
            // UIManager.Instance.OpenTips(new OpenConfigParam<KindOfSymbolDialog>(isPortrait, 0, OpenType.Normal,
            //     uiPopupStrategy: new MachineUIPopupStrategy(),
            //     dialogInitCallBack: (dialog) => { dialog.KindInit(sprite, count); }, dialogCloseCallBack: onDialogClose,
            //     animationIn: UIAnimation.NOAnimation, animationOut: UIAnimation.NOAnimation,
            //     queueId: Constants.UI_TIPS_EVENT_KEY));
            UIManager.Instance.OpenTips(new OpenConfigParam<TaskTipDialog>(isPortrait, 0, OpenType.Normal,
                uiPopupStrategy: new MachineUIPopupStrategy(), animationIn: UIAnimation.NOAnimation, animationOut: UIAnimation.NOAnimation,
                queueId: Constants.UI_TIPS_EVENT_KEY));
        }
    }
}