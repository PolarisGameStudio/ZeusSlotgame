using System.Collections.Generic;
using System.Linq;
using Classic;
using Libs;
using UnityEngine;
using Utils;

namespace System
{
    public class WithDrawManager
    {
        private const string ConfigKey = "WithDrawPanelConfig";
        private const string ItemKey = "ItemConfig";

        private const string PlatformKey = "Platform";
        private const string TaskFinishTime = "TaskFinishTime";
        private const string LoginDays = "LoginDays";
        
        //用于区分当前点击的是哪一个任务绑定的UI
        public int CurSelectTaskId = 0;
        public bool haveClickShowAccount = false;
        public bool IsInWithDrawProgress = false;
        //提现的公共冷却时间(单位秒),配置为负数的话代表“需要累计登录的天数”，如-7，代表要累计登录7天
        private int coolTime = 0;
        public static bool WithDrawUIShow = false;
        public bool NeedLoginDays = false;
        public static WithDrawManager Instance{
            get{ 
                return Singleton<WithDrawManager>.Instance;
            }
        }
        //提现任务数据
        private Dictionary<string, List<RedeemItemData>> redeemItemDict = new Dictionary<string, List<RedeemItemData>>();
        //提现记录数据
        private List<RecordItemData> recordItemDict = new List<RecordItemData>();

        // private List<RedeemItemData> redeemItemList = new List<RedeemItemData>();
        private bool isConfigReady = false;
        private int PlatFormIndex = -1;

        private WithDrawSystemProgressData progressData = new WithDrawSystemProgressData();

        private WithDrawManager() { }
        
        //初始化
        public void OnInit()
        {
            ParseConfig();
            LoadProgressData();
            if (!isConfigReady)
            {
                Debug.LogError("WithDrawManager OnInit isConfigReady is false");
                return;
            }
        }
        
        #region LoadAndSaveData
        private void LoadProgressData()
        {
            try
            {
                WithDrawSystemProgressData data = StoreManager.Instance.LoadDataJson<WithDrawSystemProgressData>(progressData.fileName);
                if (data!=null)
                {
                    progressData.LoadData(data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void SaveProgressData()
        {
            progressData.SaveData();
        }
        #endregion

        private void SaveTaskFinishTime(int taskId)
        {
            // 只保存日期部分（去掉时分秒），这样比较的是“哪一天”
            string today = DateTime.Now.Date.ToString("yyyy-MM-dd");
            string taskTime = TaskFinishTime + taskId;
            PlayerPrefs.SetString(taskTime, today);
            string loginDays = LoginDays + taskId;
            PlayerPrefs.SetInt(loginDays, 1);
            PlayerPrefs.Save();
        }

        public int GetLoginDays(int taskId)
        {
            string loginDays = LoginDays + taskId;
            int days = PlayerPrefs.GetInt(loginDays,0);
            return days;
        }
        
        private bool IsNewDayLogin(string key)
        {
            string taskFinishTime = PlayerPrefs.GetString(key);
            if (DateTime.TryParse(taskFinishTime, out DateTime lastLoginDate))
            {
                DateTime today = DateTime.Now.Date;
                if (lastLoginDate < today)
                {
                    return true;
                }
            }
            return false;
        }

        public int UpDateLoginDays(int taskId)
        {
            string taskTime = TaskFinishTime + taskId;
            if (!PlayerPrefs.HasKey(taskTime)) return 0;
            string loginDays = LoginDays + taskId;
            int days = PlayerPrefs.GetInt(loginDays);
            if (IsNewDayLogin(taskTime))
            {
                days++;
                PlayerPrefs.SetInt(loginDays, days);
                string today = DateTime.Now.Date.ToString("yyyy-MM-dd");
                PlayerPrefs.SetString(taskTime, today);
                PlayerPrefs.Save();
            }
            return days;
        }

        public void ParseConfig()
        {
            Dictionary<string,object> config = Plugins.Configuration.GetInstance().GetValue<Dictionary<string,object>>(ConfigKey,null);
            if (config==null || config.Count == 0)
            {
                Debug.LogError("WithDrawDialog ParseData config is null");
                return;
            }
            coolTime  = Utilities.GetInt(config,WithDrawConstants.CoolTimeKey,0);
            if (coolTime < 0)
            {
                NeedLoginDays = true;
                coolTime = - coolTime;
            }
            Dictionary<string,object> itemConfigs = Utilities.GetValue<Dictionary<string,object>>(config,ItemKey,null);
            if (itemConfigs==null || itemConfigs.Count == 0)
            {
                Debug.LogError("WithDrawDialog ParseData itemConfig is null");
                return;
            }
            
            List<int> platFormSpriteIndex = LocalizationManager.Instance.GetPlatFormSpriteIndex();
            if (platFormSpriteIndex == null || platFormSpriteIndex.Count == 0)
            {
                Debug.LogError("WithDrawDialog ParseData platFormSpriteIndex is null");
                return;
            }
            //根据平台数截取配置数组
            int count = Math.Min(itemConfigs.Count, platFormSpriteIndex.Count);
            for (int i = 0; i < count; i++)
            {
                string newKey = PlatformKey + i;
                List<object> items = Utilities.GetValue<List<object>>(itemConfigs,newKey,null);
                if (items==null || items.Count == 0)
                {
                    Debug.LogError("WithDrawDialog ParseData  is null");
                    return;
                }

                List<RedeemItemData> redeemItemList = new List<RedeemItemData>();
                for (int j = 0; j < items.Count; j++)
                {
                    Dictionary<string, object> data = items[j] as Dictionary<string, object>;
                    RedeemItemData itemData = new RedeemItemData(data);
                    itemData.SetPlatSprite(platFormSpriteIndex[i]);
                    //任务已完成,创建记录
                    if (itemData.IsFished())
                    {
                        RecordItemData recordItemData = itemData.ToRecordItemData();
                        recordItemDict.Add(recordItemData);
                    }
                    else
                    {
                        redeemItemList.Add(itemData);
                    }
                }
                redeemItemList.Sort((item1,item2)=>
                {
                    return (item1.index < item2.index) ? 1:0;
                });
                redeemItemDict[newKey] = redeemItemList;
            }
            isConfigReady = true;
        }

        public int GetRedeemItemCount(int index)
        {
            if (index<0 || index>=redeemItemDict.Count)
            {
                return 0;
            }
            List<RedeemItemData> redeemItemList = redeemItemDict[PlatformKey + index];
            return redeemItemList.Count;
        }

        public int GetCoolTime()
        {
            return coolTime;
        }
        
        public RedeemItemData GetRedeemItemData(int index)
        {
            List<RedeemItemData> redeemItemList = redeemItemDict[PlatformKey + PlatFormIndex];
            return redeemItemList[index];
        }

        public int GetRecordItemCount()
        {
            if (recordItemDict == null || recordItemDict.Count == 0)
            {
                return 0;
            }
            return recordItemDict.Count;
        }
        
        public RecordItemData GetRecordItemIndex(int index)
        {
            if (index < 0 || index >= recordItemDict.Count)
            {
                return null;
            }
            return recordItemDict[index];
        }
        
        public void RemoveRedeemItem(RedeemItemData data)
        {
            if (data == null)
            {
                return;
            }
            List<RedeemItemData> redeemItemList = redeemItemDict[PlatformKey + PlatFormIndex];
            if (redeemItemList.Contains(data))
            {
                redeemItemList.Remove(data);
            }
        }
        public void AddRecordItemData(RecordItemData itemData)
        {
            if (itemData == null)
            {
                return;
            }
            recordItemDict.Add(itemData);
        }

        public bool HasData(int taskId)
        {
            foreach (var data in recordItemDict)
            {
                if (data.taskId == taskId) return true;
            }
            return false;
        }
        
        //当前选中的 toggle 序号
        public void SetPlatFormIndex(int index)
        {
            PlatFormIndex = index;
        }

        public int GetPlatSpriteIndex()
        {
            List<int> index = LocalizationManager.Instance.GetPlatFormSpriteIndex();
            return index[PlatFormIndex];
        }
        
        public void ResetShowAccountTag()
        {
            haveClickShowAccount = false;
        }

        //当前操作的任务Id
        public int GetSelectId()
        {
            return CurSelectTaskId;
        }
        
        public void ResetSelectId()
        {
            CurSelectTaskId = 0;
        }
        
        /// <summary>
        /// 防止多个同时cell点击调用此方法，添加阻截
        /// </summary>
        /// <param name="taskId"></param>
        public void ShowAccountDialog(int taskId,int cash)
        {
            if (haveClickShowAccount)
            {
                return;
            }
            haveClickShowAccount = true;
            CurSelectTaskId = taskId;
            Debug.Log($"[WithDrawManager][ShowAccountDialog] PlatFormIndex:{GetPlatSpriteIndex()}");
            Messenger.Broadcast<int,int>(GameDialogManager.OpenAccountDialogMsg,GetPlatSpriteIndex(),cash);
            // CloseWithDrawDialog();
        }
        public void ShowAccountEnsureDialog(string email,int cash)
        {
            Debug.Log($"[WithDrawManager][ShowAccountDialog] PlatFormIndex:{GetPlatSpriteIndex()}");
            Messenger.Broadcast<int,string,int>(GameDialogManager.OpenAccountEnsureMsg,GetPlatSpriteIndex(),email,cash);
            // ReduceCash(cash);
        }

        public void ReduceCash(int money)
        {
            Debug.Log($"[WithDrawManager][ReduceCash] money:{money}");
            int newCash = OnLineEarningMgr.Instance.Cash() - money;
            OnLineEarningMgr.Instance.SetCash(newCash);
            //广播刷新任务减钱
            Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
            // ShowWithDrawDialog();
            if (NeedLoginDays)
            {
                SaveTaskFinishTime(CurSelectTaskId);
            }
        }
        public void ShowWithDrawDialog()
        {
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_SUSPEND);
            //打开提现弹窗
            Messenger.Broadcast(GameDialogManager.OpenWithDrawDialog);
        }
        
        //关闭提现弹窗
        public void CloseWithDrawDialog()
        {
            Messenger.Broadcast(GameDialogManager.CloseWithDrawDialog);
        }
        
        /// <summary>
        /// 用于弹窗任务进度显示新增的接口
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns></returns>
        public BaseTask GetTaskByType(int taskType)
        {
            BaseTask task = TaskManager.Instance.GetTaskByType(taskType);
            if (task == null)
            {
                Debug.LogError($"[WithDrawManager][GetTaskByType] task is null, taskType:{taskType}");
                return null;
            }
            return task;
        }
        
        public void ShowTip(string msg)
        {
            Debug.Log($"[WithDrawManager][ShowTip] msg:{msg}");
            Messenger.Broadcast(GameDialogManager.OpenTaskTipsDialogMsg);
        }
    }
}