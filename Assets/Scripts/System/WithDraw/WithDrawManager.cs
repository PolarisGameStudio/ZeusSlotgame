using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Classic;
using DG.Tweening;
using Libs;
using UnityEngine;
using UnityEngine.Localization;
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

        private const string CurrentSpinTimes = "BASE_RESULT_CHANGE_SPIN_TIMES";
        
        //用于区分当前点击的是哪一个任务绑定的UI
        public int CurSelectTaskId = 0;
        public bool haveClickShowAccount = false;
        public bool IsInWithDrawProgress = false;
        //提现的公共冷却时间(单位秒),配置为负数的话代表“需要累计登录的天数”，如-7，代表要累计登录7天
        private int coolTime = 0;
        
        private int _adCoolTime = 0;//关闭提现界面是否有广告的冷却时间
        private int _activeAdSpinCount;//激活广告所需的spin次数
        private bool _isActiveCloseAd;//是否激活提现界面关闭广告
        public bool CanPlayAd;//是否激活提现界面关闭广告
        public bool isFirstWithDraw = false;
        public bool hasShownWithDrawPrompt = false;
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
            CheckActive();

            if (_isActiveCloseAd)
            {
                StartCountdown();
            }
            else
            {
                Messenger.AddListener(GameConstants.DO_SPIN,UpdateSpinCount);
            }
        }
        private void UpdateSpinCount()
        {
            CheckActive();
            if (_isActiveCloseAd)
            {
                Messenger.RemoveListener(GameConstants.DO_SPIN,UpdateSpinCount);
                StartCountdown();
            }
        }
        private Tweener _countDownTweener;
        public void StartCountdown()
        {
            _countDownTweener?.Kill();

            float startTime = _adCoolTime;
            
            _countDownTweener = DOVirtual.Float(startTime, 0, _adCoolTime,PrintLog)
                .SetEase(Ease.Linear)
                .OnComplete(OnCountdownFinished);
        }
        private void PrintLog(float value)
        {
            //Debug.Log("广告冷却时间为"+value);
        }
        
        void OnCountdownFinished()
        {
            CanPlayAd = true;
        }

        private void CheckActive()
        {
            var allSpinCount =  SharedPlayerPrefs.GetPlayerPrefsIntValue(CurrentSpinTimes,0);
            _isActiveCloseAd =  allSpinCount >= _activeAdSpinCount;
            if (!_isActiveCloseAd)
            {
                Debug.Log("激活广告剩余spin次数为为"+(allSpinCount - _activeAdSpinCount));
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
                    isFirstWithDraw = data.isFirstWithDraw;
                    hasShownWithDrawPrompt = data.hasShownWithDrawPrompt;
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
            
            _adCoolTime  = Utilities.GetInt(config,WithDrawConstants.AdCoolTimeKey,0);
            
            _activeAdSpinCount = Utilities.GetInt(config,WithDrawConstants.ActiveAdSpinCount,0);
            
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
        public void ShowAccountDialog(RedeemItemData data)
        {
            // Debug.Log($"[WithDrawManager][ShowAccountDialog] PlatFormIndex:{GetPlatSpriteIndex()}");
            Messenger.Broadcast<RedeemItemData>(GameDialogManager.OpenAccountDialogMsg,data);
            // CloseWithDrawDialog();
        }
        public void ShowAccountEnsureDialog(string email,RedeemItemData data)
        {
            // Debug.Log($"[WithDrawManager][ShowAccountDialog] PlatFormIndex:{GetPlatSpriteIndex()}");
            Messenger.Broadcast<RedeemItemData,string>(GameDialogManager.OpenAccountEnsureMsg,data,email);
            // ReduceCash(cash);
        }

        public void ReduceCash(int money)
        {
            // Debug.Log($"[WithDrawManager][ReduceCash] money:{money}");
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
        
        public void CheckShowWithDrawTipDialog(int cash)
        {
            if (!isFirstWithDraw)
            {
                isFirstWithDraw = true;
                Messenger.Broadcast<int>(GameDialogManager.OpenWithDrawTipDialogMsg,cash);
            }
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

        //发送事件
        public void SendMsg(int money)
        {
            //上报redeem埋点
            int count = PlayerPrefs.GetInt("Redeem", 0);
            count++;
            Dictionary<string,int>  data = new Dictionary<string, int>
            {
                { "count", count },
                { "platform", PlatFormIndex},
                { "cash", money }
            };
            string datastr = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint, "Redeem", datastr);
            PlayerPrefs.SetInt("Redeem", count);
        }
        
        /// <summary>
        /// ismin:是否取当前平台的最小档位 为false则取当前平台下一个档位
        /// </summary>
        /// <param name="isMin"></param>
        /// <returns></returns>
        public int GetTaskLevelCash(bool isMin = false)
        {
            Dictionary<string, List<RedeemItemData>> tempRedeemItemDict = new Dictionary<string, List<RedeemItemData>>();
            //对redeemItemDict进行过滤，去除掉List<RedeemItemData>中，RedeemItemData.state!=RedeemItemState.InTaskProgress1 的元素
            foreach (var kvp in redeemItemDict)
            {
                List<RedeemItemData> tempList = new List<RedeemItemData>();
                foreach (var item in kvp.Value)
                {
                    if (item.state == RedeemItemState.InTaskProgress1)
                    {
                        tempList.Add(item);
                    }
                }
                tempRedeemItemDict[kvp.Key] = tempList;
            }
            
            //先选出最优先的档位数组
            int minLength = 100;
            List<RedeemItemData> targetRedeemItemList = null;
            foreach (var kvp in tempRedeemItemDict)
            {
                if (kvp.Value.Count < minLength)
                {
                    minLength = kvp.Value.Count;
                    targetRedeemItemList = kvp.Value;
                }
            }
            //当前没有可以领取的任务
            if (minLength == 0)
            {
                return 0;
            }
            int targetCash = 0;
            if (isMin)
            {
                ///取出当前平台的第一个任务
                targetCash = (int)targetRedeemItemList[0].RewardCash;
            }
            else
            {
                int cash = OnLineEarningMgr.Instance.Cash();
                for (int i = 0; i < targetRedeemItemList.Count-1; i++)
                {
                    if (cash >= targetRedeemItemList[i].RewardCash && targetRedeemItemList[i+1].RewardCash>cash)
                    {
                        targetCash = (int)targetRedeemItemList[i+1].RewardCash;
                        break;
                    }
                }
            }
            
            return targetCash;
        }

    }
}