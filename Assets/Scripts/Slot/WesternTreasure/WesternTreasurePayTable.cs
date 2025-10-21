
using Classic;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class WesternTreasurePayTable:PaytablePanel
{
    public TextMeshProUGUI Tmp4_Page2;
    public TextMeshProUGUI Tmp1_Page1;
    private string Page2_key1 = "Rule1";
    private string Page2_key2 = "Rule2";
    private string Page2_key3 = "Rule3";
    private string Page2_key4 = "Rule4";
    private string Page3_key1 = "Coin Feature1";
    private string Page3_key2 = "Coin Feature2";
    private string Page3_key3 = "Coin Feature3";
    private string Page3_key4 = "Coin Feature4";
    protected override void Awake()
    {
        base.Awake();
        // 不在Awake中设置文本
    }

    private void Start()
    {
        // 在Start中设置，此时本地化系统通常已经初始化完成
        if (LocalizationSettings.InitializationOperation.IsDone)
        {
            SetPageInfo();
        }
        else
        {
            LocalizationSettings.InitializationOperation.Completed += (op) => SetPageInfo();
        }
    }

    private void SetPageInfo()
    {
        SetPage2Info();
        SetPage3Info();
    }

    private void SetPage2Info()
    {
        LocalizedString Tmp4_Page2_Str = new LocalizedString(LocalizationManager.Instance.tableName, Page2_key4);
        Tmp4_Page2_Str.Arguments = new object[] {"\n\n\n<size=50><sprite=1>","<sprite=0></size>\n\n\n" };
        string page2_info = $"{LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationManager.Instance.tableName, Page2_key1)}\n\n" +
                            $"{LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationManager.Instance.tableName, Page2_key2)}\n\n" +
                            $"{LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationManager.Instance.tableName, Page2_key3)}\n\n" +
                            $"{Tmp4_Page2_Str.GetLocalizedString()}";
        Tmp4_Page2.text = page2_info;
    }
    
    private void SetPage3Info()
    {
        LocalizedString Tmp1_Page1_Str = new LocalizedString(LocalizationManager.Instance.tableName, Page3_key1);
        Tmp1_Page1_Str.Arguments = new object[] { "<sprite=2>" };
        string page3_info = $"{Tmp1_Page1_Str.GetLocalizedString()}\n\n" +
                            $"{LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationManager.Instance.tableName, Page3_key2)}\n\n" +
                            $"{LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationManager.Instance.tableName, Page3_key3)}\n\n" +
                            $"{LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationManager.Instance.tableName, Page3_key4)}";
        Tmp1_Page1.text = page3_info;
    }
}
