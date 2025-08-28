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
      
        private Button _ensureBtn;
        private TextMeshProUGUI _time; 
        protected override void Awake()
        {
            base.Awake();
            _ensureBtn =  Utils.Utilities.RealFindObj<Button>(transform, "Anchor/ensureBtn");
            _time =  Utils.Utilities.RealFindObj<TextMeshProUGUI>(transform, "Anchor/time");
            _ensureBtn.onClick.AddListener(EnsureBtnClick);
        }
        
        private Coroutine _countdown;
        public void SetUIData(int cash)
        {
            
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
                    _time.text = TimeUtils.GetLeftTime_Day_And_HMS(timeSpan);
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