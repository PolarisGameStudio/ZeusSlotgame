using System.BuffSystem;
using System.Collections;
using System.Collections.Generic;
using Classic;
using Libs;
using UnityEngine;
using Utils;

namespace Activity
{
    public enum RotationState
    {
        Showing, 
        Hiding, 
        Active
    }

    public class RotationBuffActivity:BaseActivity
    {
        private int freeCount = 0;
        public List<int> buffList = new List<int>();
        public int displayDuration = 30;
        public int hideDuration = 10;
        private RotationState currentState;

        private float stateTimer = 0f;
        //轮转顺序
        public int currentIndex = 0;
        private BaseBuff currentBuff = null;
        //当前是否有buff激活
        private bool isBuffActive = false;
        private RotationBuffIcon _icon;
        public bool isInitialized = false;
        private RotationBuffProgressData _rotationBuffProgressData = new RotationBuffProgressData();
        public int activityCount = 0; //记录活动激活次数
        public int AutoPopupCount = 0; //记录自动弹出次数
        public int curPopupCount = 0;
        private bool isFree = true;
        private bool OpenCloseAd = true;
        //spin次数限制
        private int spinLimit = 0;
        //当前累计spin次数
        private int curSpin = 0;
        //活动是否已开启(基于spin次数)
        private bool isActivityEnabled = false;
        //解析配置
        public RotationBuffActivity(Dictionary<string, object> data) : base(data)
        {
            freeCount = Utilities.GetInt(data, "FreeCount", 0);
            hideDuration = Utilities.GetInt(data, "CoolDown", 0);
            displayDuration = Utilities.GetInt(data, "RotationInterval", 30);
            AutoPopupCount = Utilities.GetInt(data, "PopCount", 0);
            OpenCloseAd = Utilities.GetBool(data, "OpenCloseAd", true);
            spinLimit = Utilities.GetInt(data, "SpinLimit", 0);
            List<object> buffDataList = Utilities.GetValue<List<object>>(data, "buffList",null);
            if (buffDataList==null)
            {
                Debug.LogError($"[RotationBuffActivity] buffList is null");
                return;
            }

            for (int i = 0; i < buffDataList.Count; i++)
            {
                Dictionary<string,object> buffData = buffDataList[i] as Dictionary<string, object>;
                BaseBuff buff = BuffManager.Instance.CreateBuff(buffData);
                if (buff!=null)
                {
                    buffList.Add(buff.buffId);
                }
            }
        }

        public override void OnInit()
        {
            //不在这里调用 AddListener,而是在 InitializeRotation 中统一处理
            //这样可以避免重复添加监听(RegisterIcon 时会调用 InitializeRotation)
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Messenger.RemoveListener<bool>(RotationBuffConstant.RotationBuffDialogClose, SetRulePanelShowState);
            RemoveListener();
        }

        #region LoadAndSaveData

        private void LoadSavedData()
        {
            //从本地加载数据
            RotationBuffProgressData  Data = StoreManager.Instance.LoadDataJson<RotationBuffProgressData>(_rotationBuffProgressData.fileName);
            if (Data!=null)
            {
                _rotationBuffProgressData.LoadData(Data);
                activityCount = _rotationBuffProgressData.activeCount;
                curSpin = _rotationBuffProgressData.curSpin;
            }
            //检查活动是否已开启(基于spin次数)
            isActivityEnabled = curSpin >= spinLimit;
            isFree = activityCount < freeCount;
            //根据加载的数据恢复状态
            currentIndex = _rotationBuffProgressData.currentIndex;
            currentBuff = BuffManager.Instance.GetBuffById(buffList[currentIndex]);
            Debug.Log($"[RotationBuffActivity] LoadSavedData: index={currentIndex}, buff={currentBuff?.buffName}, curSpin={curSpin}, spinLimit={spinLimit}, isActivityEnabled={isActivityEnabled}");             
            //只有活动开启时才恢复buff状态
            if (isActivityEnabled)
            {
                //保存的buff未生效
                if (!currentBuff.CheckActive())
                {
                    //展示当前buff
                    StartShowingState();
                }
                else
                {
                    currentState = RotationState.Active;
                    isBuffActive = true;
                    //设置buff初始化状态
                    BuffManager.Instance.SetBuffInit(currentBuff.buffId);
                    UpdateIconDisplay(true);
                    OnBuffActive(currentBuff.buffId);
                }
            }
            else
            {
                //活动未开启,隐藏icon
                UpdateIconDisplay(false);
            }
        }

        public override void SaveData()
        {
            _rotationBuffProgressData.SaveData(currentIndex, currentBuff, activityCount, curSpin);
        }
        
        #endregion

        #region AddListener

        public override void AddListener()
        {
            base.AddListener();
            //避免重复添加监听
            Messenger.RemoveListener(SlotControllerConstants.OnSpinEnd, UpdateSpinCount);
            Messenger.AddListener(SlotControllerConstants.OnSpinEnd, UpdateSpinCount);
            if (currentBuff!=null)
            {
                Messenger.RemoveListener<bool>(currentBuff.UpdateBuffMsg, OnBuffChange);
                Messenger.AddListener<bool>(currentBuff.UpdateBuffMsg, OnBuffChange);
            }
        }
        
        public override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener(SlotControllerConstants.OnSpinEnd, UpdateSpinCount);
            if (currentBuff!=null)
            {
                Messenger.RemoveListener<bool>(currentBuff.UpdateBuffMsg, OnBuffChange);
            }
        }

        private void UpdateSpinCount()
        {
            curSpin++;
            //保存当前spin次数
            SaveData();
            //检查是否达到开启条件
            if (!isActivityEnabled && curSpin >= spinLimit)
            {
                isActivityEnabled = true;
                Debug.Log($"[RotationBuffActivity] Activity enabled: curSpin={curSpin} > spinLimit={spinLimit}");
                //活动开启,开始轮转
                if(currentBuff == null)
                {
                    currentBuff = BuffManager.Instance.GetBuffById(buffList[currentIndex]);
                }
                if (currentBuff != null)
                {
                    //设置显示状态
                    currentState = RotationState.Showing;
                    stateTimer = displayDuration;
                    //显示icon
                    UpdateIconDisplay(true);
                    //活动初次满足开启条件时,展示RotationBuffDialog
                    ShowRulePanel(true);
                }
            }
        }
        
        private void OnBuffChange(bool isActive)
        {
            if (isActive)
            {
                OnBuffActive(currentBuff.buffId);
            }
            else
            {
                OnBuffDeActive(currentBuff.buffId);
            }
        }
        
        private void OnBuffActive(int buffId)
        {
            if (currentBuff!=null && buffId == currentBuff.buffId)
            {
                if ( currentState != RotationState.Active)
                {
                    //状态切换为激活状态
                    currentState = RotationState.Active;
                    isBuffActive = true;
                    activityCount++;
                    isFree = activityCount < freeCount;
                }
                _icon.OnBuffActive();
            }
        }

        private void OnBuffDeActive(int buffId)
        {
            if (currentBuff!=null && buffId == currentBuff.buffId)
            {
                isBuffActive = false;
                //buff失效 暂时不做处理
                //移除监听
                RemoveListener();
            }
        }
        
        #endregion
        
        private void InitializeRotation()
        {
            //在此处考虑是否要缓存，获取初始化buff
            LoadSavedData();
            Messenger.AddListener<bool>(RotationBuffConstant.RotationBuffDialogClose, SetRulePanelShowState);
            AddListener();
            isInitialized = true;
        }
        
        #region icon
        public override BaseIcon RegisterIcon(GameObject go)
        {
            _icon = go.GetComponent<RotationBuffIcon>();
            if (_icon==null)
            {
                _icon = go.AddComponent<RotationBuffIcon>();
            }
            _icon.OnInit(id,iconData);
            //初始化开始轮转
            InitializeRotation();
            return _icon;
        }

        public override void OnClickIcon()
        {
            if (currentState!=RotationState.Showing)
            {
                return;
            }
            ShowRulePanel();
        }

        #endregion

        #region 轮转
        //显示角标
        private void StartShowingState()
        {
            //只有活动开启时才显示
            if (!isActivityEnabled)
            {
                return;
            }
            currentState = RotationState.Showing;
            //计时器
            stateTimer = displayDuration;
            //控制角标icon显示
            UpdateIconDisplay(true);
        }
        //隐藏角标
        private void StartHidingState()
        {
            HideRulePanel();
            currentState = RotationState.Hiding;
            //计时器
            stateTimer = hideDuration;
            //控制角标icon显示
            UpdateIconDisplay(false);
        }
        
        public override void OnUpdate()
        {
            if (!isInitialized)
            {
               return; 
            }
            //只有活动开启时才更新
            if (!isActivityEnabled)
            {
                return;
            }
            //buff激活时不轮转
            if (currentState == RotationState.Active)
            {
                // 检查Buff是否已失效
                if (!currentBuff.isActive)
                {
                    OnBuffDeactivated();
                }
                return;
            }
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
            {
                switch (currentState)
                {
                    case RotationState.Showing:
                        StartHidingState();
                        break;
                    case RotationState.Hiding:
                        RotateToNextBuff();
                        StartShowingState();
                        break;
                }
            }
        }
        private void RotateToNextBuff()
        {
            RemoveListener();
            currentIndex = (currentIndex + 1) % buffList.Count;
            currentBuff = BuffManager.Instance.GetBuffById(buffList[currentIndex]);
            AddListener();
            
            CheckAutoPopRuleDialog();
        }
        private void CheckAutoPopRuleDialog()
        {
            //弹窗已经弹出则进行阻断
            if (RulePanelisShow)
            {
                return;
            }
            curPopupCount++;
            if (curPopupCount == AutoPopupCount && AutoPopupCount>0)
            {
                curPopupCount = 0;
                ShowRulePanel(true);
            }
        }
        
        private void OnBuffDeactivated()
        {
            //隐藏当前buff显示
            StartHidingState();
        }
        
        private void StartNextRotation()
        {
            RotateToNextBuff();
            StartShowingState();
        }
        
        private void UpdateIconDisplay(bool show)
        {
            //只有活动开启时才显示icon
            if (!isActivityEnabled)
            {
                show = false;
            }
            //控制角标显示和隐藏
            if (_icon != null)
            {
                _icon.gameObject.SetActive(show);
                if (show) 
                    _icon.UpdateBuffUI(currentBuff,this);
                else
                {
                    _icon.CancelCountDown();
                }
            }
        }
    
        private bool RulePanelisShow = false;
        private void SetRulePanelShowState(bool isShow)
        {
            RulePanelisShow = isShow;
        }
        private void ShowRulePanel(bool isAutoPop = false)
        {
            SetRulePanelShowState(true);
            Debug.Log($"[RotationBuffActivity] ShowRulePanel for buff {currentBuff.buffName}");
            Dictionary<string,object> eventData = new Dictionary<string, object>();
            eventData["OpenCloseAd"] = OpenCloseAd;
            eventData["isFree"] = isFree;
            eventData["isAutoPop"] = isAutoPop;
            Messenger.Broadcast<Dictionary<string,object>,BaseBuff>(SlotGameDialogManager.OpenRotationBuffDialogMsg,eventData,currentBuff);
        }
    
        private void HideRulePanel()
        {
            Debug.Log($"[RotationBuffActivity] HideRulePanel for buff {currentBuff.buffName}");
            Messenger.Broadcast(RotationBuffConstant.CloseRotationBuffDialog);
        }
        #endregion
    }
}