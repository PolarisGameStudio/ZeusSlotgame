using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Classic;
using TMPro;
using System.Collections.Generic;
using Core;
using DG;
using DG.Tweening;

// 测试TutorialManager是否可见
#if UNITY_EDITOR
// [UnityEditor.InitializeOnLoad]
#endif

public class SpinButtonStyle : MonoBehaviour
{
    public SpinButtonState state;
    public Image StateButtonImage;
    public GameObject CloseNumListBtn;
    public GameObject NumListMask;
    public GameObject SpinAni;
    public Sprite Disable;
    public Sprite SpinNormal;
    public Sprite SpinPress;
    public Sprite SpinFree;
    public Sprite SpinFreePress;
    public Sprite SpiningNormal;
    public Sprite SpiningPress;
    public Sprite AutoSpinNormal;
    public Sprite AutoPress;
    public Sprite AutoBtnPress;
    public Sprite AutoBtnNormal;
    public Sprite AutoBtnUnnormal;
    public Sprite AutoText;
    public Sprite AutoTextPress;
    public Image Auto;
    public Image AutoTextImage;
    
    public Sprite stopButtonNormalSpt;
    public Sprite stopButtonPressSpt;
    public GameObject numOfMenuObj;
    public List<Button> menuBtnList;
    private RectTransform spinNumTextRTSF;
    public Text spinNumText;
    private Button button;

    public GameObject FreeSpinPanel;
    public TextMeshProUGUI FreeSpinText;
    public Image StopSpin;
    public Image FreeSpinDisable;
    public Image SpinText;
    public List<Sprite> SpinTexts;
    public GameObject Guide;
    public const string DISABLEAUTOSPIN = "DisableAutoSpin";
    public const string DISABLESPIN = "DisableSpin";
    public const string ENABLESPIN = "EnableSpin";
    public const string SOPTSPININFREE = "WaitStopSpinInFree";//在freespin中临时禁用spinbutton
    public const string END_AUTO_RUN = "EndAutoRun";
    public const string FORCE_END_AUTO_RUN = "ForceEndAutoRun";
    public const string CHANGEBUTTONIMAGEONENTERFREE = "ChangeButtonImageOnEnterFree";
    public const string CHANGEBUTTONIMAGEONQUITFREE = "ChangeButtonImageOnQuitFree";
    //冰火女王新增：不显示Free次数
    public const string HIDEFREETEXT = "HideFreeText";
    
    private int autoSpinNum = 0;
    
    private bool _autoSpinState;
    private bool AutoSpinState{
        set
        {
            _autoSpinState = value;
            AutoSpinSuspendManager.Reset(value);
        }
        get
        {
          return  _autoSpinState;
        } 
    }
    
    private bool m_spinBtnActive = true;
    private bool m_isAutoSpin = true;
    private Vector3 objStartPos;
    //冰火女王新增：隐藏Free上Text
    private bool hideFreeText = false;
    private Vector3 freeSpinsImageInitValue = new Vector3(0,95f,0);
    private Vector3 freeSpinsImageCenterValue = new Vector3(0,69f,0);
    
    //public bool isStartSpin = false;
    // Use this for initialization
    void Awake()
    {
        FreeSpinPanel.SetActive(false);
        Messenger.AddListener(GameConstants.SPINNOMONEY, EndAutoRun);
        Messenger.AddListener(DISABLESPIN, DisableSpinBtn);
        Messenger.AddListener(ENABLESPIN, EnableSpinBtn);
        Messenger.AddListener(SOPTSPININFREE, WaitStopSpinInFree);
        Messenger.AddListener(END_AUTO_RUN, EndAutoRun);
        Messenger.AddListener(FORCE_END_AUTO_RUN, ForceEndAutoRun);
        Messenger.AddListener<bool>(DISABLEAUTOSPIN, IsAutoSpin);
        Messenger.AddListener(GameConstants.DO_SPIN, OnDoSpin);
        Messenger.AddListener(CHANGEBUTTONIMAGEONENTERFREE,ChangeButtonImageOnEnterFree);
        Messenger.AddListener(CHANGEBUTTONIMAGEONQUITFREE,ChangeButtonImageOnQuitFree);
        // Messenger.AddListener(GameConstants.OnSlotMachineSceneInit,InitScene);
        Messenger.AddListener<bool>(HIDEFREETEXT,SetHideFreeText);
        Messenger.AddListener<bool>(GameConstants.NEW_USER_GUIDE,RefreshNewUserGuide);
    }

    private void Start()
    {
        InitNumOfMenuBtn();
        if (Auto!=null)
        {
            Auto.gameObject.SetActive(CanShowAutoSpinButton());
        }
        RefreshNewUserGuide(UserManager.GetInstance().UserProfile().IsFirstGameSession);
    }

    /// <summary>
    /// 设置隐藏FreeText
    /// </summary>
    /// <param name="isHide"></param>
    private void SetHideFreeText(bool isHide)
    {
        hideFreeText = isHide;
    }

    private void SetHideFreeElement()
    {
        if (hideFreeText)
        {
            //隐藏紫色条
            if (FreeSpinPanel.GetComponent<Image>().enabled)
                FreeSpinPanel.GetComponent<Image>().enabled = false;
            //隐藏Text
            if (FreeSpinText.gameObject.activeSelf)
                FreeSpinText.gameObject.SetActive(false);
            //FreeSpins图标放中间
            if (FreeSpinPanel.transform.childCount > 1)
            {
                if (FreeSpinPanel.transform.GetChild(1).localPosition != freeSpinsImageCenterValue)
                    FreeSpinPanel.transform.GetChild(1).localPosition = freeSpinsImageCenterValue;
            }

            if (spinNumText.gameObject.activeSelf)
                spinNumText.gameObject.SetActive(false);
        }
        else
        {
            //显示紫色条
            if (!FreeSpinPanel.GetComponent<Image>().enabled)
                FreeSpinPanel.GetComponent<Image>().enabled = true;
            //显示Text
            if (!FreeSpinText.gameObject.activeSelf)
                FreeSpinText.gameObject.SetActive(true);
            //FreeSpins图标归位
            if (FreeSpinPanel.transform.childCount > 1)
            {
                if (FreeSpinPanel.transform.GetChild(1).localPosition != freeSpinsImageInitValue)
                    FreeSpinPanel.transform.GetChild(1).localPosition = freeSpinsImageInitValue;
            }
        }
    }

    // private void InitScene()
    // {
    //     if (BaseSlotMachineController.Instance!=null && BaseSlotMachineController.Instance.reelManager.SpinUseNetwork)
    //     {
    //         Messenger.AddListener(GameConstants.NetworkSpinResponse, EnableSpinBtn);
    //         Messenger.AddListener(GameConstants.NetworkSpinResponse, EnableFsBtn);
    //     }
    // }
    /// <summary>
    /// 收到Spin消息通知
    /// </summary>
    private void OnDoSpin()
    {
        if (Core.ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu && state != SpinButtonState.FREE)
        {
            //每次Spin后，Spin次数减1
            autoSpinNum -= 1;
            if (autoSpinNum <= 0)
            {
                spinNumText.gameObject.SetActive(false);
                StopAutoSpin();
                return;
            }
            spinNumText.text = autoSpinNum + "";
        }

        if (Auto!=null)
        {
            if (CanShowAutoSpinButton())
            {
                Auto.gameObject.SetActive(true);
                // 检查WithDrawButton引导是否已完成
                if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.AutoSpin))
                {
                    Transform banner = GameObject.Find("BannerCanvas").transform;
                    //开始第二个WithdrawButton引导
                    TutorialManager.Start(TutorialManager.TutorialStep.AutoSpin, banner, (step)=>{
                        OnAutoSpinClick();
                    });
                }
                else
                {
                    Debug.Log("[WithDrawPanel] AutoSpin引导已完成，跳过");
                }      
            }
        }    
    }

    /// <summary>
    /// 刷新guide状态
    /// </summary>
    /// <param name="state"></param>
    private void RefreshNewUserGuide(bool state)
    {
        // Messenger.Broadcast<bool>(GameConstants.ShowButtonMask,state);
        // 触发首次Spin引导开始
        // 使用新的Prefab方案，无需传入RectTransform
        if (!state)
        {
            return;
        }
        // 检查FirstSpin引导是否已完成
        if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.FirstSpin))
        {
            Transform banner = GameObject.Find("BannerCanvas").transform;
            TutorialManager.Start(TutorialManager.TutorialStep.FirstSpin, banner, (step)=>{
                //执行一次点击spin方法
                //不是第一次 spin，改为正常点击 spin
                if (BaseSlotMachineController.Instance.DoSpin())
                {
                    WaitStopSpin();
                    StateButtonImage.sprite = SpiningNormal;
                }
                else
                {
                    StateButtonImage.sprite = SpinNormal;
                    SpinText.sprite = SpinTexts[0];
                }
                Messenger.Broadcast(GameConstants.NOW_SPIN_CLICK);
            });
        }
        else
        {
            Debug.Log("[SpinButtonStyle] FirstSpin引导已完成，跳过");
        }
    }

    /// <summary>
    /// 初始化Spin按钮菜单信息
    /// </summary>
    private void InitNumOfMenuBtn()
    {
        if (menuBtnList == null)
        {
           // Debug.LogError("----------->SpinButtonStyle InitNumOfMenuBtn is invoked,the menuBtnList is null,Please check it!");
            return;
        }
        if (numOfMenuObj == null)
        {
           // Debug.LogError("----------->SpinButtonStyle InitNumOfMenuBtn is invoked,the numOfMenuObj is null,Please check it!");
            return;
        }
        //先隐藏菜单
        objStartPos = numOfMenuObj.transform.localPosition;
        NumListMask.SetActive(false);
        spinNumText.gameObject.SetActive(false);
        spinNumTextRTSF = spinNumText.GetComponent<RectTransform>();
        //spinNumTextRTSF.anchoredPosition = new Vector2(spinNumTextRTSF.anchoredPosition.x, 7);
        CloseNumListBtn.gameObject.SetActive(false);
        //============================================================================
        //解析PLIST 菜单数据
        #if UNITY_EDITOR
        Core.ApplicationConfig.GetInstance().AutoSpinNumOfMenu = "25,50,100,200";
        #endif
        if (string.IsNullOrEmpty(Core.ApplicationConfig.GetInstance().AutoSpinNumOfMenu))
        {
            //Debug.LogError("----------->SpinButtonStyle InitNumOfMenuBtn is invoked,the PLIST AutoSpinNumOfMenu is null,Please check it!");
            return;
        }
        string [] arrayMenus = Core.ApplicationConfig.GetInstance().AutoSpinNumOfMenu.Split(',');
        if(arrayMenus.Length != 4)
        {
            //Debug.LogError("----------->SpinButtonStyle InitNumOfMenuBtn is invoked,the PLIST AutoSpinNumOfMenu data is error,Please check it!");
            return;
        }
        //设置Spin按钮菜单
        for (int i  = 0;i <menuBtnList.Count;i++)
        { 
            Button btn = menuBtnList[i];
            btn.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = arrayMenus[i];
            btn.gameObject.name = "Button_" + arrayMenus[i]+"_"+(i+1);//Button_Spin次数_Spin档位
            btn.onClick.AddListener(()=> { OnClickNumOfSpinBtn(btn.gameObject); });
        }
    }
    
    /***
         *Spin菜单按钮事件
         */
    private void OnClickNumOfSpinBtn(GameObject obj)
    {
        //解析点击按钮对应的spin次数
        string numStr = obj.name.Split('_')[1];
        autoSpinNum = int.Parse(numStr);
        spinNumText.text = autoSpinNum+"";
        //解析档位
        BaseSlotMachineController.Instance.reelManager.AutoSpinSegment = int.Parse(obj.name.Split('_')[2]);
        //显示Spin按钮剩余次数
        spinNumText.gameObject.SetActive(true);
        //spinNumTextRTSF.anchoredPosition = new Vector2(spinNumTextRTSF.anchoredPosition.x, -4);
        //更换按钮背景图为stop
        SpinText.sprite = stopButtonNormalSpt;
        //隐藏按钮菜单
        CloseNumListAni();
        //auto按钮置灰
        // CanClickAutoSpin = false;
        // Auto.sprite = AutoBtnUnnormal;
        // AutoTextImage.sprite = AutoTextPress;
        //numOfMenuObj.SetActive(false);
        //开始AutoSpin
        AutoRun();
    }

    void OnDestroy()
    {
        Messenger.RemoveListener(GameConstants.SPINNOMONEY, EndAutoRun);
        Messenger.RemoveListener(DISABLESPIN, DisableSpinBtn);
        Messenger.RemoveListener(ENABLESPIN, EnableSpinBtn);
        Messenger.RemoveListener(SOPTSPININFREE, WaitStopSpinInFree);
        Messenger.RemoveListener(END_AUTO_RUN, EndAutoRun);
        Messenger.RemoveListener(FORCE_END_AUTO_RUN, ForceEndAutoRun);
        Messenger.RemoveListener<bool>(DISABLEAUTOSPIN, IsAutoSpin);
        Messenger.RemoveListener(GameConstants.DO_SPIN, OnDoSpin);
        Messenger.RemoveListener(CHANGEBUTTONIMAGEONENTERFREE,ChangeButtonImageOnEnterFree);
        Messenger.RemoveListener(CHANGEBUTTONIMAGEONQUITFREE,ChangeButtonImageOnQuitFree);
        if(BaseSlotMachineController.Instance != null && BaseSlotMachineController.Instance.reelManager != null) {
            BaseSlotMachineController.Instance.reelManager.AutoSpinSegment = 0;
        }
        // if (BaseSlotMachineController.Instance != null &&BaseSlotMachineController.Instance.reelManager.SpinUseNetwork)
        {
            Messenger.RemoveListener(GameConstants.NetworkSpinResponse, EnableSpinBtn);
            Messenger.RemoveListener(GameConstants.NetworkSpinResponse, EnableFsBtn);
        }
        // Messenger.RemoveListener(GameConstants.OnSlotMachineSceneInit,InitScene);
        Messenger.RemoveListener<bool>(HIDEFREETEXT,SetHideFreeText);
        Messenger.RemoveListener<bool>(GameConstants.NEW_USER_GUIDE,RefreshNewUserGuide);

    }

    public bool CanShowAutoSpinButton()
    {
        if (ApplicationConfig.GetInstance().SpinPattern == 0)
        {
            return false;
        }
        if (ApplicationConfig.GetInstance().SpinPattern == 1)
        {
            if (UserManager.GetInstance().UserProfile().GetTotalSpinCounter() >= ApplicationConfig.GetInstance().SpinShowAutoLimit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    public void EndAutoRun()
    {
        if(state == SpinButtonState.FREE)
        {
            return;
        }
        
        this.state = SpinButtonState.NORML;
        if (BaseSlotMachineController.Instance != null && BaseSlotMachineController.Instance.reelManager != null)
        {
            BaseSlotMachineController.Instance.reelManager.AutoRun = false;
        }
        spinNumText.gameObject.SetActive(false);
        StateButtonImage.sprite = SpinNormal;
        SpinText.sprite = SpinTexts[0];
    }
    
    public void ForceEndAutoRun()
    {
        this.state = SpinButtonState.NORML;
        if (BaseSlotMachineController.Instance != null && BaseSlotMachineController.Instance.reelManager != null)
        {
            BaseSlotMachineController.Instance.reelManager.AutoRun = false;
        }
        spinNumText.gameObject.SetActive(false);
        StateButtonImage.sprite = SpinNormal;
        SpinText.sprite = SpinTexts[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (isTouching)
        {
            Libs.EventSystemUtils.IsOpen = false;  //静态变量
        }
        else
        {
            Libs.EventSystemUtils.IsOpen = true;
        }
        if (!BaseSlotMachineController.Instance)
            return;
        switch (state)
        {
            case SpinButtonState.FREE:
                if (BaseSlotMachineController.Instance.isUseMachineFreespinBtn)
                {
                    return;
                }
                if (!isTouching) StateButtonImage.sprite = SpinFree;
                SetHideFreeElement();
                FreeSpinPanel.SetActive(true);
                break;
            case SpinButtonState.Disable:
                StateButtonImage.sprite = Disable;
                //SetSpinText ();
                break;
            case SpinButtonState.NORML:
                FreeSpinPanel.SetActive(false);
                if (!isTouching)
                {
                    //                if ((BaseSlotMachineController.Instance.reelManager.isSpining ())) {
                    //						StateButtonImage.sprite = Spining;
                    //                } else {
                    StateButtonImage.sprite = SpinNormal;
                    SpinText.sprite = SpinTexts[0];
                    //                }
                    //SetSpinText ();
                }
                else
                {
                    //spin按钮长按点击开启autospin
                    if(!m_isAutoSpin) return;
                    bool isLongPress = lastTime > 0 && Time.time > lastTime + 0.9;
                    if (!longPressActive)
                    {
                        if (isLongPress)
                        {
                            longPressActive = true;
                            if (Core.ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu)
                            {
                                if(NumListMask != null)
                                {
                                    NumListMask.SetActive(true);
                                    numOfMenuObj.SetActive(true);
                                    //添加打开动画
                                    CloseNumListBtn.gameObject.SetActive(true);
                                    numOfMenuObj.transform.DOLocalMove(new Vector3(objStartPos.x,objStartPos.y+321, 0), 0.25f);
                                }
                            }
                            else
                            {
                                //移动此功能至Autospin按钮点击
                                BaseSlotMachineController.Instance.reelManager.AutoRun = true;
                                StateButtonImage.sprite = AutoPress;
                                SpinText.sprite = SpinTexts[2];
                                this.ClearSpinAni();
                                BaseSlotMachineController.Instance.DoSpin();
                                state = SpinButtonState.AUTOSPIN;
                                Auto.sprite = AutoBtnPress;
                                AutoTextImage.sprite = AutoTextPress;
                            }
                        }
                    }
                }
                break;
            case SpinButtonState.AUTOSPIN:
                FreeSpinPanel.SetActive(false);

                //if (!BaseSlotMachineController.Instance.reelManager.AutoRun && !BaseSlotMachineController.Instance.IsAutoSpinSuspending())
                //{
                //    state = SpinButtonState.NORML;
                //    StateButtonImage.sprite = SpinNormal;
                //    SpinText.sprite = SpinTexts[0];
                //    //SetSpinText ();
                //}
                //else
                //{
                    if (isTouching)
                    {
                        if (Core.ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu)
                        {
                        }
                        else
                        {
                            StateButtonImage.sprite = AutoPress;
                            SpinText.sprite = SpinTexts[2];
                        }
                    }
                    else
                    {
                        if (Core.ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu)
                        {
                            StateButtonImage.sprite = SpinNormal;
                            SpinText.sprite = stopButtonNormalSpt;
                            if (!spinNumText.gameObject.activeSelf)
                                spinNumText.gameObject.SetActive(true);
                        }
                        else
                        {
                            StateButtonImage.sprite = AutoSpinNormal;
                            SpinText.sprite = SpinTexts[2];
                        }
                    }
                //}
                break;
            default:
                break;
        }
    }

    float lastTime;
    bool isTouching = false;
    bool longPressActive = false;

    //进入 freespin时修改按钮风格，在底框添加现金
    private void ChangeButtonImageOnEnterFree()
    {
        StateButtonImage.sprite = SpinFree;
    }
    
    private void ChangeButtonImageOnQuitFree()
    {
        StateButtonImage.sprite = SpinNormal;
    }

    // private bool CanClickAutoSpin = false;
    public void OnAutoSpinClick()
    {
        Debug.Log("OnAutoSpinClick state = " + state);
        //分为两种状态
        if (state == SpinButtonState.NORML)
        {
            BaseSlotMachineController.Instance.reelManager.AutoRun = true;
            StateButtonImage.sprite = AutoPress;
            SpinText.sprite = SpinTexts[2];
            this.ClearSpinAni();
            BaseSlotMachineController.Instance.DoSpin();
            state = SpinButtonState.AUTOSPIN;
            Auto.sprite = AutoBtnPress;
            AutoTextImage.sprite = AutoTextPress;
        }
        else if (state == SpinButtonState.AUTOSPIN)
        {
            // if (Core.ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu)
            // {
            //     StopAutoSpin();
            // }
            BaseSlotMachineController.Instance.reelManager.AutoRun = false;
            if(!BaseSlotMachineController.Instance.spinning)
            {
                Messenger.Broadcast<bool> (SlotControllerConstants.ActiveButtons,false);
            }
            state = SpinButtonState.NORML;
            Auto.sprite = AutoBtnNormal;
            AutoTextImage.sprite = AutoText;
            spinNumText.gameObject.SetActive(false);
        }
    }
    
    public void OnTouchDown()   //spin按下触发,改按钮相应的显示
    {
        if (Libs.UIManager.Instance.CheckExistDialog())
        {
            return;
        }
        isTouching = true;
        longPressActive = false;

        switch (state)   //按钮的状态
        {
            case SpinButtonState.FREE:
                if (BaseSlotMachineController.Instance.isUseMachineFreespinBtn)
                {
                    return;
                }
                StateButtonImage.sprite = SpinFreePress; //bg
                SpinText.sprite = SpinTexts[1]; //text
                break;
            case SpinButtonState.NORML:
                if(m_isAutoSpin)
                    SpinAni.SetActive(true);
                    StateButtonImage.sprite = SpinPress;
                lastTime = Time.time;
                if ((BaseSlotMachineController.Instance.reelManager.isSpining())) //游戏状态不是ready的时候
                {
                    StateButtonImage.sprite = SpiningPress;
                    lastTime = Time.time;
                    //SetSpinText ();
                }
                else
                {
                    StateButtonImage.sprite = SpinPress;
                    SpinText.sprite = SpinTexts[1];
                    lastTime = Time.time;
                    //SetSpinText ();
                }
                break;
            case SpinButtonState.AUTOSPIN:
                if (Core.ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu)//已经打开了antoplay的界面   ？
                {
                    StateButtonImage.sprite = stopButtonPressSpt;
                    //spinNumTextRTSF.anchoredPosition = new Vector2(spinNumTextRTSF.anchoredPosition.x, -13);
                }
                else
                {
                    StateButtonImage.sprite = AutoPress;
                }
                
                lastTime = -1;
                //SetAutoText ();
                break;
            default:
                break;
        }
    }

    public void OnTouchUp()
    {
        isTouching = false;
        if (Libs.UIManager.Instance!=null && Libs.UIManager.Instance.CheckExistDialog())
        {
            return;
        }
        if (!m_spinBtnActive)
            return;
        bool isLongPress = lastTime > 0 && Time.time > lastTime + 0.9;
        
        if(BaseSlotMachineController.Instance==null||BaseSlotMachineController.Instance.reelManager==null)
            return;
        switch (state)
        {
            case SpinButtonState.FREE:
                if (!BaseSlotMachineController.Instance.isUseMachineFreespinBtn)
                {
                    StateButtonImage.sprite = SpinFree;
                }
                //根据
                if (BaseSlotMachineController.Instance.reelManager.isSpining())
                { 
                    if (!BaseSlotMachineController.Instance.reelManager.fastStopDisabled) 
                    {
                        BaseSlotMachineController.Instance.reelManager.fastStop = true;
                        Messenger.Broadcast(GameConstants.ENABLE_FAST_STOP);
                        BaseSlotMachineController.Instance.reelManager.StopRun();
                    }
                }
                else
                {
                    BaseSlotMachineController.Instance.DoSpin();
                    Messenger.Broadcast(GameConstants.NOW_SPIN_CLICK);
                }
                break;
            case SpinButtonState.NORML:
                if (isLongPress)
                {
                    
                }
                else
                {
                    if (Guide.gameObject.activeInHierarchy)
                    {
                        Guide.gameObject.SetActive(false);
                    }
                    SpinAni.SetActive(false);
                    if (BaseSlotMachineController.Instance.reelManager.isSpining())
                    {
                        // StateButtonImage.sprite = SpinNormal;
                        // SpinText.sprite = SpinTexts[0];
                        // BaseSlotMachineController.Instance.reelManager.fastStop = true;
                        // Messenger.Broadcast(GameConstants.ENABLE_FAST_STOP);
                        // BaseSlotMachineController.Instance.reelManager.StopRun();
                    }
                    else
                    {
                        //此处进行判断第一次点击时直接切换为autoSpin模式
                        if (BaseSlotMachineController.Instance.MachineSpinTime==0 && CanShowAutoSpinButton())
                        {
                            OnAutoSpinClick();
                            return;
                        }
                        //不是第一次 spin，改为正常点击 spin
                        if (BaseSlotMachineController.Instance.DoSpin())
                        {
                            WaitStopSpin();
                            StateButtonImage.sprite = SpiningNormal;
                        }
                        else
                        {
                            StateButtonImage.sprite = SpinNormal;
                            SpinText.sprite = SpinTexts[0];
                        }

                        if (NumListMask!= null)
                        {
                           // 添加关闭动画
                           CloseNumListAni();
                        }
                        Messenger.Broadcast(GameConstants.NOW_SPIN_CLICK);
                    }
                }
                break;
            case SpinButtonState.AUTOSPIN:
                if (Core.ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu)
                {
                    StopAutoSpin();
                }
                
                //本次中触发auto
                if(longPressActive)
                {
                    //StateButtonImage.sprite = AutoSpinNormal;
                    //SpinText.sprite = SpinTexts[2];
                }
                else
                {
                    //移动至 AutoSpin停止按钮点击
                    BaseSlotMachineController.Instance.reelManager.AutoRun = false;
                    if(!BaseSlotMachineController.Instance.spinning)
                    {
                        Messenger.Broadcast<bool> (SlotControllerConstants.ActiveButtons,false);
                    }
                    state = SpinButtonState.NORML;
                    Auto.sprite = AutoBtnNormal;
                    AutoTextImage.sprite = AutoText;
                }
                break;
            default:
                break;
        }
    }

    public void OnButtonExit()
    {
        if (SpinAni != null) SpinAni.SetActive(false);
    }

    private void ClearSpinAni()
    {
        //190528 立马清除
        //new Libs.DelayAction (0.5f, null, () => {
        SpinAni.SetActive(false);
        //}).Play ();
    }

    private void WaitStopSpinInFree()
    {
        if (state == SpinButtonState.FREE)
        {
            if (!BaseSlotMachineController.Instance.isUseMachineFreespinBtn)
            {
                DisableFreeSpinBtn(); 
            }
            
        }
            
    }

    private void WaitStopSpin()
    {
        DisableSpinBtn();
        //0.5s后按钮变绿
        // if (!BaseSlotMachineController.Instance.reelManager.SpinUseNetwork)
        // {
        //     Libs.CoroutineUtil.DoDelayByContext(this,0.5f,delegate { EnableSpinBtn(); });
        // }
    }

    private void DisableFreeSpinBtn()
    {
        FreeSpinDisable.gameObject.SetActive(true);
        if (!BaseSlotMachineController.Instance.reelManager.SpinUseNetwork)
        {
            Libs.CoroutineUtil.DoDelayByContext(this,0.5f,delegate {
                EnableFsBtn(); });
        }
    }

    private void EnableFsBtn()
    {
        if (FreeSpinDisable != null) FreeSpinDisable?.gameObject.SetActive(false);
    }

    private void DisableSpinBtn()
    {
        if (!m_spinBtnActive)
            return;
        m_spinBtnActive = false;
        StopSpin.gameObject.SetActive(true);
    }

    private void EnableSpinBtn()
    {
//        Debug.Log("EnableSpinBtn"+ m_spinBtnActive);
        if (m_spinBtnActive)
            return;
        m_spinBtnActive = true;
        StopSpin.gameObject.SetActive(false);
    }

    public void IsAutoSpin(bool state)
    {
        m_isAutoSpin = state;
    }
    
    private void AutoRun()
    {
        state = SpinButtonState.AUTOSPIN;
        BaseSlotMachineController.Instance.reelManager.SetAutoRun(true);
        AutoSpinState = true;
        if (BaseSlotMachineController.Instance.reelManager.isSpining())
        {
               
            if (!BaseSlotMachineController.Instance.reelManager.fastStopDisabled)
            {
                StateButtonImage.sprite = SpinNormal;
                BaseSlotMachineController.Instance.reelManager.StopRun();
            }
        }
        else
        {
               
            Analytics.GetInstance().LogEvent(Analytics.AutoSpinClicked);
            if (BaseSlotMachineController.Instance.DoSpin())
            {
                if (Core.ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu)
                {
                    SpinText.sprite = stopButtonNormalSpt;
                    if (!spinNumText.gameObject.activeSelf)
                        spinNumText.gameObject.SetActive(true);
                }
                else
                {
                    StateButtonImage.sprite = AutoSpinNormal;
                }
                   
            }
            else
            {
                state = SpinButtonState.NORML;
                StateButtonImage.sprite = SpinNormal;
                spinNumText.gameObject.SetActive(false);
                BaseSlotMachineController.Instance.reelManager.SetAutoRun(false);
                AutoSpinState = false;
                BaseSlotMachineController.Instance.statisticsManager.SendAutoSpinTimesEvent();
            }
        }
    }
    
    private void StopAutoSpin()
    {
            
        BaseSlotMachineController.Instance.reelManager.AutoSpinSegment = 0;
        state = SpinButtonState.NORML;
        if (Core.ApplicationConfig.GetInstance().IsOpenAutoSpinNumOfMenu)
        {
            StateButtonImage.sprite = SpinNormal ;
            if (spinNumText != null) spinNumText.gameObject.SetActive(false);
            //显示AutoSpin按钮
            // CanClickAutoSpin = true;
            // Auto.sprite = AutoBtnNormal;
            // AutoTextImage.sprite = AutoText;
        }
        else
        {
            StateButtonImage.sprite = AutoSpinNormal;
        }
       
        BaseSlotMachineController.Instance.reelManager.SetAutoRun(false);
        BaseSlotMachineController.Instance.Set_autospinSuspendingValue(false);
        AutoSpinState = false;
        
        if (!BaseSlotMachineController.Instance.reelManager.isSpining())
        {
            if (ResultStateManager.Instante.slotController.reelManager.enableBetChangeAfterEpicWin)
            {
                if(!BaseSlotMachineController.Instance.reelManager.IsExistFeature()) 
                    Messenger.Broadcast<bool>(SlotControllerConstants.ActiveButtons, false);
            }
        }
        BaseSlotMachineController.Instance.statisticsManager.SendAutoSpinTimesEvent();
    }

    public void CloseNumListAni()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append( numOfMenuObj.transform.DOLocalMove(objStartPos,0.25f).SetEase(Ease.Linear));
        sequence.AppendCallback(delegate { NumListMask.SetActive(false); CloseNumListBtn.gameObject.SetActive(false);});
    }

    public enum SpinButtonState
    {
        NORML,
        FREE,
        AUTOSPIN,
        Disable
    }
}
