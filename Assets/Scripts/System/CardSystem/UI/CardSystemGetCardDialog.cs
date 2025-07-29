using System.Collections.Generic;
using DG.Tweening;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace CardSystem
{
    public class CardSystemGetCardDialog:UIDialog   
    {
        private int CardId;
        private Button btn_cardPack;
        private Button btn_collect;
        private Transform item;
        private GameObject parentPrefab;
        protected override void Awake()
        {
            base.Awake();
            btn_cardPack = Util.FindObject<Button>(this.transform,"Anchor/btn_cardPack");
            btn_collect = Util.FindObject<Button>(this.transform,"Anchor/btn_collect");
            item = Util.FindObject<Transform>(this.transform,"Anchor/content/item_card");
            if (btn_cardPack!=null)
            {
                UGUIEventListener.Get(btn_cardPack.gameObject).onClick = OnButtonClickHandler;
            }
            if (btn_collect!=null)
            {
                UGUIEventListener.Get(btn_collect.gameObject).onClick = OnButtonClickHandler;
            }
        }

        public void SetCardData(int cardId,GameObject parent)
        {
            CardId = cardId;
            parentPrefab = parent;
            ShowCardItem();
        }

        private void ShowCardItem()
        {
            if (item != null)
            {
                // Assuming CardSystemManager has a method to create card item UI
                Transform img_new = Util.FindObject<Transform>(item.transform, "img_new");
                if (img_new != null)
                {
                    img_new.gameObject.SetActive(CardSystemManager.Instance.IsNewCard(CardId));
                }
                int level = CardSystemManager.Instance.GetCardLevel(CardId);
                Transform img_xing = Util.FindObject<Transform>(item.transform, "img_xing");
                //一颗星星不显示
                // if (level > 1)
                // {
                    // 更新星星数量
                    for (int j = 1; j <= level; j++)
                    {
                        GameObject star = Util.FindObject<GameObject>(img_xing.transform, "" + j);
                        star.SetActive(true);
                    }
                // }
                GameObject img_cardbg = Util.FindObject<GameObject>(item.transform, "img_bg/" + level);
                img_cardbg.SetActive(true);
               
                // int cardCount = CardSystemManager.Instance.GetCardCount(CardId);
                // if (cardCount > 1)
                // {
                //     Transform img_redPoint = Util.FindObject<Transform>(item.transform, "img_redPoint");
                //     TextMeshProUGUI text_count = Util.FindObject<TextMeshProUGUI>(item.transform, "img_redPoint/tmp_count");
                //     img_redPoint.gameObject.SetActive(true);
                //     text_count.text = cardCount.ToString();
                // }
                //加载卡牌图标
                Transform img_card = Util.FindObject<Transform>(item.transform, "img_card");
                img_card.gameObject.SetActive(false);
                LoadCardItem();
                CardSystemManager.Instance.CollectCard(CardId);
            }
            else
            {
                Debug.LogError("Item transform is not set.");
            }
        }
        
        void LoadCardItem()
        {
            string cardName = "Cards/"+CardSystemManager.Instance.GetCardName(CardId);
            AddressableManager.Instance.LoadAsset<Sprite>(cardName, (asset) =>
            {
                if (asset != null)
                {
                    Image img_card = Util.FindObject<Image>(item.transform, "img_card");
                    img_card.sprite = asset;
                    img_card.SetNativeSize();
                    img_card.gameObject.SetActive(true);
                }
                else
                {
                   Debug.LogError($"Failed to load card sprite for CardId: {CardId}, CardName: {cardName}");
                }
            });
        }
        
        public override void OnButtonClickHandler(GameObject go)
        {
            base.OnButtonClickHandler(go);
            if (go == btn_cardPack.gameObject)
            {
                // Handle card pack button click
                Debug.Log("Card Pack button clicked");
                CardSystemManager.Instance.ShowCollectionDialog();
            }
            else if (go == btn_collect.gameObject)
            {
                // Handle collect button click
                this.Close();
            }
        }

        protected override void BtnCloseClick(GameObject closeBtnObject)
        {
            base.BtnCloseClick(closeBtnObject);
            Messenger.Broadcast(CardSystemConstants.RefreshLotteryMsg);
        }

        public override void Close()
        {
            this.gameObject.SetActive(false);
            base.Close();
            //卡牌飞至卡包按钮的位置
            if (parentPrefab != null)
            {
                item.SetParent(parentPrefab.transform, true);
                item.gameObject.SetActive(true);
                item.transform.DOLocalMove(Vector3.zero,0.5f).SetEase(Ease.Linear);
                item.transform.DOScale(Vector3.zero,0.5f).SetEase(Ease.InQuart).OnComplete(() =>
                {
                    item.gameObject.SetActive(false);
                    Messenger.Broadcast(CardSystemConstants.RefreshLotteryMsg);
                });
            }
        }
        
        // Vector2数组转Vector3数组（Z轴固定为0）
        private Vector3[] ConvertToVector3Array(Vector2[] path2D)
        {
            Vector3[] path3D = new Vector3[path2D.Length];
            for (int i = 0; i < path2D.Length; i++)
            {
                path3D[i] = new Vector3(path2D[i].x, path2D[i].y, 0);
            }
            return path3D;
        }
        
    }
}