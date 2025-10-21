using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Libs;
using Classic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Classic
{
    public class GiftBoxDialog : UIDialog, IPointerClickHandler
    {
        Animator myAnimator;
        public Transform giftItemParent;
        public GameObject giftIiemPrefab;
        // List<AwardItemBase> giftItemBaseList = new List<AwardItemBase>();
        public GameObject collectBtn;
        protected override void Awake()
        {
            Messenger.Broadcast(GameConstants.ShowGiftBoxDialogMsg);
            base.Awake();
            this.isNeedMask = true;
            AudioManager.Instance.AsyncPlayEffectAudio (AudioEntity.audioGiftBoxDown);
            collectBtn.SetActive(false);
            Messenger.Broadcast (SlotControllerConstants.AUTO_SPIN_SUSPEND);
            Messenger.AddListener(GiftBoxDialog_EventListener.In_Animaiton_End_Key, OnEndOfAnimationIn);
            Messenger.AddListener(GiftBoxDialog_EventListener.Open_Animation_Key, OnEndOfAnimationOpen);
        }

        public void Init(string strConfigs, bool isOpen = true)
        {
            string inAniTrigger = "in";
            //if(isOpen) inAniTrigger = "in1";
            if(isPortrait)
                myAnimator = transform.GetComponentInChildren<Animator>();
            else
                myAnimator = transform.GetComponent<Animator>();
            
            if (myAnimator != null) myAnimator.SetTrigger(inAniTrigger);

			// giftItemBaseList = AwardItemMgr.Instance.ParseGiftItemListFromConfigString(strConfigs,giftItemBaseList);
            
            new DelayAction(0.3f,null, OnEndOfAnimationOpen).Play();
        }
        public void OnEndOfAnimationIn()
        {
            if (myAnimator == null) return;
            myAnimator.SetTrigger("idle");
            AudioManager.Instance.AsyncPlayEffectAudio(AudioEntity.audioGiftBoxOpen);
        }

        public void PlaySoundEffect()
        {
            
        }
        
        public void CloseDialog()
        {
            
        }
        
        public void ShowGiftBoxReward()
        {
            
        }
        
        public void OnEndOfAnimationOpen()
        {
            // StartCoroutine(CreateGiftItem());
        }

        // IEnumerator CreateGiftItem()
        // {
        //     collectBtn.SetActive(true);
        //     for (int i = 0; i < giftItemBaseList.Count; i++)
        //     {
        //         yield return new WaitForSecondsRealtime(0.1f);
        //
        //         AwardItemBase giftItem = giftItemBaseList[i];
        //         if (giftItem == null) continue;
        //
        //         RectTransform giftItemTransfm = CreateGiftItem(giftItem) as RectTransform;
        //         if(giftItemTransfm != null)
        //         {
        //             giftItemTransfm.localPosition = AwardItemMgr.Instance.GetGiftAwardItemPostion(i, giftItemBaseList.Count);
        //             if(SkySreenUtils.CurrentOrientation == ScreenOrientation.Portrait)
        //             {
        //                 giftItemTransfm.localRotation = new Quaternion();
        //             }
        //         }
        //     }
        //     IsAnimationEnd = true;
        //     yield return null;
        //     OnEndOfAnimationIn();
        // }
            
        // Transform CreateGiftItem(AwardItemBase giftItem){
        //     GameObject GiftItemObj = Instantiate(giftIiemPrefab);
        //     if (GiftItemObj == null) return null;
        //
        //     GiftItemRender giftItemRender = GiftItemObj.GetComponent<GiftItemRender>();
        //     if (giftItemRender == null){
        //         Destroy(GiftItemObj);
        //         return null;
        //     }
        //
        //     giftItemRender.myTransfm.SetParent(giftItemParent);
        //     giftItemRender.Init(giftItem);
        //     return giftItemRender.myTransfm;
        // }

        protected override void OnDestroy(){
            // for (int i = 0; i < giftItemBaseList.Count; i++)
            // {
            //     AwardItemBase giftItem = giftItemBaseList[i];
            //     if (giftItem != null) giftItem.OnCheckGetGift();
            // }
            //
            // Messenger.Broadcast(GameConstants.RefreshInboxRewardListMsg);
            AudioManager.Instance.StopEffectAudio(AudioEntity.audioGiftBoxDown);
            AudioManager.Instance.StopEffectAudio(AudioEntity.audioGiftBoxOpen);
            Messenger.Broadcast(GameConstants.CloseGiftBoxDialogMsg);
            Messenger.Broadcast (SlotControllerConstants.AUTO_SPIN_RESUME);
            Messenger.RemoveListener(GiftBoxDialog_EventListener.In_Animaiton_End_Key, OnEndOfAnimationIn);
            Messenger.RemoveListener(GiftBoxDialog_EventListener.Open_Animation_Key, OnEndOfAnimationOpen);
            // UserManager.GetInstance().IsLevelState = false;
            base.OnDestroy();
        }

        bool IsAnimationEnd = false;
        public void OnPointerClick(PointerEventData eventData){
        }

        public void OnCollectButtonClick()
        {
            if (IsAnimationEnd) 
            { 
                this.Close();
                AudioManager.Instance.AsyncPlayEffectAudio(AudioEntity.audioGiftBoxCollectBtn);
                return;     
            }
        }
    }
}
