using System.Collections.Generic;
using Libs;
using UnityEngine;

namespace Activity
{
    public class H5RewardActivity:BaseActivity
    {
        private const string TimeIntervalKey = "TimeInterval";
        public int TimeInterval = 0;
       
        public H5RewardActivity(Dictionary<string, object> data) : base(data)
        {
            TimeInterval = Utils.Utilities.GetValue(data, TimeIntervalKey, 0);
        }

        public override void AddListener()
        {
            base.AddListener();
            Messenger.AddListener<bool>(GameConstants.OnH5InitSuccess, UpdateH5);
            Messenger.AddListener(GameConstants.OnExitH5, OnExitH5);
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener<bool>(GameConstants.OnH5InitSuccess, UpdateH5);
            Messenger.RemoveListener(GameConstants.OnExitH5, OnExitH5);
        }

        void UpdateH5(bool isOpen)
        {
            if (!IsOpen())
            {
                return;
            }
            if (icon!=null && icon is H5RewardActivityIcon h5RewardIcon)
            {
                h5RewardIcon.UpdateIcon(isOpen);
            }
        }

        public override bool IsOpen()
        {
            return base.IsOpen()&&PlatformManager.Instance.CheckCanShowH5();
        }

        /// <summary>
        /// 从h5按钮退出
        /// </summary>
        private void OnExitH5()
        {
            if (icon!=null && icon is H5RewardActivityIcon h5RewardIcon)
            {
                h5RewardIcon.OnExitH5();
            }
        }
        
        public override BaseIcon RegisterIcon(GameObject go)
        {
            icon = go.GetComponent<H5RewardActivityIcon>();
            if (icon==null)
            {
                icon = go.AddComponent<H5RewardActivityIcon>();
            }
            icon.OnInit(id,iconData);
            return icon;
        }

        public bool CheckCanShow()
        {
            return isOpen && OnLineEarningMgr.Instance.CanShowH5();
        }

        public override void OnClickIcon()
        {
            base.OnClickIcon();
#if UNITY_ANDROID
            SkySreenUtils.CurrentOrientation = ScreenOrientation.Portrait;
#endif
            int cash = OnLineEarningMgr.Instance.Cash();
            string str = ""+OnLineEarningMgr.Instance.ConvertMoneyToDouble(cash,needExchange:false);
#if UNITY_EDITOR || DEBUG
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["amount"] = 3.5f;
            string mockstr = MiniJSON.Json.Serialize(data);
            PlatformManager.Instance.H5AddCash(mockstr);
            PlatformManager.Instance.H5State("");
#else
            if (OnLineEarningMgr.Instance.isThreeHundredOpen())
            {
                int h5Reward = OnLineEarningMgr.Instance.GetH5Reward();
                string str1 = ""+OnLineEarningMgr.Instance.ConvertMoneyToDouble(h5Reward,needExchange:false);
                PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.ShowPromotion,str,str1);
            }else
            {
                PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.ShowPromotion,str);
            }
#endif
        }
    }
}