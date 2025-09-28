using System.Collections.Generic;
using Libs;
using UnityEngine;

namespace System.BuffSystem
{
    public class BuffManager : MonoSingleton<BuffManager>
    {
        private BuffProgressData _progressData = new BuffProgressData();

        //所有buff字典
        public Dictionary<int, BaseBuff> _allBuffDic = new Dictionary<int, BaseBuff>();

        public void OnInit()
        {
            LoadProgressData();
            AddListener();
        }

        public override void Dispose()
        {
            base.Dispose();
            RemoveListener();
        }

        #region 保存恢复数据

        private void LoadProgressData()
        {
            BuffProgressData data = StoreManager.Instance.LoadDataJson<BuffProgressData>(_progressData.fileName);
            if (data != null)
            {
                _allBuffDic = data.BaseBuffs;
                _progressData.LoadData(_progressData);
            }
        }

        public void SaveProgressData()
        {
            _progressData.SaveData();
        }

        #endregion

        #region 监听

        void AddListener()
        {
            // Messenger.AddListener<BaseBuff>(BuffConstant.OnBuffChange, OnBuffChange);
            Messenger.AddListener<int>(BuffConstant.OnBuffActive, OnBuffActive);
            Messenger.AddListener<int>(BuffConstant.OnBuffDeActive, OnBuffDeActive);
            Messenger.AddListener<int>(BuffConstant.OnBuffTrigger, OnBuffTrigger);
        }

        void RemoveListener()
        {
            // Messenger.RemoveListener<BaseBuff>(BuffConstant.OnBuffChange, OnBuffChange);
            Messenger.RemoveListener<int>(BuffConstant.OnBuffActive, OnBuffActive);
            Messenger.RemoveListener<int>(BuffConstant.OnBuffDeActive, OnBuffDeActive);
            Messenger.RemoveListener<int>(BuffConstant.OnBuffTrigger, OnBuffTrigger);
        }

        void OnBuffChange(BaseBuff buff)
        {
        }

        void OnBuffActive(int buffId)
        {
            BaseBuff buff = GetBuffById(buffId);
            if (buff != null)
            {
                buff.OnActive();
            }
        }

        void OnBuffDeActive(int buffId)
        {
            BaseBuff buff = GetBuffById(buffId);
            if (buff != null)
            {
                buff.OnDeactivate();
            }
        }

        void OnBuffTrigger(int buffId)
        {
            BaseBuff buff = GetBuffById(buffId);
            if (buff != null)
            {
                buff.OnTrigger();
            }
        }
        #endregion
        
        #region 接口
        public List<BaseBuff> GetBuffByType(int buffType)
        {
            List<BaseBuff> buffs = new List<BaseBuff>();
            foreach (var item in _allBuffDic)
            {
                if (item.Value.buffType == buffType)
                {
                    buffs.Add(item.Value);
                }
            }
            return buffs;
        }
        public List<BaseBuff> GetActiveBuffByType(int buffType)
        {
            List<BaseBuff> buffs = new List<BaseBuff>();
            foreach (var item in _allBuffDic)
            {
                if (item.Value.buffType == buffType && item.Value.isActive)
                {
                    buffs.Add(item.Value);
                }
            }
            return buffs;
        }
        public BaseBuff GetBuffById(int buffId)
        {
            if (_allBuffDic.ContainsKey(buffId))
            {
                return _allBuffDic[buffId];
            }
            return null;
        }
        
        public BaseBuff CreateBuff(Dictionary<string,object> dict)
        {
            BaseBuff buff = BuffFactory.CreateBuff(dict);
            BaseBuff savedBuff = GetBuffById(buff.buffId);
            if (buff!=null )
            {
                if (savedBuff!=null)
                {
                    buff.CloneBuff(savedBuff);
                }
                _allBuffDic[buff.buffId] = buff;
            }
            return buff;
        }

        private void Update()
        {
            foreach (var item in _allBuffDic)
            {
                if (item.Value.isActive)
                {
                    item.Value.OnUpdate(Time.deltaTime);
                }
            }
        }
        
        public void SetBuffInit(int buffId)
        {
            BaseBuff buff = GetBuffById(buffId);
            if (buff != null && !buff.isInitialized)
            {
                buff.IsInitialized();
            }
        }
        #endregion
    }
}