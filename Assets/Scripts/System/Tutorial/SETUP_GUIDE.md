# TutorialUI 完整配置指南

基于实际资源的Tutorial UI组件设置指南

## 📦 资源清单

已准备的资源位置：`Assets/AssetResources/Tutorial/Textures/`

| 文件名 | 用途 | 规格 |
|-------|------|------|
| `UI_Highlight_Border.png` | 矩形按钮高亮边框 | 黄色圆角矩形边框 |
| `UI_Highlight_Circle.png` | 圆形按钮高亮边框 | 黄色圆形边框 |
| `UI_Speech_Bubble.png` | 文本气泡背景 | 浅黄色圆角矩形+阴影 |
| `UI_Hand_Pointer.png` | 手指指示图标 | 白色手套+蓝色渐变 |
| `UI_Arrow_Down.png` | 向下箭头 | 浅黄色三角箭头 |
| `LuckyGift.png` | LuckyGift序列帧动画 | **3x4网格 = 12帧** |

---

## 🏗️ Unity层级结构创建步骤

### 步骤1: 创建Canvas根节点

```
1. 右键 Hierarchy → UI → Canvas
2. 重命名为: TutorialOverlay
3. 设置Canvas组件:
   - Render Mode: Screen Space - Overlay
   - Sort Order: 999 (确保在最上层)
4. 设置Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1080 x 1920
   - Match: 0.5
5. 添加Canvas Group组件到TutorialOverlay
   - 设置Alpha: 0 (初始隐藏)
   - Blocks Raycasts: false
```

### 步骤2: 创建Background（背景遮罩）

```
1. 右键 TutorialOverlay → UI → Image
2. 重命名为: Background
3. 设置RectTransform:
   - Anchor Preset: Stretch全屏
   - Left: 0, Right: 0, Top: 0, Bottom: 0
4. 设置Image组件:
   - Source Image: None
   - Color: rgba(0, 0, 0, 204)  // 黑色80%透明
   - Raycast Target: ✓
5. 添加Button组件
   - Transition: None
```

### 步骤3: 创建HighlightContainer（高亮容器）

```
1. 右键 TutorialOverlay → Create Empty
2. 重命名为: HighlightContainer
3. 添加RectTransform组件
4. 设置:
   - Anchor: Center
   - Pivot: (0.5, 0.5)
   - Position: (0, 0, 0)
   - Size: (200, 100) 默认大小，运行时动态调整
```

#### 3.1 创建 HighlightBorder（矩形边框）

```
1. 右键 HighlightContainer → UI → Image
2. 重命名为: HighlightBorder
3. 设置Image组件:
   - Source Image: AssetResources/Tutorial/Textures/UI_Highlight_Border
   - Image Type: Sliced
   - Pixels Per Unit Multiplier: 1
   - Color: rgba(255, 255, 0, 255) 黄色
   - Raycast Target: ✗ (重要！)
4. 设置RectTransform:
   - Anchor: Stretch
   - Left: 0, Right: 0, Top: 0, Bottom: 0
```

#### 3.2 创建 HighlightCircle（圆形边框）

```
1. 右键 HighlightContainer → UI → Image
2. 重命名为: HighlightCircle
3. 设置Image组件:
   - Source Image: AssetResources/Tutorial/Textures/UI_Highlight_Circle
   - Color: rgba(255, 255, 0, 255) 黄色
   - Raycast Target: ✗
   - Preserve Aspect: ✓
4. 设置RectTransform:
   - Anchor: Center
   - Size: (150, 150)
5. 设置Active: ✗ (默认隐藏)
```

#### 3.3 创建 LuckyGiftParticle（序列帧动画）

```
1. 右键 HighlightContainer → UI → Image
2. 重命名为: LuckyGiftParticle
3. 设置Image组件:
   - Source Image: LuckyGift的第一帧 (后续步骤说明如何切片)
   - Color: White
   - Raycast Target: ✗
4. 设置RectTransform:
   - Anchor: Center
   - Size: (200, 200)
5. 设置Active: ✗ (默认隐藏，仅LuckyGift步骤使用)
```

### 步骤4: 创建TutorialTextBubble（文本气泡）

```
1. 右键 TutorialOverlay → Create Empty
2. 重命名为: TutorialTextBubble
3. 设置RectTransform:
   - Anchor: Top Center
   - Position: (0, -200, 0)
4. 添加ContentSizeFitter组件:
   - Horizontal Fit: Preferred Size
   - Vertical Fit: Preferred Size
```

#### 4.1 创建 BubbleBackground

```
1. 右键 TutorialTextBubble → UI → Image
2. 重命名为: BubbleBackground
3. 设置Image组件:
   - Source Image: AssetResources/Tutorial/Textures/UI_Speech_Bubble
   - Image Type: Sliced
   - Color: White
4. 添加Layout Element组件:
   - Min Width: 300
   - Preferred Width: -
   - Flexible Width: 1
```

#### 4.2 创建 TutorialText

```
1. 右键 BubbleBackground → UI → Text - TextMeshPro
2. 重命名为: TutorialText
3. 设置TextMeshProUGUI组件:
   - Font Asset: 选择支持中文的字体 (如 NotoSansSC-Regular)
   - Font Size: 28
   - Color: rgb(101, 67, 33) 深棕色
   - Alignment: Center & Middle
   - Auto Size: ✓
   - Font Size Min: 18
   - Font Size Max: 32
   - Overflow: Overflow
4. 设置RectTransform:
   - Anchor: Stretch
   - Left: 30, Right: 30, Top: 20, Bottom: 20 (留边距)
```

### 步骤5: 创建PointerContainer（指示器容器）

```
1. 右键 TutorialOverlay → Create Empty
2. 重命名为: PointerContainer
3. 设置RectTransform:
   - Anchor: Center
   - Position: (0, 0, 0)
   - Size: (80, 80)
```

#### 5.1 创建 HandPointer

```
1. 右键 PointerContainer → UI → Image
2. 重命名为: HandPointer
3. 设置Image组件:
   - Source Image: AssetResources/Tutorial/Textures/UI_Hand_Pointer
   - Color: White
   - Raycast Target: ✗
   - Preserve Aspect: ✓
4. 设置RectTransform:
   - Anchor: Center
   - Size: (80, 80)
```

#### 5.2 创建 ArrowPointer（备用）

```
1. 右键 PointerContainer → UI → Image
2. 重命名为: ArrowPointer
3. 设置Image组件:
   - Source Image: AssetResources/Tutorial/Textures/UI_Arrow_Down
   - Color: White
   - Raycast Target: ✗
   - Preserve Aspect: ✓
4. 设置RectTransform:
   - Anchor: Center
   - Size: (60, 60)
5. 设置Active: ✗ (默认隐藏，使用手指指示器)
```

---

## 🎨 序列帧动画配置（LuckyGift.png）

### 步骤1: 切片Sprite Sheet

```
1. 选中 Assets/AssetResources/Tutorial/Textures/LuckyGift.png
2. Inspector中设置:
   - Texture Type: Sprite (2D and UI)
   - Sprite Mode: Multiple ⭐
   - Pixels Per Unit: 100
   - Mesh Type: Full Rect
   - Filter Mode: Bilinear
   - Format: RGBA 32 bit
3. 点击 "Sprite Editor" 按钮
4. 在Sprite Editor中:
   - 点击 "Slice" 下拉菜单
   - Type: Grid By Cell Count
   - Column: 3 (列数)
   - Row: 4 (行数)
   - 点击 "Slice" 按钮
5. 保存并关闭Sprite Editor
6. Apply导入设置
```

### 步骤2: 创建Sprite数组

```
现在LuckyGift.png会在Project视图中展开显示12个子sprite：
├─ LuckyGift_0
├─ LuckyGift_1
├─ LuckyGift_2
├─ ...
└─ LuckyGift_11

这12个sprite将用于序列帧动画播放
```

---

## 🔧 TutorialUI脚本配置

### 步骤1: 添加TutorialUI组件

```
1. 选中 TutorialOverlay
2. Add Component → TutorialUI
```

### 步骤2: 配置Inspector字段

#### [UI组件引用]
```
- Overlay Canvas Group: 拖拽 TutorialOverlay 的 Canvas Group 组件
- Highlight Container: 拖拽 HighlightContainer 游戏对象
- Tutorial Text: 拖拽 TutorialText (TextMeshProUGUI)
- Pointer Container: 拖拽 PointerContainer (RectTransform)
- Background Button: 拖拽 Background 的 Button 组件
```

#### [高亮边框]
```
- Highlight Border: 拖拽 HighlightBorder (Image)
- Highlight Circle: 拖拽 HighlightCircle (Image)
```

#### [LuckyGift序列帧动画] ⭐关键
```
- Lucky Gift Particle: 拖拽 LuckyGiftParticle (Image)
- Lucky Gift Frames:
  - Size: 12
  - 依次拖拽 LuckyGift_0 ~ LuckyGift_11 这12个sprite
  - 顺序很重要！必须从0到11按顺序排列
- Frame Rate: 24 (每秒播放24帧)
```

#### [指示器]
```
- Hand Pointer: 拖拽 HandPointer (Image)
- Arrow Pointer: 拖拽 ArrowPointer (Image)
```

#### [动画设置]
```
- Fade Duration: 0.3 (淡入淡出时间)
- Highlight Pulse Duration: 1.0 (高亮呼吸周期)
- Highlight Min Alpha: 0.5 (最小透明度)
- Highlight Max Alpha: 1.0 (最大透明度)
- Pointer Float Distance: 15 (手指浮动距离)
- Pointer Float Duration: 0.8 (手指浮动周期)
```

#### [引导文本配置]
```
- First Spin Text: "点击SPIN按钮开始游戏"
- With Draw Button Text: "点击这里可以提现"
- With Draw Item Text: "选择提现档位获取奖励"
- Lucky Gift Text: "点击幸运礼物获取额外奖励"
- Auto Spin Text: "开启自动旋转功能"
```

### 步骤3: 保存为Prefab

```
1. 将配置好的 TutorialOverlay 拖拽到 Project 视图
2. 保存路径: Assets/AssetResources/Tutorial/Prefab/TutorialOverlay.prefab
3. 删除场景中的TutorialOverlay实例（由TutorialManager动态加载）
```

---

## 📊 组件引用关系图

```
TutorialUI (脚本)
├── overlayCanvasGroup → TutorialOverlay (Canvas Group)
├── highlightContainer → HighlightContainer (GameObject)
│   ├── highlightBorder → HighlightBorder (Image) ⭐
│   ├── highlightCircle → HighlightCircle (Image) ⭐
│   └── luckyGiftParticle → LuckyGiftParticle (Image) ⭐
├── tutorialText → TutorialText (TextMeshProUGUI)
├── pointerContainer → PointerContainer (RectTransform)
│   ├── handPointer → HandPointer (Image)
│   └── arrowPointer → ArrowPointer (Image)
├── backgroundButton → Background (Button)
└── luckyGiftFrames → LuckyGift_0~11 (Sprite[12]) ⭐⭐⭐
```

---

## ✅ 检查清单

配置完成后，请确认以下项：

### 基础配置
- [ ] TutorialOverlay的Canvas Sort Order设置为999
- [ ] Canvas Group的Alpha初始值为0
- [ ] Background的Color设置为半透明黑色
- [ ] 所有Image的Raycast Target正确设置（边框=false，背景=true）

### 高亮边框
- [ ] HighlightBorder使用UI_Highlight_Border sprite
- [ ] HighlightCircle使用UI_Highlight_Circle sprite
- [ ] LuckyGiftParticle的luckyGiftFrames数组有12个sprite
- [ ] 序列帧顺序正确（0→11）

### 文本气泡
- [ ] TutorialText使用支持中文的字体
- [ ] ContentSizeFitter已添加到TextBubble
- [ ] BubbleBackground使用UI_Speech_Bubble sprite

### 指示器
- [ ] HandPointer使用UI_Hand_Pointer sprite
- [ ] Preserve Aspect已启用
- [ ] 浮动动画参数已配置

### 脚本引用
- [ ] TutorialUI的所有序列化字段已正确赋值
- [ ] 没有Missing Reference警告
- [ ] Prefab已保存

---

## 🧪 测试方法

### 方法1: Inspector测试

```csharp
1. 在场景中临时添加TutorialOverlay预制体
2. 选中TutorialOverlay
3. 在Inspector中手动调用:
   - TutorialUI.Show()
   - 检查遮罩是否显示
   - 检查高亮边框是否呼吸动画
   - 检查手指是否浮动
4. 调用 TutorialUI.Hide() 确认隐藏正常
```

### 方法2: 代码测试

```csharp
// 在某个测试脚本中
void Start()
{
    // 测试FirstSpin引导
    TutorialManager.Start(TutorialManager.TutorialStep.FirstSpin);

    // 5秒后测试LuckyGift引导（观察序列帧动画）
    StartCoroutine(TestLuckyGift());
}

IEnumerator TestLuckyGift()
{
    yield return new WaitForSeconds(5f);
    TutorialManager.Start(TutorialManager.TutorialStep.LuckyGift);
}
```

### 方法3: 验证序列帧

```
1. 运行游戏
2. 触发LuckyGift引导
3. 观察高亮边框是否播放12帧动画
4. 查看Console日志:
   [TutorialUI] 序列帧动画已启动，共12帧
5. 确认动画循环播放流畅
```

---

## 🔍 常见问题

### Q1: 序列帧动画不播放？
**检查项：**
- LuckyGift.png是否正确切片为3x4网格
- luckyGiftFrames数组是否有12个sprite
- Frame Rate是否设置（默认24）
- isPlayingSequence是否为true

### Q2: 高亮边框没有呼吸效果？
**检查项：**
- highlightBorder的Image组件是否正确赋值
- DOTween是否导入项目
- highlightPulseDuration参数是否大于0

### Q3: 手指不浮动？
**检查项：**
- handPointer是否正确赋值
- pointerFloatDistance和pointerFloatDuration是否设置
- DOTween动画是否冲突

### Q4: 点击穿透不工作？
**检查项：**
- 高亮边框的Raycast Target必须为false
- Background的Raycast Target必须为true
- Canvas的Graphic Raycaster组件是否存在

---

## 📚 相关文档

- [Tutorial系统概览](README.md)
- [TutorialManager API](TutorialManager.cs)
- [集成示例](README.md#集成示例)

---

## 🎉 完成！

配置完成后，你将拥有一个完整的Tutorial引导系统：

✅ 5种引导步骤（FirstSpin, WithdrawButton, WithdrawItem, LuckyGift, AutoSpin）
✅ 3种高亮效果（矩形边框、圆形边框、序列帧动画）
✅ 流畅的动画效果（呼吸、浮动、序列帧）
✅ 可配置的文本和参数
✅ 完整的状态管理

现在可以开始集成到你的游戏中了！
