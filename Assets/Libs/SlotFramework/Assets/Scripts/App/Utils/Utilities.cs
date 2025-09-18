using System.IO;
using UnityEngine;

namespace Utils
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Waiting.UGUI.Collections;
	using DG.Tweening;
    using System.Text.RegularExpressions;
    public static class Utilities
	{
		private const string WarningMissingParameter = "Did not find expected value '{0}' in dictionary";

		public static bool TryGetValue<T>(
			this IDictionary<string, object> dictionary,
			string key,
			out T value)
		{
			object resultObj;
			if (dictionary.TryGetValue(key, out resultObj) && resultObj is T)
			{
				value = (T)resultObj;
				return true;
			}
			
			value = default(T);
			return false;
		}
		
		public static long TotalSeconds(this DateTime dateTime)
		{
			TimeSpan t = dateTime - new DateTime(1970, 1, 1);
			long secondsSinceEpoch = (long)t.TotalSeconds;
			return secondsSinceEpoch;
		}
		
		public static T GetValueOrDefault<T>(
			this IDictionary<string, object> dictionary,
			string key,
			bool logWarning = true)
		{
			T result;
			if (!dictionary.TryGetValue<T>(key, out result))
			{
//				FacebookLogger.Warn(WarningMissingParameter, key);
			}
			
			return result;
		}
		
		public static string ToCommaSeparateList(this IEnumerable<string> list)
		{
			if (list == null)
			{
				return string.Empty;
			}
			
			return string.Join(",", list.ToArray());
		}
		
		public static string AbsoluteUrlOrEmptyString(this Uri uri)
		{
			if (uri == null)
			{
				return string.Empty;
			}
			
			return uri.AbsoluteUri;
		}
        
        public static int CastValueInt(object o,int defaultValue = 0){
			if (o == null) {
				return defaultValue;
			}

            if (o is int) {
                return ((int)o);
            } else if (o is long) {
                return (int)((long)o);
            } else if (o is double) {
                return (int)((double)o);
            } else if (o is float) {
                return (int)((float)o);
            } else {
                int result = defaultValue;
                if (int.TryParse(o.ToString(), out result)) {
                    return result;
                }
            }
            return defaultValue;
        }

		public static bool CastValueBool(object o, bool defaultValue = false)
		{
			if (o == null) {
				return defaultValue;
			}

            if (o is bool) {
                return ((bool)o);
            } else if (o is int) {
                return ((int)o) > 0;
            } else if (o is long) {
                return (int)((long)o) > 0;
            } else if (o is double) {
                return (int)((double)o) > 0;
            } else if (o is float) {
                return (int)((float)o) > 0;
            } else {
                bool result = defaultValue;
                if (bool.TryParse(o.ToString(), out result)) {
                    return result;
                }
            }
            return defaultValue;
        }

		public static  float CastValueFloat(object o,float defaultValue = 0){
			if (o == null) {
				return defaultValue;
			}

            if (o is int) {
                return (float)((int)o);
            } else if (o is long) {
                return (float)((long)o);
            } else if (o is double) {
                return (float)((double)o);
            } else if (o is float) {
                return ((float)o);
            } else {
                float result = defaultValue;
                if (float.TryParse(o.ToString(), out result)) {
                    return result;
                }
            }
            return defaultValue;
		}

		public static  long CastValueLong(object o,long defaultValue = 0){
			if (o == null) {
				return defaultValue;
			}

            if (o is int) {
                return (long)((int)o);
            } else if (o is long) {
                return ((long)o);
            } else if (o is double) {
                return (long)((double)o);
            } else if (o is float) {
                return (long)((float)o);
            } else {
                long result = defaultValue;
                if (long.TryParse(o.ToString(), out result)) {
                    return result;
                }
            }
            return defaultValue;
		}

		public static  double CastValueDouble(object o,double defaultValue = 0){
			if (o == null) {
				return defaultValue;
			}

            if (o is int) {
                return (double)((int)o);
            } else if (o is long) {
                return (double)((long)o);
            } else if (o is double) {
                return ((double)o);
            } else if (o is float) {
                return (double)((float)o);
            } else {
                double result = defaultValue;
                if (double.TryParse(o.ToString(), out result)) {
                    return result;
                }
            }
			return defaultValue;
		}


		public static float  GetFloat(Dictionary<string,object> dict, string key, float defaultValue)
		{
			if (dict!= null && dict.ContainsKey(key)) {
				return Utilities.CastValueFloat(dict[key],defaultValue);
			} else {
				return defaultValue;
			}
		}

		public static double  GetDouble(Dictionary<string,object> dict, string key, double defaultValue)
		{
			if (dict!= null && dict.ContainsKey(key)) {
				return Utilities.CastValueDouble(dict[key],defaultValue);
			} else {
				return defaultValue;
			}
		}

		public static long  GetLong(Dictionary<string,object> dict, string key, long defaultValue)
		{
			if (dict!= null && dict.ContainsKey(key)) {
				return Utilities.CastValueLong(dict[key],defaultValue);
			} else {
				return defaultValue;
			}
		}

		public static int  GetInt(Dictionary<string,object> dict, string key, int defaultValue=0)
		{
			if (dict!= null && dict.ContainsKey(key)) {
				return Utilities.CastValueInt(dict[key],defaultValue);
			} else {
				return defaultValue;
			}
		}

		public static bool  GetBool(Dictionary<string,object> dict, string key, bool defaultValue)
		{
			if (dict!= null && dict.ContainsKey(key)) {
				return Utilities.CastValueBool(dict[key],defaultValue);
			} else {
				return defaultValue;
			}
		}
		
		public static string GetString(Dictionary<string, object> dict, string key, string defaultValue)
		{
			if (dict != null && dict.ContainsKey(key))
			{
				return dict[key].ToString();
			}
			return defaultValue;
		}
		
		public static T GetValue<T>(Dictionary<string,object> dict, string key, T defaultValue)
		{
			if (dict!= null && dict.ContainsKey(key)) {
				try{
					return (T) dict[key];
				}
				catch(Exception e) {
					return defaultValue;
				}
			} else {
				return defaultValue;
			}
		}

		#region 一维和二维List与字符串互换
		public static List<T> StrToList<T>(string str) {
			List<T> lists = new List<T>();
			if (!string.IsNullOrEmpty(str)) {
				string[] strs;
				strs = str.Split(',');
				foreach (string s in strs) {
					if (typeof(T) == typeof(int)) {
						int n = int.Parse(s);
						object obj = (object)n;
						lists.Add((T)obj);
					} 
					else if (typeof(T) == typeof(float)) {
						float n = float.Parse(s);
						object obj = (object)n;
						lists.Add((T)obj);
					} 
					else if (typeof(T) == typeof(string)) {
						object obj = (object)s;
						lists.Add((T)obj);
					}
					else if (typeof(T)==typeof(bool)) {
						bool n = bool.Parse(s);
						object obj = (object)n;
						lists.Add ((T)obj);
					}
				}
			}
			return lists;
		}

		public static string ListToStr<T>(List<T> list){
			string str = string.Empty;
			if (list!=null&&list.Count>0) {
				for (int i = 0; i < list.Count; i++) {
					if (typeof(T) == typeof(int)) {
						str += list[i].ToString ();
					} 
					else if (typeof(T) == typeof(float)) {
						str += list[i].ToString();
					}
					else if (typeof(T) == typeof(string)) {
						str += list[i];
					}
					else if (typeof(T) == typeof(bool)) {
						str += list[i].ToString();
					}
					if (i<list.Count-1) {
						str += ",";
					}
				}
			}
			return str;
		}

		public static string ConvertListToString<T>(List<List<T>> list){
			if (list==null||list.Count==0) {
				return "";
			}
			string str = "";
			for (int i = 0; i < list.Count; i++) {
				for (int j = 0; j < list[i].Count; j++) {
					str += list [i][j].ToString();
					if (j<list[i].Count-1) {
						str += ",";
					}
				}

				if (i < list.Count-1) {
					str += ";";
				}
			}
			return str;
		}

		public static List<List<T>> ConvertStringToList<T>(string str){
			string[] OutArray = str.Split (';');
			List<List<T>> lists = new List<List<T>> ();
			for (int i = 0; i < OutArray.Length; i++) {
				string[] InArray = OutArray [i].Split (',');
				lists.Add(new List<T> ());
				for (int j = 0; j < InArray.Length; j++) {
					if (typeof(T)==typeof(bool)) {
						bool b = bool.Parse (InArray[j]);
						lists [i].Add ((T)(object)b);
					}
					else if (typeof(T)==typeof(int)) {
						int n = int.Parse (InArray[j]);
						lists [i].Add ((T)(object)n);
					}
					else if (typeof(T)==typeof(string)) {
						string st = InArray[j].ToString();
						lists [i].Add ((T)(object)st);
					}
					else if (typeof(T)==typeof(float)) {
						float st = float.Parse (InArray[j]);
						lists [i].Add ((T)(object)st);
					}
					else if(typeof(T)==typeof(long))
					{
						long l = long.Parse (InArray[j]);
						lists [i].Add ((T)(object)l);
					}
				}
			}
			return lists;
		}

		#endregion
     

		public static string ListToStr(List<string> list) {
            if (list == null) {
                return string.Empty;
            }

            return string.Join(",", list.ToArray());
        }

		/// <summary>
		/// Thousands the separator number.
		/// </summary>
		/// <returns>The separator number.</returns>
		/// <param name="o">O.</param>
		/// <param name="needMultiple">If set to <c>true</c> 是否需要显示的加倍 need multiple.</param>
		public static string ThousandSeparatorNumber(object o , bool needMultiple = true){
			double oldNumber = CastValueDouble (o);
			if (needMultiple && (!Classic.UserManager.GetInstance().UserProfile().isOlderUser)) {
				oldNumber *= Core.ApplicationConfig.GetInstance ().ShowCoinsMultiplier;
			}

			oldNumber = Math.Round(oldNumber);

			long number = CastValueLong (oldNumber);
			if (number > 99) {
//				return  string.Format ("{0:0,0}", number);	
				String str= StaticString.DefaultStaticStr.SetThousand (number);

				return str;
//				return StaticString.DefaultString.SetThousand(number);
//				return StaticString.DefaultString.Value;
			} else {
				return number.ToString ();
			}
		}


        public static string ThousandSeparatorNumberNoMultiplier(object o, bool needMultiple = true) {
            double oldNumber = CastValueDouble(o);
            long number = CastValueLong(oldNumber);
            if (number > 99) {
                //				return  string.Format ("{0:0,0}", number);	
                String str = StaticString.DefaultStaticStr.SetThousand(number);

                return str;
                //				return StaticString.DefaultString.SetThousand(number);
                //				return StaticString.DefaultString.Value;
            } else {
                return number.ToString();
            }
        }

        public static string TestPercentString(float floatV)
        {
            System.Globalization.NumberFormatInfo provider = new System.Globalization.NumberFormatInfo();
            provider.PercentDecimalDigits = 0; //小数点保留几位数. 
            provider.PercentPositivePattern = 1; //百分号出现在何处.
            return floatV.ToString("P", provider);
        }

        public static string ThousandSeparator(Object o,bool needMultiple = true){
//			long number = CastValueLong (o);
//            return  string.Format ("{0:0,0}", o);
			return ThousandSeparatorNumber(o,needMultiple);
        }

		public static string CastToIntStringWithThousandSeparator(Object obj,bool needMultiple = true){
			return ThousandSeparatorNumber(obj,needMultiple);
//			int intValue = CastValueInt (obj);
//			return intValue.ToString ("N0");
		}

		public static string CastToPercentageString(object obj, string prefix) {
			float floatValue = CastValueFloat (obj);
			int intValue = (int)(floatValue * 100.0f);
			return string.Format ("{0}{1}%", prefix, intValue.ToString());
		}

        public static int GetCountInDictList(Dictionary<int,List<int>> dictList){
            if (dictList == null) {
                return 0;
            }
            int count = 0;
            foreach(int key in dictList.Keys){
                if (dictList [key] != null) {
                    count += dictList [key].Count;
                }
            }
            return count;
        }

		public static int GetRoundInt(float value) {
			return Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));
		}

		public static string GetBigNumberShow(long num, bool needMultiple = true, float ignoreNumber = 1000, int saveDecimal = 1)
		{
			// 是否为负数
			bool isNegative = num < 0;
			double tmp = Math.Abs((double)num); // 先取绝对值用于计算

			if (needMultiple && (!Classic.UserManager.GetInstance().UserProfile().isOlderUser))
			{
				tmp = tmp * Core.ApplicationConfig.GetInstance().ShowCoinsMultiplier;
			}

			// 如果原来是负数，加回负号
			string prefix = isNegative ? "-" : "";

			double thousand = tmp / 1000f;
			if (tmp < ignoreNumber)
			{
				return prefix + ThousandSeparator(tmp, false);
			}
			else if (thousand < 1000)
			{
				double thousandShow = Math.Round(thousand, saveDecimal);
				return prefix + thousandShow + "K";
			}
			else if (thousand < 1000000)
			{
				double million = Math.Round(thousand / 1000, saveDecimal);
				return prefix + million + "M";
			}
			else if (thousand < 1000000000)
			{
				double billion = Math.Round(thousand / 1000000, saveDecimal);
				return prefix + billion + "B";
			}
			else if (thousand < 1000000000000)
			{
				double trillion = Math.Round(thousand / 1000000000, saveDecimal);
				return prefix + trillion + "T";
			}
			else
			{
				double quadrillion = Math.Round(thousand / 1000000000000, saveDecimal);
				return prefix + quadrillion + "Q";
			}
		}
		
		/**
         * 金币缩写
         * 尽可能的展示更多的数字
         * 如果增加千分符之后文本长度超过了传入的文本框显示最大字符长度,则进行最小限度缩写,向下取整
         */
		public static string GetFixedMaxLengthBigNumberShow(long num, int maxLength, bool needMultiple = true)
		{
			if (needMultiple && (!Classic.UserManager.GetInstance().UserProfile().isOlderUser))
			{
				num *= Core.ApplicationConfig.GetInstance().ShowCoinsMultiplier;
			}

			String str = StaticString.DefaultStaticStr.SetThousand(num);
			int length = str.Length;

			if (length < maxLength) return str;
			if (length < maxLength + 4)
			{
				double showNum = Utils.Utilities.CastValueLong((double) num / 1000);
				string strNum = StaticString.DefaultStaticStr.SetThousand((long)showNum);
				return strNum + "K";
			} else if (length < maxLength + 8)
			{
				double showNum = Utils.Utilities.CastValueLong((double) num / 1000000);
				string strNum = StaticString.DefaultStaticStr.SetThousand((long)showNum);
				return strNum + "M";
			}else if (length < maxLength + 12)
			{
				double showNum = Utils.Utilities.CastValueLong((double) num / 1000000000);
				string strNum = StaticString.DefaultStaticStr.SetThousand((long)showNum);
				return strNum + "B";
			}else if (length < maxLength + 16)
			{
				double showNum = Utils.Utilities.CastValueLong((double) num / 1000000000000);
				string strNum = StaticString.DefaultStaticStr.SetThousand((long)showNum);
				return strNum + "T";
			}else 
			{
				double showNum = Utils.Utilities.CastValueLong((double) num / 1000000000000000);
				string strNum = StaticString.DefaultStaticStr.SetThousand((long)showNum);
				return strNum + "Q";
			}
		}

		#region 金币显示规则

		 /// <summary>
        /// 缩写展示金币
        /// </summary>
        /// <param name="num">金币基数</param>
        /// <param name="clampDec">1为向下取整，2为向上取整，包括小数(1 : 2.345 = 2.34)</param>
        /// <param name="limitLength">限制显示长度包含单位(123.4k  6)</param>
        /// <returns></returns>
        public static string GetAbbreviationNumShow(long num, int clampDec = 1, int limitLength = 6,float ignoreNum = 1000)
        {
            string showNum = GetBigNumberShowWithDecimal(num, clampDec, limitLength - 1,ignoreNumber:ignoreNum);
            return showNum;
        }
        
        /// <summary>
        /// 金币显示接口（可以限制最大显示数量小数点也算一个单位，可以控制保留多少小数，优先以最大限制数量为准）
        /// </summary>
        /// <param name="num">金币基数</param>
        /// <param name="clampDec">取整方式,1向下取整,2为向上取整</param>
        /// <param name="limitLength">限制长度不包含单位</param>
        /// <param name="numList">单位列表</param>
        /// <param name="digits">保留小数位</param>
        /// <param name="ignoreNumber">忽略数量</param>
        /// <param name="needMultiple">需要倍数</param>
        /// <returns></returns>
        private static string GetBigNumberShowWithDecimal(long num, int clampDec = 1, int limitLength = -1, string numList = "KMBTQ", int digits = 2, float ignoreNumber = 1000, bool needMultiple = true)
        {
            if (needMultiple && (!Classic.UserManager.GetInstance().UserProfile().isOlderUser)) {
                num *= Core.ApplicationConfig.GetInstance().ShowCoinsMultiplier;
            }

            if (num < ignoreNumber)
            {
                return ThousandSeparator(num, needMultiple ? false : true);
            }

            if (string.IsNullOrEmpty(numList)) numList = "KMBTQ";
            string finalStr = "";
            double thousand = CastValueDouble( num ,-1);
            if (thousand == -1) return finalStr;
            
            int temp = (int)Math.Floor(Math.Log(thousand, 1000));
            if(temp > numList.Length - 1) return ThousandSeparator(GetNearlyNum(thousand / Math.Pow(1000, numList.Length),clampDec,digits), needMultiple ? false : true) + numList[numList.Length - 1];
            
            double targetNum = GetNearlyNum(thousand / Math.Pow(1000, temp),clampDec,digits);
            finalStr = targetNum.ToString();
            
            if (limitLength != -1)
            {
                finalStr = finalStr.Substring(0, Mathf.Clamp(limitLength, 0, finalStr.Length));
                if (finalStr.EndsWith(".")) finalStr = finalStr.Remove(finalStr.Length - 1);
            }
            
            finalStr += numList[temp - 1 < 0 ? 0 : temp - 1];
            return finalStr;
        }
        
        private static double GetNearlyNum(double num, int type, int digits)
        {
	        int multiple = (int)Mathf.Pow(10, digits);
	        double temp = num * multiple;
	        return GetRoundByType(temp, type)/multiple;
        }
        
        /// <summary>
        /// 根据类型取整
        /// </summary>
        /// <returns></returns>
        public static double GetRoundByType(double number, int type, int digits = 1)
        {
	        switch (type)
	        {
		        case 0:
			        return Math.Round(number, digits);
		        case 1:
			        return Math.Floor(number);
		        case 2:
			        return Math.Ceiling(number);
	        }

	        return number;
        }

		#endregion

		#region MyRegion

		public const float Hundred = 100;
		public const float HunK = 1000 * Hundred;
		public const float HunM = 1000 * HunK;
		public const float HunB = 1000 * HunM;
		public const float HunT = 1000 * HunB;
		public const float HunQ = 1000 * HunT;
		
		public const float OneK = 1000;
		public const float OneM = 1000 * OneK;
		public const float OneB = 1000 * OneM;
		public const float OneT = 1000 * OneB;
		public const float OneQ = 1000 * OneT;
		
		public const string Format_HunQ = "{0:###.#}Q";
		public const string Format_Q = "{0:##.##}Q";
		
		public const string Format_HunT = "{0:###.#}T";
		public const string Format_T = "{0:##.##}T";
		
		public const string Format_HunB = "{0:###.#}B";
		public const string Format_B = "{0:##.##}B";

		public const string Format_HunM = "{0:###.#}M";
		public const string Format_M = "{0:##.##}M";

		public const string Format_HunK = "{0:###.#}K";
		public const string Format_K = "{0:##.##}K";

		public const string Format_Hundred = "{0:###.#}";
		public const string Format_Num = "{0:##.##}";
		public const string Format_Zero = "{0}";

		public static string GetFormatNumShow(long num,bool needMultiple = true,float ignoreNumber = 1000)
		{
			if (needMultiple && (!Classic.UserManager.GetInstance().UserProfile().isOlderUser)) {
				num *= Core.ApplicationConfig.GetInstance ().ShowCoinsMultiplier;
			}

			if (num >= HunQ) {
				return string.Format(Format_HunQ, num / OneQ);
			}else if (num >= OneQ) {
				return string.Format(Format_Q,num / OneQ);
			} if (num >= HunT) {
				return string.Format(Format_HunT, num / OneT);
			} else if (num >= OneT) {
				return string.Format(Format_T,num / OneT);
			} if (num >= HunB){
				return string.Format(Format_HunB,num / OneB);
			}else if (num >= OneB){
				return string.Format(Format_B,num / OneB);
			} else if (num >= HunM){
				return string.Format(Format_HunM,num / OneM);
			}else if (num >= OneM){
				return string.Format(Format_M,num / OneM);
			}  else if (num >= HunK){
				return string.Format(Format_HunK,num / OneK);
			} else if (num >= OneK){
				return string.Format(Format_K,num / OneK);
			} else if (num >= Hundred){
				return string.Format(Format_Hundred,num);
			} else if (num > 0){
				return string.Format(Format_Num,num);
			} else{
				return string.Format(Format_Zero,num);
			}
		}
        #endregion

        public static void SetTextMeshProUGUIText(TMPro.TextMeshProUGUI tgtText, string strText)
        {
            if (tgtText == null) return;
            tgtText.text = strText;
        }

        //
        public static string GetDisPlayNumberWithMaxNum(long num, long maxNum, bool needMultiple = true)
        {
	        if (needMultiple && (!Classic.UserManager.GetInstance().UserProfile().isOlderUser)) {
		        num *= Core.ApplicationConfig.GetInstance().ShowCoinsMultiplier;
	        }
	        if (num > maxNum)
	        {
		        return ThousandSeparatorNumber(num / 1000, false) + "K";
	        }
	        else
	        {
		        return ThousandSeparatorNumber(num, false);
	        }
        }
        
        public static string GetThousandNumber(long num, bool needMultiple = true) {
            if (needMultiple && (!Classic.UserManager.GetInstance().UserProfile().isOlderUser)) {
                num *= Core.ApplicationConfig.GetInstance().ShowCoinsMultiplier;
            }
            double thousand = (double)num / 1000f;
            return thousand + "K";
        }
        public static string GetFileNameFromURL(string strUrl) {
            string strRet = null;
            if (!string.IsNullOrEmpty(strUrl)) {
                if (strUrl.StartsWith(GameConstants.HTTP_Key) || strUrl.StartsWith(GameConstants.HTTPS_Key)) {
                    System.Uri uri = new Uri(strUrl);
                    strRet = System.IO.Path.GetFileNameWithoutExtension(uri.LocalPath);
                }
            }
            return strRet;
        }

        public static bool IsNumber(object o)
        {
            if (o == null) return false;
            return (o is int) || (o is long) || (o is double) || (o is float);
        }
        public static bool IsInteger(string text)
        {
            Regex reg = new Regex("^[0-9]+$");
            Match ma = reg.Match(text);
            return ma.Success;
        }

        public static bool MergeDictAtoDictB(Dictionary<string, object> DictA, Dictionary<string, object> DictB)
        {
	        if (DictA == null) return false;
            string[] keys = DictA.Keys.ToArray();
//            foreach (string key in DictA.Keys) {
            for(int i = 0; i < keys.Length;i++)
            {
                var key = keys[i];
                if (DictA[key] == null) continue;

                if (DictA[key] is Dictionary<string, object>) {
                    Dictionary<string, object> valueBDict = Utilities.GetValue<Dictionary<string, object>>(DictB, key, null);
                    //value 类型不同不替换,以 DictB为准
                    if (valueBDict != null) {
                        if (!Utilities.MergeDictAtoDictB(DictA[key] as Dictionary<string, object>, valueBDict)) return false;
                        continue;
                    }
                }
                var targetValue = DictA[key];

                if (DictB.TryGetValue(key, out var oldValue))
                {

                    if (IsNumber(oldValue) && !IsNumber(targetValue) || oldValue.GetType() != targetValue.GetType())
                    {
                        object tempValue = null;
                        switch (DictB[key])
                        {
                            case bool _:
                            {
                                if (bool.TryParse(targetValue.ToString(), out var temp))
                                {
                                    tempValue = temp;
                                }

                                break;
                            }

                            case int _:
                            case long _:
                            {
                                if (long.TryParse(targetValue.ToString(), out var temp))
                                {
                                    tempValue = temp;
                                }

                                break;
                            }

                            case float _:
                            case double _:
                            {
                                if (double.TryParse(targetValue.ToString(), out var temp))
                                {
                                    tempValue = temp;
                                }

                                break;
                            }
                        }

                        targetValue = tempValue;
                    }
                }

                if (targetValue != null)
                {
                    DictB[key] = targetValue;
                }
                else
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("ErrorKey", key);
                    BaseGameConsole.ActiveGameConsole().LogBaseEvent("ConfigMergeError", parameters);
                }
            }

            return true;
        }
        /// <summary>
        ///非覆盖的形式，遇到相同节点则不替换,没有的节点用a的
        /// </summary>
        /// <param name="DictA"></param>
        /// <param name="DictB"></param>
        /// <returns></returns>
        public static void MergeDictAtoBNoCover(Dictionary<string,object> DictA,Dictionary<string,object> DictB)
        {
	        if (DictA == null) return ;
	        // Debug.Log(MiniJSON.Json.Serialize(DictA));
	        List<string> keys = new List<string>(DictA.Keys);
	        for (int i = 0; i < keys.Count; i++)
	        {
		        string key = keys[i];
		        if (DictA[key] == null) continue;

		        if (DictA[key] is Dictionary<string,object>)
		        {
			        Dictionary<string,object> valueBDict = Utilities.GetValue<Dictionary<string,object>>(DictB, key, null);
			        //value 类型不同不替换,以 DictB为准
			        if (valueBDict != null)
			        {
				        MergeDictAtoBNoCover(DictA[key] as Dictionary<string,object>, valueBDict) ;
			        }
			        else
			        {
				        DictB[key] = DictA[key];
			        }
		        }

		        //DictB   只有 包含key且和DictA[key]类型相同,条件满足则不merge，其他都merge
		        bool hasSameNodeValue = true;
		        if (DictB.ContainsKey(key)){
			        if (DictB[key] != null) {
				        if (IsNumber(DictB[key])) hasSameNodeValue = IsNumber(DictA[key]);
				        else
				        {
					        Type typeB = DictB[key].GetType();
					        hasSameNodeValue = typeB.Equals(DictA[key].GetType());
				        }
			        } else hasSameNodeValue = false;
		        }
		        else
		        {
			        hasSameNodeValue = false;
		        }

		        //有的话替换,没有的话添加，已经在底层不为dict的情况
		        if (!hasSameNodeValue) {
			        DictB[key] = DictA[key];
		        }

	        }
	        return;
        }
		/// <summary>
		/// Logs the plist error to server.
		/// </summary>
		/// <param name="errorMessage">Error message.</param>
		public static void LogPlistError(string errorMessage)
		{
			
			#if UNITY_EDITOR || DEBUG
            errorMessage += "Please contact PM to check the plist.";
			UnityEngine.Debug.LogError(errorMessage);
            Dictionary<string, object> errorContent = new Dictionary<string, object>();
            errorContent.Add ("errorMessage",errorMessage);
            BaseGameConsole.singletonInstance.LogBaseEvent ("Plist_Error",errorContent);
			#endif
			
		}

		public static void LogInfo(string msg,string color = "green")
		{
#if UNITY_EDITOR || DEBUG
			UnityEngine.Debug.Log("<color="+color+">"+msg+"</color>");
			Dictionary<string, object> content = new Dictionary<string, object>();
			content.Add ("info",msg);
			BaseGameConsole.singletonInstance.LogBaseEvent ("LogInfo",content);
#endif
		}
		/// <summary>
		/// Logs the error to server.
		/// </summary>
		/// <param name="errorMessage">Error message.</param>
		public static void LogError(string errorMessage,string errorType="",string handleTip="Please contect DEV")
		{
			#if UNITY_EDITOR || DEBUG
			UnityEngine.Debug.LogError(errorMessage);
            Dictionary<string, object> errorContent = new Dictionary<string, object>();
            errorContent.Add ("errorMessage",errorMessage);
            errorContent.Add ("errortype",errorType);
            errorContent.Add("helpTip",handleTip);
            BaseGameConsole.singletonInstance.LogBaseEvent ("Error",errorContent);
			#endif
			
		}
		
		
		public static void LogProfile(string profileMessage)
		{
#if UNITY_EDITOR || DEBUG
			UnityEngine.Debug.Log(profileMessage);
			Dictionary<string, object> profileContent = new Dictionary<string, object>();
			profileContent.Add ("profileMsg",profileMessage);
			BaseGameConsole.singletonInstance.LogBaseEvent ("ProfileLog",profileContent);
#endif
			
		}
		/// <summary>
		/// Logs the plist error to server.
		/// </summary>
		/// <param name="errorMessage">Error message.</param>
		public static void LogRewardError(string errorMessage,string strToken)
		{
			#if UNITY_EDITOR || DEBUG
            errorMessage += "Please contact PM to check the reward.";
			UnityEngine.Debug.LogError(errorMessage);
            Dictionary<string, object> errorContent = new Dictionary<string, object>();
            errorContent.Add ("token",strToken);
            errorContent.Add ("errorMessage",errorMessage);
            BaseGameConsole.singletonInstance.LogBaseEvent ("Reward_Error",errorContent);
			#endif
			
		}
        /// <summary>
        /// Sends the swap scene exception log.
        /// </summary>
        /// <param name="errorMessege">Error messege.</param>
        public static void LogSwapSceneException( string errorMessege)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("reason", "Plist Parse Error:" + errorMessege);
            BaseGameConsole.ActiveGameConsole().LogBaseEvent(Classic.Analytics.SCENE_EXCHANGE_EXCEPTION, dict);
        }

        public static void LogDataHandlerException(string errorMessege)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("reason", errorMessege);
            BaseGameConsole.ActiveGameConsole().LogBaseEvent(Classic.Analytics.DATA_HANDLER_EXCEPTION, dict);
        }
		/// <summary>
		/// Logs the message to server.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="msg">Message.</param>
		public static void LogTrace(string type,string msg)
		{
			#if UNITY_EDITOR || DEBUG

			UnityEngine.Debug.Log(msg);

			if (BaseGameConsole.singletonInstance!=null) {
				Dictionary<string, object> content = new Dictionary<string, object>();
				content.Add ("logType",type);
				content.Add ("msg",msg);
				BaseGameConsole.singletonInstance.LogBaseEvent ("LogTrace",content);
			}
			#endif

		}
        /// <summary>
        /// 深度拷贝Dictionary
        /// https://code.i-harness.com/zh-CN/q/22148
        /// </summary>
        /// <returns>Dictionary</returns>
        /// <param name="dict">要复制的Dictionary</param>
        /// <typeparam name="K">key值类型(必须实现ICloneable接口，Clone函数必须实现深度拷贝)</typeparam>
        /// <typeparam name="V">value值类型(必须实现ICloneable接口，Clone函数必须实现深度拷贝)</typeparam>
        public static Dictionary<K, V> CloneDictionary<K, V>(Dictionary<K, V> dict) where K : ICloneable where V : ICloneable
        {

            Dictionary<K, V> newDict = null;

            if (dict != null)
            {
                // If the key and value are value types, just use copy constructor.
                if (((typeof(K).IsValueType || typeof(K) == typeof(string)) &&
                     (typeof(V).IsValueType) || typeof(V) == typeof(string)))
                {
                    newDict = new Dictionary<K, V>(dict);
                }else {
                    // prepare to clone key or value or both
                    newDict = new Dictionary<K, V>();

                    foreach (KeyValuePair<K, V> kvp in dict)
                    {
                        K key;
                        if (typeof(K).IsValueType || typeof(K) == typeof(string))
                        {
                            key = kvp.Key;
                        } else{
                            key = (K)kvp.Key.Clone();
                        }
                        V value;
                        if (typeof(V).IsValueType || typeof(V) == typeof(string))
                        {
                            value = kvp.Value;
                        }else{
                            value = (V)kvp.Value.Clone();
                        }
                        newDict[key] = value;
                    }
                }
            }

            return newDict;
        }
        
        public static long ClampLong(long curValue ,long minValue ,long maxValue)
        {
	        if (curValue <= minValue) {
		        return minValue;
	        }
	        if (curValue >= maxValue) {
		        return maxValue;
	        }

	        return curValue;
        }

        public static void NativePlatformLog(string tag, string message)
		{
			Log.Trace(tag + ": " + message);
		}
		
		/// <summary>
			/// Animations to. 所有与数字相关地必须用此方法处理，否则过大地话，会有偏差
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="from">From.</param>
		/// <param name="TargetNum">Target number.</param>
		/// <param name="duration">Duration.</param>
		/// <param name="updateCB">Update C.</param>
		/// <param name="startCB">Start C.</param>
		/// <param name="finishCB">Finish C.</param>
		public static Tweener AnimationTo(long from,long TargetNum,float duration,Action<long> updateCB,Action startCB=null,Action finishCB=null){
			bool bIncrease = (from < TargetNum);
			return DOTween.To (()=>from,x=>from=x,TargetNum, duration)
			.OnStart(()=>{
				if (startCB!=null) startCB();
			})
			.OnUpdate(()=>{
				if (bIncrease){ if (updateCB!=null) updateCB(Math.Min(from,TargetNum));}
				else {if (updateCB!=null) updateCB(Math.Max(from,TargetNum));}
			})
			.OnComplete(()=>{
				if (updateCB!=null) updateCB(TargetNum);
				if(finishCB!=null) finishCB();
			}).SetUpdate(true);
		}
		public static Tweener AnimationTo(int from,int TargetNum,float duration,Action<int> updateCB,Action startCB=null,Action finishCB=null){
			bool bIncrease = (from < TargetNum);
			return DOTween.To (()=>from,x=>from=x,TargetNum, duration)
				.OnStart(()=>{
					if (startCB!=null) startCB();
				})
				.OnUpdate(()=>{
					if (bIncrease){ if (updateCB!=null) updateCB(Math.Min(from,TargetNum));}
					else {if (updateCB!=null) updateCB(Math.Max(from,TargetNum));}
				})
				.OnComplete(()=>{
					if (updateCB!=null) updateCB(TargetNum);
					if(finishCB!=null) finishCB();
				}).SetUpdate(true);
		}
		public static Tweener AnimationTo(double from,double TargetNum,float duration,Action<double> updateCB,Action startCB=null,Action finishCB=null){
			bool bIncrease = (from < TargetNum);
			return DOTween.To (()=>from,x=>from=x,TargetNum, duration)
			.OnStart(()=>{
			if (startCB!=null) startCB();
			})
			.OnUpdate(()=>{
			if (bIncrease){ if (updateCB!=null) updateCB(Math.Min(from,TargetNum));}
			else {if (updateCB!=null) updateCB(Math.Max(from,TargetNum));}
			})
			.OnComplete(()=>{
			if (updateCB!=null) updateCB(TargetNum);
			if(finishCB!=null) finishCB();
			}).SetUpdate(true);
		}
		
		public static Tweener AnimationToEase(double from, double TargetNum, float duration, Action<double> updateCB, Action startCB = null,
			Action finishCB = null, Ease easeType = Ease.Linear) {
			bool bIncrease = (from < TargetNum);
			return DOTween.To(() => from, x => from = x, TargetNum, duration)
				.OnStart(() =>
				{
					startCB?.Invoke();
				})
				.OnUpdate(() => {
					if (bIncrease)
					{
						updateCB?.Invoke(Math.Min(from, TargetNum));
					}
					else
					{
						updateCB?.Invoke(Math.Max(from, TargetNum));
					}
				})
				.OnComplete(() => {
					updateCB?.Invoke(TargetNum);
					finishCB?.Invoke();
				}).SetEase(easeType).SetUpdate(true);
		}


#region 用于升级字段范围使用，同时保证向前兼容
		public static long GetValueLongFromInt(string key,int defaultValue=0){
			return SharedPlayerPrefs.LoadPlayerPrefsLong(key, (long)SharedPlayerPrefs.GetPlayerPrefsIntValue (key, defaultValue));
		}
		public static void SetValueLongFromInt(string key,long value){
			SharedPlayerPrefs.SavePlayerPrefsLong (key,value);
		}

		public static double GetValueDoubleFromFloat(string key,float defaultValue = 0){
			return System.Convert.ToDouble (SharedPlayerPrefs.GetPlayerPrefsStringValue(key,SharedPlayerPrefs.GetPlayerPrefsFloatValue(key,defaultValue).ToString()));
		}
		public static void SetValueDoubleFromFloat(string key,double value){
			SharedPlayerPrefs.SetPlayerPrefsStringValue (key,value.ToString());
		}

		private static Random random = new Random ();
		public static long GenLongRandomNumber(long min,long max){
			long baseValue = System.Math.Min (min, max);
			long maxValue = System.Math.Max (min,max);
			return baseValue + (long)((maxValue - baseValue) * random.NextDouble ());
		}
#endregion
	
		public static string GetNumberFormat(object o,bool needMultiple = true,string preFix= "$"){
			double oldNumber = CastValueDouble (o);
			if (needMultiple && (!Classic.UserManager.GetInstance().UserProfile().isOlderUser)) {
				oldNumber *= Core.ApplicationConfig.GetInstance ().ShowCoinsMultiplier;
			}

			long number = CastValueLong (oldNumber);
			double temp = 0;
			string tempText = "";

			if (number>=1000000000) {
				temp = number / 1000000000.0;
				tempText = preFix+temp.ToString(preFix+"G3") + "B";//保留3位有效数字
			}
			else if (number >= 1000000)
			{
				temp = number / 1000000.0;
				tempText = preFix+temp.ToString("G3") + "M";
			}
			else if(number>=1000)
			{
				temp = number/1000.0;
				tempText = preFix+temp.ToString("G3") + "K";
			} 
			else {
				temp = number;
				tempText = preFix+temp.ToString("G3");
			}

			return tempText;
		}

		public static SerializationTwoDimensionList TwoDimenListToSerializa(List<List<int>> list)
        {
            List<SerializationIntList> tmp = new List<SerializationIntList>();
            list.ForEach((List<int> obj) => tmp.Add(new SerializationIntList(obj)));

            SerializationTwoDimensionList serializationTwoDimensionList = new SerializationTwoDimensionList(tmp);
            return serializationTwoDimensionList;
        }

        public static List<List<int>> SerializaToTwoDimenList(SerializationTwoDimensionList s)
        {
            List<SerializationIntList> tmp = s.ToList();
            List<List<int>> result = new List<List<int>>();
            tmp.ForEach((obj) => result.Add(obj.ToList()));
            return result;
        }

        //list数组随机排序
        public static List<T> RandomSortList<T>(List<T> ListT)
        {
            Random ran = new Random();
            List<T> newList = new List<T>();
            foreach (T item in ListT)
            {
                newList.Insert(ran.Next(newList.Count + 1), item);
            }
            return newList;
        }
        //obj to Dictionary<int,int>
        public static Dictionary<int,int> CastObjToIntDict(object o)
        {
            Dictionary<int, int> ret = new Dictionary<int, int>();
            Dictionary<string, object> dict = o as Dictionary<string, object>;
            foreach(KeyValuePair<string,object> keyValue in dict)
            {
                int key = CastValueInt(keyValue.Key);
                int v = CastValueInt(keyValue.Value);
                ret[key] = v;
            }
            return ret;
        }

        public static List<int> CastObjToIntList(object o)
        {
            List<int> ret = new List<int>();
            List<object> list = o as List<object>;
            for (int i = 0; i < list.Count; i++)
            {
                ret.Add(CastValueInt(list[i]));
            }
            return ret;
        }
        
        public static List<float> CastObjToFloatList(object o)
        {
	        List<float> ret = new List<float>();
	        List<object> list = o as List<object>;
	        for (int i = 0; i < list.Count; i++)
	        {
		        ret.Add(CastValueFloat(list[i]));
	        }
	        return ret;
        }

        public static decimal ConvertFloat2Decimal( float value,int saveDotNum)
        {
	        string format = "F" + saveDotNum;
	        string val = value.ToString(format);
	        return  Convert.ToDecimal(val) ;//4舍5入
        }

        #region 从列表中查找最接近的目标值

        public static long GetNearestTgtValue(long tgtValue,List<long> list,NearestType type = NearestType.None)
        {
        	long selectValue = 0;
        	int count = list.Count;
            
            if (list == null || list.Count == 0)
            {
                throw new Exception("GetNearestTgtValue failed! list not invalid!"+tgtValue);
            }
        	if (tgtValue.CompareTo(list[0]) <= 0)
        	{
        		return list[0];
        	}

        	if (tgtValue.CompareTo(list[count-1])>=0)
        	{
        		return list[count - 1];
        	}
        	for (int i = 1; i < list.Count; i++)
        	{
        		if (tgtValue.CompareTo(list[i - 1]) > 0 && tgtValue.CompareTo(list[i])<=0)
        		{
                    switch (type)
                    {
                        case NearestType.None:
                            selectValue = (tgtValue - list[i - 1]) < (list[i] - tgtValue)
                                ? list[i - 1]
                                : list[i];
                            break;
                        case NearestType.GreaterTgtValue:
                            selectValue = list[i];
                            break;
                        case NearestType.LessTgtValue:
                            selectValue = list[i-1];
                            break;
                    }
        			//只有当前值距离前一个值距离小于距离后一个值时，使用前一个值，否则一概使用相对较大的值
                 
        			break;
        		}
        	}
        	return selectValue;
        }
        
        
        public static int GetNearestTgtValueIndex(long tgtValue,List<long> list,NearestType type)
        {
            int selectedIdx = 0;
            int count = list.Count;
            
            if (list == null || list.Count == 0)
            {
                throw new Exception("GetNearestTgtValue failed! list not invalid!"+tgtValue);
            }
            if (tgtValue.CompareTo(list[0]) <= 0)
            {
                return 0;
            }

            if (tgtValue.CompareTo(list[count-1])>=0)
            {
                return count-1;
            }
            for (int i = 1; i < list.Count; i++)
            {
                if (tgtValue.CompareTo(list[i - 1]) > 0 && tgtValue.CompareTo(list[i])<=0)
                {
                    //只有当前值距离前一个值距离小于距离后一个值时，使用前一个值，否则一概使用相对较大的值
                    switch (type)
                    {
                        case NearestType.None:
                            selectedIdx = (tgtValue - list[i - 1]) < (list[i] - tgtValue) ? i-1 : i;
                            break;
                        case NearestType.GreaterTgtValue:
                            selectedIdx = i;
                            break;
                        case NearestType.LessTgtValue:
                            selectedIdx = i-1;
                            break;
                    }
                    break;
                }
            }
            return selectedIdx;
        }
   
        public static int GetNearestTgtLongValueIndex(long tgtValue, List<long> list,NearestType type)
        {
	        //临时从GetNearestTgtValueIndex拷贝，int改为long
	        //当bet统一改为long上线时，此处需要修改
	        int selectedIdx = 0;
	        int count = list.Count;
            
	        if (list == null || list.Count == 0)
	        {
		        throw new Exception("GetNearestTgtValue failed! list not invalid!"+tgtValue);
	        }
	        if (tgtValue.CompareTo(list[0]) <= 0)
	        {
		        return 0;
	        }

	        if (tgtValue.CompareTo(list[count-1])>=0)
	        {
		        return count-1;
	        }
	        for (int i = 1; i < list.Count; i++)
	        {
		        if (tgtValue.CompareTo(list[i - 1]) > 0 && tgtValue.CompareTo(list[i])<=0)
		        {
			        //只有当前值距离前一个值距离小于距离后一个值时，使用前一个值，否则一概使用相对较大的值
			        switch (type)
			        {
				        case NearestType.None:
					        selectedIdx = (tgtValue - list[i - 1]) < (list[i] - tgtValue) ? i-1 : i;
					        break;
				        case NearestType.GreaterTgtValue:
					        selectedIdx = i;
					        break;
				        case NearestType.LessTgtValue:
					        selectedIdx = i-1;
					        break;
			        }
			        break;
		        }
	        }
	        return selectedIdx;
        }  

        
        #endregion

        #region Unity

        /// <summary>
        /// 一定可以获取到组件
        /// </summary>
        /// <param name="go">当前游戏物体</param>
        /// <typeparam name="T">组件</typeparam>
        /// <returns></returns>
        public static T RealGetComponent<T>(this GameObject go) where T : Component
        {
	        if (go == null) return default(T);
	        T temp = go.GetComponent<T>();
	        if (temp == null)
	        {
		        temp = go.AddComponent<T>();
	        }

	        return temp;
        }

        /// <summary>
        /// 获取自物体组件
        /// </summary>
        /// <param name="trans">当前transform</param>
        /// <param name="path">路径</param>
        /// <typeparam name="T">组件</typeparam>
        /// <returns></returns>
        public static T RealFindObj<T>(this Transform trans,string path) where T : Component
        {
	        if (trans == null) return default(T);
	        GameObject go = Util.FindObject<GameObject>(trans,path);
	        if (go == null) return default(T);
	        T temp = go.RealGetComponent<T>();
	        return temp;
        }
        
        /// <summary>
        /// 一定可以获取到组件
        /// </summary>
        /// <param name="trans">当前transform</param>
        /// <typeparam name="T">组件</typeparam>
        /// <returns></returns>
        public static T RealGetComponent<T>(this Transform trans) where T : Component
        {
	        if (trans == null) return default(T);
	        GameObject go = trans.gameObject;
	        T temp = go.GetComponent<T>();
	        if (temp == null)
	        {
		        temp = go.AddComponent<T>();
	        }

	        return temp;
        }

        #endregion
        
        
        /// <summary>
        /// 加载CSV
        /// </summary>
        /// <param name="path">
        /// 	Config/ItemTemplate.csv
        /// </param>
        /// <returns></returns>
        public static string LoadCSVFromPath(string path)
        {
	        try
	        {
#if UNITY_EDITOR && !BUNDLE_SIMULATION
		        //指定目录加载
		        var rootPath = Path.Combine(Application.dataPath,"LuaAssets");
#else
            var rootPath = Application.persistentDataPath;
#endif
		        List<List<string>> allData = new List<List<string>>();
                
		        using (CsvReader reader = new CsvReader(Path.Combine(rootPath, path)))
		        {
			        allData.AddRange(from string[] values in reader.RowEnumerator select new List<string>(values));
		        }

		        return MiniJSON.Json.Serialize(allData);
	        }
	        catch (Exception e)
	        {
		        return null;
	        }
        }
	}
    
    public enum NearestType
    {
	    None,
	    GreaterTgtValue,
	    LessTgtValue,
    }
}

