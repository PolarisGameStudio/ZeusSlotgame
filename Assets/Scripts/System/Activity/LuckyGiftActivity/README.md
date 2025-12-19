# LuckyGift Activity - 功能说明文档

## 概述

LuckyGift Activity 是一个随机奖励活动系统，在玩家进行 Slot 旋转时，满足特定条件后会生成包含随机金币奖励的礼物 Item。玩家可以点击 Item 领取奖励，前几个历史 Item 免费，后续 Item 需要观看广告。

**版本**：V2.3
**最后更新**：2025-12-19

---

## 核心功能

### 1. 活动激活机制

- **解锁条件**：玩家达到指定 Spin 次数（`UnlockSpinLimit`）
- **初始状态**：活动未激活时不生成 Item
- **激活触发**：当玩家 Spin 次数 ≥ UnlockSpinLimit 时自动激活

### 2. Item 生成机制

**触发条件**（需同时满足）：
- ✅ 活动已激活（`IsActivated == true`）
- ✅ 达到 Spin 间隔（每 `TriggerSpinLimit` 次 Spin）
- ✅ Icon 未满（当前 Item 数量 < 3）
- ✅ Spin 未中奖（`totalWinCoins == 0`）
- ✅ 不与 OnLineEarning RewardCash 冲突

**生成位置**：
- Icon 上最多同时存在 3 个 Item
- 按位置顺序生成：位置 0 → 位置 1 → 位置 2

### 3. 奖励机制

#### 奖励金额
- **随机范围**：`MinReward` 到 `MaxReward`（包含）
- **金额倍率**：基础金额 × `OnLineEarningMgr.GetCashMultiple()`

#### 免费机制（FreeCount）
- **含义**：玩家历史上获得的前 N 个 Item 免费
- **判断依据**：`totalItemCount`（从 1 开始递增）
- **示例**：
  - `FreeCount = 1`：历史第 1 个免费，第 2 个及以后需要广告
  - `FreeCount = 2`：历史前 2 个免费，第 3 个及以后需要广告

#### 广告要求
- **免费 Item**：`totalItemCount <= FreeCount`
  - 无广告图片显示
  - 点击直接领取奖励
- **需要广告 Item**：`totalItemCount > FreeCount`
  - 显示广告图片（imagead）
  - 点击后需观看插屏广告（`ADEntrances.Interstitial_Entrance_LUCKYGIFT_ACTIVITY`）

### 4. 新手引导

- **触发条件**：玩家历史上第 1 个 Item（`totalItemCount == 1`）
- **触发时机**：Item 创建动画完成后
- **引导步骤**：`TutorialManager.TutorialStep.LuckyGift`
- **AutoSpin 暂停**：引导显示时通过消息机制暂停 AutoSpin，引导关闭后恢复

### 5. 进度保存与恢复

- **保存数据**：
  - Item 奖励金额（`reward`）
  - Item 槽位索引（`slotIndex`）
  - Item 历史序号（`totalItemCount`）
- **保存时机**：每次创建或移除 Item
- **恢复时机**：App 启动，活动激活时
- **存储方式**：JSON 文件（`LuckyGiftActivityProgressData`）

### 6. 历史总数管理

- **totalItemCount**：玩家历史上获得的 Item 总数（从 1 开始）
- **存储方式**：PlayerPrefs（键名：`LuckyGiftActivity_TotalItemCount`）
- **递增时机**：每次创建新 Item
- **持久化**：即使清空进度数据，历史总数仍然保留

---

## 文件结构

### 核心文件

```
LuckyGiftActivity/
├── LuckyGiftActivity.cs              # 活动核心逻辑
├── LuckyGiftActivityIcon.cs          # Icon/容器管理
├── LuckyGiftActivityItem.cs          # 单个 Item 行为
└── LuckyGiftActivityProgressData.cs  # 进度数据
```

### 文档文件

```
LuckyGiftActivity/
├── README.md                              # 本文档
├── TOTALITEMCOUNT_IMPLEMENTATION.md       # TotalItemCount 实现详解
├── TOTALITEMCOUNT_QUICK_REF.md            # TotalItemCount 快速参考
├── AUTOSPIN_MESSAGE_IMPLEMENTATION.md     # AutoSpin 消息机制详解
├── AUTOSPIN_MESSAGE_QUICK_REF.md          # AutoSpin 快速参考
├── LUCKYGIFT_MODIFICATION_SUMMARY.md      # LuckyGift 修改总结
├── LUCKYGIFT_TUTORIAL_GUIDE.md            # LuckyGift 引导实现指南
└── LUCKYGIFT_TUTORIAL_CODE_EXAMPLE.md     # LuckyGift 引导代码示例
```

---

## 类说明

### 1. LuckyGiftActivity

**文件**：`LuckyGiftActivity.cs`

**继承**：`BaseActivity`

**职责**：
- 管理活动的激活状态
- 管理 Spin 计数和触发逻辑
- 管理玩家历史 Item 总数（`totalItemCount`）
- 提供随机奖励金额生成
- 提供配置参数（FreeCount、奖励范围等）

**核心属性**：

| 属性 | 类型 | 说明 | 默认值 |
|-----|------|------|--------|
| `UnlockSpinLimit` | int | 解锁活动需要的 Spin 次数 | 0 |
| `TriggerSpinLimit` | int | 生成 Item 的 Spin 间隔 | 5 |
| `MinReward` | int | 最小奖励金额 | 100 × 倍率 |
| `MaxReward` | int | 最大奖励金额 | 500 × 倍率 |
| `FreeCount` | int | 前几个历史 Item 免费 | 1 |
| `ItemScaleDuration` | float | Item 缩放动画时长 | 0.5s |
| `ItemStayDuration` | float | Item 停留时长 | 1s |
| `IsActivated` | bool | 活动是否已激活 | false |

**核心方法**：

```csharp
// 获取随机奖励金额
public int GetRandomReward()

// 检查是否满足触发条件（Spin 间隔）
public bool CheckTriggerSpinLimit()

// 重置触发 Spin 计数
public void ResetTriggerSpinLimit()

// 递增并保存历史 Item 总数
public int IncrementTotalItemCount()

// 获取历史 Item 总数
public int GetTotalItemCount()
```

---

### 2. LuckyGiftActivityIcon

**文件**：`LuckyGiftActivityIcon.cs`

**继承**：`BaseIcon`

**职责**：
- 管理 Icon 上的 3 个 Item 槽位
- 创建和恢复 Item
- 监听 Spin 事件，判断是否生成 Item
- 处理 Item 点击事件
- 保存和加载进度数据

**核心属性**：

| 属性 | 类型 | 说明 |
|-----|------|------|
| `slotTransforms` | Transform[3] | 3 个 Item 槽位 |
| `itemList` | List<Item> | 当前持有的 Item 列表 |
| `isProcessingItem` | bool | 是否正在处理 Item 点击 |
| `currentAdItem` | Item | 当前等待广告结果的 Item |

**核心方法**：

```csharp
// 创建新 Item
private void CreateItem()

// 恢复 Item（从进度数据）
private void RestoreItems()

// Item 被点击
public void OnItemClicked(LuckyGiftActivityItem item)

// 移除 Item 并重新排列
private void RemoveItem(LuckyGiftActivityItem item)

// 保存进度数据
private void SaveProgressData()

// 加载进度数据
private void LoadProgressData()
```

**事件监听**：

| 事件 | 说明 |
|-----|------|
| `GameConstants.SpinAwardEndMsg` | Spin 奖励结束，判断是否生成 Item |
| `ADConstants.PlayLuckyGiftActivityAD` | 广告播放成功 |
| `ADConstants.PlayLuckyGiftActivityADFailed` | 广告播放失败 |

---

### 3. LuckyGiftActivityItem

**文件**：`LuckyGiftActivityItem.cs`

**继承**：`MonoBehaviour`

**职责**：
- 管理单个 Item 的显示和动画
- 处理 Item 点击事件
- 播放创建动画（缩放 → 停留 → 飞行）
- 触发 LuckyGift 新手引导
- 播放广告

**核心属性**：

| 属性 | 类型 | 说明 |
|-----|------|------|
| `reward` | int | 奖励金额 |
| `slotIndex` | int | 槽位索引（0, 1, 2） |
| `totalItemCount` | int | 历史序号（从 1 开始） |
| `isAnimating` | bool | 是否正在播放动画 |
| `isClicked` | bool | 是否已被点击 |
| `bgAD` | Image | 广告图片（imagead） |

**核心方法**：

```csharp
// 初始化 Item（带动画）
public void Initialize(icon, reward, slotIndex, totalItemCount, scaleDuration, stayDuration, freeCount)

// 初始化 Item（无动画，用于恢复）
public void InitializeWithoutAnimation(icon, reward, slotIndex, totalItemCount, freeCount)

// 播放创建动画
private IEnumerator PlayCreateAnimation()

// 检查并显示 LuckyGift 引导
private void CheckAndShowLuckyGiftTutorial()

// 播放插屏广告
public void PlayAd()

// 获取奖励值
public int GetReward()

// 获取槽位索引
public int GetSlotIndex()

// 获取历史序号
public int GetTotalItemCount()
```

**动画流程**：

```
1. 初始 scale = 0
   ↓
2. 缩放到 2 倍（scaleDuration）
   ↓
3. 停留（stayDuration）
   ↓
4. 飞行到槽位 + 缩放到 1 倍（0.5s 并行）
   ↓
5. 动画完成，检查是否触发引导
```

---

### 4. LuckyGiftActivityProgressData

**文件**：`LuckyGiftActivityProgressData.cs`

**继承**：`ProgressDataBase<LuckyGiftActivityProgressData>`

**职责**：
- 保存和加载 Item 进度数据
- 管理 Item 列表

**数据结构**：

```csharp
public class LuckyGiftActivityItemData
{
    public int reward;           // 奖励金额
    public int slotIndex;        // 槽位索引
    public int totalItemCount;   // 历史序号
}
```

**核心方法**：

```csharp
// 添加 Item 数据
public void AddItem(int reward, int slotIndex, int totalItemCount)

// 清空所有 Item
public void ClearItems()

// 保存数据
public override void SaveData()

// 加载数据
public override void LoadData(LuckyGiftActivityProgressData progressData)
```

---

## 配置说明

### JSON 配置格式

```json
{
  "LuckyGiftActivity": {
    "UnlockSpinLimit": 0,      // 解锁 Spin 次数（0 表示立即解锁）
    "TriggerSpinLimit": 5,     // 生成 Item 的 Spin 间隔
    "Min": 100,                // 最小奖励金额（基础值）
    "Max": 500,                // 最大奖励金额（基础值）
    "FreeCount": 2,            // 前几个历史 Item 免费
    "ItemPrefab": "路径",       // Item Prefab 路径（在 iconData 中配置）
    "ItemScaleDuration": 0.5,  // 缩放动画时长（可选）
    "ItemStayDuration": 1.0    // 停留时长（可选）
  }
}
```

### 配置参数说明

#### UnlockSpinLimit
- **含义**：玩家需要达到的 Spin 次数才能激活活动
- **默认值**：0（立即激活）
- **建议范围**：0 - 100

#### TriggerSpinLimit
- **含义**：每隔多少次 Spin 生成一个 Item
- **默认值**：5
- **建议范围**：3 - 10

#### Min / Max
- **含义**：奖励金额的随机范围（基础值）
- **实际金额**：基础值 × `OnLineEarningMgr.GetCashMultiple()`
- **默认值**：100 - 500
- **建议范围**：根据游戏经济设计

#### FreeCount
- **含义**：玩家历史上获得的前几个 Item 免费（无需广告）
- **默认值**：1
- **建议范围**：1 - 3
- **效果示例**：
  - `FreeCount = 1`：只有历史第 1 个免费
  - `FreeCount = 2`：历史前 2 个免费
  - `FreeCount = 3`：历史前 3 个免费

---

## 核心流程

### 1. 活动激活流程

```
游戏启动
  ↓
LuckyGiftActivity 构造函数
  - 加载配置
  - 读取 totalItemCount（PlayerPrefs）
  - curSpin = 玩家总 Spin 次数
  ↓
OnInit()
  ↓
CheckActivation()
  - 如果 UnlockSpinLimit <= 0 或 curSpin >= UnlockSpinLimit
  ↓
Activate()
  - IsActivated = true
  - 触发 OnActivated 事件
  ↓
LuckyGiftActivityIcon.OnActivityActivated()
  - LoadProgressData()
  - RestoreItems()（如果有进度数据）
```

---

### 2. Item 生成流程

```
玩家进行 Spin
  ↓
Spin 结束（SpinAwardEndMsg）
  ↓
LuckyGiftActivityIcon.OnSpinAwardEnd()
  ↓
检查生成条件：
  - activity.IsActivated == true ✓
  - activity.CheckTriggerSpinLimit() == true ✓
  - itemList.Count < 3 ✓
  - totalWinCoins == 0 ✓
  - !OnLineEarningMgr.PredictShowRewardCash() ✓
  ↓
CreateItem()
  ↓
1. 加载 Item Prefab（Addressable 异步）
  ↓
2. 实例化 GameObject
  ↓
3. 生成随机奖励金额
  ↓
4. totalItemCount = activity.IncrementTotalItemCount()
   - totalItemCount++
   - PlayerPrefs 保存
  ↓
5. 设置父节点到对应槽位
  ↓
6. item.Initialize(..., totalItemCount, ..., freeCount)
   - 判断是否显示广告图片：totalItemCount > freeCount
   - 开始播放创建动画
  ↓
7. itemList.Add(item)
  ↓
8. SaveProgressData()
  ↓
9. 动画完成后检查是否触发引导
   - 如果 totalItemCount == 1，触发 LuckyGift 引导
```

---

### 3. Item 点击流程

```
玩家点击 Item
  ↓
LuckyGiftActivityItem.OnItemClick()
  - 检查 isAnimating、isClicked
  - 检查 icon.CanClickItem()
  ↓
LuckyGiftActivityIcon.OnItemClicked(item)
  - 设置 isProcessingItem = true
  - 保存 currentAdItem = item
  ↓
判断是否免费：
  - if (item.GetTotalItemCount() <= activity.FreeCount)
  ↓
【免费路径】
  ↓
  HandleAdResult()
    - 打开奖励弹窗（ExtraAwardCashDialog）
    - 弹窗关闭后：
      - RemoveItem(item)
      - ReorderItems()
      - SaveProgressData()
      - isProcessingItem = false

【需要广告路径】
  ↓
  item.PlayAd()
    - 埋点：BuryPoint "Ad_LuckyGift"
    - 广播广告请求：ADConstants.PlayAdByEntrance
    - ADEntrances: Interstitial_Entrance_LUCKYGIFT_ACTIVITY
  ↓
  广告播放成功/失败
  ↓
  OnAdSuccess() 或 OnAdFailed()
  ↓
  HandleAdResult()
    - 同免费路径
```

---

### 4. 引导流程

```
Item 创建动画完成
  ↓
PlayCreateAnimation() 结束
  ↓
if (totalItemCount == 1)  // 历史第 1 个 Item
  ↓
CheckAndShowLuckyGiftTutorial()
  ↓
检查：TutorialManager.ShouldShow(LuckyGift)
  - 如果已完成，跳过
  ↓
暂停 AutoSpin：
  - Messenger.Broadcast(AUTO_SPIN_SUSPEND)
  ↓
显示引导：
  - TutorialManager.Start(LuckyGift, parentNode, callback)
  - parentNode: BannerCanvas 或 null
  ↓
玩家点击关闭引导
  ↓
引导完成回调：
  - 恢复 AutoSpin: Messenger.Broadcast(AUTO_SPIN_RESUME)
  - 调用 OnItemClick() 触发点击逻辑
```

---

### 5. 进度恢复流程

```
App 启动
  ↓
LuckyGiftActivity 构造函数
  - totalItemCount = PlayerPrefs.GetInt("LuckyGiftActivity_TotalItemCount", 0)
  ↓
活动激活
  ↓
LuckyGiftActivityIcon.OnActivityActivated()
  ↓
LoadProgressData()
  - 从 JSON 加载 LuckyGiftActivityProgressData
  - progressData.itemList（包含 reward, slotIndex, totalItemCount）
  ↓
RestoreItems()
  ↓
遍历 progressData.itemList
  ↓
1. 加载 Item Prefab
  ↓
2. 实例化到对应槽位
  ↓
3. item.InitializeWithoutAnimation(..., totalItemCount, freeCount)
   - 判断是否显示广告图片：totalItemCount > freeCount
   - 不播放动画，直接显示
  ↓
4. itemList.Add(item)
  ↓
恢复完成
```

---

## 关键逻辑

### 1. 免费判断逻辑

**显示广告图片**：
```csharp
bgAD.gameObject.SetActive(totalItemCount > freeCount);
```

**点击是否免费**：
```csharp
if (item.GetTotalItemCount() <= activity.FreeCount)
{
    HandleAdResult(); // 免费
}
else
{
    item.PlayAd(); // 需要广告
}
```

**规则**：
- `totalItemCount <= freeCount`：免费
- `totalItemCount > freeCount`：需要广告

---

### 2. 引导触发逻辑

**触发条件**：
```csharp
if (totalItemCount == 1)
{
    CheckAndShowLuckyGiftTutorial();
}
```

**规则**：
- 只有玩家历史上第 1 个 Item 触发引导
- 即使 `FreeCount > 1`，也只有第 1 个触发引导
- 引导显示时暂停 AutoSpin，引导关闭后恢复

---

### 3. AutoSpin 暂停/恢复

**暂停**（引导显示前）：
```csharp
Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_SUSPEND);
```

**恢复**（引导完成后）：
```csharp
Messenger.Broadcast(SlotControllerConstants.AUTO_SPIN_RESUME);
```

**机制**：
- 通过消息系统通知 `BaseSlotMachineController`
- 内部管理 `_autospinSuspending` 标志位
- 只有在 AutoRun 运行时才会暂停
- 恢复时自动调用 `DoSpin()` 开始新的 spin

---

### 4. totalItemCount 管理

**递增时机**：
```csharp
int totalItemCount = activity.IncrementTotalItemCount();
// totalItemCount++
// PlayerPrefs.SetInt("LuckyGiftActivity_TotalItemCount", totalItemCount)
// PlayerPrefs.Save()
```

**使用场景**：
- 判断是否免费
- 判断是否触发引导
- 保存进度数据

**特点**：
- 全局递增，不会重置
- 即使清空进度数据，totalItemCount 仍然保留
- 用于实现"玩家历史上前几个免费"的功能

---

## 使用示例

### 场景 1：新玩家首次玩游戏（FreeCount=2）

```
初始状态：
  - totalItemCount = 0
  - IsActivated = false

玩家进行 Spin（假设 UnlockSpinLimit = 0）：
  ↓
活动立即激活（IsActivated = true）
  ↓
第 5 次 Spin 后生成第 1 个 Item：
  - totalItemCount = 1
  - slotIndex = 0
  - 无广告图片（1 <= 2）
  - 动画完成后触发 LuckyGift 引导
  - AutoSpin 暂停
  ↓
玩家关闭引导：
  - AutoSpin 恢复
  - 自动触发点击逻辑
  ↓
点击第 1 个 Item：
  - totalItemCount (1) <= FreeCount (2)
  - 免费领取奖励
  - Item 被移除
  ↓
第 10 次 Spin 后生成第 2 个 Item：
  - totalItemCount = 2
  - slotIndex = 0（前一个被移除，位置重用）
  - 无广告图片（2 <= 2）
  - 不触发引导（totalItemCount != 1）
  ↓
点击第 2 个 Item：
  - totalItemCount (2) <= FreeCount (2)
  - 免费领取奖励
  ↓
第 15 次 Spin 后生成第 3 个 Item：
  - totalItemCount = 3
  - slotIndex = 0
  - 有广告图片（3 > 2）
  ↓
点击第 3 个 Item：
  - totalItemCount (3) > FreeCount (2)
  - 需要观看广告
  - 广告播放成功后领取奖励
```

---

### 场景 2：老玩家重启 App（FreeCount=2，历史已获得 10 个）

```
App 启动：
  - totalItemCount = 10（从 PlayerPrefs 加载）
  - 活动激活
  - 恢复进度数据：3 个 Item
    - Item 1: totalItemCount = 8, slotIndex = 0, 有广告图片
    - Item 2: totalItemCount = 9, slotIndex = 1, 有广告图片
    - Item 3: totalItemCount = 10, slotIndex = 2, 有广告图片
  ↓
点击 Item 1：
  - totalItemCount (8) > FreeCount (2)
  - 需要观看广告
  ↓
点击后移除，Item 2 和 3 前移：
  - Item 2: slotIndex = 0
  - Item 3: slotIndex = 1
  ↓
第 5 次 Spin 后生成新 Item：
  - totalItemCount = 11（递增）
  - slotIndex = 2
  - 有广告图片（11 > 2）
  - 不触发引导（totalItemCount != 1）
  ↓
点击新 Item：
  - totalItemCount (11) > FreeCount (2)
  - 需要观看广告
```

---

## 调试指南

### 日志关键字

搜索以下日志关键字可以快速定位问题：

```
[LuckyGiftActivity]        - 活动核心逻辑
[LuckyGiftActivityIcon]    - Icon/容器管理
[LuckyGiftActivityItem]    - Item 行为
[TutorialManager]          - 引导系统
```

### 常见日志示例

#### 活动激活
```
[LuckyGiftActivity] 从本地加载 TotalItemCount: 0
[LuckyGiftActivity] Config loaded - FreeCount: 2
[LuckyGiftActivityAdNode] Activity activated (curSpin: 0, UnlockSpinLimit: 0)
```

#### Item 生成
```
[LuckyGiftActivity] TotalItemCount 递增为: 1
[LuckyGiftActivityItem] Initialize - slotIndex: 0, totalItemCount: 1, freeCount: 2, showAd: False
[LuckyGiftActivityIcon] Item created - slotIndex: 0, totalItemCount: 1, current count: 1
```

#### 引导触发
```
[LuckyGiftActivityItem] 开始显示 LuckyGift 引导
[LuckyGiftActivityItem] 已广播 AUTO_SPIN_SUSPEND 消息
[TutorialManager] StartTutorial被调用 - Step: LuckyGift
[LuckyGiftActivityItem] LuckyGift 引导已完成
[LuckyGiftActivityItem] 已广播 AUTO_SPIN_RESUME 消息
```

#### Item 点击
```
[LuckyGiftActivityIcon] 第 1 个历史 item 是免费的（FreeCount=2），无需广告，直接发放奖励
[LuckyGiftActivityIcon] 第 3 个历史 item 需要观看广告（FreeCount=2）
```

---

### 调试命令

#### 查看 totalItemCount
```csharp
int count = PlayerPrefs.GetInt("LuckyGiftActivity_TotalItemCount", 0);
Debug.Log($"当前 TotalItemCount: {count}");
```

#### 重置 totalItemCount
```csharp
PlayerPrefs.SetInt("LuckyGiftActivity_TotalItemCount", 0);
PlayerPrefs.Save();
Debug.Log("TotalItemCount 已重置为 0");
```

#### 清空进度数据
```csharp
LuckyGiftActivityProgressData progressData = new LuckyGiftActivityProgressData();
progressData.ClearData();
Debug.Log("进度数据已清空");
```

---

## 常见问题

### Q1: 为什么 Item 没有生成？

**检查清单**：
1. ✅ 活动是否已激活（`IsActivated == true`）
2. ✅ 是否达到 Spin 间隔（`TriggerSpinLimit`）
3. ✅ Icon 是否已满（`itemList.Count < 3`）
4. ✅ Spin 是否中奖（`totalWinCoins == 0`）
5. ✅ 是否与 RewardCash 冲突（`!PredictShowRewardCash()`）

---

### Q2: 为什么免费 Item 仍显示广告图片？

**原因**：`totalItemCount > freeCount`

**检查**：
- 查看 `totalItemCount`（PlayerPrefs）
- 查看 `FreeCount` 配置

**示例**：
- 玩家历史上已获得 10 个 Item
- `totalItemCount = 11`
- `FreeCount = 2`
- 第 11 个 Item 需要广告（11 > 2）

---

### Q3: 为什么引导没有触发？

**检查清单**：
1. ✅ 是否是玩家历史上第 1 个 Item（`totalItemCount == 1`）
2. ✅ 引导是否已完成（`TutorialManager.ShouldShow(LuckyGift)`）
3. ✅ 引导动画是否完成（触发时机在动画完成后）

---

### Q4: 为什么 AutoSpin 没有恢复？

**原因**：可能是引导完成回调未执行，或消息未正确广播

**检查**：
- 搜索日志：`已广播 AUTO_SPIN_RESUME 消息`
- 检查 `BaseSlotMachineController` 的 `_autospinSuspending` 标志位

---

### Q5: App 重启后 Item 丢失？

**原因**：进度数据未正确保存

**检查**：
- 确认 `SaveProgressData()` 被调用
- 检查 JSON 文件是否存在：`LuckyGiftActivityProgressData`

---

### Q6: 为什么历史总数与实际 Item 数量不匹配？

**说明**：这是正常现象

- `totalItemCount`：玩家历史上获得的所有 Item 总数（递增，不重置）
- `itemList.Count`：当前 Icon 上的 Item 数量（0-3）

**示例**：
- 玩家历史上获得了 20 个 Item
- 当前 Icon 上只有 2 个 Item
- `totalItemCount = 20`
- `itemList.Count = 2`

---

## 性能优化建议

### 1. Addressable 异步加载

- 使用 Addressable 异步加载 Item Prefab
- 避免同步加载卡顿

### 2. Item 复用

- 考虑实现对象池，复用 Item GameObject
- 减少频繁创建和销毁的开销

### 3. 进度数据优化

- 只保存必要的数据（reward, slotIndex, totalItemCount）
- 避免保存冗余信息

### 4. 广告预加载

- 考虑在 Item 生成时预加载广告
- 减少点击后等待时间

---

## 测试清单

### 功能测试

- [ ] 活动激活（达到 UnlockSpinLimit）
- [ ] Item 生成（满足所有触发条件）
- [ ] 免费 Item 点击领取
- [ ] 广告 Item 观看广告后领取
- [ ] 引导触发（历史第 1 个 Item）
- [ ] AutoSpin 暂停和恢复
- [ ] 进度保存和恢复
- [ ] totalItemCount 递增和持久化

### 边界测试

- [ ] FreeCount = 0（全部需要广告）
- [ ] FreeCount = 100（远超实际 Item 数）
- [ ] Icon 已满（3 个 Item）时不再生成
- [ ] 广告播放失败仍能领取奖励
- [ ] App 重启后数据正确恢复

### 性能测试

- [ ] Item 创建动画流畅
- [ ] 广告加载不卡顿
- [ ] 进度数据保存不影响性能

---

## 未来扩展建议

### 1. 多种奖励类型

- 支持金币以外的奖励（道具、代币等）
- 增加特殊奖励（稀有物品）

### 2. 更丰富的动画

- Item 生成特效
- 领取奖励特效
- 槽位切换动画

### 3. 限时活动

- 支持活动开始和结束时间
- 活动倒计时显示

### 4. 每日限制

- 限制每日最多生成 Item 数量
- 每日重置 FreeCount

### 5. VIP 特权

- VIP 玩家增加 FreeCount
- VIP 玩家减少 TriggerSpinLimit

---

## 联系方式

**维护者**：Claude AI
**最后更新**：2025-12-19
**版本**：V2.3

---

## 版本历史

### V2.3 (2025-12-19)
- ✅ 使用消息机制（AUTO_SPIN_SUSPEND/RESUME）控制 AutoSpin
- ✅ 移除 Time.timeScale 的使用

### V2.2 (2025-12-19)
- ✅ 使用 Time.timeScale 暂停游戏（替代 AutoSpinSuspendManager）

### V2.1 (2025-12-19)
- ✅ 添加引导时 AutoSpin 暂停/恢复功能

### V2.0 (2025-12-19)
- ✅ 实现基于历史总数的 FreeCount 机制
- ✅ 添加 totalItemCount 本地存储
- ✅ 引导触发改为基于历史第 1 个 Item

### V1.2 (2025-12-18)
- ✅ 添加 FreeCount 配置（基于位置）

### V1.1 (2025-12-18)
- ✅ 添加 LuckyGift 新手引导

### V1.0 (2025-12-12)
- ✅ 初始版本实现
- ✅ 基础 Item 生成和领取功能
- ✅ 广告集成
- ✅ 进度保存和恢复
