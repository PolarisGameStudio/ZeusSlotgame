using System;
using System.Collections.Generic;
//using RealYou.Core.UI;
using Classic;
using UnityEngine;

namespace Libs
{
    /// <summary>
    /// todo 后续添加轮询检查统计机制
    /// </summary>
    public class UIEventMgr
    {
        private UIEventMgr()
        {
            RegisterEvent();
        }

        ~UIEventMgr()
        {
            UnregisterEvent();
        }

        private Dictionary<string,List<UIEvent>> eventSequence  = new Dictionary<string, List<UIEvent>>();
        private void RegisterEvent()
        {
            Messenger.AddListener<Dictionary<string,object>>(Constants.ADD_UI_EVENT_KEY,AddUIEvent);
            Messenger.AddListener<Dictionary<string,object>>(Constants.ADD_UI_EVENT_IN_SAME_GROUP_KEY,AddUIEventInSameGroup);

            Messenger.AddListener<Dictionary<string,object>>(Constants.INSERT_AT_HEAD_UI_EVENT_KEY,InsertAtHead);
            Messenger.AddListener<Dictionary<string,object>>(Constants.INSERT_IN_FRONT_OF_HEAD_UI_EVENT_KEY,InsertInFrontOfHead);
            Messenger.AddListener<Dictionary<string,object>>(Constants.INSERT_BEHIND_HEAD_UI_EVENT_KEY,InsertBehindHead);
            
            Messenger.AddListener<Dictionary<string,object>>(Constants.INSERT_IN_FRONT_OF_CURRENT_UI_EVENT_KEY,InsertInFrontOfCurrent);
            Messenger.AddListener<Dictionary<string,object>>(Constants.INSERT_BEHIND_CURRENT_UI_EVENT_KEY,InsertBehindCurrent);
            Messenger.AddListener<bool>(Constants.CLEAR_ALL_UI_EVENTS_KEY,ClearAll);
            //Messenger.AddListener<string,int>(Constants.REMOVE_UI_EVENT_KEY,RemoveUIEvent);
            Messenger.AddListener<string,Action<int>>(GameConstants.GET_UI_EVENT_NUM_KEY,GetUIEventNum);
        }

        private void UnregisterEvent()
        {
            Messenger.RemoveListener<Dictionary<string,object>>(Constants.ADD_UI_EVENT_KEY,AddUIEvent);
            Messenger.RemoveListener<Dictionary<string,object>>(Constants.ADD_UI_EVENT_IN_SAME_GROUP_KEY,AddUIEventInSameGroup);

            Messenger.RemoveListener<Dictionary<string,object>>(Constants.INSERT_AT_HEAD_UI_EVENT_KEY,InsertAtHead);
            Messenger.RemoveListener<Dictionary<string,object>>(Constants.INSERT_IN_FRONT_OF_HEAD_UI_EVENT_KEY,InsertInFrontOfHead);
            Messenger.RemoveListener<Dictionary<string,object>>(Constants.INSERT_BEHIND_HEAD_UI_EVENT_KEY,InsertBehindHead);

            Messenger.RemoveListener<Dictionary<string,object>>(Constants.INSERT_IN_FRONT_OF_CURRENT_UI_EVENT_KEY,InsertInFrontOfCurrent);
            Messenger.RemoveListener<Dictionary<string,object>>(Constants.INSERT_BEHIND_CURRENT_UI_EVENT_KEY,InsertBehindCurrent);
            Messenger.RemoveListener<bool>(Constants.CLEAR_ALL_UI_EVENTS_KEY,ClearAll);
            //Messenger.RemoveListener<string,int>(Constants.REMOVE_UI_EVENT_KEY,RemoveUIEvent);
            Messenger.RemoveListener<string,Action<int>>(GameConstants.GET_UI_EVENT_NUM_KEY,GetUIEventNum);

        }

        public void Init()
        {
            ClearAll(true);
        }
       
        // 移除指定事件，并执行下一个事件
        public void RemoveEventAndDoNext(string queueID, int id)
        {
            if(!eventSequence.ContainsKey(queueID))
                return;
            RemoveUIEvent(queueID, id);
            DoNext(queueID);
        }

        // 判断某个队列是否为空
        public bool IsSequenceEmpty(string queueID)
        {
            if (!eventSequence.ContainsKey(queueID))
                return true;
            return eventSequence[queueID].Count <= 0;
        }
        
        #region UIEvent Handle

        private void CreateUISequnce(string queue_id)
        {
            if (!eventSequence.ContainsKey(queue_id))
            {
                eventSequence.Add(queue_id,new List<UIEvent>());
            }
        }
        private void RemoveUIEvent(string queueID, int id)
        {
            List<UIEvent> list = eventSequence[queueID];
            if (list == null || list.Count == 0) return;
            UIEvent tgtEvent = list.Find(e => e.Id == id);
            if (tgtEvent == null) return;
            //Print(tgtEvent.uiName,"RemoveUIEvent id Before");
            list.Remove(tgtEvent);
            CheckPauseState(list);
           // Print(tgtEvent.uiName,"RemoveUIEvent id After");

            //if (list == null || list.Count == 0||list[0]==null) return;
           // if(!list[0].isRunning) list[0].Excute();
        }

        private void DoNext(string queueID)
        {
            List<UIEvent> list = eventSequence[queueID];
            if (list == null)
            {
                return;
            }
            if (list.Count == 0)
            {
                Messenger.Broadcast(GameConstants.ON_UIEVENT_SEQUENCE_EMPTY_KEY, queueID);
                return;
            }

            if (list[0] == null)
            {
                list.RemoveAt(0);
                DoNext(queueID);
                return;
            }
            if(!list[0].isRunning) list[0].Excute();
        }

        private void RemoveUIEvent(string queueID,string uiName)
        {
            List<UIEvent> list = eventSequence[queueID];
            if (list == null || list.Count == 0) return;
            List<UIEvent> tgtEvents = list.FindAll(e => e.uiName == uiName);
            if (tgtEvents == null||tgtEvents.Count==0) return;
            //Print(uiName,"RemoveUIEvent Before");
            for (int i = 0; i < tgtEvents.Count; i++)
            {
                tgtEvents[i].Cancel();
            }
            
            CheckPauseState(list);
            //Print(uiName,"RemoveUIEvent After 1");

            if (list == null || list.Count == 0||list[0]==null) return;
            
            if(!list[0].isRunning) list[0].Excute();
            
            //Print(uiName,"RemoveUIEvent After 2");

            
        }
        private void AddUIEvent(Dictionary<string,object> dict)
        {
            (string queueID,int id,string uiName, string type, int currentId, Action<Action<int>> openCB, bool pauseMachine,Action<bool> runEnd,bool skipClearAll,bool forceSkipClearAll)=ParseUIEventParams(dict);
            CreateUISequnce(queueID);
            UIEvent uiEvent = new UIEvent(queueID,id,type,uiName,openCB,RemoveUIEvent,pauseMachine,runEnd,skipClearAll,DoNext,forceSkipClearAll);
            List<UIEvent> list = eventSequence[queueID];
            list.Add(uiEvent);
            
            //Print(uiName,"AddUIEvent");
            
            if (list.Count == 1&&!uiEvent.isRunning) uiEvent.Excute();
            CheckPauseState(list);
        }

        private void Print(string eventName,string trigger,string queueId)
        {
            List<UIEvent> list = eventSequence[queueId];
            string debug = "";
            foreach (var ui in list)
            {
                debug += ui.uiName+", ";
            }
            //PopUpUILogES.SendError2ES("Trggier:"+ trigger +" "+eventName+" List:"+list.Count+" Names:"+debug);
        }
        private void AddUIEventInSameGroup(Dictionary<string,object> dict)
        {
            (string queueID,int id,string uiName, string type, int currentId, Action<Action<int>> openCB, bool pauseMachine,Action<bool> runEnd,bool skipClearAll,bool forceSkipClearAll)=ParseUIEventParams(dict);
            CreateUISequnce(queueID);
            UIEvent uiEvent = new UIEvent(queueID,id,type,uiName,openCB,RemoveUIEvent,pauseMachine,runEnd,skipClearAll,DoNext,forceSkipClearAll);
            //Print(uiName,"InsertBehindCurrent Before");
            List<UIEvent> list = eventSequence[queueID];
            bool find = false;
            bool needInsert = false;
            int insertIdx = list.Count!=0?list.Count-1:0;
            for (int i = 0; i < list.Count; i++)
            {
                if(!find&&list[i]==null||list[i].Type!=type) continue;
                if(!find) find = true;
                if(find&&list[i]==null||list[i].Type==type) continue;
                insertIdx = i;
                needInsert = true;
                break;
            }

            if (needInsert)
            {
                list.Insert(insertIdx, uiEvent);
            }
            else
            {
                list.Add(uiEvent);
            }
            if (list.Count == 1&&!uiEvent.isRunning) uiEvent.Excute();
            CheckPauseState(list);
            //Print(uiName,"InsertBehindCurrent After");
        }

        private void InsertAtHead(Dictionary<string,object> dict)
        {
            Debug.Log("ShowLoadingUI InsertAtHead");
            (string queueID,int id,string uiName, string type, int currentId, Action<Action<int>> openCB, bool pauseMachine,Action<bool> runEnd,bool skipClearAll,bool forceSkipClearAll)=ParseUIEventParams(dict);
            CreateUISequnce(queueID);
            UIEvent uiEvent = new UIEvent(queueID,id,type,uiName,openCB,RemoveUIEvent,pauseMachine,runEnd,skipClearAll,DoNext,forceSkipClearAll);
            List<UIEvent> list = eventSequence[queueID];

            //Print(uiName,"InsertAtHead Before");
            if (list.Count==0)
            {
                list.Insert(0,uiEvent);
                if(!uiEvent.isRunning) list[0].Excute();
               // Print(uiName,"InsertAtHead After 1");
                CheckPauseState(list);
                return;
            }

            bool enQueue = false;
            for (int i = 0; i < list.Count; i++)
            {
                if(list[i] ==null) continue;//都是alert是必须往后放的
                //1.插入时首个元素为默认类型，直接插入到头部即可
                //2.插入时首个元素不是默认类型，插入到当前类型的后面
                if(!string.IsNullOrEmpty(list[i].Type)&& GetTypePriority(type) <= GetTypePriority(list[i].Type) ) continue;
                bool running = list[i].isRunning;
                list.Insert(i,uiEvent);
                enQueue = true;
                //插队首元素为第一个元素或者原队首元素已经处于运行状态，此时插入的队首元素是允许运行的，alert则需要排队 Loading和CutScene是为了阻塞系统弹窗的事件
                if((i==0||running)&&!uiEvent.isRunning) list[i].Excute();
                break;
            }

            if (!enQueue)
            {
                list.Add(uiEvent);
            }
            CheckPauseState(list);
            //Print(uiName,"InsertAtHead After 2");

        }

        private void InsertInFrontOfHead(Dictionary<string,object> dict)
        {
            InsertAtHead(dict);
        }
        private void InsertBehindHead(Dictionary<string,object> dict)
        {
            (string queueID,int id,string uiName, string type, int currentId, Action<Action<int>> openCB, bool pauseMachine,Action<bool> runEnd,bool skipClearAll,bool forceSkipClearAll)=ParseUIEventParams(dict);
            CreateUISequnce(queueID);
            UIEvent uiEvent = new UIEvent(queueID,id,type,uiName,openCB,RemoveUIEvent,pauseMachine,runEnd, skipClearAll,DoNext,forceSkipClearAll);
            List<UIEvent> list = eventSequence[queueID];
            //Print(uiName,"InsertBehindHead Before");

            if (list.Count==0)
            {
                list.Insert(0,uiEvent);
                if(!uiEvent.isRunning) list[0].Excute();
                CheckPauseState(list);
               // Print(uiName,"InsertBehindHead After 1");
                return;
            }

            bool enQueue = false;
            for (int i = 0; i < list.Count; i++)
            {
                if(list[i] ==null) continue;//都是alert是必须往后放的
                //1.插入时首个元素为默认类型，直接插入到头部即可
                //2.插入时首个元素不是默认类型，插入到当前类型的后面
                if(!string.IsNullOrEmpty(list[i].Type)&& GetTypePriority(type) <= GetTypePriority(list[i].Type) ) continue;
                if (i+1<list.Count)
                {
                    list.Insert(i+1,uiEvent);
                }
                else
                {
                    list.Add(uiEvent);
                }

                enQueue = true;
                break;
            }

            if (!enQueue)
            {
                list.Add(uiEvent);
            }
            CheckPauseState(list);
           // Print(uiName,"InsertBehindHead After 2");
        }
        private int GetTypePriority(string type)
        {
            switch (type)
            {
                case Constants.BLOCK_UI:
                    return 1;
                case Constants.LOADING:
                    return 2;
                case Constants.CUT_SCENE:
                    return 4;
                case Constants.INTERSTITIAL_AD:
                    return 8;
                case Constants.ALERT:
                    return 16;
            }

            return 0;
        }
        private void InsertInFrontOfCurrent(Dictionary<string,object> dict)
        {
            (string queueID,int id,string uiName, string type, int currentId, Action<Action<int>> openCB, bool pauseMachine,Action<bool> runEnd,bool skipClearAll,bool forceSkipClearAll)=ParseUIEventParams(dict);
            //Print(uiName,"InsertInFrontOfCurrent Before");
            CreateUISequnce(queueID);
            UIEvent uiEvent = new UIEvent(queueID,id,type,uiName,openCB,RemoveUIEvent,pauseMachine,runEnd,skipClearAll,DoNext,forceSkipClearAll);
            List<UIEvent> list = eventSequence[queueID];
            if (list == null || list.Count == 0)
            {
                uiEvent?.Cancel();
                //Print(uiName,"InsertInFrontOfCurrent After 1");
                return;
            }
            int tgtIdx = list.FindIndex(e => e.Id == currentId);
            if (tgtIdx == -1)
            {
                uiEvent?.Cancel();
                //Print(uiName,"InsertInFrontOfCurrent After 2");
                return;
            }
            list.Insert(tgtIdx,uiEvent);
            if(tgtIdx==0&&!uiEvent.isRunning) uiEvent.Excute();
            CheckPauseState(list);
            //Print(uiName,"InsertInFrontOfCurrent After");
        }

        private void InsertBehindCurrent(Dictionary<string,object> dict)
        {
            (string queueID,int id,string uiName, string type, int currentId, Action<Action<int>> openCB, bool pauseMachine,Action<bool> runEnd,bool skipClearAll,bool forceSkipClearAll)=ParseUIEventParams(dict);
            CreateUISequnce(queueID);
            UIEvent uiEvent = new UIEvent(queueID,id,type,uiName,openCB,RemoveUIEvent,pauseMachine,runEnd,skipClearAll,DoNext,forceSkipClearAll);
            //Print(uiName,"InsertBehindCurrent Before");
            List<UIEvent> list = eventSequence[queueID];
            if (list == null || list.Count == 0)
            {
                uiEvent?.Cancel();
                //Print(uiName,"InsertBehindCurrent After 1");
                return;
            }
            
            int tgtIdx = list.FindIndex(e => e.Id == currentId);
            if (tgtIdx == -1)
            {
                uiEvent?.Cancel();
                //Print(uiName,"InsertBehindCurrent After 2");
                return;
            }
            tgtIdx = Math.Min(tgtIdx + 1, list.Count);
            list.Insert(tgtIdx,uiEvent);
            if(tgtIdx==0&&!uiEvent.isRunning) uiEvent.Excute();
            CheckPauseState(list);
            //Print(uiName,"InsertBehindCurrent After");
        }

        private (string queueID,int id,string uiName, string type, int currentId, Action<Action<int>> openCB, bool pauseMachine,Action<bool> runEndCB,bool skipClearAll,bool forceSkipClearAll) ParseUIEventParams(Dictionary<string, object> dict)
        {
            string queue_id = dict[Constants.E_EVENT_QUEUE_ID_KEY].ToString();
            int id = Utils.Utilities.CastValueInt(dict[Constants.E_ID_KEY]);
            string uiName = dict[Constants.E_UI_NAME_KEY].ToString();
            string type = dict[Constants.E_TYPE_KEY].ToString();
            int currentId = Utils.Utilities.CastValueInt(dict[Constants.E_CUR_E_ID_KEY]);
            Action<Action<int>> openCB = (Action<Action<int>>) dict[Constants.E_OPEN_CB_KEY];
            bool pauseMachine = Utils.Utilities.CastValueBool(dict[Constants.E_PAUSE_KEY]);
            Action<bool> runEndCB = null;
            if (dict.ContainsKey(Constants.RUN_END_KEY))
            {
                runEndCB = (Action<bool>)dict[Constants.RUN_END_KEY];
            }

            Action<Action<int>> initCB = null;
            if (dict.ContainsKey(Constants.E_INIT_CB_KEY))
            {
                initCB = (Action<Action<int>>)dict[Constants.E_INIT_CB_KEY];
                Action<int> endCB = (eid) =>
                {
                    RemoveUIEvent(queue_id,eid);
                    DoNext(queue_id);
                };
                if (initCB != null) initCB(endCB);
            }
            bool skipClearAll = Utils.Utilities.CastValueBool(dict[Constants.E_SKIP_CLEAR_ALL_KEY]);
            bool forceSkipClearAll = Utils.Utilities.CastValueBool(dict[Constants.E_FORCE_SKIP_CLEAR_ALL_KEY]);
            
            return (queue_id,id,uiName,type,currentId,openCB,pauseMachine,runEndCB,skipClearAll,forceSkipClearAll);
        }

        private void CheckPauseState(List<UIEvent> list)
        {
            int signals = 0;
            foreach (UIEvent uiEvent in list)
            {
                if (uiEvent.pauseMachine)
                {
                    signals++;
                }
            }

            if (signals > 0)
            {
                // 如果不是暂停，对其进行暂停
                //MachineUtility.Instance.Pause(true);
                return;
            }
            // 如果是暂停，对其进行解除暂停
            //MachineUtility.Instance.Pause(false);
        }
        private void ClearAll(bool forceClear)
        {
            foreach (var queueID in eventSequence.Keys)
            {
                if (!eventSequence.ContainsKey(queueID)) continue;
                List<UIEvent> list = eventSequence[queueID];
                if (list == null || list.Count == 0) continue;
                //Print("","ClearAll Before");

                List<UIEvent> removelist = new List<UIEvent>();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == null)
                    {
                        removelist.Add(list[i]);
                        continue;
                    }
                    //if(list[i].isRunning) continue;//ClearAll时，即使在运行的也要清除掉，比如PushPromptTextDialog Tips

                    if(!forceClear&&list[i].skipClearAll) continue;
                    if(list[i].forceSkipClearAll) continue;
                    removelist.Add(list[i]);
                }
                while (removelist.Count!=0)
                {
                    if (removelist[0] == null)
                    {
                        removelist.RemoveAt(0);
                        continue;
                    }
                    removelist[0].Cancel();//将已经排队将要打开的对话框关闭，并将元素从序列中剔除
                    removelist.Remove(removelist[0]);
                }
                CheckPauseState(list);
                //Print("","ClearAll After");
            }

        }

        private void GetUIEventNum(string queueID,Action<int> cb)
        {
            if (string.IsNullOrEmpty(queueID))
            {
                if(cb!=null)cb(0);
                return;
            }
            
            if (!eventSequence.ContainsKey(queueID))
            {
                if(cb!=null)cb(0);
                return;
            }
            List<UIEvent> list = eventSequence[queueID];
            if (list == null || list.Count == 0)
            {
                if(cb!=null)cb(0);
                return;
            }
            if(cb!=null)cb(list.Count);
        }
        #endregion
       
        
        public static UIEventMgr Instance { get { return Singleton<UIEventMgr>.Instance; } }
    }

    public class Constants
    {
        public const string ADD_UI_EVENT_KEY = "AddUIEvent";
        public const string INSERT_AT_HEAD_UI_EVENT_KEY = "InsertAtHead";
        public const string INSERT_IN_FRONT_OF_HEAD_UI_EVENT_KEY = "InsertInFrontOfHead";
        public const string INSERT_BEHIND_HEAD_UI_EVENT_KEY = "InsertBehindHead";

        
        public const string INSERT_IN_FRONT_OF_CURRENT_UI_EVENT_KEY = "InsertInFrontOfCurrent";
        public const string INSERT_BEHIND_CURRENT_UI_EVENT_KEY = "InsertBehindCurrent";
        public const string ADD_UI_EVENT_IN_SAME_GROUP_KEY = "AddUIEventInSameGroup";

        public const string REMOVE_UI_EVENT_KEY = "RemoveUIEvent";
        public const string CLEAR_ALL_UI_EVENTS_KEY = "ClearAllUIEvents";

        public const string BLOCK_UI = "BlockUI";//切换进入机器时，阻塞弹窗弹出
        public const string LOADING = "Loading";
        public const string CUT_SCENE = "CutScene";//切场景 从大厅到机器，从机器到大厅
        public const string INTERSTITIAL_AD = "INTERSTITIAL_AD";
        public const string ALERT = "Alert";

        public const string E_ID_KEY = "E_ID";
        public const string E_UI_NAME_KEY = "E_UI_NAME";
        public const string E_CUR_E_ID_KEY = "CUR_E_ID";
        public const string E_TYPE_KEY = "E_TYPE";
        public const string E_OPEN_CB_KEY = "E_OPEN_CB";
        public const string E_INIT_CB_KEY = "E_INIT_CB";
        public const string E_PAUSE_KEY = "E_PAUSE";

        public const string RUN_END_KEY = "RUN_END_CB";
        public const string E_SKIP_CLEAR_ALL_KEY = "E_SKIP_CLEAR_ALL";
        public const string E_FORCE_SKIP_CLEAR_ALL_KEY = "E_FORCE_SKIP_CLEAR_ALL";
        public const string E_EVENT_QUEUE_ID_KEY = "E_EVENT_QUEUE_ID";
        
        public const string UI_DIALOG_EVENT_KEY = "UIDialog";
        public const string UI_TIPS_EVENT_KEY = "UITips";
    }

    public class UIEvent
    {
        public string queueId;
        public int Id;
        public string uiName;
        public string Type;
        public bool isRunning;
        public bool isCancel;
        public bool skipClearAll;
        public bool forceSkipClearAll;
        public Action<Action<int>> excuteCB;
        public Action<string,int> removeCB;
        public Action<string> doNextCB;
        public bool pauseMachine;
        public Action<bool> runEndCB;
        

        public UIEvent(string queueID, int id,string type,string uiName,Action<Action<int>>doCB,Action<string,int> removeCB,bool pauseMachine,Action<bool> runEndCb,bool skipClearAll,Action<string> doNextCb,bool forceSkipClearAll)
        {
            this.queueId = queueID;
            this.Id = id;
            this.uiName = uiName;
            this.excuteCB = doCB;
            this.removeCB = removeCB;
            this.doNextCB = doNextCb;
            this.pauseMachine = pauseMachine;
            this.runEndCB = runEndCb;
            this.Type = type;
            this.skipClearAll = skipClearAll;
            this.forceSkipClearAll = forceSkipClearAll;
        }
        
        public void Excute()
        {
            try
            {
                Action<int> finishedCB = (id) =>
                {
                    if (removeCB != null) removeCB(queueId, id);
                    if (doNextCB != null) doNextCB(queueId);
                };
                isRunning = true;
                if (excuteCB != null) excuteCB(finishedCB);
            }
            catch (Exception e)
            {
                PopUpUILogES.SendError2ES("Excute "+uiName+" errorMsg:"+e.Message+" stack:"+e.StackTrace);
                if (removeCB != null) removeCB(queueId,Id);
                if (runEndCB != null) runEndCB(false);
                if (doNextCB != null) doNextCB(queueId);
            }
        }

        public void Cancel()
        {
            try
            {
                if (removeCB != null) removeCB(queueId,Id);
                if (runEndCB != null) runEndCB(false);
            }
            catch (Exception e)
            {
                PopUpUILogES.SendError2ES("Cancel "+uiName+" errorMsg:"+e.Message+" stack:"+e.StackTrace);
                if (removeCB != null) removeCB(queueId,Id);
                if (runEndCB != null) runEndCB(false);
            }
           
        }

        private void RemoveCB(int id)
        {
            if (this.Id != id) return;
            removeCB(queueId,id);
        }

    }
}