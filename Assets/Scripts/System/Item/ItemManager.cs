using System.Resources;
using System.Text;
using Classic;
using Libs;
using UnityEngine;
using ResourceManager = UnityEngine.ResourceManagement.ResourceManager;

namespace System
{
    //加载道具和任务icon等资源，管理道具数据
    public class ItemManager
    {
        private string resourcePath = "Item/";
        private static ItemManager instance;
        public static ItemManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ItemManager();
                }
                return instance;
            }
        }

        private ItemManager()
        {
            //初始化道具数据
        }
        
        public async void GetIcon(string iconName,Action<Sprite> callback)
        {
            Sprite icon = await AddressableManager.Instance.LoadAssetAsync<Sprite>(resourcePath +"TaskIcon/"+ iconName);
            if (icon!=null)
            {
                callback?.Invoke(icon);
            }
        }
        
        //加载task图标
        public async void GetTaskIcon(int taskType,Action<Sprite> callback)
        {
            string iconName = GetTaskIconName(taskType);
            
            Sprite icon = await AddressableManager.Instance.LoadAssetAsync<Sprite>(resourcePath +"TaskIcon/"+ iconName);
            if (icon!=null)
            {
                callback?.Invoke(icon);
            }
        }
        
        //其他方法，如获取道具信息，加载资源等
        private string GetTaskIconName(int taskType)
        {
            StringBuilder name = new StringBuilder();
            switch (taskType)
            {
                case TaskConstants.CollectCashFromZeroTask_Key:
                case TaskConstants.AccumulateCashTask_Key:
                    name.Append("1001");
                    break;
                case TaskConstants.CollectNewCardCountTask_Key:
                case TaskConstants.CollectNewCardTypeCountTask_Key:
                case TaskConstants.CollectCardTask_Key:
                    name.Append("1007");
                    break;
                case TaskConstants.CollectFreeGameTriggerCountTask_Key:
                    name.Append("1002");
                    break;
                case TaskConstants.CollectJackpotGameCountTask_Key:
                    name.Append("1003");
                    break;
                case TaskConstants.CollectTriggerSpinWinCountTask_Key:
                    name.Append("1004");
                    break;
                case TaskConstants.CollectSpinCountTask_Key:
                    name.Append("1005");
                    break;
                case TaskConstants.WatchADTimeTask_Key:
                    name.Append("1006");
                    break;
                case TaskConstants.CollectWildSymbolCountTask_Key:
                    name.Append("1008");
                    break;
                case TaskConstants.CollectSymbolCountTask_Key:
                    name.Append("1009");
                    break;
            }
            return name.ToString();
        }
    }
}