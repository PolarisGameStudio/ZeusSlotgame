using System;
using System.Collections;
using Libs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Classic
{
    public class AccountLoginTipsDialog:UIDialog
    {
      
        public Button ensureBtn;
        public Button closeBtn;
        public TextMeshProUGUI time; 
        private int money = 0;
        protected override void Awake()
        {
            base.Awake();
            ensureBtn.onClick.AddListener(EnsureBtnClick);
            closeBtn.onClick.AddListener(Close);
        }
        
        private Coroutine _countdown;
        public void SetUIData(int cash)
        {
            money = cash;
            
            var endTime = TimeUtils.ConvertDateTimeLong(DateTime.Now) + WithDrawManager.Instance.GetCoolTime();
            
            _countdown = CoroutineUtil.Instance.StartCoroutine(ShowCountDownText(endTime));
        }
        
        private readonly WaitForSecondsRealtime _waitOneSecond = new WaitForSecondsRealtime(1);
        IEnumerator ShowCountDownText(long endTime)
        {
            long nowTime = TimeUtils.ConvertDateTimeLong(DateTime.Now);
            while (nowTime <= endTime)
            {
                try
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds(endTime - nowTime);
                    time.text = TimeUtils.GetLeftTime_Day_And_HMS(timeSpan);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                yield return _waitOneSecond;
                nowTime++;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CoroutineUtil.Instance.StopCoroutine(_countdown);
        }

        private void EnsureBtnClick()
        {
            Debug.Log("[AccountEnsureDialog][EnsureBtnClick]");
            this.Close();
        }
    }
}