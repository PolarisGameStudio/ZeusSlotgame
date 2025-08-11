using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Libs;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class TaskTipPanel : MonoBehaviour
{
    private UIText txtInfo;

    private UIText progress;
    private RectTransform progressBar;

    private Image slider;
    private Transform particlePar;
    private ParticleSystem particle1;
    private ParticleSystem particle2;
    public float maxLifeTime = 4f;
    private Image image_head;
    // Start is called before the first frame update
    void Awake()
    {
        // txtInfo = transform.Find("txt_info").GetComponent<UIText>();
        // progress = transform.Find("slider_progress/txt_progress").GetComponent<UIText>();
        // slider = transform.Find("slider_progress/img_bar").GetComponent<Image>();
        // slider.fillAmount = 0f;
        // progress.SetText("");
    }

    public void RefreshInfo(int taskType)
    {
        gameObject.SetActive(false);
        BaseTask task = TaskManager.Instance.GetTaskByType(taskType);
        if (task==null)
        {
            Debug.LogError("TaskTipPanel task is null, taskType: " + taskType);
            return;
        }

        if (task.IsConditionOK())
        {
            return;
        }
        
        txtInfo = transform.Find("txt_info").GetComponent<UIText>();
        progress = transform.Find("slider_progress/txt_progress").GetComponent<UIText>();
        slider = transform.Find("slider_progress/img_bar").GetComponent<Image>();
        progressBar = slider.GetComponent<RectTransform>();
        slider.fillAmount = 0f;
        progress.SetText("");
        particlePar= transform.Find("slider_progress/img_bar/UIParticle");
        particle1= transform.Find("slider_progress/img_bar/UIParticle/lizi").GetComponent<ParticleSystem>();
        particle2= transform.Find("slider_progress/img_bar/UIParticle/lizi1").GetComponent<ParticleSystem>();
        image_head = transform.Find("slider_progress/img_bar/img_head").GetComponent<Image>();

        string stringEntry = task.GetDesc();
        LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,stringEntry);
        if (localizedString!=null)
        {
            string agr1 = string.Format("<color=#118D1D>{0}</color>", task.TargetNum);
            string agr2 = "";

            if (!string.IsNullOrEmpty(task.RewardList))
            {
                List<BaseAwardItem> awardItems =  RewardManager.Instance.CreateRewardByStr(task.RewardList);
                if (awardItems.Count > 0)
                {
                    //只显示第一个奖励
                    BaseAwardItem awardItem = awardItems[0];
                    agr2 = string.Format("<color=#FF0000>{0}</color>", awardItem.GetAwardCountDesc());
                }
            }
            localizedString.Arguments = new object[] {agr1,agr2};
            txtInfo.SetText(localizedString.GetLocalizedString());
        }

        float f = task.HasCollectNum/(task.TargetNum * 1f);
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
        progress.SetText(task.GetProgressDesc());
        DelayShowSelf();
    }

    private void DelayShowSelf()
    {
        transform.localScale = Vector3.zero;
        new DelayAction(1f, null, () =>
        {
            gameObject.SetActive(true);
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
        }).Play();
    }
}
