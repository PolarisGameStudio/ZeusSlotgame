using System.Collections.Generic;
using Libs;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Classic
{
    public class TaskTipDialog:UIDialog
    {
        public Image SymbolImage;

        public UIText SymbolCountText;
        public Animator anim;

        public float QuitAniDuration = 0.433f;
        protected override void Awake()
        {
            base.Awake();
            AudioManager.Instance.AsyncPlayMusicAudio("of_kind_dialog",loop:false);
            RefreshInfo();
        }

        public void RefreshInfo()
        {
            BaseTask task = TaskManager.Instance.GetTaskByType(TaskConstants.CollectFreeSpinSymbolCountTask_Key);
            if (task==null)
            {
                Debug.LogError("TaskTipPanel task is null, taskType: " + TaskConstants.CollectFreeSpinSymbolCountTask_Key);
                return;
            }

            string stringEntry = task.GetDesc();
            LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,stringEntry);
            if (localizedString!=null)
            {
                string agr1 = "";
                if (!string.IsNullOrEmpty(task.RewardList))
                {
                    List<BaseAwardItem> awardItems =  RewardManager.Instance.CreateRewardByStr(task.RewardList);
                    if (awardItems.Count > 0)
                    {
                        //只显示第一个奖励
                        BaseAwardItem awardItem = awardItems[0];
                        agr1 = string.Format("<color=#29F706>{0}</color>", awardItem.GetAwardCountDesc());
                    }
                }
                localizedString.Arguments = new object[] {agr1,task.HasCollectNum,task.TargetNum};
                SymbolCountText.SetText(localizedString.GetLocalizedString());
            }
            // if(SymbolImage != null)
            // {
            //     this.SymbolImage.sprite = sprite;
            //     this.SymbolImage.SetNativeSize();
            // }
            //防止SetNativeSize()后，有的symbol尺寸过大，文字覆盖主symbol
            // if (SymbolImage.rectTransform.sizeDelta.x >= 500)
            // {
            //     SymbolImage.rectTransform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            // }
            // else if (SymbolImage.rectTransform.sizeDelta.x >= 300)
            // {
            //     SymbolImage.rectTransform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            // }
            // else
            // {
            //     SymbolImage.rectTransform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            // }
            // if (SymbolCountText != null) this.SymbolCountText.text = count.ToString();

        }

        public override void ShowOut()
        {
            if(anim!=null) anim.SetTrigger("out");
            Libs.DelayAction da = new DelayAction(QuitAniDuration,null, () =>
            {
                base.ShowOut();
            });
            da.Play();
            AudioEntity.Instance.StopMusicAudio("of_kind_dialog");
        }
    }
}