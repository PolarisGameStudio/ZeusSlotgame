using System;
using System.Collections.Generic;
using UnityEngine;
using Classic;
using Random = System.Random;


public class WesternTreasureSpinResult : SceneProgressDataBase
{
    public int freespinNum;
    private bool isHaveFreeSymbol = false;
    private List<int> reelScatterList = new List<int>();
    public TreeGrow treeGrow;
   
    public Dictionary<int,int> freeTriggerSymbolDic = new Dictionary<int, int>();
    //0:进入respin 没有玩过选择游戏 1:玩过
    public bool isInJackpotGame = false;
    public bool isTriggerJackpotGame = false;
    private int scatterNum;
    //当前 spin结果中累计获得的wild数量，在触发 jackpotgame 后需要重置
    public int wildNum;
    
    public int curTreeLevel = 1;
    [NonSerialized]
    private WesternTreasureReelManager TreeManager;
    [NonSerialized]
    public List<ResultContent.ReelResult> reelResults = new List<ResultContent.ReelResult>();
    [NonSerialized]
    private Dictionary<string, object> info = new Dictionary<string, object>();
    [NonSerialized]
    private JackpotGameInfo Bonus_Trigger_F;
    [NonSerialized]
    private JackpotGameInfo Bonus_Trigger_B;
    [NonSerialized]
    public JackpotGameInfo curJackpotgameInfo;
    [NonSerialized]
    private Dictionary<int, int> T01_Random_Spin_B = new Dictionary<int, int>();
    [NonSerialized]
    private Dictionary<int, int> T01_Random_Spin_F = new Dictionary<int, int>();
    public int clickIndex = 0;
    public int pickNum = 0;
    public int winType;
    public double winMoney;
    public bool isOpenEndDialog = false;
    public List<int> jackpotList = new List<int>();
    public List<int> clickDataList = new List<int>();
    private List<List<int>> reelChangeResults = new List<List<int>>();
//点击列表：点击：true 未点击：false
    public List<bool> clickIndexList = new List<bool>();
    public WesternTreasureSpinResult(WesternTreasureReelManager reelManager, Dictionary<string, object> extroInfos)
    {
        TreeManager = reelManager;
        info = extroInfos;
        this.InitSlotsData();
    }

    private void InitSlotsData()
    {
        this.InitSymbolInfos("T01_Random_Spin_B", T01_Random_Spin_B);
        this.InitSymbolInfos("T01_Random_Spin_F", T01_Random_Spin_F);
      
        if (info.ContainsKey("Bonus_Trigger_B"))
        {
            Bonus_Trigger_B = new JackpotGameInfo(info["Bonus_Trigger_B"]);
            
        }
        if (info.ContainsKey("Bonus_Trigger_F"))
        {
            Bonus_Trigger_F = new JackpotGameInfo(info["Bonus_Trigger_F"]);

        }

        InitTreeGrow();
    }
    List<int> growUpNum = new List<int>();
    int baseWild;
    int freeWild;
    private void InitTreeGrow()
    {
        if (info.ContainsKey("TreeGrow"))
        {
            Dictionary<string, object> infos = info["TreeGrow"] as Dictionary<string, object>;
            if (infos.ContainsKey("GrowUpNum"))
            {
                growUpNum = Utils.Utilities.CastObjToIntList(infos["GrowUpNum"]);
            }
            if (infos.ContainsKey("BaseWild"))
            {
                baseWild = Utils.Utilities.CastValueInt(infos["BaseWild"]);
            }
            if (infos.ContainsKey("FreeWild"))
            {
                freeWild = Utils.Utilities.CastValueInt(infos["FreeWild"]);
            }
            treeGrow = new TreeGrow(growUpNum,baseWild,freeWild);
        }
    }

    private void InitSymbolInfos(string name, Dictionary<int, int> dicInfo)
    {
        if (info.ContainsKey(name))
        {
            foreach (var item in info[name] as List<object>)
            {
                Dictionary<string, object> infos = item as Dictionary<string, object>;
                int value = Utils.Utilities.CastValueInt(infos["value"]);
                int weight = Utils.Utilities.CastValueInt(infos["weight"]);
                dicInfo.Add(value, weight);
            }
        }
    }

   
    public bool CheckHitBonus()
    {
        //JackpotGame
        curJackpotgameInfo = GetCurJackpotGameInfo();
        //wildNum的累加在收集动画播放完之后
        int wildNum =  this.wildNum + GetWildCount();
        if (curJackpotgameInfo.isEnterJackpotFGame(wildNum,treeGrow.level4)) {
            //Debug.LogError(TreeManager.isFreespinBonus+ "    ------------   "+curJackpotgameInfo.Trigger_Random);
            clickIndex = 0;
           
            jackpotList = curJackpotgameInfo.GetJackpotGameResult();
            pickNum = curJackpotgameInfo.pickNum;
            winType = curJackpotgameInfo.jackpotType;
            isTriggerJackpotGame = true;
            isInJackpotGame = true;
            isOpenEndDialog = false;
            if (TestController.Instance != null)
            {
                winMoney=TreeManager.jackPotData.GetJackPotAward(winType);
            }
            
            Messenger.Broadcast(GameConstants.TriggerBonusGame);
            clickIndexList.Clear();
            for (int i = 0; i < 12; i++)
            {
                clickIndexList.Add(false);
            }
            clickDataList.Clear();
            return true;
        }

        return false;
    }

    public void ChangeResult()
    {
        reelResults = TreeManager.resultContent.ReelResults;
        //BaseGame和Freegame 替换T01
        reelChangeResults = ChangeT01.CetInstance().GetResult(this);
        ChangeSpinResult();
    }

    public void SetFreespin()
    {
        //Freegame
        TreeManager.FreespinCount = 0;
        TreeManager.HitFs = false;
        SetFreeSpinCount();
    }

    private bool ishaveFree;
    private void SetFreeSpinCount()
    {
        freeTriggerSymbolDic.Clear();
        reelScatterList.Clear();

        reelResults = TreeManager.resultContent.ReelResults;
        for (int i = 0;  i< reelResults.Count; i++)
        {
            ishaveFree = false;
            for (int j = 0; j < reelResults[i].SymbolResults.Count; j++)
            {
                int itemId = this.reelResults[i].SymbolResults[j];
                if (itemId == 0 || itemId == 1)
                {
                    ishaveFree = true;
                    if (!freeTriggerSymbolDic.ContainsKey(i))
                    {
                        freeTriggerSymbolDic.Add(i,j);
                    }
                }
               
            }

            if (!ishaveFree)
            {
                break;
            }

        }
        if (freeTriggerSymbolDic.Count>=3) {
            scatterNum = freeTriggerSymbolDic.Count; 
            
            TreeManager.FreespinCount = TreeManager.gameConfigs.FreeSpinTimes;
            TreeManager.HitFs = true;

        }
        else
        {
            scatterNum = 0;
        }

    }

    int bv;
    public int GetScatterPayAward()
    {
        bv = 0;
        long baseMoney = 0;
        if (scatterNum == 3)
        {
            bv = 5;  
        }
        else if (scatterNum == 4)
        {
            bv = 10;  
        }
        else if (scatterNum == 5)
        {
            bv = 50;
        }
        
        scatterNum = 0;
        return bv;
    }

    //清理JackPotGame数据
    public void ClearJackpotData()
    {
        isInJackpotGame = false;
        wildNum = 0;
        curTreeLevel = 1;
        winMoney = 0;
        clickDataList.Clear();
    }

    private int GetWildCount()
    {
        int wildNum = 0;
        for (int i = 0; i < TreeManager.resultContent.ReelResults.Count; i++)
        {
            for (int j = 0; j < TreeManager.resultContent.ReelResults[i].SymbolResults.Count; j++)
            {
                int symbolIndex =TreeManager.resultContent.ReelResults[i].SymbolResults[j];
                if (symbolIndex==1)
                {
                    wildNum++;
                }
            }

        }

        return wildNum;
    }

    #region 生成BonusGame数据结果

    public JackpotGameInfo GetCurJackpotGameInfo()
    {
        if (TreeManager.isFreespinBonus )
        {
            return Bonus_Trigger_F;
        }
        else {
            return Bonus_Trigger_B;
        }
    }
    //把预先创建好的结果赋值过来
    public void ChangeSpinResult()
    {
        for (int i = 0; i < TreeManager.resultContent.ReelResults.Count; i++)
        {
            for (int j = 0; j < TreeManager.resultContent.ReelResults[i].SymbolResults.Count; j++)
            {
                TreeManager.resultContent.ReelResults[i].SymbolResults[j] = reelChangeResults[i][j];
                
            }

        }
       
      
    }
   
    
   
    
    #endregion


    #region 随机结果数据

   
  

    public int SpinBonusItem(Dictionary<int, int> dic)
    {
        int weight = 0;
        int totalWeight = 0;
        foreach (var id in dic.Keys) totalWeight += dic[id];
        int random = UnityEngine.Random.Range(0, totalWeight);
        foreach (var id in dic.Keys)
        {
            if (weight <= random && random < (weight + dic[id])) return id;
            weight += dic[id];
        }
        return 0;
    }
    public int SpinBonusItem()
    {
        return this.SpinBonusItem(GetT01_random_spin());
    }



    public Dictionary<int, int> GetT01_random_spin()
    {
        if (TreeManager.isFreespinBonus) {
            return T01_Random_Spin_F;
        }
        else {
            return T01_Random_Spin_B;
        }
    }
    
   
    private JackpotGameInfo GetJackpotGameInfo()
    {
        if (TreeManager.isFreespinBonus)
        {
            return Bonus_Trigger_F;
        }
        else
        {
            return Bonus_Trigger_B;
        }
    }
    //是否已经选取出中奖图标
    public bool isPickOver()
    {
        //Debug.LogError(pickNum+"    pickNum----------------------clickIndex   "+clickIndex);
        return pickNum == clickIndex;
    }

    #endregion

    



    #region 保存数据
  
    public List<string> saveBaseSpinTable = new List<string>();
    public List<string> saveFreeSpinTable = new List<string>();
    public string jackpotListStr;
    public string clickIndexListStr;
    //点击顺序列表
    public string clickDataListStr;
    public void SaveGameData()
    {
        saveBaseSpinTable.Clear();
        saveFreeSpinTable.Clear();
        jackpotListStr = "";
        clickIndexListStr = "";
        clickDataListStr = "";
        if (TreeManager.isFreespinBonus) {
            this.IsInFreespin = true;
           
        }
        
        this.awardMultipler = TreeManager.FreespinGame.multiplier;
        this.freespinNum = TreeManager.FreespinGame.LeftTime;
        this.freespinCount = TreeManager.FreespinCount;
        this.leftFreeSpinCount = TreeManager.FreespinGame.LeftTime;
        this.totalFreeSpinCount = TreeManager.FreespinGame.TotalTime;
        this.needShowCash = TreeManager.FreespinGame.NeedShowCash;
        if (TreeManager.isFreespinBonus)
        {
            if (TreeManager.FreespinGame.LeftTime==TreeManager.FreespinGame.TotalTime && TreeManager.FreespinGame.TotalTime!=0)
            {
                this.freespinCount = 0;
            }
        }
        
        this.currentWinCoins = BaseSlotMachineController.Instance.winCoinsForDisplay;

        this.currentBetting = BaseSlotMachineController.Instance.currentBetting;
        
        this.currentWinCash = BaseSlotMachineController.Instance.winCashForDisplay;
      
        
        if (TreeManager.baseGameResult != null) {
            foreach (var item in TreeManager.baseGameResult)
            {
                if (item.Count == 0) break;
                saveBaseSpinTable.Add(Utils.Utilities.ListToStr<int>(item));
            }
        }


        if (TreeManager.freeSpinResult != null) {
            foreach (var item in TreeManager.freeSpinResult)
            {
                if (item.Count == 0) break;
                saveFreeSpinTable.Add(Utils.Utilities.ListToStr<int>(item));
            }
        }

        if (isInJackpotGame)
        {
            jackpotListStr = Utils.Utilities.ListToStr<int>(jackpotList);
            clickIndexListStr = Utils.Utilities.ListToStr<bool>(clickIndexList);
            clickDataListStr = Utils.Utilities.ListToStr<int>(clickDataList);
        }


    }
    //加载
    public void LoadGameData(WesternTreasureSpinResult spinResult)
    {
        if (spinResult == null) return;
        
        TreeManager.baseGameResult = new List<List<int>>();
        foreach (var item in spinResult.saveBaseSpinTable)
        {
            TreeManager.baseGameResult.Add(Utils.Utilities.StrToList<int>(item));
        }
       
      
        this.freespinNum = spinResult.freespinNum;
        this.IsInFreespin = spinResult.IsInFreespin;
        this.awardMultipler = spinResult.awardMultipler;
        this.leftFreeSpinCount = spinResult.leftFreeSpinCount;
        this.freespinCount = spinResult.freespinCount;
        this.totalFreeSpinCount = spinResult.totalFreeSpinCount;

        this.currentWinCoins = spinResult.currentWinCoins;
        this.isInJackpotGame = spinResult.isInJackpotGame;
        this.isTriggerJackpotGame = spinResult.isTriggerJackpotGame;
        BaseSlotMachineController.Instance.winCoinsForDisplay = spinResult.currentWinCoins;
        this.wildNum = spinResult.wildNum;
        this.currentBetting = spinResult.currentBetting;
        this.isOpenEndDialog = spinResult.isOpenEndDialog;
        this.awardMultipler = spinResult.awardMultipler;
        //显示金钱相关
        this.currentWinCash = spinResult.currentWinCash;
        BaseSlotMachineController.Instance.winCashForDisplay = spinResult.currentWinCash;
        this.needShowCash = spinResult.needShowCash;
        
        if (IsInFreespin) {
            TreeManager.freeSpinResult = new List<List<int>>();
            foreach (var item in spinResult.saveFreeSpinTable)
            {
                TreeManager.freeSpinResult.Add(Utils.Utilities.StrToList<int>(item));
            }
            
        }
        if (isInJackpotGame)
        {
            this.jackpotListStr = spinResult.jackpotListStr;
            this.clickIndexListStr = spinResult.clickIndexListStr;
            this.clickDataListStr = spinResult.clickDataListStr;
            this.pickNum = spinResult.pickNum;
            this.winMoney = spinResult.winMoney;
            pickNum = spinResult.pickNum;
            clickIndex = 0;
            winType = spinResult.winType;
            jackpotList = Utils.Utilities.StrToList<int>(jackpotListStr);
            clickIndexList = Utils.Utilities.StrToList<bool>(clickIndexListStr);
            clickDataList = Utils.Utilities.StrToList<int>(clickDataListStr);
        }
    }

    #endregion

    public class ChangeT01
    {
        private static ChangeT01 _Singleton = null;
        private static object Singleton_Lock = new object();
        public static ChangeT01 CetInstance()
        {
            if (_Singleton == null) //双if +lock
            {
                lock (Singleton_Lock)
                {
                    if (_Singleton == null)
                    {
                        _Singleton = new ChangeT01();
                    }
                }
            }
            return _Singleton;
        }
        private WesternTreasureSpinResult spinResult;
        private List<List<int>> reelResults = new List<List<int>>();
        private Dictionary<int, int> T01Dic = new Dictionary<int, int>();
        private Dictionary<int, int> NoTreeDic = new Dictionary<int, int>();
        private Dictionary<int, int> firstTreeDic = new Dictionary<int, int>();
        private bool twoReelHaveWild;
        private bool threeReelHaveWild;
        private int T01;
        public List<List<int>> GetResult(WesternTreasureSpinResult _spinResult)
        {
            spinResult = _spinResult;
            T01Dic = spinResult.GetT01_random_spin();
            CopyReelResults();
            ChangeResult();
            return reelResults;

        }
        private void CopyReelResults()
        {
            reelResults.Clear();
            for (int i = 0; i < spinResult.TreeManager.resultContent.ReelResults.Count; i++)
            {
                List<int> symbolResults = new List<int>();
                for (int j = 0; j < spinResult.TreeManager.resultContent.ReelResults[i].SymbolResults.Count; j++)
                {
                    symbolResults.Add(spinResult.TreeManager.resultContent.ReelResults[i].SymbolResults[j]);
                }
                reelResults.Add(symbolResults);
            }
        }
      
        private void ChangeResult()
        {
            ChargeWild();
            //替换第2、4、5列的T01
            ChangeWild(threeReelHaveWild,2,new int[]{1,3,4});
            //替换第3列的T01
            ChangeWild(twoReelHaveWild,1,new int[]{2});
            
        }
//reelIndex :参考的列数
        private void ChangeWild(bool isHaveWild,int reelIndex,int[] changeReelArr)
        {
            NoTreeDic.Clear();
            foreach (var item in T01Dic)
            {
                if (isHaveWild)
                {
                    if (reelResults[0].IndexOf(item.Key)<0 ) {
                        NoTreeDic.Add(item.Key,item.Value);
                    }
                }
                else
                {
                    NoTreeDic.Add(item.Key,item.Value);
                }
            }
            if (!isHaveWild)
            {
                for (int i = 0; i < reelResults[0].Count; i++)
                {
                    for (int j = 0; j < reelResults[reelIndex].Count; j++)
                    {
                        if (reelResults[0][i]==reelResults[reelIndex][j] && NoTreeDic.ContainsKey(reelResults[reelIndex][j]))
                        {
                            NoTreeDic.Remove(reelResults[reelIndex][j]);
                        }
                    } 
                }
                firstTreeDic.Clear();
                foreach (var item in NoTreeDic)
                {
                    if (reelResults[0].IndexOf(item.Key)>0 ) {
                        firstTreeDic.Add(item.Key,item.Value);
                    }
                }

                if (firstTreeDic.Count>0)
                {
                    T01 = spinResult.SpinBonusItem(firstTreeDic);
                }
                else
                {
                    T01 = spinResult.SpinBonusItem(NoTreeDic);
                }
            }
            else
            {
                T01 = spinResult.SpinBonusItem(NoTreeDic);
            }

            //Debug.LogError("%%%%%%%%%%%% T01  "+T01);
            for (int i = 0; i < changeReelArr.Length; i++)
            {
                for (int j = 0; j < reelResults[i].Count; j++)
                {
                    if (reelResults[changeReelArr[i]][j]==13) {
                        reelResults[changeReelArr[i]][j] = T01;
                    } 
                }
            }
        }

        private void ChargeWild()
        {
            twoReelHaveWild = false;
            threeReelHaveWild = false;
            for (int i = 0; i < reelResults.Count; i++)
            {
                for (int j = 0; j < reelResults[i].Count; j++)
                {
                    if (i==2 && reelResults[i][j]==1)
                    {
                        threeReelHaveWild = true;
                    }
                    if (i==1 && reelResults[i][j]==1)
                    {
                        twoReelHaveWild = true;
                    }
                }
            }
        }

        private void DebugData(string aa)
        {
            for (int i = 0; i < reelResults.Count; i++)
            {
                for (int j = 0; j < reelResults[i].Count; j++)
                {
                    Debug.LogError(aa+"     %%%%%%%%%%%%  ["+i+"]   ["+j+"]  "+reelResults[i][j]);
                }
            }
            Debug.LogError("     ------------------------------------------------  ");
        }
    }




    public class JackpotGameInfo
    {
        //触发小游戏的概率
        public float Trigger_Random;
        //小游戏中，中什么jackpot
        private Dictionary<int, int> Reward_Random = new Dictionary<int, int>();
        //选取几次金币中奖
        private Dictionary<int, int> Pick_Conuts_Random = new Dictionary<int, int>();
        //所有图标出现的概率
        private Dictionary<int, int> JackPot_Appear_Random = new Dictionary<int, int>();

        //除去中奖图标，其他图标出现的概率
        private Dictionary<int, int> NoReward_Appear_Random = new Dictionary<int, int>();
        private Dictionary<int, int> middleDic = new Dictionary<int, int>();
        //除去中奖图标，其他图标出现的列表
        private List<int> NoReward_Appear_list = new List<int>();
        private List<int> allJackpot = new List<int>() {2,2,2,3,3,3,4,4,4,5,5,5 };
        //jackpot游戏的最终列表
        private List<int> jackpotGame_list = new List<int>();
       
        public int pickNum;
        public int jackpotType;
        public JackpotGameInfo(object infoItem)
        {
            Dictionary<string, object> infos = infoItem as Dictionary<string, object>;
            if (infos.ContainsKey("Trigger_Random"))
            {
                Trigger_Random = Utils.Utilities.CastValueFloat(infos["Trigger_Random"]);
//                Debug.LogError( "     ------------wildNum   "+Trigger_Random);
            }
            

            if (infos.ContainsKey("Reward_Random"))
            {
                foreach (var item in infos["Reward_Random"] as List<object>)
                {
                    Dictionary<string, object> respin_C1_info = item as Dictionary<string, object>;
                    int count = Utils.Utilities.CastValueInt(respin_C1_info["value"]);
                    int weight = Utils.Utilities.CastValueInt(respin_C1_info["weight"]);
                    Reward_Random.Add(count, weight);
                }
            }


            if (infos.ContainsKey("Pick_Conuts_Random"))
            {
                foreach (var item in infos["Pick_Conuts_Random"] as List<object>)
                {
                    Dictionary<string, object> respin_C1_info = item as Dictionary<string, object>;
                    int count = Utils.Utilities.CastValueInt(respin_C1_info["value"]);
                    int weight = Utils.Utilities.CastValueInt(respin_C1_info["weight"]);
                    Pick_Conuts_Random.Add(count, weight);
                }
            }

            if (infos.ContainsKey("JackPot_Appear_Random"))
            {
                foreach (var item in infos["JackPot_Appear_Random"] as List<object>)
                {
                    Dictionary<string, object> respin_C1_info = item as Dictionary<string, object>;
                    int count = Utils.Utilities.CastValueInt(respin_C1_info["value"]);
                    int weight = Utils.Utilities.CastValueInt(respin_C1_info["weight"]);
                    JackPot_Appear_Random.Add(count, weight);
                }
            }

            
        }
        //(int)(1 / Trigger_Random) 
        //此处需要做修改改为累计数量达到临界值
        public bool isEnterJackpotFGame(int wildNum,int targetNum)
        {
           
            // float rate = Trigger_Random * wildNum;
            // int random = UnityEngine.Random.Range(1,101);
            // return random <= rate;
            return wildNum >= targetNum;
        }
        
       
        public List<int> GetJackpotGameResult()
        {
            jackpotGame_list.Clear();
           
            jackpotType = GetNumByWeight(Reward_Random);
           
            pickNum = GetNumByWeight(Pick_Conuts_Random);
           
            GetNoRewardDic(jackpotType, JackPot_Appear_Random);
           
            GetNoRewardList(jackpotType);
           
            int creatNoRewardNum = pickNum - 3;
            count = 0;
            for (int i = 0; i < creatNoRewardNum; i++)
            {
                CreatNoRewardJackpot();
            }
           // Debug.LogError(jackpotType+"     666666666666      "+pickNum);
            AddJackpot(jackpotType);
//            for (int i = 0; i < jackpotGame_list.Count; i++)
//            {
//                Debug.LogError(i+"     ########## ["+jackpotGame_list.Count+"]     "+jackpotGame_list[i]);
//            }
            GetNoClickJackpotList();
            jackpotGame_list.AddRange(allJackpot);
//            for (int i = 0; i < jackpotGame_list.Count; i++)
//            {
//                  Debug.LogError(i+"     ["+allJackpot.Count+"]       ["+jackpotGame_list.Count+"]  "+jackpotGame_list[i]);
//            }
            return jackpotGame_list;
        }
        
        //把中奖的jackpot放到列表里
        private void AddJackpot(int jackpotType)
        {
            for (int i = 0; i < 2; i++)
            {
                jackpotGame_list.Add(jackpotType);
            }
            jackpotGame_list.Sort(delegate (int a, int b) { return (new System.Random()).Next(-1, 1); });
            jackpotGame_list.Add(jackpotType);
            
        }
        private void GetNoClickJackpotList()
        {
            allJackpot = new List<int>() { 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 5 };
            for (int i = 0; i < jackpotGame_list.Count; i++)
            {
                int index = allJackpot.IndexOf(jackpotGame_list[i]);
                if (index>-1) {
                    allJackpot.RemoveAt(index);
                }
            }
            allJackpot.Sort(delegate (int a, int b) { return (new System.Random()).Next(-1, 1); });
        }

        private int count;
        private void CreatNoRewardJackpot()
        {
            int noJackpotType = GetNumByWeight(NoReward_Appear_Random);
            int index = NoReward_Appear_list.IndexOf(noJackpotType);
            count++;
            if (count>=70)
            {
                Debug.LogError(index+"   超出范围--------------  "+noJackpotType);
                return;
            }

            if (index >= 0)
            {
//                Debug.LogError(noJackpotType+"   --------创建其他图标--------------    "+ index);
                jackpotGame_list.Add(noJackpotType);
                NoReward_Appear_list.RemoveAt(index);
            }
            else
            {
                middleDic.Clear();
                foreach (var item in NoReward_Appear_Random)
                {
                    middleDic.Add(item.Key,item.Value);
                }
                GetNoRewardDic(noJackpotType, middleDic);
                CreatNoRewardJackpot();
            }
        }
        private void GetNoRewardDic(int jackpotType, Dictionary<int, int> dic)
        {
            NoReward_Appear_Random.Clear();
            foreach (var item in dic)
            {
                if (item.Key != jackpotType) {
                    NoReward_Appear_Random.Add(item.Key, item.Value);
                }
            }
        }
        private void GetNoRewardList(int jackpotType)
        {
            NoReward_Appear_list.Clear();
            for (int i = 2; i < 6; i++)
            {
                if (i != jackpotType) {
                    NoReward_Appear_list.Add(i);
                    NoReward_Appear_list.Add(i);
                }
            }
        }
  
        public int GetNumByWeight(Dictionary<int, int> dic)
        {
            int weight = 0;
            int totalWeight = 0;
            foreach (var id in dic.Keys) totalWeight += dic[id];
            int random = UnityEngine.Random.Range(0, totalWeight);
            foreach (var id in dic.Keys)
            {
                if (weight <= random && random < (weight + dic[id])) return id;
                weight += dic[id];
            }
            return 0;
        }
        
    }
    public class TreeGrow
    {
        private List<int> growUpNum = new List<int>();
        public int level2;
        public int level3;
        public int level4;
        public int baseWild;
        public int freeWild;

        public TreeGrow(List<int> GrowUpNum,int BaseWild,int FreeWild)
        {
            growUpNum = GrowUpNum;
            baseWild = BaseWild;
            freeWild = FreeWild;
            if (growUpNum.Count==3)
            {
                level2 = growUpNum[0];
                level3 = growUpNum[1];
                level4 = growUpNum[2];
            }
            else
            {
                Debug.LogError("树的升级配表不对");
            }

            
        }
        
    }
}
