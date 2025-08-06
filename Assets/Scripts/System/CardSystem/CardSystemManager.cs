using System;
using System.Collections.Generic;
using System.Linq;
using Activity;
using CardSystem.Activity;
using Libs;
using UnityEngine;

namespace CardSystem
{
    public class CardSystemManager:MonoSingleton<CardSystemManager>
    {
        private List<BaseWeightCondition> baseWeightLists = new List<BaseWeightCondition>();
        private List<CardsReel> baseCardsGroupLists = new List<CardsReel>();
        private List<BaseCard> cardsInfo= new List<BaseCard>();
        private Dictionary<string,BaseCardCollect> baseCardCollects = new Dictionary<string,BaseCardCollect>();

        public CardSystemProgressData progressData = new CardSystemProgressData();
        private bool isOpen = false;
        private int EndTime = 0;
        public int CurrentSpinIndex { get; private set; } = 0; //当前轮转到的spin序号
        private Dictionary<int,int> currentCards = new Dictionary<int,int>(); //当前持有的卡牌，key为卡牌ID，value为数量
        //本次session新卡牌索引列表
        private List<int> newCardIndex = new List<int>();
        private CardLotteryActivity cardLotteryActivity;
        private CardPackActivity cardPackActivity;

        private List<int> lotteryUI = new List<int>();

        public string curCollectionName = "collection1";
        public bool isFirstShow = true; //是否第一次展示卡牌系统
        private int spinCount = 0; //当前转动次数
        private int spinLimit = 20; //转动次数限制,开启卡牌系统
        public void OnInit()
        {
            ParsePlist();
            LoadResource();
            AddListener();
        }

        void ParsePlist()
        {
            try
            {
                Dictionary<string, object> plist = Plugins.Configuration.GetInstance().GetValue<Dictionary<string,object>>(CardSystemConstants.CardSystem,null);
                if (plist==null)
                {
                    Debug.LogError("plist is null");
                }

                EndTime = Utils.Utilities.GetInt(plist, CardSystemConstants.EndTime, 0);
                isOpen =  Utils.Utilities.GetBool(plist, CardSystemConstants.Open, false);
                
                if (!IsOpen())
                {
                    Debug.LogError($"CardSystem is not open isOpen:{isOpen} EndTime:{EndTime}");
                    return;
                }
                
                spinLimit = Utils.Utilities.GetInt(plist, CardSystemConstants.SpinLimit, 0);
                //解析卡牌
                ParseCards(plist);
                if (cardsInfo == null || cardsInfo.Count == 0)
                {
                    Debug.LogError("CardSystemManager ParseCards cardsInfo is null or empty");
                    return;
                }
                Dictionary<string, object> lotteryConfig = Utils.Utilities.GetValue<Dictionary<string,object>>(plist, CardSystemConstants.LotteryConfig, null);
                //先解析卡牌权重组
                ParseWeightGroup(lotteryConfig);
                //再解析条件转轮组 顺序不可更改
                ParseSpinGroup(lotteryConfig);
                //解析UIConfig
                ParseUIConfig(lotteryConfig);
                
                //加载进度数据
                LoadProgressData();
                
                //创建活动，注册活动入口
                CreateCardActivity(plist);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        void ParseCards(Dictionary<string, object> config)
        {
            try
            {
                Dictionary<string,object> cardsList = Utils.Utilities.GetValue<Dictionary<string,object>>(config, CardSystemConstants.CardInfo, null);
                if (cardsList == null || cardsList.Count == 0)
                {
                    Debug.LogError("CardSystemManager ParseCards cardsList is null or empty");
                    return;
                }
                foreach (var item in cardsList)
                {
                    string name = item.Key;
                    Dictionary<string, object> itemConfig = item.Value as Dictionary<string, object>;
                    if (itemConfig == null || itemConfig.Count == 0)
                    {
                        Debug.LogError($"CardSystemManager ParseCards itemConfig is null or empty for card: {name}");
                        continue;
                    }

                    Dictionary<string, object> cards =
                        Utils.Utilities.GetValue<Dictionary<string, object>>(itemConfig, CardSystemConstants.Cards,
                            null);
                    if (cards == null || cards.Count == 0)
                    {
                        Debug.LogError($"CardSystemManager ParseCards cards is null or empty for card: {name}");
                        continue;
                    }
                    
                    //创建BaseCardCollect对象
                    BaseCardCollect baseCardCollect = new BaseCardCollect(name, Utils.Utilities.GetInt(itemConfig, CardSystemConstants.Coins, 0), cards);
                    if (baseCardCollect == null || baseCardCollect.Cards == null || baseCardCollect.Cards.Count == 0)
                    {
                        Debug.LogError($"CardSystemManager ParseCards baseCardCollect is null or empty for card: {name}");
                        continue;
                    }
                    //将卡包信息添加到baseCardCollects字典中
                    baseCardCollects[name] = baseCardCollect;
                    //将卡牌信息添加到cardsInfo列表中
                    foreach (var card in baseCardCollect.Cards)
                    {
                        if (card != null && !cardsInfo.Exists(c => c.Index == card.Index))
                        {
                            cardsInfo.Add(card);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        void ParseWeightGroup(Dictionary<string, object> config)
        {
            try
            {
                List<object> weightList = Utils.Utilities.GetValue<List<object>>(config, CardSystemConstants.WeightGroup, null);
                foreach (var item in weightList)
                {
                    Dictionary<string, object> itemConfig = item as Dictionary<string, object>;
                    string name = Utils.Utilities.GetString(itemConfig, CardSystemConstants.Name, string.Empty);
                    List<object> cards = Utils.Utilities.GetValue<List<object>>(itemConfig, CardSystemConstants.CardReel, null);
                    CardsReel cardsReel = new CardsReel(name,cards);
                    baseCardsGroupLists.Add(cardsReel);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        void ParseSpinGroup(Dictionary<string,object> config)
        {
            try
            {
                List<object> weightList = Utils.Utilities.GetValue<List<object>>(config, CardSystemConstants.SpinGroup, null);
                foreach (var item in weightList)
                {
                    Dictionary<string, object> itemConfig = item as Dictionary<string, object>;
                    BaseWeightCondition weightCondition = CreateWeightCondition(itemConfig);
                    weightCondition.Init(itemConfig);
                    baseWeightLists.Add(weightCondition);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        BaseWeightCondition CreateWeightCondition(Dictionary<string,object> config)
        {
            try
            {
                BaseWeightCondition weightCondition = null;
                Dictionary<string,object> conditions = Utils.Utilities.GetValue<Dictionary<string,object>>(config, CardSystemConstants.Condition, null);
                int type = Utils.Utilities.GetInt(conditions, CardSystemConstants.type, -1);
                switch (type)
                {
                    case (int)ConditionType.CostCoin:
                        weightCondition = new CostCoinsCondition();
                        break;
                    case (int)ConditionType.AD:
                        weightCondition = new ADCondition();
                        break;
                    default:
                        weightCondition = new FreeGameCondition();
                        break;
                }

                return weightCondition;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
       
        
        void CreateCardActivity(Dictionary<string, object> config)
        {
            try
            {
                Dictionary<string,object> activityData = Utils.Utilities.GetValue<Dictionary<string,object>>(config, ActivityConstants.ACTIVITY, null);
                if (activityData == null || activityData.Count == 0)
                {
                    Debug.LogError("CardSystemManager CreateCardActivity activityData is null");
                    return;
                }
                Dictionary<string,object> lotteryData = Utils.Utilities.GetValue<Dictionary<string,object>>(activityData, CardSystemConstants.CardLottery, null);
                if (lotteryData != null && lotteryData.Count > 0)
                {
                    cardLotteryActivity = ActivityManager.Instance.RegisterActivity(lotteryData) as CardLotteryActivity;
                    if (cardLotteryActivity == null)
                    {
                        Debug.LogError("CardSystemManager CreateCardActivity cardLotteryActivity is null");
                        return;
                    }    
                }
                else
                {
                    Debug.LogError("CardSystemManager CreateCardActivity lotteryData is null");
                }
                Dictionary<string,object> packData = Utils.Utilities.GetValue<Dictionary<string,object>>(activityData, CardSystemConstants.CardPack, null);
                if (packData != null && packData.Count > 0)
                {
                    cardPackActivity = ActivityManager.Instance.RegisterActivity(packData) as CardPackActivity;
                    if (cardPackActivity == null)
                    {
                        Debug.LogError("CardSystemManager CreateCardActivity cardPackActivity is null");
                       
                    }    
                }
                else
                {
                    Debug.LogError("CardSystemManager CreateCardActivity packData is null");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        void ParseUIConfig(Dictionary<string,object> lotteryConfig)
        {
            try
            {
                List<object> uiConfig =Utils.Utilities.GetValue<List<object>>(lotteryConfig, CardSystemConstants.UIConfig, null);
                if (uiConfig == null)
                {
                    Debug.LogError("CardSystemManager ParseUIConfig uiConfig is null");
                    return;
                }
                // 这里可以解析UI配置
                // 例如：UIManager.Instance.RegisterUI(uiConfig);
                foreach (var item in uiConfig)
                {
                   int index= (int)item;
                   lotteryUI.Add(index);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        void LoadResource()
        {
            
        }

        #region LifeCycle
        
        void AddListener()
        {
            if (isFirstShow)
            {
                Messenger.AddListener(SlotControllerConstants.OnSpinEnd,UpdateSpinCount);
            }
        }
        
        void RemoveListener()
        {
            Messenger.RemoveListener(SlotControllerConstants.OnSpinEnd,UpdateSpinCount);
        }
        
        public bool IsOpen()
        {
            if (!isOpen)
            {
                return false;
            }

            return CheckTime();
        }

        public bool CheckTime()
        {
            if (EndTime<=0)
            {
                return true;
            }

            long nowTime = TimeUtils.ConvertDateTimeLong(DateTime.Now);
            return nowTime < EndTime;
        }
        
        public void Reset()
        {
            baseWeightLists.Clear();
            baseCardsGroupLists.Clear();
            isOpen = false;
            EndTime = 0;
            CurrentSpinIndex = 0;
        }

        void UpdateSpinCount()
        {
            spinCount++;
            Debug.Log("CardSystemManager UpdateSpinCount spinCount: " + spinCount);
            if (spinCount >= spinLimit)
            {
                // 达到转动次数限制，重置转动次数
                Messenger.RemoveListener(SlotControllerConstants.OnSpinEnd,UpdateSpinCount);
                // 这里可以添加其他逻辑，比如通知UI更新等
                // if(CheckCurWeightCondition())
                // {
                    // if (cardLotteryActivity!=null)
                    // {
                    //     cardLotteryActivity.ShowHand();
                    // }
                // }
                isFirstShow = false;
                ShowActivityIcon();
                ShowLotteryDialog();
            }
        }

        void ShowActivityIcon()
        {
            if (cardLotteryActivity!=null)
            {
                cardLotteryActivity.CheckShowIcon();
            }
            if (cardPackActivity!=null)
            {
                cardPackActivity.CheckShowIcon();
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            RemoveListener();
        }

        bool CheckCurWeightCondition()
        {
            if (CurrentSpinIndex < 0 || CurrentSpinIndex >= baseWeightLists.Count)
            {
                Debug.LogError($"CheckCurWeightCondition CurrentSpinIndex is out of range: {CurrentSpinIndex}");
                return false;
            }
            BaseWeightCondition currentWeight = baseWeightLists[CurrentSpinIndex];
            if (currentWeight == null)
            {
                Debug.LogError($"CheckCurWeightCondition currentWeight is null for index: {CurrentSpinIndex}");
                return false;
            }
            return currentWeight.CheckCondition();
        }
        
        #endregion LifeCycle

        #region LoadAndSaveData
        private void LoadProgressData()
        {
            try
            {
                CardSystemProgressData data = StoreManager.Instance.LoadDataJson<CardSystemProgressData>(progressData.fileName);
                if (data!=null)
                {
                    progressData.LoadData(data);
                    currentCards = progressData.currentCards;
                    CurrentSpinIndex = progressData.currentSpinIndex;
                    isFirstShow = progressData.isFirstShow;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void SaveProgressData()
        {
            progressData.SaveData();
        }
        #endregion

        //收集卡片
        public void CollectCard(int cardId)
        {
            bool collectnew = false;
            if (cardId < 0)
            {
                Debug.LogError("CollectCard cardId is invalid");
                return;
            }
            if (currentCards.ContainsKey(cardId))
            {
                currentCards[cardId]++;
            }
            else
            {
                collectnew = true;
                currentCards[cardId] = 1;
            }
            // 保存进度数据
            SaveProgressData();
            //广播收集到新卡牌
            if (collectnew)
            {
                Messenger.Broadcast(CardSystemConstants.GetCardNewTypeCountMsg);
            }
            Messenger.Broadcast(CardSystemConstants.GetCardNewCountMsg);
            PlatformManager.Instance.SendMsgToPlatFormByType(MessageType.BuryPoint, "CardsNum", GetCardsCount());
        }

        /// <summary>
        /// 持有的卡牌数量，不论类型
        /// </summary>
        /// <returns></returns>
        public int GetCardsCount()
        {
            int count = 0;
            foreach (var UPPER in currentCards)
            {
                count += UPPER.Value;
            }
            return count;
        }
        
        public bool IsNewCard(int cardId)
        {
            if (cardId < 0)
            {
                Debug.LogError("IsNewCard cardId is invalid");
                return false;
            }
            // 检查当前卡牌是否在当前持有的卡牌中
            if (!progressData.HasCard(cardId))
            {
                newCardIndex.Add(cardId);
                return true;
            }
            return false;
        }
        public bool CheckCollectionNewCard(int cardId)
        {
            if (cardId < 0)
            {
                Debug.LogError("CheckCollectionNewCard cardId is invalid");
                return false;
            }
            // 检查当前卡牌是否在当前持有的卡牌中
            return newCardIndex.Contains(cardId);
        }
        
        public void ClearNewCardIndex()
        {
            newCardIndex.Clear();
        }
        
        public int GetCardCount(int id)
        {
            if (id < 0)
            {
                Debug.LogError("GetCardCount id is invalid");
                return 0;
            }
            if (currentCards.ContainsKey(id))
            {
                return currentCards[id];
            }
            return 0;
        }
        
        public int GetCardCountByName(string cardName)
        {
            if (string.IsNullOrEmpty(cardName))
            {
                Debug.LogError("GetCardCountByName cardName is invalid");
                return 0;
            }
            int cardId = GetCardIndex(cardName);
            if (cardId < 0)
            {
                Debug.LogError($"GetCardCountByName card not found: {cardName}");
                return 0;
            }
            return GetCardCount(cardId);
        }
        /// <summary>
        /// 已收集了多少种卡牌
        /// </summary>
        /// <returns></returns>
        public int GetHaveCardTypeCount()
        {
            if (currentCards == null)
            {
                return 0;
            }
            return currentCards.Count;
        }
        /// <summary>
        /// 总共有多少种卡牌
        /// </summary>
        /// <returns></returns>
        public int GetTotalCardTypeCount()
        {
            if (cardsInfo == null)
            {
                return 0;
            }
            return cardsInfo.Count;
        }
        
        #region 查询卡牌信息
        //获取卡牌等级
        public int GetCardLevel(int cardId)
        {
            if (cardId < 0)
            {
                Debug.LogError("GetCardLevel cardId is invalid");
                return 0;
            }
            BaseCard card = cardsInfo.Find(c => c.Index == cardId);
            if (card == null)
            {
                Debug.LogError($"GetCardLevel card not found: {cardId}");
                return 0;
            }
            int cardlevel = card.Level;
            return cardlevel;
        }

        public int GetCardIndex(string cardName)
        {
            if (string.IsNullOrEmpty(cardName))
            {
                Debug.LogError("GetCardIndex cardName is invalid");
                return -1;
            }
            BaseCard card = cardsInfo.Find(c => c.Name == cardName);
            if (card != null)
            {
                return card.Index;
            }
            Debug.LogError($"GetCardIndex card not found: {cardName}");
            return -1;
        }
        
        public string GetCardName(int id)
        {
            if (id < 0)
            {
                Debug.LogError("GetCardName id is invalid");
                return string.Empty;
            }
            BaseCard card = cardsInfo.Find(c => c.Index == id);
            if (card != null)
            {
                return card.Name;
            }
            Debug.LogError($"GetCardName card not found: {id}");
            return string.Empty;
        }
        
        public List<BaseCard> GetCardsInfo()
        {
            for (int i = 0; i < cardsInfo.Count; i++)
            {
                cardsInfo[i].SetCount(GetCardCount(cardsInfo[i].Index));
            }
            return cardsInfo;
        }
        
        public int GetCurCollectionCoins()
        {
            BaseCardCollect curCollection = GetCurCollection();
            if (curCollection != null)
            {
                return curCollection.Cash;
            }
            Debug.LogError("GetCurCollectionCoins curCollection is null");
            return 0;
        }
        
        public BaseCard GetCardById(int id)
        {
            if (id < 0)
            {
                Debug.LogError("GetCardById id is invalid");
                return null;
            }
            BaseCard card = cardsInfo.Find(c => c.Index == id);
            if (card != null)
            {
                return card;
            }
            Debug.LogError($"GetCardById card not found: {id}");
            return null;
        }
        #endregion
        
        public BaseCardCollect GetCurCollection()
        {
            if (string.IsNullOrEmpty(curCollectionName))
            {
                Debug.LogError("GetCurCollection curCollectionName is invalid");
                return null;
            }
            if (baseCardCollects.ContainsKey(curCollectionName))
            {
                return baseCardCollects[curCollectionName];
            }
            Debug.LogError($"GetCurCollection card collect not found: {curCollectionName}");
            return null;
        }
        
        public BaseCardCollect GetCardCollectByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError("GetCardCollectByName name is invalid");
                return null;
            }
            if (baseCardCollects.ContainsKey(name))
            {
                return baseCardCollects[name];
            }
            Debug.LogError($"GetCardCollectByName card collect not found: {name}");
            return null;
        }
        
        public BaseWeightCondition GetBaseWeightByIndex(int index = -1)
        {
            return index < 0 || index >= baseWeightLists.Count ? null : baseWeightLists[index];
        }

        public BaseWeightCondition GetCurrentWeightCondition()
        {
            return GetBaseWeightByIndex(CurrentSpinIndex);
        }
        
        public CardsReel GetCardsGroupByIndex(int index = -1)
        {
            return index < 0 || index >= baseCardsGroupLists.Count ? null : baseCardsGroupLists[index];
        }
        
        public List<BaseWeightCondition> GetBaseWeightLists()
        {
            return baseWeightLists;
        }
        public List<CardsReel> GetBaseCardsGroupLists()
        {
            return baseCardsGroupLists;
        }

        public Dictionary<int,int> GetCurrentCards()
        {
            if (currentCards == null || currentCards.Count == 0)
            {
                return new Dictionary<int, int>();
            }
            return currentCards;
        }

        public List<int> GetLotteryUI()
        {
            return lotteryUI;
        }

        public void AddWeightIndex()
        {
            CurrentSpinIndex++;
            CurrentSpinIndex = CurrentSpinIndex >= baseWeightLists.Count ? 0 : CurrentSpinIndex;
            //广播切换了权重条件
            Messenger.Broadcast(CardSystemConstants.LotteryChangeWeightConditionMsg);
        }

        #region ui
        public void ShowCollectionDialog()
        {
            // 这里可以添加特定于CardLotteryActivity的开始对话框逻辑
            Messenger.Broadcast(GameDialogManager.OpenCardSystemCollectionDialogMsg);
        }
        
        public void ShowLotteryDialog()
        {
            // 这里可以添加特定于CardLotteryActivity的开始对话框逻辑
            Messenger.Broadcast(GameDialogManager.OpenCardSystemLuckyDrawDialogMsg);
        }
        
        public void ShowGetRewardDialog(int cardId,GameObject parent)
        {
            // 这里可以添加特定于CardLotteryActivity的开始对话框逻辑
            Messenger.Broadcast<int,GameObject>(GameDialogManager.OpenCardSystemGetCardDialogMsg,cardId,parent);
        }
        #endregion
       
    }
}