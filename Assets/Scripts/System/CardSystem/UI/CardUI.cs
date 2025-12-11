using System;
using DG.Tweening;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace CardSystem
{
    public class CardUI: MonoBehaviour
    {
        private int CardId;
        private BaseCard Card;
        private Transform img_xing;
        private Transform img_new;
        private Transform img_redPoint;
        private TextMeshProUGUI text_count;
        private Image img_card;
        private Image img_card_gray;
        private Transform img_bg;
        private Transform img_normalBtn;
        private Transform img_unnormalBtn;
        
        private void Awake()
        {
            img_xing = Util.FindObject<Transform>(this.transform, "img_xing");
            img_new = Util.FindObject<Transform>(this.transform, "img_new");
            img_redPoint = Util.FindObject<Transform>(transform, "img_redPoint");
            text_count = Util.FindObject<TextMeshProUGUI>(transform, "img_redPoint/tmp_count");
            img_card = Util.FindObject<Image>(this.transform, "img_card");
            img_card_gray = Util.FindObject<Image>(this.transform, "img_card_gray");
            img_bg = Util.FindObject<Transform>(this.transform, "img_bg");
            img_normalBtn =  Util.FindObject<Transform>(this.transform, "btn_collect/bg_normal");
            Reset(); // 初始化时重置数据
        }

        public void UpdateData(int cardId,BaseCard card)
        {
            CardId = cardId;
            Card = card;
            ShowUI();
        }
        
        private void Reset()
        {
            CardId = -1; // 默认值，表示未设置
            Card = null; // 默认值，表示未设置
            //重置星星
            for (int j = 1; j <= 3; j++)
            {
                GameObject star = Util.FindObject<GameObject>(img_xing.transform, "" + j);
                star.SetActive(false);
                GameObject bg = Util.FindObject<GameObject>(img_bg.transform, "" + j);
                bg.SetActive(false);
            }
            img_new.gameObject.SetActive(false);
            img_card.gameObject.SetActive(false);
            img_card_gray.gameObject.SetActive(false);
            img_redPoint.gameObject.SetActive(false);
        }
        public void ShowUI()
        {
            if (CardId < 0)
            {
                Debug.LogError("CardUI: card is null");
                return;
            }
            
            // Assuming CardSystemManager has a method to create card item UI
            // Transform img_new = Util.FindObject<Transform>(this.transform, "img_new");
            if (img_new != null)
            {
                img_new.gameObject.SetActive(CardSystemManager.Instance.CheckCollectionNewCard(CardId));
            }
            //根据等级显示星星
            int level = CardSystemManager.Instance.GetCardLevel(CardId);
            // 更新星星数量
            for (int j = 1; j <= level; j++)
            {
                GameObject star = Util.FindObject<GameObject>(img_xing.transform, "" + j);
                star.SetActive(true);
            }

            DOVirtual.DelayedCall(0.1f, () =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(img_xing.GetComponent<RectTransform>());
            });
            
            Util.FindObject<Transform>(this.transform, "btn_collect/bg_normal");
            
            GameObject img_bg_1 = Util.FindObject<GameObject>(this.transform, "img_bg/" + level);
            img_bg_1.SetActive(true);

            //根据数量显示图标
            int cardCount = CardSystemManager.Instance.GetCardCount(CardId);
            if (cardCount >= 1)
            {
                //加载卡牌图标
                LoadCardItem();
                if (cardCount > 1)
                {
                    img_redPoint.gameObject.SetActive(true);
                    text_count.text = cardCount.ToString();
                }
                else 
                {
                    //如何显示 new 的图标
                }
            }
            else
            {
                LoadCardGray();
            }
        }

        void LoadCardGray()
        {
            string cardNameBg = "Cards/"+CardSystemManager.Instance.GetCardName(CardId)+"_gray";
            AddressableManager.Instance.LoadAsset<Sprite>(cardNameBg, (asset) =>
            {
                if (asset != null)
                {
                    img_card_gray.sprite = asset;
                    img_card_gray.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError($"Failed to load card sprite for CardId: {CardId}, CardName: {cardNameBg}");
                }
            });
        }
        
        void LoadCardItem()
        {
            string cardName = "Cards/"+CardSystemManager.Instance.GetCardName(CardId);
            AddressableManager.Instance.LoadAsset<Sprite>(cardName, (asset) =>
            {
                if (asset != null)
                {
                    img_card.sprite = asset;
                    img_card.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError($"Failed to load card sprite for CardId: {CardId}, CardName: {cardName}");
                }
            });
        }
    }
}