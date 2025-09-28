using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Ads
{
    /// <summary>
    /// 广告投放节点
    /// </summary>
    [Serializable]
    public class BaseAdNode
    {
        /// 广告节点名称
        string _name;
        /// 广告条件
        private ADCondition _adCondition;
        /// 广告投放倍数
        private int _multiple=1;
        
        private int _adType = 0; //广告类型，0为激励，1为插屏，
        
        public int AdType
        {
            get { return _adType; }
            set { _adType = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        
        public ADCondition AdCondition
        {
            get { return _adCondition; }
            set { _adCondition = value; }
        }
        public int Multiple
        {
            get { return _multiple; }
            set { _multiple = value; }
        }
        
        public BaseAdNode(string name,Dictionary<string,object>data,ADCondition adCondition)
        {
            _adType = Utils.Utilities.GetInt(data, ADConstants.AdType, 0);
            _multiple = Utils.Utilities.GetInt(data, ADConstants.Multiple, 0);
            _name = name;
            _adCondition = adCondition;
        }

        protected BaseAdNode()
        {
            throw new NotImplementedException();
        }

        public virtual void ResetCondition()
        {
            if (_adCondition != null)
            {
                _adCondition.ResetCondition();
            }
        }
        
        public virtual void UpdateCondition()
        {
            if (_adCondition != null)
            {
                _adCondition.UpdateCondition();
            }
        }
        
        public virtual bool IsMeetCondition()
        {
            if (_adCondition != null)
            {
                return _adCondition.isMeetCondition();
            }
            return true;
        }
        
        public virtual void AddListener()
        {
            // 可以在这里添加监听器
        }

        public virtual void RemoveListener()
        {
            // 可以在这里添加监听器
        }
        public virtual void DoAction()
        {
            
        }

        public virtual void PlayAd()
        {
            ADManager.Instance.StartPlayAD(Name, AdType);
        }
        
        public virtual int GetMultiple()
        {
            return _multiple;
        }
    }
}
   
