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
    public TextMeshProUGUI taskTimeCor;
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
                condition.gameObject.SetActive(false);
                sequentialTaskObj.SetActive(false);
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
            //检测是否所有子任务都完成了
            if (itemData.SequentialChildTask.IsAllChildComplete())
            {
                inProgress.gameObject.SetActive(false);
                redeemBtn.gameObject.SetActive(false);
                condition.gameObject.SetActive(true);
                LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"waittimetips");
                conditionTMP.text = localizedString.GetLocalizedString();
            }
            else
            {
                //显示childTask的ui进度显示
                SetSequentialChildTaskUI();
            }
           
        }
        else if (itemData.SequentialTask.State == (int)TaskState.CLOSE)
        {
            //暂留接口，暂时无需处理
        }
    }
    
    public void UpdateSequentialUI()
    {
        sequentialTaskObj.SetActive(true);
        sequentialTaskProgressTMP.text =itemData.SequentialTask.GetChildInfo();
        //对taskTimeCor进行显隐的判断，只有Task配置了durationTime才能显示
        taskTimeCor.gameObject.SetActive(itemData.SequentialChildTask.DurationTime > 0);
        if (itemData.SequentialChildTask.DurationTime > 0)
        {
            StartTimeCoroutine(taskTimeCor);
        }
    }
    
    public void SetSequentialChildTaskUI()
    {
        BaseTask childTask = itemData.SequentialChildTask.GetOnGoingChildTask();
        //显示子任务对应的UI显示
        if (childTask.State == (int)TaskState.ONGOING)
        {
            if (childTask.IsTaskConditionOK)
            {
                //任务完成
            }
            else
            {
                SetTaskConditionInfo(childTask);
            }
        }else if (childTask.State == (int)TaskState.CLOSE)
        {
            //暂留接口，暂时无需处理
        }
    }

    void SetTaskConditionInfo(BaseTask task)
    {
        inProgress.gameObject.SetActive(false);
        redeemBtn.gameObject.SetActive(false);
        condition.gameObject.SetActive(true);
        conditionTMP.text = "["+itemData.SequentialChildTask.GetChildInfo()+"]"+TaskManager.Instance.GetTaskInfo(task);
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
        taskIconImg.gameObject.SetActive(true);
        inProgress.gameObject.SetActive(true);
        redeemBtn.gameObject.SetActive(false);
        condition.gameObject.SetActive(false);
        sequentialTaskObj.SetActive(false);
        SetProgressText(task);
        //设置任务图标排除任务类型
        if (task.TaskType != TaskConstants.CollectLoginDaysTask_Key)
        {
            ItemManager.Instance.GetTaskIcon(task.TaskType, (icon) =>
            {
                taskIconImg.sprite = icon;
            });
        }
        else
        {
            taskIconImg.gameObject.SetActive(false);
        }
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
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"withdrawprogress");
            Messenger.Broadcast<string>(WithDrawConstants.ShowTipMsg,localizedString.GetLocalizedString());
        }
    }

    private WaitForSecondsRealtime waitOneSceond = new WaitForSecondsRealtime(1);
    
    //开启协程
    private void StartTimeCoroutine(TextMeshProUGUI CountDownText)
    {
        if (timeCor != null)
        {
            CoroutineUtil.Instance.StopCoroutine(timeCor);
        }
        timeCor = CoroutineUtil.Instance.StartCoroutine(Co_UpdateSequentialTime(CountDownText,itemData.SequentialChildTask));
    }
    
    private IEnumerator Co_UpdateSequentialTime(TextMeshProUGUI CountDownText,BaseTask childTask)
    {
        while (!childTask.IsConditionOK())
        {
            long now = TimeUtils.ConvertDateTimeLong(DateTime.Now);
            long endTime = childTask.StartTime+childTask.DurationTime;
            long remainTime = endTime - now;
            if (remainTime <= 0)
            {
                CountDownText.text = "00:00:00";
            }
            else
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(remainTime);
                CountDownText.text = TimeUtils.GetLeftTime_Day_And_HMS(timeSpan);
            }
            yield return waitOneSceond;
        }
        //任务时间到，刷新UI
        childTask.CompleteTask();
    }
    
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