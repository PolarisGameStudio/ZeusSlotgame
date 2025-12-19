# Tutorial 新手引导系统

## 概述

Tutorial模块是ZeusSlotgame游戏的新手引导系统，采用Prefab预制体架构，提供灵活可扩展的分步骤游戏功能引导，帮助新玩家快速熟悉游戏操作。

## 核心架构

本系统采用模块化设计，由以下核心组件构成：

### 1. TutorialManager（引导管理器）
- **职责**：流程控制、状态管理、Prefab加载
- **类型**：MonoSingleton单例
- **关键功能**：
  - 异步加载引导Prefab
  - 管理引导生命周期
  - 持久化完成状态（PlayerPrefs）
  - 防止重复显示引导

### 2. TutorialOverlay（覆盖层组件）
- **职责**：UI交互、生命周期管理、动画播放
- **挂载位置**：每个Tutorial_XXX Prefab的根节点
- **关键功能**：
  - 淡入淡出动画
  - 点击关闭处理
  - 完成回调执行
  - 动画元素管理

### 3. TutorialPrefabConfig（配置资产）
- **职责**：步骤与Prefab映射管理
- **类型**：ScriptableObject
- **关键功能**：
  - 配置每个步骤的Prefab路径
  - 设置交互行为（点击关闭、自动关闭）
  - 编辑器可视化配置

### 4. TutorialAnimations（动画工具）
- **职责**：提供公用动画效果
- **类型**：静态工具类
- **支持动画**：
  - 手指指示器浮动
  - 高亮边框呼吸
  - 圆形高亮呼吸
  - 序列帧播放
  - 淡入淡出效果

## 引导步骤

| 步骤 | 枚举值 | 描述 | 典型用途 |
|------|--------|------|----------|
| None | 0 | 无引导状态 | 初始状态/已完成 |
| FirstSpin | 1 | 首次Spin引导 | 引导玩家进行第一次旋转 |
| WithDrawButton | 2 | 提现按钮引导 | 引导玩家找到提现功能 |
| WithDrawItem | 3 | 提现档位引导 | 引导玩家选择提现档位 |
| LuckyGift | 4 | LuckyGift引导 | 引导玩家了解礼物系统 |
| AutoSpin | 5 | Auto按钮引导 | 引导玩家使用自动旋转 |

## 文件结构

```
Assets/Scripts/System/Tutorial/
├── TutorialManager.cs          # 引导管理器核心类（单例）
├── TutorialOverlay.cs          # 覆盖层组件（挂载到Prefab）
├── TutorialPrefabConfig.cs     # ScriptableObject配置资产
├── TutorialAnimations.cs       # 公用动画工具类
└── README.md                   # 说明文档（本文件）

Assets/AssetResources/Tutorial/Prefab/
├── Tutorial_FirstSpin.prefab   # 首次Spin引导Prefab
├── Tutorial_WithDrawButton.prefab
├── Tutorial_WithDrawItem.prefab
├── Tutorial_LuckyGift.prefab
└── Tutorial_AutoSpin.prefab

Assets/Resources/Tutorial/
└── TutorialPrefabConfig.asset  # 配置资产实例
```

## 快速开始

### 1. 创建配置资产

在Unity编辑器中创建配置资产：

1. 右键点击Project视图 → `Create` → `Tutorial` → `Prefab Config`
2. 命名为 `TutorialPrefabConfig`
3. 在Inspector中配置各步骤：

```
Step Prefabs (5 项):
  [0]
    Step: FirstSpin
    Prefab Path: "Tutorial/Prefab/Tutorial_FirstSpin"
    Display Name: "首次旋转引导"
    Can Click To Close: ✓
    Auto Close Delay: 0
  [1]
    Step: WithDrawButton
    Prefab Path: "Tutorial/Prefab/Tutorial_WithDrawButton"
    Display Name: "提现按钮引导"
    Can Click To Close: ✓
    Auto Close Delay: 0
  ...
```

### 2. 设置TutorialManager

在场景中配置TutorialManager：

```csharp
// 在主场景的初始化脚本中
public class GameSceneInit : MonoBehaviour
{
    private void Start()
    {
        // TutorialManager是单例，自动初始化
        // 在需要时启动引导
        if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.FirstSpin))
        {
            TutorialManager.Start(
                TutorialManager.TutorialStep.FirstSpin,
                parentNode: null,  // 使用默认DialogCanvas
                onComplete: (step) =>
                {
                    Debug.Log($"引导完成: {step}");
                    // 执行后续逻辑...
                }
            );
        }
    }
}
```

### 3. 创建引导Prefab

创建 Tutorial_FirstSpin.prefab 示例结构：

```
Tutorial_FirstSpin (GameObject)
├── [TutorialOverlay] (Component)        // 必须挂载此组件
├── CanvasGroup (Component)              // 必须配置
├── Background (Image)                   // 半透明黑色背景
│   └── ClickArea (Button)               // 全屏点击区域（可选）
├── Highlight (GameObject)
│   ├── UI_Highlight_Border (Image)      // 矩形高亮边框
│   └── UI_Highlight_Circle (Image)      // 圆形高亮边框
├── UI_Hand_Pointer (Image)              // 手指指示器
├── TutorialText (TextMeshPro)           // 引导文字
└── SequenceAnimation (Image)            // 序列帧动画（可选）
```

TutorialOverlay组件配置：

```
[必须引用]
  Canvas Group: → CanvasGroup组件

[高亮元素（至少一个）]
  Highlight Border: → UI_Highlight_Border
  Highlight Circle: → UI_Highlight_Circle

[序列帧动画（可选）]
  Sequence Image: → SequenceAnimation
  Sequence Frames: [Sprite数组]
  Sequence Frame Rate: 24

[指示器]
  Hand Pointer: → UI_Hand_Pointer

[点击区域（可选）]
  Click Area: → ClickArea Button

[动画设置]
  Fade Duration: 0.3
```

## 核心API

### TutorialManager API

#### 静态快捷方法

```csharp
// 开始指定引导步骤
TutorialManager.Start(
    TutorialManager.TutorialStep.FirstSpin,
    parentNode: null,           // 父节点，null则使用DialogCanvas
    onComplete: (step) => {     // 完成回调（可选）
        Debug.Log($"完成引导: {step}");
    }
);

// 检查是否需要显示引导
bool shouldShow = TutorialManager.ShouldShow(TutorialManager.TutorialStep.FirstSpin);

// 检查是否正在显示引导
bool isShowing = TutorialManager.IsShowing();

// 完成当前引导（通常不需要手动调用，点击时自动触发）
TutorialManager.Complete();
```

#### 实例方法

```csharp
var manager = TutorialManager.Instance;

// 开始引导
manager.StartTutorial(
    TutorialManager.TutorialStep.FirstSpin,
    parentNode: null,
    onComplete: (step) => { /* ... */ }
);

// 完成指定引导（带验证）
manager.CompleteSpecificTutorial(
    TutorialManager.TutorialStep.FirstSpin,
    overlay  // TutorialOverlay实例
);

// 获取当前步骤
TutorialManager.TutorialStep current = manager.GetCurrentStep();

// 重置所有引导状态（调试用）
manager.ResetAllTutorials();
```

### TutorialOverlay API

```csharp
// 初始化（由TutorialManager自动调用）
overlay.Initialize(
    step: TutorialManager.TutorialStep.FirstSpin,
    allowClickToClose: true,
    onComplete: (step) => { /* 完成回调 */ }
);

// 显示引导
overlay.Show();

// 隐藏引导
overlay.Hide(onComplete: () => {
    Debug.Log("隐藏完成");
});
```

### TutorialAnimations API

```csharp
// 启动手指浮动动画
Tweener tweener = TutorialAnimations.StartHandPointerAnimation(handPointerImage);

// 启动高亮边框呼吸动画
Tweener borderTweener = TutorialAnimations.StartHighlightBorderAnimation(highlightImage);

// 启动圆形高亮呼吸动画
Tweener circleTweener = TutorialAnimations.StartHighlightCircleAnimation(circleImage);

// 启动序列帧动画
SequenceAnimator sequenceAnimator = TutorialAnimations.StartSequenceAnimation(
    targetImage: sequenceImage,
    frames: spriteArray,
    frameRate: 24f
);

// 淡入动画
TutorialAnimations.FadeIn(canvasGroup, duration: 0.3f, onComplete: () => {});

// 淡出动画
TutorialAnimations.FadeOut(canvasGroup, duration: 0.3f, onComplete: () => {});

// 停止动画
TutorialAnimations.StopAnimation(tweener);
TutorialAnimations.StopSequenceAnimation(sequenceAnimator);
```

## 集成示例

### 示例1: 首次Spin引导

```csharp
// 在SpinButtonStyle.cs或类似脚本中
public class SpinButtonController : MonoBehaviour
{
    private void Start()
    {
        // 游戏启动时检查是否需要显示首次Spin引导
        if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.FirstSpin))
        {
            // 延迟显示，等待UI初始化完成
            StartCoroutine(ShowFirstSpinTutorial());
        }
    }

    private IEnumerator ShowFirstSpinTutorial()
    {
        yield return new WaitForSeconds(0.5f);

        TutorialManager.Start(
            TutorialManager.TutorialStep.FirstSpin,
            onComplete: (step) =>
            {
                Debug.Log("玩家已完成首次Spin引导");
                // 可选：自动执行一次Spin
            }
        );
    }

    private void OnSpinButtonClick()
    {
        // 如果当前正在显示FirstSpin引导，点击Spin按钮会自动完成引导
        // 不需要手动调用TutorialManager.Complete()

        // 执行旋转逻辑...
    }
}
```

### 示例2: 提现按钮引导

```csharp
// 在RewardCashDialog.cs中
public class RewardCashDialog : MonoBehaviour
{
    protected override void Close()
    {
        base.Close();

        // 对话框关闭后，显示提现按钮引导
        if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.WithDrawButton))
        {
            StartCoroutine(ShowWithDrawTutorialDelayed());
        }
    }

    private IEnumerator ShowWithDrawTutorialDelayed()
    {
        // 等待对话框完全关闭
        yield return new WaitForSeconds(0.3f);

        TutorialManager.Start(
            TutorialManager.TutorialStep.WithDrawButton,
            onComplete: (step) =>
            {
                Debug.Log("玩家已了解提现按钮位置");
            }
        );
    }
}
```

### 示例3: 提现档位引导

```csharp
// 在WithDrawDialog.cs中
public class WithDrawDialog : MonoBehaviour
{
    protected override void OnEnable()
    {
        base.OnEnable();

        // 对话框打开后，引导玩家选择提现档位
        if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.WithDrawItem))
        {
            StartCoroutine(ShowWithDrawItemTutorial());
        }
    }

    private IEnumerator ShowWithDrawItemTutorial()
    {
        // 等待UI元素加载完成
        yield return null;

        TutorialManager.Start(
            TutorialManager.TutorialStep.WithDrawItem,
            parentNode: transform,  // 指定父节点为WithDrawDialog
            onComplete: (step) =>
            {
                Debug.Log("玩家已了解如何选择提现档位");
            }
        );
    }
}
```

### 示例4: LuckyGift引导（带暂停Auto Spin）

```csharp
// 在LuckyGiftActivity.cs中
public class LuckyGiftActivity : MonoBehaviour
{
    private void CreateItem()
    {
        // 创建LuckyGift图标...

        // 如果需要显示引导，暂停Auto Spin
        if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.LuckyGift))
        {
            // 暂停自动旋转
            Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_SUSPEND);

            TutorialManager.Start(
                TutorialManager.TutorialStep.LuckyGift,
                onComplete: (step) =>
                {
                    Debug.Log("玩家已了解LuckyGift功能");
                    // 可选：恢复自动旋转
                }
            );
        }
    }
}
```

### 示例5: 自定义父节点和回调

```csharp
// 在自定义功能中使用引导
public class CustomFeature : MonoBehaviour
{
    [SerializeField] private Transform customCanvas;

    private void ShowCustomTutorial()
    {
        TutorialManager.Start(
            TutorialManager.TutorialStep.AutoSpin,
            parentNode: customCanvas,  // 使用自定义Canvas
            onComplete: (step) =>
            {
                // 引导完成后的自定义逻辑
                Debug.Log($"引导 {step} 完成");

                // 触发下一个引导
                if (step == TutorialManager.TutorialStep.AutoSpin)
                {
                    // 可以继续触发其他功能...
                }
            }
        );
    }
}
```

## 动画系统

### 支持的动画效果

#### 1. 手指指示器浮动

```csharp
// 自动循环的上下浮动动画
HandPointerSettings:
  - floatDistance: 15f   // 浮动距离（像素）
  - floatDuration: 0.8f  // 浮动周期（秒）
  - floatEase: InOutSine // 缓动曲线
```

#### 2. 高亮边框呼吸

```csharp
// 透明度呼吸效果
HighlightSettings:
  - pulseDuration: 1f    // 呼吸周期（秒）
  - minAlpha: 0.5f       // 最小透明度
  - maxAlpha: 1f         // 最大透明度
  - pulseEase: InOutSine // 缓动曲线
```

#### 3. 序列帧动画

```csharp
// 帧动画播放
SequenceSettings:
  - defaultFrameRate: 24f  // 默认帧率

使用方法：
1. 在TutorialOverlay中配置sequenceImage和sequenceFrames
2. 设置frameRate（默认24fps）
3. 自动循环播放
```

#### 4. 淡入淡出

```csharp
// 整体Canvas Group淡入淡出
Fade Settings:
  - Duration: 0.3f       // 默认淡入淡出时长
  - Ease: OutQuad (淡入) / InQuad (淡出)
```

### 自定义动画参数

可以修改 TutorialAnimations 中的静态配置类：

```csharp
// 自定义手指浮动效果
TutorialAnimations.HandPointerSettings.floatDistance = 20f;
TutorialAnimations.HandPointerSettings.floatDuration = 1f;

// 自定义高亮呼吸效果
TutorialAnimations.HighlightSettings.pulseDuration = 1.5f;
TutorialAnimations.HighlightSettings.minAlpha = 0.3f;
```

## 配置系统详解

### TutorialPrefabConfig 配置参数

```csharp
[System.Serializable]
public class StepPrefab
{
    // 基础配置
    public TutorialStep step;           // 引导步骤枚举
    public string prefabPath;           // AssetResources相对路径
    public string displayName;          // 显示名称（调试用）

    // 交互配置
    public bool canClickToClose;        // 是否允许点击关闭
    public float autoCloseDelay;        // 自动关闭延迟（0=不自动关闭）
}
```

### 配置示例

```
FirstSpin 配置:
  Step: FirstSpin
  Prefab Path: "Tutorial/Prefab/Tutorial_FirstSpin"
  Display Name: "首次旋转引导"
  Can Click To Close: true          // 允许玩家点击关闭
  Auto Close Delay: 0               // 不自动关闭

LuckyGift 配置:
  Step: LuckyGift
  Prefab Path: "Tutorial/Prefab/Tutorial_LuckyGift"
  Display Name: "礼物功能引导"
  Can Click To Close: true
  Auto Close Delay: 3               // 3秒后自动关闭
```

### 验证配置

在Inspector中右键点击配置资产，选择 "验证配置"：

```csharp
// 验证所有步骤的Prefab路径是否配置完整
bool isValid = tutorialPrefabConfig.ValidateConfig();
```

## 状态管理

### 持久化存储

引导完成状态使用 PlayerPrefs 存储：

```csharp
// 存储键格式
Tutorial_FirstSpin = 1      // 已完成
Tutorial_WithDrawButton = 1 // 已完成
Tutorial_WithDrawItem = 0   // 未完成
...

// 代码示例
PlayerPrefs.SetInt($"Tutorial_{step}", 1);  // 标记完成
bool completed = PlayerPrefs.GetInt($"Tutorial_{step}", 0) == 1;
```

### 重置引导状态

```csharp
// 重置所有引导（开发调试用）
TutorialManager.Instance.ResetAllTutorials();

// 重置特定引导
PlayerPrefs.DeleteKey("Tutorial_FirstSpin");
PlayerPrefs.Save();
```

### 状态查询

```csharp
// 检查引导是否完成
bool isCompleted = !TutorialManager.ShouldShow(TutorialManager.TutorialStep.FirstSpin);

// 获取当前正在显示的引导
TutorialManager.TutorialStep current = TutorialManager.Instance.GetCurrentStep();

// 检查是否有引导正在显示
bool isShowing = TutorialManager.IsShowing();
```

## 调试和测试

### 调试模式

在TutorialManager的Inspector中启用调试：

```
[调试设置]
  Enable Tutorials: ✓           // 引导功能总开关
  Debug Mode: ✓                 // 详细日志输出
```

调试日志示例：

```
[TutorialManager] 引导数据初始化完成
[TutorialManager] StartTutorial被调用 - Step: FirstSpin
[TutorialManager] 开始异步加载Prefab: Tutorial/Prefab/Tutorial_FirstSpin
[TutorialManager] Prefab加载成功: Tutorial_FirstSpin
[TutorialOverlay] 初始化完成 - Step: FirstSpin
[TutorialAnimations] 手指浮动动画已启动
```

### 测试命令

在控制台或测试脚本中使用：

```csharp
// 重置所有引导，重新测试
TutorialManager.Instance.ResetAllTutorials();

// 强制显示特定引导（即使已完成）
PlayerPrefs.DeleteKey("Tutorial_FirstSpin");
TutorialManager.Start(TutorialManager.TutorialStep.FirstSpin);

// 查看当前状态
Debug.Log($"当前步骤: {TutorialManager.Instance.GetCurrentStep()}");
Debug.Log($"是否正在显示: {TutorialManager.IsShowing()}");
```

### 编辑器工具

TutorialOverlay 提供编辑器验证工具：

```csharp
// 在Inspector中右键点击TutorialOverlay组件
// 选择 "验证组件引用"

验证输出示例:
=== TutorialOverlay 组件引用检查 ===
✅ CanvasGroup 已设置
✅ 高亮元素已配置: Border=True, Circle=False, Sequence=False
✅ handPointer 已设置
✅ clickArea 已设置
```

### 常见问题排查

#### 1. 引导不显示

```csharp
// 检查1: 引导功能是否启用
Debug.Log(TutorialManager.Instance.enableTutorials);

// 检查2: 引导是否已完成
Debug.Log(TutorialManager.ShouldShow(TutorialManager.TutorialStep.FirstSpin));

// 检查3: 配置是否正确
Debug.Log(TutorialManager.Instance.prefabConfig != null);

// 检查4: 是否有其他引导正在显示
Debug.Log(TutorialManager.Instance.GetCurrentStep());
```

#### 2. Prefab加载失败

```csharp
// 检查路径配置
TutorialPrefabConfig.StepPrefab config =
    tutorialPrefabConfig.GetStepConfig(TutorialManager.TutorialStep.FirstSpin);
Debug.Log($"Prefab路径: {config.prefabPath}");

// 验证配置完整性
tutorialPrefabConfig.ValidateConfig();
```

#### 3. 动画不播放

```csharp
// 在TutorialOverlay中右键选择"验证组件引用"
// 确保所有必要的引用都已设置
```

## 性能优化

### 资源加载

- 使用异步加载避免卡顿
- 引导Prefab按需加载
- 完成后自动销毁释放内存

```csharp
// 异步加载示例（TutorialManager内部实现）
yield return ResourceLoadManager.Instance.AsyncLoadResource<GameObject>(
    prefabPath,
    onSuccess: (name, prefab) => { /* 实例化 */ },
    onProgress: (progress) => { /* 进度回调 */ },
    onFail: (error) => { /* 错误处理 */ }
);
```

### 内存管理

- TutorialOverlay 在 OnDestroy 中清理所有动画
- DOTween动画使用 Kill() 正确释放
- SequenceAnimator 自动销毁GameObject

```csharp
// TutorialOverlay.OnDestroy 自动清理
private void OnDestroy()
{
    StopAllAnimations();
    clickArea?.onClick.RemoveListener(OnClickArea);
}
```

### 最佳实践

1. **单次显示原则**: 同一时间只显示一个引导
2. **状态验证**: 使用 CompleteSpecificTutorial 防止状态不一致
3. **延迟显示**: 等待UI初始化完成后再显示引导
4. **回调清理**: 完成回调中避免长时间操作

## 扩展开发

### 添加新的引导步骤

#### 步骤1: 添加枚举

```csharp
// TutorialManager.cs
public enum TutorialStep
{
    None = 0,
    FirstSpin = 1,
    WithDrawButton = 2,
    WithDrawItem = 3,
    LuckyGift = 4,
    AutoSpin = 5,
    NewFeature = 6  // 新增步骤
}
```

#### 步骤2: 创建Prefab

创建 `Tutorial_NewFeature.prefab`，按照标准结构配置所有组件。

#### 步骤3: 配置路径

在 TutorialPrefabConfig 资产中添加配置：

```
Step Prefabs (6 项):
  [5]
    Step: NewFeature
    Prefab Path: "Tutorial/Prefab/Tutorial_NewFeature"
    Display Name: "新功能引导"
    Can Click To Close: true
    Auto Close Delay: 0
```

#### 步骤4: 触发引导

```csharp
// 在合适的时机触发
if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.NewFeature))
{
    TutorialManager.Start(TutorialManager.TutorialStep.NewFeature);
}
```

### 自定义动画效果

#### 扩展 TutorialAnimations

```csharp
// 添加新的动画效果
public static class TutorialAnimations
{
    // 新增：缩放脉冲动画
    public static Tweener StartScalePulseAnimation(Transform target)
    {
        return target.DOScale(1.1f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
```

#### 在 TutorialOverlay 中使用

```csharp
public class TutorialOverlay : MonoBehaviour
{
    [SerializeField] private Transform customElement;
    private Tweener customTweener;

    private void OnShowComplete()
    {
        // 使用自定义动画
        if (customElement != null)
        {
            customTweener = TutorialAnimations.StartScalePulseAnimation(customElement);
        }
    }

    private void StopAllAnimations()
    {
        TutorialAnimations.StopAnimation(customTweener);
    }
}
```

## 注意事项

### 重要约束

1. **单例模式**: TutorialManager 是 MonoSingleton，全局唯一
2. **状态阻塞**: 同一时间只能显示一个引导（currentStep != None时阻塞）
3. **异步加载**: Prefab 加载是异步的，需要等待加载完成
4. **引用验证**: CompleteSpecificTutorial 会验证 step 和 overlay 实例

### 常见陷阱

#### 1. 重复触发

```csharp
// 错误：没有检查是否需要显示
TutorialManager.Start(TutorialManager.TutorialStep.FirstSpin);

// 正确：先检查
if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.FirstSpin))
{
    TutorialManager.Start(TutorialManager.TutorialStep.FirstSpin);
}
```

#### 2. 父节点错误

```csharp
// 错误：父节点在引导显示前被销毁
TutorialManager.Start(step, parentNode: temporaryCanvas);

// 正确：使用持久化的Canvas或传null
TutorialManager.Start(step, parentNode: null);  // 使用DialogCanvas
```

#### 3. 回调时序

```csharp
// 注意：onComplete 在 CompleteSpecificTutorial 之前执行
TutorialManager.Start(step, onComplete: (s) =>
{
    // 此时引导尚未销毁，IsShowing() 仍为 true
    // 如果需要在销毁后执行操作，使用协程延迟
    StartCoroutine(DelayedAction());
});
```

### 最佳实践建议

1. **延迟显示**: 使用 `yield return null` 或 `WaitForSeconds` 等待UI初始化
2. **路径配置**: Prefab路径使用相对于 AssetResources 的路径
3. **组件引用**: 使用编辑器验证工具检查 TutorialOverlay 配置
4. **测试覆盖**: 重置引导状态后测试所有步骤
5. **日志记录**: 开发阶段启用 Debug Mode 记录详细日志

## 版本历史

### v2.0.0 (2024-12-19)
- 重构为Prefab架构
- 新增 TutorialPrefabConfig 配置系统
- 新增 TutorialAnimations 公用动画工具
- 新增序列帧动画支持
- 改进异步加载机制
- 优化状态管理和验证逻辑
- 完善调试工具和错误处理

### v1.0.0 (2024-12-17)
- 初始版本发布
- 支持5个核心引导步骤
- 基础UI和状态管理

## 技术支持

遇到问题时：

1. 启用 Debug Mode 查看详细日志
2. 使用编辑器验证工具检查配置
3. 查看代码注释中的详细说明
4. 重置引导状态重新测试
5. 联系开发团队获取支持

---

文档最后更新时间: 2024-12-19
