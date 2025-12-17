# Tutorial 新手引导系统

## 概述

Tutorial模块是ZeusSlotgame游戏的新手引导系统，提供分步骤的游戏功能引导，帮助新玩家快速熟悉游戏操作。

## 功能特性

- 🎯 **分步骤引导**: 5个核心引导步骤，覆盖主要游戏功能
- 🎨 **可视化界面**: 半透明遮罩 + 高亮效果 + 动态文本
- 💾 **状态持久化**: 使用PlayerPrefs保存引导完成状态
- 🔧 **可配置性**: ScriptableObject配置系统，支持编辑器配置
- 🌐 **多语言支持**: 内置本地化接口，支持多语言文本
- 🐛 **调试支持**: 调试模式和测试工具

## 引导步骤

| 步骤 | 名称 | 描述 | 目标UI元素 |
|------|------|------|------------|
| 1 | FirstSpin | 首次Spin引导 | SpinButton |
| 2 | WithDrawButton | 提现按钮引导 | WithDrawButton |
| 3 | WithDrawItem | 提现档位引导 | RedeemItem_0 |
| 4 | LuckyGift | LuckyGift引导 | LuckyGiftIcon |
| 5 | AutoSpin | Auto按钮引导 | AutoButton |

## 文件结构

```
Assets/Scripts/System/Tutorial/
├── TutorialManager.cs      # 引导管理器核心类
├── TutorialUI.cs           # 引导UI控制器
├── TutorialStep.cs         # 引导步骤配置
└── README.md              # 说明文档
```

## 快速开始

### 1. 安装预制体

创建TutorialOverlay预制体，包含以下组件：

- `Canvas Group` - 控制整体透明度
- `Background Image` - 半透明黑色背景 (Alpha: 0.8)
- `Highlight Effect` - 高亮效果Image组件
- `Tutorial Text` - TextMeshPro文本组件
- `Arrow` - 指示箭头Image

### 2. 配置管理器

在场景中创建空GameObject，添加TutorialManager组件：

```csharp
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManagerPrefab;
    
    private void Start()
    {
        // 实例化TutorialManager
        Instantiate(tutorialManagerPrefab);
        
        // 检查新用户引导
        if (PlayerPrefs.GetInt("FirstTimeUser", 0) == 0)
        {
            TutorialManager.Start(TutorialManager.TutorialStep.FirstSpin);
        }
    }
}
```

### 3. 设置UI元素命名

确保目标UI元素使用正确的名称：

- Spin按钮: `"SpinButton"`
- 提现按钮: `"WithDrawButton"`
- 第一个提现档位: `"RedeemItem_0"`
- LuckyGift图标: `"LuckyGiftIcon"`
- Auto按钮: `"AutoButton"`

## 核心API

### TutorialManager 静态方法

```csharp
// 开始引导步骤
TutorialManager.Start(TutorialManager.TutorialStep.FirstSpin);

// 完成当前引导
TutorialManager.Complete();

// 检查是否需要显示引导
bool shouldShow = TutorialManager.ShouldShow(TutorialManager.TutorialStep.FirstSpin);

// 检查是否正在显示引导
bool isShowing = TutorialManager.IsShowing();
```

### 引导步骤枚举

```csharp
public enum TutorialStep
{
    None = 0,              // 无引导
    FirstSpin = 1,         // 首次Spin引导
    WithDrawButton = 2,    // 提现按钮引导
    WithDrawItem = 3,      // 提现档位引导
    LuckyGift = 4,         // LuckyGift引导
    AutoSpin = 5           // Auto按钮引导
}
```

## 集成示例

### 1. Spin按钮引导

```csharp
// 在SpinButtonStyle.cs中添加
private void OnSpinButtonClick()
{
    // 完成首次Spin引导
    if (TutorialManager.GetCurrentStep() == TutorialManager.TutorialStep.FirstSpin)
    {
        TutorialManager.Complete();
    }
    
    // 原有点击逻辑...
}
```

### 2. RewardCashDialog关闭后引导

```csharp
// 在RewardCashDialog.cs中添加
protected override void Close()
{
    base.Close();
    
    // 显示提现按钮引导
    if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.WithDrawButton))
    {
        TutorialManager.Start(TutorialManager.TutorialStep.WithDrawButton);
    }
}
```

### 3. WithDrawDialog引导

```csharp
// 在WithDrawDialog.cs中添加
protected override void OnEnable()
{
    base.OnEnable();
    
    StartCoroutine(DelayedWithDrawTutorial());
}

private IEnumerator DelayedWithDrawTutorial()
{
    yield return null; // 等待UI初始化
    
    if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.WithDrawItem))
    {
        TutorialManager.Start(TutorialManager.TutorialStep.WithDrawItem);
    }
}
```

### 4. LuckyGift引导

```csharp
// 在LuckyGiftActivity.cs中添加
private void CreateItem()
{
    // 原有创建逻辑...
    
    // 显示LuckyGift引导
    if (TutorialManager.ShouldShow(TutorialManager.TutorialStep.LuckyGift))
    {
        Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_SUSPEND);
        TutorialManager.Start(TutorialManager.TutorialStep.LuckyGift);
    }
}
```

## 配置系统

### ScriptableObject配置

创建TutorialConfig资产：

1. 右键点击Project视图 → Create → ZeusSlotgame → Tutorial Config
2. 配置各个引导步骤的参数

### 配置参数

- `step`: 引导步骤枚举
- `stepName`: 步骤名称
- `description`: 步骤描述
- `targetUIElement`: 目标UI元素名称
- `tutorialText`: 引导文本内容
- `canSkip`: 是否可跳过
- `delayBeforeShow`: 显示延迟时间

## 本地化支持

### 设置多语言文本

```csharp
// 获取TutorialUI实例
TutorialUI tutorialUI = FindObjectOfType<TutorialUI>();

// 设置多语言文本
tutorialUI.SetTutorialTexts(
    "Click SPIN to start",
    "Click here to withdraw", 
    "Select withdrawal amount",
    "Click lucky gift for rewards",
    "Enable auto spin feature"
);
```

### 本地化键值

建议使用本地化系统键值：

```csharp
[SerializeField] private string firstSpinTextKey = "TUTORIAL_FIRST_SPIN";
[SerializeField] private string withDrawButtonTextKey = "TUTORIAL_WITHDRAW_BUTTON";
// ...其他键值
```

## 调试和测试

### 控制台命令

```csharp
// 重置所有引导状态
TutorialManager.Instance.ResetAllTutorials();

// 测试特定引导步骤
TutorialManager.Start(TutorialManager.TutorialStep.FirstSpin);

// 查看当前引导状态
Debug.Log("Current step: " + TutorialManager.Instance.GetCurrentStep());
```

### 调试模式

在Inspector中启用调试模式：

- `Enable Tutorials`: 总开关
- `Debug Mode`: 调试日志输出

## 注意事项

1. **执行顺序**: TutorialManager需要在游戏初始化时尽早创建
2. **UI层级**: 引导UI应该在最上层，设置合适的Canvas Sort Order
3. **事件处理**: 引导期间需要正确处理UI事件穿透
4. **内存管理**: 使用DontDestroyOnLoad保持单例实例
5. **错误处理**: 目标UI元素找不到时提供友好的错误提示

## 版本历史

- **v1.0.0** (2024-12-17)
  - 初始版本发布
  - 支持5个核心引导步骤
  - 完整的配置和本地化系统
  - 调试和测试工具

## 技术支持

如有问题请联系开发团队或查看代码注释中的详细说明。