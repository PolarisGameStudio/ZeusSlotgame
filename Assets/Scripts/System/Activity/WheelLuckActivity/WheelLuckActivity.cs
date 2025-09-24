using System.Collections.Generic;
using Ads;
using Libs;
using UnityEngine;
using UnityEngine.UI;

namespace Activity
{
    public class ShopItemData
    {
        public int Id;
        public string Icon;
        public string Name;
        public int TargetNum;
        public int CurrentSpinCount;//当前已经旋转到的次数
    }
    
    public class SpinItemData
    {
        public int ShopItemId;
        public Dictionary<int, float> Probability;//概率数组
        
    }

    public enum  WheelLuckAdType
    {
        Spin,
        Bubble,
        MultipleReward
    }

    public class WheelLuckActivity : BaseActivity
    {
        public static bool IsActive = false;
        public static WheelLuckAdType AdType;
        public static GameObject OnClickBubble;
        public static int MoneyShopItemId = -2; //所以商品都达到 “目标值 - 1” 的时候，只返回金钱；
        public static int RandomShopItemId = -1; //随机商品的id;
        private static int _activeSpinCount;
        
        private const string BaseResultChangeSpinTimes = "BASE_RESULT_CHANGE_SPIN_TIMES";
        private const string ShopItemCurrentSpinCount = "SHOPITEM_CURRENR_SPIN_COUNT";
        
        public const string RefreshShopItemCurrentCount = "RefreshShopItemCurrentCount";

        private readonly Dictionary<int, ShopItemData> _shopItems = new Dictionary<int, ShopItemData>();

        private readonly List<SpinItemData> _spinItems = new List<SpinItemData>(8);

        public Dictionary<int, ShopItemData> ShopItems => _shopItems;
        
        public List<SpinItemData> SpinItems => _spinItems;
        
        public static int ActiveId;
        
        private int _minReward;
        private int _maxReward;
        private int _minMultiple;
        private int _maxMultiple;
        
        public WheelLuckActivity(Dictionary<string, object> data) : base(data)
        {
            ParseTaskData();

            CheckActive();
        }
        
        public override void AddListener()
        {
            base.AddListener();
            Messenger.AddListener(GameConstants.DO_SPIN,UpdateSpinCount);
        }
        
        public override void RemoveListener()
        {
            base.RemoveListener();
            Messenger.RemoveListener(GameConstants.DO_SPIN,UpdateSpinCount);
          
        }
        private void UpdateSpinCount()
        {
            CheckActive();
            icon.RefreshProgress(0);
        }
        
        public override void OnClickIcon()
        {
            base.OnClickIcon();
            UIDialog dialog = UIManager.Instance.GetActiveDialog<WheelLuckDialog>();
            if (dialog!=null)
            {
                return;
            }
            Messenger.Broadcast(GameDialogManager.OpenWheelLucyDialogMsg,id);
        }
        
        private void CheckActive()
        {
            if(IsActive) return;
            int allSpinCount = SharedPlayerPrefs.GetPlayerPrefsIntValue(BaseResultChangeSpinTimes);
            IsActive = allSpinCount >= _activeSpinCount;
        }
        
        public override BaseIcon RegisterIcon(GameObject go)
        {
            icon = go.AddComponent<WheelLuckTaskIcon>();
            icon.OnInit(id,iconData);
            return icon;
        }

        protected override sealed void ParseTaskData()
        {
            ActiveId = id;
            
            Dictionary<string, object> taskData = Utils.Utilities.GetValue<Dictionary<string, object>>(Data, ActivityConstants.TASKS, null);
            
            Dictionary<string, object> randomConfig = Utils.Utilities.GetValue<Dictionary<string, object>>(Data, "RandomConfig", null);

            
            RandomShopItemId = Utils.Utilities.GetValue(randomConfig, "RandomItemId", -1);
            
            MoneyShopItemId = Utils.Utilities.GetValue(randomConfig, "MoneyItemId", -2);
            
            var moneyMultiple= Utils.Utilities.GetValue(randomConfig, "MoneyMultiple", "");
           
            string[] multiple = moneyMultiple.Split(",");
            _minMultiple = int.Parse(multiple[0]);
            _maxMultiple = int.Parse(multiple[1]);
           
                
            var moneyRange= Utils.Utilities.GetValue(randomConfig, "MoneyRange", "");
            string[] rewards = moneyRange.Split(",");
            _minReward = int.Parse(rewards[0]);
            _maxReward = int.Parse(rewards[1]);
            
            
            if (taskData == null || taskData.Count == 0)
            {
                return;
            }
            int taskId = Utils.Utilities.GetInt(taskData, TaskConstants.TaskId_Key, 0);

            _activeSpinCount = Utils.Utilities.GetInt(taskData, "activeSpinCount", 0);
            
            Task = TaskManager.Instance.RegisterTask(taskId,taskData);
            
            
            List<object> shopItems = Utils.Utilities.GetValue<List<object>>(Data, "ShopItem",null);

            
            foreach (object item in shopItems)
            {
                Dictionary<string, object> itemDic = item as Dictionary<string, object>;
            
                ShopItemData shopItemData = new ShopItemData
                {
                    Id = Utils.Utilities.GetValue(itemDic, "Id", 0),
                    Icon = Utils.Utilities.GetValue(itemDic, "Icon", ""),
                    Name = Utils.Utilities.GetValue(itemDic, "Name", ""),
                    TargetNum = Utils.Utilities.GetValue(itemDic, "TargetNum", 0)
                };
                shopItemData.CurrentSpinCount = SharedPlayerPrefs.GetPlayerPrefsIntValue(ShopItemCurrentSpinCount + shopItemData.Id);
                _shopItems.TryAdd(shopItemData.Id, shopItemData);
            }
            
            List<object> spinItems = Utils.Utilities.GetValue<List<object>>(Data, "SpinItem",null);

            foreach (object spinItem in spinItems)
            {
                Dictionary<string, object> itemDic = spinItem as Dictionary<string, object>;
                
                SpinItemData spinItemData = new SpinItemData
                {
                    ShopItemId = Utils.Utilities.GetValue(itemDic, "ShopItemId", 0),
                };
                

                List<object> probability = Utils.Utilities.GetValue<List<object>>(itemDic, "Probability", null);

                spinItemData.Probability = new Dictionary<int, float>(probability.Count);

                foreach (var strItem in probability)
                {

                    if (strItem is string proStr)
                    {
                        string[] strArray = proStr.Split(",");
                        if (int.TryParse(strArray[0], out int count) && float.TryParse(strArray[1], out float prob))
                        {
                            // 转换成功，可以安全使用 count 和 prob
                            Debug.Log($"Count: {count}, Probability: {prob}");
                            
                            spinItemData.Probability.Add(count,prob);
                        }
                        else
                        {
                            Debug.LogError("解析失败，请检查输入格式！");
                        }
                    }
                    else
                    {
                        Debug.LogError("不是字符串类型，解析失败，请检查输入格式！");
                    }
                }
                _spinItems.Add(spinItemData);
            }
            
        }

        private readonly List<int> _weightList = new List<int>(100);
        
        public int GetPos()
        {
            _weightList.Clear();

            int allCount = 0;
            var list = new List<int>();//随机槽位所在位置
            for (int i = 0; i < _spinItems.Count; i++)
            {
                var shopItemDate = _spinItems[i];
                if (shopItemDate.ShopItemId == RandomShopItemId)
                {
                    list.Add(i);
                    continue;
                }
                var probability = (int)GetProbability(shopItemDate);
                allCount += probability;
                for (int j = 0; j < probability; j++)
                {
                    _weightList.Add(i);
                }
            }

            if (allCount > 100)
            {
                Debug.LogError("错误，总概率大于100");
                return -1;
            }

            //动态计算随机概率
            var avenge = (100 - allCount) / list.Count;

            foreach (var posIndex in list)
            {
                for (int i = 0; i < avenge; i++)
                {
                    _weightList.Add(posIndex);
                }
            }
            Shuffle(_weightList);
            var randIndex = Random.Range(0, 100);
            
            return _weightList[randIndex];
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                // 生成 [0, i] 之间的随机索引（包含 i）
                int randomIndex = Random.Range(0, i + 1);
            
                // 交换元素
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }

        private float GetProbability(SpinItemData spinItemData)
        {
            var currentSpinCount = _shopItems[spinItemData.ShopItemId].CurrentSpinCount;
            foreach (var kp in spinItemData.Probability)
            {
                var count = kp.Key;
                var probability = kp.Value;
                if (currentSpinCount < count)
                {
                    return probability;
                }
            }
            
            return 0;
        }

        public void AddShopItemCount(int shopItemId, int count)
        {
            if (shopItemId == MoneyShopItemId)
            {
                OnLineEarningMgr.Instance.IncreaseCash(count);
                Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
                Debug.Log("加钱");
                return;
            }
            
            var shopItemData = ShopItems[shopItemId];

            shopItemData.CurrentSpinCount += count;
            
            SharedPlayerPrefs.SetPlayerPrefsIntValue(ShopItemCurrentSpinCount + shopItemData.Id,shopItemData.CurrentSpinCount);
            
            Messenger.Broadcast(RefreshShopItemCurrentCount,shopItemId);
        }

        public int GetRandomShopItem()
        {
            _weightList.Clear();
            foreach (var shopItemsValue in ShopItems.Values)
            {
                if(shopItemsValue.Id == RandomShopItemId) continue;
                if(shopItemsValue.Id == MoneyShopItemId) continue;
                var leftCount = shopItemsValue.TargetNum - shopItemsValue.CurrentSpinCount;
                if (leftCount > 1)
                {
                    for (int i = 0; i < leftCount - 1; i++)
                    {
                        _weightList.Add(shopItemsValue.Id);
                    }
                }
            }
            if (_weightList.Count > 0)
            {
                Shuffle(_weightList);
                var randIndex = Random.Range(0, _weightList.Count);
                return _weightList[randIndex];
            }

            return MoneyShopItemId;//不满足条件，直接获得金钱奖励
        }

        public void LoadIcon(int shopItemId,Image imgCard)
        {
            var shopItemData = ShopItems[shopItemId];
            var path = "WheelLuck/Shop/"+shopItemData.Icon;
            AddressableManager.Instance.LoadAsset<Sprite>(path, asset =>
            {
                if (asset != null)
                {
                    imgCard.sprite = asset;
                }
                else
                {
                    Debug.LogError($"Failed to load card sprite for CardId: {shopItemId}, Path: {path}");
                }
            });
        }

        private const int MaxMultipleCount = 3;
        
        public int GetMultipleCount(int shopItemId)
        {
            if (shopItemId == MoneyShopItemId)
            {
                int multiple = Random.Range(_minMultiple,_maxMultiple+1);
                return multiple;
            }
            var shopItemDate = _shopItems[shopItemId];

            var leftCount = shopItemDate.TargetNum - shopItemDate.CurrentSpinCount;

            if (leftCount > 2)
            {
                var count = Random.Range(2,Mathf.Min(MaxMultipleCount + 1, leftCount));
                return count;
            }
            
            return 1;
        }

        public int GetRandomReward()
        {
            return Random.Range(_minReward,_maxReward);
        }

    }
}