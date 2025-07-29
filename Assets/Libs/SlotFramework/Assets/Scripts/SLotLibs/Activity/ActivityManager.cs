using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CardSystem.Activity;
using Plugins;
using UnityEngine;

namespace Activity
{
    public class ActivityManager:MonoSingleton<ActivityManager>
    {

        private Dictionary<int,BaseActivity> Activities = new Dictionary<int, BaseActivity>();
        private Dictionary<int, Dictionary<string,object>> IconDicts = new Dictionary<int, Dictionary<string,object>>();
        public void OnInit()
        {
            Dictionary<string, object> activity = Configuration.GetInstance().GetValue<Dictionary<string,object>>("Activity", null);
            if (activity==null)
            {
                Debug.LogError($"Plist error: Activity is null");
                return;
            }
            //遍历节点创建活动
            foreach (var item in activity)
            {
                string name = item.Key;
                Dictionary<string, object> data = item.Value as Dictionary<string, object>;
                int activityType = Utils.Utilities.GetInt(data, ActivityConstants.TYPE, -1);
                if (activityType<0)
                {
                    Debug.LogError("[ActivityManager][OnInit] Activity Type is Error");
                    continue;
                }
                BaseActivity baseActivity = CreateActivity(activityType,data);
                if (!baseActivity.IsOpen())
                {
                    continue;
                }

                if (Activities.ContainsKey(baseActivity.id))
                {
                    Debug.Log($"[ActivityManager][OnInit]Already Have Id:{baseActivity.id}");
                    continue;
                }
                Activities.Add(baseActivity.id,baseActivity);
                baseActivity.OnInit();
            }
        }

        BaseActivity CreateActivity(int type,Dictionary<string,object> data)
        {
            BaseActivity activity = null;
            switch (type)
            {
                case (int)ActivityType.SpinWithDraw:
                    activity = new SpinWithDrawActivity(data);
                    break;
                case (int)ActivityType.CardLottery:
                    activity = new CardLotteryActivity(data);
                    break;
                case (int)ActivityType.CardPack:
                    activity = new CardPackActivity(data);
                    break;
                case (int)ActivityType.WithDrawTask:
                    activity = new WithDrawTaskActivity(data);
                    break;
                case (int)ActivityType.H5RewardActivity:
                    activity = new H5RewardActivity(data);
                    break;
                case (int)ActivityType.ContinueSpin:
                    activity = new ContinueSpinActivity(data);
                    break;
            }

            return activity;
        }

        public BaseActivity RegisterActivity(Dictionary<string,object> data)
        {
            int activityType = Utils.Utilities.GetInt(data, ActivityConstants.TYPE, -1);
            BaseActivity baseActivity = CreateActivity(activityType,data);
            if (!baseActivity.IsOpen())
            {
                return null;
            }

            if (Activities.ContainsKey(baseActivity.id))
            {
                Debug.Log($"[ActivityManager][OnInit]Already Have Id:{baseActivity.id}");
                return Activities[baseActivity.id];
            }
            Activities.Add(baseActivity.id,baseActivity);
            baseActivity.OnInit();
            return baseActivity;
        }   
        
        public void RemoveActivity(int id)
        {
            if (Activities.ContainsKey(id))
            {
                BaseActivity activity = Activities[id];
                activity.OnDestroy();
                Activities.Remove(id);
            }
        }
        
        public void RemoveIcon(int activityId)
        {
            Messenger.Broadcast<int>(ActivityConstants.RemoveIconMsg,activityId);
        }
        
        public Dictionary<int,BaseActivity> GetActivities()
        {
            return Activities;
        }
        private void Update()
        {
            foreach (var item in Activities)
            {
                if (item.Value.IsOpen())
                {
                    item.Value.OnUpdate();
                }
            }
        }

        public BaseActivity GetActivityByID(int id)
        {
            return Activities[id];
        }
        
        public BaseActivity GetActivityByType(ActivityType type)
        {
            BaseActivity activity = null;
           
            foreach (var item in Activities)
            {
                if (item.Value.Type == type)
                {
                    activity = item.Value;
                    break;
                }
            }
            return activity;
        }
        
        public void OnClickIcon(int id)
        {
            BaseActivity activity = GetActivityByID(id);
            activity.OnClickIcon();
        }
    }
}