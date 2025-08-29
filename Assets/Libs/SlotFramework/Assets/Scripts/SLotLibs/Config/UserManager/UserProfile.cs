using System;
using UnityEngine;
using Classic;
using Core;
using System.Collections.Generic;
using App.SubSystems;
using Utils;

namespace Core
{
    [Serializable]
    public class UserProfile
    {
        public UserProfile()
        {
        }

        public virtual int Level()
        {
            return 5000;
            // return level;
        }

        public virtual long LastLaunchTimeLong()
        {
            return lastLaunchTimeLong;
        }

        public void SetLastLaunchTimeLong(long time)
        {
            if (time < 0)
            {
                throw new System.ArgumentOutOfRangeException("newLastLaunchTimeLong");
            }

            lastLaunchTimeLong = time;
            SaveToPlayerPrefs();
        }

        public virtual long Balance()
        {
            return balance;
        }

        public void SetBalance(long newBalance)
        {
            if (newBalance < 0)
            {
                throw new System.ArgumentOutOfRangeException("newBalance");
            }

            balance = newBalance;
            SharedPlayerPrefs.SavePlayerPrefsLong(BALANCE, balance);
        }

        public virtual void IncreaseBalance(long prize)
        {
            SetBalance(balance + prize);
        }

        public virtual void IncreaseBalanceWithoutFreshUI(long prize)
        {
            SetBalance(balance + prize);
        }

        public virtual void Init()
        {
            LoadFromPlayerPrefs();
        }

        public virtual bool IsPlayFirstTime()
        {
            return PlayGameTotalTimes() <= 1;
        }

        public void SetPlayTotalTimes(int totalTimes)
        {
            PlayerPrefs.SetInt(PLAY_GAME_TOTAL_TIMES, totalTimes);
            PlayerPrefs.Save();
        }

        public virtual void IncreasePlayGameTotalTimes()
        {
            SetPlayTotalTimes(PlayGameTotalTimes() + 1);
        }

        public virtual int PlayGameTotalTimes()
        {
            return PlayerPrefs.GetInt(PLAY_GAME_TOTAL_TIMES, 0);
        }

        public virtual bool HasGrantInitialBonus()
        {
            return SharedPlayerPrefs.GetPlayerBoolValue(HAS_GRANT_INITIAL_BONUS, false);
        }


        public virtual void SetHasGrantInitialBonus(bool hasGrant)
        {
            SharedPlayerPrefs.SetPlayerPrefsBoolValue(HAS_GRANT_INITIAL_BONUS, hasGrant);
            PlayerPrefs.Save();
        }


        public virtual bool HasRateUs()
        {
            return SharedPlayerPrefs.GetPlayerBoolValue(HAS_RATE_US, false);
        }


        public virtual void SetHasRateUs(bool hasRateUs)
        {
            SharedPlayerPrefs.SetPlayerPrefsBoolValue(HAS_RATE_US, hasRateUs);
            PlayerPrefs.Save();
        }

        //获取打开界面的冷却时间，在冷却时间内，不可以打开界面
        public virtual DateTime GetWaitOpenRateAlertTime()
        {
            return Libs.TimeUtils.GetDateTimeFromPlayerPrefs(WAIT_OPEN_RATE_ALERT_TIME);
        }

        public virtual void SetWaitOpenRateAlertTime(DateTime value)
        {
            Libs.TimeUtils.SaveDataTimeToPlayerPrefs(WAIT_OPEN_RATE_ALERT_TIME, value);
        }

        public virtual int BetLinesInSlotMachine(SlotMachineConfig slotMachine, int defaultValue = 0)
        {
            CheckSlotMachineName(slotMachine);

            string key = KeyForSlotMachine(slotMachine, BET_LINES);
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetBetLinesInSlotMachine(SlotMachineConfig slotMachine, int betLines)
        {
            CheckSlotMachineName(slotMachine);

            string key = KeyForSlotMachine(slotMachine, BET_LINES);
            PlayerPrefs.SetInt(key, betLines);
        }

        public virtual long GetTotalSpins()
        {
            return SharedPlayerPrefs.LoadPlayerPrefsLong(TOTAL_SPINS, 0);
        }

        public virtual int ManualSpinCounter()
        {
            return PlayerPrefs.GetInt(MANUAL_SPIN_COUNTER, 0);
        }

        private int CurrentTotalHourlyBonusCollectTimes()
        {
            return PlayerPrefs.GetInt(PLAYER_COLLECT_HOURLY_BONUS_TIMES);
        }


        public void IncreaseSpinCounter()
        {
            totalSpins++;
        }

        public long GetTotalSpinCounter()
        {
            return totalSpins;
        }

        public virtual int IncreaseManualSpinCounter()
        {
            int oldSpinCounter = ManualSpinCounter();
            int newSpinCounter = oldSpinCounter + 1;
            PlayerPrefs.SetInt(MANUAL_SPIN_COUNTER, newSpinCounter);
            return newSpinCounter;
        }

        public void SetManualSpinCounter(int mSpinCounter)
        {
            PlayerPrefs.SetInt(MANUAL_SPIN_COUNTER, 0);
        }


        public void SaveToPlayerPrefs()
        {
            PlayerPrefs.SetInt(LEVEL, level);
            SharedPlayerPrefs.SavePlayerPrefsLong(BALANCE, balance);
            SharedPlayerPrefs.SavePlayerPrefsLong(TOTAL_SPINS, totalSpins);
            SharedPlayerPrefs.SavePlayerPrefsLong(USER_LAST_TIME_LANUCH_TIME, lastLaunchTimeLong);
        }

        public void SaveCoinsMultiplier(int coinsMultiplier)
        {
            SharedPlayerPrefs.SetPlayerPrefsIntValue(SHOWCOINSMULTIPLIER, coinsMultiplier);
        }

        public int LoadCoinsMultiplier()
        {
            return SharedPlayerPrefs.GetPlayerPrefsIntValue(SHOWCOINSMULTIPLIER, 0);
        }

        private void LoadFromPlayerPrefs()
        {
            level = PlayerPrefs.GetInt(LEVEL,1);
            lastLaunchTimeLong = SharedPlayerPrefs.LoadPlayerPrefsLong(USER_LAST_TIME_LANUCH_TIME, -1);
            SharedPlayerPrefs.SavePlayerPrefsLong(USER_LAST_TIME_LANUCH_TIME,
                CSharpUtil.ConvertDateTimeLongInSecond(DateTime.Now));
            balance = SharedPlayerPrefs.LoadPlayerPrefsLong(BALANCE,
                (long)SharedPlayerPrefs.GetPlayerPrefsFloatValue(BALANCE, 0));
            totalSpins = SharedPlayerPrefs.LoadPlayerPrefsLong(TOTAL_SPINS, 0);
        }

        private string KeyForSlotMachine(SlotMachineConfig slotMachine, string key)
        {
            return slotMachine.Name() + "." + key;
        }

        private void CheckSlotMachineName(SlotMachineConfig slotMachine)
        {
            string machineName = slotMachine.Name();
            System.Diagnostics.Debug.Assert(machineName != null);
        }

        public virtual bool HasChangeBet()
        {
            return SharedPlayerPrefs.GetPlayerBoolValue(HAS_CHANGE_BET, false);
        }

        public virtual void SetChangeBet()
        {
            SharedPlayerPrefs.SetPlayerPrefsBoolValue(HAS_CHANGE_BET, true);
        }

        #region VIP

        public virtual bool HasRateMachine(string machineName)
        {
            return SharedPlayerPrefs.GetPlayerBoolValue(Has_Rate_Machine + machineName, false);
        }

        public virtual void RateMachine(string machineName)
        {
            SharedPlayerPrefs.SetPlayerPrefsBoolValue(Has_Rate_Machine + machineName, true);
            SharedPlayerPrefs.SavePlayerPreference();
        }

        public bool HasSendAllSpinTimes
        {
            set
            {
                SharedPlayerPrefs.SetPlayerPrefsBoolValue(HAS_SEND_ALL_SPIN_TIME, true);
                SharedPlayerPrefs.SavePlayerPreference();
            }
            get { return SharedPlayerPrefs.GetPlayerBoolValue(HAS_SEND_ALL_SPIN_TIME, false); }
        }

        public int AllSpinTimes
        {
            set
            {
                SharedPlayerPrefs.SetPlayerPrefsIntValue(ALL_SPIN_TIMES, value);
                SharedPlayerPrefs.SavePlayerPreference();
            }
            get { return SharedPlayerPrefs.GetPlayerPrefsIntValue(ALL_SPIN_TIMES, 0); }
        }

        #endregion


        public bool IsFirstGameSession
        {
            set
            {
                SharedPlayerPrefs.SetPlayerPrefsBoolValue(FIRST_GAME_SESSION, false);
                SharedPlayerPrefs.SavePlayerPreference();
            }
            get { return SharedPlayerPrefs.GetPlayerBoolValue(FIRST_GAME_SESSION, true); }
        }

        public long GameTimeWhenLastClose
        {
            set { SharedPlayerPrefs.SavePlayerPrefsLong(GAME_SESSION_TIME, value); }
            get { return SharedPlayerPrefs.LoadPlayerPrefsLong(GAME_SESSION_TIME); }
        }

        //上次关闭应用的时间 
        public DateTime LastExitGameTime
        {
            set { Libs.TimeUtils.SaveDataTimeToPlayerPrefs(LAST_EXIT_GAME_DATETIME, value); }
            get { return Libs.TimeUtils.GetDateTimeFromPlayerPrefs(LAST_EXIT_GAME_DATETIME); }
        }

        public int WelcomeBackReceiveCount
        {
            set { SharedPlayerPrefs.SetPlayerPrefsIntValue(WELCOME_BACK_RECEIVE_COUNT, value); }
            get { return SharedPlayerPrefs.GetPlayerPrefsIntValue(WELCOME_BACK_RECEIVE_COUNT, 0); }
        }

        public string CurrentLevelPlan
        {
            set { SharedPlayerPrefs.SetPlayerPrefsStringValue(USER_CURRENT_LEVEL_PLAN, value); }
            get { return SharedPlayerPrefs.GetPlayerPrefsStringValue(USER_CURRENT_LEVEL_PLAN); }
        }

        public long CurrentGameTime()
        {
            return GameTimeWhenLastClose +
                   Libs.TimeUtils.SecondsBetween(BaseGameConsole.singletonInstance.GameStartDateTime, DateTime.Now);
        }

        public bool HasSendFirstNoMoneySessionTime
        {
            set { SharedPlayerPrefs.SetPlayerPrefsBoolValue(SEND_GAME_SESSION_TIME, true); }
            get { return SharedPlayerPrefs.GetPlayerBoolValue(SEND_GAME_SESSION_TIME, false); }
        }

        public bool HasShowBetChangeTips
        {
            set { SharedPlayerPrefs.SetPlayerPrefsBoolValue(HAS_SHOW_BET_CHANGE, true); }
            get { return SharedPlayerPrefs.GetPlayerBoolValue(HAS_SHOW_BET_CHANGE, false); }
        }

        public bool IsClickedSupport { get; set; }

        public string LastChooseSlotName
        {
            set { SharedPlayerPrefs.SetPlayerPrefsStringValue(LAST_PLAY_SLOT_INDEX, value); }
            get { return SharedPlayerPrefs.GetPlayerPrefsStringValue(LAST_PLAY_SLOT_INDEX, ""); }
        }

        public long CurrentBet
        {
            set { PlayerPrefs.SetString("CurrentBet", value.ToString()); }
            get
            {
                return
                    Utils.Utilities.CastValueLong(PlayerPrefs.GetString("CurrentBet", "100"));
            }
        }

        public void SetCurrentBet(long bet)
        {
            CurrentBet = bet;
        }

        public long GetFirstLaunchTime()
        {
            long firstLaunchTimeLong = SharedPlayerPrefs.LoadPlayerPrefsLong(FIRST_LANUCH_TIME, -1);
            if (firstLaunchTimeLong == -1)
            {
                DateTime currentDateTime = DateTime.Now;
                firstLaunchTimeLong = CSharpUtil.ConvertDateTimeLongInSecond(currentDateTime);
                SharedPlayerPrefs.SavePlayerPrefsLong(FIRST_LANUCH_TIME, firstLaunchTimeLong);
            }

            return firstLaunchTimeLong;
        }

        #region Launch Time Backup And Recovery

        public static long GetRawFirstLaunchTime()
        {
            return SharedPlayerPrefs.LoadPlayerPrefsLong(FIRST_LANUCH_TIME, -1);
        }

        public static void SetRawFirstLaunchTime(long firstLaunchTime)
        {
            SharedPlayerPrefs.SavePlayerPrefsLong(FIRST_LANUCH_TIME, firstLaunchTime);
        }

        #endregion

        public bool isOlderUser = false;

        public void SaveFirstLaunchTime()
        {
            long firstLaunchTimeLong = SharedPlayerPrefs.LoadPlayerPrefsLong(FIRST_LANUCH_TIME, -1);
            if (firstLaunchTimeLong == -1)
            {
                DateTime currentDateTime = DateTime.Now;
                firstLaunchTimeLong = CSharpUtil.ConvertDateTimeLongInSecond(currentDateTime);
                SharedPlayerPrefs.SavePlayerPrefsLong(FIRST_LANUCH_TIME, firstLaunchTimeLong);
                SharedPlayerPrefs.SetPlayerPrefsBoolValue("isOlderUser", false);
                isOlderUser = false;
            }
            else
            {
                isOlderUser = SharedPlayerPrefs.GetPlayerBoolValue("isOlderUser", true);
            }
        }

        public float DaysFromFirstLaunch()
        {
            long firstLaunchTimeLong = GetFirstLaunchTime();
            long currentTime = CSharpUtil.ConvertDateTimeLongInSecond(DateTime.Now);
            return (currentTime - firstLaunchTimeLong) / (CSharpUtil.DAY * 1f);
        }

        public int DaysFromLastLaunch()
        {
            if (lastLaunchTimeLong == -1)
            {
                lastLaunchTimeLong = CSharpUtil.ConvertDateTimeLongInSecond(DateTime.Now);
                SharedPlayerPrefs.SavePlayerPrefsLong(USER_LAST_TIME_LANUCH_TIME, lastLaunchTimeLong);
            }

            DateTime lastLaunchTime = CSharpUtil.ConvertLongToDateInSecond(lastLaunchTimeLong);
            return Libs.TimeUtils.DaysBetweenTwoDates(lastLaunchTime, DateTime.Now);
        }

        public void SaveLastLaunchTime()
        {
            DateTime currentDateTime = DateTime.Now;
            long lastLaunchTimeLong = CSharpUtil.ConvertDateTimeLongInSecond(currentDateTime);
            SharedPlayerPrefs.SavePlayerPrefsLong(USER_LAST_TIME_LANUCH_TIME, lastLaunchTimeLong);
        }

        public void SaveSpinCountForLevelUpInterval(int count)
        {
            SharedPlayerPrefs.SetPlayerPrefsIntValue(LEVEL_UP_INTERVAL_SPIN_COUNT, count);
            SharedPlayerPrefs.SavePlayerPreference();
        }

        public int GetSpinCountForLevelUpInterval()
        {
            return SharedPlayerPrefs.GetPlayerPrefsIntValue(LEVEL_UP_INTERVAL_SPIN_COUNT, 0);
        }

        public void SetLevelUpIntervalStartTime()
        {
            SharedPlayerPrefs.SavePlayerPrefsLong(LEVEL_UP_INTERVAL_TOTAL_TIME,
                Libs.TimeUtils.ConvertDateTimeLong(DateTime.Now));
            SharedPlayerPrefs.SavePlayerPreference();
        }

        public long GetLevelUpIntervalTotalTime()
        {
            long nowTime = Libs.TimeUtils.ConvertDateTimeLong(DateTime.Now);
            long startTime = SharedPlayerPrefs.LoadPlayerPrefsLong(LEVEL_UP_INTERVAL_TOTAL_TIME, nowTime);
            return nowTime - startTime;
        }

        [SerializeField] private int level;
        [SerializeField] private long balance;
        [SerializeField] long lastLaunchTimeLong;

        [SerializeField] private long totalSpins;
        

        //评分弹窗出现次数记录
        public int GetRatingDialogPopedTime(string machineName)
        {
            return SharedPlayerPrefs.GetPlayerPrefsIntValue(RATING_DIALOG_POP_TIME + '_' + machineName, -1);
        }

        public void SetRatingDialogPopedTime(string machineName, int popedTime)
        {
            SharedPlayerPrefs.SetPlayerPrefsIntValue(RATING_DIALOG_POP_TIME + '_' + machineName, popedTime);
            SharedPlayerPrefs.SavePlayerPreference();
        }


       
        private static readonly string BET_LINES = "betLines";
        private static readonly string LEVEL = "level";
        private static readonly string BALANCE = "balance";

        private static readonly string PLAY_GAME_TOTAL_TIMES = "playGameTotalTimes";
        private static readonly string SHOWCOINSMULTIPLIER = "ShowCoinsMultiplier";

        private static readonly string TOTAL_SPINS = "totalSpins";
        private static readonly string MANUAL_SPIN_COUNTER = "manualSpinCounter";
        private static readonly string HAS_GRANT_INITIAL_BONUS = "hasGrantInitialBonus";

        private static readonly string HAS_RATE_US = "HasRateUs";
        private static readonly string HAS_CHANGE_BET = "HasChangeBet";

        //Hourly Bonus LQ Add
        private static readonly string PLAYER_COLLECT_HOURLY_BONUS_TIMES = "PlayerCollectHourlyBonusTimes";

        private const string Has_Rate_Machine = "HasRateMachine_";
        private const string HAS_SEND_ALL_SPIN_TIME = "HasSendAllSpinTime";
        private const string ALL_SPIN_TIMES = "AllSpinTimesInLife";

        private const string FIRST_GAME_SESSION = "FIRST_GAME_SESSION";
        private const string GAME_SESSION_TIME = "GAME_SESSION_TIME";
        private const string SEND_GAME_SESSION_TIME = "SEND_GAME_SESSION_TIME";
        private const string HAS_SHOW_BET_CHANGE = "HAS_SHOW_BET_CHANGE";

        private const string FIRST_LANUCH_TIME = "firstLanuchTime";

        private const string LAST_PLAY_SLOT_INDEX = "LAST_PLAY_SLOT_INDEX";
        private const string USER_LAST_TIME_LANUCH_TIME = "UserLastLanuchTime";

        private const string WAIT_OPEN_RATE_ALERT_TIME = "WaitOpenRateAlertTime";

        private const string RATING_DIALOG_POP_TIME = "RATING_DIALOG_POP_TIME";

        public readonly string FIRST_PLAY_SLOT_MACHINE_SPIN_COUNT = "FirstPlaySlotMachineSpinCount";
        public readonly string FIRST_PLAY_SLOT_MACHINE_SPIN_FLAG = "FirstPlaySlotMachineSpinFlag";

        public const string LAST_EXIT_GAME_DATETIME = "LAST_EXIT_GAME_DATETIME";
        public const string WELCOME_BACK_RECEIVE_COUNT = "WELCOME_BACK_RECEIVE_COUNT";

        public const string USER_CURRENT_LEVEL_PLAN = "USER_CURRENT_LEVEL_SCHEME";

        public const string LEVEL_UP_INTERVAL_SPIN_COUNT = "LevelUpIntervalSpinCount";
        public const string LEVEL_UP_INTERVAL_TOTAL_TIME = "LevelUpIntervalTotalTime";
    }
}