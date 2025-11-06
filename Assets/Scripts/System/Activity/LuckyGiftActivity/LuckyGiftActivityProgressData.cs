using System;
using System.Collections.Generic;
using Libs;
using UnityEngine;

namespace Activity
{
    [Serializable]
    public class LuckyGiftActivityItemData
    {
        public int reward;
        public int slotIndex;
        
        public LuckyGiftActivityItemData()
        {
        }
        
        public LuckyGiftActivityItemData(int reward, int slotIndex)
        {
            this.reward = reward;
            this.slotIndex = slotIndex;
        }
    }
    
    [Serializable]
    public class LuckyGiftActivityProgressData : ProgressDataBase<LuckyGiftActivityProgressData>
    {
        public string fileName = "LuckyGiftActivityProgressData";
        
        // 当前持有的item列表数据
        public List<LuckyGiftActivityItemData> itemList = new List<LuckyGiftActivityItemData>();
        
        public override void LoadData(LuckyGiftActivityProgressData progressData)
        {
            if (progressData == null)
            {
                Debug.LogError("[LuckyGiftActivityProgressData] progressData is null");
                return;
            }
            
            itemList = progressData.itemList ?? new List<LuckyGiftActivityItemData>();
            Debug.Log($"[LuckyGiftActivityProgressData] Loaded {itemList.Count} items");
        }

        public override void SaveData()
        {
            StoreManager.Instance.SaveDataJson(fileName, this);
        }

        public override void ClearData()
        {
            itemList.Clear();
            StoreManager.Instance.DeleteProgress(fileName);
        }
        
        /// <summary>
        /// 添加item数据
        /// </summary>
        public void AddItem(int reward, int slotIndex)
        {
            itemList.Add(new LuckyGiftActivityItemData(reward, slotIndex));
        }
        
        /// <summary>
        /// 移除指定索引的item
        /// </summary>
        public void RemoveItemAt(int index)
        {
            if (index >= 0 && index < itemList.Count)
            {
                itemList.RemoveAt(index);
            }
        }
        
        /// <summary>
        /// 清空所有item
        /// </summary>
        public void ClearItems()
        {
            itemList.Clear();
        }
    }
}

