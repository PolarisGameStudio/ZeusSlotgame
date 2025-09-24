using System.Collections.Generic;
using System.Linq;
using Libs;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace Activity
{
    public class WheelLuckDialog:UIDialog
    {
        
        public GameObject shopItemPrefab;
        
        private Text _cashCount;
        private Button _adButton;
        private Button _closeButton;
        private Button _getButton;
        
        private WheelLuckActivity _activity;

        private readonly Dictionary<int, ShopItem> _shopItems = new Dictionary<int, ShopItem>();

        private bool _init = false;
        
        public void SetUIData(int activityId)
        {
            _activity = ActivityManager.Instance.GetActivityByID(activityId) as WheelLuckActivity;
            OpenUI();
        }

        private void OpenUI()
        {
            if (_init) return;
            CreateItem();
            _init = true;
        }

        private void CreateItem()
        {
            var date = _activity.ShopItems;
            
            foreach (var shopItemData in date)
            {
                if(shopItemData.Key == WheelLuckActivity.RandomShopItemId) continue;
                if(shopItemData.Key == WheelLuckActivity.MoneyShopItemId) continue;
                var shopItemObj = Instantiate(shopItemPrefab,shopItemPrefab.transform.parent);
                shopItemObj.SetActive(true);
                shopItemObj.transform.localPosition = Vector3.zero;
                var shopItem = shopItemObj.AddComponent<ShopItem>();
                shopItem.SetData(shopItemData.Value);
                _shopItems.Add(shopItemData.Key,shopItem);
            }
        }
        
        // on

        protected override void Awake()
        {
            base.Awake();
            
            //_skeletonGraphic = Utilities.RealFindObj<SkeletonGraphic>(transform,"Anchor/Animation/House");
            
        }
   
    }
}