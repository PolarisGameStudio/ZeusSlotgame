using System;
using System.BuffSystem;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Libs;
using TMPro;
using UniRx.Async;
using UnityEngine;
using UnityEngine.UI;

namespace Activity
{
    public class RotationBuffIcon:BaseIcon
    {
        public GameObject morebet;
        public GameObject morebonus;
        public GameObject morewild;
        public TextMeshProUGUI tmpUI;
        private Button btn;
        private BaseBuff currentBuff;
        public Coroutine timeCor;
        public Image filledImage;
        public TextMeshProUGUI activeProgress;
        public GameObject sliderMask;
        public RotationBuffActivity rotationBuffActivity;
        public override void OnInit(int Id, Dictionary<string, object> data)
        {
            base.OnInit(Id, data);
            btn = gameObject.GetComponent<Button>();
            if (btn!=null)
            {
                btn.onClick.AddListener(OnClick);
            }
        }

        public void UpdateBuffUI(BaseBuff buff,RotationBuffActivity activity)
        {
            rotationBuffActivity = activity;
            currentBuff = buff;
            tmpUI.text = "";
            sliderMask.gameObject.SetActive(false);
            //更新buff显示
            switch (buff.buffType)
            {
                case BuffConstant.MoreCashBuff:
                    //显示MoreCashBuff图标
                    ShowMoreBonusUI();
                    break;
                case BuffConstant.ChangeADMultipleBuff:
                    //显示ChangeADMultipleBuff图标
                    ShowMoreBetUI();
                    break;
                case BuffConstant.MultipleWildSymbolBuff:
                    //显示MultipleWildSymbolBuff图标
                    ShowMoreWildUI();
                    break;
            }
            sliderMask.gameObject.SetActive(currentBuff.isActive);
            if (!currentBuff.isActive)
            {
                //显示轮转的倒计时
                StartActiveCountdown();
            }
        }

        #region 轮转倒计时

        private CancellationTokenSource _cancellationTokenSource;
        private int countDownStart = 30; // 倒计时起始值（秒）
        public void StartActiveCountdown()
        {
            countDownStart = rotationBuffActivity.displayDuration;
            _cancellationTokenSource = new CancellationTokenSource();
            StartCountdown(_cancellationTokenSource.Token).Forget();
        }
        private async UniTaskVoid StartCountdown(CancellationToken token)
        {
            try
            {
                int currentTime = countDownStart;

                while (currentTime > 0)
                {
                    // 等待1秒（可被取消）
                    await UniTask.Delay(1000, cancellationToken: token); 

                    currentTime--;
                    if (activeProgress!=null)
                    {
                        activeProgress.text = TimeUtils.GetLeftSecondString(currentTime);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 当GameObject被销毁时自动取消
                Debug.Log("倒计时已取消");
            }
        }
        public void CancelCountDown()
        {
            _cancellationTokenSource?.Cancel();
            activeProgress.text = "";
        }

        #endregion
        
        private void ShowMoreBetUI()
        {
            HideMoreBonusUI();
            HideMoreWildUI();
            morebet.gameObject.SetActive(true);
            if (currentBuff.isActive)
            {
                tmpUI.text = currentBuff.targetNum - currentBuff.currentNum +"/"+ currentBuff.targetNum;
            }
        }
        private void HideMoreBetUI()
        {
            morebet.gameObject.SetActive(false);
        }
        
        private void ShowMoreBonusUI()
        {
            HideMoreBetUI();
            HideMoreWildUI();
            morebonus.gameObject.SetActive(true);
        }
        
        private void HideMoreBonusUI()
        {
            morebonus.gameObject.SetActive(false);
        }
        
        private void ShowMoreWildUI()
        {
            HideMoreBetUI();
            HideMoreBonusUI();
            morewild.gameObject.SetActive(true);
        }
        
        private void HideMoreWildUI()
        {
            morewild.gameObject.SetActive(false);
        }
        
        private void OnClick()
        {
            //处理点击事件
            ActivityManager.Instance.OnClickIcon(activityId);
        }

        public void StartCountDown()
        {
            if (tmpUI==null)
            {
                Debug.Log ("Please add count down text in inspector");
                return;
            }
            timeCor = StartCoroutine (StartTimeCor());
        }
        
        IEnumerator StartTimeCor()
        {
            // 初始剩余时间（浮点数用于精确计算）
            float totalLeftTime = currentBuff.duration - currentBuff.activiteTime;
          
            while (true)
            {
                if (IsTimeOver())
                {
                    yield break;
                }

                // 记录当前帧开始时的整数秒
                int lastSecond = Mathf.FloorToInt(totalLeftTime);

                // 每帧更新进度条
                while (totalLeftTime > 0)
                {
                    // 实时计算剩余时间
                    totalLeftTime -= Time.deltaTime;
                    totalLeftTime = Mathf.Max(totalLeftTime, 0);

                    // 进度条平滑更新
                    filledImage.fillAmount = totalLeftTime / currentBuff.duration;

                    // 每秒触发文本更新
                    int currentSecond = Mathf.FloorToInt(totalLeftTime);
                    if (currentSecond != lastSecond)
                    {
                        tmpUI.text = TimeUtils.GetLeftSecondString(currentSecond);
                        lastSecond = currentSecond;
                    }

                    yield return null; // 等待下一帧
                }

                // 时间耗尽处理
                tmpUI.text = TimeUtils.GetLeftSecondString(0);
                filledImage.fillAmount = 0;
                yield break;
            }
        }

        public void OnBuffActive()
        {
            CancelCountDown();
            sliderMask.gameObject.SetActive(true);
            //根据buff类型更新UI显示
            switch (currentBuff.buffType)
            {
                case BuffConstant.MoreCashBuff:
                    StartCountDown();                        
                    break;
                case BuffConstant.ChangeADMultipleBuff:
                    tmpUI.text = currentBuff.targetNum-currentBuff.currentNum +"/"+ currentBuff.targetNum;
                    filledImage.fillAmount = (float)(currentBuff.targetNum-currentBuff.currentNum)/ currentBuff.targetNum;
                    break;
                case BuffConstant.MultipleWildSymbolBuff:
                    StartCountDown();
                    break;
            }
        }
        
        public bool IsTimeOver()
        {
            return !currentBuff.isActive;
        }
    }
}