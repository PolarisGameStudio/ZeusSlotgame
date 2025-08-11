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
        }

        public void RefreshInfo(Sprite sprite, int taskType)
        {
            BaseTask task = TaskManager.Instance.GetTaskByType(taskType);
            if (task==null)
            {
                Debug.LogError("TaskTipPanel task is null, taskType: " + taskType);
                return;
            }
            if(SymbolImage != null)
            {
                SymbolImage.sprite = sprite;
                this.SymbolImage.SetNativeSize();
                if (taskType == TaskConstants.CollectSymbolCountTask_Key)
                {
                    SymbolImage.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                }
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