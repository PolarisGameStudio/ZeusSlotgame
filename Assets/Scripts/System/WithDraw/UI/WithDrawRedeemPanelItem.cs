using System;
using System.Collections.Generic;
using System.Linq;
using Libs;
using MarchingBytes;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class WithDrawRedeemPanelItem:MonoBehaviour,LoopScrollPrefabSource, LoopScrollDataSource
{
    private GameObject itemPrefab;
    private int totalCount = 10;
    private ToggleGroup titleToggleGroup;
    private Toggle platToggle1, platToggle2,platToggle3;
    private LoopVerticalScrollRect loopVerticalScrollRect;
    private const string poolName = "WithDrawRedeemPanelItem";
    private List<BaseTask> withDarwTasks;
    private List<Toggle> Toggles = new List<Toggle>();
    private void Awake()
    {
        Debug.Log("WithDrawRedeemPanelItem Awake");
        titleToggleGroup = Util.FindObject<ToggleGroup>(transform, "titlegroup");
        platToggle1= Util.FindObject<Toggle>(transform, "titlegroup/Toggle1");
        platToggle2= Util.FindObject<Toggle>(transform, "titlegroup/Toggle2");
        platToggle3= Util.FindObject<Toggle>(transform, "titlegroup/Toggle3");
        platToggle1.onValueChanged.AddListener(OnValueChanged);
        platToggle2.onValueChanged.AddListener(OnValueChanged);
        platToggle3.onValueChanged.AddListener(OnValueChanged);
        Toggles.Add(platToggle1);
        Toggles.Add(platToggle2);
        Toggles.Add(platToggle3);
        itemPrefab = Util.FindObject<GameObject>(transform, "Loop/RedeemItem");
        //创建 loopScrollRect
        loopVerticalScrollRect = Util.FindObject<LoopVerticalScrollRect>(transform, "Loop/Scroll View");
        PoolResourceManager.Instance.InitPool(poolName,itemPrefab,5);
        loopVerticalScrollRect.prefabSource = this;
        loopVerticalScrollRect.dataSource = this;
        List<string> spritePath = LocalizationManager.Instance.GetPlatFormSpriteResourcePath();
        int num = spritePath.Count;
        //toggle显隐
        for (int i = 0; i < Toggles.Count; i++)
        {
            Toggles[i].name = i + "";
            Toggles[i].gameObject.SetActive(i<num);
        }
        AddressableManager.Instance.LoadAsset<SpriteAtlas>("Platform.spriteatlas", (result) =>
        {
            if (result != null)
            {
                for (int i = 0; i < num; i++)
                {
                    Sprite sp = result.GetSprite(spritePath[i]);
                    if (sp != null)
                    {
                        SetToggleSprite(i, sp);
                    }
                }
            }
        });
    }

    private void OnEnable()
    {
        Messenger.AddListener(WithDrawConstants.UpdateRedeemItemMsg, RefreshCurrentToggle);
    }

    private void OnDisable()
    {
        Messenger.RemoveListener(WithDrawConstants.UpdateRedeemItemMsg, RefreshCurrentToggle);
    }

    void SetToggleSprite(int index,Sprite sp)
    {
        Toggle toggle = Toggles[index];
        Image platform1 = Util.FindObject<Image>(toggle.transform, "Background/platform");
        platform1.sprite = sp;
    }
    
    
    #region LoopScrollRect必需实现

    public GameObject GetObject(int index)
    {
        GameObject go= PoolResourceManager.Instance.GetObjectFromPool(poolName);
        return go;
    }

    public void ReturnObject(Transform trans)
    {
        trans.GetComponent<RedeemItem>().OnDispose();
        PoolResourceManager.Instance.ReturnTransformToPool(trans);
    }
    
    public void ProvideData(Transform transform, int idx)
    {
        RedeemItem redeemItem = transform.GetComponent<RedeemItem>();
        //释放之前绑定的数据对象
        redeemItem.OnDispose();
        RedeemItemData itemData = WithDrawManager.Instance.GetRedeemItemData(idx);
        itemData.OnInit(redeemItem);
        redeemItem.UpdateData(idx,itemData);
    }
    
    #endregion
    
    private void Start()
    {
        platToggle1.isOn = true;
    }

    void OnValueChanged(bool value)
    {
        if (value == false)
        {
            return;
        }
        Debug.Log($"触发事件 - 当前状态: {value}, 调用堆栈: {Environment.StackTrace}");
        Toggle selected = titleToggleGroup.ActiveToggles().FirstOrDefault();
        if (selected != null)
        {
            Debug.Log("当前选中: " + selected.name);
            int index = int.Parse(selected.name);
            //设置当前选中的平台
            WithDrawManager.Instance.SetPlatFormIndex(index);
            loopVerticalScrollRect.totalCount = WithDrawManager.Instance.GetRedeemItemCount(index);
            loopVerticalScrollRect.RefillCells();
        }
    }
    
    void RefreshCurrentToggle()
    {
        Toggle selected = titleToggleGroup.ActiveToggles().FirstOrDefault();
        if (selected != null)
        {
            Debug.Log("当前选中: " + selected.name);
            int index = int.Parse(selected.name);
            //设置当前选中的平台
            loopVerticalScrollRect.totalCount = WithDrawManager.Instance.GetRedeemItemCount(index);
            //小批量更新
            loopVerticalScrollRect.RefreshCells();
        }
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Refresh()
    {
        
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}