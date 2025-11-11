using System;
using System.Collections;
using System.Collections.Generic;
using Ads;
using Classic;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;

namespace Activity
{
    public class LuckyGiftActivityIcon : BaseIcon
    {
        // 关联的活动实例
        private LuckyGiftActivity activity;
        // item创建起点
        private RectTransform itemStartPos;
        // item挂载的三个槽位
        private readonly Transform[] slotTransforms = new Transform[3];
        // 当前持有的item列表
        private readonly List<LuckyGiftActivityItem> itemList = new List<LuckyGiftActivityItem>();

        // 最大item数量
        private const int MaxItemCount = 3;

        // 是否正在处理广告或点击
        private bool isProcessingItem;
        // 当前等待广告结果的item
        private LuckyGiftActivityItem currentAdItem;
        // 配置的item prefab路径
        private string itemPrefabPath = string.Empty;
        // 本地化的进度数据
        private readonly LuckyGiftActivityProgressData progressData = new LuckyGiftActivityProgressData();
        
        public RectTransform mask;


        private void Awake()
        {
            itemStartPos = Utilities.RealFindObj<RectTransform>(transform, "startPos");
            mask = Utilities.RealFindObj<RectTransform>(transform, "mask");
        }

        public override void OnInit(int Id, Dictionary<string, object> data)
        {
            base.OnInit(Id, data);
            
            if (data != null)
            {
                itemPrefabPath = Utilities.GetString(data, "ItemPrefab", "");
            }

            FindSlotTransforms();
            gameObject.SetActive(true);
        }

        public void SetActivity(LuckyGiftActivity act)
        {
            // 避免重复订阅
            if (activity != null)
            {
                activity.OnActivated -= OnActivityActivated;
            }
            activity = act;
            if (activity != null)
            {
                activity.OnActivated += OnActivityActivated;
            }
            if (activity != null && activity.IsActivated)
            {
                OnActivityActivated();
            }
        }

        /// <summary>
        /// 活动激活时回调，由LuckyCashActivity触发
        /// </summary>
        public void OnActivityActivated()
        {
            if (activity == null)
            {
                return;
            }
            Debug.Log("[LuckyGiftActivityIcon] Activity activated, functionality unlocked");
            
            // 加载进度数据并恢复item
            LoadProgressData();
        }

        /// <summary>
        /// 查找三个子物体标志位（名字为1,2,3）
        /// </summary>
        private void FindSlotTransforms()
        {
            for (int i = 0; i < 3; i++)
            {
                string slotName = (i + 1).ToString();
                slotTransforms[i] = Utilities.RealFindObj<Transform>(transform,"content/"+slotName);
                if (slotTransforms[i] == null)
                {
                    Debug.LogWarning($"[LuckyGiftActivityIcon] Slot transform '{slotName}' not found");
                }
            }
        }

        protected override void AddListener()
        {
            base.AddListener();
            Messenger.AddListener<ReelManager, long>(GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
            Messenger.AddListener<int>(ADConstants.PlayLuckyGiftActivityAD, OnAdSuccess);
            Messenger.AddListener<int>(ADConstants.PlayLuckyGiftActivityADFailed, OnAdFailed);
        }

        protected override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener<ReelManager, long>(GameConstants.SpinAwardEndMsg, OnSpinAwardEnd);
            Messenger.RemoveListener<int>(ADConstants.PlayLuckyGiftActivityAD, OnAdSuccess);
            Messenger.RemoveListener<int>(ADConstants.PlayLuckyGiftActivityADFailed, OnAdFailed);
        }

        /// <summary>
        /// Spin奖励结束消息处理
        /// </summary>
        private void OnSpinAwardEnd(ReelManager reelManager, long totalWinCoins)
        {
            if (activity == null ||
                !activity.IsActivated ||
                !activity.CheckTriggerSpinLimit() ||
                itemList.Count >= MaxItemCount ||
                totalWinCoins > 0 ||
                OnLineEarningMgr.Instance.PredictShowRewardCash())
            {
                return;
            }

            CreateItem();
            activity.ResetTriggerSpinLimit();
        }

        /// <summary>
        /// 创建item
        /// </summary>
        private void CreateItem()
        {
            if (string.IsNullOrEmpty(itemPrefabPath) || itemStartPos == null)
            {
                Debug.LogError("[LuckyGiftActivityIcon] ItemPrefab path is invalid");
                return;
            }

            AddressableManager.Instance.LoadAsset<GameObject>(itemPrefabPath + ".prefab", (asset) =>
            {
                if (asset == null)
                {
                    Debug.LogError($"[LuckyGiftActivityIcon] Failed to load item prefab: {itemPrefabPath}");
                    return;
                }

                GameObject itemObj = Instantiate(asset, itemStartPos.transform, false);
                itemObj.transform.localPosition = Vector3.zero;
                LuckyGiftActivityItem item = itemObj.GetComponent<LuckyGiftActivityItem>();
                if (item == null)
                {
                    item = itemObj.AddComponent<LuckyGiftActivityItem>();
                }
                mask.gameObject.SetActive(true);
                int reward = activity.GetRandomReward();
                int slotIndex = itemList.Count;
                itemObj.transform.SetParent(slotTransforms[slotIndex], true);
                item.Initialize(this, reward, slotIndex, activity.ItemScaleDuration, activity.ItemStayDuration);

                itemList.Add(item);
                SaveProgressData();

                Debug.Log($"[LuckyGiftActivityIcon] Item created, current count: {itemList.Count}");
            });
        }
        
        /// <summary>
        /// 加载进度数据并恢复item
        /// </summary>
        private void LoadProgressData()
        {
            LuckyGiftActivityProgressData data = StoreManager.Instance.LoadDataJson<LuckyGiftActivityProgressData>(progressData.fileName);
            if (data == null)
            {
                return;
            }

            progressData.LoadData(data);
            RestoreItems();
        }
        
        /// <summary>
        /// 保存进度数据
        /// </summary>
        private void SaveProgressData()
        {
            progressData.ClearItems();
            foreach (var item in itemList)
            {
                if (item != null)
                {
                    progressData.AddItem(item.GetReward(), item.GetSlotIndex());
                }
            }
            
            progressData.SaveData();
        }
        
        /// <summary>
        /// 恢复item（无需动画，直接显示在对应位置）
        /// </summary>
        private void RestoreItems()
        {
            if (string.IsNullOrEmpty(itemPrefabPath))
            {
                Debug.LogError("[LuckyGiftActivityIcon] ItemPrefab path is empty, cannot restore items");
                return;
            }

            if (progressData.itemList == null || progressData.itemList.Count == 0)
            {
                return;
            }

            AddressableManager.Instance.LoadAsset<GameObject>(itemPrefabPath + ".prefab", (asset) =>
            {
                if (asset == null)
                {
                    Debug.LogError($"[LuckyGiftActivityIcon] Failed to load item prefab: {itemPrefabPath}");
                    return;
                }
                
                foreach (var itemData in progressData.itemList)
                {
                    if (itemData.slotIndex < 0 || itemData.slotIndex >= slotTransforms.Length)
                    {
                        Debug.LogWarning($"[LuckyGiftActivityIcon] Invalid slotIndex: {itemData.slotIndex}, skipping");
                        continue;
                    }
                    
                    if (slotTransforms[itemData.slotIndex] == null)
                    {
                        Debug.LogWarning($"[LuckyGiftActivityIcon] Slot transform at index {itemData.slotIndex} is null, skipping");
                        continue;
                    }
                    
                    GameObject itemObj = Instantiate(asset, slotTransforms[itemData.slotIndex], false);
                    LuckyGiftActivityItem item = itemObj.GetComponent<LuckyGiftActivityItem>();
                    if (item == null)
                    {
                        item = itemObj.AddComponent<LuckyGiftActivityItem>();
                    }

                    item.InitializeWithoutAnimation(this, itemData.reward, itemData.slotIndex);
                    itemObj.transform.localPosition = Vector3.zero;
                    itemObj.transform.localScale = Vector3.one;
                    itemList.Add(item);
                    
                    Debug.Log($"[LuckyGiftActivityIcon] Item restored: reward={itemData.reward}, slotIndex={itemData.slotIndex}");
                }
                
                Debug.Log($"[LuckyGiftActivityIcon] Restored {itemList.Count} items from progress data");
            });
        }

     

        /// <summary>
        /// Item被点击后的回调
        /// </summary>
        public void OnItemClicked(LuckyGiftActivityItem item)
        {
            if (isProcessingItem)
            {
                return;
            }

            isProcessingItem = true;
            currentAdItem = item;
            item.PlayAd();
        }

        /// <summary>
        /// 广告播放成功回调
        /// </summary>
        private void OnAdSuccess(int type)
        {
            if (currentAdItem == null)
            {
                return;
            }
            
            HandleAdResult();
        }

        /// <summary>
        /// 广告播放失败回调（可由外部调用）
        /// </summary>
        public void OnAdFailed(int type)
        {
            if (currentAdItem == null)
            {
                return;
            }
            HandleAdResult();
        }

        /// <summary>
        /// 移除item并重新排列其他item
        /// </summary>
        private void RemoveItem(LuckyGiftActivityItem item)
        {
            if (!itemList.Remove(item))
            {
                return;
            }

            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }

            ReorderItems();
            SaveProgressData();
        }

        /// <summary>
        /// 重新排列item（顺位向前填充）
        /// </summary>
        private void ReorderItems()
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i] != null)
                {
                    itemList[i].SetSlotIndex(i);
                    itemList[i].transform.SetParent(slotTransforms[i], false);
                    itemList[i].transform.localPosition = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// 检查是否可以点击item
        /// </summary>
        public bool CanClickItem()
        {
            return !isProcessingItem;
        }

        /// <summary>
        /// 统一处理广告成功或失败后的奖励逻辑
        /// </summary>
        private void HandleAdResult()
        {
            if (currentAdItem == null)
            {
                return;
            }

            int reward = currentAdItem.GetReward();
            Debug.Log($"[LuckyGiftActivityIcon] Reward granted: {reward}");

            Messenger.Broadcast<int, Action>(GameDialogManager.OpenExtraAwardCashDialogMsg, reward, () =>
            {
                RemoveItem(currentAdItem);
                currentAdItem = null;
                isProcessingItem = false;
            });
        }

        public override void OnDestroy()
        {
            if (activity != null)
            {
                activity.OnActivated -= OnActivityActivated;
            }
            
            foreach (var item in itemList)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            itemList.Clear();
            
            base.OnDestroy();
        }
    }
}

