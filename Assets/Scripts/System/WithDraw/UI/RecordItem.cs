
using System;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.U2D;
using UnityEngine.UI;
public class RecordItem: MonoBehaviour
{
    private TextMeshProUGUI cashTMP;
    private TextMeshProUGUI progressTMP;
    private Image progressBar;
    public RecordItemData itemData;
    private Image paltformImg;
    private Button redeemBtn;
    private Image moneyIMG;
    private void Awake()
    {
        moneyIMG = Utils.Utilities.RealFindObj<Image>(transform, "inProgress/bottom/moneyIMG");
        cashTMP = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "cashTMP");
        progressTMP = Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "inProgress/bottom/progressTMP");
        progressBar = Utils.Utilities.RealFindObj<Image>(transform, "inProgress/bottom/checking/progressBar");
        paltformImg = Utils.Utilities.RealFindObj<Image>(transform, "platformIMG");
        redeemBtn =  Utils.Utilities.RealFindObj<Button>(transform, "redeemBtn ");
        if (redeemBtn != null)
        {
            UGUIEventListener.Get(redeemBtn.gameObject).onClick = OnButtonClickHandler;
        }
    }
    private void OnButtonClickHandler(GameObject go)
    {
        LocalizedString localizedString = new LocalizedString(LocalizationManager.Instance.tableName,"MustCollectCards");
        Messenger.Broadcast<string>(WithDrawConstants.ShowTipMsg,localizedString.GetLocalizedString());
    }

    public void Refresh()
    {
        //设置任务图标排除任务类型
        ItemManager.Instance.GetIcon("1007", (icon) =>
        {
            moneyIMG.sprite = icon;
        });
       
        cashTMP.text = OnLineEarningMgr.Instance.GetCashStr(itemData.cash,needIcon:false);
        progressTMP.text = $"{itemData.curProgress}/{itemData.targetProgress}";
        if (progressBar != null)
        {
            if (itemData.targetProgress > 0)
            {
                progressBar.fillAmount = (float)itemData.curProgress / itemData.targetProgress;
            }
            else
            {
                progressBar.fillAmount = 0;
            }
        }
        
        AddressableManager.Instance.LoadAsset<SpriteAtlas>("Platform.spriteatlas", (result) =>
        {
            if (result != null)
            {
                Sprite sp = result.GetSprite(itemData.platSpIndex.ToString());
                if (sp != null)
                {
                    paltformImg.sprite = sp;
                }
            }
        });
        
    }
    
    public void UpdateData(int i, RecordItemData data)
    {
        itemData = data;
        Refresh();
    }
    
    public void OnDispose()
    {
        //旧数据解绑prefab
        if (itemData!=null)
        {
            itemData.UnBindUI();
        }
    }
    private void OnDestroy()
    {
        OnDispose();
    }
}
