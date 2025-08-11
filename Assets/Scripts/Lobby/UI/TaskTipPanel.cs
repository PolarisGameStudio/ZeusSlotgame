using System.Collections;
using System.Collections.Generic;
using Libs;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class TaskTipPanel : MonoBehaviour
{
    private UIText txtInfo;

    private UIText progress;
    
    private Image slider;
    
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
        txtInfo = transform.Find("txt_info").GetComponent<UIText>();
        progress = transform.Find("slider_progress/txt_progress").GetComponent<UIText>();
        slider = transform.Find("slider_progress/img_bar").GetComponent<Image>();
        slider.fillAmount = 0f;
        progress.SetText("");
        BaseTask task = TaskManager.Instance.GetTaskByType(taskType);
        if (task==null)
        {
           Debug.LogError("TaskTipPanel task is null, taskType: " + taskType);
           return;
        }

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
            txtInfo.SetText(localizedString.GetLocalizedString()); ;
        }

        float progressValue = task.HasCollectNum/(task.TargetNum * 1f);
        slider.fillAmount = progressValue;
        progress.SetText(task.GetProgressDesc());
    }
}
