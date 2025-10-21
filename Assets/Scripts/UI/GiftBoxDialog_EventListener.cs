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
    public class GiftBoxDialog_EventListener : MonoBehaviour
    {
        public const string In_Animaiton_End_Key = "In_Animaiton_End";
        public const string Open_Animation_Key = "Open_Animation";

        public void BroadcastEndOfAnimationIn()
        {
            Messenger.Broadcast(In_Animaiton_End_Key);
        }

        public void BroadcastEndOfAnimationOpen()
        {
            Messenger.Broadcast(Open_Animation_Key);
        }
        public void CloseDialog()
        {
            this.transform.parent.GetComponent<GiftBoxDialog>().CloseDialog();
        }
        public void ShowGiftBoxReward()
        {
            this.transform.parent.GetComponent<GiftBoxDialog>().ShowGiftBoxReward();
        }
        public void PlaySoundEffect()
        {
            this.transform.parent.GetComponent<GiftBoxDialog>().PlaySoundEffect();
        }
    }
}
