using System.Collections.Generic;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Utils;
namespace Activity
{
    public class ContinueSpinTaskIcon:BaseIcon
    {
        private Button clickButton;
        private TextMeshProUGUI progressTxt;
        private Image progressBar;
        private SkeletonGraphic effect;
        private SkeletonGraphic hand;
        
        ContinueSpinActivity activity;
        
        public override void OnInit(int Id, Dictionary<string, object> data)
        {
            base.OnInit(Id, data);
            
           activity =ActivityManager.Instance.GetActivityByID(activityId) as ContinueSpinActivity;
            
            clickButton = GetComponent<Button>();
            if (clickButton!=null)
            {
                clickButton.onClick.AddListener(OnButtonClick);
            }
            progressTxt = Utilities.RealFindObj<TextMeshProUGUI>(transform,"ProgressTxt");
            progressBar = Utilities.RealFindObj<Image>(transform,"ProgressBar");
            effect =  Utilities.RealFindObj<SkeletonGraphic>(transform,"Effect");
            hand =  Utilities.RealFindObj<SkeletonGraphic>(transform,"Hand");
            
            effect.gameObject.SetActive(false);
            hand.gameObject.SetActive(false);
            
            UpdateProgressTxt();
            RefreshIcon();
            
        }
        private void RefreshIcon()
        {
            clickButton.gameObject.SetActive(ContinueSpinActivity.IsActive);
        }
        private void UpdateProgressTxt()
        {
            activity.UpdateProgress();
        }
        
        private float _lastClickTime = 0f;
        private readonly float _clickCooldown = 0.8f; // 点击冷却时间（秒）

        private void OnButtonClick()
        {
            // 检查是否过了冷却时间
            if (Time.time - _lastClickTime < _clickCooldown)
            {
                return; // 冷却中，不执行逻辑
            }

            _lastClickTime = Time.time; // 更新最后一次点击时间

            if (activity.Task.IsTaskConditionOK)
            {
                StopEffect(hand);
                ActivityManager.Instance.OnClickIcon(activityId);
            }
            else
            {
                /*LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"SpinCountReaches");
                localizedString.Arguments = new object[] { activity.Task.TargetNum - activity.Task.HasCollectNum };
                var info = localizedString.GetLocalizedString();
                Messenger.Broadcast(GameDialogManager.OpenTipsDialogMsg, info);*/
            }
        }
        
        public override void RefreshProgress(float progress,string info=null)
        {
            RefreshIcon();
            progressTxt.text = $"{(int)(progress * 100)}%";
            progressBar.fillAmount = progress;
            if (Mathf.Approximately(progressBar.fillAmount, 1))
            {
                if (!ContinueSpinActivity.IsFirstPop)
                {
                    PlayEffect(effect);
                    //PlayEffect(hand);
                }
            }
            else
            {
                StopEffect(effect);
                //StopEffect(hand);
            }
        }

        private void PlayEffect(SkeletonGraphic  skeletonGraphic)
        {
            if(skeletonGraphic.gameObject.activeSelf) return;
            skeletonGraphic.gameObject.SetActive(true);
            skeletonGraphic.AnimationState.SetAnimation(0, "animation", true);
        }

        private void StopEffect(SkeletonGraphic  skeletonGraphic)
        {
            skeletonGraphic.AnimationState.ClearTrack(0);
            skeletonGraphic.gameObject.SetActive(false);
        }
        
    }
    
    
    
}