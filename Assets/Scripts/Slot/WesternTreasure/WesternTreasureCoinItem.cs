using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WesternTreasureCoinItem : MonoBehaviour
{
    private Image jackpotImage;
    private Animator animator;
    private GameObject grandEffect;
    private GameObject majorEffect;
    private GameObject manorEffect;
    private GameObject miniEffect;
    private GameObject clickGrandEffect;
    private GameObject clickMajorEffect;
    private GameObject clickManorEffect;
    private GameObject clickMiniEffect;
    public int curType;
    public void Init()
    {
        jackpotImage = Util.FindObject<Image>(transform, "Jackpot/MINI");
        animator = this.GetComponent<Animator>();
        grandEffect = Util.FindObject<GameObject>(transform, "Jackpot/Jackpot_effect/Grand");
        majorEffect = Util.FindObject<GameObject>(transform, "Jackpot/Jackpot_effect/Major");
        manorEffect = Util.FindObject<GameObject>(transform, "Jackpot/Jackpot_effect/Minor");
        miniEffect = Util.FindObject<GameObject>(transform, "Jackpot/Jackpot_effect/Mini");
        clickGrandEffect = Util.FindObject<GameObject>(transform, "Jackpot_Glow/Glow_Grand");
        clickMajorEffect = Util.FindObject<GameObject>(transform, "Jackpot_Glow/Glow_Major");
        clickManorEffect = Util.FindObject<GameObject>(transform, "Jackpot_Glow/Glow_Minor");
        clickMiniEffect = Util.FindObject<GameObject>(transform, "Jackpot_Glow/Glow_Mini");
    }
    public void Click(int type, List<Image> selectImageList,int animationID)
    {
        SetContent(type,selectImageList);
        if (animationID==1)
        {
            PlayEffect(type);
        }
        PlayAnimation(animationID);
        
        curType = type;
    }
    public void SetContent(int type, List<Image> selectImageList)
    {
        if (type == 2)
        {
            jackpotImage.sprite = selectImageList[0].sprite;
        }
        else if (type == 3)
        {
            jackpotImage.sprite = selectImageList[1].sprite;
        }
        else if (type == 4)
        {
            jackpotImage.sprite = selectImageList[2].sprite;
        }
        else if (type == 5)
        {
            jackpotImage.sprite = selectImageList[3].sprite;
        }
        jackpotImage.SetNativeSize();
    }
    //0:JackpotGame_Coin_Idle  1: 点击展示动画  JackpotGame_Coin  2: 未点击展示动画 JackpotGame_show 3：展示中奖动画 JackpotGame_win
    public void PlayAnimation(int id)
    {
        switch (id)
        {
            case 0:
                animator.Play("JackpotGame_Coin_Idle");
                break;
            case 1:
                animator.Play("JackpotGame_Coin");
                break;
            case 2:
                animator.Play("JackpotGame_show");
                break;
            case 3:
                animator.Play("JackpotGame_win");
                break;
            default:
                break;
        }
    }

    private void PlayEffect(int type)
    {
        switch (type)
        {
            case 2:
                grandEffect.SetActive(true);
                clickGrandEffect.SetActive(true);
                break;
            case 3:
                majorEffect.SetActive(true);
                clickMajorEffect.SetActive(true);
                break;
            case 4:
                manorEffect.SetActive(true);
                clickManorEffect.SetActive(true);
                break;
            case 5:
                miniEffect.SetActive(true);
                clickMiniEffect.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void PlayWinEffect(int _curType )
    {
        if (curType==_curType)
        {
            //Debug.LogError("PlayWinEffect-----------    "+_curType);
            PlayAnimation(3);
            PauseAllParticle(_curType);
        }
    }

    private void PauseAllParticle(int _curType)
    {
        if (_curType==2)
        {
            StartCoroutine(PauseParticle(grandEffect));
        }else if (_curType==3)
        {
            StartCoroutine(PauseParticle(majorEffect));
        }else if (_curType==4)
        {
            StartCoroutine(PauseParticle(manorEffect));
        }else if (_curType==5)
        {
            StartCoroutine(PauseParticle(miniEffect));
        }
    }
        
    
         

    private IEnumerator PauseParticle(GameObject go)
    {
        yield return new WaitForSeconds(1f);
        ParticleSystem[] arr = go.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i].Pause();
        }
     
    }

    public void ClearEffect()
    {
        grandEffect.SetActive(false);
        majorEffect.SetActive(false);
        manorEffect.SetActive(false);
        miniEffect.SetActive(false);
        clickGrandEffect.SetActive(false);
        clickMajorEffect.SetActive(false);
        clickManorEffect.SetActive(false);
        clickMiniEffect.SetActive(false);
    }
}
