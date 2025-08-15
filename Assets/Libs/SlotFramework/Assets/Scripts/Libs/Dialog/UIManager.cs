using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Libs
{
	public partial class UIManager
	{
		public static string UIPrefabLocalPath = "Prefab/UI/";
		public static  string  UITipsPrefabLocalPath = "Prefab/UI/";
		public static string UIPrefabCameraPath = "Prefab/Shared/DialogCamera";
        
        private static UIManager _instance;

		public static UIManager Instance { 
			get {
				if (_instance == null)
					_instance = new UIManager ();
				return _instance;
			}
		}
		
        private GameObject root;
        
        private GameObject topRoot;
        
        public GameObject Root { get { return this.root; } }

        private GameObject tips;
        
        public void SetTipParentNode(GameObject go)
        {
	        tips = go;
        }
		public Camera UICamera {
			get {
				return root.transform.parent.parent.GetComponent<Camera> ();
			}
		}

		private List<string> waitDownloadCSVLowerName = new List<string>();

		private UIManager()
		{
			root = GameObject.Find("DialogCamera/DialogCanvas/Panel/");
			if (root == null)
			{
				GameObject prefab = Resources.Load(UIPrefabCameraPath, typeof(GameObject)) as GameObject;

				GameObject cameraPrefab = GameObject.Instantiate(prefab);
				cameraPrefab.name = "DialogCamera";
				root = GameObject.Find("DialogCamera/DialogCanvas/Panel/");
			}

			topRoot = GameObject.Find("Main Camera/BannerCanvas/BannerPrefab/SlotsDialogPanel/Panel");
			if (topRoot == null)
				topRoot = GameObject.Find("Main Camera/BannerCanvas/BannerPrefab_Portrait/SlotsDialogPanel/Panel");

			#region POC

			CreateLoadingGo();
			AddExceptionHandlerMonitor();
			Messenger.AddListener(GameConstants.CHECK_UP_UI_EXCEPTION_KEY, CheckUpUIException);
			Messenger.AddListener<string>(GameConstants.ON_UIEVENT_SEQUENCE_EMPTY_KEY, onUIEventSequenceEmpty);
			#endregion

			_instance = this;
			
		}
		#region mask

		~UIManager()
		{
			Messenger.RemoveListener(GameConstants.CHECK_UP_UI_EXCEPTION_KEY,CheckUpUIException);
			Messenger.RemoveListener<string>(GameConstants.ON_UIEVENT_SEQUENCE_EMPTY_KEY, onUIEventSequenceEmpty);
		}
		//遮罩层
		//1.初始创建并隐藏 2.添加ui的时候显示 3.destory的时候消除
		private float maskAlpha = 0.5f;
		private List<UIDialog> activeList = new List<UIDialog> ();
		private List<UIDialog> activeTipsList = new List<UIDialog>();
		
		#endregion mask
		public bool CheckActiveDialog()
        {
            return activeList.Count > 0;
        }

		public bool CheckExistDialog()
        {
	        return  CheckActiveDialog() || dialogCount>0;
        }

        public string FindAllNataiveName()
        { 
	        string str = "";
	        str += "activeList: " + string.Join(",", activeList.ConvertAll(new Converter<UIDialog, string>(delegate(UIDialog input) { return input.name; })).ToArray())  +"   ";
	        str += "activeTipsList: " + string.Join(",", activeTipsList.ConvertAll(new Converter<UIDialog, string>(delegate(UIDialog input) { return input.name; })).ToArray())  +"   ";
	        return str;
        }

        #region adjust dialog orientation
        private  void AddDialogOrientationAdapter(GameObject panel)
        {
            if (panel.GetComponent<DialogOrientationAdapter>() == null) 
	            panel.AddComponent<DialogOrientationAdapter>();
        }
        #endregion
        
		//activeList有值且是不是都是不打断的，如果是的话不缩放time，不是的话则时间为0.长度为0则不缩放
		private void SeBackgroundTimeScale()
		{
            if (BaseGameConsole.singletonInstance.IsInSlotMachine()) { 
                bool isNeedScaleTime = false;
    			for (int i = 0; i < activeList.Count; i++) {
    				if (activeList [i].IsBackGroundStop) {
    					isNeedScaleTime = true;
    					break;
    				}
    			}

    			if (isNeedScaleTime) {
	                Debug.Log("Time.TimeScale = 0f");
    				//if (Time.timeScale != 0f) {
    					Time.timeScale = 0f;
    				//}
    			} else {
    				//if (Time.timeScale != 1f) {
    					Time.timeScale = 1f;
    				//}
    			}
            }
        }

		public void Close (UIDialog ui)
		{
			Debug.Log("UIManager Close Dialog: " + ui.name);
			if (ui == null) {
				return;
			}
			bool inQueue = false;//是因为对话框在打开前异常销毁，没有放入到指定队列内

			if (ui.EndCallback != null) ui.EndCallback();
			while (activeList.Contains (ui)) {
				inQueue = true;
				activeList.Remove (ui);
				ShowFrontUIParticles();
				GameObject.Destroy (GetDlgRootNodeGo<UIDialog>(ui.gameObject));
			}

			while (activeTipsList.Contains (ui))
			{
				inQueue = true;
				activeTipsList.Remove (ui);
				GameObject.Destroy (GetTipsRootNodeGo<UIDialog>(ui.gameObject));
			}
			if(!inQueue) GameObject.DestroyImmediate(ui.gameObject);//不适用Destroy方法，是因为调用此方法不能帧内销毁，后面判断不生效
			
			SeBackgroundTimeScale ();
        }

		private GameObject GetDlgRootNodeGo<T>(GameObject ui)
		{
			//临时处理，等都改完后,再删除
			
			if (ui.transform.parent == root.transform)
			{
				return ui;
			}

			return ui.transform.parent.gameObject;
		}
		private GameObject GetTipsRootNodeGo<T>(GameObject ui)
		{
			//临时处理，等都改完后,再删除
			
			if (ui.transform.parent == tips.transform)
			{
				return ui;
			}

			return ui.transform.parent.gameObject;
		}

		public void Close<T>() where T:UIDialog
		{
			for (int i = activeList.Count - 1; i >= 0; i--) {
				if (activeList [i].GetType ().Equals (typeof(T))) {
					Close(activeList[i]);
				}
			}
			for (int i = activeTipsList.Count - 1; i >= 0; i--) {
				if (activeTipsList [i].GetType ().Equals (typeof(T))) {
					Close(activeTipsList[i]);
				}
			}
		}

		public void CloseAll (bool forceClear = false)
		{
			Debug.Log("UIManager CloseAll Dialogs");
			#region POC
			/*
			 * 修改原因是reload过程中，dialog调用close方法会把下一个面板弹出来，导致只会在机器内弹出的弹版会在大厅弹出
			 */
			Messenger.Broadcast(Constants.CLEAR_ALL_UI_EVENTS_KEY,forceClear);
			#endregion
			int cnt = activeList.Count - 1;
			for (int i = cnt; i >= 0; i--) {
				if(activeList[i]!=null&&activeList[i].forceSkipCloseAll) continue;
				Close (activeList [i]);
			}
			int tipsCnt = activeTipsList.Count - 1;
			for (int i = tipsCnt; i >= 0; i--) {
				Close (activeTipsList [i]);
			}
		}

		public T GetFromActiveList<T> () where T : UIBase
		{
			for (int i=0; i<activeList.Count; i++) {
				if (activeList [i].GetType ().Equals (typeof(T)))
					return activeList [i] as T;
			}
			return null;
		}
		private int dialogCount = 0;
		public bool IsEmpty()
		{
			return dialogCount==0;
		}

		public UIDialog GetActiveDialog ()
		{
			for (int index = activeList.Count-1; index >= 0; index --) {
				if (activeList [index])
					return activeList [index];
			}
			return null;
		}

		public UIDialog GetActiveDialog<T>() where T : UIDialog
		{
			return activeList.Find(dlg => dlg is T); ;
		}
		public UIDialog GetActiveTipDialog<T>() where T : UIDialog
		{
			return activeTipsList.Find(dlg => dlg is T); ;
		}
		public UIDialog GetNowActiveDialog()
		{
			if (activeList == null || activeList.Count == 0) return null;
			return activeList[0];
		}

		/// <summary>
		/// 是最新的挂载在DialogCanvas 下的dialog
		/// </summary>
		/// <param name="dialog"></param>
		/// <returns></returns>
		public bool IsLastDialogWithNotTopBanner(UIDialog dialog)
		{
			if (dialog == null) return false;
			if (dialog.DialogRootPath == UIDialog.EnumDialogRootPath.TopBanner) return false;
			for (int index = activeList.Count-1; index >= 0; index --) {
				if (activeList [index] && activeList [index].DialogRootPath == UIDialog.EnumDialogRootPath.None)
					return activeList [index] == dialog;
			}
			return false;
		}

		public  Dictionary<string,GameObject> DefaultDialogPrefabDictionary = new Dictionary<string,GameObject> ();


		public void LoadInitDialog(List<string> dialogNameList)
		{
			if (GlobalObjectReference.Instance == null) {
				return;
			}

			for (int i = 0; i < dialogNameList.Count; i++) {
				GlobalObjectReference.AssetReference aR = GlobalObjectReference.Instance.dialogList.Find (x => {
					if (x.assetID == dialogNameList[i])
						return true;
					else
						return false;
				});
				if (aR!=null&&aR.assetRef!=null) {
					DefaultDialogPrefabDictionary [dialogNameList[i]] = aR.assetRef;
				}
			}
		}
		
	}
	
}
