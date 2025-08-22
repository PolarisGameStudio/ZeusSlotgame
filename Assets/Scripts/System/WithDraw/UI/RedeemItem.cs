using System;
using System.Collections;
using System.Threading.Tasks;
using CardSystem;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class RedeemItem : MonoBehaviour
{
    public RedeemItemData itemData;
    //序号
    public int index;
    private TextMeshProUGUI cashTMP;
    private Image paltformImg;
    private Button redeemBtn;
    private Button withdrawBtn;

    private RectTransform checking;
    private RectTransform inProgress;
    private RectTransform withDraw;
    private RectTransform moneyFinish;
    private RectTransform condition;
    private TextMeshProUGUI progressTMP;
    private TextMeshProUGUI moneyFinishTMP;
    private Image progressBar;
    private TextMeshProUGUI conditionTMP;
    private TextMeshProUGUI countDownTMP;
    private Coroutine timeCor;
    private TextMeshProUGUI FaildText;
    private TextMeshProUGUI withdrawProgressTmp;
    private void Awake()
    {
        cashTMP = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "cashTMP");
        conditionTMP = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "Condition/taskInfoTMP");
        progressTMP = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "inProgress/bottom/progressTMP");
        paltformImg = Utils.Utilities.RealFindObj<Image>(transform, "platformIMG");
        redeemBtn = Utils.Utilities.RealFindObj<Button>(transform, "moneyFinish/redeemBtn");
        moneyFinishTMP = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "moneyFinish/progressTMP");
        checking = Utils.Utilities.RealFindObj<RectTransform>(transform, "checking");
        inProgress = Utils.Utilities.RealFindObj<RectTransform>(transform, "inProgress");
        withDraw = Utils.Utilities.RealFindObj<RectTransform>(transform, "withDraw");
        moneyFinish = Utils.Utilities.RealFindObj<RectTransform>(transform, "moneyFinish");
        condition = Utils.Utilities.RealFindObj<RectTransform>(transform, "Condition");
        progressBar = Utils.Utilities.RealFindObj<Image>(transform, "inProgress/bottom/checking/progressBar");
        countDownTMP = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "checking/CountDown");
        withdrawProgressTmp = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "withDraw/withdrawProgressTMP");
        withdrawBtn = Utils.Utilities.RealFindObj<Button>(transform, "withDraw/withdrawBtn");

        FaildText= Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "FaildTMP");
        if (redeemBtn != null)
        {
            UGUIEventListener.Get(redeemBtn.gameObject).onClick = OnButtonClickHandler;
        }
        if (withdrawBtn != null)
        {
            UGUIEventListener.Get(withdrawBtn.gameObject).onClick = OnButtonClickHandler;
        }
    }

    private void OnEnable()
    {
        Messenger.AddListener(WithDrawConstants.UpdateRedeemItemState, OnTaskStatusChange);
    }
    
    private void OnDisable()
    {
        Messenger.RemoveListener(WithDrawConstants.UpdateRedeemItemState, OnTaskStatusChange);
    }


    void OnTaskStatusChange()
    {
        //刷新ui显示
        Refresh();
    }
    
    public void UpdateData(int i, RedeemItemData data)
    {
        itemData = data;
        Refresh();
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
        
        int TargetNum = (int)itemData.task.TargetNum;
        cashTMP.text = OnLineEarningMgr.Instance.GetMoneyStr(TargetNum, needIcon: false);
    }

    private RedeemItemState curState;
    public void Refresh()
    {
        Debug.Log($"[RedeemItem][Refresh] itemData.TaskID:{itemData.task.TaskId} itemData.state:{itemData.state}");
        // if (curState == RedeemItemState.Complete&&curState ==itemData.state)
        // {
        //     return;
        // }
        curState = itemData.state;
        switch (itemData.state)
        {
            //进行中未完成
            case RedeemItemState.InProgress:
                ShowInProgressUI();
                break;
            //进行中满足领取条件
            case RedeemItemState.Complete:
                ShowCompleteUI2();
                break;
            //进行中已点击领取按钮
            case RedeemItemState.Wait:
                ShowCountDownUI();
                break;
            //进行中提现失败
            case RedeemItemState.Failed:
                ShowFailedUI();
                break;
            //任务已完成
            case RedeemItemState.Done:
                ShowWithDrawUI();
                break;
        }
    }

    private void ShowInProgressUI()
    {
        moneyFinish.gameObject.SetActive(false);
        checking.gameObject.SetActive(false);
        inProgress.gameObject.SetActive(true);
        withDraw.gameObject.SetActive(false);
        condition.gameObject.SetActive(false);
        FaildText.gameObject.SetActive(false);
        Transform cardIcon = Utils.Utilities.RealFindObj<Transform>(transform, "inProgress/bottom/cardIMG");
        Transform cashIcon = Utils.Utilities.RealFindObj<Transform>(transform, "inProgress/bottom/moneyIMG");
        if (cardIcon!=null)
        {
            cardIcon.gameObject.SetActive(false);
            cashIcon.gameObject.SetActive(true);
        }
        int HasCollectNum = (int)itemData.task.HasCollectNum;
        int TargetNum = (int)itemData.task.TargetNum;
        progressTMP.text = string.Format("{0}/{1}",
            OnLineEarningMgr.Instance.GetMoneyStr((int)itemData.task.HasCollectNum, needIcon: false),
            OnLineEarningMgr.Instance.GetMoneyStr((int)itemData.task.TargetNum, needIcon: false));
        progressBar.fillAmount = HasCollectNum >= TargetNum ? 1.0f : HasCollectNum / (TargetNum * 1.0f);
    }

    private void ShowCompleteUI()
    {
        moneyFinish.gameObject.SetActive(false);
        checking.gameObject.SetActive(false);
        inProgress.gameObject.SetActive(false);
        withDraw.gameObject.SetActive(true);
        condition.gameObject.SetActive(false);
        FaildText.gameObject.SetActive(false);
        //钱的图标换成了卡牌图标
        int HasCollectNum = CardSystemManager.Instance.GetHaveCardTypeCount();
        int TargetNum = CardSystemManager.Instance.GetTotalCardTypeCount();
        withdrawProgressTmp.text = string.Format("{0}/{1}", HasCollectNum, TargetNum);
    }
    
    private void ShowCompleteUI2()
    {
        moneyFinish.gameObject.SetActive(true);
        checking.gameObject.SetActive(false);
        inProgress.gameObject.SetActive(false);
        withDraw.gameObject.SetActive(false);
        condition.gameObject.SetActive(false);
        FaildText.gameObject.SetActive(false);
       
        moneyFinishTMP.text = string.Format("{0}/{1}",
            OnLineEarningMgr.Instance.GetMoneyStr((int)itemData.task.HasCollectNum, needIcon: false),
            OnLineEarningMgr.Instance.GetMoneyStr((int)itemData.task.TargetNum, needIcon: false));
    }

    private void ShowWithDrawUI()
    {
        moneyFinish.gameObject.SetActive(false);
        checking.gameObject.SetActive(false);
        inProgress.gameObject.SetActive(false);
        withDraw.gameObject.SetActive(true);
        condition.gameObject.SetActive(false);
        FaildText.gameObject.SetActive(false);
    }

    private void ShowCountDownUI()
    {
        checking.gameObject.SetActive(true);
        inProgress.gameObject.SetActive(false);
        withDraw.gameObject.SetActive(false);
        condition.gameObject.SetActive(false);
        moneyFinish.gameObject.SetActive(false);
        FaildText.gameObject.SetActive(false);
        if (timeCor!=null)
        {
            CoroutineUtil.Instance.StopCoroutine(timeCor);
            timeCor = null;
        }
        long endTime = itemData.task.CanRewardTime;
        timeCor = CoroutineUtil.Instance.StartCoroutine(ShowCountDownText(endTime));
    }

    private void ShowFailedUI()
    {
        //先隐藏自身
        Debug.Log("RedeemItem ShowFailedUI");
        // this.gameObject.SetActive(false);
        Messenger.Broadcast(WithDrawConstants.UpdateRedeemItemMsg);
    }
    
    private void ShowConditionUI()
    {
        checking.gameObject.SetActive(false);
        inProgress.gameObject.SetActive(false);
        withDraw.gameObject.SetActive(false);
        condition.gameObject.SetActive(true);
    }

    private void OnButtonClickHandler(GameObject go)
    {
        // WithDrawManager.Instance.CurSelectTaskId = itemData.task.TaskId;
        // WithDrawManager.Instance.ReduceCash((int)itemData.task.TargetNum);
        if (go == redeemBtn.gameObject)
        {
            //点击了领取按钮
            WithDrawManager.Instance.ShowAccountDialog(itemData.task.TaskId,(int)itemData.task.TargetNum);
        }
        else if (go == withdrawBtn.gameObject)
        {
            //点击了提现按钮,给出提示消息
            Messenger.Broadcast(WithDrawConstants.ShowTipMsg);
        }
    }

    private WaitForSecondsRealtime waitOneSceond = new WaitForSecondsRealtime(1);
    IEnumerator ShowCountDownText(long endTime)
    {
        long nowTime = TimeUtils.ConvertDateTimeLong(DateTime.Now);
        while (itemData.state == RedeemItemState.Wait && nowTime < endTime)
        {
            try
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(endTime - nowTime);
                countDownTMP.text = TimeUtils.GetLeftTime_Day_And_HMS(timeSpan);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            yield return waitOneSceond;
            nowTime++;
        }
        if (timeCor!=null)
        {
            CoroutineUtil.Instance.StopCoroutine(timeCor);
        }
        timeCor = null;
        RecoverToComplete();
    }
    
    void RecoverToComplete()
    {
        //加钱
        itemData.WithDrawFailed();
        //提现失败，删除当前元素，通知 scrollView 刷新
        ShowFailedUI();
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