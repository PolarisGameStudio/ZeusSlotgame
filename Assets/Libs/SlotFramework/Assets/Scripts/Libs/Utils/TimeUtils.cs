using UnityEngine;
using System.Collections;
using System;
using Beebyte.Obfuscator;

namespace Libs
{
	[Skip]
	public class TimeUtils : MonoBehaviour {

		public static long ConvertDateTimeLong (System.DateTime time)
		{  
			return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
		}  

		public static long ConvertDateMillTimeLong (System.DateTime time)
		{  
			return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000;
		}  
		public static long ConvertDateTimeMillisecondsToLong (System.DateTime time)
		{  
			return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000;
		}
		/// <summary>
		/// 以秒为单位返回时间戳
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public static long GetTimestampInSeconds(System.DateTime time)
		{
			return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
		}
		/// <summary>
		/// 以毫秒为单位返回时间戳
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public static long GetTimestampInMilliseconds(System.DateTime time)
		{
			return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000;
		}
		public static DateTime ConvertLongToDate (long time)
		{
			// System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
			// return startTime.AddTicks(time * 10000000);
			return new DateTime(1970, 1, 1).AddTicks(time * 10000000).ToLocalTime();
		}

		public static long SecondsBetween(DateTime start,DateTime end)
		{
			long result = (long)(end - start).TotalSeconds;
			return result > 0? result:0;
		}
		
		public static double SecondsDoubleBetween(DateTime start,DateTime end)
		{
			double result = (end - start).TotalSeconds;
			return result > 0? result:0;
		}

		public static bool IsSameDay(System.DateTime firstTime, DateTime secondTime ){
			return firstTime.Date == secondTime.Date;
		}

		public static bool isToday (System.DateTime time)
		{
			return IsSameDay (time, DateTime.Now);
		}

		public static bool isTodayWithPlayerPrefs(string key)
		{
			return isToday (GetDateTimeFromPlayerPrefs (key));
		}
		/// <summary>
		/// Determines if is in days the specified beginTime endTime.
		/// </summary>
		/// <returns><c>true</c> if is in days the specified beginTime endTime; otherwise, <c>false</c>.</returns>
		/// <param name="beginTime">Begin time.</param>
		/// <param name="endTime">End time.</param>
		public static bool IsInDays(System.DateTime beginTime,DateTime endTime)
		{
			return DateTime.Now >= beginTime && DateTime.Now <= endTime;
		}
		
		public static string Cast_H_OR_M (long leftTime)
		{ 
			long days = leftTime / CSharpUtil.DAY;
			if (days >= 1) 
			{
				return string.Format (GameConstants.Time_N_D_B_Key,days);
			}

			long leftHour = leftTime / CSharpUtil.HOUR;
			leftTime -= leftHour * CSharpUtil.HOUR;
			long leftMinute = leftTime / CSharpUtil.MINUTE;
			leftTime -= leftMinute * CSharpUtil.MINUTE;
			long leftSecond = leftTime;
			if (leftHour > 0) {
				return (leftHour + " h");
			} else if (leftMinute > 0) {
				return (leftMinute + " m");
			} else {
				return (leftSecond + " s");
			}
		}
		#region playerpref
		public static DateTime GetDateTimeFromPlayerPrefs(string key)
		{
			long temp = LoadPlayerPrefsLong (key);
			return ConvertLongToDate (temp);
		}

		public static void SaveDataTimeToPlayerPrefs(string key, DateTime time)
		{
			long temp = ConvertDateTimeLong (time);
			SavePlayerPrefsLong (key,temp);
		}

		public static long LoadPlayerPrefsLong (string key, long defaultValue = 0)
		{
			string strValue = PlayerPrefs.GetString (key, null);
			if (strValue == null) {
				return defaultValue;
			}
			long result = defaultValue;
			if (long.TryParse (strValue, out result)) {
				return result;
			} else {
				return defaultValue;
			}
		}
		public static int CheckSameDayAndGetInterval(long activetime)
		{
			// 如果 activetime 为0，表示没有激活时间，返回特定值或抛出异常
			if (activetime == 0)
			{
				return -1; // 或者根据业务需求返回其他值
			}

			// 将 long 类型的时间戳转换为 DateTime
			DateTime activeDate = ConvertLongToDate(activetime);
			DateTime currentDate = DateTime.Now;
    
			// 判断是否为同一天
			if (activeDate.Date == currentDate.Date)
			{
				return 0; // 同一天
			}
			else
			{
				// 计算间隔的天数（取绝对值，确保正数）
				TimeSpan interval = currentDate.Date - activeDate.Date;
				return Math.Abs(interval.Days);
			}
		}
		public static void SavePlayerPrefsLong (string key, long value)
		{
			PlayerPrefs.SetString (key, value.ToString());
			PlayerPrefs.Save ();
		}

		public static float LoadPlayerPrefsFloat (string key, float defaultValue = 0)
		{
			string strValue = PlayerPrefs.GetString (key, null);
			if (strValue == null) {
				return defaultValue;
			}
			float result = defaultValue;
			if (float.TryParse (strValue, out result)) {
				return result;
			} else {
				return defaultValue;
			}
		}

		public static void SavePlayerPrefsFloat (string key, float value)
		{
			PlayerPrefs.SetString (key, value.ToString());
			PlayerPrefs.Save ();
		}

		public static DateTime ToDateTimeFromMillTimeStamp (long timeStamp)
		{
			return ToDateTimeFromTimeStamp(timeStamp / 1000);
		}

		public static DateTime ToDateTimeFromTimeStamp (long timeStamp)
		{
			// Unix timestamp is seconds past epoch
			if(timeStamp < 0) timeStamp = 0;
			System.DateTime dtDateTime = new DateTime (1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds (timeStamp).ToLocalTime ();
			return dtDateTime;
		}

		/// <summary>
		/// Gets the left time string.
		/// </summary>
		/// <returns>The left time string.</returns>
		/// <param name="leftTime">Left time.  miliSec</param>
		public static string GetLeftDay_OR_HMS(long leftTime)
		{ 
			return GetLeftSecondString(leftTime / 1000);
		}
		
		public static string GetLeftHM_OR_MS(long leftTime)
		{
			leftTime = leftTime / 1000;
			long leftHour = leftTime / CSharpUtil.HOUR;
			leftTime -= leftHour * CSharpUtil.HOUR;
			long leftMinute = leftTime / CSharpUtil.MINUTE;
			leftTime -= leftMinute * CSharpUtil.MINUTE;
			long leftSecond = leftTime;
			if (leftHour > 0)
				return string.Format (GameConstants.Time_HM_Key, leftHour, leftMinute);
			else
				return string.Format (GameConstants.Time_MS_Key2, leftMinute, leftSecond);
		}

		public static string GetLeftSecondString(long leftTime)
		{ 
			long days = leftTime / CSharpUtil.DAY;
			if (days > 1) {
				return string.Format (GameConstants.Time_N_Day_Key,days);
			} else if (days == 1) {
				return GameConstants.Time_OneDay_Key;
			}

			long leftHour = leftTime / CSharpUtil.HOUR;
			leftTime -= leftHour * CSharpUtil.HOUR;
			long leftMinute = leftTime / CSharpUtil.MINUTE;
			leftTime -= leftMinute * CSharpUtil.MINUTE;
			long leftSecond = leftTime;
			if(leftHour > 0)
				return string.Format (GameConstants.Time_HMS_Key, leftHour, leftMinute, leftSecond);
			else
				return string.Format (GameConstants.Time_MS_Key, leftMinute, leftSecond);
		}

		public static string Get_H_OR_M_S_ForShop(long leftTime)
		{ 
			long days = leftTime / CSharpUtil.DAY;
			if (days > 1) {
				return string.Format ("{0} DAYS",days);
			} else if (days == 1) {
				return "1 DAY";
			}

			long leftHour = leftTime / CSharpUtil.HOUR;
			leftTime -= leftHour * CSharpUtil.HOUR;
			long leftMinute = leftTime / CSharpUtil.MINUTE;
			leftTime -= leftMinute * CSharpUtil.MINUTE;
			long leftSecond = leftTime;
			if (leftHour > 0) {
				if (leftHour == 1) {
					return (leftHour + " HRS");
				} else {
					return (leftHour + " HRS");
				}
			} else if (leftMinute > 0) {
				if (leftMinute == 1) {
					return (leftMinute + " MIN");
				} else {
					return (leftMinute + " MIN");
				}
			} else {
				if (leftSecond == 1) {
					return (leftSecond + " SECOND");
				} else {
					return (leftSecond + " SECONDS");
				}
			}
		}
		
		public static string Get_H_OR_M_S(long leftTime)
		{ 
			long days = leftTime / CSharpUtil.DAY;
			if (days > 1) {
				return string.Format (GameConstants.Time_N_Day_B_Key,days);
			} else if (days == 1) {
				return GameConstants.Time_OneDay_B_Key;
			}

			long leftHour = leftTime / CSharpUtil.HOUR;
			leftTime -= leftHour * CSharpUtil.HOUR;
			long leftMinute = leftTime / CSharpUtil.MINUTE;
			leftTime -= leftMinute * CSharpUtil.MINUTE;
			long leftSecond = leftTime;
			if (leftHour > 0) {
				if (leftHour == 1) {
					return (leftHour + " HOUR");
				} else {
					return (leftHour + " HOURS");
				}
			} else if (leftMinute > 0) {
				if (leftMinute == 1) {
					return (leftMinute + " MINUTE");
				} else {
					return (leftMinute + " MINUTES");
				}
			} else {
				if (leftSecond == 1) {
					return (leftSecond + " SECOND");
				} else {
					return (leftSecond + " SECONDS");
				}
			}
		}
		public static string Get_H_M_S(long leftTime)
		{ 
			long days = leftTime / CSharpUtil.DAY;
			if (days > 1) {
				return string.Format ("{0} d",days);
			} else if (days == 1) {
				return "1 d";
			}

			long leftHour = leftTime / CSharpUtil.HOUR;
			leftTime -= leftHour * CSharpUtil.HOUR;
			long leftMinute = leftTime / CSharpUtil.MINUTE;
			leftTime -= leftMinute * CSharpUtil.MINUTE;
			long leftSecond = leftTime;
			if (leftHour > 0) {
				if (leftHour == 1) {
					return (leftHour + " h");
				} else {
					return (leftHour + " h");
				}
			} else if (leftMinute > 0) {
				if (leftMinute == 1) {
					return (leftMinute + " min");
				} else {
					return (leftMinute + " min");
				}
			} else {
				if (leftSecond == 1) {
					return (leftSecond + " s");
				} else {
					return (leftSecond + " s");
				}
			}
		}

		public static string GetLeftTime_Day_And_HMS(DateTime endDateTime)
		{
			return GetLeftTime_Day_And_HMS(endDateTime - DateTime.Now);
		}

		public static string GetLeftTime_Day_And_HMS(TimeSpan timeSpan)
		{
			if (timeSpan.Days >= 1) 
                return string.Format (GameConstants.Time_Day_HMS_Key,timeSpan.Days,timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            else 
                return string.Format (GameConstants.Time_HMS_Key,timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}

		public static string GetLeftTime_Day_OR_HMS(DateTime endDateTime)
		{
			return GetLeftDay_OR_HMS(endDateTime - DateTime.Now);
		}
		/// <summary>
		/// format ; 1DAY or 3DAYS
		/// </summary>
		/// <param name="endDateTime"></param>
		/// <returns></returns>
		public static string GetLeftTime_Day_OR_HMS_SpecialFormat_MAX(DateTime endDateTime)
		{
			return GetLeftDay_OR_HMS_Special_MAX(endDateTime - DateTime.Now);
		}
		
		public static string GetLeftDay_OR_HMS(TimeSpan timeSpan)
		{			
			if (timeSpan.Days > 1) return string.Format (GameConstants.Time_N_Day_Key,timeSpan.Days);
			else if (timeSpan.Days == 1) return GameConstants.Time_OneDay_Key;

			return string.Format (GameConstants.Time_HMS_Key, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}
		public static string GetLeftDay_OR_HMS_Special_MAX(TimeSpan timeSpan)
		{
			if (timeSpan.Days > 1) return string.Format (GameConstants.Time_N_Day_B_Key,timeSpan.Days);
			else if (timeSpan.Days == 1) return GameConstants.Time_OneDay_B_Key;

			return string.Format (GameConstants.Time_HMS_Key, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}
		public static string GetLeftDay_OR_HMS_FromMilliseconds(long milliseconds)
		{			
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
			if (timeSpan.Days > 1) return string.Format (GameConstants.Time_N_Day_B_Key,timeSpan.Days);
			else if (timeSpan.Days == 1) return GameConstants.Time_OneDay_B_Key;

			return string.Format (GameConstants.Time_HMS_Key, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}
		public static string GetLeft_HMS_FromMilliseconds(long milliseconds)
		{			
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
			
			return string.Format (GameConstants.Time_HMS_Key, (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
		}
		
		
		/// <summary>
		/// Dayses the between two dates.
		/// 具体的事件间隔导出的天数
		/// </summary>
		/// <returns>The between two dates.</returns>
		/// <param name="oldDate">Old date.</param>
		/// <param name="newDate">New date.</param>
		public static int DaysBetweenTwoDates(DateTime oldDate, DateTime newDate)
		{ 
			// Difference in days, hours, and minutes.
			TimeSpan ts = newDate - oldDate; 
			return ts.Days;
		}

		/// <summary>
		/// Dayses the between two date time.
		/// 计算是否为连续两天时使用,DaysBetweenTwoDates相比针对的是12点过后就区分一天。
		/// </summary>
		/// <returns>The between two date time.</returns>
		/// <param name="start">Start.</param>
		/// <param name="now">Now.</param>
		public static int DaysBetweenTwoDateTime(DateTime start, DateTime now){
			System.DateTime orignTime = GetDefaultTime();
			long startTotalDays = (long)(start - orignTime).TotalDays;
			long nowTotalDays = (long)(now - orignTime).TotalDays;
			return (int)(nowTotalDays - startTotalDays);
		}

		public static DateTime GetDefaultTime(){
			return TimeZone.CurrentTimeZone.ToLocalTime (new System.DateTime (1970, 1, 1));
		}
		//Mac chinese OS show error
		public static string GetLocalTime(bool accurateToMS= false){
			if (accurateToMS) {
				return System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
			}
			return System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
		}
		#endregion
		
		public static string GetLeftSecondStringMax(long leftTime)
		{ 
			long days = leftTime / CSharpUtil.DAY;
			if (days > 1) {
				return string.Format (GameConstants.Time_N_Day_B_Key,days);
			} else if (days == 1) {
				return GameConstants.Time_OneDay_B_Key;
			}
			long leftHour = leftTime / CSharpUtil.HOUR;
			leftTime -= leftHour * CSharpUtil.HOUR;
			long leftMinute = leftTime / CSharpUtil.MINUTE;
			leftTime -= leftMinute * CSharpUtil.MINUTE;
			long leftSecond = leftTime;
			if(leftHour > 0)
				return string.Format (GameConstants.Time_HMS_Key, leftHour, leftMinute, leftSecond);
			else
				return string.Format (GameConstants.Time_MS_Key, leftMinute, leftSecond);
		}

		#region 去掉pause的真实游戏时间
		
		public static double PauseDurTime=0;
		public static DateTime LastPauseDataTime = DateTime.Now; //上次pause的时候具体时间
		public static double GetStartUpRunTime()
		{
			return Time.unscaledTime - PauseDurTime;
		}

		public static void SetPauseDataTime()
		{
			LastPauseDataTime = DateTime.Now;
		}

		public static void StatisticsPauseDurTime()
		{
			PauseDurTime = TimeUtils.SecondsDoubleBetween(LastPauseDataTime, DateTime.Now);
		}

		#endregion

		
		
	}
	
	
}
