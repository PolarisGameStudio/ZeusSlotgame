using System.Collections.Generic;
using UnityEngine.UI;

namespace Activity
{
    public class WheelLuckTaskIcon:BaseIcon
    {
        private Button _clickButton;
        
        WheelLuckActivity activity;
        
        public override void OnInit(int id, Dictionary<string, object> data)
        {
            base.OnInit(id, data);
            
            activity = ActivityManager.Instance.GetActivityByID(activityId) as WheelLuckActivity;
            
            _clickButton = GetComponent<Button>();
            if (_clickButton!=null)
            {
                _clickButton.onClick.AddListener(OnButtonClick);
            }
            
            RefreshIcon();
        }
        private void RefreshIcon()
        {
            _clickButton.gameObject.SetActive(WheelLuckActivity.IsActive);
        }
        
        private void OnButtonClick()
        {
            ActivityManager.Instance.OnClickIcon(activityId);
        }
        
        public override void RefreshProgress(float progress,string info=null)
        {
            RefreshIcon();
        }
        
    }
    
    
    
}