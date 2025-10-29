using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardSystem;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.U2D;
using UnityEngine.UI;

public class RedeemItem : MonoBehaviour
{
    private RedeemItemData itemData;
    //序号
    private int index;
    public TextMeshProUGUI cashTMP;
    public Image paltformImg;
    public Button redeemBtn;
    public RectTransform inProgress;
    public RectTransform condition;
    public TextMeshProUGUI progressTMP;
    public Image progressBar;
    public TextMeshProUGUI conditionTMP;
    private Coroutine timeCor;
    public Image taskIconImg;
    public GameObject sequentialTaskObj;
    public TextMeshProUGUI sequentialTaskProgressTMP;
    public Button sequentialTaskBtn;
    private void Awake()
    {
        if (redeemBtn != null)
        {
            UGUIEventListener.Get(redeemBtn.gameObject).onClick = OnButtonClickHandler;
        }
        if (sequentialTaskBtn != null)
        {
            UGUIEventListener.Get(sequentialTaskBtn.gameObject).onClick = OnButtonClickHandler;
        }
    }

    private void OnEnable()
    {
        Messenger.AddListener<int>(WithDrawConstants.UpdateRedeemItemState, OnTaskStatusChange);
    }
    
    private void OnDisable()
    {
        Messenger.RemoveListener<int>(WithDrawConstants.UpdateRedeemItemState, OnTaskStatusChange);
    }


    //当前环节任务状态变更
    void OnTaskStatusChange(int taskId)
    {
        if (itemData == null)
        {
            return;
        }
        if(taskId == itemData.CurTask.TaskId)
        {
            //提现任务完成，进行任务切换
            itemData.SwitchToNextTask();
            RefreshUI();
        }
    }
    
    public void UpdateData(int i, RedeemItemData data)
    {
        index = i;
        itemData = data;
        RefreshUI();
        AddressableManager.Instance.LoadAsset<SpriteAtlas>("Platform.spriteatlas", (result) =>
        {
            if (result != null)
            {
                Sprite sp = result.GetSprite(itemData.platSpIndex.ToString());
                if (sp != null)
                {
                    paltformImg.sprite = sp;
                }
            }
        });
        cashTMP.text = OnLineEarningMgr.Instance.GetCashStr(itemData.RewardCash,needIcon:false);
    }

    
    public void RefreshUI()
    {
        switch (itemData.state)
        {
            case RedeemItemState.InTaskProgress1:
                //执行到第一个子任务
                SetAccumulateCashTaskUI();
                break;
            case RedeemItemState.InTaskProgress2:
                SetSequentialTaskUI();
                break;
            case RedeemItemState.Failed:
                //提现失败，暂时不处理
                break;
        }
    }

    private void SetAccumulateCashTaskUI()
    {
        if (itemData.CurTask.State == (int)TaskState.ONGOING)
        {
            if (itemData.CurTask.IsTaskConditionOK)
            {
                inProgress.gameObject.SetActive(false);
                redeemBtn.gameObject.SetActive(true);
            }
            else
            {
                ShowInProgressUI(itemData.CurTask);
            }
        }
        else if (itemData.CurTask.State == (int)TaskState.CLOSE)
        {
            //暂留接口，暂时无需处理
        }
    }
    
    private void SetSequentialTaskUI()
    {
        if (itemData.SequentialTask.State == (int)TaskState.ONGOING)
        {
            paltformImg.gameObject.SetActive(false);
            UpdateSequentialUI();
            //显示childTask的ui进度显示
            SetSequentialChildTaskUI();
        }
        else if (itemData.SequentialTask.State == (int)TaskState.CLOSE)
        {
            //暂留接口，暂时无需处理
        }
    }

    
    public void UpdateSequentialUI()
    {
        sequentialTaskObj.SetActive(true);
        sequentialTaskProgressTMP.text = string.Format("{0}/{1}", (int)itemData.CurTask.HasCollectNum, (int)itemData.CurTask.TargetNum);
    }
    
    public void SetSequentialChildTaskUI()
    {
        BaseTask childTask = itemData.SequentialTask.GetOnGoingChildTask();
        //显示子任务对应的UI显示
        if (childTask.State == (int)TaskState.ONGOING)
        {
            if (childTask.IsTaskConditionOK)
            {
                //任务完成
            }
            else
            {
                ShowInProgressUI(childTask);
            }
        }else if (childTask.State == (int)TaskState.CLOSE)
        {
            //暂留接口，暂时无需处理
        }
    }
    void SetProgressText(BaseTask task)
    {
        int type = task.TaskType;
        int HasCollectNum = (int)task.HasCollectNum;
        int TargetNum = (int)task.TargetNum;
        if (type == TaskConstants.AccumulateCashTask_Key)
        {
            progressTMP.text = string.Format("{0}/{1}",
                OnLineEarningMgr.Instance.GetMoneyStr(HasCollectNum, needIcon: false),
                OnLineEarningMgr.Instance.GetMoneyStr(TargetNum, needIcon: false));
        }
        else
        {
            progressTMP.text = string.Format("{0}/{1}",HasCollectNum,TargetNum);
        }
        progressBar.fillAmount = HasCollectNum >= TargetNum ? 1.0f : HasCollectNum / (TargetNum * 1.0f);
    }
    
    private void ShowInProgressUI(BaseTask task)
    {
        inProgress.gameObject.SetActive(true);
        redeemBtn.gameObject.SetActive(false);
        condition.gameObject.SetActive(false);
        ItemManager.Instance.GetTaskIcon(task.TaskType, (icon) =>
        {
            taskIconImg.sprite = icon;
        });
        SetProgressText(task);
    }
    
    private void OnButtonClickHandler(GameObject go)
    {
        // WithDrawManager.Instance.CurSelectTaskId = itemData.task.TaskId;
        // WithDrawManager.Instance.ReduceCash((int)itemData.task.TargetNum);
        if (go == redeemBtn.gameObject)
        {
            //点击了领取按钮
            WithDrawManager.Instance.ShowAccountDialog(itemData);
        }else if (go == sequentialTaskBtn.gameObject)
        {
            //点击了信息展示按钮
        }
    }

    private WaitForSecondsRealtime waitOneSceond = new WaitForSecondsRealtime(1);
    
    
    void RecoverToComplete()
    {
        //加钱
        itemData.WithDrawFailed();
        //提现失败，删除当前元素，通知 scrollView 刷新
        // ShowFailedUI();
    }

    public void OnDispose()
    {
        //旧数据解绑prefab
        if (itemData!=null)
        {
            itemData.UnBindUI();
        }
        if (timeCor != null)
        {
            CoroutineUtil.Instance.StopCoroutine(timeCor);
        }
        timeCor = null;
    }

    private void OnDestroy()
    {
        OnDispose();
    }
}