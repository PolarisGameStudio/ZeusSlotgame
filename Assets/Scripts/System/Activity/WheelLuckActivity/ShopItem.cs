using System;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Utils;
namespace Activity
{
    public class ShopItem:MonoBehaviour
    {
        private ShopItemData _data;
        
        private Image _icon;

        private TextMeshProUGUI _name;

        private Text _progress;

        private WheelLuckActivity _activity;
        public void SetData(ShopItemData data)
        {
            _data = data;
            
            _icon = transform.RealFindObj<Image>("Icon");
            _progress= transform.RealFindObj<Text>("Progress");
            
            _name = transform.RealFindObj<TextMeshProUGUI>("Name");
            
            _activity = ActivityManager.Instance.GetActivityByID(WheelLuckActivity.ActiveId) as WheelLuckActivity;

            _activity?.LoadIcon(_data.Id, _icon);

            _progress.text = $"{_data.CurrentSpinCount}/{_data.TargetNum}";

            var localizedString = new LocalizedString(LocalizationManager.Instance.tableName, _data.Name);
            
            _name.text = localizedString.GetLocalizedString();
        }

        private void OnEnable()
        {
            Messenger.AddListener<int>(WheelLuckActivity.RefreshShopItemCurrentCount,RefreshTextCount);
        }

        private void OnDisable()
        {
            Messenger.RemoveListener<int>(WheelLuckActivity.RefreshShopItemCurrentCount,RefreshTextCount);
        }

        private void RefreshTextCount(int shopItemId)
        {
            if(shopItemId != _data.Id) return;
            _progress.text = $"{_data.CurrentSpinCount}/{_data.TargetNum}";
        }
    }
}