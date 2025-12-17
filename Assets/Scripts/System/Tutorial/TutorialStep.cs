using System;
using UnityEngine;

namespace ZeusSlotgame.Tutorial
{
    /// <summary>
    /// 引导步骤配置数据
    /// </summary>
    [Serializable]
    public class TutorialStepConfig
    {
        public TutorialManager.TutorialStep step;
        public string stepName;
        public string description;
        public string targetUIElement;
        public string tutorialText;
        public bool canSkip = false;
        public float delayBeforeShow = 0f;
        
        public TutorialStepConfig(TutorialManager.TutorialStep step, string stepName, string description, 
                                 string targetUIElement, string tutorialText, bool canSkip = false, float delayBeforeShow = 0f)
        {
            this.step = step;
            this.stepName = stepName;
            this.description = description;
            this.targetUIElement = targetUIElement;
            this.tutorialText = tutorialText;
            this.canSkip = canSkip;
            this.delayBeforeShow = delayBeforeShow;
        }
    }
    
    /// <summary>
    /// 引导步骤配置集合
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialConfig", menuName = "ZeusSlotgame/Tutorial Config")]
    public class TutorialConfig : ScriptableObject
    {
        public TutorialStepConfig[] stepConfigs;
        
        public TutorialStepConfig GetConfig(TutorialManager.TutorialStep step)
        {
            foreach (var config in stepConfigs)
            {
                if (config.step == step)
                {
                    return config;
                }
            }
            return null;
        }
        
        public string GetTutorialText(TutorialManager.TutorialStep step)
        {
            var config = GetConfig(step);
            return config?.tutorialText ?? string.Empty;
        }
        
        public string GetTargetUIElement(TutorialManager.TutorialStep step)
        {
            var config = GetConfig(step);
            return config?.targetUIElement ?? string.Empty;
        }
    }
}