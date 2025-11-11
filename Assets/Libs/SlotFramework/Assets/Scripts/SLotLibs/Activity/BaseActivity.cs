using System;
using System.Collections.Generic;
using Libs;
using UnityEngine;
using Utils;

namespace Activity
{
    public enum ActivityType
    {
        None = -1,
        SpinWithDraw=0,
        CardLottery=1,
        CardPack=2,
        WithDrawTask=3,
        H5RewardActivity=4,
        ContinueSpin=5,
        WheelLucy=6,
        RotationBuff=7,
        DailyTask=8
    }

    public enum ActivityState
    {
        Invalid = 0,
        Open = 1,
        Close =2
    }
    public class BaseActivity:IActivity
    {

        private string name;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public ActivityType Type;
        public ActivityState State = ActivityState.Invalid;

        public BaseTask Task;
        
        public bool isOpen = false;

        private long endTime = 0;

        public Dictionary<string, object> Data = new Dictionary<string, object>();
        public Dictionary<string, object> iconData = new Dictionary<string, object>();
        
        public BaseIcon icon;

        //唯一标识，通过id区别不同的活动
        public int id;
        public virtual void OnInit()
        {
            AddListener();
        }

        public BaseActivity(Dictionary<string, object> data)
        {
            if (data == null)
            {
                Debug.LogError($"[BaseActivity][OnCreate] data is null");
                return;
            }

            Data = data;
            
            Type = (ActivityType)Utilities.GetInt(data, ActivityConstants.TYPE, 0);
            if (Type == ActivityType.None)
            {
                Debug.LogError($"[BaseActivity][OnCreate] type is error");
                return;
            }
            //解析数据
            name = Utilities.GetString(data, ActivityConstants.NAME, "");
            isOpen = Utilities.GetBool(data, ActivityConstants.OPEN, false);
            endTime = Utilities.GetLong(data, ActivityConstants.ENDTIME, 0);
            id = Utilities.GetInt(data, ActivityConstants.ID, 0);
            iconData =  Utilities.GetValue<Dictionary<string,object>>(data, ActivityConstants.ICON, null);
            //ParseTaskData();
            InitState();
        }
        public virtual bool IsOpen()
        {
            return isOpen;
        }

        //创建并注册角标
        public virtual BaseIcon RegisterIcon(GameObject go)
        {
            return null;
        }
        protected virtual void ParseTaskData()
        {
            
        }
        private void InitState()
        {
            if (endTime == -1)
            {
                State = ActivityState.Open;
                return;
            }
            long curTimeLong = TimeUtils.ConvertDateTimeLong(DateTime.Now);
            State = curTimeLong<endTime?ActivityState.Open:ActivityState.Close;
        }

        public string GetIconResourceName()
        {
            if (iconData == null)
            {
                return string.Empty;
            }

            string name = Utilities.GetString(iconData, ActivityConstants.IconResource, string.Empty);
            return name;
        }
        
        //进行销毁释放对象
        public virtual void OnDestroy()
        {
            RemoveListener();
        }
        
        //刷新数据
        public virtual void OnRefresh()
        {
        }

        //需要进行update更新的数据
        public virtual void OnUpdate()
        {
        }

        public virtual void AddListener()
        {
        }
        
        public virtual void RemoveListener()
        {
        }

        public virtual void OnClickIcon()
        {
            
        }

        public virtual float GetProgress()
        {
            return 0;
        }
        public virtual string GetProgressText()
        {
            return string.Empty;
        }

        public virtual void SaveData()
        {
            
        }
    }
}