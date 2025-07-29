using System.Collections.Generic;
using System.Linq;
using Classic;
using Libs;
using MarchingBytes;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace CardSystem
{
    public class CardSystemCollectionDialog : UIDialog,LoopScrollDataSource,LoopScrollPrefabSource
    {
        private BaseCardCollect cardCollect;
        [Header("倾斜角度")]
        public int TiltAngle = 15;
        public TextMeshProUGUI tmp_coin;
        public Button btn_collect;
        public TextMeshProUGUI tmp_info;
        public LoopVerticalScrollRect loopScrollRect;
        public Transform content;
        public GameObject itemPrefab;
        // public LocalizedString tmp_Info_Str;
        public TextMeshProUGUI tmp_cardInfo;
        private string poolName = "CardSystemCardItem";
        private List<BaseCard> Cards;
        protected override void Awake()
        {
            base.Awake();
            if (btn_collect!=null)
            {
                UGUIEventListener.Get(btn_collect.gameObject).onClick = BtnCollectClick;
            }
            Cards = CardSystemManager.Instance.GetCardsInfo();
            Cards.Sort((a, b) =>
            {
                // 先判断 Count 是否等于0
                bool aIsZero = a.Count == 0;
                bool bIsZero = b.Count == 0;

                // 先将 Count==0 的放前面
                if (aIsZero != bIsZero)
                    return aIsZero ? -1 : 1;

                // Count都不为0或都为0时，按 Level 排序
                return b.Level.CompareTo(a.Level);
            });

            string info = OnLineEarningMgr.Instance.GetMoneyStr(CardSystemManager.Instance.GetCurCollectionCoins(),0);
            tmp_coin.text = info;
            PoolResourceManager.Instance.InitPool(poolName,itemPrefab,14);
            loopScrollRect.prefabSource = this;
            loopScrollRect.dataSource = this;
            loopScrollRect.totalCount = Cards.Count;
            loopScrollRect.RefillCells();
        }

        public override void Refresh()
        {
            base.Refresh();
            // int cardCount = CardSystemManager.Instance.GetHaveCardTypeCount();
            // tmp_Info_Str.Arguments = new object[]{cardCount};
            // // Debug.Log("CardSystemCollectionDialog tmp_Info_Str"+tmp_Info_Str.GetLocalizedString());
            // tmp_info.text = tmp_Info_Str.GetLocalizedString();
            tmp_cardInfo.text = $"{CardSystemManager.Instance.GetHaveCardTypeCount()}/{CardSystemManager.Instance.GetTotalCardTypeCount()}";
        }

        protected override void BtnCloseClick(GameObject closeBtnObject)
        {
            base.BtnCloseClick(closeBtnObject);
            CardSystemManager.Instance.ClearNewCardIndex();

        }
        private void BtnCollectClick(GameObject closeBtnObject)
        {
            this.Close();
            CardSystemManager.Instance.ShowLotteryDialog();
        }
        #region LoopScrollRect必需实现
        public void ProvideData(Transform transform, int idx)
        {
            CardUI cardUI = transform.GetComponent<CardUI>();
            BaseCard itemData = Cards[idx];
            //释放之前绑定的数据对象
            cardUI.UpdateData(itemData.Index,itemData);
        }

        public GameObject GetObject(int index)
        {
            GameObject go= PoolResourceManager.Instance.GetObjectFromPool(poolName);
            go.AddComponent<CardUI>();
            go.transform.localEulerAngles = new Vector3(0, 0, Random.Range(-TiltAngle, TiltAngle));
            return go;
        }

        public void ReturnObject(Transform trans)
        {
            Destroy(trans.GetComponent<CardUI>());
            PoolResourceManager.Instance.ReturnTransformToPool(trans);
        }
        #endregion
    }
}