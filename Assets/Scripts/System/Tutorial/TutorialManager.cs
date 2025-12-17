using System.Collections.Generic;
using UnityEngine;
using System;

namespace ZeusSlotgame.Tutorial
{
    /// <summary>
    /// 新手引导管理器
    /// 负责控制引导流程和状态管理
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        private static TutorialManager _instance;
        public static TutorialManager Instance => _instance;
        
        /// <summary>
        /// 引导步骤枚举
        /// </summary>
        public enum TutorialStep
        {
            None = 0,
            FirstSpin = 1,
            WithDrawButton = 2,
            WithDrawItem = 3,
            LuckyGift = 4,
            AutoSpin = 5
        }
        
        [Header("UI配置")]
        [SerializeField] private GameObject tutorialUIPrefab;
        
        [Header("调试设置")]
        [SerializeField] private bool enableTutorials = true;
        [SerializeField] private bool debugMode = false;
        
        private TutorialStep currentStep = TutorialStep.None;
        private Dictionary<TutorialStep, bool> completedSteps = new Dictionary<TutorialStep, bool>();
        private TutorialUI tutorialUI;
        
        private void Awake()
        {
            InitializeSingleton();
            InitializeTutorialData();
        }
        
        private void InitializeSingleton()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void InitializeTutorialData()
        {
            foreach (TutorialStep step in Enum.GetValues(typeof(TutorialStep)))
            {
                if (step != TutorialStep.None)
                {
                    completedSteps[step] = PlayerPrefs.GetInt($"Tutorial_{step}", 0) == 1;
                }
            }
            
            if (debugMode)
            {
                Debug.Log("[TutorialManager] 引导数据初始化完成");
            }
        }
        
        /// <summary>
        /// 开始指定引导步骤
        /// </summary>
        public void StartTutorial(TutorialStep step)
        {
            if (!enableTutorials) return;
            if (currentStep != TutorialStep.None) return;
            
            currentStep = step;
            
            if (tutorialUI == null && tutorialUIPrefab != null)
            {
                GameObject uiInstance = Instantiate(tutorialUIPrefab);
                tutorialUI = uiInstance.GetComponent<TutorialUI>();
                if (tutorialUI != null)
                {
                    DontDestroyOnLoad(uiInstance);
                }
                else
                {
                    Debug.LogError("[TutorialManager] TutorialUI组件未找到");
                    return;
                }
            }
            
            if (tutorialUI != null)
            {
                tutorialUI.SetupTutorial(step);
            }
            
            if (debugMode)
            {
                Debug.Log($"[TutorialManager] 开始引导步骤: {step}");
            }
        }
        
        /// <summary>
        /// 完成当前引导步骤
        /// </summary>
        public void CompleteCurrentTutorial()
        {
            if (currentStep != TutorialStep.None)
            {
                completedSteps[currentStep] = true;
                PlayerPrefs.SetInt($"Tutorial_{currentStep}", 1);
                PlayerPrefs.Save();
                
                if (tutorialUI != null)
                {
                    tutorialUI.Hide();
                }
                
                if (debugMode)
                {
                    Debug.Log($"[TutorialManager] 完成引导步骤: {currentStep}");
                }
                
                currentStep = TutorialStep.None;
            }
        }
        
        /// <summary>
        /// 检查是否需要显示指定引导步骤
        /// </summary>
        public bool ShouldShowStep(TutorialStep step)
        {
            return !completedSteps.ContainsKey(step) || !completedSteps[step];
        }
        
        /// <summary>
        /// 获取当前引导步骤
        /// </summary>
        public TutorialStep GetCurrentStep() => currentStep;
        
        /// <summary>
        /// 是否正在显示引导
        /// </summary>
        public bool IsShowingTutorial() => currentStep != TutorialStep.None;
        
        /// <summary>
        /// 重置所有引导状态
        /// </summary>
        public void ResetAllTutorials()
        {
            foreach (TutorialStep step in Enum.GetValues(typeof(TutorialStep)))
            {
                if (step != TutorialStep.None)
                {
                    PlayerPrefs.DeleteKey($"Tutorial_{step}");
                    completedSteps[step] = false;
                }
            }
            PlayerPrefs.Save();
            
            if (debugMode)
            {
                Debug.Log("[TutorialManager] 所有引导状态已重置");
            }
        }
        
        // 静态快捷方法
        public static bool ShouldShow(TutorialStep step) => Instance?.ShouldShowStep(step) ?? false;
        public static void Start(TutorialStep step) => Instance?.StartTutorial(step);
        public static void Complete() => Instance?.CompleteCurrentTutorial();
        public static bool IsShowing() => Instance?.IsShowingTutorial() ?? false;
    }
}