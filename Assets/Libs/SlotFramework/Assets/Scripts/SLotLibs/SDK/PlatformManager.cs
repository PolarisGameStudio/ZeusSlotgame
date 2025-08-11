using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Classic;
using Core;
using UnityEngine;
using MiniJSON;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Utils;
using Ads;
namespace Libs
{
    public enum MessageType
    {
        None = 0,
        CommonParam = 1,
        PrivacyPolicy =2,
        TermsofUse =3,
        PlayMergeAudio =4,
        ShowWithdraw =5,
        GetMergeReward =6,
        BuryPoint =7,
        ShowVideo = 8,
        RequestIsWhiteBao=9,
        GetUnifyCurrency=10,
        FeedBack=11,
        ShowPromotion=12,
        EnterGame=13,
        UpdateLevel=14,
        UserAmount=15,
        IsInterADReady = 16,
        IsRewardADReady = 17
    }
    
#if UNITY_IOS || UNITY_TVOS
    public class NativeAPI {
        [DllImport("__Internal")]
        public static extern string callNative(string msg);
    }
#endif
    public class PlatformManager : MonoSingleton<PlatformManager>
    {
        private bool isInit = false;
        private bool commonParamReturn = false;
        private bool isUnifyCurrencyRef = false;
        public string packageName = "";
        public string androidMethodName="";
#if UNITY_ANDROID
        private AndroidJavaClass _androidJavaClass;
        private AndroidJavaObject _androidJavaObject;
#endif
        private bool _h5InitResult = false;
        private bool _isWhiteBao = false;
        private const string M_KEY = "m";
        private const string P1_KEY = "p1";
        private const string P2_KEY = "p2";

        private const string Language_KEY = "language";
        private const string Country_KEY = "country";
        private const string NumberGK_KEY = "numberGK";
        private const string Amount_KEY = "amount";

        // Start is called before the first frame update
        protected override void Init()
        {
#if UNITY_ANDROID
            _androidJavaObject = new AndroidJavaObject(packageName);
#endif
#if !UNITY_EDITOR
            commonParamReturn = false;
            isUnifyCurrencyRef = false;
		    //调用公参
		    SendMsgToPlatFormByType(MessageType.CommonParam);
		    SendMsgToPlatFormByType(MessageType.RequestIsWhiteBao);
		    SendMsgToPlatFormByType(MessageType.GetUnifyCurrency);
#else
            commonParamReturn = true;
            isUnifyCurrencyRef = true; 
#endif
        }
        public bool IsInitSuccess()
        {
            return commonParamReturn && isUnifyCurrencyRef;
        }
        private string GetMsgKeyByType(MessageType type)
        {
            string methodKey = "";
            switch (type)
            {
                case MessageType.CommonParam:
                    methodKey =  "getCommonParm";
                    break;
                case MessageType.PrivacyPolicy:
                    methodKey =  "PrivacyPolicy";
                    break;
                case MessageType.TermsofUse:
                    methodKey =  "TermsofUse";
                    break;
                case MessageType.PlayMergeAudio:
                    methodKey =  "playMergeAudio";
                    break;
                case MessageType.GetMergeReward:
                    methodKey =  "getMergeReward";
                    break;
                case MessageType.ShowWithdraw:
                    methodKey =  "showWithdraw";
                    break;
                case MessageType.BuryPoint:
                    methodKey =  "buryPoint";
                    break;
                case MessageType.ShowVideo:
                    methodKey =  "showVideo";
                    break;
                case MessageType.RequestIsWhiteBao:
                    methodKey =  "requestIsWhiteBao";
                    break;
                case MessageType.GetUnifyCurrency:
                    methodKey =  "getUnifyCurrency";
                    break;
                case MessageType.FeedBack:
                    methodKey =  "feedback";
                    break;
                case MessageType.ShowPromotion:
                    methodKey =  "showPromotion";
                    break;
                case MessageType.EnterGame:
                    methodKey =  "enterGame";
                    break;
                case MessageType.UpdateLevel:
                    methodKey =  "updateLevel";
                    break;
                case MessageType.UserAmount:
                    methodKey = "userAmount";
                    break;
                case MessageType.IsInterADReady:
                    methodKey = "isInterReady";
                    break;
                case MessageType.IsRewardADReady:
                    methodKey = "isRewardReady";
                    break;
                default: 
                    break;
            }

            return methodKey;
        }
        
        #region UnityCallNative
        public void SendMsgToPlatFormByType(MessageType type, object para1=null,object para2 = null)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            string methodName = GetMsgKeyByType(type);
            if (!string.IsNullOrEmpty(methodName))
            {
                data[M_KEY] = methodName;
            }

            if (para1!=null)
            {
                data[P1_KEY] = para1;
            }
            if (para2!=null)
            {
                data[P2_KEY] = para2;
            }

            string jsonData = MiniJSON.Json.Serialize(data);
            Debug.Log("GetMsgFromPlatFormByType MethodName=="+methodName+"\n data==" +jsonData);
            string msg = string.Empty;
#if UNITY_EDITOR
            return;
#endif
#if UNITY_ANDROID
            if (_androidJavaObject == null)
            {
                Debug.LogError("_androidJavaObject is null");
                return;
            }
            msg = _androidJavaObject.Call<string>(androidMethodName,jsonData);
           
#elif UNITY_IOS
            msg = NativeAPI.callNative(jsonData);
#endif
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.Log("PlatformManager DecodeMsg MethodName=="+methodName+"\n msg ==" +msg);
                DecodeMsg(type,msg);
            }
        }
        
        public void DecodeMsg(MessageType type,string msg)
        {
            Dictionary<string, object> data = Json.Deserialize(msg) as Dictionary<string, object>;
            switch (type)
            {
                case MessageType.CommonParam:
                    HandleCommonParaRef(data);
                    break;
                case MessageType.PrivacyPolicy:
                    HandlePrivatePolicyRef(data);
                    break;
                case MessageType.TermsofUse:
                    HandleTermsofUseRef(data);
                    break;
                case MessageType.PlayMergeAudio:
                    HandlePlayMergeAudioRef(data);
                    break;
                case MessageType.GetMergeReward:
                    HandleGetMergeRewardRef(data);
                    break;
                case MessageType.ShowWithdraw:
                    HandleShowWithdrawRef(data);
                    break;
                case MessageType.BuryPoint:
                    HandleBuryPointRef(data);
                    break;
                case MessageType.ShowVideo:
                    HandleShowVideoRef(data);
                    break;
                case MessageType.RequestIsWhiteBao:
                    HandleRequestIsWhiteBaoRef(data);
                    break;
                case MessageType.GetUnifyCurrency:
                    HandleGetUnifyCurrencyRef(data);
                    break;
                case MessageType.FeedBack:
                    HandleFeedBackRef(data);
                    break;
                case MessageType.ShowPromotion:
                    HandleShowPromotionRef(data);
                    break;
                case MessageType.EnterGame:
                    HandleEnterGameRef(data);
                    break;
                case MessageType.UpdateLevel:
                    HandleUpdateLevelRef(data);
                    break;
                case MessageType.UserAmount:
                    HandleUserAmountRef(data);
                    break;
                default: 
                    break; 
            }
        }

        private void HandleCommonParaRef(Dictionary<string,object> data)
        {
            string Language = "";
            string Country = "";
            int numberGK = 0;

            if (data.ContainsKey(Language_KEY))
            {
                Language = Utilities.GetString(data,Language_KEY,"en");
            }
            if (data.ContainsKey(Country_KEY))
            {
                Country = Utilities.GetString(data,Country_KEY,"cn");
            }
            if (data.ContainsKey(NumberGK_KEY))
            {
                numberGK = Utilities.GetInt(data,NumberGK_KEY,0);
            }
            OnLineEarningMgr.Instance.SetCommonPara(Language,Country,numberGK);
            commonParamReturn = true;
        }
        private void HandlePrivatePolicyRef(Dictionary<string,object> data)
        {
        }
        private void HandleTermsofUseRef(Dictionary<string,object> data)
        {
        }
        private void HandlePlayMergeAudioRef(Dictionary<string,object> data)
        {
            
        }
        
        private void HandleGetMergeRewardRef(Dictionary<string,object> data)
        {
            
        }
        private void HandleShowWithdrawRef(Dictionary<string,object> data)
        {
            
        }
        private void HandleBuryPointRef(Dictionary<string,object> data)
        {
            
        }
        private void HandleShowVideoRef(Dictionary<string,object> data)
        {
            Messenger.Broadcast(ADConstants.OnPlayVideoEnd);
        }
        private void HandleRequestIsWhiteBaoRef(Dictionary<string,object> data)
        {
           
        }
        //获取货币符号
        private void HandleGetUnifyCurrencyRef(Dictionary<string,object> data)
        {
            if (!data.ContainsKey(Amount_KEY))
            {
                Debug.LogError("PlatformManager GetUnifyCurrency data not contains Amount_KEY=");
                return;
            }
            string amount = Utilities.GetString(data, Amount_KEY, "$");
            OnLineEarningMgr.Instance.SetCashSymbol(amount);
            isUnifyCurrencyRef = true;
        }
        private void HandleFeedBackRef(Dictionary<string,object> data)
        {
            
        }
        private void HandleShowPromotionRef(Dictionary<string,object> data)
        {
        }
        private void HandleEnterGameRef(Dictionary<string,object> data)
        {
        }
        private void HandleUpdateLevelRef(Dictionary<string,object> data)
        {
        }
        private void HandleUserAmountRef(Dictionary<string,object> data)
        {
        }
        #endregion

        #region 广告状态查询接口
        /// <summary>
        /// 根据传入的type查询不同广告的准备状态
        /// </summary>
        /// <param name="type"> 0--rewardAD  1--InterstitalAd </param>
        /// <returns></returns>
        public bool IsADReady(int type)
        {
            MessageType messageType = type == 0 ? MessageType.IsRewardADReady : MessageType.IsInterADReady;
            Dictionary<string, object> data = new Dictionary<string, object>();
            string methodName = GetMsgKeyByType(messageType);
            if (!string.IsNullOrEmpty(methodName))
            {
                data[M_KEY] = methodName;
            }
            string jsonData = MiniJSON.Json.Serialize(data);
            string msg = string.Empty;
#if UNITY_EDITOR
            return true;
#endif
            
#if UNITY_ANDROID
            if (_androidJavaObject == null)
            {
                Debug.LogError("_androidJavaObject is null");
                return false;
            }
            msg = _androidJavaObject.Call<string>(androidMethodName,jsonData);
#elif UNITY_IOS
            msg = NativeAPI.callNative(jsonData);
#endif
            bool isReady  = false;
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.Log("PlatformManager DecodeMsg MethodName=="+methodName+"\n msg ==" +msg);
                Dictionary<string, object> data1 = Json.Deserialize(msg) as Dictionary<string, object>;
                if (data1.ContainsKey(Amount_KEY))
                {
                    int amount = Utils.Utilities.GetInt(data1, Amount_KEY, 0);
                    isReady = amount == 1;
                }
            }
            return isReady;
        }
        /// <summary>
        /// 看广告成功回调
        /// </summary>
        /// <param name="msg">{"amount":"0||1"} 0表示是激励视频，1是全屏广告</param>
        public void ADPlayResult(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                Debug.LogError("PlatformManager ADPlayResult msg is null or empty");
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.Deserialize(msg) as Dictionary<string, object>;
            if (data == null || data.Count == 0)
            {
                Debug.LogError("PlatformManager ADPlayResult Deserialize data is null msg="+msg);
                return;
            }

            if (!data.ContainsKey(Amount_KEY))
            {
                Debug.LogError("PlatformManager ADPlayResult data not contains Amount_KEY=");
                return;
            }
            int amount = Utils.Utilities.GetInt(data, Amount_KEY, -1);
            Debug.Log("PlatformManager ADPlayResult amount ===="+amount);
            ADManager.Instance.HandlePlayVideoResult(amount);
        }
        #endregion

        
        
        #region AndroidCallUnity

        //h5初始化结果，只要调用就是成功
        public void H5InitResult()
        {
            Debug.Log("PlatformManager H5InitResult Success");
            if (_h5InitResult)
            {
                return;
            }
            _h5InitResult = true;
            Messenger.Broadcast<bool>(GameConstants.OnH5InitSuccess,_h5InitResult);
        }

        public bool CheckCanShowH5()
        {
#if UNITY_EDITOR
            return true;
#endif
            return _h5InitResult;
        }
        //h5加钱 amount表示加的钱数
        //"{"amount":"1"}"
        //
        public void H5AddCash(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                Debug.LogError("PlatformManager ReceiveMsgFromPlatform msg is null or empty");
                return;
            }
            Debug.Log("PlatformManager H5AddCash msg ===="+msg);
            Dictionary<string, object> data = Json.Deserialize(msg) as Dictionary<string, object>;
            if (data == null || data.Count == 0)
            {
                Debug.LogError("PlatformManager H5AddCash Deserialize data is null msg="+msg);
                return;
            }
            float amount = Utilities.GetFloat(data, Amount_KEY, 0f);
            if (!data.ContainsKey(Amount_KEY))
            {
                Debug.LogError("PlatformManager H5AddCash data not contains Amount_KEY=");
                return;
            }
            if (amount > 0)
            {
                OnLineEarningMgr.Instance.HandleH5Event(amount);
            }
        }

        //H5退出
        public void H5State(string msg)
        {
            //广播h5退出状态
            Messenger.Broadcast(GameConstants.OnExitH5);
        }
        
        //h5初始化结果，只要调用就是成功
        public void SetOrientation(string msg)
        {
            Debug.Log("PlatformManager SetOrientation");
            //设置转向
            if (string.IsNullOrEmpty(msg))
            {
                Debug.LogError("PlatformManager ADPlayResult msg is null or empty");
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.Deserialize(msg) as Dictionary<string, object>;
            if (data == null || data.Count == 0)
            {
                Debug.LogError("PlatformManager ADPlayResult Deserialize data is null msg="+msg);
                return;
            }

            if (!data.ContainsKey(Amount_KEY))
            {
                Debug.LogError("PlatformManager ADPlayResult data not contains Amount_KEY=");
                return;
            }
            int amount = Utils.Utilities.GetInt(data, Amount_KEY, 0);
            Debug.Log("PlatformManager SetOrientation amount ===="+amount);
            if (amount==0)
            {
                //切换横屏
                SkySreenUtils.CurrentOrientation = ScreenOrientation.LandscapeLeft;
            }
            else
            {
                //切换竖屏
                SkySreenUtils.CurrentOrientation = ScreenOrientation.Portrait;
            }
        }
        #endregion
        
         public string SwitchLanguage()
        {
            string languageCode = "";
            switch (OnLineEarningMgr.Instance.GetLanguage())
            {
                case "cs":
                    languageCode = "Czech (cs)";
                    break;
                case "nl":
                    languageCode = "Dutch (nl)";
                    break;
                case "en":
                    languageCode = "English (en)";
                    break;
                case "tl":
                    languageCode = "Filipino (fil)";
                    break;
                case "fr":
                    languageCode = "French (fr)";
                    break;
                case "de":
                    languageCode = "German (de)";
                    break;
                case "hi":
                    languageCode = "Hindi (hi)";
                    break;
                case "id":
                    languageCode = "Indonesian (id)";
                    break;
                case "it":
                    languageCode = "Italian (it)";
                    break;
                case "ja":
                    languageCode = "Japanese (ja)";
                    break;
                case "ko":
                    languageCode = "Korean (ko)";
                    break;
                case "ms":
                    languageCode = "Malay (ms)";
                    break;
                case "pl":
                    languageCode = "Polish (pl)";
                    break;
                case "pt":
                    languageCode = "Portuguese (pt)";
                    break;
                case "ro":
                    languageCode = "Romanian (ro)";
                    break;
                case "ru":
                    languageCode = "Russian (ru)";
                    break;
                case "es":
                    languageCode = "Spanish (es)";
                    break;
                case "th":
                    languageCode = "Thai (th)";
                    break;
                case "tr":
                    languageCode = "Turkish (tr)";
                    break;
                case "vi":
                    languageCode = "Vietnamese (vi)";
                    break;
                default:
                    languageCode = "English (en)";
                    break;
            }

            return languageCode;
        }
         public bool IsWhiteBao()
         {
             return OnLineEarningMgr.Instance.IsWhiteBao();
         }
    }
}

