using UnityEngine;
using UnityEngine.UI;
using Libs;

namespace Classic
{
  public class SettingDialogBase : UIDialog 
  {
    [HideInInspector]
    public UnityEngine.UI.ToggleButton ToggleSound;
    [HideInInspector]
    public UnityEngine.UI.ToggleButton ToggleScreenLock;
    [HideInInspector]
    public UnityEngine.UI.Image Checkmark;
    [HideInInspector]
    public UnityEngine.UI.Button TermsButton;
    [HideInInspector]
    public UnityEngine.UI.Button PolicyButton; 
    [HideInInspector]
    public UnityEngine.UI.Button FeedBackButton;
    

    private const string MID_ZONE_CONTENT_KEY = "Anchor/MidZone/Viewport/Content/";
    protected override void Awake()
    {
        base.Awake();
        this.ToggleSound = Util.FindObject<UnityEngine.UI.ToggleButton>(transform,MID_ZONE_CONTENT_KEY+"Sounds/Toggle/");
        UGUIEventListener.Get(this.ToggleSound.gameObject).onToggleChanged = this.onToggleSelect;
        this.ToggleScreenLock = Util.FindObject<UnityEngine.UI.ToggleButton>(transform,MID_ZONE_CONTENT_KEY+"ScreenLock/Toggle/");
        UGUIEventListener.Get(this.ToggleScreenLock.gameObject).onToggleChanged = this.onToggleSelect;
        this.Checkmark = Util.FindObject<UnityEngine.UI.Image>(transform,MID_ZONE_CONTENT_KEY+"ScreenLock/Toggle/Background/Checkmark/");
        this.TermsButton = Util.FindObject<UnityEngine.UI.Button>(transform,"Anchor/About/TermsButton/");
        UGUIEventListener.Get(this.TermsButton.gameObject).onClick = this.OnButtonClickHandler;
        this.PolicyButton = Util.FindObject<UnityEngine.UI.Button>(transform,"Anchor/About/PolicyButton/");
        UGUIEventListener.Get(this.PolicyButton.gameObject).onClick = this.OnButtonClickHandler;
        this.FeedBackButton = Util.FindObject<UnityEngine.UI.Button>(transform,"Anchor/About/FeedBackButton/");
        UGUIEventListener.Get(this.FeedBackButton.gameObject).onClick = this.OnButtonClickHandler;
        if(this.BtnClose == null)
        {
          this.BtnClose = Util.FindObject<UnityEngine.UI.Button>(transform,"Anchor/BtnClose/");
          UGUIEventListener.Get(this.BtnClose.gameObject).onClick = this.BtnCloseClick;
        }
#if UNITY_IOS
      TermsButton.gameObject.SetActive(false);
#endif
      
    }
  }
}
