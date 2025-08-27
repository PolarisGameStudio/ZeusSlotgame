using System.Collections.Generic;
using CardSystem;
using Libs;
using UnityEngine;
using Utils;

namespace System
{
    [Obsolete("no use")]
    public class RecordItemData
    {
        public RecordItem itemUI;
        private Dictionary<string, object> data = new Dictionary<string, object>();
        public int index;
        private GameObject prefab;
        //平台序号
        public int platSpIndex = 0;
        public int curProgress = 0;
        public int targetProgress = 0;
        public int cash = 0;
        public int taskId;
        public RecordItemData(Dictionary<string, object> config)
        {
            data = config;
            index = Utilities.GetInt(config, "index", -1);
            cash = Utilities.GetInt(config, "cash", 0);
            platSpIndex = Utilities.GetInt(config, "platSpIndex", -1);
            taskId =  Utilities.GetInt(config, "taskId", -1);
        }
        
        public void OnInit(RecordItem item)
        {
            BindUI(item);
            curProgress = CardSystemManager.Instance.GetHaveCardTypeCount();
            targetProgress = CardSystemManager.Instance.GetTotalCardTypeCount();
        }
        
        public void BindUI(RecordItem item)
        {
            itemUI = item;
        }
        
        public void UnBindUI()
        {
            itemUI = null;
        }
    }
}