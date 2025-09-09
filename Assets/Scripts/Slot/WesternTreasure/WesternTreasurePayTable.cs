
using Classic;
using TMPro;
using UnityEngine.Localization;

public class WesternTreasurePayTable:PaytablePanel
{
    public TextMeshProUGUI Tmp4_Page2;
    public TextMeshProUGUI Tmp1_Page1;
    public LocalizedString Tmp4_Page2_Str;
    public LocalizedString Tmp1_Page1_Str;
    protected override void Awake()
    {
        base.Awake();
        Tmp4_Page2_Str.Arguments = new object[] {"\n \n \n<size=50><sprite=1>","<sprite=0> </size>\n \n \n" };
        Tmp1_Page1_Str.Arguments = new object[] { "<sprite=2>" };
        Tmp4_Page2.text = Tmp4_Page2_Str.GetLocalizedString();
        Tmp1_Page1.text = Tmp1_Page1_Str.GetLocalizedString();
    }
}
