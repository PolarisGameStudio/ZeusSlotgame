using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Classic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Binding;

public class TestBoardDataPanel : MonoBehaviour
{
    public Toggle m_UseFunction;
    public Toggle unLimitMoney;
    public Toggle m_AlwaysUseFunction;
    public Toggle m_IsFsTog;
    public Text TipsInfoTxt;
    [FormerlySerializedAs("m_Obj")] public GameObject m_BoardPanel;
    
    public GameObject m_ReelPanel;
    public GameObject m_SymbolRenderPrefab;
    public GameObject m_SymbolImagePrefab;
    private ScrollRect m_ScrollRect;

    public ScrollRect m_HorizontalScrollRect;
    public ScrollRect m_VerticleScrollRect;
    [FormerlySerializedAs("m_SymbolParentPanel")] public Transform m_BoardParentPanel;

    private ReelManager m_ReelManager;
    private BoardConfigs m_BoardConfig;

//    private List< List<Dropdown>> m_AllDropDown = new List<List<Dropdown>>();
    private List< List<TestBoardItemRender>> m_AllItemRenders = new List<List<TestBoardItemRender>>();

    private bool m_HasInit = false;
    
    private List<List<int>> m_SelectSymbolData = new List<List<int>>();
     
    BindableValue<string> tipsInfo = new BindableValue<string>("");

    private void Start()
    {
        gameObject.Bind(tipsInfo,TipsInfoTxt.For(v=>v.text));
        
        unLimitMoney.onValueChanged.AddListener(OnToggleValueChanged);
    }
    private void OnToggleValueChanged(bool isOn)
    {
        OnLineEarningMgr.UnLimitMoney = isOn;
        Messenger.Broadcast(SlotControllerConstants.OnCashChangeForDisPlay);
    }

    public void Close()
    {
        Time.timeScale = 1;
        m_BoardPanel.SetActive(false);
        
        m_SelectSymbolData.Clear();
        for (int i = 0; i < m_AllItemRenders.Count; i++)
        {
            List<int> reelData = new List<int>();
            for (int j = 0; j < m_AllItemRenders[i].Count; j++)
            {
                int symbolIndex =GetSymbolId( m_AllItemRenders[i][j].SymbolId);
                reelData.Add(symbolIndex);
            }
            m_SelectSymbolData.Add(reelData);
        }
    }

    public void Reset()
    {
        m_SelectSymbolData.Clear();
        for (int i = 0; i < m_AllItemRenders.Count; i++)
        {
            for (int j = 0; j < m_AllItemRenders[i].Count; j++)
            {
                m_AllItemRenders[i][j].Reset();
            }
        }
    }
    
    public void SelectAll()
    {
        m_SelectSymbolData.Clear();
        for (int i = 0; i < m_AllItemRenders.Count; i++)
        {
            for (int j = 0; j < m_AllItemRenders[i].Count; j++)
            {
                m_AllItemRenders[i][j].IsSelect = true;
            }
        }
    }

    private int GetSymbolId(int _originId)
    {
        if (_originId > -2)
            return _originId;
        List<int> list = m_ReelManager.symbolMap.ElementsData.Values.ToList();
        int all = list.Count;
        int random = Random.Range(0, all);
        
        return random;
    }
    public bool IsEnable
    {
        get => this.m_UseFunction.isOn || this.m_AlwaysUseFunction.isOn;
        set => this.m_UseFunction.isOn = value;
    }

    public void AlwaysCheck()
    {
        if (this.m_AlwaysUseFunction.isOn)
        {
            if (this.m_UseFunction.isOn)
                this.m_UseFunction.isOn = false;
        }
    }

    public void OnceCheck()
    {
        if(this.m_UseFunction.isOn)
        {
            if (this.m_AlwaysUseFunction.isOn)
                this.m_AlwaysUseFunction.isOn = false;
        }
    }

    public List<List<int>> SymbolResult
    {
        get => m_SelectSymbolData;
    }

    public void ChooseItem(int _symbolId,Sprite sprite)
    {
        Debug.Log("click"+_symbolId);
        for (int i = 0; i < m_AllItemRenders.Count ; i++)
        {
            for (int j = 0; j < m_AllItemRenders[i].Count; j++)
            {
                TestBoardItemRender render = m_AllItemRenders[i][j];
                if(render.IsSelect)
                {
                    render.IsSelect = false;
                    render.SetData(_symbolId,sprite);
                }
            }
        }
    }

    public void Open()
    {
        Time.timeScale = 0;
        m_BoardPanel.SetActive(true);
        m_ReelManager = BaseSlotMachineController.Instance.reelManager;
        m_BoardConfig =  m_ReelManager.GetComponent<BoardConfigs>();
        tipsInfo.Value = "";
        (m_BoardParentPanel.transform as RectTransform).sizeDelta = new Vector2(Screen.width,Screen.height);
         if (SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait)
         {
             m_BoardPanel.transform.localPosition = new Vector3(m_BoardPanel.transform.localPosition.x,700,0);
             m_IsFsTog.transform.localPosition = new Vector3(441,m_IsFsTog.transform.localPosition.y,0);
         }
        
        if(!m_HasInit)
        {
            RefreshUi();
            m_HasInit = true;
        }
    }

    public void RefreshUi()
    {
        if (SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait)
        {
            this.m_ScrollRect = m_HorizontalScrollRect;
            m_VerticleScrollRect.gameObject.SetActive(false);
        }
        else
        {
            this.m_ScrollRect = m_VerticleScrollRect;
            m_HorizontalScrollRect.gameObject.SetActive(false);
        }
        
        m_AllItemRenders.Clear();

        int xCount = m_BoardConfig.ReelConfigs.Length;
        for (int i = 0; i < xCount; i++)
        {
            GameObject reelPanel = Instantiate(m_ReelPanel, this.m_BoardParentPanel);
            (reelPanel.transform as RectTransform).localPosition = new Vector3(i * 200+50,0,0);
            int yCount = m_BoardConfig.ReelConfigs[i].ReelShowNum;
            if(m_BoardConfig.name == "WildWildZ")
                yCount = 7;
            List<TestBoardItemRender> currentReelDropdowns = new List<TestBoardItemRender>();
            for (int j = 0; j < yCount; j++)
            {
                GameObject obj = Instantiate(m_SymbolRenderPrefab, reelPanel.transform);
                TestBoardItemRender itemRender = obj.GetComponent<TestBoardItemRender>();
                currentReelDropdowns.Add(itemRender);
            }

            m_AllItemRenders.Add(currentReelDropdowns);
        }

        foreach (var key in m_ReelManager.symbolMap.ElementsData.Keys)
        {
            GameObject symbolImage = Instantiate(m_SymbolImagePrefab, this.m_ScrollRect.content);
            TestBoardImageRender render  = symbolImage.GetComponent<TestBoardImageRender>();

            int id = m_ReelManager.symbolMap.ElementsData[key];
            Sprite s = null;
            if (id >= 0 && id <m_ReelManager.gameConfigs.elementResources.Length)
            {
                s = m_ReelManager.gameConfigs.elementResources[id].staticSprite;
            }
                render.Init(this, id, s);
        }
    }

    public void ClearServerData()
    {
        JsonClearData data = new JsonClearData();
        data.machine = BaseSlotMachineController.Instance.slotMachineConfig.Name();
        data.bet = (long) BaseSlotMachineController.Instance.currentBetting;
        string str= JsonUtility.ToJson(data);
        
        Debug.Log(str);
    }
    public void ResetServerData()
    {
        Dictionary<string,object> postDataDict = new Dictionary<string, object>();
        List<int> symbolIds = new List<int>();
        for (int i = 0; i < m_AllItemRenders.Count; i++)
        {
            List<int> reelData = new List<int>();
            for (int j = 0; j < m_AllItemRenders[i].Count; j++)
            {
                int symbolIndex =GetSymbolId( m_AllItemRenders[i][j].SymbolId);
                symbolIds.Add(symbolIndex);
            }
        }
        JsonData data = new JsonData();
        data.machine = BaseSlotMachineController.Instance.slotMachineConfig.Name();
        data.bet = (long) BaseSlotMachineController.Instance.currentBetting;
        data.spin_type = m_IsFsTog.isOn ? "free" :"base";//TODO 将来优化
        data.reels = symbolIds;
        string str= JsonUtility.ToJson(data);
        
        Debug.Log(str);

        byte[] postBytes= Encoding.UTF8.GetBytes(str);
        StartCoroutine(SendToServer("save_reel",postBytes));
    }

    /*
     * params:
    mid:  用户id
    machine: 机器名称
    spin_type: 游戏模式(base/bonus/free)
    reels: 牌面（[0,1,2,3,4,5,6,7,8])
     */
    IEnumerator SendToServer(string afterUrl, byte[] postBytes,Action callback=null)
    {
        tipsInfo.Value = "";
        string url = "https://bfslots-test.RealYous.cn/spin_server/spin/";

        url = $"{url}{afterUrl}";

        using (UnityWebRequest request = UnityWebRequest.Put(url, postBytes))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            
            if (request.isNetworkError || request.isHttpError)
            {
                tipsInfo.Value = $"{request.error}  {request.url}";
                Debug.LogError($"{request.error}  {request.url}");
            }
            else
            {
                tipsInfo.Value = "Success:" + request.downloadHandler.text;
                Log.LogLimeColor("Success:" +request.downloadHandler.text);
                if (callback != null)
                {
                    callback();
                }
            }
        }
    }

    [Serializable]
    class JsonData
    {
        public string mid;
        public string machine;
        public string spin_type;
        public long bet;
        public List<int> reels;
    }
    
    [Serializable]
    class JsonClearData
    {
        public string mid;
        public string machine;
        public long bet;
    }
}