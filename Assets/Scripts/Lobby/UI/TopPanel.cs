using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Libs;
using UnityEngine.UI;


namespace Classic
{
    public class TopPanel : MonoBehaviour
    {
        public CoinsPanel coinsPanel;
        public LevelPanel levelPanel;
        public WithDrawPanel withDrawPanel;
        public LevelPanel levelPanel_300;
        public WithDrawPanel300 withDrawPanel_300;
        public MenuPanel menuPanel;
        private long lastBalance;
        private int lastCash;
        private GameObject root;
        public static string CLICK_SETTING_OPEN = "CLICK_SETTING_OPEN";
        public static string CLICK_SETTING_CLOSE = "CLICK_SETTING_CLOSE";
        public Sprite image_300bg;
        public Button H5Button;
        public Transform HandSpin;

        private bool FlyCoinsPanelCanShow
        {
            get { return gameObject.activeInHierarchy; }
        }

        private bool initOver = false;

        void Awake()
        {
            Messenger.AddListener(GameConstants.OnSceneInit, Init);
            Messenger.AddListener(SlotControllerConstants.OnBlanceChangeForDisPlay, BalanceChange);
            Messenger.AddListener(SlotControllerConstants.OnCashChangeForDisPlay, CashChange);

            Messenger.AddListener<long>(SlotControllerConstants.OnBlanceChange, RefreshBalance);
            Messenger.AddListener(CLICK_SETTING_OPEN, OpenSettingPanel);
            Messenger.AddListener(CLICK_SETTING_CLOSE, CloseSettingPanel);
            Messenger.AddListener(SlotControllerConstants.OnPopLevelChange, OnPopLevelChange);
            Animator[] animators = GetComponentsInChildren<Animator>();
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].updateMode = AnimatorUpdateMode.UnscaledTime;
            }
            if (HandSpin!=null)
            {
                HandSpin.gameObject.SetActive(false);
            }
        }

        void OnDestroy()
        {
            Messenger.RemoveListener(GameConstants.OnSceneInit, Init);
            Messenger.RemoveListener(CLICK_SETTING_OPEN, OpenSettingPanel);
            Messenger.RemoveListener(CLICK_SETTING_CLOSE, CloseSettingPanel);
            StopAllCoroutines();
            Messenger.RemoveListener(SlotControllerConstants.OnBlanceChangeForDisPlay, BalanceChange);
            Messenger.RemoveListener(SlotControllerConstants.OnCashChangeForDisPlay, CashChange);
            Messenger.RemoveListener<long>(SlotControllerConstants.OnBlanceChange, RefreshBalance);
            Messenger.RemoveListener(SlotControllerConstants.OnPopLevelChange, OnPopLevelChange);
        }

        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            Messenger.AddListener(SlotControllerConstants.OnSpinEnd,onSpinEnd);
            Messenger.AddListener(GameConstants.GetTopPanelScaleAdaption, AssignmentProperty);
        }

        private void OnDisable()
        {
            Messenger.RemoveListener(GameConstants.GetTopPanelScaleAdaption, AssignmentProperty);
            Messenger.RemoveListener(SlotControllerConstants.OnSpinEnd,onSpinEnd);
        }

        public void Init()
        {
            initOver = false;
            if (BaseGameConsole.singletonInstance.IsInSlotMachine())
            {
                BaseGameConsole.singletonInstance.SlotMachineController.MenuTransform = menuPanel.m_MenuBtn.transform;
                if (OnLineEarningMgr.Instance.isInfiniteOpen())
                {
                    BaseGameConsole.singletonInstance.SlotMachineController.CoinsTransform = coinsPanel.coinTarget;
                    BaseGameConsole.singletonInstance.SlotMachineController.CashTransform = withDrawPanel.cashTarget;
                }
                else
                {
                    BaseGameConsole.singletonInstance.SlotMachineController.CashTransform = withDrawPanel_300.cashTarget;
                }
            }
            
            menuPanel.ResetMenuButton();
            lastBalance = -1;
            UpdateUI();
            OnPopLevelChange();
            this.BalanceChange();
            FlyCoinsPanel.Instance.InitNum(lastBalance);
            initOver = true;
        }

        void UpdateUI()
        {
            //300模式和区间模式共用一套ui
            if (!OnLineEarningMgr.Instance.isInfiniteOpen()&& image_300bg!=null)
            {
                GetComponent<Image>().sprite = image_300bg;
            }
            coinsPanel.gameObject.SetActive(OnLineEarningMgr.Instance.isInfiniteOpen());
            withDrawPanel.gameObject.SetActive(OnLineEarningMgr.Instance.isInfiniteOpen()&!PlatformManager.Instance.IsWhiteBao());
            withDrawPanel.InitMoney(OnLineEarningMgr.Instance.Cash());
            if (withDrawPanel_300!=null)
            {
                withDrawPanel_300.gameObject.SetActive(!OnLineEarningMgr.Instance.isInfiniteOpen()&!PlatformManager.Instance.IsWhiteBao());
                withDrawPanel_300.InitMoney(OnLineEarningMgr.Instance.Cash());
            }
            levelPanel.gameObject.SetActive(OnLineEarningMgr.Instance.isInfiniteOpen());
            if (levelPanel_300!=null)
            {
                //只有300模式显示
                levelPanel_300.gameObject.SetActive(OnLineEarningMgr.Instance.isThreeHundredOpen());
            }
        }
        
        void OpenSettingPanel()
        {
        }

        void CloseSettingPanel()
        {
        }
        void OnPopLevelChange()
        {
            if (levelPanel.gameObject.activeInHierarchy)
            {
                levelPanel.SetLevel(OnLineEarningMgr.Instance.GetLevelDesc());
            }

            if (levelPanel_300.gameObject.activeInHierarchy)
            {
                levelPanel_300.SetLevel(OnLineEarningMgr.Instance.GetLevelDesc());
            }
        }

        private void RefreshBalance(long addCoins)
        {
            coinsPanel.SetCoinsNumber(coinsPanel.GetCurrentBalance() + addCoins);
        }

        private void BalanceChange()
        {
            lastBalance = UserManager.GetInstance().UserProfile().Balance();
            coinsPanel.SetCoinsNumber((lastBalance));
            if (FlyCoinsPanelCanShow && initOver) FlyCoinsPanel.Instance.ShowWithCoinsFly(lastBalance);
        }
        private void CashChange()
        {
            lastCash = OnLineEarningMgr.Instance.Cash();
            if (withDrawPanel!=null&& withDrawPanel.gameObject.activeInHierarchy)
            {
                withDrawPanel.ShowWithCoinsFly(lastCash);
            }

            if (withDrawPanel_300!=null&& withDrawPanel_300.gameObject.activeInHierarchy)
            {
                withDrawPanel_300.ShowWithCoinsFly(lastCash);
            }
        }
        private float lastValue = -1f;

        private void AssignmentProperty()
        {
            if (transform == null || coinsPanel == null) return;
            FlyCoinsPanel.Instance.topPanelParentScale = transform.parent ? transform.parent : null;
            FlyCoinsPanel.Instance.topPanelRect = (transform as RectTransform);
            FlyCoinsPanel.Instance.GetCurrentCoinsPanelTrans(coinsPanel.transform);
        }

        private void onSpinEnd()
        {
            if((int)UserManager.GetInstance().UserProfile().GetTotalSpinCounter()==3)
            {
                if (HandSpin!=null)
                {
                    HandSpin.gameObject.SetActive(true);
                }
            }
        }
        public void H5ButtonClick()
        {
            // if (HandSpin!=null)
            // {
            //     HandSpin.gameObject.SetActive(false);
            // }
            // PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.ShowWithDrawGuide);
        }
    }
}