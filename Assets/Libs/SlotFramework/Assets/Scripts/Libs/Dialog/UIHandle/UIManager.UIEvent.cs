using UnityEngine;
using System.Collections.Generic;
using System;
using RealYou.Core.UI;
using RealYou.Utility.AsyncOperation;
using UniRx.Async;
using UnityEngine.UI;
using IAwaiter = RealYou.Utility.AsyncOperation.IAwaiter;

namespace Libs
{
    public partial class UIManager
    {
        #region POC
        private GameObject loadingPanel;
        #endregion
        
	    #region POC

		private GameObject loadingUI = null;
		private UIExceptionHandler exceptionHandler;
		private bool showLoading = false;
		private int eventID = 0;
		private const int MAX_QUEUE_COUNT = 256;

		public void Init()
		{
			//Mask Fade时间
			 fadeInDuration =  Plugins.Configuration.GetInstance().GetValueWithPath<float>("ApplicationConfig/MaskConfig/FadeInDuration", 0.1f);
			 fadeOutDuration =  Plugins.Configuration.GetInstance().GetValueWithPath<float>("ApplicationConfig/MaskConfig/FadeOutDuration", 0.3f);
			 startAlpha =  Plugins.Configuration.GetInstance().GetValueWithPath<float>("ApplicationConfig/MaskConfig/StartAlpha", 0.2f);
			 _waitEventDict.Clear();
		}
		private int GetID()
		{
			return (++eventID % MAX_QUEUE_COUNT);
		}

		/// <summary>
		/// 仅用于机器逻辑弹窗，不包括Alert Error 切换场景情况
		/// </summary>
		/// <param name="dialogInitCallBack"></param>
		/// <param name="dialogCloseCallBack"></param>
		/// <param name="openType"></param>
		/// <param name="maskAlpha"></param>
		/// <param name="inAnimation"></param>
		/// <param name="outAnimation"></param>
		/// <param name="eId"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>

		public int OpenMachineDialog<T>(Action<T> dialogInitCallBack, Action dialogCloseCallBack,OpenType openType = OpenType.Normal,float maskAlpha = 0.7f,UIAnimation inAnimation = UIAnimation.Scale,UIAnimation outAnimation = UIAnimation.Scale,int eId = 0, string defaultResourcePath = null) where T : UIDialog
		{
			return OpenExs(new OpenConfigParam<T>(eid:eId,openType:openType,uiPopupStrategy:new MachineUIPopupStrategy(),
				dialogInitCallBack:dialogInitCallBack,
				dialogCloseCallBack:dialogCloseCallBack,animationIn:inAnimation,animationOut:outAnimation,maskAlpha:maskAlpha, defaultResourcePath: defaultResourcePath));
		}

		/// <summary>
		/// 仅用于机器逻辑弹窗，不包括Alert Error 切换场景情况
		/// </summary>
		/// <param name="dialogInitCallBack"></param>
		/// <param name="dialogCloseCallBack"></param>
		/// <param name="openType"></param>
		/// <param name="maskAlpha"></param>
		/// <param name="inAnimation"></param>
		/// <param name="outAnimation"></param>
		/// <param name="eId"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IAwaiter OpenMachineDialogAsync<T>(Action<T> dialogInitCallBack, Action dialogCloseCallBack,OpenType openType = OpenType.Normal,float maskAlpha = 0.7f,UIAnimation inAnimation = UIAnimation.Scale,UIAnimation outAnimation = UIAnimation.Scale,int eId = 0,string defaultResourcePath=null) where T : UIDialog
		{
			return OpenAsync(new OpenConfigParam<T>(eid:eId,openType:openType,uiPopupStrategy:new MachineUIPopupStrategy(),
				dialogInitCallBack:dialogInitCallBack,
				dialogCloseCallBack:dialogCloseCallBack,animationIn:inAnimation,animationOut:outAnimation,maskAlpha:maskAlpha, defaultResourcePath: defaultResourcePath));
		}
		
		/// <summary>
		/// 仅用于系统功能逻辑弹窗，不包括Alert Error 切换场景情况
		/// </summary>
		/// <param name="dialogInitCallBack"></param>
		/// <param name="dialogCloseCallBack"></param>
		/// <param name="openType"></param>
		/// <param name="maskAlpha"></param>
		/// <param name="inAnimation"></param>
		/// <param name="outAnimation"></param>
		/// <param name="eId"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int OpenSystemDialog<T>(OpenConfigParam<T> p) where T : UIDialog
		{
			return OpenExs<T>(p);
		}

		/// <summary>
		/// 只能是Loading、切换场景、Alert类、购买成功使用，其他不允许使用AtOnce
		/// </summary>
		/// <param name="p"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int OpenHighLevelDialog<T>(OpenConfigParam<T> p) where T : UIDialog
		{
			return OpenExs<T>(p);
		}
		
		/// <summary>
		/// 禁止UIManager类外部直接使用此方法 系统组使用OpenSystemDialog，机器组使用OpenMachineDialogAsync or OpenMachineDialog
		/// 改动此方法，必须经过讨论，严谨私自修改
		/// </summary>
		/// <param name="p"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private IAwaiter OpenAsync<T>(OpenConfigParam<T> p) where T : UIDialog
		{
			if (p == null)
				return CompletedAsyncOperation.DefaultObject;
			
			SimpleAsyncOperation simpleAsyncOperation = new SimpleAsyncOperation();

			
			Action closeCb = p.dlgCloseCB;
			
			p.dlgCloseCB = () =>
			{
				closeCb?.Invoke();
				simpleAsyncOperation.Done();
			};
			
			OpenExs<T>(p);
			return simpleAsyncOperation;
		}

		
		/// <summary>
		/// 禁止UIManager类外部直接使用此方法 系统组使用OpenSystemDialog，机器组使用OpenMachineDialogAsync or OpenMachineDialog
		/// 改动此方法，必须经过讨论，严谨私自修改
		/// </summary>
		/// <param name="p"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private int Open<T> (OpenConfigParam<T> p) where T : UIDialog
		{

			if (!CheckMutexCondition(p)) return 0;

			CheckLoading<T>(p);

			string T_Name = p.GetTName();
			int currentID = p.eId;
			int eID = GetID();
			p.eId = eID;
			if(p.PopupStrategy ==null) p.PopupStrategy = new DefaultUIPopupStrategy();

			if (!p.isTipsUI)//Tips和Dialog的区别是Tips不计数，默认加载位置不同
			{
				//统计当前对话框数
				dialogCount++;
				dialogNames.Add(T_Name);//监控功能使用
				StartMonitor();

				Action<bool> decreaseCB = (suc) =>
				{
					dialogCount -= 1;
					dialogCount = Mathf.Max(dialogCount,0);
					int findIdx = dialogNames.FindIndex((name) => name.Equals(T_Name));
					if(findIdx!=-1){
						dialogNames.RemoveAt(findIdx);//监控功能使用
					}
					if(dialogCount == 0) StopMonitor();
				};
				if (p.runEndCB == null) p.runEndCB = decreaseCB;
				else p.runEndCB += decreaseCB;
			}
			
			Action<Action<int>> OpenCB = action =>
			{
				p.RemoveCB = action;
				
				if (!p.PopupStrategy.CanPopup())
				{
					p.ExcuteEndEvent(false,eID);
					return;
				}
				try
				{
					if (p.startRunCB != null) p.startRunCB();
				}
				catch (Exception e)
				{
					PopUpUILogES.SendError2ES($"UIMgr StartRunCB Error {T_Name} Msg:"+e.Message+" Stack:"+e.StackTrace);
					p.ExcuteEndEvent(false,eID);
					return;
				}
				
				//ShowLoading(suspendDialog:false); 不添加Loading是因为有时弹窗反应速度太快，会出现闪的情况，根据业务逻辑按需设置展示Loading弹窗
				p.readyAssetCo = CoroutineUtil.Instance.StartCoroutine(p.AssetLoad.LoadRes<T>(p, (newui) =>
				{
					UIDialog uiDialog = null;
					try
					{
						uiDialog = newui;
						if (uiDialog != null)
						{
							uiDialog.isNew = true;
							uiDialog.skipCloseAll = p.skipCloseAll;
							uiDialog.forceSkipCloseAll = p.forceSkipCloseAll;
							uiDialog.bundleName = p.bundleName;
						}
						
						if(!p.isTipsUI) HideLoading(); //目前调用此处能关闭Loading的只有比Loading优先级高的弹窗，此处才会生效，正常情况下，不能够靠此处关闭Loading 必须从外部处理

						newui.transform.parent.SetAsLastSibling();
						newui.OnDialogCloseCallBack = p.dlgCloseCB;
						newui.EndCallback = () => { p.ExcuteEndEvent(true,eID); };
						
						if (p.aniIn != null) newui.UIAnimationIn = p.aniIn;
						if (p.aniOut != null) newui.UIAnimationOut = p.aniOut;

						if (p.data != null) newui.SetData(p.data);//newui 可能因为数据或者ui元素检测不通过页面会被销毁
						if (p.dlgInitCB != null)
						{
							if (newui !=null) p.dlgInitCB(newui);//newui 防止SetData里有异常销毁了 对话框对象，后续方法不再执行
						}

						if (p.isTipsUI)
						{
							if (newui !=null) activeTipsList.Add(newui);//newui 防止SetData 或者dlgInitCB里有异常销毁了 对话框对象，后续方法不再执行
						}
						else
						{
							if (newui !=null) activeList.Add(newui);//newui 防止SetData 或者dlgInitCB 里有异常销毁了 对话框对象，后续方法不再执行
						}
					}
					catch (Exception e)
					{
						PopUpUILogES.SendError2ES($"UIMgr LoadRes Exception{T_Name}:"+e.Message+" stack:"+e.StackTrace);
						if(uiDialog!=null)
						{
							if (p.isTipsUI)
							{
								GameObject.DestroyImmediate(GetTipsRootNodeGo<UIDialog>(uiDialog.gameObject)); //不适用Destroy方法，是因为调用此方法不能帧内销毁，后面判断不生效
							}
							else
							{
								GameObject.DestroyImmediate(GetDlgRootNodeGo<UIDialog>(uiDialog.gameObject)); //不适用Destroy方法，是因为调用此方法不能帧内销毁，后面判断不生
							}
						}
						p.ExcuteEndEvent(false,eID);
					}
					
				}, (error) =>
				{
					//一旦实例化后出错，对dialog UI对象进行销毁并刷新mask，防止阻塞用户不能继续操作
					p.ExcuteEndEvent(false,eID);
					PopUpUILogES.SendError2ES($"UIMgr LoadRes Error {T_Name}:"+error);
				}));
			};
			
			CreateUIEvent(p.queueId,p.OpenType, eID, T_Name, p.type, OpenCB, currentID, p.pauseMachine, p.runEndCB, p.skipCloseAll,null,p.forceSkipCloseAll);
			return eID;
		}

		private int OpenExs<T>(OpenConfigParam<T> param) where T : UIDialog
		{
			
#if GAME_SLOTS
			maskAlpha = 0.80f;
#endif
			if (param == null)
				return 0;
			if (param.type == null)
			{
				PopUpUILogES.SendError2ES($"Open<T> {typeof(T).Name} type is null");
				param.type = "";
			}
			if (param.AssetLoad == null)
			{                                                                                                                                                                                                            
				param.AssetLoad = new DefaultAssetLoad();
			}
			string T_Name = param.GetTName();
			param.RemoteBundleName = T_Name;
			if (string.IsNullOrEmpty(param.defaultResourcePath)) param.defaultResourcePath = GetDefaultResPath(param);
			Debug.Log($"OpenExs defaultResourcePath:{param.defaultResourcePath}");
			if (param.isPortrait)
			{
				if (!param.defaultResourcePath.EndsWith(".prefab"))
				{
					param.defaultResourcePath += "_p";
					param.RemoteBundleName += "_p";
				}
			}
			Open<T>(param);
			return param.eId;
		}
		private void CreateUIEvent(string queue_id, OpenType inQueueType, int eID,string uiName,string type,Action<Action<int>> doCB,int currentID,bool pauseMachine,Action<bool> runEndCB,bool skipCloseAll,Action<Action<int>> initCB,bool forceSkipCloseAll)
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict.Add(Constants.E_ID_KEY,eID);
			dict.Add(Constants.E_UI_NAME_KEY,uiName);
			dict.Add(Constants.E_TYPE_KEY,type);
			dict.Add(Constants.E_OPEN_CB_KEY,doCB);
			dict.Add(Constants.E_INIT_CB_KEY,initCB);
			dict.Add(Constants.E_CUR_E_ID_KEY,currentID);
			dict.Add(Constants.E_PAUSE_KEY,pauseMachine);
			dict.Add(Constants.RUN_END_KEY,runEndCB);
			dict.Add(Constants.E_SKIP_CLEAR_ALL_KEY,skipCloseAll);
			dict.Add(Constants.E_FORCE_SKIP_CLEAR_ALL_KEY,forceSkipCloseAll);
			dict.Add(Constants.E_EVENT_QUEUE_ID_KEY,queue_id);
			CreateEventUI(inQueueType,dict);
		}
		public int OpenTips<T> (OpenConfigParam<T> p) where T : UIDialog
		{
			if (p == null || tips == null)
			{
				PopUpUILogES.SendError2ES($"UIMgr OpenTips Error {typeof(T).Name}: tips is null:{tips==null}");
				return 0;
			}
				
			p.isTipsUI = true;
			return OpenExs<T>(p);
		}
		
		/// <summary>
		/// Loading 目前只阻塞UIDialog事件队列，对UITips事件队列不阻塞
		/// </summary>
		/// <param name="duratiion"></param>
		/// <param name="title"></param>
		/// <param name="lockId"></param>
		/// <param name="endCB"></param>
		/// <param name="suspendDialog"></param>
		/// <param name="delayShow"></param>
		private async void ShowLoading(float duratiion = 30f,string title = "",int lockId = 0,Action endCB = null,bool suspendDialog= true,float delayShow =0f)
		{
			Debug.Log("Show Loading UI");
			if (loadingUI == null) return;
			if (!showLoading)
			{
				bool enableUI = false;
				showLoading = true;
				if (!loadingUI.activeSelf)
				{
					Debug.Log("Show Loading UI activeself");
					loadingUI.SetActive(true);
					enableUI = true;
				}
				if (loadingUI != null)
				{
					//尽量不要使用协程，协程可能会被暂停掉，受time.timescale影响
					Loading loading = loadingUI.GetComponent<Loading>();
					if (loading != null)
					{
						if (suspendDialog)
						{
							int eID = GetID();
							Action<Action<int>> initCB = action => { loading.SetEventUICB(action,eID); };
							Action<Action<int>> runCB = action => { };
							Debug.Log("Show Loading UI CreateUIEvent");
							CreateUIEvent(Constants.UI_DIALOG_EVENT_KEY,OpenType.AtOnce, eID, "Loading", Constants.LOADING, runCB, 0, false, null, false,initCB,false);
						}

						loading.Init(duratiion, title, lockId, endCB, () => { HideLoading(forceHide:true);});
					}
				}
				
				if (enableUI)
				{
					if (delayShow > 0.001f)
					{
						CanvasGroup cgroup = loadingUI.GetComponent<CanvasGroup>();
						if(cgroup != null)cgroup.alpha = 0;
						int delay = (int) (((delayShow + 1) > duratiion ? Mathf.Max(duratiion - 1,0) : delayShow) * 1000);
						await UniTask.Delay(delay);
						if(cgroup != null)cgroup.alpha = 1;
					}
				} 
			}
		}
		private void HideLoading(int lockId = 0,bool forceHide = false)
		{
			if (loadingUI == null) return;
			if (showLoading)
			{
				Loading loading = loadingUI.GetComponent<Loading>();
				if (loading != null&&(!forceHide&&!loading.CheckHideLoadingUI(lockId)))
					return;
				showLoading = false;
				loading.DoHideEvent();
				if(loadingUI.activeSelf) loadingUI.SetActive(false);
			}
		}
		public static void ShowLoadingUI(float duration = 30f,string title = "",int lock_id = 0,Action endCB = null,bool suspendDialog = true,float showDelay = 0.5f)
		{
			Instance.ShowLoading(duration,title,lock_id,endCB,suspendDialog,showDelay);
		}

		public static void HideLoadingUI(int lockId = 0)
		{
			Instance.HideLoading(lockId);
		}
		
		#region 等待弹框的事件

		/// <summary>
		/// 在Tips队列中插入一个事件，事件在有Dialog时不执行;
		/// 轮到事件执行时，先回调NeedDo判断是否还需要执行；
		/// 如果需要，给业务层回调doCB，然后等待duration秒后执行下一个事件
		/// 如果不需要，直接执行下一个事件
		/// </summary>
		/// <param name="waitEvent"></param>
		public static void AddWaitDialogEventToTipQueue(IWaitDialogEvent waitEvent)
		{
			Instance.addWaitDialogEvent(waitEvent);
		}

		private Dictionary<string, WaitEvent> _waitEventDict = new Dictionary<string, WaitEvent>();
		private void addWaitDialogEvent(Libs.IWaitDialogEvent waitDialogEvent)
		{
			if(waitDialogEvent == null)
				return;
			int eID = GetID();
			Action<Action<int>> runCB = action =>
			{
				WaitEvent waitEvent = new WaitEvent(Constants.UI_TIPS_EVENT_KEY, eID, waitDialogEvent);
				// 如果Dialog队列空的, 直接执行
				if (UIEventMgr.Instance.IsSequenceEmpty(Constants.UI_DIALOG_EVENT_KEY))
				{
					waitEvent.DoEvent();
				}
				else // 否则，等待Dialog队列为空时的广播再执行
				{
					_waitEventDict[Constants.UI_DIALOG_EVENT_KEY] = waitEvent;
				}
			};
			CreateUIEvent(Constants.UI_TIPS_EVENT_KEY,OpenType.Normal, eID, "WaitDialogEvent", 
				"", runCB, 0, false, null, 
				false,null,false);
		}

		/// <summary>
		/// 当UIEvent队列为空时的回调
		/// </summary>
		/// <param name="sequenceID">
		/// 	队列ID
		/// </param>
		private void onUIEventSequenceEmpty(string sequenceID)
		{
			if(!_waitEventDict.ContainsKey(sequenceID))
				return;
			WaitEvent waitEvent = _waitEventDict[sequenceID];
			waitEvent?.DoEvent();
			_waitEventDict.Remove(sequenceID);
		}
		
		#endregion
		
		
		private void CreateLoadingGo()
		{
			#region POC
			if (loadingPanel == null)
			{
				GameObject go = new GameObject("LoadingPanel");
				go.layer = 8;//Dialog 层
				GameObject parent =  GameObject.Find("DialogCamera/DialogCanvas/");
				go.transform.SetParent(parent.transform);
				go.transform.SetAsLastSibling();
				RectTransform Trans = go.GetComponent<RectTransform>();
				if (Trans == null) Trans = go.AddComponent<RectTransform>();

				Trans.localScale = Vector3.one;
				Trans.localRotation = Quaternion.identity;
				Trans.anchorMin = new Vector2(-0.5f,-0.5f);
				Trans.anchorMax = new Vector2(1.5f,1.5f);
				Trans.offsetMin = Vector2.zero;
				Trans.offsetMax = Vector2.zero;
				Trans.localPosition = Vector3.zero;
				loadingPanel = go;
			}
			//后续可以更改为Bundle或者从包内读取
			if (loadingUI == null)
			{
				GameObject prefab = Resources.Load<GameObject>("Prefab/Shared/Loading");
				loadingUI = GameObject.Instantiate(prefab,loadingPanel.transform);
				loadingUI.name = "Loading";//对象全局不销毁
				Util.SetChildrenLayer(loadingUI.transform,LayerMask.NameToLayer ("Dialog"));
				loadingUI.SetActive(false);
				showLoading = false;
			}
			#endregion
		}

		private void AddExceptionHandlerMonitor()
		{
			if (root!=null)
			{
				exceptionHandler = root.GetComponent<UIExceptionHandler>();
				if (exceptionHandler == null)
				{
					exceptionHandler = root.AddComponent<UIExceptionHandler>();
				}
			}
		}

		private void StartMonitor()
		{
			if(exceptionHandler!=null) exceptionHandler.StartMonitor();
		}

		private void StopMonitor()
		{
			if(exceptionHandler!=null) exceptionHandler.StopMonitor();
		}

		private List<string> dialogNames = new List<string>();
		private void CheckUpUIException()
		{
			Action<int> callback = (num) =>
			{
				if (dialogCount > num)
				{
					string dialogStr = "";
					for (int i = 0; i < dialogNames.Count; i++)
					{
						dialogStr += " "+dialogNames[i];
					}
					PopUpUILogES.SendError2ES($"CheckUpUIException dialogCount not right:dialogNum:{dialogCount}>uiEventNum:{num} currentDlgNames:{dialogStr}! but has fixed by exception hander");
					dialogNames.Clear();
					dialogCount = 0;
					StopMonitor();
				}
			};
			Messenger.Broadcast<string,Action<int>>(GameConstants.GET_UI_EVENT_NUM_KEY,Constants.UI_DIALOG_EVENT_KEY,callback);
		}
		
		private string GetDefaultResPath<T>(OpenConfigParam<T> p) where T:UIBase
		{
			string T_Name = p.GetTName();

			string defaultPath = p.isTipsUI?(UITipsPrefabLocalPath + T_Name) :(UIPrefabLocalPath + T_Name);
			
			return p.defaultResourcePath?? defaultPath;
		}
		public void CreateGameObjectUI<T>(OpenConfigParam<T> p,GameObject prefab,string slotName,Action<T> callBack,Action<string> failedCB) where T : UIBase
		{
			bool canPopup = p.PopupStrategy.CanPopup();
			if (!canPopup)
			{
				failedCB($"canPopup is false");
				return;
			}
			if(prefab == null){
				failedCB($"{slotName} prefab is null");
				Dictionary<string, object> eventParams = new Dictionary<string, object>();
				eventParams.Add("PrefabName", p.GetTName());
				eventParams.Add("ErrorType", "CacheAssetBundleLoadFailed");
				BaseGameConsole.ActiveGameConsole().LogBaseEvent("AssetBundleLoadFailed", eventParams);
				return;
			}
			
			GameObject panel = GameObject.Instantiate (prefab);
			if(panel == null)
			{
				failedCB($"{slotName} go is null");
				Dictionary<string, object> eventParams = new Dictionary<string, object>();
				eventParams.Add("PrefabName", p.GetTName());
				eventParams.Add("ErrorType", "PrefabInstantiateFailed");
				BaseGameConsole.ActiveGameConsole().LogBaseEvent("AssetBundleLoadFailed", eventParams);
				return;
			}
			if(p.isTipsUI&& tips == null)
			{
				failedCB($"{slotName} Tips is null");
				GameObject.DestroyImmediate(panel);
				return;
			}
			if (!p.isTipsUI && root == null)
			{
				failedCB($"{slotName} root is null");
				GameObject.DestroyImmediate(panel);
				return;
			}
			
			panel.name = p.GetTName();
			T ui;
			if (panel.GetComponent<T> () != null) ui = panel.GetComponent<T> ();
			else ui = panel.AddComponent<T> ();
			
			ui.CurPrefabName = prefab.name;
			if (ui is UIDialog dialog)
			{
				dialog.eId = p.eId;
				dialog.m_Data = p.data;
			}
			
			if (typeof(T).Name ==typeof(SceneExchange).Name||typeof(T).Name ==typeof(FirstToLobbyDialog).Name)
			{
				SwapSceneManager.Instance.SetScreenOrientation(p.data);
				Messenger.Broadcast(DialogOrientationAdapter.ADJUST_SCREEN_ORIENTATION_SETTINGS);
				AddDialogOrientationAdapter(panel.gameObject);
			}
			
			SetUIPosition(p, panel);
			CreateUIMask(p,ui as UIDialog);
			
			HideBackUIParticles();
			
			callBack (ui);//防止Init回调内包含关于查找父对象的代码，所以在执行此代码之前，必须设置父对象
			
		}

		private void HideBackUIParticles()
		{
			if (activeList!=null && activeList.Count >= 1) {
				bool meetModel = false;
				
				for(int i=activeList.Count-1; i>=0;i--){
					UIDialog currentDialog = activeList [i];
					
					if (currentDialog == null)
						continue;

					if (meetModel)
					{
						currentDialog.HideShowingParticles();
						continue;
					}

					if(currentDialog.NeedShowMask()) 
						meetModel = true;

				}
			}
		}

		private void ShowFrontUIParticles()
		{
			if (activeList!=null && activeList.Count >= 1) {
				bool meetModel = false;
				
				for(int i=activeList.Count-1; i >= 0;i--){
					UIDialog currentDialog = activeList [i];
					
					if (currentDialog == null)
						continue;

					if (meetModel)
					{
						continue;
					}

					if(currentDialog.NeedShowMask())
						meetModel = true;
					currentDialog.ShowParticles();
				}
			}
		}
		private void CreateUIMask<T>(OpenConfigParam<T> p,UIDialog ui)
		{
			if (ui == null) return;
			if (ui.NeedShowMask())
			{   
				//0.在dummy节点下创建mask对象
				GameObject maskGo = new GameObject("Mask");
				maskGo.transform.SetParent(ui.transform.parent);
				RectTransform mTrans = maskGo.transform as RectTransform;
				if (mTrans == null) 
					mTrans = maskGo.AddComponent<RectTransform>();
				Vector3 scale =  ui.transform.parent.localScale;
				mTrans.localScale = new Vector3(2/scale.x,2/scale.y,1);//保证IPad iPhoneXMask足够大
				mTrans.anchorMin = Vector2.zero;
				mTrans.anchorMax = Vector2.one;
				mTrans.offsetMin = Vector2.zero;
				mTrans.offsetMax = Vector2.zero;
				mTrans.localPosition = Vector3.zero;
				
				//1.检测当前对话框的根节点是否存在Canvas，存在的话获取层级，给mask添加canvas，并设置mask的canvas层级，不存在canvas，则无需设置

				//Tips必须有Canvas或者sortingOrder的相关组件，否则设置会使用dialog的默认层级
				int masklayerId = GetUILowestLayerId(ui.gameObject);
				
				if (masklayerId > UI_LAYER_ID)
				{
					Canvas cvs = ui.gameObject.GetComponent<Canvas>();
					if (cvs == null) masklayerId = UI_LAYER_ID;
				}

				if (p.isTipsUI||(!p.isTipsUI&& masklayerId != UI_LAYER_ID))
				{
					Canvas maskCvs = maskGo.AddComponent<Canvas>();
					maskCvs.overrideSorting = true;
					maskCvs.sortingOrder = masklayerId-1;
					maskGo.AddComponent<GraphicRaycaster>();
				}
				
				Util.SetChildrenLayer(mTrans,ui.gameObject.layer);

				
				//2.将创建的mask对象设置为FirstSibling
				mTrans.SetAsFirstSibling();
				//3.将maskui对象指向给UIDialog的MaskUI参数
				ui.maskUI = maskGo.AddComponent<MaskUI>();
				//UIDialog 开始渐变， 关闭时 开始渐变 销毁时，一起销毁
				if (!ui.EnableInspectorMaskAlpha) 
					ui.maskAlpha = p.maskAlpha;
				
				if (ui.enableFadeMask)
				{
					ui.maskUI.SetAlpha(ui.startAlpha,ui.maskAlpha);
					//对话框自身需要设置的话，则专有设置即可，否则走通用
					ui.maskUI.SetFadeDuration(ui.maskFadeIn,ui.maskFadeOut);
				}
				else
				{
					ui.maskUI.SetAlpha(startAlpha,ui.maskAlpha);
					ui.maskUI.SetFadeDuration(fadeInDuration,fadeOutDuration);
				}
				ui.maskUI.FadeIn();//此处调用是防止部分对话框没有执行基类UIWindow的Start方法，导致无法添加mask image，销毁没有调用不会播放渐变动画，但是会被销毁
			}
		}

		private const int UI_LAYER_ID = 10;

		private float fadeInDuration = 0.3f;
		private float fadeOutDuration = 0.3f;
		private float startAlpha = 0.2f;
		private int GetUILowestLayerId(GameObject go)
		{
			int minUIOrder = Int16.MaxValue;
			if (go != null)
			{
				List<ParticleSystem> list = Util.FindChildrenIn<ParticleSystem>(go);
				if (list != null)
				{
					foreach (var ps in list)
					{
						Renderer renderer =  ps.GetComponent<Renderer>();
						if(renderer==null) continue;
						if (renderer.sortingOrder < minUIOrder)
						{
							minUIOrder = renderer.sortingOrder;
						}
					}
				}
				SpriteRenderer[] srs = go.GetComponentsInChildren<SpriteRenderer>();
				if (srs != null)
				{
					foreach (var ps in srs)
					{
						if(ps==null) continue;
						if (ps.sortingOrder < minUIOrder)
						{
							minUIOrder = ps.sortingOrder;
						}
					}
				}
				List<Canvas> cvses = Util.FindChildrenIn<Canvas>(go);
				if (cvses != null)
				{
					foreach (var cvs in cvses)
					{
						if(cvs==null) continue;
						if (cvs.sortingOrder < minUIOrder)
						{
							minUIOrder = cvs.sortingOrder;
						}
					}
				}
			}
			return minUIOrder == Int16.MaxValue ? UI_LAYER_ID : minUIOrder;
		}
		private void SetUIPosition<T>(OpenConfigParam<T> p,GameObject ui)
		{
			if (p.isTipsUI)
			{
				ui.transform.SetParent (GetDummyNodeBetweenTipsPanelAndDlg<T>(ui), false);
			}
			else
			{
				ui.transform.SetParent (GetDummyNodeBetweenRootAndDlg<T>(ui,p), false);
			}
		}

		private readonly Vector3 _rotateDir = new Vector3(0f, 0f, 270f);
		/// <summary>
		/// 获得对话框对象父节点(有Dummy返回dummy，没有dummy 返回Root节点)
		/// </summary>
		/// <param name="ui"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private Transform GetDummyNodeBetweenRootAndDlg<T>(GameObject ui,OpenConfigParam<T> p)
		{
			if (ui.transform.parent == null || ui.transform.parent == root.transform)
			{
				GameObject dummy = new GameObject("Anchor");
				dummy.transform.SetParent(root.transform);
					
				RectTransform dummyTrans = dummy.GetComponent<RectTransform>();
				if(dummyTrans==null)
					dummyTrans = dummy.AddComponent<RectTransform>();
				
				//弹窗未勾选 isportrait 需要旋转
				if (NeedRotateDummyNode<T>(ui.gameObject,p)&&typeof(T).Name !=typeof(SceneExchange).Name&&typeof(T).Name !=typeof(FirstToLobbyDialog).Name)
				{
					dummyTrans.localEulerAngles = _rotateDir;
				}
				else
				{
					dummyTrans.localEulerAngles = Vector3.zero;
				}
				//竖屏时，不是竖版的弹窗弹窗需要缩放比例
				float scale = SkySreenUtils.GetDlgScale();
				T dlg =ui.GetComponent<T>();
				if (dlg is UIDialog d)
				{
					scale = !d.isPortrait && !p.isLuaPortrait ?scale:1;
				}
				dummyTrans.localScale = new Vector3(scale,scale,1);
				dummyTrans.anchorMin = Vector2.zero;
				dummyTrans.anchorMax = Vector2.one;
				dummyTrans.offsetMin = Vector2.zero;
				dummyTrans.offsetMax = Vector2.zero;
				dummyTrans.localPosition = Vector3.zero;
					
				return dummyTrans;
			}

			return ui.transform.parent;
		}
		private Transform GetDummyNodeBetweenTipsPanelAndDlg<T>(GameObject ui)
		{
			if (ui.transform.parent == null || ui.transform.parent == tips.transform)
			{
				GameObject dummy = new GameObject("Anchor");
				dummy.transform.SetParent(tips.transform);
					
				RectTransform dummyTrans = dummy.GetComponent<RectTransform>();
				if(dummyTrans==null)
					dummyTrans = dummy.AddComponent<RectTransform>();

				dummyTrans.localScale = Vector3.one;
				dummyTrans.anchorMin = Vector2.zero;
				dummyTrans.anchorMax = Vector2.one;
				dummyTrans.offsetMin = Vector2.zero;
				dummyTrans.offsetMax = Vector2.zero;
				dummyTrans.localPosition = Vector3.zero;
					
				return dummyTrans;
			}

			return ui.transform.parent;
		}
		private bool NeedRotateDummyNode<T>(GameObject ui,OpenConfigParam<T> p)
		{
			T t = ui.GetComponent<T>();
			if (t != null)
			{
				UIDialog uiDlg = t as UIDialog;
				if (uiDlg != null&&!(uiDlg.isPortrait || p.isLuaPortrait)&&SkySreenUtils.CurrentOrientation== ScreenOrientation.Portrait)
				{
					return true;
				}
			}

			return false;
		}

		#region 互斥锁
	    private Dictionary<string,bool> mutexDict = new Dictionary<string, bool>();
		private bool CheckOpenCondition<T>(OpenConfigParam<T> p)
		{
			if (p == null)
			{
				return false;
			}

			switch (p.mutexType)
			{
				case MutexType.None:
					break;
				case MutexType.Name:
					if (string.IsNullOrEmpty(p.MutexToken))
						p.MutexToken = p.GetTName();
					if (!string.IsNullOrEmpty(p.MutexToken) && mutexDict.ContainsKey(p.MutexToken))
					{
						return false;
					}
					break;
				case MutexType.Custom:
					if (!string.IsNullOrEmpty(p.MutexToken) && mutexDict.ContainsKey(p.MutexToken))
					{
						return false;
					}
					break;
			}

			
			return true;
		}

		private void AddMutexToken2Cache(string mutexToken)
		{
			if (!string.IsNullOrEmpty(mutexToken)&&!mutexDict.ContainsKey(mutexToken))
			{
				mutexDict.Add(mutexToken,true);
			}
		}
		private void RemoveMutexTokenFromCache(string mutexToken)
		{
			if (!string.IsNullOrEmpty(mutexToken)&&mutexDict.ContainsKey(mutexToken))
			{
				mutexDict.Remove(mutexToken);
			}
		}

		private bool CheckMutexCondition<T>(OpenConfigParam<T> p)
		{
			if (!CheckOpenCondition(p))
			{
				//Debug.LogAssertion("CheckMutexCondition p:"+typeof(T).Name+" ");
				if(p!=null&&p.runEndCB!=null) p.runEndCB(false);
				return false;
			}
			
			AddMutexToken2Cache(p.MutexToken);
			
			if (p.runEndCB == null)
			{
				p.runEndCB = (suc) => { RemoveMutexTokenFromCache(p.MutexToken);};
			}
			else
			{
				p.runEndCB+=(suc) => { RemoveMutexTokenFromCache(p.MutexToken);};
			}

			return true;
		}
		#endregion

		#region Loading
		public void CheckLoading<T>(OpenConfigParam<T> p) where T : UIDialog
		{
			if (p.loadingType == LoadingLock.WithOutLock) return;
			int lock_id = (int) p.loadingType;
			if (p.startRunCB == null) p.startRunCB = () => UIManager.ShowLoadingUI(lock_id:lock_id,showDelay: p.delayShowLoading);
			else p.startRunCB += () => UIManager.ShowLoadingUI(lock_id:lock_id,showDelay: p.delayShowLoading);
			if (p.dlgInitCB == null) p.dlgInitCB = (dialog) => UIManager.HideLoadingUI(lock_id);
			else p.dlgInitCB += (dialog) => UIManager.HideLoadingUI(lock_id);
			if (p.runEndCB == null) p.runEndCB = (state) => { if (state == false) UIManager.HideLoadingUI(lock_id); };
			else  p.runEndCB += (state) => { if (state == false) UIManager.HideLoadingUI(lock_id); };
		}
		

		#endregion
		private bool CheckMachineState()
		{
			if (MachineUtility.Instance ==null||!MachineUtility.Instance.IsSpining()) return true;
			return false;
		}
		private void CreateEventUI(OpenType openType,Dictionary<string,object> dict)
		{
			switch (openType)
			{
				case OpenType.Normal:
					Messenger.Broadcast(Libs.Constants.ADD_UI_EVENT_KEY,dict);
					break;
				case OpenType.AtOnce:
					Messenger.Broadcast(Libs.Constants.INSERT_AT_HEAD_UI_EVENT_KEY,dict);
					break;
				case OpenType.BehindHead:
					Messenger.Broadcast(Libs.Constants.INSERT_BEHIND_HEAD_UI_EVENT_KEY,dict);
					break;
				case OpenType.FrontOfHead:
					Messenger.Broadcast(Libs.Constants.INSERT_IN_FRONT_OF_HEAD_UI_EVENT_KEY,dict);
					break;
				
				case OpenType.BehindCurrent:
					Messenger.Broadcast(Libs.Constants.INSERT_BEHIND_CURRENT_UI_EVENT_KEY,dict);
					break;
				case OpenType.InFrontOfCurrent:
					Messenger.Broadcast(Libs.Constants.INSERT_IN_FRONT_OF_CURRENT_UI_EVENT_KEY,dict);
					break;
				case OpenType.NormalGroup:
					Messenger.Broadcast(Libs.Constants.ADD_UI_EVENT_IN_SAME_GROUP_KEY,dict);
					break;
			}
		}
		#endregion

		#region 跳转机器

		/// <summary>
		/// 不对具有优先级的对话框进行限制，只是对常规类的进行处理
		/// </summary>
		/// <param name="uiName"></param>
		/// <param name="doCB"></param>
		/// <param name="runEndCB"></param>
		public int RegisterUIEventAtOnce(string uiName,out Action<int> finishedCB)
		{
			int eID = GetID();
			Action<int> endCB = null;
			Action<Action<int>> initCB = action => { endCB = action; };
			Action<Action<int>> runCB = action => { };
			CreateUIEvent(Constants.UI_DIALOG_EVENT_KEY, OpenType.FrontOfHead,eID,uiName,Constants.BLOCK_UI,runCB,0,false,null,true,initCB,false);
			finishedCB = endCB;
			return eID;
		}
		
		public void UnRegisterUIEventAtOnce(int eid,Action<int> finishedCB)
		{
			if (finishedCB != null) finishedCB(eid);
			//CreateUIEvent(OpenType.FrontOfHead,0,uiName,"",doCB,0,false,runEndCB,false,null,false);
		}

		#endregion
    }
    
    #region POC

    public class OpenConfigParam<T>
    {
	    public string queueId;
	    public OpenType OpenType;
	    public string type;
	    public Action<T> dlgInitCB;
	    public Action dlgCloseCB;
	    public object data;
	    public UIAnimation aniIn;
	    public UIAnimation aniOut;
	    public float maskAlpha;
	    public string defaultResourcePath;
	    public string bundleName;
	    public string RemoteBundleName;
	    public bool pauseMachine;

	    public IUIPopupStrategy PopupStrategy;
	    public IAssetLoad AssetLoad;
	    public int eId;
	    public Action<int> RemoveCB;
	    
	    public Coroutine readyAssetCo;
	    public List<string> bundles;

	    public Action startRunCB;
	    //100%结束事件回调 ---无论正常事件结束还是意外事件结束
	    public Action<bool> runEndCB;
	    
	    public bool isPortrait;
	    
	    /// <summary>
	    /// lua dialog专用的竖版属性，等同于dialog.isPortrait
	    /// </summary>
	    public bool isLuaPortrait;

	    public bool isTipsUI;
	    //orignRes newRes
	    public bool skipCloseAll = true;//只有广告和切场景使用，其他位置禁止使用
	    public bool forceSkipCloseAll = false;
	    public string PrefabName = string.Empty;

	    //互斥Token 主要用于弹窗去重和系统2级或者3级弹窗可以同时点击多次，弹多次问题
	    //使用规则:1.常量定义 2.系统名+"_"+弹窗基数 或者 弹窗名 eg missionpass_2 or missionpassdilalog 
	    public string MutexToken = string.Empty;
	    public MutexType mutexType = MutexType.None;
	    public LoadingLock loadingType = LoadingLock.WithOutLock;

	    public float delayShowLoading;
	    #region temp property

	    public bool newResourceeLoad;

	    #endregion
	    
	    public OpenConfigParam(int eid =0,OpenType openType = OpenType.Normal, string type = "",IUIPopupStrategy uiPopupStrategy = null, Action<T> dialogInitCallBack=null, Action dialogCloseCallBack=null, object data = null, UIAnimation animationIn =UIAnimation.Scale, UIAnimation animationOut =UIAnimation.Scale, float maskAlpha = 0.7f, string defaultResourcePath=null,string bundleName =null,List<string> bundles = null,bool pauseMachine = true,Action<bool> runEnd = null,bool isTips = false,Action startRunCB = null,MutexType mutexType = MutexType.None,string mutexToken = "",LoadingLock loadingType = LoadingLock.WithOutLock,float delayShowLoading = 0.5f,string queueId = Constants.UI_DIALOG_EVENT_KEY)
	    {
		    this.queueId = queueId;
		    this.OpenType = openType;
		    this.type = type;
		    this.dlgInitCB = dialogInitCallBack;
		    this.dlgCloseCB = dialogCloseCallBack;
		    this.data = data;
		    this.aniIn = animationIn;
		    this.aniOut = animationOut;
		    this.maskAlpha = maskAlpha;
		    this.defaultResourcePath = defaultResourcePath;
		    this.bundleName = bundleName;
		    this.PopupStrategy = uiPopupStrategy;
		    this.eId = eid;
		    this.pauseMachine = pauseMachine;
		    this.runEndCB = runEnd;
		    this.isPortrait = false;
		    this.isTipsUI = isTips;
		    
		    this.startRunCB = startRunCB;
		    this.mutexType = mutexType;
		    this.MutexToken = mutexToken;
		    this.loadingType = loadingType;
		    this.delayShowLoading = delayShowLoading;
	    }
	    public OpenConfigParam(bool isPortrait,int eid =0,OpenType openType = OpenType.Normal, string type = "",IUIPopupStrategy uiPopupStrategy = null, Action<T> dialogInitCallBack=null, Action dialogCloseCallBack=null, object data = null, UIAnimation animationIn =UIAnimation.Scale, UIAnimation animationOut =UIAnimation.Scale, float maskAlpha = 0.7f, string defaultResourcePath=null,string bundleName ="",List<string> bundles = null,bool pauseMachine = true,Action<bool> runEnd = null,bool isTips = false,Action startRunCB = null,MutexType mutexType = MutexType.None,string mutexToken = "",LoadingLock loadingType = LoadingLock.WithOutLock,float delayShowLoading = 0.5f,string queueId = Constants.UI_DIALOG_EVENT_KEY)
	    {
		    this.queueId = queueId;
		    this.OpenType = openType;
		    this.type = type;
		    this.dlgInitCB = dialogInitCallBack;
		    this.dlgCloseCB = dialogCloseCallBack;
		    this.data = data;
		    this.aniIn = animationIn;
		    this.aniOut = animationOut;
		    this.maskAlpha = maskAlpha;
		    this.defaultResourcePath = defaultResourcePath;
		    this.bundleName = bundleName;
		    this.PopupStrategy = uiPopupStrategy;
		    this.eId = eid;
		    this.pauseMachine = pauseMachine;
		    this.runEndCB = runEnd;
		    this.isTipsUI = isTips;
		    this.isPortrait = isPortrait;
		    this.startRunCB = startRunCB;
		    this.mutexType = mutexType;
		    this.MutexToken = mutexToken;
		    this.loadingType = loadingType;
		    this.delayShowLoading = delayShowLoading;

	    }
	    public void ExcuteEndEvent(bool suc,int eid)
	    {
		    try
		    {
			    if (runEndCB != null) runEndCB(suc);
			    if (RemoveCB != null) RemoveCB(eid);
		    }
		    catch (Exception e)
		    {
			    if (RemoveCB != null) RemoveCB(eid);
			    PopUpUILogES.SendError2ES("ExcuteEndEvent uiName:"+typeof(T).Name+" errorMsg:"+e.Message+" stack:"+e.StackTrace);
		    }
		   
	    }

	    //此处主要是防止Lua接口调用时，对话框类名为同一个，所以使用PrefabName为TName
	    public string GetTName()
	    {
		    return string.IsNullOrEmpty(PrefabName) ?  typeof(T).Name : PrefabName;
	    }
	    
	    public string GetCSVDialogName()
	    {
		    return string.IsNullOrEmpty(defaultResourcePath) ? RemoteBundleName : System.IO.Path.GetFileName(defaultResourcePath);
	    }
    }

    public enum OpenType
    {
	    AtOnce,//插入到队首
	    FrontOfHead,//插入逻辑序列的头部，在特殊类型之后
	    BehindHead,//插入逻辑队列的头部元素的后面，在特殊类型之后
	    Normal,//追加到序列的尾部
	    
	    InFrontOfCurrent,//插入到当前弹窗位置之前,需要提供当前弹窗的eid，当前位置弹窗不存在，则插入失败
	    BehindCurrent,//插入到当前弹窗位置后面，需要提供当前弹窗的eid，当前位置弹窗不存在，则插入失败
	    NormalGroup//目前只有卡牌系统在使用，将一组奖励插入到序列中
    }

    public enum MutexType
    {
	    None = 0,
	    Name = 1,
	    Custom =3
    }

    /// <summary>
    /// 所有使用Loading的需要指定自身关闭的必须都在LoadingLock里进行定义，谁打开，谁关闭 中间属于原子操作，不可被第三方中断
    /// </summary>
    public enum LoadingLock
    {
	    WithOutLock = -1,
	    None = 0,
	    DailyQuest = 1,
	    DailyQuestPoint = 2,
	    ShareLink = 3,
	    GiftBox = 4,
	    DiceMap = 5,
	    PokerKing = 6,
	    PickGame = 7,
	    Purchase = 8,
	    LevelUp = 9,
	    NewGuide = 10,
	    Shop = 11,
	    ClubSystem = 12,
	    HourlyBonusWheel = 13,
	    Lotto = 14,
	    
	    WildSpin = 15,
	    HourlyWheel= 16,
	    Rookie = 17,
	    Synthesis = 18,
		MQStagePassOffer = 19
    }
    #endregion
}