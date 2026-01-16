using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Activity;
using Ads;
using DG.Tweening;
using Libs;
using MarchingBytes;
using Plugins;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Utils;

public class WithDrawDialog : UIDialog
{
    public TextMeshProUGUI money;
    public ToggleGroup panelToggleGroup;
    public Toggle redeemToggle, recordToggle;
    public Button closeBtn;
    public GameObject RedPoint;

    public GameObject panelRedeem;
    public GameObject panelRecord;
    public GameObject tip;
    public TextMeshProUGUI tipInfo;
    private Tween tweenerTip;
    private WithDrawRedeemPanelItem redeemPanelItem;
    private WithDrawRecordPanelItem recordPanelItem;
    public TextMeshProUGUI taskInfo;
    public override void Refresh()
    {
        base.Refresh();
        UpdateCashNum();
        WithDrawTaskActivity withDrawTaskActivity = ActivityManager.Instance.GetActivityByType(ActivityType.WithDrawTask) as WithDrawTaskActivity;
        if (withDrawTaskActivity!=null)
        {
            taskInfo.text = withDrawTaskActivity.GetIconTaskInfo();
        }
        
        closeBtn.onClick.AddListener(OnCloseBtnClick);

    }
    private void OnCloseBtnClick()
    {
        if (WithDrawManager.Instance.CanPlayAd)
        {
            Messenger.Broadcast(ADConstants.PlayAdByEntrance,ADEntrances.Interstitial_Entrance_WITHDRAWCLOSE);
        }
        else
        {
            Close();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Messenger.Broadcast(WithDrawConstants.WithDrawDialogOpened);
        redeemToggle.onValueChanged.AddListener(OnPanelToggleChanged);
        recordToggle.onValueChanged.AddListener(OnPanelToggleChanged);
        
        if (panelRedeem!=null)
        {
            redeemPanelItem = panelRedeem.AddComponent<WithDrawRedeemPanelItem>();
        }
        if (panelRecord!=null)
        {
            recordPanelItem = panelRecord.AddComponent<WithDrawRecordPanelItem>();
        }
        //添加withdrawitem引导
        if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.WithDrawItem))
        {
            // 使用null让系统自动使用DialogCanvas作为父节点
            // 如果需要指定父节点，确保节点存在：transform.Find("Anchor")?.transform
            //开始WithDrawItem引导
            TutorialManager.Start(TutorialManager.TutorialStep.WithDrawItem, transform.Find("Anchor")?.transform, (step) => {
                Debug.Log($"[WithDrawDialog] WithDrawItem引导已完成");
            });
        }
        else
        {
            Debug.Log("[WithDrawDialog] WithDrawItem引导已完成，跳过");
        }
    }

    protected override void Start()
    {
        base.Start();
        //默认展示第一个
        redeemToggle.isOn = true;
        // 初始化红点显示状态
        UpdateRedPointDisplay();
    }

    protected override void OnEnable()
    {
        Messenger.AddListener(GameDialogManager.CloseWithDrawDialog,Close);
        Messenger.AddListener(SlotControllerConstants.OnCashChangeForDisPlay,UpdateCashNum);
        Messenger.AddListener<string>(WithDrawConstants.ShowTipMsg,ShowTip);
        Messenger.AddListener<int>(ADConstants.PlayWithDrawCloseAD,ShowVideoCallBack);
        Messenger.AddListener<int>(ADConstants.PlayWithDrawCloseADFailed,ShowVideoCallBack);
        Messenger.AddListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);
        Messenger.AddListener(WithDrawConstants.ShowRecordRedPoint,UpdateRedPointDisplay);
        WithDrawManager.WithDrawUIShow = true;
    }
    private void ShowVideoCallBack(int res)
    {
        Close();
        WithDrawManager.Instance.CanPlayAd = false;
        WithDrawManager.Instance.StartCountdown();
    }

    protected override void OnDisable()
    {
        Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
        Messenger.RemoveListener(GameDialogManager.CloseWithDrawDialog,Close);
        Messenger.RemoveListener(SlotControllerConstants.OnCashChangeForDisPlay,UpdateCashNum);
        Messenger.RemoveListener<string>(WithDrawConstants.ShowTipMsg,ShowTip);
        Messenger.RemoveListener<int>(ADConstants.PlayWithDrawCloseAD,ShowVideoCallBack);
        Messenger.RemoveListener<int>(ADConstants.PlayWithDrawCloseADFailed,ShowVideoCallBack);
        Messenger.RemoveListener<string>(ADConstants.NotMeetConditionMsg,HandleNotMeetConditionMsg);
        Messenger.RemoveListener(WithDrawConstants.ShowRecordRedPoint,UpdateRedPointDisplay);
        WithDrawManager.WithDrawUIShow = false;
    }
    private void HandleNotMeetConditionMsg(string arg0)
    {
        Close();
    }

    public void UpdateCashNum()
    {
        money.text = OnLineEarningMgr.Instance.GetMoneyStr(OnLineEarningMgr.Instance.Cash(),needIcon:false);
    }

    void OnPanelToggleChanged(bool value)
    {
        Toggle selected = panelToggleGroup.ActiveToggles().FirstOrDefault();
        if (selected != null)
        {
            if (selected == redeemToggle)
            {
                redeemPanelItem.Show();
                recordPanelItem.Hide();
            }else if (selected == recordToggle)
            {
                redeemPanelItem.Hide();
                recordPanelItem.Show();
                // 点击recordToggle时，隐藏红点并保存状态
                HideRecordRedPoint();
            }
        }
    }
    
    void ShowTip(string info)
    {
        if (tweenerTip!=null && tip.activeInHierarchy)
        {
            return;
        }
        tipInfo.text = info;
        tip.SetActive(true);
        tip.transform.localPosition = new Vector3(0, -100f, 0);
        tweenerTip = tip.transform.DOLocalMoveY(0f, 2f)
            .OnComplete(() => {
                if (tip!=null)
                {
                    tip.gameObject.SetActive(false);
                    tip.transform.localPosition = new Vector3(0, -100f, 0);
                }
                tweenerTip.Kill();
                tweenerTip = null;
            });
    }

    void UpdateRedPointDisplay()
    {
        if (RedPoint != null)
        {
            RedPoint.SetActive(WithDrawManager.Instance.showRecordRedPoint);
        }
    }

    void HideRecordRedPoint()
    {
        // 隐藏红点并保存状态
        WithDrawManager.Instance.showRecordRedPoint = false;
        WithDrawManager.Instance.SaveProgressData();
        UpdateRedPointDisplay();
    }
}
