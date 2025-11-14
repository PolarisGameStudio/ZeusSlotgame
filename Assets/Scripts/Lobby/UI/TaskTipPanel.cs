using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Libs;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class TaskTipPanel : MonoBehaviour
{
    public Image background;
    public Sprite[] sliderSprites = new Sprite[2];
    public Sprite[] backgroundSprites = new Sprite[2];
    public Image image_icon;
    private UIText txtInfo;

    private UIText progress;
    private RectTransform progressBar;

    private Image slider;
    private Transform particlePar;
    private ParticleSystem particle1;
    private ParticleSystem particle2;
    public float maxLifeTime = 4f;
    private Image image_head;
    
    private Dictionary<int, int> TaskToBGSpriteIndex = new Dictionary<int, int>
    {
        { TaskConstants.AccumulateCashTask_Key, 1 },
        { TaskConstants.CollectCashFromZeroTask_Key, 1 },
        { TaskConstants.CollectSpinCountTask_Key, 1 },
        { TaskConstants.WatchADTimeTask_Key, 1 },
        { TaskConstants.CollectCardTask_Key, 1 }
    };

    public void RefreshInfo(int taskType)
    {
        if (!TaskTipManager.Instance.isOpen)
        {
            this.gameObject.SetActive(false);
            return;
        }
        txtInfo = transform.Find("txt_info").GetComponent<UIText>();
        progress = transform.Find("slider_progress/txt_progress").GetComponent<UIText>();
        slider = transform.Find("slider_progress/img_bar").GetComponent<Image>();
        particlePar= transform.Find("slider_progress/img_bar/UIParticle");
        particle1= transform.Find("slider_progress/img_bar/UIParticle/lizi").GetComponent<ParticleSystem>();
        particle2= transform.Find("slider_progress/img_bar/UIParticle/lizi1").GetComponent<ParticleSystem>();
        image_head = transform.Find("slider_progress/img_bar/img_head").GetComponent<Image>();
        progressBar = slider.GetComponent<RectTransform>();
        gameObject.SetActive(false);
        //300模式下，只显示金钱的进度
        
        BaseTask task = TaskTipManager.Instance.GetTask(taskType);
        if (task==null)
        {
            // if (TaskTipManager.Instance.CheckShow300CashTip())
            // {
            //     //显示现金的钱
            //     image_icon.sprite = GetSpriteByType(TaskConstants.CollectCashFromZeroTask_Key);
            //     txtInfo.gameObject.SetActive(false);
            //     txtInfo = transform.Find("txt_info1").GetComponent<UIText>();
            //     txtInfo.gameObject.SetActive(true);
            //     txtInfo.SetText(TaskTipManager.Instance.Get300CashStr());
            //     int maxValue = OnLineEarningMgr.Instance.GetMaxValue()* OnLineEarningMgr.Instance.GetCashMultiple();
            //     float f1 = OnLineEarningMgr.Instance.Cash()/(maxValue*1.0f);
            //     string info = string.Format("{0}/{1}",OnLineEarningMgr.Instance.GetMoneyStr((int)OnLineEarningMgr.Instance.Cash(),2,false,true),
            //         OnLineEarningMgr.Instance.GetMoneyStr((int)maxValue,2,false,true));
            //     HandleShowProgressBar(f1,info);
            //     DelayShowSelf();
            //     return;
            // }
            // Debug.LogError("TaskTipPanel task is null, taskType: " + taskType);
            return;
        }

        if (task.IsConditionOK())
        {
            return;
        }
        
        //设置背景
        int index = 1;
        // background.sprite = backgroundSprites[TaskToBGSpriteIndex.TryGetValue(task.TaskType, out int index) ? index : 0];
        background.sprite = backgroundSprites[index];
        //设置图标
        if (task.TaskType != TaskConstants.CollectLoginDaysTask_Key)
        {
            ItemManager.Instance.GetTaskIcon(task.TaskType, (icon) =>
            {
                image_icon.sprite = icon;
            });
        }
        else
        {
            image_icon.gameObject.SetActive(false);
        }
        slider.fillAmount = 0f;
        slider.sprite = sliderSprites[index];
        if (index>0)
        {
            txtInfo.gameObject.SetActive(false);
            txtInfo = transform.Find("txt_info1").GetComponent<UIText>();
            txtInfo.gameObject.SetActive(true);
        }
        progress.SetText("");
        //获取提示文本
        txtInfo.SetText(TaskTipManager.Instance.GetTaskTipText(task));
        float f = task.HasCollectNum/(task.TargetNum * 1f);
        HandleShowProgressBar(f, task.GetProgressDesc());
        DelayShowSelf();
    }
    void HandleShowProgressBar(float f,string info)
    {
        if (particlePar!=null)
        {
            Vector3 endPosition = new Vector3(progressBar.rect.width * f - progressBar.rect.width/2, 0, 0);
            image_head.transform.localPosition = endPosition;
            // 转换坐标并更新
            Vector3 worldPos = progressBar.TransformPoint(endPosition);
            particlePar.position = worldPos;
            float particleLifeTime = Mathf.Max(maxLifeTime * f,0.05f);
            if (particle1 != null)
            {
                ParticleSystem.MainModule main = particle1.main;
                main.startLifetime = particleLifeTime;
            }
            if (particle2 != null)
            {
                ParticleSystem.MainModule main = particle2.main;
                main.startLifetime = particleLifeTime;
            }
        }
        slider.fillAmount = f;
        progress.SetText(info);
    }
    
    private void DelayShowSelf()
    {
        transform.localScale = Vector3.zero;
        new DelayAction(0.7f, null, () =>
        {
            gameObject.SetActive(true);
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }).Play();
    }

    private void HandleOnLine300Show()
    {
        float f1 = OnLineEarningMgr.Instance.Cash()/(OnLineEarningMgr.Instance.GetMaxValue() * 1.0f);
        
    }
}
