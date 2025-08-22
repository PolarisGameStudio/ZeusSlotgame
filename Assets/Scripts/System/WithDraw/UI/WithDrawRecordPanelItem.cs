using System;
using MarchingBytes;
using UnityEngine;
using UnityEngine.UI;

public class WithDrawRecordPanelItem:MonoBehaviour,LoopScrollPrefabSource, LoopScrollDataSource
{
    private GameObject itemPrefab;
    private int totalCount = 10;
    private LoopVerticalScrollRect loopVerticalScrollRect;
    private const string poolName = "WithDrawRecordItem";

    private void Awake()
    {
        itemPrefab = Util.FindObject<GameObject>(transform, "RecordItem");
        //创建 loopScrollRect
        loopVerticalScrollRect = Util.FindObject<LoopVerticalScrollRect>(transform, "Scroll View");
        PoolResourceManager.Instance.InitPool(poolName,itemPrefab,5);
        loopVerticalScrollRect.prefabSource = this;
        loopVerticalScrollRect.dataSource = this;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        loopVerticalScrollRect.totalCount = WithDrawManager.Instance.GetRecordItemCount();
        loopVerticalScrollRect.RefillCells();
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public GameObject GetObject(int index)
    {
        GameObject go= PoolResourceManager.Instance.GetObjectFromPool(poolName);
        go.AddComponent<RecordItem>();
        return go;
    }

    public void ReturnObject(Transform trans)
    {
        Destroy(trans.GetComponent<RecordItem>());
        PoolResourceManager.Instance.ReturnTransformToPool(trans);
    }

    public void ProvideData(Transform transform, int idx)
    {
        RecordItem recordItem = transform.GetComponent<RecordItem>();
        //这里可以根据idx获取对应的记录数据
        RecordItemData itemData =WithDrawManager.Instance.GetRecordItemIndex(idx);
        itemData.OnInit(recordItem);
        recordItem.UpdateData(idx,itemData);
    }
}