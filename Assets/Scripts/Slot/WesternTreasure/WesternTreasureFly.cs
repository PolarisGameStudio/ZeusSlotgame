using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Spine.Unity;
using UnityEngine.UI;
using UnityEngine.Video;

public class WesternTreasureFly : MonoBehaviour
{
  
    #region BaseGame bonus collect

    public GameObject DestinationGO;
    public GameObject Fly;
    //pen的动画 skin:"BG_Tree_1","BG_Tree_2","BG_Tree_3","BG_Tree_4",clip:"idle","win","Level up","Tree 4"
    public SkeletonGraphic skeletonGraphic;
    private WesternTreasureReelManager treeManager;
    public GameObject triggerEffect;
    private Stack<GameObject> m_ImagePool = new Stack<GameObject>();
    //转场动画
    public SkeletonGraphic JackPotAni;
    //girl的动画  0:"hu xi",1:"animation6"
    public SkeletonGraphic GirlGraphic;
    //girl的动画  0:"hu xi",1:"animation6"
    public Image slider;
    
    // 调整时间为符合总长1.5秒
    public float moveDuration = 0.4f;          
    public float holdDuration = 0.2f;          
    public float endMoveDuration = 0.4f;       
    public float scaleUpDuration = 0.3f;       
    public float scaleDownDuration = 0.2f;  
    public void Init(WesternTreasureReelManager _treeManager)
    {
        treeManager = _treeManager;
    }

    public void BaseGameCollectBonus(int reelIndex,List<Vector3> sourcePosition)
    {
        for (int i = 0; i < sourcePosition.Count; i++)
        {
            AudioManager.Instance.AsyncPlayEffectAudio("W01_collect");
            GameObject go = null;
            if (m_ImagePool.Count > 0)
            {
                go = this.m_ImagePool.Pop();
            }
            else
            {
                go =  Instantiate(Fly, this.transform) as GameObject; 
            }
            
            
            go.transform.position = sourcePosition[i];
           
            go.gameObject.SetActive(true);
            go.transform.localScale = Vector3.one;
            
            Sequence seq = DOTween.Sequence();
            seq.Append(go.transform.DOMove(DestinationGO.transform.position, 1.2f).OnComplete(delegate
            {
                go.SetActive(false);
                m_ImagePool.Push(go);
                if (treeManager.isFreespinBonus)
                {
                    treeManager.spinResult.wildNum += treeManager.spinResult.treeGrow.freeWild;
                }
                else
                {
                    treeManager.spinResult.wildNum += treeManager.spinResult.treeGrow.baseWild;
                }
            
            
                ChangeTreeState();
             
            }));
            //sequence.Join(go.transform.DOScale(0.1f, 1.2f));
            seq.SetUpdate(true);
            seq.Play();
        }  
    }

    private void ChangeTreeState()
    {
        int level = ChargeTreeLevel(treeManager.spinResult.wildNum);
        if (treeManager.spinResult.curTreeLevel<level)
        {
            treeManager.spinResult.curTreeLevel++;
            StartCoroutine(TriggerLevelUp());
        }
        else
        {
            PlayAnimation(1);
        }
        //刷新进度条的进度
        RefreshSlider();
    }

    //触发 level up升级
    private IEnumerator TriggerLevelUp()
    {
        yield return new WaitForSeconds(2f);
        PlayAnimation(2);
        AudioManager.Instance.AsyncPlayEffectAudio("treelevelup");
        yield return new WaitForSeconds(0.6f);
        ChangeSkin(treeManager.spinResult.curTreeLevel);
        yield return new WaitForSeconds(1);
    }

    private Tweener tweener;
    public void RefreshSlider(bool isInit = false)
    {
        int nowValue = treeManager.spinResult.wildNum;
        int targetNum = treeManager.spinResult.treeGrow.level4;
        float initValue = slider.fillAmount;
        float targetValue =  nowValue * 1.0f / targetNum;
        if (isInit)
        {
            slider.fillAmount = targetValue;
        }
        else
        {
            if (tweener!=null)
            {
                tweener?.Kill();
            }
            //DOTween增长进度条
            if (initValue<targetValue)
            {
                tweener = DOTween.To(() => initValue, x => initValue = x, targetValue, 1.0f)
                    .OnUpdate(() =>
                    {
                        slider.fillAmount = initValue;
                    })
                    .OnComplete(() =>
                    {
                        slider.fillAmount = targetValue;
                        tweener?.Kill();
                    }).SetUpdate(true);
            }
            else
            {
                slider.fillAmount = targetValue;
            }
        }
    }
    public void EnterJackpotGame()
    {
        if (treeManager.spinResult.isTriggerJackpotGame)
        {
            StartCoroutine(TriggerJackpot());
        }else if (treeManager.spinResult.isOpenEndDialog)
        {
            treeManager.OpenJackpotEndDialog(treeManager.spinResult.winType,treeManager.spinResult.winMoney);
        }
        else
        {
            treeManager.OpenJackpotGame();
        }
    }

    //触发jackpot游戏 动画表现结束后进入游戏
    private IEnumerator TriggerJackpot()
    {
        if (treeManager.spinResult.isInJackpotGame)
        {
            treeManager.spinResult.wildNum = 40000;
            treeManager.spinResult.curTreeLevel = 4;
            // PlayGirlAni(1);
            // yield return new WaitForSeconds(1.0f);
            PlayAnimation(2);
            // AudioManager.Instance.AsyncPlayEffectAudio("treelevelup");
            yield return new WaitForSeconds(0.6f);
            ChangeSkin(4);
            yield return new WaitForSeconds(1);
            // PlayGirlAni(0);
            //此粒子特效先关闭
            //SetTriggerEffect(true);
            PlayAnimation(3);
            AudioManager.Instance.AsyncPlayEffectAudio("treelevelup");
            yield return new WaitForSeconds(1f);
            Messenger.Broadcast<string>(GameConstants.PlayFullScreenAnimation, "JackPotStart");
            AudioManager.Instance.AsyncPlayEffectAudio("JackPotStart");
            yield return new WaitForSeconds(3.5f);
            // SetTriggerEffect(false);
            treeManager.OpenJackpotGame();
        }
    }

    private void SetTriggerEffect(bool isShow)
    {
        if(triggerEffect!=null)
            triggerEffect.SetActive(isShow);
    }

    //判断树的等级
    public int ChargeTreeLevel(int wildNum)
    {
        if (wildNum<treeManager.spinResult.treeGrow.level2)
        {
            return 1;
        }else if (wildNum<treeManager.spinResult.treeGrow.level3)
        {
            return 2;
        }else if (wildNum<treeManager.spinResult.treeGrow.level4)
        {
            return 3;
        }
        else
        {
            return 4;
        }
      
    }

    #endregion

    #region 树动画
    private string animationName = "idle";
   
    void Start() 
    {
        animationName = "idle";
        if(skeletonGraphic != null)
        {
            skeletonGraphic.startingAnimation = "idle";
            if(skeletonGraphic.AnimationState != null)
            {
                skeletonGraphic.AnimationState.Complete += OnComplete;
            }
        }

        // if(GirlGraphic != null)
        // {
        //     GirlGraphic.startingAnimation = "hu xi";
        //     if(GirlGraphic.AnimationState != null)
        //     {
        //         GirlGraphic.AnimationState.Complete += OnGirlPlayComplete;
        //     }
        // }
    }

    private void OnComplete(Spine.TrackEntry entry)
    {
        if (entry.animation.name != animationName)
        {
            //播放完Tree 4动画后，继续播放idle动画
            skeletonGraphic.AnimationState.ClearTrack(0);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            skeletonGraphic.AnimationState.SetAnimation(0, animationName, true);
        }
    }

    private void OnGirlPlayComplete(Spine.TrackEntry entry)
    {
        
    }
    public void PlayAnimation(int animationId)
    {
        string aniName = GetAnimationName(animationId);
        if (!string.IsNullOrEmpty(aniName))
            skeletonGraphic.AnimationState.SetAnimation(0, aniName, false);
    }

    public void ChangeSkin(int skinId)
    {
        string skinName = GetSkinName(skinId);
        if (!string.IsNullOrEmpty(skinName))
            skeletonGraphic.Skeleton.SetSkin(skinName);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton); 
    }

    public void ChangeCandyJarSkin(int skinId)
    {
        string skinName = GetSkinName(skinId);
        if (!string.IsNullOrEmpty(skinName))
            skeletonGraphic.Skeleton.SetSkin(skinName);
        skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton);
    }

    
    //0-->循环呼吸 1-->挥魔法棒
    public void PlayGirlAni(int id)
    {
        //动画重置一下
        GirlGraphic.AnimationState.ClearTracks();
        if (id==0)
        {
            GirlGraphic.AnimationState.SetAnimation(0, "hu xi", true);
        }
        else if (id ==1)
        {
            GirlGraphic.AnimationState.SetAnimation(0, "animation6", false);
        }
        
    }
    
    public void ClearAnimation()
    {
        //skeletonGraphic.AnimationState.SetEmptyAnimations(0); 
        skeletonGraphic.AnimationState.SetAnimation(0,animationName,true);
        if (JackPotAni!=null)
        {
            JackPotAni.AnimationState.SetAnimation(0, animationName, true);
        }

        if (GirlGraphic!=null)
        {
            PlayGirlAni(0);
        }
    }

    
    //animationId:  1、win 2、level up 3、Tree_4
    private string GetAnimationName(int animationId)
    {
        if (animationId==1)
        {
            return "win";
        }
        else if (animationId==2)
        {
            return "Level up"; 
        }
        else if (animationId==3)
        {
            return "Tree 4";
        }

        return null;
    }
//skinId: 1、BG_Tree_1    2、BG_Tree_2    3、BG_Tree_3   4、BG_Tree_4
    private string GetSkinName(int skinId)
    {
        if (skinId==1)
        {
            return "BG_Tree_1";
        }
        else if (skinId==2)
        {
            return "BG_Tree_2"; 
        }
        else if (skinId==3)
        {
            return "BG_Tree_3";
        }
        else if (skinId==4)
        {
            return "BG_Tree_4";
        }
        return null;
    }

    private void OnDestroy()
    {
        if (skeletonGraphic != null)
            skeletonGraphic.AnimationState.Complete -= OnComplete;
        if (GirlGraphic != null)
            GirlGraphic.AnimationState.Complete -= OnGirlPlayComplete;
    }

    //    void OnGUI()
//        {
//            if (GUI.Button(new Rect(100, 100, 300, 300), "DisactiveButtons true"))
//            {
//                Messenger.Broadcast<bool>(SlotControllerConstants.DisactiveButtons, true);
//                
//            }
//
//            if (GUI.Button(new Rect(500, 100, 300, 300), "DISABLEAUTOSPIN true"))
//            {
//                Messenger.Broadcast<bool>(SpinButtonStyle.DISABLEAUTOSPIN, true);
//               
//            }
//            if (GUI.Button(new Rect(900, 100, 300, 300), "DisableSpinButton true"))
//            {
//                
//                Messenger.Broadcast<bool>(SlotControllerConstants.DisableSpinButton, true); 
//            }
//            if (GUI.Button(new Rect(100, 500, 300, 300), "DisactiveButtons false"))
//            {
//                Messenger.Broadcast<bool>(SlotControllerConstants.DisactiveButtons, false);
//                
//            }
//
//            if (GUI.Button(new Rect(500, 500, 300, 300), "DISABLEAUTOSPIN false"))
//            {
//               
//                Messenger.Broadcast<bool>(SpinButtonStyle.DISABLEAUTOSPIN, false);
//               
//            }
//            if (GUI.Button(new Rect(900, 500, 300, 300), "DisableSpinButton false"))
//            {
//                
//                Messenger.Broadcast<bool>(SlotControllerConstants.DisableSpinButton, false); 
//            }
//        }

    
    #endregion

   
    
    
}
