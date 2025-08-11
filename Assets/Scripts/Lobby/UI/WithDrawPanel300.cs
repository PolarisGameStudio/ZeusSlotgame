using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Libs;
using TMPro;
using UI.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DelayAction = Libs.DelayAction;
using Image = UnityEngine.UI.Image;

public class WithDrawPanel300 : WithDrawPanel
{
    public Transform particlePar;
    public ParticleSystem particle1;
    public ParticleSystem particle2;
    public Image Progress;
    public Image image_head;
    private int maxValue = 0;
    private RectTransform progressBar;
    public float maxLifeTime = 12f;
    private Animator animator;
    public override void InitMoney(int money)
    {
        animator = GetComponent<Animator>();
        initNum = money;
        maxValue = OnLineEarningMgr.Instance.GetMaxValue()*OnLineEarningMgr.Instance.GetCashMultiple();
        progressBar = Progress.transform.GetComponent<RectTransform>();
        SetCashText();
        ShowProgress();
    }

    public override void CompleteShow(int coins)
    {
        base.CompleteShow(coins);
        ShowProgress();
        if (animator!=null)
        {
            animator.SetTrigger("idle");
        }
    }

    public override void CaculateTxt()
    {
        base.CaculateTxt();
        ShowProgress();
    }

    public override void ShowWithCoinsFly(int coinsNum)
    {
        if (animator!=null)
        {
            animator.SetTrigger("update");
        }
        
        base.ShowWithCoinsFly(coinsNum);
    }

    void ShowProgress()
    {
        if (Progress!=null)
        {
            float f = initNum / (maxValue*1.0f);
            if (particlePar!=null)
            {
                Vector3 endPosition = new Vector3(progressBar.rect.width * f - progressBar.rect.width/2, 0, 0);
                image_head.transform.localPosition = endPosition;
                // 转换坐标并更新
                Vector3 worldPos = progressBar.TransformPoint(endPosition);
                particlePar.position = worldPos;
                float particleLifeTime = Mathf.Max(maxLifeTime * f,0.05f);
                if (particle1 != null)
                {
                    ParticleSystem.MainModule main = particle1.main;
                    main.startLifetime = particleLifeTime;
                }
                if (particle2 != null)
                {
                    ParticleSystem.MainModule main = particle2.main;
                    main.startLifetime = particleLifeTime;
                }
            }
            Progress.fillAmount = f;
        }
    }

    public override void SetCashText()
    {
        cashText.text = string.Format("{0}/{1}",OnLineEarningMgr.Instance.GetMoneyStr((int)initNum,2,false,false),
            OnLineEarningMgr.Instance.GetMoneyStr(maxValue,2,false,false));
    }
}
