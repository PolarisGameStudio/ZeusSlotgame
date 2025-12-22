using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Libs;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LocalizationManager: MonoSingleton<LocalizationManager>
{

    public string tableName = "CandyFantasyString";
    
    private Coroutine startCor = null;
    private bool isInitSuccess = false;

    public bool IsInitSuccess()
    {
        return isInitSuccess;
    }

    protected override void Init()
    {
        base.Init();
        startCor = StartCoroutine(LoadLocalization());
    }
    IEnumerator LoadLocalization()
    {
        // 等待Localization插件初始化完成
        yield return LocalizationSettings.InitializationOperation;
        
        string localName = PlatformManager.Instance.SwitchLanguage();
        Locale needChangeLocal = null;
        // 预加载其他所有语言的字符串表（遍历AvailableLocales）
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (localName == locale.name)
            {
                Debug.Log($"[LocalizationManager] [LoadLocalization] localName:{localName}");
                needChangeLocal = locale;
                break;
            }
        }
        if (needChangeLocal != null)
        {
            AsyncOperationHandle<StringTable> defaultTableLoad = 
                LocalizationSettings.StringDatabase.GetTableAsync(tableName, needChangeLocal);
            
            yield return defaultTableLoad;
            
            yield return GameConstants.FrameTime;
            //切换语言
            LocalizationSettings.SelectedLocale = needChangeLocal;
        }
        isInitSuccess = true;
    }
    
    // 通过索引切换语言（适用于下拉菜单）
    public void SwitchLanguageByIndex(int languageIndex)
    {
        if (LocalizationSettings.AvailableLocales.Locales.Count > languageIndex)
        {
            // 设置目标语言
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIndex];
            // 刷新所有LocalizeStringEvent组件（可选）
            StartCoroutine(RefreshLocalizedTexts());
        }
    }

    // 通过语言名称切换（如"zh"/"en"）
    public void SwitchLanguageByName(string languageCode)
    {
        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (locale.Identifier.Code == languageCode)
            {
                LocalizationSettings.SelectedLocale = locale;
                StartCoroutine(RefreshLocalizedTexts());
                break;
            }
        }
    }

    // 协程：等待语言切换完成并刷新UI文本
    private IEnumerator RefreshLocalizedTexts()
    {
        // 等待一帧确保语言切换完成
        yield return new WaitForEndOfFrame();
        // 遍历场景中所有LocalizeStringEvent组件并刷新
        var allLocalizeEvents = FindObjectsOfType<LocalizeStringEvent>();
        foreach (var localizeEvent in allLocalizeEvents)
        {
            localizeEvent.RefreshString();
        }
    }

    protected void OnDisable()
    {
        if (startCor!=null)
        {
            StopCoroutine(startCor);
            startCor = null;
        }
    }

    public List<string> GetPlatFormSpriteResourcePath()
    {
        // string platformImagePath = "Platform/";
        List<int> index = GetPlatFormSpriteIndex();
        List<string> res = new List<string>();
        for (int i = 0; i < index.Count; i++)
        {
            res.Add(index[i].ToString());
        }
        return res;
    }

    public List<int> GetPlatFormSpriteIndex()
    {
        List<int> index = null;
        switch (OnLineEarningMgr.Instance.GetCountry())
        {
            case CountryType.BR:
                index= new List<int>(){1,2,4};
                break;
            case CountryType.IN:
                index= new List<int>(){9,10,26};
                break;
            case CountryType.EN:
                index= new List<int>(){8,5,7};
                break;
            case CountryType.ID:
                index= new List<int>(){9,11,12};
                break;
            case CountryType.JP:
                index= new List<int>(){19,20,21};
                break;
            case CountryType.DE:
                index= new List<int>(){13,14,15};
                break;
            case CountryType.KR:
                index= new List<int>(){22,23,24};
                break;
            case CountryType.RU:
                index= new List<int>(){16,17,18};
                break;
            case CountryType.VN:
                index= new List<int>(){28,33,0};
                break;
            case CountryType.TR:
                index= new List<int>(){8,38,6};
                break;
            case CountryType.TH:
                index= new List<int>(){8,39,6};
                break;
            case CountryType.PH:
                index= new List<int>(){40,41,42};
                break;
            case CountryType.MY:
                index= new List<int>(){8,44,6};
                break;
            default:
                index= new List<int>(){8,7,5};
                break;
        }

        return index;
    }
}
