using System;
using System.Collections.Generic;
using System.IO;
using Classic;
using Libs;
using UnityEngine;

namespace CardSystem
{
    [Serializable]
    public class CardSystemProgressData: ProgressDataBase<CardSystemProgressData>
    {
        public string fileName = "CardSystemProgressData";
        //卡牌系统的展示条件，需要满足spin20次后才显示
        public bool isFirstShow = true; //是否第一次展示卡牌系统
        //当前轮转到的spin序号          
        public int currentSpinIndex = 0;
        //当前持有的卡牌
        public Dictionary<int, int> currentCards = new Dictionary<int, int>();
        
        public override void LoadData(CardSystemProgressData progressData)
        {
            if (progressData == null)
            {
                Debug.LogError("CardSystemProgressData is null");
                return;
            }
            currentSpinIndex = progressData.currentSpinIndex;
            currentCards = progressData.currentCards;
            isFirstShow = progressData.isFirstShow;
        }

        public override void SaveData()
        {
            currentSpinIndex = CardSystemManager.Instance.CurrentSpinIndex;
            currentCards = CardSystemManager.Instance.GetCurrentCards();  
            isFirstShow = CardSystemManager.Instance.isFirstShow;
            StoreManager.Instance.SaveDataJson(fileName,this);
        }

        public override void ClearData()
        {
            StoreManager.Instance.DeleteProgress(fileName);
        }
        
        public bool HasCard(int cardId)
        {
            return currentCards.ContainsKey(cardId) && currentCards[cardId] > 0;
        }
    }
}